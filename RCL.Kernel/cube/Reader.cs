
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Reader : Visitor
  {
    //Stuff we are given.
    protected ReadCounter m_counter;
    protected ReadSpec m_spec;
    protected int m_end;
    protected RCCube m_source;
  
    //The results and information about the results.
    protected RCCube m_target;
    protected Dictionary<RCSymbolScalar, long> m_inSymbols = new Dictionary<RCSymbolScalar, long> ();
    protected RCArray<int> m_inLines = new RCArray<int> ();
    protected RCArray<int> m_acceptedLines;
    protected RCArray<RCSymbolScalar> m_acceptedSymbols;
  
    //State variables to be used along the way.
    protected bool m_accept = false;
    protected bool m_fill = false;
    protected RCSymbolScalar m_symbol = null;
    protected long m_initg = 0;
  
    public Reader (RCCube source,
                   ReadSpec spec,
                   ReadCounter counter,
                   bool forceGCol,
                   int end)
    {
      if (source == null)
      {
        throw new ArgumentNullException ("source");
      }
      if (spec == null)
      {
        throw new ArgumentNullException ("spec");
      }
      if (counter == null)
      {
        throw new ArgumentNullException ("counter");
      }
  
      m_source = source;
      m_spec = spec;
      m_counter = counter;
      m_end = end;
      RCArray<string> axisCols = new RCArray<string> (source.Axis.Colset);
      if (forceGCol)
      {
        axisCols.Write ("G");
      }
      if (m_source.Axis.Global != null && m_source.Axis.Count > 0)
      {
        m_initg = m_source.Axis.Global [0];
      }
      m_target = new RCCube (axisCols);
    }
  
    public void Lock ()
    {
      m_acceptedLines.Lock ();
      m_acceptedSymbols.Lock ();
      m_inLines.Lock ();
    }
  
    public RCCube Read ()
    {
      if (m_spec.Forward)
      {
        int startRow = m_spec.Start;
        if (m_source.Axis.Global != null && m_source.Axis.Count > 0)
        {
          //Max prevents the row from going negative if the section has been cleared.
          startRow -= (int) m_initg;
          startRow = Math.Max (startRow, 0);
        }
        m_source.VisitCellsForward (this, startRow, m_end);
        m_acceptedLines = m_inLines;
      }
      else
      {
        m_source.VisitCellsBackward (this, m_spec.Start, m_end);
        m_target.ReverseInPlace ();
        m_inLines.ReverseInPlace ();
        m_acceptedLines = m_inLines;
      }
      m_acceptedSymbols = new RCArray<RCSymbolScalar> (m_inSymbols.Keys);
      return m_target;
    }
  
    public RCArray<int> AcceptedLines
    {
      get { return m_acceptedLines; }
    }
  
    public RCArray<RCSymbolScalar> AcceptedSymbols
    {
      get { return m_acceptedSymbols; }
    }

    protected int m_skipCount = 0;
    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      m_accept = false;
      //This will get any matching wild card symbol in the spec.
      SpecRecord spec = m_spec.Get (s);
      //If the symbol does not match anything in m_spec then move on.
      if (spec == null)
      {
        return;
      }
      //If we are too early for the symbol then move on.
      if ((m_initg + row) < spec.start)
      {
        return;
      }
      //If we already have enough of this symbol then move on.
      //Don't forget to update this counter when accepting rows.
      //ReadSpec.Get is going to do this for you now.
      //if (spec.count >= spec.limit) return;
      //Was this line already read in the case of dispatch, if so move on.
      if (m_spec.IgnoreDispatchedRows && m_counter.WasDispatched (row))
      {
        return;
      }

      //We need some way to break out of the loop here rather than keep going to the end.
      if (m_inLines.Count >= m_spec.TotalLimit)
      {
        return;
      }
      //This row would be accepted but we are supposed to skip the first
      //m_skipCount rows.
      if (m_skipCount < m_spec.SkipFirst)
      {
        ++m_skipCount;
        return;
      }
      m_accept = true;
      m_symbol = s;
      m_inLines.Write (row);
      long inCount = 0;
      //m_inSymbols will not be populated if there is no timeline.
      if (s != null)
      {
        m_inSymbols.TryGetValue (s, out inCount);
        m_inSymbols[s] = inCount++;
      }
      ++spec.count;
      m_fill = false;
      //if inCount is one then this is the first row and we need to fill in values from prior states.
      //if symbol == null then every single row and column needs to be included in the output.
      if (m_spec.Fill && inCount == 1)
      {
        m_fill = true;
      }
      else m_fill = false;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      if (!m_accept)
      {
        return;
      }
      //Do this after writing the individual cells,
      //the correct behavior of WriteCell depends on it.
      //Do not pass the counter here.
      long g = row;
      if (m_source.Axis.Global != null)
      {
        g = m_source.Axis.Global [row];
      }
      m_target.Write (g, e, t, s);
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      if (!m_accept)
      {
        return;
      }
      m_target.ReserveColumn (name);
      if (!m_fill)
      {
        return;
      }
      T last;
      if (column.Last (m_symbol, out last))
      {
        m_target.WriteCell (name, m_symbol, last);
      }
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      if (!m_accept)
      {
        return;
      }
      RCSymbolScalar symbol = m_source.Axis.SymbolAt (column.Index[row]);
      m_target.WriteCell (name, symbol, column.Data[row], -1, false, m_spec.Force);
    }
  }
}

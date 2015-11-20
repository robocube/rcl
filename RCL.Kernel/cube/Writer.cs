
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Writer : Visitor
  {
    protected RCCube m_target;
    protected RCCube m_source;
    protected ReadCounter m_counter;
    protected HashSet<RCSymbolScalar> m_result = new HashSet<RCSymbolScalar> ();
    protected bool m_writeAxis;
    protected bool m_keepIncrs;
    protected bool m_force;
    protected long m_initg;
    protected long m_e;
  
    public Writer (RCCube target, ReadCounter counter, bool keepIncrs, bool force, long initg)
    {
      //counter may be null;
      //If the counter is null then Timeline won't increment the counter on Writes.
      //Cube created using merge don't have counters, maybe this should change.
      m_initg = initg;
      m_target = target;
      m_counter = counter;
      m_keepIncrs = keepIncrs;
      m_force = force;
    }
  
    public RCArray<RCSymbolScalar> Write (RCCube source)
    {
      m_source = source;
      //This controls the value E will take if it is not provided by source.
      if (m_target.Axis.Event != null &&
          m_target.Axis.Event.Count > 0)
      {
        m_e = m_target.Axis.Event[m_target.Axis.Event.Count - 1] + 1;
      }
      else
      {
        m_e = m_target.Count;
      }
      m_source.VisitCellsForward (this, 0, m_source.Count);
      return new RCArray<RCSymbolScalar> (m_result);
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      m_writeAxis = false;
    }
  
    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar symbol, int row)
    {
      if (!m_writeAxis)
      {
        return;
      }
      long g = m_initg + row;
      if (m_counter != null)
      {
        m_counter.Write (symbol, (int) g);
      }
      //I think this needs to change to a sequence number. 2015.06.03
      e = m_e;
      //I need a unit test for not incrementing this after each row.
      ++m_e;
      //This needs to change to a sequence number but 
      //the m_e logic is not quite right yet. 2015.09.17
      //e = DateTime.Now.Ticks;
      if (m_source.Axis.Has ("E"))
      {
        e = m_source.Axis.Event [row];
      }
      //Things I do not understand, why doesn't every row have the same g?
      //Why does G reset to zero after clearing, even though initg > 0?
      //I think it is probably correct internally but that the reader below assigns G 
      //based on it's own count.
      //No it's even worse than that, there isn't and never has been any G row at all on the 
      //blackboard cubes. Wow.
      //So to solve this initially I can add m_initg to row to get g.
      //But ultimately we need to make G truly global.

      long targetLastG = -1;
      if (m_target.Axis.Global != null &&
          m_target.Axis.Global.Count > 0)
      {
        targetLastG = m_target.Axis.Global[m_target.Axis.Global.Count - 1];
      }
      if (m_source.Axis.Has ("G"))
      {
        //This means force specific G values into the blackboard.
        //This could be good or bad.
        if (m_source.Axis.Global[row] <= targetLastG)
        {
          throw new Exception ("G values may not be written out of order.");
        }
        g = m_source.Axis.Global[row];
      }
      else if (targetLastG > -1)
      {
        g = targetLastG + 1;
      }
      m_target.Axis.Write (g, e, t, symbol);
    }
  
    public override void VisitScalar<T> (string name, Column<T> column, int i)
    {
      //Can I turn this into WriteCell<T> and get rid of the boxing?
      RCSymbolScalar result = m_target.WriteCell (name,
                                                  m_source.Axis.SymbolAt (column.Index[i]),
                                                  column.Data[i], -1,
                                                  m_keepIncrs,
                                                  m_force);

      if (result != null || m_target.Axis.ColCount == 0)
      {
        m_result.Add (result);
        m_writeAxis = true;
      }
    }
  }
}
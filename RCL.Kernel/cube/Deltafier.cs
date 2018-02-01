
using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Deltafier : Visitor
  {
    protected readonly RCCube m_before, m_after;
    protected readonly RCCube m_target;
    protected Dictionary<RCSymbolScalar, long> m_beforeSyms;
    protected RCSymbolScalar m_symbol;
    protected bool m_rowChanged;

    public Deltafier (RCCube before, RCCube after) 
    {
      m_before = before;
      m_after = after;
      m_target = new RCCube (after.Axis.Match ());
    }

    public RCCube Delta ()
    {
      HashSet<RCSymbolScalar> removeSyms = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < m_before.Axis.Count; ++i)
      {
        removeSyms.Add (m_before.Axis.Symbol[i]);
      }
      HashSet<RCSymbolScalar> afterSyms = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < m_after.Axis.Count; ++i)
      {
        afterSyms.Add (m_after.Axis.Symbol[i]);
      }
      removeSyms.ExceptWith (afterSyms);
      if (removeSyms.Count > 0)
      {
        foreach (RCSymbolScalar sym in removeSyms)
        {
          //Still unsure whether I really want to hardcode "i" here...
          m_target.WriteCell ("i", sym, RCIncrScalar.Delete, -1, true, false);
          m_target.Write (-1, -1, new RCTimeScalar (DateTime.Now.Ticks, RCTimeType.Timestamp), sym);
        }
      }
      m_after.VisitCellsForward (this, 0, m_after.Count);
      return m_target;
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      m_rowChanged = false;
      m_symbol = s;
    }

    public override void AfterRow (long e, 
                                   RCTimeScalar t, 
                                   RCSymbolScalar s, 
                                   int row)
    {
      if (m_rowChanged)
      {
        long g = -1;
        m_target.Write (g, e, t, s);
      }
    }

    public override void VisitNull<T> (string name, 
                                       Column<T> vector, 
                                       int row)
    {
      //base.VisitNull (name, vector, row);
    }

    public override void VisitScalar<T> (string name,
                                         Column<T> column,
                                         int row)
    {
      T val = column.Data[row];
      int tlrow = column.Index[row];
      Column<T> beforeCol = (Column<T>) m_before.GetColumn (name);
      if (beforeCol != null)
      {
        object box;
        beforeCol.BoxLast (m_symbol, out box);
        if (box != null)
        {
          if (!box.Equals (val))
          {
            m_rowChanged = true;
            m_target.WriteCell (name, m_symbol, val, -1, true, false);
          }
        }
        else 
        {
          m_rowChanged = true;
          m_target.WriteCell (name, m_symbol, val, -1, true, false);
        }
      }
      else
      {
        m_rowChanged = true;
        m_target.WriteCell (name, m_symbol, val, -1, true, false);
      }
    }
  }
}

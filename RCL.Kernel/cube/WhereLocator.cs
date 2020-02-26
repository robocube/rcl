
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class WhereLocator : Visitor
  {
    protected readonly RCCube m_source;
    protected readonly Column<bool> m_locator;
    protected readonly RCCube m_target;
    protected readonly Dictionary<RCSymbolScalar, bool> m_last;
    protected bool m_indicator;

    public WhereLocator (RCCube source, 
                         Column<bool> locator)
    {
      m_source = source;
      m_locator = locator;
      m_target = new RCCube (source.Axis.Match ());
      m_last = new Dictionary<RCSymbolScalar, bool> ();
    }

    public RCCube Where ()
    {
      m_source.VisitCellsForward (this, 0, m_source.Count);
      return m_target;
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      bool found;
      int vrow = m_locator.Index.BinarySearch (row, out found);
      if (found && vrow < m_locator.Index.Count)
      {
        m_indicator = (bool) m_locator.BoxCell (vrow);
        m_last[s] = m_indicator;
      }
      else
      {
        if (!m_last.TryGetValue (s, out m_indicator))
        {
          m_indicator = false;
        }
      }
    }

    public override void AfterRow (long e, 
                                   RCTimeScalar t, 
                                   RCSymbolScalar s, 
                                   int row)
    {
      if (m_indicator)
      {
        long g = -1;
        m_target.Write (g, e, t, s);
      }
      m_indicator = false;
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
      int tlrow = column.Index[row];
      if (m_indicator)
      {
        m_target.WriteCell (name,
                            m_source.SymbolAt (tlrow),
                            column.Data[row],
                            -1, true, true);
      }
    }
  }
}

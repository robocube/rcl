
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class WhereIndicator : Visitor
  {
    protected readonly RCCube m_source;
    protected readonly RCArray<bool> m_indicator;
    protected readonly RCCube m_target;

    public WhereIndicator (RCCube source, RCArray<bool> indicator)
    {
      m_source = source;
      m_indicator = indicator;
      m_target = new RCCube (source.Axis.Match ());
    }

    public RCCube Where ()
    {
      m_source.VisitCellsForward (this, 0, m_source.Count);
      return m_target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      if (m_indicator[row]) {
        // g needs to be included in the signature above.
        long g = -1;
        m_target.Write (g, e, t, s);
      }
    }

    public override void VisitNull<T> (string name, Column<T> vector, int row)
    {
      // base.VisitNull (name, vector, row);
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      int tlrow = column.Index[row];
      if (m_source.Axis.Symbol != null) {
        if (m_indicator[tlrow]) {
          m_target.WriteCell (name, m_source.SymbolAt (tlrow), column.Data[row], -1, true, true);
        }
      }
      else {
        if (m_indicator[tlrow]) {
          m_target.WriteCell (name, null, column.Data[row], -1, true, true);
        }
      }
    }
  }
}

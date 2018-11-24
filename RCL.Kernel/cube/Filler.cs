
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Filler : Visitor
  {
    protected RCCube m_target;
    protected RCCube m_source;
    int m_row = 0;

    public Filler (RCCube target)
    {
      m_target = target;
    }

    public RCCube Fill (RCCube source)
    {
      m_source = source;
      m_source.VisitCellsCanonical (this, 0, m_source.Count);
      return m_target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      ++m_row;
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      RCSymbolScalar scalar = m_source.Axis.Symbol[column.Index[row]];
      m_target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      T last;
      RCSymbolScalar scalar = m_source.Axis.Symbol[m_row];
      // We use the last value from the TARGET, otherwise you pull values backwards - not good
      Column<T> targetColumn = (Column<T>) m_target.GetColumn (name);
      if (targetColumn != null && targetColumn.Last (scalar, out last))
      {
        m_target.WriteCell (name, scalar, last, m_row, true, true);
      }
    }
  }

  /// <summary>
  /// Plug null cells of a cube using a default value.
  /// </summary>
  public class Plugger : Visitor
  {
    protected RCCube m_target;
    protected RCCube m_source;
    protected int m_row = 0;
    protected object m_defaultValue;

    public Plugger (RCCube target, object defaultValue)
    {
      m_target = target;
      m_defaultValue = defaultValue;
    }

    public RCCube Plug (RCCube source)
    {
      m_source = source;
      m_source.VisitCellsForward (this, 0, m_source.Count);
      return m_target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      ++m_row;
    }

    public override void VisitScalar<T> (string name,
                                         Column<T> column, int row)
    {
      RCSymbolScalar scalar = m_source.Axis.Symbol[column.Index[row]];
      m_target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
    }

    public override void VisitNull<T> (string name,
                                       Column<T> column, int row)
    {
      RCSymbolScalar scalar = m_source.Axis.Symbol[m_row];
      m_target.WriteCell (name, scalar, m_defaultValue, m_row, true, true);
    }
  }
}

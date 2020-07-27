
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
    protected Dictionary<string, object> m_last = new Dictionary<string, object> ();

    public Filler (RCCube target)
    {
      m_target = target;
    }

    public RCCube Fill (RCCube source)
    {
      m_source = source;
      for (int i = 0; i < source.Cols; ++i)
      {
        m_target.ReserveColumn (source.ColumnAt (i), canonical:false);
      }
      m_source.VisitCellsCanonical (this, 0, m_source.Axis.Count);
      return m_target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      ++m_row;
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      if (m_source.Axis.Symbol != null)
      {
        RCSymbolScalar scalar = m_source.Axis.Symbol[column.Index[row]];
        m_target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
      }
      else
      {
        T val = column.Data[row];
        m_last[name] = val;
        m_target.WriteCell (name, null, val, column.Index[row], true, true);
      }
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      if (m_source.Axis.Symbol != null)
      {
        T last;
        RCSymbolScalar scalar = m_source.Axis.Symbol[m_row];
        // We use the last value from the TARGET, otherwise you pull values backwards - not good
        ColumnBase targetBaseColumn = m_target.GetColumn (name);
        if (targetBaseColumn != null)
        {
          Column<T> targetColumn = (Column<T>) targetBaseColumn;
          if (targetColumn != null && targetColumn.Last (scalar, out last))
          {
            m_target.WriteCell (name, scalar, last, m_row, true, true);
          }
        }
      }
      else
      {
        if (m_last.ContainsKey (name))
        {
          T lastVal = (T) m_last[name];
          m_target.WriteCell (name, null, lastVal, m_row, true, true);
        }
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
    protected bool m_unplug = false;
    protected object m_defaultValue;
    protected System.Collections.IComparer m_comparer;

    public Plugger (RCCube target, object defaultValue, System.Collections.IComparer comparer)
    {
      m_target = target;
      m_defaultValue = defaultValue;
      m_comparer = comparer;
    }

    public RCCube Plug (RCCube source)
    {
      m_source = source;
      m_source.VisitCellsCanonical (this, 0, m_source.Axis.Count);
      return m_target;
    }

    public RCCube Unplug (RCCube source)
    {
      m_source = source;
      m_unplug = true;
      for (int i = 0; i < m_source.Cols; ++i)
      {
        m_target.ReserveColumn (m_source.ColumnAt (i));
      }
      m_source.VisitCellsCanonical (this, 0, m_source.Axis.Count);
      return m_target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      ++m_row;
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      // Just a touch of ugly, slow
      if (m_unplug && typeof (T) == m_defaultValue.GetType ())
      {
        RCSymbolScalar scalar = null;
        if (m_source.Axis.Symbol != null)
        {
          scalar = m_source.Axis.Symbol[column.Index[row]];
        }
        if (m_comparer.Compare (column.Data[row], m_defaultValue) != 0)
        {
          m_target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
        }
        else
        {
        }
      }
      else
      {
        RCSymbolScalar scalar = null;
        if (m_source.Axis.Symbol != null)
        {
          scalar = m_source.Axis.Symbol[column.Index[row]];
        }
        m_target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
      }
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      if (!m_unplug)
      {
        RCSymbolScalar scalar = null;
        if (m_source.Axis.Symbol != null)
        {
          scalar = m_source.Axis.Symbol[m_row];
        }
        m_target.WriteCell (name, scalar, m_defaultValue, m_row, true, true);
      }
    }
  }
}


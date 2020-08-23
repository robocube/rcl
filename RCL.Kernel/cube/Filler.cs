
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Filler : Visitor
  {
    protected RCCube _target;
    protected RCCube _source;
    int _row = 0;
    protected Dictionary<string, object> _last = new Dictionary<string, object> ();

    public Filler (RCCube target)
    {
      _target = target;
    }

    public RCCube Fill (RCCube source)
    {
      _source = source;
      for (int i = 0; i < source.Cols; ++i)
      {
        _target.ReserveColumn (source.ColumnAt (i), canonical: false);
      }
      _source.VisitCellsCanonical (this, 0, _source.Axis.Count);
      return _target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      ++_row;
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      if (_source.Axis.Symbol != null) {
        RCSymbolScalar scalar = _source.Axis.Symbol[column.Index[row]];
        _target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
      }
      else {
        T val = column.Data[row];
        _last[name] = val;
        _target.WriteCell (name, null, val, column.Index[row], true, true);
      }
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      if (_source.Axis.Symbol != null) {
        T last;
        RCSymbolScalar scalar = _source.Axis.Symbol[_row];
        // We use the last value from the TARGET, otherwise you pull values backwards -
        // not good
        ColumnBase targetBaseColumn = _target.GetColumn (name);
        if (targetBaseColumn != null) {
          Column<T> targetColumn = (Column<T>)targetBaseColumn;
          if (targetColumn != null && targetColumn.Last (scalar, out last)) {
            _target.WriteCell (name, scalar, last, _row, true, true);
          }
        }
      }
      else {
        if (_last.ContainsKey (name)) {
          T lastVal = (T) _last[name];
          _target.WriteCell (name, null, lastVal, _row, true, true);
        }
      }
    }
  }

  /// <summary>
  /// Plug null cells of a cube using a default value.
  /// </summary>
  public class Plugger : Visitor
  {
    protected RCCube _target;
    protected RCCube _source;
    protected int _row = 0;
    protected bool _unplug = false;
    protected object _defaultValue;
    protected System.Collections.IComparer _comparer;

    public Plugger (RCCube target, object defaultValue, System.Collections.IComparer comparer)
    {
      _target = target;
      _defaultValue = defaultValue;
      _comparer = comparer;
    }

    public RCCube Plug (RCCube source)
    {
      _source = source;
      _source.VisitCellsCanonical (this, 0, _source.Axis.Count);
      return _target;
    }

    public RCCube Unplug (RCCube source)
    {
      _source = source;
      _unplug = true;
      for (int i = 0; i < _source.Cols; ++i)
      {
        _target.ReserveColumn (_source.ColumnAt (i));
      }
      _source.VisitCellsCanonical (this, 0, _source.Axis.Count);
      return _target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      ++_row;
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      // Just a touch of ugly, slow
      if (_unplug && typeof (T) == _defaultValue.GetType ()) {
        RCSymbolScalar scalar = null;
        if (_source.Axis.Symbol != null) {
          scalar = _source.Axis.Symbol[column.Index[row]];
        }
        if (_comparer.Compare (column.Data[row], _defaultValue) != 0) {
          _target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
        }
        else {}
      }
      else {
        RCSymbolScalar scalar = null;
        if (_source.Axis.Symbol != null) {
          scalar = _source.Axis.Symbol[column.Index[row]];
        }
        _target.WriteCell (name, scalar, column.Data[row], column.Index[row], true, true);
      }
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      if (!_unplug) {
        RCSymbolScalar scalar = null;
        if (_source.Axis.Symbol != null) {
          scalar = _source.Axis.Symbol[_row];
        }
        _target.WriteCell (name, scalar, _defaultValue, _row, true, true);
      }
    }
  }
}


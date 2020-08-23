
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class WhereLocator : Visitor
  {
    protected readonly RCCube _source;
    protected readonly Column<bool> _locator;
    protected readonly RCCube _target;
    protected readonly Dictionary<RCSymbolScalar, bool> _last;
    protected bool _indicator;

    public WhereLocator (RCCube source, Column<bool> locator)
    {
      _source = source;
      _locator = locator;
      _target = new RCCube (source.Axis.Match ());
      _last = new Dictionary<RCSymbolScalar, bool> ();
    }

    public RCCube Where ()
    {
      _source.VisitCellsForward (this, 0, _source.Count);
      return _target;
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      bool found;
      int vrow = _locator.Index.BinarySearch (row, out found);
      if (found && vrow < _locator.Index.Count) {
        _indicator = (bool) _locator.BoxCell (vrow);
        _last[s] = _indicator;
      }
      else {
        if (!_last.TryGetValue (s, out _indicator)) {
          _indicator = false;
        }
      }
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      if (_indicator) {
        long g = -1;
        _target.Write (g, e, t, s);
      }
      _indicator = false;
    }

    public override void VisitNull<T> (string name, Column<T> vector, int row)
    {
      // base.VisitNull (name, vector, row);
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      int tlrow = column.Index[row];
      if (_indicator) {
        _target.WriteCell (name,
                            _source.SymbolAt (tlrow),
                            column.Data[row],
                            -1,
                            true,
                            true);
      }
    }
  }
}

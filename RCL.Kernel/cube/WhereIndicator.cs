
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class WhereIndicator : Visitor
  {
    protected readonly RCCube _source;
    protected readonly RCArray<bool> _indicator;
    protected readonly RCCube _target;

    public WhereIndicator (RCCube source, RCArray<bool> indicator)
    {
      _source = source;
      _indicator = indicator;
      _target = new RCCube (source.Axis.Match ());
    }

    public RCCube Where ()
    {
      _source.VisitCellsForward (this, 0, _source.Count);
      return _target;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      if (_indicator[row]) {
        // g needs to be included in the signature above.
        long g = -1;
        _target.Write (g, e, t, s);
      }
    }

    public override void VisitNull<T> (string name, Column<T> vector, int row)
    {
      // base.VisitNull (name, vector, row);
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      int tlrow = column.Index[row];
      if (_source.Axis.Symbol != null) {
        if (_indicator[tlrow]) {
          _target.WriteCell (name, _source.SymbolAt (tlrow), column.Data[row], -1, true, true);
        }
      }
      else {
        if (_indicator[tlrow]) {
          _target.WriteCell (name, null, column.Data[row], -1, true, true);
        }
      }
    }
  }
}

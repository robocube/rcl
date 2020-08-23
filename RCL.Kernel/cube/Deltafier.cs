
using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Deltafier : Visitor
  {
    protected readonly RCCube _before, _after;
    protected readonly RCCube _target;
    protected Dictionary<RCSymbolScalar, long> _beforeSyms;
    protected RCSymbolScalar _symbol;
    protected bool _rowChanged;

    public Deltafier (RCCube before, RCCube after)
    {
      _before = before;
      _after = after;
      _target = new RCCube (after.Axis.Match ());
    }

    public RCCube Delta ()
    {
      HashSet<RCSymbolScalar> removeSyms = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < _before.Axis.Count; ++i)
      {
        removeSyms.Add (_before.Axis.Symbol[i]);
      }
      HashSet<RCSymbolScalar> afterSyms = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < _after.Axis.Count; ++i)
      {
        afterSyms.Add (_after.Axis.Symbol[i]);
      }
      removeSyms.ExceptWith (afterSyms);
      if (removeSyms.Count > 0) {
        foreach (RCSymbolScalar sym in removeSyms)
        {
          // Still unsure whether I really want to hardcode "i" here...
          _target.WriteCell ("i", sym, RCIncrScalar.Delete, -1, true, false);
          _target.Write (-1,
                          -1,
                          new RCTimeScalar (DateTime.UtcNow.Ticks, RCTimeType.Timestamp),
                          sym);
        }
      }
      _after.VisitCellsForward (this, 0, _after.Count);
      return _target;
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      _rowChanged = false;
      _symbol = s;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      if (_rowChanged) {
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
      T val = column.Data[row];
      int tlrow = column.Index[row];
      Column<T> beforeCol = (Column<T>)_before.GetColumn (name);
      if (beforeCol != null) {
        object box;
        beforeCol.BoxLast (_symbol, out box);
        if (box != null) {
          if (!box.Equals (val)) {
            _rowChanged = true;
            _target.WriteCell (name, _symbol, val, -1, true, false);
          }
        }
        else {
          _rowChanged = true;
          _target.WriteCell (name, _symbol, val, -1, true, false);
        }
      }
      else {
        _rowChanged = true;
        _target.WriteCell (name, _symbol, val, -1, true, false);
      }
    }
  }
}

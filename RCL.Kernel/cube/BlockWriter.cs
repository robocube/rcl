
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class BlockWriter : Visitor
  {
    protected RCCube _source;
    protected RCBlock _target = RCBlock.Empty;
    protected RCBlock _row = RCBlock.Empty;
    protected string _rowName = "";

    public BlockWriter (RCCube source)
    {
      _source = source;
    }

    public RCBlock Write ()
    {
      _source.VisitCellsForward (this, 0, _source.Count);
      return _target;
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar symbol, int row)
    {
      if (_source.Axis.Has ("G")) {
        _row = new RCBlock (RCBlock.Empty, "G", ":", new RCLong (_source.Axis.Global[row]));
      }
      if (_source.Axis.Has ("E")) {
        _row = new RCBlock (_row, "E", ":", new RCLong (_source.Axis.Event[row]));
      }
      if (_source.Axis.Has ("T")) {
        _row = new RCBlock (_row, "T", ":", new RCTime (_source.Axis.Time[row]));
      }
      if (_source.Axis.Has ("S")) {
        // Include S as both a field on the row blocks and as the names of the rows
        // themselves
        // This is so that you can get back to what you had with "flip block $my_cube"
        // While also being able to treat the result of "block $my_cube" as a dictionary
        // if you wish
        _row = new RCBlock (_row, "S", ":", new RCSymbol (_source.Axis.Symbol[row]));
        _rowName = _source.Axis.Symbol[row].Key.ToString ();
      }
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      _row = new RCBlock (_row,
                           name,
                           ":",
                           RCVectorBase.FromArray (new RCArray<T> (column.Data[row])));
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      _target = new RCBlock (_target, _rowName, ":", _row);
      _row = RCBlock.Empty;
      _rowName = "";
    }
  }
}

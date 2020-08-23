
using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Writer : Visitor
  {
    protected RCCube _target;
    protected RCCube _source;
    protected ReadCounter _counter;
    protected HashSet<RCSymbolScalar> _result = new HashSet<RCSymbolScalar> ();
    protected bool _writeAxis;
    protected bool _keepIncrs;
    protected bool _force;
    protected bool _delete;
    protected long _initg;
    protected long _e;

    public Writer (RCCube target, ReadCounter counter, bool keepIncrs, bool force, long initg)
    {
      // counter may be null;
      // If the counter is null then Timeline won't increment the counter on Writes.
      // Cube created using merge don't have counters, maybe this should change.
      _initg = initg;
      _target = target;
      _counter = counter;
      _keepIncrs = keepIncrs;
      _force = force;
    }

    public RCArray<RCSymbolScalar> Write (RCCube source)
    {
      _source = source;
      // This controls the value E will take if it is not provided by source.
      if (_target.Axis.Event != null && _target.Axis.Event.Count > 0) {
        _e = _target.Axis.Event[_target.Axis.Event.Count - 1] + 1;
      }
      else {
        _e = _target.Count;
      }
      _source.VisitCellsForward (this, 0, _source.Count);
      return new RCArray<RCSymbolScalar> (_result);
    }

    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      _writeAxis = false;
      _delete = false;
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar symbol, int row)
    {
      if (!_writeAxis) {
        return;
      }
      long g = _initg + row;
      // I think this needs to change to a sequence number. 2015.06.03
      e = _e;
      // I need a unit test for not incrementing this after each row.
      ++_e;
      // This needs to change to a sequence number but
      // the _e logic is not quite right yet. 2015.09.17
      if (_source.Axis.Has ("E")) {
        e = _source.Axis.Event[row];
      }
      // Things I do not understand, why doesn't every row have the same g?
      // Why does G reset to zero after clearing, even though initg > 0?
      // I think it is probably correct internally but that the reader below assigns G
      // based on its own count.
      // No it's even worse than that, there isn't and never has been any G row at all on
      // the
      // blackboard cubes. Wow.
      // So to solve this initially I can add _initg to row to get g.
      // But ultimately we need to make G truly global.
      long targetLastG = -1;
      if (_target.Axis.Global != null && _target.Axis.Global.Count > 0) {
        targetLastG = Math.Abs (_target.Axis.Global[_target.Axis.Global.Count - 1]);
      }
      if (_source.Axis.Has ("G")) {
        // This means force specific G values into the blackboard.
        // This could be good or bad.
        if (_source.Axis.Global[row] <= targetLastG) {
          throw new Exception ("G values may not be written out of order.");
        }
        g = _source.Axis.Global[row];
      }
      else if (targetLastG > -1) {
        g = targetLastG + 1;
      }
      if (_delete) {
        g = -g;
      }
      // I just moved this from the top of the function to the bottom
      // The tests are ok but I keep this note til all the concurrency examples run
      if (_counter != null) {
        _counter.Write (symbol, (int) g);
      }
      _target.Axis.Write (g, e, t, symbol);
    }

    public override void VisitScalar<T> (string name, Column<T> column, int i)
    {
      // Can I turn this into WriteCell<T> and get rid of the boxing?
      bool delete;
      RCSymbolScalar result = _target.WriteCell (name,
                                                 _source.Axis.SymbolAt (column.Index[i]),
                                                 column.Data[i],
                                                 -1,
                                                 _keepIncrs,
                                                 _force,
                                                 out delete);
      _delete = _delete || delete;
      if (result != null || _target.Axis.ColCount == 0) {
        _result.Add (result);
        _writeAxis = true;
      }
    }
  }
}

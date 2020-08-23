
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Reader : Visitor
  {
    // Stuff we are given.
    protected ReadCounter _counter;
    protected ReadSpec _spec;
    protected int _end;
    protected RCCube _source;

    // The results and information about the results.
    protected RCCube _target;
    protected Dictionary<RCSymbolScalar, long> _inSymbols =
      new Dictionary<RCSymbolScalar, long> ();
    protected RCArray<int> _inLines = new RCArray<int> ();
    protected RCArray<int> _acceptedLines;
    protected RCArray<RCSymbolScalar> _acceptedSymbols;

    // State variables to be used along the way.
    protected bool _accept = false;
    protected bool _fill = false;
    protected RCSymbolScalar _symbol = null;
    protected long _initg = 0;

    public Reader (RCCube source, ReadSpec spec, ReadCounter counter, bool forceGCol, int end)
    {
      if (source == null) {
        throw new ArgumentNullException ("source");
      }
      if (spec == null) {
        throw new ArgumentNullException ("spec");
      }
      if (counter == null) {
        throw new ArgumentNullException ("counter");
      }

      _source = source;
      _spec = spec;
      _counter = counter;
      _end = end;
      RCArray<string> axisCols = new RCArray<string> (source.Axis.Colset);
      if (forceGCol) {
        axisCols.Write ("G");
      }
      if (_source.Axis.Global != null && _source.Axis.Count > 0) {
        _initg = _source.Axis.Global[0];
      }
      _target = new RCCube (axisCols);
    }

    public void Lock ()
    {
      _acceptedLines.Lock ();
      _acceptedSymbols.Lock ();
      _inLines.Lock ();
    }

    public RCCube Read ()
    {
      if (_spec.Forward) {
        int startRow = _spec.Start;
        if (_source.Axis.Global != null && _source.Axis.Count > 0) {
          // Max prevents the row from going negative if the section has been cleared.
          startRow -= (int) _initg;
          startRow = Math.Max (startRow, 0);
        }
        _source.VisitCellsForward (this, startRow, _end);
        _acceptedLines = _inLines;
      }
      else {
        _source.VisitCellsBackward (this, _spec.Start, _end);
        _target.ReverseInPlace ();
        _inLines.ReverseInPlace ();
        _acceptedLines = _inLines;
      }
      _acceptedSymbols = new RCArray<RCSymbolScalar> (_inSymbols.Keys);
      return _target;
    }

    public RCArray<int> AcceptedLines
    {
      get { return _acceptedLines; }
    }

    public RCArray<RCSymbolScalar> AcceptedSymbols
    {
      get { return _acceptedSymbols; }
    }

    protected int _skipCount = 0;
    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      _accept = false;
      // This will get any matching wild card symbol in the spec.
      SpecRecord spec = _spec.Get (s);
      // If the symbol does not match anything in _spec then move on.
      if (spec == null) {
        return;
      }
      // If we are too early for the symbol then move on.
      if ((_initg + row) < spec.start) {
        return;
      }
      // If we already have enough of this symbol then move on.
      // Don't forget to update this counter when accepting rows.
      // ReadSpec.Get is going to do this for you now.
      // if (spec.count >= spec.limit) return;
      // Was this line already read in the case of dispatch, if so move on.
      if (_spec.IgnoreDispatchedRows && _counter.WasDispatched (row)) {
        return;
      }

      // We need some way to break out of the loop here rather than keep going to the end.
      if (_inLines.Count >= _spec.TotalLimit) {
        return;
      }
      // This row would be accepted but we are supposed to skip the first
      // _skipCount rows.
      if (_skipCount < _spec.SkipFirst) {
        ++_skipCount;
        return;
      }
      _accept = true;
      _symbol = s;
      _inLines.Write (row);
      long inCount = 0;
      // _inSymbols will not be populated if there is no timeline.
      if (s != null) {
        _inSymbols.TryGetValue (s, out inCount);
        _inSymbols[s] = inCount++;
      }
      ++spec.count;
      _fill = false;
      // if inCount is one then this is the first row and we need to fill in values from
      // prior
      // states.
      // if symbol == null then every single row and column needs to be included in the
      // output.
      if (_spec.Fill && inCount == 1) {
        _fill = true;
      }
      else {
        _fill = false;
      }
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      if (!_accept) {
        return;
      }
      // Do this after writing the individual cells,
      // the correct behavior of WriteCell depends on it.
      // Do not pass the counter here.
      long g = row;
      if (_source.Axis.Global != null) {
        g = _source.Axis.Global[row];
      }
      _target.Write (g, e, t, s);
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      if (!_accept) {
        return;
      }
      _target.ReserveColumn (name);
      if (!_fill) {
        return;
      }
      T last;
      if (column.Last (_symbol, out last)) {
        _target.WriteCell (name, _symbol, last);
      }
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      if (!_accept) {
        return;
      }
      RCSymbolScalar symbol = _source.Axis.SymbolAt (column.Index[row]);
      _target.WriteCell (name, symbol, column.Data[row], -1, false, _spec.Force);
    }
  }
}

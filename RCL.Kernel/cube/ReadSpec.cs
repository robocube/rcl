
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class ReadSpec
  {
    protected readonly ReadCounter _counter;
    protected readonly Dictionary<RCSymbolScalar, SpecRecord> _records =
      new Dictionary<RCSymbolScalar, SpecRecord> ();
    protected readonly bool _forward = true;
    protected readonly bool _ignoreDispatchedRows = true;
    protected int _start = int.MaxValue;
    protected bool _force = false;
    protected bool _fill = true;
    public readonly bool ShowDeleted;

    protected bool _unlimited;
    protected int _symbolLimit;
    protected int _skipFirst = 0;
    protected int _totalLimit = int.MaxValue;

    // This is for page.
    public ReadSpec (ReadCounter counter,
                     RCSymbol left,
                     int skipFirst,
                     int totalLimit,
                     bool
                     showDeleted)
    {
      // For paging.
      _counter = counter;
      _forward = totalLimit >= 0;
      _ignoreDispatchedRows = false;
      _start = 0;
      // each symbol is unlimited individually.  But the total is going to be limited by
      // stopAfter.
      _unlimited = true;
      _symbolLimit = int.MaxValue;
      _skipFirst = Math.Abs (skipFirst);
      _totalLimit = Math.Abs (totalLimit);
      ShowDeleted = showDeleted;
      left = _counter.ConcreteSymbols (left, showDeleted);
      for (int i = 0; i < left.Count; ++i)
      {
        Add (left[i], 0, _symbolLimit);
      }
    }

    // This is for read and its ilk
    // In this case symbols are added by the client
    public ReadSpec (ReadCounter counter, int defaultLimit, bool force, bool fill)
    {
      _counter = counter;
      _forward = defaultLimit >= 0;
      _unlimited = defaultLimit == 0;
      _symbolLimit = Math.Abs (_unlimited ? int.MaxValue : defaultLimit);
      _force = force;
    }

    public ReadSpec (ReadCounter counter,
                     RCSymbol left,
                     RCLong right,
                     int defaultLimit,
                     bool
                     ignoreDispatchedRows,
                     bool force,
                     bool fill,
                     bool showDeleted)
    {
      _counter = counter;
      ShowDeleted = showDeleted;
      _ignoreDispatchedRows = ignoreDispatchedRows;
      _force = force;
      _fill = fill;
      if (right.Count == 1) {
        // It's the start point.
        _forward = defaultLimit >= 0;
        _unlimited = defaultLimit == 0;
        _symbolLimit = Math.Abs (_unlimited ? int.MaxValue : defaultLimit);
        left = _counter.ConcreteSymbols (left, showDeleted);
        for (int i = 0; i < left.Count; ++i)
        {
          Add (left[i], (int) right[0], _symbolLimit);
        }
      }
      else if (right.Count == 2) {
        // It's the start point and the limit.
        _forward = right[1] >= 0;
        _unlimited = right[1] == 0;
        _symbolLimit = Math.Abs (_unlimited ? int.MaxValue : (int) right[1]);
        left = _counter.ConcreteSymbols (left, showDeleted);
        for (int i = 0; i < left.Count; ++i)
        {
          Add (left[i], (int) right[0], _symbolLimit);
        }
      }
      else {
        // Who knows what this should do.
        // Maybe let you give different counts for each symbol.
        throw new ArgumentException ("Read takes a maximum of two right arguments.");
      }
    }

    public long SymbolLimit
    {
      get { return _symbolLimit; }
    }

    public bool SymbolUnlimited
    {
      get { return _unlimited; }
    }

    public bool Forward
    {
      get { return _forward; }
    }

    public int Start
    {
      get { return _start; }
    }

    public bool IgnoreDispatchedRows
    {
      get { return _ignoreDispatchedRows; }
    }

    public int SkipFirst
    {
      get { return _skipFirst; }
    }

    public int TotalLimit
    {
      get { return _totalLimit; }
    }

    public bool Force
    {
      get { return _force; }
    }

    public bool Fill
    {
      get { return _fill; }
    }

    public int RecordCount
    {
      get { return _records.Count; }
    }

    public RCBlock ToBlock ()
    {
      RCBlock result = RCBlock.Empty;
      result = new RCBlock (result, "SymbolLimit", ":", SymbolLimit);
      result = new RCBlock (result, "SymbolUnlimited", ":", SymbolUnlimited);
      result = new RCBlock (result, "Forward", ":", Forward);
      result = new RCBlock (result, "Start", ":", (long) Start);
      result = new RCBlock (result, "IgnoreDispatchedRows", ":", IgnoreDispatchedRows);
      result = new RCBlock (result, "SkipFirst", ":", (long) SkipFirst);
      result = new RCBlock (result, "TotalLimit", ":", (long) TotalLimit);
      result = new RCBlock (result, "Force", ":", Force);
      result = new RCBlock (result, "Fill", ":", Fill);
      result = new RCBlock (result, "RecordCount", ":", RecordCount);
      RCBlock records = RCBlock.Empty;
      foreach (KeyValuePair<RCSymbolScalar, SpecRecord> specRecord in _records)
      {
        records = new RCBlock (records,
                               specRecord.Key.ToString (),
                               ":",
                               specRecord.Value.ToBlock ());
      }
      result = new RCBlock (result, "Records", ":", records);
      return result;
    }

    public override string ToString ()
    {
      return ToBlock ().Format (RCFormat.Canonical);
    }

    public void Add (RCSymbolScalar scalar, int start, int limit)
    {
      SpecRecord record = new SpecRecord (scalar);
      record.start = start;
      record.limit = limit;
      if (start < _start) {
        _start = start;
      }
      RCSymbolScalar current = scalar;
      while (current != null)
      {
        _records[current] = record;
        current = current.Previous;
      }
      // What happens if you pass multiple identical symbols? boom!
      // Jan 4 2015 - we really need a test for that!
      // _records[record.symbol] = record;
    }

    public SpecRecord Get (RCSymbolScalar scalar)
    {
      SpecRecord result;
      // This is for cubes that don't have a timeline: the symbol will be null.
      if (scalar == null) {
        return _records[RCSymbolScalar.Empty];
      }
      RCSymbolScalar current = scalar;
      while (current != null)
      {
        // This will make sure counts are allocated to the most specific symbol first
        // if you have multiple symbols in the same heirarchy within the same spec.
        if (_records.TryGetValue (current, out result) &&
            result.count < result.limit &&
            ((current == scalar) || (!result.Concrete && scalar.IsConcreteOf (result.original)))) {
          return result;
        }
        current = current.Previous;
      }
      return null;
    }

    public System.Collections.IEnumerator GetEnumerator ()
    {
      return _records.Values.GetEnumerator ();
    }
  }
}

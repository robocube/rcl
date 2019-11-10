
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class ReadSpec
  {
    protected readonly ReadCounter m_counter;
    protected readonly Dictionary<RCSymbolScalar, SpecRecord> m_records = new Dictionary<RCSymbolScalar, SpecRecord> ();
    protected readonly bool m_forward = true;
    protected readonly bool m_ignoreDispatchedRows = true;
    protected int m_start = int.MaxValue;
    protected bool m_force = false;
    protected bool m_fill = true;
    public readonly bool ShowDeleted;

    protected bool m_unlimited;
    protected int m_symbolLimit;
    protected int m_skipFirst = 0;
    protected int m_totalLimit = int.MaxValue;

    //This is for page.
    public ReadSpec (ReadCounter counter, RCSymbol left, int skipFirst, int totalLimit, bool showDeleted)
    {
      //For paging.
      m_counter = counter;
      m_forward = totalLimit >= 0;
      m_ignoreDispatchedRows = false;
      m_start = 0;
      //each symbol is unlimited individually.  But the total is going to be limited by stopAfter.
      m_unlimited = true;
      m_symbolLimit = int.MaxValue;
      m_skipFirst = Math.Abs (skipFirst);
      m_totalLimit = Math.Abs (totalLimit);
      ShowDeleted = showDeleted;
      left = m_counter.ConcreteSymbols (left, showDeleted);
      for (int i = 0; i < left.Count; ++i)
      {
        Add (left[i], 0, m_symbolLimit);
      }
    }

    //This is for read and its ilk
    //In this case symbols are added by the client
    public ReadSpec (ReadCounter counter, int defaultLimit, bool force, bool fill)
    {
      m_counter = counter;
      m_forward = defaultLimit >= 0;
      m_unlimited = defaultLimit == 0;
      m_symbolLimit = Math.Abs (m_unlimited ? int.MaxValue : defaultLimit);
      m_force = force;
    }

    public ReadSpec (ReadCounter counter, RCSymbol left, RCLong right, int defaultLimit, bool ignoreDispatchedRows, bool force, bool fill, bool showDeleted)
    {
      m_counter = counter;
      ShowDeleted = showDeleted;
      m_ignoreDispatchedRows = ignoreDispatchedRows;
      m_force = force;
      m_fill = fill;
      if (right.Count == 1)
      {
        //It's the start point.
        m_forward = defaultLimit >= 0;
        m_unlimited = defaultLimit == 0;
        m_symbolLimit = Math.Abs (m_unlimited ? int.MaxValue : defaultLimit);
        left = m_counter.ConcreteSymbols (left, showDeleted);
        for (int i = 0; i < left.Count; ++i)
        {
          Add (left[i], (int) right[0], m_symbolLimit);
        }
      }
      else if (right.Count == 2)
      {
        //It's the start point and the limit.
        m_forward = right[1] >= 0;
        m_unlimited = right[1] == 0;
        m_symbolLimit = Math.Abs (m_unlimited ? int.MaxValue : (int) right[1]);
        left = m_counter.ConcreteSymbols (left, showDeleted);
        for (int i = 0; i < left.Count; ++i)
        {
          Add (left[i], (int) right[0], m_symbolLimit);
        }
      }
      else
      {
        //Who knows what this should do.
        //Maybe let you give different counts for each symbol.
        throw new ArgumentException ("Read takes a maximum of two right arguments.");
      }
    }

    public long SymbolLimit
    {
      get { return m_symbolLimit; }
    }
  
    public bool SymbolUnlimited
    {
      get { return m_unlimited; }
    }
  
    public bool Forward
    {
      get { return m_forward; }
    }
  
    public int Start
    {
      get { return m_start; }
    }
  
    public bool IgnoreDispatchedRows
    {
      get { return m_ignoreDispatchedRows; }
    }

    public int SkipFirst
    {
      get { return m_skipFirst; }
    }

    public int TotalLimit
    {
      get { return m_totalLimit; }
    }
  
    public bool Force
    {
      get { return m_force; }
    }

    public bool Fill
    {
      get { return m_fill; }
    }

    public int RecordCount
    {
      get { return m_records.Count; }
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
      foreach (KeyValuePair<RCSymbolScalar, SpecRecord> specRecord in m_records)
      {
        records = new RCBlock (records, specRecord.Key.ToString (), ":", specRecord.Value.ToBlock ());
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
      if (start < m_start)
      {
        m_start = start;
      }
      RCSymbolScalar current = scalar;
      while (current != null)
      {
        m_records[current] = record;
        current = current.Previous;
      }
      //What happens if you pass multiple identical symbols? boom!
      //Jan 4 2015 - we really need a test for that!
      //m_records[record.symbol] = record;
    }
  
    public SpecRecord Get (RCSymbolScalar scalar)
    {
      SpecRecord result;
      //This is for cubes that don't have a timeline: the symbol will be null.
      if (scalar == null)
      {
        return m_records[RCSymbolScalar.Empty];
      }
      RCSymbolScalar current = scalar;
      while (current != null)
      {
        //This will make sure counts are allocated to the most specific symbol first
        //if you have multiple symbols in the same heirarchy within the same spec.
        if (m_records.TryGetValue (current, out result) &&
            result.count < result.limit &&
            ((current == scalar) || (!result.Concrete && scalar.IsConcreteOf (result.original))))
        {
          return result;
        }
        current = current.Previous;
      }
      return null;
    }
  
    public System.Collections.IEnumerator GetEnumerator ()
    {
      return m_records.Values.GetEnumerator();
    }
  }
}


using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class ReadCounter
  {
    protected readonly Dictionary<RCSymbolScalar, CountRecord> m_records =
      new Dictionary<RCSymbolScalar, CountRecord> ();
    protected readonly Dictionary<RCSymbolScalar, CountRecord> m_abstracts =
      new Dictionary<RCSymbolScalar, CountRecord> ();
  
    //A null here will indicate that the counter is for reading not dispatching.
    protected readonly List<bool> m_dispatched = new List<bool>();
  
    public ReadSpec GetReadSpec (RCSymbol symbol, int limit)
    {
      ReadSpec result = new ReadSpec (limit);
      if (limit == 0)
      {
        limit = int.MaxValue;
      }
      for (int i = 0; i < symbol.Count; ++i)
      {
        CountRecord current = Get (symbol[i]);
        if (current != null)
        {
          //Start searching from the first known record.
          result.Add (symbol[i], current.start, limit);
        }
        else
        {
          //In this case we know there is no data so any search should begin
          //from the end of time.
          result.Add (symbol[i], m_dispatched.Count, limit);
        }
      }
      return result;
    }
  
    public ReadSpec GetReadSpec (RCSymbol symbol, RCLong starts)
    {
      ReadSpec result = new ReadSpec (0);
      for (int i = 0; i < symbol.Count; ++i)
      {
        result.Add (symbol[i], (int) starts[i], int.MaxValue);
      }
      return result;
    }

    public CountRecord Get (RCSymbolScalar scalar)
    {
      CountRecord result;
      if (!scalar.Key.Equals ("*"))
      {
        m_records.TryGetValue (scalar, out result);
      }
      else
      {
        m_abstracts.TryGetValue (scalar.Previous, out result);
      }
      return result;
    }

    public CountRecord Get (SpecRecord spec)
    {
      CountRecord result;
      if (spec.Concrete)
      {
        m_records.TryGetValue (spec.symbol, out result);
      }
      else
      {
        m_abstracts.TryGetValue (spec.symbol, out result);
      }
      return result;
    }

    public void Dispatch (RCSymbolScalar scalar, long line)
    {
      Dictionary<RCSymbolScalar, CountRecord> map = m_records;
      while (scalar != null)
      {
        //I assume you wouldn't try to dispatch if the symbol was not represented...
        CountRecord record = map[scalar];
        if (line == record.start)
        {
          ++record.start;
        }
        --record.count;
        scalar = scalar.Previous;
        map = m_abstracts;
      }
      m_dispatched[(int)line] = true;
    }
  
    public void Dispatch (RCCube target, RCArray<int> lines)
    {
      for (int i = 0; i < lines.Count; ++i)
      {
        RCSymbolScalar scalar = target.Axis.SymbolAt (lines[i]);
        Dispatch (scalar, lines[i]);
      }
    }

    public void Write (RCSymbolScalar scalar, int line)
    {
      CountRecord record;
      Dictionary<RCSymbolScalar, CountRecord> map = m_records;
      while (scalar != null)
      {
        if (!map.TryGetValue (scalar, out record))
        {
          record = new CountRecord (scalar, map == m_records);
          map[scalar] = record;
          record.start = line;
        }
        //I have to take stuff into account here when clearing...
        //But How? Can I just blow away the counters?
        ++record.count;
        ++record.total;
        record.end = line;
        scalar = scalar.Previous;
        map = m_abstracts;
      }
      m_dispatched.Add (false);
    }

    public RCSymbol ConcreteSymbols (RCSymbol symbol)
    {
      RCArray<RCSymbolScalar> result = new RCArray<RCSymbolScalar> (8);
      foreach (CountRecord count in m_records.Values)
      {
        for (int i = 0; i < symbol.Count; ++i)
        {
          RCSymbolScalar scalar = symbol[i];
          if (scalar.Key.Equals ("*"))
          {
            scalar = scalar.Previous;
          }
          //I think it should only return children if there was an asterisk.
          if (count.Concrete && (count.symbol.IsConcreteOf (scalar) || count.symbol.Equals (scalar)))
          {
            result.Write (count.symbol);
          }
        }
      }
      return new RCSymbol (result);
    }
  
    public bool WasDispatched (long line)
    {
      return m_dispatched[(int) line];
    }
  
    public Satisfy CanSatisfy (ReadSpec spec)
    {
      if (spec.IgnoreDispatchedRows)
      {
        if (spec.SymbolUnlimited)
        {
          foreach (SpecRecord specRecord in spec)
          {
            //CountRecord countRecord = Get (specRecord.symbol);
            CountRecord countRecord = Get (specRecord);
            //The symbol is missing entirely.
            if (countRecord == null)
            {
              continue;
            }
            //If there are zero rows for the symbol then we are never satsified.
            if (countRecord.count > 0)
            {
              return Satisfy.Yes;
            }
          }
          return Satisfy.No;
        }
        else
        {
          foreach (SpecRecord specRecord in spec)
          {
            //CountRecord countRecord = Get (specRecord.symbol);
            CountRecord countRecord = Get (specRecord);
            //The symbol is missing entirely.
            if (countRecord == null)
            {
              return Satisfy.No;
            }
            //If there are zero rows for the symbol then we are never satsified.
            if (countRecord.count == 0)
            {
              return Satisfy.No;
            }
            //We will accept any number of them.
            if (specRecord.limit == int.MaxValue)
            {
              continue;
            }
            //There are not enough records to satisfy the spec for this symbol.
            if (countRecord.count < specRecord.limit)
            {
              return Satisfy.No;
            }
          }
        }
      }
      else
      {
        if (spec.SymbolUnlimited)
        {
          foreach (SpecRecord specRecord in spec)
          {
            //CountRecord countRecord = Get (specRecord.symbol);
            CountRecord countRecord = Get (specRecord);
            //The symbol is missing entirely.
            if (countRecord == null)
            {
              continue;
            }
            //Notice that while dispatching we look at count. Here at total.
            if (countRecord.total > 0)
            {
              if (specRecord.start == 0)
              {
                return Satisfy.Yes;
              }
              else return Satisfy.Maybe;
            }
          }
          return Satisfy.No;
        }
        else
        {
          foreach (SpecRecord specRecord in spec)
          {
            //CountRecord countRecord = Get (specRecord.symbol);
            CountRecord countRecord = Get (specRecord);
            //The symbol is missing entirely.
            if (countRecord == null)
            {
              return Satisfy.No;
            }
            //Check there are enough records to satisfy the spec for this symbol.
            if (specRecord.start == 0)
            {
              //We will accept any number of them.
              if (specRecord.limit == int.MaxValue)
              {
                continue;
              }
              if (countRecord.total < specRecord.limit)
              {
                return Satisfy.No;
              }
            }
            else
            {
              //This is for the case where you use read or last from an arbitrary start point.
              //Since we don't track where the symbols are in this counter, we won't know whether
              //The query would be satisfied.  We just have to do it and see.
              return Satisfy.Maybe;
            }
          }
        }
      }
      return Satisfy.Yes;
    }
  }
}
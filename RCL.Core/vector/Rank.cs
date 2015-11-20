
using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Rank
  {
    //I wanted rank to support the same set of operators as sort.
    //But in practice you could just take the abs of your column
    //before ranking it, which doesn't work for sorting.
    //So I'm not certain bending over backwards for absolute ranking makes sense.
    //On the other hand the work is already done and this should be a little more performant;
    //no additional vector created for the abs.
    public class AbsoluteValue<T> { public virtual T Abs (T val){return val;} }
    public class LongAbs : AbsoluteValue<long>
    {public override long Abs (long val) { return Math.Abs (val); }}
    public class DoubleAbs : AbsoluteValue<double>
    {public override double Abs (double val) { return Math.Abs (val); }}
    public class DecimalAbs : AbsoluteValue<decimal>
    {public override decimal Abs (decimal val) { return Math.Abs (val); }}

    public class RankState<T> where T : IComparable<T>
    {
      protected static readonly Dictionary<char, object> m_absmap = new Dictionary<char, object>();

      static RankState ()
      {
        m_absmap['l'] = new LongAbs();
        m_absmap['d'] = new DoubleAbs();
        m_absmap['m'] = new DecimalAbs();
      }

      protected RCVector<T> m_data;
      protected AbsoluteValue<T> m_abs;
      public RankState (RCVector<T> data)
      {
        m_data = data;
        object abs = null;
        m_absmap.TryGetValue (data.TypeCode, out abs);
        if (abs == null)
          m_abs = new AbsoluteValue<T>();
        else
          m_abs = (AbsoluteValue<T>) abs;
      }

      public virtual int Asc (long x, long y){return m_data[(int)x].CompareTo (m_data[(int)y]);}
      public virtual int Desc (long x, long y){return m_data[(int)y].CompareTo (m_data[(int)x]);}
      public virtual int AbsAsc (long x, long y){return m_abs.Abs (m_data[(int)x]).CompareTo (m_abs.Abs (m_data[(int)y]));}
      public virtual int AbsDesc (long x, long y){return m_abs.Abs (m_data[(int)y]).CompareTo (m_abs.Abs (m_data[(int)x]));}
    }

    public class RankStateArray<T> where T : IComparable<T>
    {
      protected static readonly Dictionary<char, object> m_absmap = new Dictionary<char, object>();

      static RankStateArray ()
      {
        m_absmap['l'] = new LongAbs ();
        m_absmap['d'] = new DoubleAbs ();
        m_absmap['m'] = new DecimalAbs ();
      }

      protected RCArray<T> m_data;
      protected AbsoluteValue<T> m_abs;
      public RankStateArray (RCArray<T> data, char typeCode)
      {
        m_data = data;
        object abs = null;
        m_absmap.TryGetValue (typeCode, out abs);
        if (abs == null)
          m_abs = new AbsoluteValue<T>();
        else
          m_abs = (AbsoluteValue<T>) abs;
      }

      public virtual int Asc (long x, long y)
      {
        return m_data[(int)x].CompareTo (m_data[(int)y]);
      }
      public virtual int Desc (long x, long y)
      {
        return m_data[(int)y].CompareTo (m_data[(int)x]);
      }
      public virtual int AbsAsc (long x, long y)
      {
        return m_abs.Abs (m_data[(int)x]).CompareTo (m_abs.Abs (m_data[(int)y]));
      }
      public virtual int AbsDesc (long x, long y)
      {
        return m_abs.Abs (m_data[(int)y]).CompareTo (m_abs.Abs (m_data[(int)x]));
      }
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCLong (DoRank<byte> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCByte right)
    {
      runner.Yield (closure, new RCLong (DoRank<byte> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoRank<long> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoRank<long> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCLong (DoRank<double> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCDouble right)
    {
      runner.Yield (closure, new RCLong (DoRank<double> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCLong (DoRank<decimal> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCDecimal right)
    {
      runner.Yield (closure, new RCLong (DoRank<decimal> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCLong (DoRank<bool> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCBoolean right)
    {
      runner.Yield (closure, new RCLong (DoRank<bool> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCLong (DoRank<string> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      runner.Yield (closure, new RCLong (DoRank<string> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (
      RCRunner runner, RCClosure closure, RCSymbol left, RCTime right)
    {
      runner.Yield (closure, new RCLong (DoRank<RCTimeScalar> (Sort.ToDir (left), right)));
    }

    public static long[] DoRank<T> (SortDirection direction, RCVector<T> vector) 
      where T : IComparable<T>
    {
      long[] indices = new long[vector.Count];
      for (long i = 0; i < indices.Length; ++i)
        indices[i] = i;
      RankState<T> state = new RankState<T> (vector);
      Comparison<long> comparison;
      switch (direction)
      {
        case SortDirection.asc : comparison = new Comparison<long> (state.Asc); break;
        case SortDirection.desc : comparison = new Comparison<long> (state.Desc); break;
        case SortDirection.absasc : comparison = new Comparison<long> (state.AbsAsc); break;
        case SortDirection.absdesc : comparison = new Comparison<long> (state.AbsDesc); break;
        default : throw new Exception ("Unknown SortDirection: " + direction.ToString ());
      }
      Array.Sort (indices, comparison);
      return indices;
    }

    public static long[] DoRank<T> (SortDirection direction, char typeCode, RCArray<T> array) 
      where T : IComparable<T>
    {
      long[] indices = new long[array.Count];
      for (long i = 0; i < indices.Length; ++i)
        indices[i] = i;
      RankStateArray<T> state = new RankStateArray<T> (array, typeCode);
      Comparison<long> comparison;
      switch (direction)
      {
        case SortDirection.asc : comparison = new Comparison<long> (state.Asc); break;
        case SortDirection.desc : comparison = new Comparison<long> (state.Desc); break;
        case SortDirection.absasc : comparison = new Comparison<long> (state.AbsAsc); break;
        case SortDirection.absdesc : comparison = new Comparison<long> (state.AbsDesc); break;
        default : throw new Exception ("Unknown SortDirection: " + direction.ToString ());
      }
      Array.Sort (indices, comparison);
      return indices;
    }
  }
}
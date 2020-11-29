
using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RankUtils
  {
    public static long[] DoRank<T> (SortDirection direction, RCVector<T> vector)
        where T : IComparable<T>
    {
      long[] indices = new long[vector.Count];
      for (long i = 0; i < indices.Length; ++i)
      {
        indices[i] = i;
      }
      RankState<T> state = new RankState<T> (vector);
      Comparison<long> comparison;
      switch (direction)
      {
        case SortDirection.asc: comparison = new Comparison<long> (state.Asc); break;
        case SortDirection.desc: comparison = new Comparison<long> (state.Desc); break;
        case SortDirection.absasc: comparison = new Comparison<long> (state.AbsAsc); break;
        case SortDirection.absdesc: comparison = new Comparison<long> (state.AbsDesc); break;
        default: throw new Exception ("Unknown SortDirection: " + direction.ToString ());
      }
      Array.Sort (indices, comparison);
      return indices;
    }

    public static long[] DoRank<T> (SortDirection direction, char typeCode, RCArray<T> array)
        where T : IComparable<T>
    {
      long[] indices = new long[array.Count];
      for (long i = 0; i < indices.Length; ++i)
      {
        indices[i] = i;
      }
      RankStateArray<T> state = new RankStateArray<T> (array, typeCode);
      Comparison<long> comparison;
      switch (direction)
      {
        case SortDirection.asc: comparison = new Comparison<long> (state.Asc); break;
        case SortDirection.desc: comparison = new Comparison<long> (state.Desc); break;
        case SortDirection.absasc: comparison = new Comparison<long> (state.AbsAsc); break;
        case SortDirection.absdesc: comparison = new Comparison<long> (state.AbsDesc); break;
        default: throw new Exception ("Unknown SortDirection: " + direction.ToString ());
      }
      Array.Sort (indices, comparison);
      return indices;
    }

    public static void ApplyArrayRank<T> (RCArray<T> inputData,
                                          RCArray<int> inputIndex,
                                          Dictionary<long, int> map,
                                          int axisCount,
                                          out RCArray<T> data,
                                          out RCArray<int> index)
    {
      RCArray<long> im = new RCArray<long> (inputData.Count);
      T[] fd = new T[inputData.Count];
      int[] fi = new int[inputData.Count];
      if (inputIndex != null)
      {
        // Any rows that are lacking values in the sort column will be missing from map.
        // Push these rows so that they come after all the rows with valid values in the
        // sort
        // column.
        // But importantly, they are kept in the original order.
        for (int j = 0; j < inputIndex.Count; ++j)
        {
          int newRow;
          if (!map.TryGetValue (inputIndex[j], out newRow))
          {
            throw new Exception (string.Format ("inputIndex[j]: {0} was not represented in map!",
                                                inputIndex[j]));
          }
          im.Write (newRow);
        }
      }
      else
      {
        for (int j = 0; j < inputData.Count; ++j)
        {
          int newRow;
          if (!map.TryGetValue (j, out newRow))
          {
            newRow = map.Count + j;
          }
          im.Write (newRow);
        }
      }
      // Now rank the values in im ascending.
      long[] rim = DoRank<long> (SortDirection.asc, 'l', im);
      for (int j = 0; j < rim.Length; ++j)
      {
        fd[j] = inputData[(int) rim[j]];
        fi[j] = (int) im[(int) rim[j]];
      }
      index = new RCArray<int> (fi);
      data = new RCArray<T> (fd);
    }

    public static Timeline ApplyAxisRank (Timeline axis, Dictionary<long, int> map)
    {
      Timeline result;
      RCArray<long> g = axis.Global;
      RCArray<long> gNew = null;
      RCArray<long> e = axis.Event;
      RCArray<long> eNew = null;
      RCArray<RCTimeScalar> t = axis.Time;
      RCArray<RCTimeScalar> tNew = null;
      RCArray<RCSymbolScalar> s = axis.Symbol;
      RCArray<RCSymbolScalar> sNew = null;
      if (axis.Global != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (g, null, map, axis.Count, out gNew, out ignore);
      }
      if (axis.Event != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (e, null, map, axis.Count, out eNew, out ignore);
      }
      if (axis.Time != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (t, null, map, axis.Count, out tNew, out ignore);
      }
      if (axis.Symbol != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (s, null, map, axis.Count, out sNew, out ignore);
      }
      if (gNew == null && eNew == null && tNew == null && sNew == null)
      {
        result = new Timeline (axis.Count);
      }
      else
      {
        result = new Timeline (gNew, eNew, tNew, sNew);
      }
      return result;
    }

    /// <summary>
    /// DoAxisRank implements the standard ranking for a sorted timeline axis,
    /// which is by T or E, followed by S.
    /// </summary>
    public static Dictionary<long, int> DoAxisRank (Timeline axis)
    {
      int[] indices = new int[axis.Count];
      for (int i = 0; i < axis.Count; ++i)
      {
        indices[i] = i;
      }
      Array.Sort (indices, axis.Proto.CompareAxisRows);
      Dictionary<long, int> result = new Dictionary<long, int> ();
      for (int i = 0; i < indices.Length; ++i)
      {
        result[indices[i]] = i;
      }
      return result;
    }
  }
}

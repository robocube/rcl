using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Sort
  {
    protected static Dictionary<char, Dictionary<SortDirection, object>> m_comparers =
      new Dictionary<char, Dictionary<SortDirection, object>>();

    static Sort ()
    {
      Dictionary<SortDirection, object> comparers;

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<byte>(delegate(byte x, byte y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<byte>(delegate(byte x, byte y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<byte>(delegate(byte x, byte y){return Math.Abs (x).CompareTo (Math.Abs (y));});
      comparers[SortDirection.absdesc] = new Comparison<byte>(delegate(byte x, byte y){return 0-Math.Abs (x).CompareTo (Math.Abs (y));});
      m_comparers.Add ('x', comparers);

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<long>(delegate(long x, long y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<long>(delegate(long x, long y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<long>(delegate(long x, long y){return Math.Abs (x).CompareTo (Math.Abs (y));});
      comparers[SortDirection.absdesc] = new Comparison<long>(delegate(long x, long y){return 0-Math.Abs (x).CompareTo (Math.Abs (y));});
      m_comparers.Add ('l', comparers);

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<double>(delegate(double x, double y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<double>(delegate(double x, double y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<double>(delegate(double x, double y){return Math.Abs (x).CompareTo (Math.Abs (y));});
      comparers[SortDirection.absdesc] = new Comparison<double>(delegate(double x, double y){return 0-Math.Abs (x).CompareTo (Math.Abs (y));});
      m_comparers.Add ('d', comparers);

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<decimal>(delegate(decimal x, decimal y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<decimal>(delegate(decimal x, decimal y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<decimal>(delegate(decimal x, decimal y){return Math.Abs (x).CompareTo (Math.Abs (y));});
      comparers[SortDirection.absdesc] = new Comparison<decimal>(delegate(decimal x, decimal y){return 0-Math.Abs (x).CompareTo (Math.Abs (y));});
      m_comparers.Add ('m', comparers);

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<bool>(delegate(bool x, bool y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<bool>(delegate(bool x, bool y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<bool>(delegate(bool x, bool y){return x.CompareTo (y);});
      comparers[SortDirection.absdesc] = new Comparison<bool>(delegate(bool x, bool y){return 0-x.CompareTo (y);});
      m_comparers.Add ('b', comparers);

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<string>(delegate(string x, string y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<string>(delegate(string x, string y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<string>(delegate(string x, string y){return x.CompareTo (y);});
      comparers[SortDirection.absdesc] = new Comparison<string>(delegate(string x, string y){return 0-x.CompareTo (y);});
      m_comparers.Add ('s', comparers);

      comparers = new Dictionary<SortDirection, object>();
      comparers[SortDirection.asc] = new Comparison<RCTimeScalar>(delegate(RCTimeScalar x, RCTimeScalar y){return x.CompareTo (y);});
      comparers[SortDirection.desc] = new Comparison<RCTimeScalar>(delegate(RCTimeScalar x, RCTimeScalar y){return 0-x.CompareTo (y);});
      comparers[SortDirection.absasc] = new Comparison<RCTimeScalar>(delegate(RCTimeScalar x, RCTimeScalar y){return x.CompareTo (y);});
      comparers[SortDirection.absdesc] = new Comparison<RCTimeScalar>(delegate(RCTimeScalar x, RCTimeScalar y){return 0-x.CompareTo (y);});
      m_comparers.Add ('t', comparers);
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoSort<byte> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoSort<byte> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoSort<long> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoSort<long> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoSort<double> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoSort<double> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoSort<decimal> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoSort<decimal> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoSort<bool> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoSort<bool> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoSort<string> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      runner.Yield (closure, new RCString (DoSort<string> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoSort<RCTimeScalar> (SortDirection.asc, right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoSort<RCTimeScalar> (ToDir (left), right)));
    }

    [RCVerb ("sort")]
    public void EvalSort (
      RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      SortDirection direction = ToDir (left);
      string col = (string) left[0].Part (1);
      RCVectorBase column = (RCVectorBase) right.Get (col);
      //It would be nice if there was an easy way to call one operator from another.
      //I tried to add one but found I would have to create a weird closure for this purpose.
      //So I decided to wait and see and live with the switch statement for now.
      RCLong rank;
      switch (column.TypeCode)
      {
        case 'x' : rank = new RCLong (Rank.DoRank<byte> (direction, (RCByte)column)); break;
        case 'l' : rank = new RCLong (Rank.DoRank<long> (direction, (RCLong)column)); break;
        case 'd' : rank = new RCLong (Rank.DoRank<double> (direction, (RCDouble)column)); break;
        case 'm' : rank = new RCLong (Rank.DoRank<decimal> (direction, (RCDecimal)column)); break;
        case 's' : rank = new RCLong (Rank.DoRank<string> (direction, (RCString)column)); break;
        case 'b' : rank = new RCLong (Rank.DoRank<bool> (direction, (RCBoolean)column)); break;
        case 'y' : rank = new RCLong (Rank.DoRank<RCSymbolScalar> (direction, (RCSymbol)column)); break;
        case 't' : rank = new RCLong (Rank.DoRank<RCTimeScalar> (direction, (RCTime)column)); break;
        default: throw new Exception ("Type:" + column.TypeCode + " is not supported by sort");
      }

      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock name = right.GetName (i);
        column = (RCVectorBase) name.Value;
        RCValue reordered;
        switch (column.TypeCode)
        {
          case 'x' : reordered = ReorderColumn<byte> (rank, (RCVector<byte>)column); break;
          case 'l' : reordered = ReorderColumn<long> (rank, (RCVector<long>)column); break;
          case 'd' : reordered = ReorderColumn<double> (rank, (RCVector<double>)column); break;
          case 'm' : reordered = ReorderColumn<decimal> (rank, (RCVector<decimal>)column); break;
          case 's' : reordered = ReorderColumn<string> (rank, (RCVector<string>)column); break;
          case 'b' : reordered = ReorderColumn<bool> (rank, (RCVector<bool>)column); break;
          case 'y' : reordered = ReorderColumn<RCSymbolScalar> (rank, (RCVector<RCSymbolScalar>)column); break;
          case 't' : reordered = ReorderColumn<RCTimeScalar> (rank, (RCVector<RCTimeScalar>)column); break;
          default: throw new Exception ("Type:" + column.TypeCode + " is not supported by sort");
        }
        result = new RCBlock (result, name.Name, ":", reordered);
      }

      runner.Yield (closure, result);
    }

    protected RCCube SortCubeByDirectionAndColumn (RCSymbol left, RCCube right)
    {
      if (right.Count == 0)
      {
        return right;
      }

      SortDirection direction = Sort.ToDir (left);
      string name = (string) left[0].Part (1);
      ColumnBase sortCol = right.GetColumn (name);
      RCArray<ColumnBase> columns = new RCArray<ColumnBase> ((int) right.Cols);
      long[] rank;

      if (sortCol == null)
      {
        switch (name)
        {
          case "G" : rank = Rank.DoRank<long> (direction, 'l', right.Axis.Global); break;
          case "E" : rank = Rank.DoRank<long> (direction, 'l', right.Axis.Event); break;
          case "T" : rank = Rank.DoRank<RCTimeScalar> (direction, 't', right.Axis.Time); break;
          case "S" : rank = Rank.DoRank<RCSymbolScalar> (direction, 's', right.Axis.Symbol); break;
          default: throw new Exception ("Unknown timeline column: " + name);
        }
      }
      else
      {
        switch (sortCol.TypeCode)
        {
          case 'x' : rank = Rank.DoRank<byte> (direction, sortCol.TypeCode, (RCArray<byte>) sortCol.Array); break;
          case 'l' : rank = Rank.DoRank<long> (direction, sortCol.TypeCode, (RCArray<long>) sortCol.Array); break;
          case 'd' : rank = Rank.DoRank<double> (direction, sortCol.TypeCode, (RCArray<double>) sortCol.Array); break;
          case 'm' : rank = Rank.DoRank<decimal> (direction, sortCol.TypeCode, (RCArray<decimal>) sortCol.Array); break;
          case 's' : rank = Rank.DoRank<string> (direction, sortCol.TypeCode, (RCArray<string>) sortCol.Array); break;
          case 'b' : rank = Rank.DoRank<bool> (direction, sortCol.TypeCode, (RCArray<bool>) sortCol.Array); break;
          case 'y' : rank = Rank.DoRank<RCSymbolScalar> (direction, sortCol.TypeCode, (RCArray<RCSymbolScalar>) sortCol.Array); break;
          case 't' : rank = Rank.DoRank<RCTimeScalar> (direction, sortCol.TypeCode, (RCArray<RCTimeScalar>) sortCol.Array); break;
          default: throw new Exception ("Type:" + sortCol.TypeCode + " is not supported by sort");
        }
      }

      int[] rowRank = new int[rank.Length];
      if (sortCol == null)
      {
        for (int i = 0; i < rowRank.Length; ++i)
        {
          rowRank[i] = (int) rank[i];
        }
      }
      else
      {
        for (int i = 0; i < rowRank.Length; ++i)
        {
          rowRank[i] = sortCol.Index[(int) rank[i]];
        }
      }
      Dictionary<long, int> map = new Dictionary<long, int> ();
      for (int i = 0; i < rowRank.Length; ++i)
      {
        map[rowRank[i]] = i;
      }

      Timeline axis;
      RCArray<long> g = right.Axis.Global;
      RCArray<long> gNew = null;
      RCArray<long> e = right.Axis.Event;
      RCArray<long> eNew = null;
      RCArray<RCTimeScalar> t = right.Axis.Time;
      RCArray<RCTimeScalar> tNew = null;
      RCArray<RCSymbolScalar> s = right.Axis.Symbol;
      RCArray<RCSymbolScalar> sNew = null;
      if (right.Axis.Global != null)
      {
        RCArray<int> ignore;
        DoArray (g, null, map, right.Axis.Count, out gNew, out ignore);
      }
      if (right.Axis.Event != null)
      {
        RCArray<int> ignore;
        DoArray (e, null, map, right.Axis.Count, out eNew, out ignore);
      }
      if (right.Axis.Time != null)
      {
        RCArray<int> ignore;
        DoArray (t, null, map, right.Axis.Count, out tNew, out ignore);
      }
      if (right.Axis.Symbol != null)
      {
        RCArray<int> ignore;
        DoArray (s, null, map, right.Axis.Count, out sNew, out ignore);
      }
      if (gNew == null && eNew == null && tNew == null && sNew == null)
      {
        axis = new Timeline (right.Axis.Count);
      }
      else
      {
        axis = new Timeline (gNew, eNew, tNew, sNew);
      }

      int lastRow = map.Count;
      for (int i = 0; i < axis.Count; ++i)
      {
        if (!map.ContainsKey (i))
        {
          map[i] = lastRow;
          ++lastRow;
        }
      }

      for (int col = 0; col < right.Cols; ++col)
      {
        ColumnBase oldcol = right.GetColumn (col);
        ColumnBase newcol = null;
        switch (oldcol.TypeCode)
        {
          case 'x' : newcol = DoColumn<byte> (oldcol, map, axis); break;
          case 'l' : newcol = DoColumn<long> (oldcol, map, axis); break;
          case 'd' : newcol = DoColumn<double> (oldcol, map, axis); break;
          case 'm' : newcol = DoColumn<decimal> (oldcol, map, axis); break;
          case 's' : newcol = DoColumn<string> (oldcol, map, axis); break;
          case 'b' : newcol = DoColumn<bool> (oldcol, map, axis); break;
          case 'y' : newcol = DoColumn<RCSymbolScalar> (oldcol, map, axis); break;
          case 't' : newcol = DoColumn<RCTimeScalar> (oldcol, map, axis); break;
          default: throw new Exception ("Type:" + newcol.TypeCode + " is not supported by sort");
        }
        columns.Write (newcol);
      }

      RCArray<string> names = new RCArray<string> (columns.Count);
      for (int i = 0; i < right.Cols; ++i)
      {
        names.Write (right.NameAt (i));
      }
      RCCube result = new RCCube (axis, names, columns);
      return result;
    }

    [RCVerb ("sort")]
    public void EvalSort (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      runner.Yield (closure, SortCubeByDirectionAndColumn (left, right));
    }

    protected RCValue ReorderColumn<T> (RCLong rank, RCVector<T> column) where T : IComparable<T>
    {
      return RCVectorBase.FromArray (
        VectorMath.MonadicOp<long, T> (
          rank.Data, delegate (long r) { return column[(int)r]; }));
    }

    public static SortDirection ToDir (RCSymbol left)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left argument must be exactly one of #asc #desc #absasc #absdesc");
      }
      SortDirection result = (SortDirection)Enum.Parse (typeof(SortDirection), (string)left[0].Part (0));
      return result;
    }

    protected virtual T[] DoSort<T> (SortDirection direction, RCVector<T> vector)
    {
      Comparison<T> comparison = (Comparison<T>) m_comparers[vector.TypeCode][direction];
      T[] array = vector.ToArray ();
      Array.Sort (array, comparison);
      return array;
    }

    protected void DoArray<T> (RCArray<T> inputData,
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
        //Any rows that are lacking values in the sort column will be missing from map.
        //Push these rows so that they come after all the rows with valid values in the sort column.
        //But importantly, they are kept in the original order.
        for (int j = 0; j < inputIndex.Count; ++j)
        {
          int newRow;
          if (!map.TryGetValue (inputIndex[j], out newRow))
          {
            throw new Exception (string.Format ("inputIndex[j]: {0} was not represented in map!", inputIndex[j]));
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
      //Now rank the values in im ascending.
      long[] rim = Rank.DoRank<long> (SortDirection.asc, 'l', im);
      for (int j = 0; j < rim.Length; ++j)
      {
        fd[j] = inputData[(int) rim[j]];
        fi[j] = (int) im[(int) rim[j]];
      }
      index = new RCArray<int> (fi);
      data = new RCArray<T> (fd);
    }

    protected ColumnBase DoColumn<T> (ColumnBase oldcol, Dictionary<long, int> map, Timeline axis)
    {
      RCArray<int> index;
      RCArray<T> data;
      DoArray ((RCArray<T>) oldcol.Array, oldcol.Index, map, axis.Count, out data, out index);
      ColumnBase result = ColumnBase.FromArray (axis, index, data);
      return result;
    }
  }
}

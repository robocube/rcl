using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class CubeMath
  {
    protected static readonly Dictionary <RCActivator.OverloadKey, Overload> m_overloads =
      new Dictionary<RCActivator.OverloadKey, Overload> ();

    static CubeMath ()
    {
      try
      {
        foreach (MethodInfo method in typeof (ScalarMath).GetMethods ())
        {
          object[] attributes = method.GetCustomAttributes (typeof (Primitive), false);
          for (int i = 0; i < attributes.Length; ++i)
          {
            Primitive verb = (Primitive) attributes[i];
            ParameterInfo[] parameters = method.GetParameters ();
            try
            {
              if (verb.Profile == Profile.Monadic)
              {
                Type rtype = parameters[0].ParameterType;
                Type otype = method.ReturnType;
                RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, typeof (RCCube), null, rtype);

                Type vectoroptype = typeof (CubeMath).GetNestedType ("CubeOp`2").
                  MakeGenericType (rtype, otype);
                //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
                MethodInfo vectoropMethod = typeof (CubeMath).GetMethod ("MonadicOp").
                  MakeGenericMethod (rtype, otype);
                Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
                Type primoptype = typeof (CubeMath).GetNestedType ("ScalarOp`2").
                  MakeGenericType (rtype, otype);
                Delegate primop = Delegate.CreateDelegate (primoptype, method);

                if (m_overloads.ContainsKey (key))
                  throw new Exception ("dispatch table already contains the key:" + key);
                m_overloads.Add (key, new Overload (vectorop, primop));
              }
              else if (verb.Profile == Profile.Dyadic)
              {
                Type ltype = parameters[0].ParameterType;
                Type rtype = parameters[1].ParameterType;
                Type otype = method.ReturnType;

                Type latype = typeof (RCVector<>).MakeGenericType (ltype);
                Type ratype = typeof (RCVector<>).MakeGenericType (rtype);

                RCActivator.OverloadKey key0 = new RCActivator.OverloadKey (
                  verb.Name, typeof (RCCube), ltype, rtype);
                Type cubeOpType = typeof (CubeMath).GetNestedType ("CubeOp`3").
                  MakeGenericType (ltype, rtype, otype);
                //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
                MethodInfo cubeOpMethod = typeof (CubeMath).GetMethod ("DyadicOp").
                  MakeGenericMethod (ltype, rtype, otype);
                Delegate cubeOp = Delegate.CreateDelegate (cubeOpType, cubeOpMethod);
                Type scalarOpType = typeof (CubeMath).GetNestedType ("ScalarOp`3").
                  MakeGenericType (ltype, rtype, otype);
                Delegate scalarOp = Delegate.CreateDelegate (scalarOpType, method);
                m_overloads.Add (key0, new Overload (cubeOp, scalarOp));
                //Problem is that all of the cube overloads will be duplicates.
                //we could hack it by using the scalar types for the cube overloads.
                //or we could introduce a new type of key that includes both the scalar type
                //and the collection type -- what to do?...

                RCActivator.OverloadKey key1 = new RCActivator.OverloadKey (
                  verb.Name, typeof (RCCube), latype, rtype);
                cubeOpType = typeof (CubeMath).GetNestedType ("CubeOpLeftScalar`3").
                  MakeGenericType (ltype, rtype, otype);
                cubeOpMethod = typeof (CubeMath).GetMethod ("DyadicOpLeftScalar").
                  MakeGenericMethod (ltype, rtype, otype);
                cubeOp = Delegate.CreateDelegate (cubeOpType, cubeOpMethod);
                m_overloads.Add (key1, new Overload (cubeOp, scalarOp));

                RCActivator.OverloadKey key2 = new RCActivator.OverloadKey (
                  verb.Name, typeof (RCCube), ltype, ratype);
                cubeOpType = typeof (CubeMath).GetNestedType ("CubeOpRightScalar`3").
                  MakeGenericType (ltype, rtype, otype);
                cubeOpMethod = typeof (CubeMath).GetMethod ("DyadicOpRightScalar").
                  MakeGenericMethod (ltype, rtype, otype);
                cubeOp = Delegate.CreateDelegate (cubeOpType, cubeOpMethod);
                m_overloads.Add (key2, new Overload (cubeOp, scalarOp));
              }
              else if (verb.Profile == Profile.Sequential)
              {
                //The parameter is a ref parameter which is actually a different type than the type
                //which is being referenced.  GetElementType () gives you the referenced type which is
                //what we want when constructing generic methods.
                Type stype = parameters[0].ParameterType.GetElementType ();
                Type rtype = parameters[1].ParameterType;
                Type otype = method.ReturnType;

                // The monadic version without modifiers
                RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, typeof (RCCube), null, rtype);
                Type vectoroptype = typeof (CubeMath).GetNestedType ("SeqCubeOpMonadic`3").
                  MakeGenericType (stype, rtype, otype);
                //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
                MethodInfo vectoropMethod = typeof (CubeMath).GetMethod ("SequentialOpMonadic").
                  MakeGenericMethod (stype, rtype, otype);
                Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
                Type primoptype = typeof (CubeMath).GetNestedType ("SeqScalarOp`3").
                  MakeGenericType (stype, rtype, otype);
                Delegate primop = Delegate.CreateDelegate (primoptype, method);
                if (m_overloads.ContainsKey (key))
                {
                  throw new Exception ("dispatch table already contains the key:" + key);
                }
                m_overloads.Add (key, new Overload (vectorop, primop));

                // The dyadic version allows passing a vector on the left
                RCActivator.OverloadKey key1 = new RCActivator.OverloadKey (verb.Name, typeof (RCCube), typeof (RCVector<long>), rtype);
                Type vectoroptype1 = typeof (CubeMath).GetNestedType ("SeqCubeOpDyadic`3").
                  MakeGenericType (stype, rtype, otype);
                MethodInfo vectoropMethod1 = typeof (CubeMath).GetMethod ("SequentialOpDyadic").
                  MakeGenericMethod (stype, rtype, otype);
                Delegate vectorop1 = Delegate.CreateDelegate (vectoroptype1, vectoropMethod1);
                if (m_overloads.ContainsKey (key1))
                {
                  throw new Exception ("dispatch table already contains the key:" + key1);
                }
                m_overloads.Add (key1, new Overload (vectorop1, primop));

                // The monadic version for BLOCKS
                RCActivator.OverloadKey key2 = new RCActivator.OverloadKey (verb.Name, typeof (RCBlock), null, rtype);
                Type blockOpType = typeof (CubeMath).GetNestedType ("SeqBlockOpMonadic`3").
                  MakeGenericType (stype, rtype, otype);
                //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
                MethodInfo blockOpMethod = typeof (CubeMath).GetMethod ("SequentialOpBlockMonadic").
                  MakeGenericMethod (stype, rtype, otype);
                Delegate blockOp = Delegate.CreateDelegate (blockOpType, blockOpMethod);
                if (m_overloads.ContainsKey (key2))
                {
                  throw new Exception ("dispatch table already contains the key:" + key2);
                }
                m_overloads.Add (key2, new Overload (blockOp, primop));

                // There is no need for a "dyadic sequential" operation like "1 sum {x:1 2 3 y:4 5 6}"
                // This only comes into play in the case of cubes.
              }
              else if (verb.Profile == Profile.Contextual)
              {
                //The parameter is a ref parameter which is actually a different type than the type
                //which is being referenced.  GetElementType () gives you the referenced type which is
                //what we want when constructing generic methods.
                Type ctype = parameters[0].ParameterType;
                Type ltype = ctype.GetGenericArguments()[0];
                Type rtype = parameters[1].ParameterType;
                Type otype = method.ReturnType;
                //The stype is not a parameter to the operator so it is not included in the key.
                Type latype = typeof (RCVector<>).MakeGenericType (ltype);
                RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, typeof (RCCube), latype, rtype);

                Type vectoroptype = typeof (CubeMath).GetNestedType ("ConCubeOp`4").
                  MakeGenericType (ctype, ltype, rtype, otype);
                //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
                MethodInfo vectoropMethod = typeof (CubeMath).GetMethod ("ContextualOp").
                  MakeGenericMethod (ctype, ltype, rtype, otype);
                Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
                Type primoptype = typeof (CubeMath).GetNestedType ("ConScalarOp`4").
                  MakeGenericType (ctype, ltype, rtype, otype);
                Delegate primop = Delegate.CreateDelegate (primoptype, method);

                if (m_overloads.ContainsKey (key))
                {
                  throw new Exception ("dispatch table already contains the key:" + key);
                }
                m_overloads.Add (key, new Overload (vectorop, primop));
              }
            }
            finally {}
          }
        }
      }
      finally {}
    }

    protected class IndexPair<O> where O : IComparable
    {
      public IndexPair (int li, int ri)
      {
        LI = li;
        RI = ri;
      }
      public int LI;
      public int RI;
      public O Result;
    }

    public delegate O ScalarOp <R, O> (R r);
    public delegate O ScalarOp <L, R, O> (L l, R r);
    public delegate O SeqScalarOp <S, R, O> (ref S s, R r);
    public delegate O ConScalarOp <C, L, R, O> (C c, R r);

    public delegate RCValue CubeOp <R, O> (RCCube right, ScalarOp <R, O> op);
    public delegate RCValue CumCubeOp <R, O> (RCCube right, ScalarOp <R, R, O> op);
    public delegate RCValue CubeOp <L, R, O> (RCCube left, RCCube right, ScalarOp<L, R, O> op);
    public delegate RCValue CubeOpLeftScalar <L, R, O> (RCVector<L> left, RCCube right, ScalarOp<L, R, O> op);
    public delegate RCValue CubeOpRightScalar <L, R, O> (RCCube left, RCVector<R> right, ScalarOp<L, R, O> op);

    public delegate RCValue SeqCubeOpMonadic <S, R, O> (RCCube right, SeqScalarOp<S, R, O> op);
    public delegate RCValue SeqCubeOpDyadic <S, R, O> (RCLong left, RCCube right, SeqScalarOp<S, R, O> op);
    public delegate RCValue SeqBlockOpMonadic <S, R, O> (RCBlock right, SeqScalarOp<S, R, O> op);
    public delegate RCValue SeqBlockOpDyadic <S, R, O> (RCLong left, RCBlock right, SeqScalarOp<S, R, O> op);
    public delegate RCValue ConCubeOp <C, L, R, O> (RCVector<L> left, RCCube right, ConScalarOp<C, L, R, O> op);

    public static RCValue MonadicOp <R, O> (RCCube right, ScalarOp<R, O> op)
      where O : IComparable
    {
      Comparer<O> c = Comparer<O>.Default;
      Dictionary<RCSymbolScalar, IndexPair<O>> map = new Dictionary<RCSymbolScalar, IndexPair<O>> ();
      RCArray<int> rindex = right.GetIndex<R> (0);
      RCArray<R> rdata = right.GetData<R> (0);
      RCArray<O> data = new RCArray<O> ();
      RCArray<int> index = new RCArray<int> ();

      if (right.Axis.ColCount == 0)
      {
        for (int i = 0; i < rdata.Count; ++i)
        {
          O value = op (rdata[i]);
          index.Write (rindex[i]);
          data.Write (value);
        }
      }
      else //if (right.Axis.Symbol != null && right.Axis.ColCount == 1)
      {
        for (int i = 0; i < rdata.Count; ++i)
        {
          IndexPair<O> pair;
          RCSymbolScalar symbol = right.SymbolAt (i);
          int rt = rindex[i];
          map.TryGetValue (symbol, out pair);
          bool first;
          if (first = pair == null)
          {
            pair = new IndexPair<O> (-1, i);
            map.Add (symbol, pair);
          }
          pair.RI = i;
          O value = op (rdata[pair.RI]);
          if (first || c.Compare (value, pair.Result) != 0)
          {
            pair.Result = value;
            index.Write (rt);
            data.Write (pair.Result);
          }
        }
      }
      if (data.Count > 0)
      {
        ColumnBase column = ColumnBase.FromArray (right.Axis, index, data);
        return new RCCube (
          right.Axis,
          new RCArray<string> ("x"),
          new RCArray<ColumnBase> (column));
      }
      else return new RCCube ();
    }

    public static RCValue ContextualOp <C, L, R, O> (RCVector<L> left, RCCube right, ConScalarOp<C, L, R, O> op)
      where O : IComparable where C : Context<L>, new ()
    {
      C c = new C ();
      c.Init (left.Data);
      //This is a bit of a weird hack however
      return MonadicOp<R, O> (right, delegate (R r) { return op (c, r); });
    }

    /// <summary>
    /// Columnar evaluation looks at two vectors and operates on them together
    /// in a row by row fashion.  Choose between the multiple timeline, single
    /// timeline, or no timeline variations of the algorithm.
    /// </summary>
    public static RCValue DyadicOp <L, R, O> (RCCube left, RCCube right, ScalarOp<L, R, O> op)
      where O : IComparable
    {
      Timeline ltimeline = left.Axis;
      RCArray<int> lindex = left.GetIndex<L> (0);
      RCArray<L> ldata = left.GetData<L> (0);
      Timeline rtimeline = right.Axis;
      RCArray<int> rindex = right.GetIndex<R> (0);
      RCArray<R> rdata = right.GetData<R> (0);
      Timeline timeline = null;
      RCArray<int> index = null;
      RCArray<O> data = null;

      if (left.Axis.Colset.Count == 0 && right.Axis.Colset.Count == 0)
      {
        data = new RCArray<O> ();
        timeline = left.Axis;
        index = new RCArray<int> ();
        EvalColumnarNone<L, R, O> (left, right, index, data, op);
      }
      else if (left.Axis.Symbol != null && left.Axis.ColCount == 1 &&
               right.Axis.Symbol != null && right.Axis.ColCount == 1)
      {
        data = new RCArray<O> ();
        timeline = left.Axis.Match ();
        index = new RCArray<int> ();
        Column<R> rcolumn = (Column<R>) right.GetColumn (0);
        Column<L> lcolumn = (Column<L>) left.GetColumn (0);
        //Dictionary<RCSymbolScalar, R> rlast = right.GetLast<R> (0);
        //Dictionary<RCSymbolScalar, L> llast = left.GetLast<L> (0);
        EvalColumnarObject<L, R, O> (ltimeline, lcolumn,
                                     rtimeline, rcolumn,
                                     timeline, index, data, op);
      }
      else if (left.Axis != right.Axis)
      {
        //left.Axis and right.Axis should have the same set of timeline columns.
        //otherwise semantics get hairy.  For simplicity here let's make the
        //new timeline match the left one argument and just assume that the right one
        //is matching it.
        data = new RCArray<O> ();
        timeline = left.Axis.Match ();
        index = new RCArray<int> ();
        EvalColumnarMulti<L, R, O> (
          ltimeline, lindex, ldata,
          rtimeline, rindex, rdata,
          timeline, index, data, op);
      }
      else //if (left.Timeline == right.Timeline)
      {
        //If both vectors are on the same, single timeline
        //the EvalSharedTimeline routine will ensure that
        //duplicate values in the input will be handled consistently
        //with that input.
        data = new RCArray<O> ();
        index = new RCArray<int> ();
        timeline = left.Axis;
        EvalColumnarSingle<L, R, O> (
          ltimeline, lindex, ldata, rindex, rdata, index, data, op);
      }

      //The only way I could find to eliminate this cast was unnacceptable
      //syntactically, you have to declare all of the side arrays in the
      //calling routine.  You can bury the cast inside of the EmptyOf routine
      //but I don't want to do that.
      if (data.Count > 0)
      {
        ColumnBase column = ColumnBase.FromArray (timeline, index, data);
        return new RCCube (
          timeline, new RCArray<string> ("x"), new RCArray<ColumnBase> (column));
      }
      else return new RCCube ();
    }

    public static RCValue DyadicOpLeftScalar <L, R, O> (
      RCVector<L> left, RCCube right, ScalarOp<L, R, O> op)
      where O : IComparable
    {
      //This is an easier way to write this because the cube already
      //handles duplicate values for you.  I think this whole thing should
      //use WriteCell instead of manipulating the underlying vectors directly.
      Timeline rtimeline = right.Axis;
      RCArray<int> rindex = right.GetIndex<R> (0);
      RCArray<R> rdata = right.GetData<R> (0);
      RCCube result = new RCCube (
        rtimeline, new RCArray<string> (), new RCArray<ColumnBase> ());
      for (int i = 0; i < rdata.Count; ++i)
      {
        O val = op (left[0], rdata[i]);
        result.WriteCell ("x", right.SymbolAt (i), val, rindex[i], false, false);
      }
      return result;
    }

    public static RCValue DyadicOpRightScalar <L, R, O> (
      RCCube left, RCVector<R> right, ScalarOp<L, R, O> op)
      where O : IComparable
    {
      //This is an easier way to write this because the cube already
      //handles duplicate values for you.  I think this whole thing should
      //use WriteCell instead of manipulating the underlying vectors directly.
      Timeline ltimeline = left.Axis;
      RCArray<int> lindex = left.GetIndex<L> (0);
      RCArray<L> ldata = left.GetData<L> (0);
      RCCube result = new RCCube (
        ltimeline, new RCArray<string> (), new RCArray<ColumnBase> ());
      for (int i = 0; i < ldata.Count; ++i)
      {
        O val = op (ldata[i], right[0]);
        result.WriteCell ("x", left.SymbolAt (i), val, lindex[i], false, false);
      }
      return result;
    }

    /// <summary>
    /// This function computes a new RCWorkspace using two vectors that
    /// are already aligned to a single RCTimeline.
    /// The parser removes any duplicate values in an RCWorkspace when it is parsed.
    /// This requires some special finagelling that is different from the case
    /// of merging two totally different timelines.  This is important when you want
    /// to look at a cube as a series of "transactions" or events, rather than
    /// as a series of observations of a variable through time.
    /// </summary>
    protected static void EvalColumnarSingle <L, R, O> (
      Timeline timeline,
      RCArray<int> lindex, RCArray<L> ldata,
      RCArray<int> rindex, RCArray<R> rdata,
      RCArray<int> index, RCArray<O> data,
      ScalarOp<L, R, O> op) where O : IComparable
    {
      //RCTimeline timeline = left.Timeline;
      //The resulting values go into this vector.
      Comparer<O> c = Comparer<O>.Default;

      //li and ri are the current indices on the left vector and the right
      //vector respectively.
      //THESE ARE NOT INDICES INTO THE TIMELINE.
      int li = 0, ri = 0;
      //lt and rt are the indices into the timeline.
      //THESE ARE NOT TIMESTAMPS
      //The indices work better in this case because both arrays are setup on the same timeline.
      int lt = -1, rt = -1;

      //This map stores the current index into the left and right arguments.
      Dictionary<RCSymbolScalar, IndexPair<O>> map =
        new Dictionary<RCSymbolScalar, IndexPair<O>>();

      while (true)
      {
        //check to see if there is more to do.
        bool hasLeft = li < ldata.Count;
        bool hasRight = ri < rdata.Count;
        //Notice we don't really use timestamps but indices in the timeline instead.
        if (hasLeft) lt = lindex[li];
        if (hasRight) rt = rindex[ri];
        if (!(hasLeft || hasRight)) break;

        RCSymbolScalar symbol = null;
        IndexPair<O> pair = null;

        if (lt == rt)
        {
          symbol = timeline.SymbolAt (rt);
          map.TryGetValue (symbol, out pair);
          if (pair == null)
          {
            pair = new IndexPair<O> (li, ri);
            map.Add (symbol, pair);
          }
          pair.RI = ri;
          pair.LI = li;
          O value = op (ldata[li], rdata[ri]);
          if (c.Compare (value, pair.Result) != 0)
          {
            pair.Result = value;
            data.Write (pair.Result);
            index.Write (rt);
          }
          ++li; ++ri;
        }
        else if (hasRight && (rt < lt || !hasLeft))
        {
          symbol = timeline.SymbolAt (rt);
          map.TryGetValue (symbol, out pair);
          if (pair == null)
          {
            pair = new IndexPair<O> (-1, ri);
            map.Add (symbol, pair);
          }
          else if (pair.LI >= 0)
          {
            pair.RI = ri;
            O value = op (ldata[pair.LI], rdata[pair.RI]);
            if (c.Compare (value, pair.Result) != 0)
            {
              pair.Result = value;
              data.Write (pair.Result);
              index.Write (rt);
            }
          }
          ++ri;
        }
        else if (hasLeft && (lt < rt || !hasRight))
        {
          symbol = timeline.SymbolAt (lt);
          map.TryGetValue (symbol, out pair);
          if (pair == null)
          {
            pair = new IndexPair<O> (li, -1);
            map.Add (symbol, pair);
          }
          else if (pair.RI >= 0)
          {
            pair.LI = li;
            O value = op (ldata[pair.LI], rdata[pair.RI]);
            if (c.Compare (value, pair.Result) != 0)
            {
              pair.Result = value;
              data.Write (pair.Result);
              index.Write (lt);
            }
          }
          ++li;
        }
        else throw new InvalidOperationException ();
      }
    }

    protected static void EvalColumnarNone<L, R, O> (
      RCCube left, RCCube right,
      RCArray<int> index, RCArray<O> data,
      ScalarOp<L, R, O> op)
    {
      //Neither cube may have a timeline.
      if (left.Axis.Colset.Count > 0 || right.Axis.Colset.Count > 0)
      {
        throw new Exception ("Neither cube may have timeline columns.");
      }
      //Both cubes must have the same number of rows.
      if (left.Count != right.Count)
      {
        throw new Exception ("Both cubes must have the same count.");
      }
      //And the same number of columns.
      if (left.Cols != right.Cols)
      {
        throw new Exception ("Both cubes must have the same number of columns.");
      }

      if (left.Cols == 1 && right.Cols == 1)
      {
        RCArray<int> leftIndex = left.GetIndex<L> (0);
        RCArray<L> leftData = left.GetData<L> (0);
        RCArray<int> rightIndex = right.GetIndex<R> (0);
        RCArray<R> rightData = right.GetData<R> (0);
        for (int i = 0; i < left.Axis.Count; ++i)
        {
          bool leftFound;
          int leftIndexRow = leftIndex.BinarySearch (i, out leftFound);
          bool rightFound;
          int rightIndexRow = rightIndex.BinarySearch (i, out rightFound);
          if (leftFound && rightFound)
          {
            L leftScalar = leftData[leftIndexRow];
            R rightScalar = rightData[rightIndexRow];
            data.Write (op (leftScalar, rightScalar));
            index.Write (i);
          }
        }
      }
      else
      {
        throw new Exception ("Implement matched column operations.");
      }
    }

    protected static void EvalColumnarObject<L, R, O> (
      Timeline ltimeline, Column<L> llast,
      Timeline rtimeline, Column<R> rlast,
      Timeline otimeline, RCArray<int> oindex, RCArray<O> odata,
      ScalarOp<L, R, O> op) where O : IComparable
    {
      HashSet<RCSymbolScalar> added = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < ltimeline.Count; ++i)
      {
        if (!added.Contains (ltimeline.Symbol[i]))
        {
          otimeline.Write (ltimeline.Symbol[i]);
          added.Add (ltimeline.Symbol[i]);
        }
      }
      for (int i = 0; i < rtimeline.Count; ++i)
      {
        if (!added.Contains (rtimeline.Symbol[i]))
        {
          otimeline.Write (rtimeline.Symbol[i]);
          added.Add (rtimeline.Symbol[i]);
        }
      }
      for (int i = 0; i < otimeline.Count; ++i)
      {
        RCSymbolScalar s = otimeline.Symbol[i];
        L l;
        R r;
        if (llast.Last (s, out l))
        {
          if (rlast.Last (s, out r))
          {
            oindex.Write (i);
            odata.Write (op (l, r));
          }
        }
      }
    }

    protected static void EvalColumnarMulti<L, R, O> (
      Timeline ltimeline, RCArray<int> lindex, RCArray<L> ldata,
      Timeline rtimeline, RCArray<int> rindex, RCArray<R> rdata,
      Timeline otimeline, RCArray<int> oindex, RCArray<O> odata,
      ScalarOp<L, R, O> op) where O : IComparable
    {
      Comparer<O> c = Comparer<O>.Default;
      //li and ri are the current indices on the left vector and the right
      //vector respectively.  lt is the index of the timeline of the left
      int li = 0, ri = 0;
      long lt = -1;
      long rt = -1;

      //This map stores the current index into the left and right arguments.
      Dictionary<RCSymbolScalar, IndexPair<O>> map =
        new Dictionary<RCSymbolScalar, IndexPair<O>> ();
      while (true)
      {
        //check to see if there is more to do.
        bool hasLeft = li < ldata.Count;
        bool hasRight = ri < rdata.Count;
        if (hasLeft) lt = ltimeline.TimeAt (lindex[li]);
        if (hasRight) rt = rtimeline.TimeAt (rindex[ri]);
        if (!(hasLeft || hasRight))
        {
          break;
        }
        RCSymbolScalar symbol = null;
        IndexPair<O> pair = null;
        if (lt == rt)
        {
          symbol = rtimeline.SymbolAt (rindex[ri]);
          if (symbol == null)
          {
            symbol = RCSymbolScalar.Empty;
          }
          bool first = !map.TryGetValue (symbol, out pair);
          if (pair == null)
          {
            pair = new IndexPair<O> (li, ri);
            map.Add (symbol, pair);
          }
          pair.RI = ri;
          pair.LI = li;
          O value = op (ldata[li], rdata[ri]);
          if (first || c.Compare (value, pair.Result) != 0)
          {
            pair.Result = value;
            oindex.Write (otimeline.Count);
            odata.Write (pair.Result);
            otimeline.Write (lt, symbol);
          }
          ++li; ++ri;
        }
        else if (hasRight && (rt < lt || !hasLeft))
        {
          //I think we need the "first" logic here as well.
          //booleans are especially problematic.
          //symbol = right.SymbolAt (ri);
          symbol = rtimeline.SymbolAt (rindex[ri]);
          map.TryGetValue (symbol, out pair);
          if (pair == null)
          {
            pair = new IndexPair<O> (-1, ri);
            map.Add (symbol, pair);
          }
          else if (pair.LI >= 0)
          {
            //On the first time through there will be nothing
            //to write.  What we really want here is "second".
            pair.RI = ri;
            O value = op (ldata[pair.LI], rdata[pair.RI]);
            if (pair.RI == 0 || c.Compare (value, pair.Result) != 0)
            {
              pair.Result = value;
              oindex.Write (otimeline.Count);
              odata.Write (pair.Result);
              long t = Math.Max (
                ltimeline.TimeAt (lindex[pair.LI]),
                rtimeline.TimeAt (rindex[pair.RI]));
              otimeline.Write (t, symbol);
            }
          }
          ++ri;
        }
        else if (hasLeft && (lt < rt || !hasRight))
        {
          symbol = ltimeline.SymbolAt (lindex[li]);
          map.TryGetValue (symbol, out pair);
          if (pair == null)
          {
            pair = new IndexPair<O> (li, -1);
            map.Add(symbol, pair);
          }
          else if (pair.RI >= 0)
          {
            pair.LI = li;
            O value = op (ldata[pair.LI], rdata[pair.RI]);
            if (pair.LI == 0 || c.Compare (value, pair.Result) != 0)
            {
              pair.Result = value;
              oindex.Write (otimeline.Count);
              odata.Write (pair.Result);
              long t = Math.Max (
                ltimeline.TimeAt (lindex[pair.LI]),
                rtimeline.TimeAt (rindex[pair.RI]));
              otimeline.Write (t, symbol);
            }
          }
          ++li;
        }
        else throw new InvalidOperationException ();
      }
    }

    protected struct SeqState<S, O> { public S s; public O o; }

    public static RCValue SequentialOpMonadic<S, R, O> (RCCube right, SeqScalarOp<S, R, O> op)
      where S : struct where O : struct
    {
      return SequentialOpDyadic <S, R, O> (null, right, op);
    }

    public static RCValue SequentialOpBlockMonadic<S, R, O> (RCBlock right, SeqScalarOp<S, R, O> op)
    {
      int resultCount;
      RCArray<RCArray<R>> inputColumns = new RCArray<RCArray<R>> (right.Count);
      if (right.Count == 0)
      {
        throw new Exception ("right argument must be a block containing at least one vector");
      }
      else
      {
        RCVectorBase column = (RCVectorBase) right.Get (0);
        resultCount = column.Count;
        inputColumns.Write ((RCArray<R>) column.Array);
        for (int i = 1; i < right.Count; ++i)
        {
          column = (RCVectorBase) right.Get (i);
          int count = column.Count;
          if (resultCount != count)
          {
            throw new Exception (string.Format ("Expected all columns to have count {0}, but column {1} had count {2}", resultCount, i, count));
          }
          inputColumns.Write ((RCArray<R>) column.Array);
        }
      }
      RCArray<O> result = new RCArray<O> (resultCount);
      for (int i = 0; i < resultCount; ++i)
      {
        SeqState<S, O> state = new SeqState<S, O> ();
        for (int j = 0; j < inputColumns.Count; ++j)
        {
          state.o = op (ref state.s, inputColumns[j][i]);
        }
        result.Write (state.o);
      }
      return RCVectorBase.FromArray (result);
    }

    /// <summary>
    /// Now with multiple column support.
    /// </summary>
    public static RCValue SequentialOpDyadic<S, R, O> (RCLong left, RCCube right, SeqScalarOp<S, R, O> op)
      where S : struct where O : struct
    {
      if (right.Count == 0)
      {
        return right;
      }
      else if (right.Axis.Symbol != null && right.Axis.ColCount == 1)
      {
        RCArray<string> names = new RCArray<string> (right.Columns.Count);
        RCArray<ColumnBase> columns = new RCArray<ColumnBase> (right.Columns.Count);
        RCArray<Dictionary<RCSymbolScalar, SeqState<S,O>>> resultsByColumn =
          new RCArray<Dictionary<RCSymbolScalar, SeqState<S,O>>> (right.Columns.Count);
        RCArray<RCSymbolScalar> symbol = new RCArray<RCSymbolScalar> ();
        for (int col = 0; col < right.Columns.Count; ++col)
        {
          RCArray<int> rindex = right.GetIndex<R> (col);
          RCArray<R> rdata = right.GetData<R> (col);
          Dictionary<RCSymbolScalar, SeqState<S,O>> results = new Dictionary<RCSymbolScalar, SeqState<S,O>> ();
          int levels = left == null ? 0 : (int) left[0];
          for (int i = 0; i < rdata.Count; ++i)
          {
            RCSymbolScalar scalar = right.Axis.SymbolAt (rindex[i]);
            int level = (int) scalar.Length;
            int count = (int) scalar.Length;
            while (scalar != null)
            {
              SeqState<S,O> state;
              if (levels == 0 ||
                  (levels > 0 && level < levels) ||
                  (levels < 0 && (count - level) < Math.Abs (levels)))
              {
                if (!results.TryGetValue (scalar, out state))
                {
                  state = new SeqState<S,O> ();
                  results.Add (scalar, state);
                }
                O o = op (ref state.s, rdata[i]);
                state.o = o;
                results[scalar] = state;
                if (!symbol.Contains (scalar))
                {
                  symbol.Write (scalar);
                }
              }
              //We should change this to make it possible to say how many levels you want.
              //Specify the number from the beginning or the end using a negative number.
              scalar = scalar.Previous;
              --level;
            }
          }
          names.Write (right.NameAt (col));
          resultsByColumn.Write (results);
        }
        // Sort by symbol before writing the results
        symbol.SortInPlace ();
        Timeline axis = new Timeline (null, null, null, symbol);
        for (int col = 0; col < right.Columns.Count; ++col)
        {
          RCArray<O> data = new RCArray<O> (resultsByColumn[col].Count);
          RCArray<int> index = new RCArray<int> (resultsByColumn[col].Count);
          for (int i = 0; i < symbol.Count; ++i)
          {
            SeqState<S,O> state;
            if (symbol[i] == null)
            {
              throw new Exception (string.Format ("wtf symbol[{0}] was null", i));
            }
            if (resultsByColumn[col].TryGetValue (symbol[i], out state))
            {
              index.Write (i);
              data.Write (state.o);
            }
          }
          columns.Write (ColumnBase.FromArray (axis, index, data));
        }
        if (symbol.Count > 0)
        {
          return new RCCube (axis, names, columns);
        }
        else return new RCCube ();
      }
      else if (right.Axis.ColCount == 0)
      {
        RCArray<int> rindex = right.GetIndex<R> (0);
        RCArray<R> rdata = right.GetData<R> (0);
        RCArray<int> index = new RCArray<int> ();
        RCArray<O> data = new RCArray<O> ();
        Comparer<O> c = Comparer<O>.Default;
        SeqState<S,O> s = new SeqState<S,O> ();
        for (int i = 0; i < rdata.Count; ++i)
        {
          s.o = op (ref s.s, rdata[i]);
          index.Write (rindex[i]);
          data.Write (s.o);
        }
        if (data.Count > 0)
        {
          ColumnBase column = ColumnBase.FromArray (right.Axis, index, data);
          return new RCCube (
            right.Axis,
            new RCArray<string> ("x"),
            new RCArray<ColumnBase> (column));
        }
        else return new RCCube ();
      }
      else
      {
        int levels = left == null ? 0 : (int) left[0];
        RCArray<int> rindex = right.GetIndex<R> (0);
        RCArray<R> rdata = right.GetData<R> (0);
        Dictionary<RCSymbolScalar, SeqState<S,O>> states =
          new Dictionary<RCSymbolScalar, SeqState<S,O>> ();
        RCArray<int> index = new RCArray<int> ();
        RCArray<O> data = new RCArray<O> ();
        Comparer<O> c = Comparer<O>.Default;
        RCTimeScalar prevTime = right.Axis.RealTimeAt (rindex[0]);
        Timeline axis = new Timeline ("T", "S");
        RCArray<RCSymbolScalar> symbol = new RCArray<RCSymbolScalar> ();
        for (int i = 0; i < rdata.Count; ++i)
        {
          RCSymbolScalar scalar = right.Axis.SymbolAt (rindex[i]);
          RCTimeScalar time = right.Axis.RealTimeAt (rindex[i]);
          int timeCompare = time.CompareTo (prevTime);
          if (timeCompare < 0)
          {
            throw new Exception (string.Format ("Timeline out of order: time:{0} < prevTime:{1}", time, prevTime));
          }
          else if (timeCompare > 0)
          {
            symbol.SortInPlace ();
            for (int j = 0; j < symbol.Count; ++j)
            // Should we make aggregate rows appear AFTER their components in the time series case?
            // This would be inconsistent with sorting for non-time cubes.
            {
              SeqState<S, O> state = states[symbol[j]];
              axis.Write (prevTime, symbol[j]);
              index.Write (axis.Count - 1);
              data.Write (state.o);
            }
            symbol.Clear ();
            states.Clear ();
            prevTime = time;
          }
          //BEGIN NEW
          int level = (int) scalar.Length;
          int count = (int) scalar.Length;
          while (scalar != null)
          {
            SeqState<S,O> state;
            if (levels == 0 ||
                (levels > 0 && level < levels) ||
                (levels < 0 && (count - level) < Math.Abs (levels)))
            {
              if (!states.TryGetValue (scalar, out state))
              {
                state = new SeqState<S,O> ();
                states.Add (scalar, state);
              }
              O o = op (ref state.s, rdata[i]);
              state.o = o;
              states[scalar] = state;
              if (!symbol.Contains (scalar))
              {
                symbol.Write (scalar);
              }
            }
            //We should change this to make it possible to say how many levels you want.
            //Specify the number from the beginning or the end using a negative number.
            scalar = scalar.Previous;
            --level;
          }
          if (i == rdata.Count - 1)
          {
            symbol.SortInPlace ();
            // Should we make aggregate rows appear AFTER their components in the time series case?
            // This would be inconsistent with sorting for non-time cubes.
            for (int j = 0; j < symbol.Count; ++j)
            {
              SeqState<S, O> state = states[symbol[j]];
              axis.Write (prevTime, symbol[j]);
              index.Write (axis.Count - 1);
              data.Write (state.o);
            }
            symbol.Clear ();
            states.Clear ();
            prevTime = time;
          }
          //END NEW
        }
        if (data.Count > 0)
        {
          ColumnBase column = ColumnBase.FromArray (axis, index, data);
          return new RCCube (
            axis,
            new RCArray<string> ("x"),
            new RCArray<ColumnBase> (column));
        }
        else return new RCCube ();
      }
    }

    public static RCValue Invoke (RCClosure closure, string name, RCCube left, RCCube right)
    {
      //If the scalar type is used as the key it is assumed that the
      //argument will be a cube whose first column will be a vector of that scalar type.
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCCube), left.GetType (0), right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, left, right);
      //The result should almost always be a cube.
      //But there might be a couple exceptions.
      RCValue result = (RCValue) overload.Invoke (left, right);
      return result;
    }

    public static RCValue Invoke (RCClosure closure, string name, RCVectorBase left, RCCube right)
    {
      //Here BaseType is always RCVector<T>.
      //This is a bit fragile.
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCCube), left.GetType ().BaseType, right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, left, right);
      //The result should almost always be a cube.
      //But there might be a couple exceptions.
      RCValue result = (RCValue) overload.Invoke (left, right);
      return result;
    }

    public static RCValue Invoke (RCClosure closure, string name, RCCube left, RCVectorBase right)
    {
      //RCVectorBase lvector = left.GetVector (0);
      //Here BaseType is always RCVector<T>.
      //This is a bit fragile.
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCCube), left.GetType (0), right.GetType ().BaseType);
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, left, right);
      //The result should almost always be a cube.
      //But there might be a couple exceptions.
      RCValue result = (RCValue) overload.Invoke (left, right);
      return result;
    }

    public static RCValue InvokeMonadic (RCClosure closure, string name, RCCube right)
    {
      // So we need a version that does this, and a version that looks for all
      // columns which can have the specified operator applied to them
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCCube), null, right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
      {
        throw RCException.Overload (closure, name, right);
      }
      //The result should almost always be a cube.
      //But there might be a couple exceptions.
      RCValue result = (RCValue) overload.Invoke (right);
      return result;
    }

    public static RCValue InvokeSequential (RCClosure closure, string name, RCCube right)
    {
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCCube), null, right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
      {
        throw RCException.Overload (closure, name, right);
      }
      RCValue result = (RCValue) overload.Invoke (right);
      return result;
    }

    public static RCValue InvokeSequential (RCClosure closure, string name, RCVectorBase left, RCCube right)
    {
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCCube), left.GetType ().BaseType, right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
      {
        throw RCException.Overload (closure, name, right);
      }
      RCValue result = (RCValue) overload.Invoke (left, right);
      return result;
    }

    public static RCValue InvokeSequential (RCClosure closure, string name, RCBlock right)
    {
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, typeof (RCBlock), null, right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
      {
        throw RCException.Overload (closure, name, right);
      }
      RCValue result = (RCValue) overload.Invoke (right);
      return result;
    }

    [RCVerb ("+")] [RCVerb ("-")] [RCVerb ("*")] [RCVerb ("/")] [RCVerb ("%")]
    [RCVerb ("and")] [RCVerb ("or")]
    [RCVerb ("==")] [RCVerb ("!=")] [RCVerb ("<")] [RCVerb (">")] [RCVerb ("<=")] [RCVerb (">=")]
    [RCVerb ("min")] [RCVerb ("max")]
    public void EvalDyadic (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (left.Count == 0 && right.Count == 0)
      {
        runner.Yield (closure, new RCCube ());
      }
      else if (left.Count == 0 && right.Count > 0 && right.Axis.Symbol != null && right.Axis.ColCount == 1)
      {
        runner.Yield (closure, right);
      }
      else if (left.Count == 0)
      {
        runner.Yield (closure, new RCCube ());
      }
      else if (right.Count == 0 && left.Count > 0 && left.Axis.Symbol != null && left.Axis.ColCount == 1)
      {
        runner.Yield (closure, left);
      }
      else if (right.Count == 0)
      {
        runner.Yield (closure, new RCCube ());
      }
      else
      {
        runner.Yield (closure, Invoke (closure, op.Name, left, right));
      }
    }

    [RCVerb ("+")] [RCVerb ("-")] [RCVerb ("*")] [RCVerb ("/")]
    [RCVerb ("and")] [RCVerb ("or")]
    [RCVerb ("==")] [RCVerb ("!=")] [RCVerb ("<")] [RCVerb (">")] [RCVerb ("<=")] [RCVerb (">=")]
    [RCVerb ("min")] [RCVerb ("max")]
    public void EvalDyadic (RCRunner runner, RCClosure closure, object left, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, Invoke (closure, op.Name, (RCVectorBase) left, right));
    }

    [RCVerb ("+")] [RCVerb ("-")] [RCVerb ("*")] [RCVerb ("/")]
    [RCVerb ("and")] [RCVerb ("or")]
    [RCVerb ("==")] [RCVerb ("!=")] [RCVerb ("<")] [RCVerb (">")] [RCVerb ("<=")] [RCVerb (">=")]
    [RCVerb ("min")] [RCVerb ("max")]
    public void EvalDyadic (RCRunner runner, RCClosure closure, RCCube left, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (left.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, Invoke (closure, op.Name, left, (RCVectorBase) right));
    }

    [RCVerb ("sqrt")] [RCVerb ("abs")] [RCVerb ("sums")]
    [RCVerb ("long")] [RCVerb ("double")] [RCVerb ("decimal")]
    [RCVerb ("byte")] [RCVerb ("string")] [RCVerb ("symbol")]
    [RCVerb ("boolean")] [RCVerb ("not")] [RCVerb ("upper")]
    [RCVerb ("lower")] [RCVerb ("length")] [RCVerb ("day")]
    [RCVerb ("hour")] [RCVerb ("minute")] [RCVerb ("second")]
    [RCVerb ("nano")] [RCVerb ("date")] [RCVerb ("daytime")]
    [RCVerb ("datetime")] [RCVerb ("timestamp")] [RCVerb ("timespan")]
    //[RCVerb ("high")] [RCVerb ("low")] [RCVerb ("sum")] [RCVerb ("avg")]
    //[RCVerb ("any")] [RCVerb ("all")] [RCVerb ("none")]
    public void EvalMonadic (RCRunner runner, RCClosure closure, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, InvokeMonadic (closure, op.Name, right));
    }

    [RCVerb ("map")] [RCVerb ("replace")] [RCVerb ("part")]
    public void EvalContextual (RCRunner runner, RCClosure closure, object left, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)  
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, Invoke (closure, op.Name, (RCVectorBase) left, right));
    }

    [RCVerb ("sum")] [RCVerb ("avg")] [RCVerb ("high")] [RCVerb ("low")]
    //[RCVerb ("any")] [RCVerb ("all")] [RCVerb ("none")]
    public void EvalAggregate (RCRunner runner, RCClosure closure, RCLong left, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, InvokeSequential (closure, op.Name, (RCVectorBase) left, right));
    }

    [RCVerb ("sum")] [RCVerb ("avg")] [RCVerb ("high")] [RCVerb ("low")]
    [RCVerb ("any")] [RCVerb ("all")] [RCVerb ("none")]
    public void EvalAggregate (RCRunner runner, RCClosure closure, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, InvokeSequential (closure, op.Name, right));
    }

    [RCVerb ("sum")] [RCVerb ("avg")] [RCVerb ("high")] [RCVerb ("low")]
    [RCVerb ("any")] [RCVerb ("all")] [RCVerb ("none")]
    public void EvalAggregate (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Count == 0)
        runner.Yield (closure, new RCDouble (0.0));
      else runner.Yield (
        closure, InvokeSequential (closure, op.Name, right));
    }

    [RCVerb ("in")] [RCVerb ("like")]
    public void EvalRightContextual (RCRunner runner, RCClosure closure, RCCube left, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (left.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, Invoke (closure, op.Name, (RCVectorBase) right, left));
    }

    //Use a dedicated function for this because it has non-standard handling for the empty cube.
    //The default behavior is to yield a [] if there is an [] as an argument.
    [RCVerb ("switch")]
    public void SwitchOp (RCRunner runner, RCClosure closure, RCCube left, RCBlock right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (left.Cols == 0)
        throw new Exception ("The left argument was an empty cube.");
      RCSystem.Activator.Invoke (runner, closure, op.Name, left.GetSimpleVector (0), right);
    }

    [RCVerb ("read")] [RCVerb ("write")]
    [RCVerb ("dispatch")] [RCVerb ("gawk")] [RCVerb ("throttle")] [RCVerb ("peek")] [RCVerb ("poll")]
    [RCVerb ("reply")]
    public void ConcurrencyOp (RCRunner runner, RCClosure closure, RCCube left, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (left.Cols == 0)
        throw new Exception ("The left argument was an empty cube.");
      RCSystem.Activator.Invoke (runner, closure, op.Name, left.GetSimpleVector (0), right);
    }

    [RCVerb ("read")] [RCVerb ("write")]
    [RCVerb ("dispatch")] [RCVerb ("gawk")] [RCVerb ("throttle")] [RCVerb ("peek")] [RCVerb ("poll")]
    [RCVerb ("reply")]
    public void ConcurrencyOp (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (left.Cols == 0)
        throw new Exception ("The left argument was an empty cube.");
      if (right.Cols == 0)
        throw new Exception ("The right argument was an empty cube.");
      RCSystem.Activator.Invoke (
        runner, closure, op.Name, left.GetSimpleVector (0), right.GetSimpleVector (0));
    }

    [RCVerb ("object")]
    public void EvalObject (RCRunner runner, RCClosure closure, RCCube cube)
    {
      RCCube result = Bang (cube, cube, null, false, true, true, false);
      runner.Yield (closure, result);
    }

    [RCVerb ("object")]
    public void EvalObject (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      RCCube result = Bang (right, right, null, false, true, true, false);
      string name = (string) left[0].Part (0);
      int col = result.FindColumn (name);
      RCArray<RCSymbolScalar> keycol = result.GetData<RCSymbolScalar> (col);
      result = Key (keycol, result);
      runner.Yield (closure, result);
    }

    [RCVerb ("fill")]
    public void EvalFill (RCRunner runner, RCClosure closure, RCCube cube)
    {
      RCCube result = new RCCube (cube.Axis);
      Filler filler = new Filler (result);
      filler.Fill (cube);
      runner.Yield (closure, result);
    }

    [RCVerb ("plug")]
    public void EvalPlug (RCRunner runner, RCClosure closure, RCLong left, RCCube cube)
    {
      if (left.Count != 1)
      {
        throw new Exception ("plug takes only one default value on the left");
      }
      RCCube result = new RCCube (cube.Axis);
      Plugger plugger = new Plugger (result, left[0]);
      plugger.Plug (cube);
      runner.Yield (closure, result);
    }

    [RCVerb ("plug")]
    public void EvalPlug (RCRunner runner, RCClosure closure, RCDouble left, RCCube cube)
    {
      if (left.Count != 1)
      {
        throw new Exception ("plug takes only one default value on the left");
      }
      RCCube result = new RCCube (cube.Axis);
      Plugger plugger = new Plugger (result, left[0]);
      plugger.Plug (cube);
      runner.Yield (closure, result);
    }

    /// <summary>
    /// Given a long in the left argument, key will rearrange parts of the S column using the same behavior as the part operator.
    /// </summary>
    [RCVerb ("key")]
    public void EvalKey (RCRunner runner, RCClosure closure, RCLong parts, RCCube cube)
    {
      RCArray<RCSymbolScalar> newKey = new RCArray<RCSymbolScalar> ();
      for (int i = 0; i < cube.Axis.Count; ++i)
      {
        newKey.Write (cube.Axis.Symbol[i].Part (parts.Data.ToArray ()));
      }
      runner.Yield (closure, Key (newKey, cube));
    }

    [RCVerb ("key")]
    public void EvalKey (RCRunner runner, RCClosure closure, RCSymbol key, RCCube cube)
    {
      runner.Yield (closure, Key (key.Data, cube));
    }

    [RCVerb ("key")]
    public void EvalKey (RCRunner runner, RCClosure closure, RCCube key, RCCube cube)
    {
      if (cube.Count == 0)
      {
        runner.Yield (closure, RCCube.Empty);
        return;
      }
      RCVectorBase vector = key.GetSimpleVector (0);
      RCArray<RCSymbolScalar> symbol = vector.Array as RCArray<RCSymbolScalar>;
      if (symbol == null)
      {
        symbol = new RCArray<RCSymbolScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
        {
          symbol.Write (new RCSymbolScalar (null, vector.Child (i)));
        }
      }
      RCCube result = Key (symbol, cube);
      runner.Yield (closure, result);
    }

    protected RCCube Key (RCArray<RCSymbolScalar> key, RCCube cube)
    {
      if (key.Count != cube.Axis.Count)
      {
        throw new Exception (string.Format ("New symbol column must have the same count as the old one. New count: {0}, old count: {1}",
                                            key.Count, cube.Count));
      }
      Timeline axis = new Timeline (cube.Axis.Global, cube.Axis.Event, cube.Axis.Time, key);
      axis.Count = cube.Axis.Count;
      RCArray<ColumnBase> columns = new RCArray<ColumnBase> ();
      RCArray<string> names = new RCArray<string> ();
      for (int i = 0; i < cube.Cols; ++i)
      {
        ColumnBase oldcol = cube.GetColumn (i);
        ColumnBase newcol = ColumnBase.FromArray (axis, oldcol.Index, oldcol.Array);
        columns.Write (newcol);
        names.Write (cube.NameAt (i));
      }
      RCCube result = new RCCube (axis, names, columns);
      return result;
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, Bang (right));
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCCube right)
    {
      // This overload is here specifically to accomodate the case where
      // you want to use cube & $my_op_yields_a_cube each {}
      // when the block is not empty & will yield a cube, otherwise it will yield a block
      // Another way would be to consider this as a bug in &
      runner.Yield (closure, right);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCCube left, RCBlock right)
    {
      RCCube result = Bang (left, right, false, false, true, true);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      RCCube result = Bang (left, right, null, false, false, true, true);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCString right)
    {
      RCCube result = VectorToCube<string> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCCube result = VectorToCube<RCSymbolScalar> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCDouble right)
    {
      RCCube result = VectorToCube<double> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      RCCube result = VectorToCube<decimal> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCLong right)
    {
      RCCube result = VectorToCube<long> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      RCCube result = VectorToCube<bool> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCTime right)
    {
      RCCube result = VectorToCube<RCTimeScalar> (right.Data);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCIncr right)
    {
      RCCube result = VectorToCube<RCIncrScalar> (right.Data);
      runner.Yield (closure, result);
    }

    protected RCCube VectorToCube<T> (RCArray<T> data)
    {
      RCCube result = new RCCube (new RCArray<string> ());
      for (int i = 0; i < data.Count; ++i)
      {
        result.WriteCell ("x", null, data[i], result.Axis.Count, parsing:true, force:false);
        result.Axis.Write ();
      }
      return result;
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      for (int i = 0; i < left.Count; ++i)
      {
        result.Write (left[i]);
      }
      result = Bang (result, right, false, false, true, true);
      runner.Yield (closure, result);
    }

    [RCVerb ("cube")]
    public void EvalCube (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      for (int i = 0; i < left.Count; ++i)
      {
        result.Write (left[i]);
      }
      result = Bang (result, right, null, false, false, true, true);
      runner.Yield (closure, result);
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCCube right)
    {
      RCCube result = new RCCube (right.Axis.Match ());
      Column<bool> indicator = (Column<bool>) right.GetColumn (0);
      if (indicator.Count == right.Axis.Count)
      {
        for (int i = 0; i < right.Axis.Count; ++i)
        {
          if (indicator.Data[i])
          {
            result.WriteCell ("x", right.Axis.SymbolAt (i), (long) i);
            result.Axis.Write (right.Axis, i);
          }
        }
      }
      else
      {
        for (int i = 0; i < right.Axis.Count; ++i)
        {
          bool found;
          int index = indicator.Index.BinarySearch (i, out found);
          if (found && indicator.Data[index])
          {
            result.WriteCell ("x", right.Axis.SymbolAt (i), (long) i);
            result.Axis.Write (right.Axis, i);
          }
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      if (right.Columns.Count == 0)
      {
        runner.Yield (closure, RCCube.Empty);
        return;
      }
      if (left.Axis.ColCount == 0)
      {
        RCArray<bool> whereCol = right.DoColof<bool> (col:0, def:false, allowSparse:true);
        WhereIndicator wherer = new WhereIndicator (left, whereCol);
        runner.Yield (closure, wherer.Where ());
      }
      else if (left.Axis.ColCount == 1 && left.Axis.Symbol != null)
      {
        RCArray<bool> whereCol = right.DoColof<bool> (col:0, def:false, allowSparse:true);
        RCCube result = new RCCube (new RCArray<string> ("S"));
        for (int i = 0; i < whereCol.Count; ++i)
        {
          if (whereCol[i])
          {
            result.Write (right.Axis.SymbolAt (i));
          }
        }
        result = Bang (result, left, null, false, false, true, true);
        runner.Yield (closure, result);
      }
      else
      {
        Column<bool> whereCol = (Column<bool>) right.GetColumn (0);
        WhereLocator wherer = new WhereLocator (left, whereCol);
        runner.Yield (closure, wherer.Where ());
      }
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCCube left, RCBoolean right)
    {
      if (left.Axis.ColCount == 0)
      {
        WhereIndicator wherer = new WhereIndicator (left, right.Data);
        runner.Yield (closure, wherer.Where ());
      }
      else if (left.Axis.ColCount == 1 && left.Axis.Symbol != null)
      {
        RCCube result = new RCCube (new RCArray<string> ("S"));
        for (int i = 0; i < right.Data.Count; ++i)
        {
          if (right[i])
          {
            result.Write (left.Axis.SymbolAt (i));
          }
        }
        result = Bang (result, left, null, false, false, true, true);
        runner.Yield (closure, result);
      }
      else
      {
        WhereIndicator wherer = new WhereIndicator (left, right.Data);
        runner.Yield (closure, wherer.Where ());
      }
    }

    [RCVerb ("join")]
    public void EvalJoin (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      HashSet<string> leftCols = new HashSet<string> (left.Names);
      HashSet<string> rightCols = new HashSet<string> (right.Names);
      HashSet<string> joinCols = new HashSet<string> (leftCols);
      joinCols.IntersectWith (rightCols);
      string[] joinColsArray = new string[joinCols.Count];
      joinCols.CopyTo (joinColsArray);
      // Do not join on any column containing nulls in either cube
      for (int i = 0; i < joinColsArray.Length; ++i)
      {
        string joinCol = joinColsArray[i];
        ColumnBase column = right.GetColumn (joinCol);
        if (column.Count != right.Count)
        {
          joinCols.Remove (joinCol);
        }
        column = left.GetColumn (joinCol);
        if (column.Count != left.Count)
        {
          joinCols.Remove (joinCol);
        }
      }
      leftCols.ExceptWith (rightCols);
      rightCols.ExceptWith (leftCols);
      // I just can't do it without constructing the symbols. I'm too stupid.
      // Symbols are a convenient data structure here for extracting the shared values.
      RCSymbolScalar[] rightSyms = new RCSymbolScalar[right.Count];
      foreach (string joinCol in joinCols)
      {
        ColumnBase column = right.GetColumn (joinCol);
        if (column.Count != right.Count)
        {
          throw new Exception (string.Format ("join column '{0}' may not have nulls", joinCol));
        }
        //There is no need to do the irow schtick here because there are no nulls.
        for (int j = 0; j < column.Count; ++j)
        {
          object val = column.BoxCell (j);
          if (val.GetType () != typeof (RCSymbolScalar))
          {
            rightSyms[j] = new RCSymbolScalar (null, val);
          }
          else
          {
            rightSyms[j] = (RCSymbolScalar) val;
          }
        }
      }
      RCSymbolScalar[] leftSyms = new RCSymbolScalar[left.Count];
      int[] leftRow = new int[left.Count];
      foreach (string joinCol in joinCols)
      {
        ColumnBase column = left.GetColumn (joinCol);
        if (column.Count != left.Count)
        {
          throw new Exception (string.Format ("join column '{0}' may not have nulls", joinCol));
        }
        for (int j = 0; j < leftSyms.Length; ++j)
        {
          bool found;
          int irow = column.Index.BinarySearch (j, out found);
          if (found && irow < column.Count)
          {
            object val = column.BoxCell (irow);
            if (val.GetType () != typeof (RCSymbolScalar))
            {
              leftSyms[j] = new RCSymbolScalar (null, val);
            }
            else
            {
              leftSyms[j] = (RCSymbolScalar) val;
            }
          }
          else
          {
            leftSyms[j] = null;
          }
        }
      }
      for (int i = 0; i < leftSyms.Length; ++i)
      {
        //Indicate a missing symbol from the right-hand column.
        leftRow[i] = -1;
        for (int j = 0; j < rightSyms.Length; ++j)
        {
          if (leftSyms[i] != null && leftSyms[i].Equals (rightSyms[j]))
          {
            leftRow[i] = j;
            break;
          }
        }
      }
      int col = 0;
      RCCube result = new RCCube (left.Axis.Match ());
      if (result.Axis.Global != null)
      {
        result.Axis.Global.Write (left.Axis.Global);
      }
      if (result.Axis.Event != null)
      {
        result.Axis.Event.Write (left.Axis.Event);
      }
      if (result.Axis.Time != null)
      {
        result.Axis.Time.Write (left.Axis.Time);
      }
      if (result.Axis.Symbol != null)
      {
        result.Axis.Symbol.Write (left.Axis.Symbol);
      }
      result.Axis.Count = left.Axis.Count;
      foreach (string leftCol in left.Names)
      {
        col = left.FindColumn (leftCol);
        //Need a test that fails with this line commented in:
        //if (col > -1 && !joinCols.Contains (leftCol))
        if (col > -1 && rightCols.Contains (leftCol) && !joinCols.Contains (leftCol))
        {
          ColumnBase rightColumn = right.GetColumn (leftCol);
          ColumnBase leftColumn = left.GetColumn (col);
          for (int i = 0; i < leftSyms.Length; ++i)
          {
            if (leftRow[i] < 0)
            {
              // The symbol is not represented in the right-hand column
              // Use the value in the left-hand column instead
              // Don't forget that the the left-hand column too can be empty
              bool found;
              int irow = leftColumn.Index.BinarySearch (i, out found);
              if (found && irow < leftColumn.Count)
              {
                object box = leftColumn.BoxCell (irow);
                RCSymbolScalar symbol = result.Axis.SymbolAt (i);
                result.WriteCell (leftCol, symbol, box, i, false, true);
              }
            }
            else
            {
              // The symbol is in the right-hand column, override the value on the left
              // However this particular column may not have a value for this row
              bool found;
              int irow = rightColumn.Index.BinarySearch (leftRow[i], out found);
              if (found && irow < rightColumn.Count)
              {
                object box = rightColumn.BoxCell (irow);
                RCSymbolScalar symbol = result.Axis.SymbolAt (i);
                result.WriteCell (leftCol, symbol, box, i, false, true);
              }
            }
          }
        }
        else
        {
          ColumnBase leftColumn = left.GetColumn (leftCol);
          for (int i = 0; i < leftSyms.Length; ++i)
          {
            bool found;
            int irow = leftColumn.Index.BinarySearch (i, out found);
            if (found && irow < leftColumn.Count)
            {
              object box = leftColumn.BoxCell (irow);
              RCSymbolScalar symbol = result.Axis.SymbolAt (i);
              result.WriteCell (leftCol, symbol, box, i, false, true);
            }
          }
        }
      }
      foreach (string rightCol in rightCols)
      {
        ColumnBase column = right.GetColumn (rightCol);
        for (int i = 0; i < leftSyms.Length; ++i)
        {
          if (leftRow[i] >= 0)
          {
            bool found;
            int vrow = column.Index.BinarySearch (leftRow[i], out found);
            if (found && vrow < column.Count)
            {
              object box = column.BoxCell (vrow);
              RCSymbolScalar symbol = result.Axis.SymbolAt (i);
              result.WriteCell (rightCol, symbol, box, i, false, true);
            }
          }
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("flatPack")]
    public void EvalFlatPack (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, right.FlatPack ());
    }

    [RCVerb ("delta")]
    public void EvalDelta (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      Deltafier deltafier = new Deltafier (left, right);
      RCCube result = deltafier.Delta ();
      runner.Yield (closure, result);
    }

    [RCVerb ("!")]
    public void Bang (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      runner.Yield (closure, Bang (left, right, null, true, true, false, false));
    }

    [RCVerb ("!")]
    public void Bang (RCRunner runner, RCClosure closure, RCCube left, RCBlock right)
    {
      runner.Yield (closure, Bang (left, right, true, true, false, false));
    }

    [RCVerb ("!")]
    public void Bang (RCRunner runner, RCClosure closure, RCBlock right)
    {
      if (right.Count == 0)
      {
        runner.Yield (closure, RCCube.Empty);
        return;
      }
      RCCube result = new RCCube (((RCCube) right.Get (0)).Axis.Match ());
      Bang (runner, closure, result, right);
    }

    [RCVerb ("inter")]
    public void Inter (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      if (left.Axis.Symbol == null || right.Axis.Symbol == null)
        throw new Exception ("inter requires both cubes to have S");

      HashSet<RCSymbolScalar> rightSyms = new HashSet<RCSymbolScalar> (right.Axis.Symbol);
      RCArray<RCSymbolScalar> s = new RCArray<RCSymbolScalar> (8);
      for (int i = 0; i < left.Count; ++i)
      {
        if (rightSyms.Contains (left.Axis.Symbol[i]))
        {
          s.Write (left.Axis.Symbol[i]);
        }
      }
      Timeline axis = new Timeline (s);
      RCCube result = new RCCube (axis);
      result = Bang (result, left, null, false, false, false, true);
      result = Bang (result, right, null, false, false, false, true);
      runner.Yield (closure, result);
    }

    [RCVerb ("except")]
    public void Except (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      if (left.Axis.Symbol == null || right.Axis.Symbol == null)
      {
        throw new Exception ("except requires both cubes to have S");
      }
      HashSet<RCSymbolScalar> rightSyms = new HashSet<RCSymbolScalar> (right.Axis.Symbol);
      RCArray<RCSymbolScalar> s = new RCArray<RCSymbolScalar> (8);
      for (int i = 0; i < left.Count; ++i)
      {
        if (!rightSyms.Contains (left.Axis.Symbol[i]))
        {
          s.Write (left.Axis.Symbol[i]);
        }
      }
      Timeline axis = new Timeline (s);
      RCCube result = new RCCube (axis);
      result = Bang (result, left, null, false, false, false, true);
      result = Bang (result, right, null, false, false, false, true);
      runner.Yield (closure, result);
    }

    [RCVerb ("except")]
    public void Except (RCRunner runner, RCClosure closure, RCCube left, RCSymbol right)
    {
      RCCube result = new RCCube (left.Axis.Match ());
      for (int i = 0; i < left.Axis.Count; ++i)
      {
        bool include = true;
        for (int j = 0; j < right.Count; ++j)
        {
          RCSymbolScalar symbol = left.Axis.Symbol[i];
          if ((symbol.Equals (right[j])) || (symbol.IsConcreteOf (right[j])))
          {
            include = false;
            break;
          }
        }
        if (include)
        {
          for (int k = 0; k < left.Cols; ++k)
          {
            RCSymbolScalar sym = left.Axis.SymbolAt (i);
            ColumnBase column = left.GetColumn (k);
            if (column == null)
            {
              continue;
            }
            bool found;
            int index = column.Index.BinarySearch (i, out found);
            if (found)
            {
              string name = left.NameAt (k);
              object box = column.BoxCell (index);
              result.WriteCell (name, sym, box);
            }
          }
          result.Axis.Write (left.Axis, i);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("except")]
    public void Except (RCRunner runner, RCClosure closure, RCCube left, RCLong right)
    {
      RCCube result = new RCCube (left.Axis.Match ());
      HashSet<long> exceptRows = new HashSet<long> (right);
      for (int i = 0; i < left.Axis.Count; ++i)
      {
        if (!exceptRows.Contains (i))
        {
          for (int j = 0; j < left.Cols; ++j)
          {
            RCSymbolScalar sym = left.Axis.SymbolAt (i);
            ColumnBase column = left.GetColumn (j);
            if (column == null)
            {
              continue;
            }
            bool found;
            int index = column.Index.BinarySearch (i, out found);
            if (found)
            {
              string name = left.NameAt (j);
              object box = column.BoxCell (index);
              result.WriteCell (name, sym, box);
            }
          }
          result.Axis.Write (left.Axis, i);
        }
      }
      runner.Yield (closure, result);
    }

    ///insert    - result will contain symbols from right that were missing from left
    ///dedup     - only one row per symbol
    ///force     - create new symbol row
    ///keepIncrs - do not evaluate incrops
    protected RCCube Bang (RCCube left, RCCube right, 
                           string colname, bool insert, bool dedup, bool force, bool keepIncrs)
    {
      if (left == null)
      {
        if (colname != null && right.Cols > 0)
        {
          right = new RCCube (right);
          right.Names.Write (0, colname);
        }
        return right;
      }
      if (left.Axis.Symbol != null && right.Axis.Symbol != null)
      {
        if (left.Axis.Time != null)
        {
          return MergeMultipleCubes (new string[]{"", ""}, new RCCube[] {left, right});
        }
        else
        {
          return BangSymCubes (left, right, colname, insert, dedup, force, keepIncrs);
        }
      }
      else
      {
        return BangRectCubes (left, right, colname);
      }
    }

    public RCCube Bang (RCCube left, RCBlock right,
                        bool insert, bool dedup, bool force, bool keepIncrs)
    {
      // For operators having no left cube parameter, the left axis will be built to match the first cube in the block.
      RCCube result = left;
      try
      {
        if (result.IsLocked)
        {
          result = new RCCube (left);
        }
        if (result.Axis.Time != null)
        {
          string[] names = new string[right.Count];
          RCCube[] cubes = new RCCube[right.Count];
          for (int i = 0; i < right.Count; ++i)
          {
            names[i] = right.GetName (i).Name;
            cubes[i] = (RCCube) right.Get (i);
          }
          result = MergeMultipleCubes (names, cubes);
        }
        else
        {
          for (int i = 0; i < right.Count; ++i)
          {
            //Here we need to check to see if the cubes are time cubes and use MergeMultipleCubes
            RCBlock name = right.GetName (i);
            RCCube cube = name.Value as RCCube;
            if (cube != null)
            {
              string colname = null;
              if (cube.Cols == 1 && name.Name != "")
              {
                colname = name.Name;
              }
              result = Bang (result, cube, colname, insert, dedup, force, keepIncrs);
              continue;
            }
            RCVectorBase vector = name.Value as RCVectorBase;
            if (vector != null)
            {
              if (name.Name == "")
              {
                throw new Exception ("vector values must have names assigned");
              }
              result = Bang (result, vector, name.Name, force, keepIncrs);
              continue;
            }
          }
        }
      }
      catch (Exception)
      {
        throw;
      }
      return result;
    }

    protected RCCube BangSymCubes (RCCube left, RCCube right, 
                                   string colname, bool insert, bool dedup, 
                                   bool force, bool keepIncrs)
    {
      HashSet<string> leftCols, rightCols;
      leftCols = new HashSet<string> ();
      rightCols = new HashSet<string> ();
      for (int i = 0; i < left.Cols; ++i)
      {
        leftCols.Add (left.NameAt (i));
      }
      for (int i = 0; i < right.Cols; ++i)
      {
        if (colname != null && i == 0)
        {
          rightCols.Add (colname);
        }
        else
        {
          rightCols.Add (right.NameAt (i));
        }
      }
      HashSet<string> leftOnly = new HashSet<string> (leftCols);
      HashSet<string> rightOnly = new HashSet<string> (rightCols);
      HashSet<string> both = new HashSet<string> (leftCols);
      leftOnly.ExceptWith (rightCols);
      rightOnly.ExceptWith (leftCols);
      both.IntersectWith (rightCols);
      RCCube result = new RCCube (new RCArray<string> ("S"));
      HashSet<RCSymbolScalar> existing = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < left.Axis.Count; ++i)
      {
        RCSymbolScalar symbol = left.Axis.SymbolAt (i);
        if (!dedup)
        {
          result.Write (symbol);
        }
        else if (!existing.Contains (symbol))
        {
          result.Write (symbol);
          existing.Add (symbol);
        }
      }
      if (insert)
      {
        for (int i = 0; i < right.Axis.Count; ++i)
        {
          RCSymbolScalar symbol = right.Axis.SymbolAt (i);
          if (!existing.Contains (symbol))
          {
            result.Write (symbol);
            //Commenting this check out assumes there would not be any dups in the right cube.
            //Is that a safe assumption?
            existing.Add (symbol);
          }
        }
      }
      for (int col = 0; col < left.Cols; ++col)
      {
        string name = left.NameAt (col);
        if (leftOnly.Contains (name))
        {
          this.CopyUnsharedColumn (left, result, col, name, force:force, keepIncrs:keepIncrs);
        }
        else if (both.Contains (name))
        {
          this.MergeSharedColumn (left, right, result, col, name:name, colname:colname, force:force, keepIncrs:keepIncrs);
        }
      }
      for (int col = 0; col < right.Cols; ++col)
      {
        string name = colname;
        if (colname == null || col > 0)
        {
          name = right.NameAt (col);
        }
        if (rightOnly.Contains (name))
        {
          this.CopyUnsharedColumnSmart (right, result, col, name, keepIncrs:keepIncrs, force:force);
        }
      }
      return result;
    }

    protected void CopyUnsharedColumn (RCCube source, RCCube result,
                                       int col, string name, bool force, bool keepIncrs)
    {
      //Possible optimization here is to steal the underlying column object
      //because it will not change and we will not be changing it.
      ColumnBase column = source.GetColumn (col);
      if (column == null)
      {
        throw new Exception ("No column " + col + " in source");
      }
      for (int vrow = 0; vrow < column.Count; ++vrow)
      {
        int row = column.Index[vrow];
        RCSymbolScalar symbol = result.Axis.SymbolAt (row);
        object box = column.BoxCell (vrow);
        result.WriteCell (name, symbol, box, row, keepIncrs, force);
      }
    }

    protected void CopyUnsharedColumnSmart (RCCube source, RCCube result,
                                            int col, string name, bool keepIncrs, bool force)
    {
      if (source.Axis.Symbol != null)
      {
        ColumnBase column = source.GetColumn (col);
        for (int row = 0; row < result.Axis.Count; ++row)
        {
          RCSymbolScalar symbol = result.Axis.SymbolAt (row);
          object box;
          if (column.BoxLast (symbol, out box))
          {
            result.WriteCell (name, symbol, box, row, false, force);
          }
        }
      }
      else
      {
        ColumnBase column = source.GetColumn (col);
        for (int row = 0; row < result.Axis.Count; ++row)
        {
          RCSymbolScalar symbol = result.Axis.SymbolAt (row);
          object box = column.BoxCell (row);
          result.WriteCell (name, symbol, box, row, false, force);
        }
      }
    }

    protected void MergeSharedColumn (RCCube left, RCCube right, RCCube result,
                                      int col, string name, string colname, bool keepIncrs, bool force)
    {
      //We have to do a careful merge for these columns.
      ColumnBase lcol = left.GetColumn (col);
      if (lcol == null)
      {
        throw new Exception (string.Format ("Invalid column index col:{0}", col));
      }
      ColumnBase rcol;
      if (colname != null)
      {
        rcol = right.GetColumn (0);
      }
      else
      {
        rcol = right.GetColumn (name);
      }
      for (int row = 0; row < result.Axis.Count; ++row)
      {
        RCSymbolScalar symbol = result.Axis.SymbolAt (row);
        object box;
        if (rcol.BoxLast (symbol, out box))
        {
          if (box is RCIncrScalar)
          {
            object lbox;
            if (lcol.BoxLast (symbol, out lbox))
            {
              //You could try to do this with a double or decimal.
              //In the end I think incrops should be restricted to integer types.
              //And decimals should be taken out of the language.
              //Aug 22, 2014. No longer so sure about taking decimals out.
              long lint = (long) lbox;
              ++lint;
              result.WriteCell (name, symbol, lint, row, false, force);
            }
            else
            {
              result.WriteCell (name, symbol, 0L, row, false, force);
            }
          }
          else
          {
            result.WriteCell (name, symbol, box, row, false, force);
          }
        }
        else if (lcol.BoxLast (symbol, out box))
        {
          result.WriteCell (name, symbol, box, row, false, force);
        }
      }
    }

    protected RCCube BangRectCubes (RCCube left, RCCube right, string colname)
    {
      if (left.Axis.Count != right.Count)
      {
        throw new Exception ("Both cubes must have the same count");
      }
      if (colname != null && right.Cols > 1)
      {
        throw new Exception ("Only one column allowed with colname override.");
      }
      RCCube result;
      if (left.Axis.Symbol != null)
      {
        result = new RCCube (left.Axis.Match ());
        for (int i = 0; i < left.Axis.Count; ++i)
        {
          result.Axis.Write (left.Axis, i);
        }
      }
      else if (right.Axis.Symbol != null)
      {
        result = new RCCube (right.Axis.Match ());
        for (int i = 0; i < right.Axis.Count; ++i)
        {
          result.Axis.Write (right.Axis, i);
        }
      }
      else
      {
        result = new RCCube (new RCArray<string> ());
        result.Axis.Count = left.Count;
      }
      HashSet<string> rightDone = new HashSet<string> ();
      for (int i = 0; i < left.Cols; ++i)
      {
        ColumnBase column = left.GetColumn (i);
        string name = left.NameAt (i);
        int righti = -1;
        if (colname != null && colname == name)
        {
          righti = 0;
        }
        else if (colname == null)
        {
          righti = right.FindColumn (name);
        }
        if (righti > -1)
        {
          column = right.GetColumn (righti);
          rightDone.Add (name);
        }
        for (int row = 0; row < column.Count; ++row)
        {
          object box = column.BoxCell (row);
          result.WriteCell (name, null, box, row, false, false);
        }
      }
      for (int i = 0; i < right.Cols; ++i)
      {
        string name = colname != null ? colname : right.NameAt (i);
        if (rightDone.Contains (name))
        {
          continue;
        }
        ColumnBase column = right.GetColumn (i);
        for (int row = 0; row < column.Count; ++row)
        {
          object box = column.BoxCell (row);
          result.WriteCell (name, null, box, row, false, false);
        }
      }
      return result;
    }

    public RCCube Bang (RCCube left, RCVectorBase right, string colname, bool force, bool keepIncrs)
    {
      if (colname == null)
      {
        throw new Exception ();
      }
      if (left == null)
      {
        left = new RCCube (new RCArray<string> ());
        for (int i = 0; i < right.Count; ++i)
        {
          object box = right.Child (i);
          left.WriteCell (colname, null, box, i, keepIncrs, force);
        }
        left.Axis.Count = right.Count;
        return left;
      }
      int col = left.FindColumn (colname);
      if (col < 0)
      {
        if (right.Count == 1)
        {
          object box = right.Child (0);
          for (int i = 0; i < left.Axis.Count; ++i)
          {
            left.WriteCell (colname, left.Axis.SymbolAt (i), box, i, keepIncrs, force);
          }
          return left;
        }
        else if (right.Count == left.Axis.Count)
        {
          for (int i = 0; i < left.Axis.Count; ++i)
          {
            object box = right.Child (i);
            left.WriteCell (colname, left.Axis.SymbolAt (i), box, i, keepIncrs, force);
          }
          return left;
        }
        else if (left.Axis.Count == 0)
        {
          left = new RCCube (left.Axis);
          for (int i = 0; i < right.Count; ++i)
          {
            object box = right.Child (i);
            left.WriteCell (colname, null, box, i, keepIncrs, force);
          }
          return left;
        }
        else
        {
          throw new Exception (string.Format ("Unable to align vector having count {0} with cube having count {1}", right.Count, left.Axis.Count));
        }
      }
      else
      {
        //I HATE creating a new timeline here. HATE it.
        //I think we should be able to get rid of this now that we have checks for locked cubes.
        RCCube result = new RCCube (new RCArray<string> ("S"));
        for (int i = 0; i < left.Axis.Count; ++i)
        {
          result.Write (left.Axis.SymbolAt (i));
        }
        for (int i = 0; i < left.Cols; ++i)
        {
          if (i == col)
          {
            if (right.Count == 1)
            {
              object box = right.Child (0);
              for (int j = 0; j < left.Axis.Count; ++j)
              {
                result.WriteCell (colname, left.Axis.SymbolAt (j), box, j, false, false);
              }
            }
            else if (right.Count == left.Count)
            {
              for (int j = 0; j < result.Axis.Count; ++j)
              {
                RCSymbolScalar symbol = result.Axis.SymbolAt (j);
                object box = right.Child (j);
                result.WriteCell (colname, symbol, box, j, false, false);
              }
            }
          }
          else
          {
            ColumnBase column = left.GetColumn (i);
            string name = left.NameAt (i);
            for (int vrow = 0; vrow < column.Count; ++vrow)
            {
              int row = column.Index[vrow];
              object box = column.BoxCell (vrow);
              RCSymbolScalar symbol = result.Axis.SymbolAt (row);
              result.WriteCell (name, symbol, box, row, false, false);
            }
          }
        }
        return result;
      }
    }

    protected RCCube Bang (RCBlock data)
    {
      if (data.Count == 0)
      {
        return new RCCube ();
      }
      RCCube result = null;
      //If the first row is a block, all rows must be blocks.
      //If the first has a name, all rows must have names.
      //If the block is created with "block" it will have S fields as well as names.
      //The S field is what we want to use when recreating the cube.
      //Otherwise we use the (shortened) name. Ex #a,b,c becomes #c if there is no "S."
      RCBlock first = data.GetName (0);
      bool hasS = first.Get ("S", null) != null;
      bool hasName = first.Name != "";
      bool isRow = first.Value is RCBlock;

      // It is a block of rows
      if (isRow)
      {
        if (hasS || hasName)
        {
          result = new RCCube (new RCArray<string> ("S"));
        }
        else
        {
          result = new RCCube (new RCArray<string> ());
        }
        for (int i = 0; i < data.Count; ++i)
        {
          RCBlock rowkv = data.GetName (i);
          RCBlock row = (RCBlock) rowkv.Value;
          RCSymbolScalar symbol = null;
          for (int j = 0; j < row.Count; ++j)
          {
            RCBlock colkv = row.GetName (j);
            if (hasS)
            {
              symbol = colkv.GetSymbol ("S");
            }
            else if (hasName)
            {
              symbol = new RCSymbolScalar (null, rowkv.Name);
            }
            if (colkv.Name != "S")
            {
              RCVectorBase col = (RCVectorBase) colkv.Value;
              object box = col.Child (0);
              result.WriteCell (colkv.Name, symbol, box, -1, false, true);
            }
          }
          if (hasS || hasName)
          {
            result.Axis.Write (symbol);
          }
          else
          {
            result.Axis.Count++;
          }
        }
        return result;
      }

      // It is a block of columns
      RCArray<long> G = null, E = null;
      RCArray<RCTimeScalar> T = null;
      RCArray<RCSymbolScalar> S = null;
      int count = -1;
      RCArray<string> names = new RCArray<string> (data.Count);
      RCArray<RCValue> vectors = new RCArray<RCValue> (data.Count);
      bool isAllCubes = true;
      for (int i = 0; i < data.Count; ++i)
      {
        RCBlock block = data.GetName (i);
        if (block.Name == "G")
        {
          if (block.Value is RCLong)
          {
            G = ((RCLong) block.Value).Data;
          }
          else if (block.Value is RCCube)
          {
            G = ((RCLong) ((RCCube) block.Value).GetSimpleVector (0)).Data;
          }
          isAllCubes = false;
          count = G.Count;
        }
        else if (block.Name == "E")
        {
          if (block.Value is RCLong)
          {
            E = ((RCLong) block.Value).Data;
          }
          else if (block.Value is RCCube)
          {
            E = ((RCLong) ((RCCube) block.Value).GetSimpleVector (0)).Data;
          }
          isAllCubes = false;
          count = E.Count;
        }
        else if (block.Name == "T")
        {
          if (block.Value is RCTime)
          {
            T = ((RCTime) block.Value).Data;
          }
          else if (block.Value is RCCube)
          {
            T = ((RCTime) ((RCCube) block.Value).GetSimpleVector (0)).Data;
          }
          isAllCubes = false;
          count = T.Count;
        }
        else if (block.Name == "S")
        {
          if (block.Value is RCSymbol)
          {
            S = ((RCSymbol) block.Value).Data;
          }
          else if (block.Value is RCCube)
          {
            S = ((RCSymbol) ((RCCube) block.Value).GetSimpleVector (0)).Data;
          }
          isAllCubes = false;
          count = S.Count;
        }
        else
        {
          if (!(block.Value is RCCube))
          {
            isAllCubes = false;
          }
          vectors.Write (block.Value);
          names.Write (block.Name);
        }
      }

      // Build the final result using the regular bang code path
      string name;
      RCCube cube;
      RCVectorBase vector;
      if (G != null || E != null || T != null || S != null)
      {
        Timeline axis = new Timeline (G, E, T, S);
        result = new RCCube (axis);
      }
      if (isAllCubes)
      {
        RCCube[] cubes = new RCCube[vectors.Count];
        for (int i = 0; i < vectors.Count; ++i)
        {
          cubes[i] = (RCCube) vectors[i];
        }
        result = MergeMultipleCubes (names.ToArray (), cubes);
      }
      else
      {
        for (int i = 0; i < vectors.Count; ++i)
        {
          name = names[i];
          cube = vectors[i] as RCCube;
          if (cube != null)
          {
            string colname = null;
            if (cube.Cols == 1 && name != "")
            {
              colname = name;
            }
            result = Bang (result, cube, colname, false, false, true, true);
            continue;
          }
          vector = vectors[i] as RCVectorBase;
          if (vector != null)
          {
            if (name == "")
            {
              throw new Exception ("vector values must have names assigned");
            }
            result = Bang (result, vector, name, true, true);
            continue;
          }
        }
      }
      return result;
    }

    [RCVerb ("count")]
    public void CountOp (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("rename")]
    public void RenameOp (RCRunner runner, RCClosure closure, RCString left, RCCube right)
    {
      RCCube result = new RCCube (right.Axis, left.Data, right.Columns);
      runner.Yield (closure, result);
    }

    [RCVerb ("has")]
    public void EvalHas (RCRunner runner, RCClosure closure, RCCube left, RCString right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (left.Has (right[i]));
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("has")]
    public void EvalHas (RCRunner runner, RCClosure closure, RCCube left, RCSymbol right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (left.Has (right[i].Part (0).ToString ()));
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      RCCube result = new RCCube (right.Axis);
      for (int i = 0; i < left.Count; ++i)
      {
        string name = (string) left[i].Part (0);
        if (right.Has (name))
        {
          ColumnBase column = right.GetColumn (name);
          result.Names.Write (name);
          result.Columns.Write (column);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCString left, RCCube right)
    {
      RCCube result = new RCCube (right.Axis);
      for (int i = 0; i < left.Count; ++i)
      {
        string name = left[i];
        if (right.Has (name))
        {
          ColumnBase column = right.GetColumn (name);
          result.Names.Write (name);
          result.Columns.Write (column);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCCube left, RCString right)
    {
      RCArray<long> g = null;
      RCArray<long> e = null;
      RCArray<RCTimeScalar> t = null;
      RCArray<RCSymbolScalar> s = null;
      if (!right.Data.Contains ("G"))
      {
        g = left.Axis.Global;
      }
      if (!right.Data.Contains ("E"))
      {
        e = left.Axis.Event;
      }
      if (!right.Data.Contains ("T"))
      {
        t = left.Axis.Time;
      }
      if (!right.Data.Contains ("S"))
      {
        s = left.Axis.Symbol;
      }
      Timeline axis = new Timeline (g, e, t, s);
      axis.Count = left.Count;
      RCCube result = new RCCube (axis);
      for (int i = 0; i < left.Names.Count; ++i)
      {
        string name = left.Names[i];
        if (!right.Data.Contains (name))
        {
          ColumnBase column = left.GetColumn (name);
          result.Names.Write (name);
          result.Columns.Write (column);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("at")]
    public void EvalAt (RCRunner runner, RCClosure closure, RCCube left, RCSymbol right)
    {
      EvalFrom (runner, closure, right, left);
    }

    [RCVerb ("at")]
    public void EvalAt (RCRunner runner, RCClosure closure, RCCube left, RCString right)
    {
      EvalFrom (runner, closure, right, left);
    }

    [RCVerb ("empty")]
    public void EvalEmpty (RCRunner runner, RCClosure closure, RCCube right)
    {
      if (right.Axis.Count == 0)
      {
        runner.Yield (closure, right);
        return;
      }

      RCCube result = new RCCube (right.Axis.Match ());
      ColumnBase column = right.GetColumn (0);
      RCArray<int> index = column.Index;
      int vrow = 0;
      for (int i = 0; i < right.Axis.Count; ++i)
      {
        if (vrow > index.Count)
        {
          result.WriteCell ("x", right.SymbolAt (i), true);
          result.Axis.Write (right.Axis, i);
        }
        else if (vrow == index.Count)
        {
          result.WriteCell ("x", right.SymbolAt (i), true);
          result.Axis.Write (right.Axis, i);
          ++vrow;
        }
        else if (i == index[vrow])
        {
          result.WriteCell ("x", right.SymbolAt (i), false);
          // Operators should not lose G and E columns like this, the bill will come.
          result.Axis.Write (right.Axis, i);
          ++vrow;
        }
        else if (i < index[vrow])
        {
          result.WriteCell ("x", right.SymbolAt (i), true);
          result.Axis.Write (right.Axis, i);
        }
        else
        {
          throw new Exception ();
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("names")]
    public void NamesOp (RCRunner runner, RCClosure closure, RCCube right)
    {
      string[] result = new string[right.Cols];
      for (int i = 0; i < result.Length; ++i)
        result[i] = right.NameAt (i);
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("print")]
    public void EvalPrint (RCRunner runner, RCClosure closure, RCCube right)
    {
      Console.Out.WriteLine (right.ToString ());
      runner.Yield (closure, new RCLong (0));
    }

    [RCVerb ("untimeline")]
    public void EvalUntimeline (RCRunner runner, RCClosure closure, RCCube cube)
    {
      runner.Yield (closure, cube.Untimeline ());
    }

    [RCVerb ("retimeline")]
    public void EvalRetimeline (RCRunner runner, RCClosure closure, RCString tlcols, RCCube cube)
    {
      runner.Yield (closure, cube.Retimeline (tlcols.Data));
    }

    [RCVerb ("rows")]
    public void EvalRows (RCRunner runner, RCClosure closure, RCLong left, RCCube right)
    {
      if (right.Axis.Global != null)
      {
        throw new Exception ("Cannot rows a cube with column G");
      }
      if (right.Axis.Event != null)
      {
        throw new Exception ("Cannot rows a cube with column E");
      }
      if (right.Axis.Time != null)
      {
        throw new Exception ("Cannot rows a cube with column T");
      }
      if (right.Axis.Symbol != null)
      {
        throw new Exception ("Cannot rows a cube with column S");
      }

      //ways to select things....
      //a range of rows 0 to 49
      //a range of cols 0 to 49
      //a list of rows by name #a #b #c
      //a list of cols by name #x #y #z
      //a list of cols by number 0 3 5l
      //a list of rows by number 2 4 6 8
      //by cells or cell ranges 0 0 49 49
      RCCube result = null;
      try
      {
        ReadCounter counter = new ReadCounter ();
        ReadSpec spec = new ReadSpec (counter, RCSymbol.Wild, (int) left[0], (int) left[1] + 1, false);
        result = right.Read (spec, counter, false, right.Count);
        for (int i = 0; i < result.Cols; ++i)
        {
          ColumnBase column = result.GetColumn (i);
          if (column == null)
          {
            result.Columns.Write (i, new RCCube.ColumnOfNothing (result.Axis));
          }
        }
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("meta")]
    public void EvalMeta (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCCube target = new RCCube (new RCArray<string> ("S"));
      Stack<object> names = new Stack<object> ();
      Meta (target, right, names);
      runner.Yield (closure, target);
    }

    protected void Meta (RCCube target, RCBlock source, Stack<object> names)
    {
      for (int i = 0; i < source.Count; ++i)
      {
        RCBlock child = source.GetName (i);
        if (child.Name == "")
        {
          names.Push ((long) i);
        }
        else
        {
          names.Push (child.Name);
        }
        if (child.Value.IsBlock)
        {
          Meta (target, (RCBlock) child.Value, names);
        }
        else
        {
          object[] array = names.ToArray ();
          Array.Reverse (array);
          RCSymbolScalar symbol = RCSymbolScalar.From (array);
          target.WriteCell ("type", symbol, new RCSymbolScalar (null, child.Value.TypeName));
          target.WriteCell ("count", symbol, (long) child.Value.Count);
          target.Write (0, symbol);
        }
        names.Pop ();
      }
    }

    [RCVerb ("cubify")]
    public void EvalCubify (RCRunner runner, RCClosure closure, RCOperator right)
    {
      RCCube target = new RCCube (new RCArray<string> ("S"));
      target.ReserveColumn ("o");
      Stack<object> names = new Stack<object> ();
      right.Cubify (target, names);
      runner.Yield (closure, target);
    }

    [RCVerb ("cubify")]
    public void EvalCubify (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCCube target = new RCCube (new RCArray<string> ("S"));
      target.ReserveColumn ("o");
      Stack<object> names = new Stack<object> ();
      right.Cubify (target, names);
      runner.Yield (closure, target);
    }

    protected static RCCube MergeMultipleCubes (string[] names, RCCube[] cubes)
    {
      if (cubes.Length == 0)
      {
        return RCCube.Empty;
      }
      int doneCubes = 0;
      //Timeline[] sortedAxes = new Timeline[cubes.Length];
      // These maps take each cube from unsorted to sorted.
      Dictionary<long, int>[] sortedMaps = new Dictionary<long, int>[cubes.Length];
      // These maps take each cube from sorted (or should it be unsorted?) to merged. Accounting for matching timestamps.
      Dictionary<long, int>[] mergedMaps = new Dictionary<long, int>[cubes.Length];
      for (int i = 0; i < cubes.Length; ++i)
      {
        Dictionary<long, int> rankMap = RankUtils.DoAxisRank (cubes[i].Axis);
        Dictionary<long, int> invertedRankMap = new Dictionary<long, int> ();
        foreach (KeyValuePair<long, int> kv in rankMap)
        {
          invertedRankMap[kv.Value] = (int) kv.Key;
        }
        sortedMaps[i] = invertedRankMap;
        mergedMaps[i] = new Dictionary<long, int> ();
        if (cubes[i].Count == 0)
        {
          ++doneCubes;
        }
      }
      // Now build the merged axis from the sorted source axes.
      Timeline resultAxis = cubes[0].Axis.Match ();
      int[] sortedAxisIndex = new int[cubes.Length];
      for (int i = 0; i < cubes.Length; ++i)
      {
        if (cubes[i].Axis.Count == 0)
        {
          sortedAxisIndex[i] = -1;
        }
        else
        {
          sortedAxisIndex[i] = 0;
        }
      }

      Queue<int> cubesWithCurrentRow = new Queue<int> ();
      while (doneCubes < cubes.Length)
      {
        long g = -1;
        long e = -1;
        RCTimeScalar t = RCTimeScalar.Empty;
        RCSymbolScalar s = RCSymbolScalar.Empty;
        // Figure out which cube(s) have the next axis row.
        // The cube with the lowest ranking row.
        for (int i = 0; i < cubes.Length; ++i)
        {
          RCCube cube = cubes[i];
          //First, scan for the lowest ranking row in sortedAxisIndex.
          //Then find all matches.
          if (sortedAxisIndex[i] > -1)
          {
            if (cubesWithCurrentRow.Count == 0)
            {
              cubesWithCurrentRow.Enqueue (i);
              int rowInUnsorted = sortedMaps[i][sortedAxisIndex[i]];
              g = cube.Axis.Global != null ? cube.Axis.Global[rowInUnsorted] : -1;
              e = cube.Axis.Event != null ? cube.Axis.Event[rowInUnsorted] : -1;
              t = cube.Axis.Time != null ? cube.Axis.Time[rowInUnsorted] : RCTimeScalar.Empty;
              s = cube.Axis.Symbol != null ? cube.Axis.Symbol[rowInUnsorted] : RCSymbolScalar.Empty;
            }
            else
            {
              int other = cubesWithCurrentRow.Peek ();
              // sortedAxisIndex goes from 0 to length.
              // Find the index in the source row that matches the desired sorted row.
              int unsortedIndexCurrent = sortedMaps[i][sortedAxisIndex[i]];
              int unsortedIndexOther = sortedMaps[other][sortedAxisIndex[other]];
              int comparison = cubes[other].Axis.Proto.CompareAxisRows (cubes[i].Axis,
                                                                        unsortedIndexCurrent,
                                                                        cubes[other].Axis,
                                                                        unsortedIndexOther);
              if (comparison < 0)
              {
                cubesWithCurrentRow.Clear ();
                cubesWithCurrentRow.Enqueue (i);
                int rowInUnsorted = sortedMaps[i][sortedAxisIndex[i]];
                cube = cubes[i];
                g = cube.Axis.Global != null ? cube.Axis.Global[rowInUnsorted] : -1;
                e = cube.Axis.Event != null ? cube.Axis.Event[rowInUnsorted] : -1;
                t = cube.Axis.Time != null ? cube.Axis.Time[rowInUnsorted] : RCTimeScalar.Empty;
                s = cube.Axis.Symbol != null ? cube.Axis.Symbol[rowInUnsorted] : RCSymbolScalar.Empty;
              }
              else if (comparison > 0)
              {
              }
              else if (comparison == 0)
              {
                cubesWithCurrentRow.Enqueue (i);
              }
            }
          }
        }
        // This must be the part where we update the mergedMaps
        while (cubesWithCurrentRow.Count > 0)
        {
          int i = cubesWithCurrentRow.Dequeue ();
          //mergedMaps[i].Add (sortedAxisIndex[i], resultAxis.Count);
          mergedMaps[i].Add (resultAxis.Count, sortedAxisIndex[i]);
          if (sortedAxisIndex[i] > -1)
          {
            ++sortedAxisIndex[i];
            if (sortedAxisIndex[i] >= cubes[i].Axis.Count)
            {
              // This means all rows have been merged and mapped.
              sortedAxisIndex[i] = -1;
              ++doneCubes;
            }
          }
          if (cubesWithCurrentRow.Count == 0)
          {
            resultAxis.Write (g, e, t, s);
          }
        }
      }

      Dictionary<int, Dictionary<string, int>> columnMaps = new Dictionary<int, Dictionary<string, int>> ();
      RCCube result = new RCCube (resultAxis);
      // Produce final column order for the result set.
      RCArray<string> columns = new RCArray<string> (8);
      for (int i = 0; i < cubes.Length; ++i)
      {
        columnMaps[i] = new Dictionary<string, int> ();
        for (int j = 0; j < cubes[i].Cols; ++j)
        {
          string colName;
          if (cubes[i].Columns.Count == 1 && names[i] != "")
          {
            colName = names[i];
          }
          else
          {
            colName = cubes[i].ColumnAt (j);
          }
          int columnIndex = columns.IndexOf (colName);
          if (columnIndex < 0)
          {
            columns.Write (colName);
            result.ReserveColumn (colName);
          }
          columnMaps[i][colName] = j;
        }
      }

      // Write source data to the result cube by consulting mergedMaps.
      for (int k = 0; k < result.Axis.Count; ++k)
      {
        for (int j = 0; j < columns.Count; ++j)
        {
          for (int i = 0; i < cubes.Length; ++i)
          {
            RCCube cube = cubes[i];
            ColumnBase column = null;
            int sourceCol;
            if (columnMaps[i].TryGetValue (columns[j], out sourceCol))
            {
              column = cube.GetColumn (sourceCol);
            }
            if (column != null)
            {
              string name = columns[j];
              RCArray<int> index = column.Index;
              // mergedMaps goes from a merged axis index to a sorted axis index
              if (mergedMaps[i].ContainsKey (k))
              {
                int sortedIndex = mergedMaps[i][k];
                // sortedMaps goes from a sorted axis index to an unsorted axis index
                int unsortedIndex = sortedMaps[i][sortedIndex];
                bool found;
                int unsortedVrow = index.BinarySearch (unsortedIndex, out found);
                if (found)
                {
                  object box = column.BoxCell (unsortedVrow);
                  RCSymbolScalar symbol = result.SymbolAt (k);
                  result.WriteCell (name, symbol, box, k, parsing:false, force:true);
                }
              }
            }
          }
        }
      }
      return result;
    }
  }
}


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
                RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, null, rtype);

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

                Type latype = typeof (RCVector<>).MakeGenericType (ltype); //parameters[0].ParameterType;
                Type ratype = typeof (RCVector<>).MakeGenericType (rtype);

                RCActivator.OverloadKey key0 = new RCActivator.OverloadKey (
                  verb.Name, ltype, rtype);
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
                  verb.Name, latype, rtype);
                cubeOpType = typeof (CubeMath).GetNestedType ("CubeOpLeftScalar`3").
                  MakeGenericType (ltype, rtype, otype);
                cubeOpMethod = typeof (CubeMath).GetMethod ("DyadicOpLeftScalar").
                  MakeGenericMethod (ltype, rtype, otype);
                cubeOp = Delegate.CreateDelegate (cubeOpType, cubeOpMethod);
                m_overloads.Add (key1, new Overload (cubeOp, scalarOp));

                RCActivator.OverloadKey key2 = new RCActivator.OverloadKey (
                  verb.Name, ltype, ratype);
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

                RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, null, rtype);
                Type vectoroptype = typeof (CubeMath).GetNestedType ("SeqCubeOpM`3").
                  MakeGenericType (stype, rtype, otype);
                //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
                MethodInfo vectoropMethod = typeof (CubeMath).GetMethod ("SequentialOpMonadic").
                  MakeGenericMethod (stype, rtype, otype);
                Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
                Type primoptype = typeof (CubeMath).GetNestedType ("SeqScalarOp`3").
                  MakeGenericType (stype, rtype, otype);
                Delegate primop = Delegate.CreateDelegate (primoptype, method);
                if (m_overloads.ContainsKey (key))
                  throw new Exception ("dispatch table already contains the key:" + key);
                m_overloads.Add (key, new Overload (vectorop, primop));

                RCActivator.OverloadKey key1 = new RCActivator.OverloadKey (verb.Name, typeof (RCVector<long>), rtype);
                Type vectoroptype1 = typeof (CubeMath).GetNestedType ("SeqCubeOpD`3").
                  MakeGenericType (stype, rtype, otype);
                MethodInfo vectoropMethod1 = typeof (CubeMath).GetMethod ("SequentialOpDyadic").
                  MakeGenericMethod (stype, rtype, otype);
                Delegate vectorop1 = Delegate.CreateDelegate (vectoroptype1, vectoropMethod1);
                Type primoptype1 = typeof (CubeMath).GetNestedType ("SeqScalarOp`3").
                  MakeGenericType (stype, rtype, otype);
                Delegate primop1 = Delegate.CreateDelegate (primoptype1, method);
                if (m_overloads.ContainsKey (key1))
                  throw new Exception ("dispatch table already contains the key:" + key1);
                m_overloads.Add (key1, new Overload (vectorop1, primop1));
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
                RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, latype, rtype);

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
                  throw new Exception ("dispatch table already contains the key:" + key);
                m_overloads.Add (key, new Overload (vectorop, primop));
              }
            }
            catch (Exception)
            {
              throw;
            }
          }
        }
      }
      catch (Exception)
      {
        throw;
      }
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

    public delegate RCValue SeqCubeOpM <S, R, O> (RCCube right, SeqScalarOp<S, R, O> op);
    public delegate RCValue SeqCubeOpD <S, R, O> (RCLong left, RCCube right, SeqScalarOp<S, R, O> op);
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
    public static RCValue DyadicOp <L, R, O> (
      RCCube left, RCCube right, ScalarOp<L, R, O> op)
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
        throw new Exception ("Neither cube may have timeline columns.");
      //Both cubes must have the same number of rows.
      if (left.Count != right.Count)
        throw new Exception ("Both cubes must have the same count.");
      //And the same number of columns.
      if (left.Cols != right.Cols)
        throw new Exception ("Both cubes must have the same number of columns.");

      if (left.Cols == 1 && right.Cols == 1)
      {
        RCArray<L> ldata = left.GetData<L> (0);
        RCArray<R> rdata = right.GetData<R> (0);
        for (int i = 0; i < left.Count; ++i)
        {
          data.Write (op (ldata[i], rdata[i]));
          index.Write (i);
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
        llast.Last (s, out l);
        rlast.Last (s, out r);
        oindex.Write (i);
        odata.Write (op (l, r));
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
        if (!(hasLeft || hasRight)) break;

        RCSymbolScalar symbol = null;
        IndexPair<O> pair = null;

        if (lt == rt)
        {
          symbol = rtimeline.SymbolAt (rindex[ri]);
          if (symbol == null)
            symbol = RCSymbolScalar.Empty;

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

    //Combine the significant statistics from RCScalar
    protected struct SeqState<S, O> { public S s; public O o; }

    public static RCValue SequentialOpMonadic<S, R, O> (RCCube right, SeqScalarOp<S, R, O> op)
      where S : struct where O : struct
    {
      return SequentialOpDyadic <S, R, O> (null, right, op);
    }

    public static RCValue SequentialOpDyadic<S, R, O> (RCLong left, RCCube right, SeqScalarOp<S, R, O> op)
      where S : struct where O : struct
    {
      if (right.Count == 0)
      {
        return right;
      }
      else if (right.Axis.Symbol != null && right.Axis.ColCount == 1)
      {
        RCArray<int> rindex = right.GetIndex<R> (0);
        RCArray<R> rdata = right.GetData<R> (0);
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
            }
            //We should change this to make it possible to say how many levels you want.
            //Specify the number from the beginning or the end using a negative number.
            scalar = scalar.Previous;
            --level;
          }
        }
        RCSymbolScalar[] symbol = new RCSymbolScalar[results.Count];
        results.Keys.CopyTo (symbol, 0);
        Array.Sort (symbol);
        RCArray<O> data = new RCArray<O> (results.Count);
        RCArray<int> index = new RCArray<int> (results.Count);
        for (int i = 0; i < symbol.Length; ++i)
        {
          index.Write (i);
          O x = results[symbol[i]].o;
          data.Write (x);
        }
        if (data.Count > 0)
        {
          Timeline axis = new Timeline (null, null, null,
                                        new RCArray<RCSymbolScalar> (symbol));
          ColumnBase column = ColumnBase.FromArray (axis, index, data);
          return new RCCube (axis,
                             new RCArray<string> ("x"),
                             new RCArray<ColumnBase> (column));
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
        Timeline rtimeline = right.Axis;
        RCArray<int> rindex = right.GetIndex<R> (0);
        RCArray<R> rdata = right.GetData<R> (0);
        Dictionary<RCSymbolScalar, SeqState<S,O>> states =
          new Dictionary<RCSymbolScalar, SeqState<S,O>> ();
        RCArray<int> index = new RCArray<int> ();
        RCArray<O> data = new RCArray<O> ();
        Comparer<O> c = Comparer<O>.Default;
        for (int i = 0; i < rdata.Count; ++i)
        {
          RCSymbolScalar symbol = rtimeline.SymbolAt (rindex[i]);
          SeqState<S,O> s = new SeqState<S,O> ();
          states.TryGetValue (symbol, out s);
          O o = op (ref s.s, rdata[i]);
          if (c.Compare (o, s.o) != 0)
          {
            s.o = o;
            index.Write (rindex[i]);
            data.Write (o);
            states[symbol] = s;
          }
        }
        if (data.Count > 0)
        {
          ColumnBase column = ColumnBase.FromArray (rtimeline, index, data);
          return new RCCube (
            rtimeline,
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
        name, left.GetType (0), right.GetType (0));
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
        name, left.GetType ().BaseType, right.GetType (0));
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
        name, left.GetType (0), right.GetType ().BaseType);
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, left, right);
      //The result should almost always be a cube.
      //But there might be a couple exceptions.
      RCValue result = (RCValue) overload.Invoke (left, right);
      return result;
    }

    public static RCValue Invoke (RCClosure closure, string name, RCCube right)
    {
      //RCVectorBase rvector = right.GetVector (0);
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, null, right.GetType (0));
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, right);
      //The result should almost always be a cube.
      //But there might be a couple exceptions.
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

    [RCVerb ("sqrt")] [RCVerb ("abs")] [RCVerb ("high")] [RCVerb ("low")]
    [RCVerb ("sum")] [RCVerb ("sums")] [RCVerb ("avg")]
    [RCVerb ("long")] [RCVerb ("double")] [RCVerb ("decimal")]
    [RCVerb ("byte")] [RCVerb ("string")] [RCVerb ("symbol")]
    [RCVerb ("boolean")] [RCVerb ("not")] [RCVerb ("upper")]
    [RCVerb ("lower")] [RCVerb ("length")] [RCVerb ("day")]
    [RCVerb ("hour")] [RCVerb ("minute")] [RCVerb ("second")]
    [RCVerb ("nano")] [RCVerb ("date")] [RCVerb ("daytime")]
    [RCVerb ("datetime")] [RCVerb ("timestamp")] [RCVerb ("timespan")]
    [RCVerb ("any")] [RCVerb ("all")] [RCVerb ("none")]
    public void EvalMonadic (RCRunner runner, RCClosure closure, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, Invoke (closure, op.Name, right));
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
    public void EvalAggregate (RCRunner runner, RCClosure closure, RCLong left, RCCube right)
    {
      RCOperator op = (RCOperator) closure.Code;
      if (right.Cols == 0)  
        runner.Yield (closure, new RCCube ());
      else runner.Yield (
        closure, Invoke (closure, op.Name, (RCVectorBase) left, right));
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
      if (key.Count != cube.Count)
      {
        throw new Exception ("New symbol column must have the same length as the old one.");
      }
      Timeline axis = new Timeline (cube.Axis.Global, cube.Axis.Event, cube.Axis.Time, key);
      axis.Count = cube.Count;
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
    public void EvalWhere (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      if (right.Columns.Count == 0)
      {
        runner.Yield (closure, RCCube.Empty);
        return;
      }
      Column<bool> wherecol = (Column<bool>)right.GetColumn (0);
      if (left.Axis.ColCount == 0)
      {
        WhereIndicator wherer = new WhereIndicator (left, wherecol.Data);
        runner.Yield (closure, wherer.Where ());
      }
      else if (left.Axis.ColCount == 1 && left.Axis.Symbol != null)
      {
        RCCube result = new RCCube (new RCArray<string> ("S"));
        for (int i = 0; i < wherecol.Data.Count; ++i)
        {
          if (wherecol.Data[i])
          {
            result.Write (right.Axis.SymbolAt (wherecol.Index[i]));
          }
        }
        result = Bang (result, left, null, false, false, true, true);
        runner.Yield (closure, result);
      }
      else
      {
        WhereLocator wherer = new WhereLocator (left, wherecol);
        runner.Yield (closure, wherer.Where ());
      }
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCCube left, RCBoolean right)
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
      leftCols.ExceptWith (rightCols);
      rightCols.ExceptWith (leftCols);
      //I just can't do it without constructing the symbols. I'm too stupid.
      RCSymbolScalar[] rightSyms = new RCSymbolScalar[right.Count];
      foreach (string joinCol in joinCols)
      {
        ColumnBase column = right.GetColumn (joinCol);
        if (column.Count != right.Count)
          throw new Exception ("join column may not have nulls");
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
          throw new Exception ("join column may not have nulls");
        for (int j = 0; j < column.Count; ++j)
        {
          object val = column.BoxCell (j);
          if (val.GetType () != typeof (RCSymbolScalar))
          {
            leftSyms[j] = new RCSymbolScalar (null, val);
          }
          else
          {
            leftSyms[j] = (RCSymbolScalar) val;
          }
        }
      }
      for (int i = 0; i < leftSyms.Length; ++i)
      {
        leftRow[i] = -1;
        for (int j = 0; j < rightSyms.Length; ++j)
        {
          if (leftSyms[i].Equals (rightSyms[j]))
          {
            leftRow[i] = j;
            break;
          }
        }
      }
      RCCube result = new RCCube (left);
      foreach (string rightCol in rightCols)
      {
        ColumnBase column = right.GetColumn (rightCol);
        for (int i = 0; i < leftSyms.Length; ++i)
        {
          if (leftRow[i] >= 0)
          {
            int vrow = column.Index.BinarySearch (leftRow[i]);
            object box = column.BoxCell (vrow);
            RCSymbolScalar symbol = result.Axis.SymbolAt (i);
            result.WriteCell (rightCol, symbol, box, i, false, false);
          }
        }
      }
      runner.Yield (closure, result);
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
      Bang (runner, closure, new RCCube (new RCArray<string> ("S")), right);
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
        throw new Exception ("except requires both cubes to have S");

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
      if (left.Axis.Symbol == null || right == null)
        throw new Exception ("except requires both cubes to have S");

      HashSet<RCSymbolScalar> rightSyms = new HashSet<RCSymbolScalar> (right);
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
      //result = Bang (result, right, null, false, false, false, true);
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
        return BangSymCubes (left, right, colname, insert, dedup, force, keepIncrs);
      }
      else
      {
        return BangRectCubes (left, right, colname);
      }
    }

    public RCCube Bang (RCCube left, RCBlock right,
                        bool insert, bool dedup, bool force, bool keepIncrs)
    {
      RCCube result = left;
      try
      {
        if (result.IsLocked)
        {
          result = new RCCube (left);
        }
        for (int i = 0; i < right.Count; ++i)
        {
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
          //Possible optimization here is to steal the underlying column object
          //because it will not change and we will not be changing it.
          ColumnBase column = left.GetColumn (col);
          for (int vrow = 0; vrow < column.Count; ++vrow)
          {
            int row = column.Index[vrow];
            RCSymbolScalar symbol = result.Axis.SymbolAt (row);
            object box = column.BoxCell (vrow);
            result.WriteCell (name, symbol, box, row, keepIncrs, force);
          }
        }
        else if (both.Contains (name))
        {
          //We have to do a careful merge for these columns.
          ColumnBase lcol = left.GetColumn (col);
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
          if (right.Axis.Symbol != null)
          {
            ColumnBase column = right.GetColumn (col);
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
            ColumnBase column = right.GetColumn (col);
            for (int row = 0; row < result.Axis.Count; ++row)
            {
              RCSymbolScalar symbol = result.Axis.SymbolAt (row);
              object box = column.BoxCell (row);
              result.WriteCell (name, symbol, box, row, false, force);
            }
          }
        }
      }
      return result;
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
        result = new RCCube (new RCArray<string> ("S"));
        for (int i = 0; i < left.Axis.Count; ++i)
        {
          result.Write (left.Axis.Symbol[i]);
        }
      }
      else if (right.Axis.Symbol != null)
      {
        result = new RCCube (new RCArray<string> ("S"));
        for (int i = 0; i < right.Axis.Count; ++i)
        {
          result.Write (right.Axis.Symbol[i]);
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

    public RCCube Bang (RCCube left, RCVectorBase right, 
                        string colname, bool force, bool keepIncrs)
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
      //Console.WriteLine("colname: " + colname);
      if (col < 0)
      {
        //Console.WriteLine("colname was not found");
        //Console.WriteLine("right.Count:" + right.Count + " left.Count:" + left.Count);
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
        else throw new Exception ();
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
      RCBlock first = data.GetName (0);
      bool hasS = first.Name != "";
      bool isRow = first.Value is RCBlock;
      if (isRow)
      {
        if (hasS)
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
          RCSymbolScalar symbol = null;
          if (hasS)
          {
            symbol = new RCSymbolScalar (null, rowkv.Name);
          }
          RCBlock row = (RCBlock) rowkv.Value;
          for (int j = 0; j < row.Count; ++j)
          {
            RCBlock colkv = row.GetName (j);
            RCVectorBase col = (RCVectorBase) colkv.Value;
            object box = col.Child (0);
            result.WriteCell (colkv.Name, symbol, box);
          }
          if (hasS)
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
      RCArray<long> G = null, E = null;
      RCArray<RCTimeScalar> T = null;
      RCArray<RCSymbolScalar> S = null;
      int count = -1;
      RCArray<string> names = new RCArray<string> (data.Count);
      RCArray<RCValue> vectors = new RCArray<RCValue> (data.Count);
      for (int i = 0; i < data.Count; ++i)
      {
        RCBlock block = data.GetName (i);
        if (block.Name == "G")
        {
          G = ((RCLong) block.Value).Data;
          count = G.Count;
        }
        else if (block.Name == "E")
        {
          E = ((RCLong) block.Value).Data;
          count = E.Count;
        }
        else if (block.Name == "T")
        {
          T = ((RCTime) block.Value).Data;
          count = T.Count;
        }
        else if (block.Name == "S")
        {
          S = ((RCSymbol) block.Value).Data;
          count = S.Count;
        }
        else
        {
          vectors.Write (block.Value);
          names.Write (block.Name);
        }
      }
      string name;
      RCCube cube;
      RCVectorBase vector;
      if (G != null || E != null || T != null || S != null)
      {
        Timeline axis = new Timeline (G, E, T, S);
        result = new RCCube (axis);
        //result.m_count = count;
      }
      for (int i = 0; i < vectors.Count; ++i)
      {
        name = names[i];
        cube = vectors[i] as RCCube;
        if (cube != null)
        {
          string colname = null;
          if (cube.Cols == 1 && name != "")
            colname = name;
          result = Bang (result, cube, colname, false, false, true, true);
          continue;
        }
        vector = vectors[i] as RCVectorBase;
        if (vector != null)
        {
          if (name == "")
            throw new Exception ("vector values must have names assigned");
          result = Bang (result, vector, name, true, true);
          continue;
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
      throw new Exception ("rename needs a new implementation specifically for cubes.");
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
            //result.m_columns.Write (i, new RCCube.ColumnOfNothing (result.Axis));
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
    public void EvalCubify (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCCube target = new RCCube (new RCArray<string> ("S"));
      target.ReserveColumn ("o");
      Stack<object> names = new Stack<object> ();
      right.Cubify (target, names);
      runner.Yield (closure, target);
    }
  }
}

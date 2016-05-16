
using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class VectorMath
  {
    protected static readonly Dictionary <RCActivator.OverloadKey, Overload> m_overloads =
      new Dictionary<RCActivator.OverloadKey, Overload> ();

    static VectorMath ()
    {
      foreach (MethodInfo method in typeof (ScalarMath).GetMethods ())
      {
        object[] attributes =
          method.GetCustomAttributes (typeof (Primitive), false);
        for (int i = 0; i < attributes.Length; ++i)
        {
          Primitive verb = (Primitive) attributes[i];
          ParameterInfo[] parameters = method.GetParameters ();
          if (verb.Profile == Profile.Monadic)
          {
            Type rtype = parameters[0].ParameterType;
            Type otype = method.ReturnType;
            RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, null, rtype);

            Type vectoroptype = typeof (VectorMath).GetNestedType ("VectorOp`2").
              MakeGenericType (rtype, otype);
            //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
            MethodInfo vectoropMethod = typeof (VectorMath).GetMethod ("MonadicOp").
              MakeGenericMethod (rtype, otype);
            Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
            Type primoptype = typeof (VectorMath).GetNestedType ("ScalarOp`2").
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

            RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, ltype, rtype);
            Type vectoroptype = typeof (VectorMath).GetNestedType ("VectorOp`3").
              MakeGenericType (ltype, rtype, otype);
            //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
            MethodInfo vectoropMethod = typeof (VectorMath).GetMethod ("DyadicOp").
              MakeGenericMethod (ltype, rtype, otype);
            Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
            Type primoptype = typeof (VectorMath).GetNestedType ("ScalarOp`3").
              MakeGenericType (ltype, rtype, otype);
            Delegate primop = Delegate.CreateDelegate (primoptype, method);
            if (m_overloads.ContainsKey (key))
              throw new Exception ("dispatch table already contains the key:" + key);
            m_overloads.Add (key, new Overload (vectorop, primop));
          }
          else if (verb.Profile == Profile.Cumulative)
          {
            //The parameter is a ref parameter which is actually a different type than the type
            //which is being referenced.  GetElementType () gives you the referenced type which is
            //what we want when constructing generic methods.
            Type stype = parameters[0].ParameterType.GetElementType ();
            Type rtype = parameters[1].ParameterType;
            Type otype = method.ReturnType;
            //The stype is not a parameter to the operator so it is not included in the key.
            RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, null, rtype);

            Type cumoptype = typeof (VectorMath).GetNestedType ("CumVectorOp`2").
              MakeGenericType (stype, rtype);
            MethodInfo cumopMethod = typeof (VectorMath).GetMethod ("CumulativeOp").
              MakeGenericMethod (stype, rtype);
            Type primoptype = typeof (VectorMath).GetNestedType ("SeqScalarOp`3").
              MakeGenericType (stype, rtype, otype);
            Delegate cumop = Delegate.CreateDelegate (cumoptype, cumopMethod);
            Delegate primop = Delegate.CreateDelegate (primoptype, method);

            if (m_overloads.ContainsKey (key))
              throw new Exception ("dispatch table already contains the key:" + key);
            m_overloads.Add (key, new Overload (cumop, primop));
          }
          else if (verb.Profile == Profile.Sequential)
          {
            //The parameter is a ref parameter which is actually a different type than the type
            //which is being referenced.  GetElementType () gives you the referenced type which is
            //what we want when constructing generic methods.
            Type stype = parameters[0].ParameterType.GetElementType ();
            Type rtype = parameters[1].ParameterType;
            Type otype = method.ReturnType;
            //The stype is not a parameter to the operator so it is not included in the key.
            RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, null, rtype);

            Type vectoroptype = typeof (VectorMath).GetNestedType ("SeqVectorOp`3").
              MakeGenericType (stype, rtype, otype);
            //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
            MethodInfo vectoropMethod = typeof (VectorMath).GetMethod ("SequentialOp").
              MakeGenericMethod (stype, rtype, otype);
            Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
            Type primoptype = typeof (VectorMath).GetNestedType ("SeqScalarOp`3").
              MakeGenericType (stype, rtype, otype);
            Delegate primop = Delegate.CreateDelegate (primoptype, method);

            if (m_overloads.ContainsKey (key))
              throw new Exception ("dispatch table already contains the key:" + key);
            m_overloads.Add (key, new Overload (vectorop, primop));
          }
          else if (verb.Profile == Profile.Contextual)
          {
            try
            {
            
            //The parameter is a ref parameter which is actually a different type than the type
            //which is being referenced.  GetElementType () gives you the referenced type which is
            //what we want when constructing generic methods.
            Type ctype = parameters[0].ParameterType;
            Type ltype = ctype.GetGenericArguments()[0];
            Type rtype = parameters[1].ParameterType;
            Type otype = method.ReturnType;
            //The stype is not a parameter to the operator so it is not included in the key.
            RCActivator.OverloadKey key = new RCActivator.OverloadKey (verb.Name, ltype, rtype);

            Type vectoroptype = typeof (VectorMath).GetNestedType ("ConVectorOp`4").
              MakeGenericType (ctype, ltype, rtype, otype);
            //Why do I have to do the backtick thingy on GetNestedType but not on GetMethod?
            MethodInfo vectoropMethod = typeof (VectorMath).GetMethod ("ContextualOp").
              MakeGenericMethod (ctype, ltype, rtype, otype);
            Delegate vectorop = Delegate.CreateDelegate (vectoroptype, vectoropMethod);
            Type primoptype = typeof (VectorMath).GetNestedType ("ConScalarOp`4").
              MakeGenericType (ctype, ltype, rtype, otype);
            Delegate primop = Delegate.CreateDelegate (primoptype, method);

            if (m_overloads.ContainsKey (key))
              throw new Exception ("dispatch table already contains the key:" + key);
            m_overloads.Add (key, new Overload (vectorop, primop));
            }
            catch (Exception ex)
            {
              throw;
            }
          }
        }
      }
    }

    public delegate O ScalarOp <R, O> (R r);
    public delegate O ScalarOp <L, R, O> (L l, R r);
    public delegate O SeqScalarOp <S, R, O> (ref S s, R r);
    public delegate O ConScalarOp <C, L, R, O> (C c, R r);

    public delegate RCArray<O> VectorOp <R, O> (
      RCArray<R> right, ScalarOp <R, O> op);
    public delegate RCArray<O> VectorOp <L, R, O> (
      RCArray<L> left, RCArray<R> right, ScalarOp<L, R, O> op);
    public delegate RCArray<O> SeqVectorOp <S, R, O> (
      RCArray<R> right, SeqScalarOp<S, R, O> op);
    public delegate RCArray<O> ConVectorOp <C, L, R, O> (
      RCArray<L> left, RCArray<R> right, ConScalarOp<C, L, R, O> op);
    public delegate RCArray<R> CumVectorOp <S, R> (
      RCArray<R> right, SeqScalarOp<S, R, R> op);

    public static RCArray<O> DyadicOp <L, R, O> (
      RCArray<L> left, RCArray<R> right, ScalarOp<L, R, O> op)
    {
      if (left.Count == 1)
      {
        RCArray<O> output = new RCArray<O> (right.Count);
        for (int i = 0; i < right.Count; ++i)
          output.Write (op (left[0], right[i]));
        return output;
      }
      else if (right.Count == 1)
      {
        RCArray<O> output = new RCArray<O> (left.Count);
        for (int i = 0; i < left.Count; ++i)
          output.Write (op (left[i], right[0]));
        return output;
      }
      else if (left.Count == right.Count)
      {
        RCArray<O> output = new RCArray<O> (left.Count);
        for (int i = 0; i < left.Count; ++i)
          output.Write (op (left[i], right[i]));
        return output;
      }
      else throw new Exception ("Both vectors must have the same count, or one of them must have a single element.");
    }

    public static RCArray<O> MonadicOp <R, O> (
      RCArray<R> right, ScalarOp<R, O> op)
    {
      RCArray<O> output = new RCArray<O> (right.Count);
      for (int i = 0; i < right.Count; ++i)
        output.Write (op (right[i]));
      return output;
    }

    public static RCArray<O> SequentialOp <S, R, O> (
      RCArray<R> right, SeqScalarOp<S, R, O> op)
      where S : struct where O : struct
    {
      S s = new S ();
      O o = new O ();
      for (int i = 0; i < right.Count; ++i)
        o = op (ref s, right[i]);
      return new RCArray<O> (o);
    }

    public static RCArray<O> ContextualOp <C, L, R, O> (
      RCArray<L> left, RCArray<R> right, ConScalarOp<C, L, R, O> op)
      where C : Context<L>, new ()
    {
      //Things left to do to make this work:
      //Add the equivalent on CubeMath.
      //Update both type constructors.
      //I think it will work well, perhaps a little overengineered...
      //Most people would just expand these two operations by hand rather than adding a new profile.
      //I have a feeling I will use this facility for other operations... I dunno, I'm going skiing.

      C c = new C ();
      c.Init (left);
      RCArray<O> output = new RCArray<O> ();
      for (int i = 0; i < right.Count; ++i)
        output.Write (op (c, right[i]));
      return output;
    }

    public static RCArray<R> CumulativeOp <S, R> (
      RCArray<R> right, SeqScalarOp <S, R, R> op)
      where S : struct
    {
      if (right.Count == 0)
      {
        return RCArray<R>.Empty;
      }
      RCArray<R> result = new RCArray<R> (right.Count);
      R accumulator = right[0];
      S state = new S ();
      for (int i = 0; i < right.Count; ++i)
      {
        accumulator = op (ref state, right[i]);
        result.Write (accumulator);
      }
      return result;
    }

    public static RCValue InvokeDyadic (
      RCClosure closure, string name, RCVectorBase left, RCVectorBase right)
    {
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, left.ScalarType, right.ScalarType);
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, left, right);
      object array = overload.Invoke (left.Array, right.Array);
      return RCVectorBase.FromArray (array);
    }

    public static RCValue InvokeMonadic (
      RCClosure closure, string name, RCVectorBase right)
    {
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, null, right.ScalarType);
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, right);
      object array = overload.Invoke (right.Array);
      return RCVectorBase.FromArray (array);
    }

    public static RCValue InvokeSequential (
      RCClosure closure, string name, RCVectorBase right)
    {
      RCActivator.OverloadKey key = new RCActivator.OverloadKey (
        name, null, right.ScalarType);
      Overload overload;
      if (!m_overloads.TryGetValue (key, out overload))
        throw RCException.Overload (closure, name, right);
      object array = overload.Invoke (right.Array);
      return RCVectorBase.FromArray (array);
    }

    [RCVerb ("+")] [RCVerb ("-")] [RCVerb ("*")] [RCVerb ("/")]
    [RCVerb ("and")] [RCVerb ("or")]
    [RCVerb ("==")] [RCVerb ("!=")] [RCVerb ("<")] [RCVerb (">")] [RCVerb ("<=")] [RCVerb (">=")]
    [RCVerb ("min")] [RCVerb ("max")]
    public void EvalDyadic (
      RCRunner runner, RCClosure closure, object left, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      runner.Yield (closure, VectorMath.InvokeDyadic (
        closure, op.Name, (RCVectorBase) left, (RCVectorBase) right));
    }

    [RCVerb ("+")] [RCVerb ("-")]
    public void EvalCumulative (
      RCRunner runner, RCClosure closure, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      runner.Yield (closure, VectorMath.InvokeMonadic (
        closure, op.Name, (RCVectorBase) right));
    }

    [RCVerb ("sum")] [RCVerb ("avg")] [RCVerb ("high")] [RCVerb ("low")]
    public void EvalSequential (
      RCRunner runner, RCClosure closure, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      runner.Yield (closure, VectorMath.InvokeSequential (
        closure, op.Name, (RCVectorBase) right));
    }

    [RCVerb ("not")] [RCVerb ("sqrt")] [RCVerb ("abs")]
    [RCVerb ("long")] [RCVerb ("double")] [RCVerb ("decimal")] [RCVerb ("byte")] [RCVerb ("string")] [RCVerb ("symbol")] [RCVerb ("boolean")] [RCVerb ("time")]
    [RCVerb ("upper")] [RCVerb ("lower")] [RCVerb ("length")]
    [RCVerb ("day")] [RCVerb ("hour")] [RCVerb ("minute")] [RCVerb ("second")] [RCVerb ("nano")]
    [RCVerb ("date")] [RCVerb ("daytime")] [RCVerb ("datetime")] [RCVerb ("timestamp")] [RCVerb ("timespan")]
    public void EvalMonadic (
      RCRunner runner, RCClosure closure, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      runner.Yield (closure, VectorMath.InvokeMonadic (
        closure, op.Name, (RCVectorBase) right));
    }

    [RCVerb ("typecode")]
    public void EvalTypecode (
      RCRunner runner, RCClosure closure, object right)
    {
      RCValue val = (RCValue) right;
      runner.Yield (closure, new RCString (val.TypeCode.ToString ()));
    }

    [RCVerb ("typename")]
    public void EvalTypename (
      RCRunner runner, RCClosure closure, object right)
    {
      RCValue val = (RCValue) right;
      runner.Yield (closure, new RCString (val.TypeName.ToString ()));
    }

    [RCVerb ("map")] [RCVerb ("replace")] [RCVerb ("part")] [RCVerb ("fill")]
    public void EvalContextual (
      RCRunner runner, RCClosure closure, object left, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      runner.Yield (closure, VectorMath.InvokeDyadic (
        closure, op.Name, (RCVectorBase) left, (RCVectorBase) right));
    }

    [RCVerb ("time")]
    public void EvalTime (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      RCArray<RCTimeScalar> result = new RCArray<RCTimeScalar> ();
      string name = left[0].Part (0).ToString ();
      RCTimeType type = (RCTimeType) Enum.Parse (typeof (RCTimeType), name, true);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (new RCTimeScalar (new DateTime (right[i]), type));
      }
      runner.Yield (closure, new RCTime (result));
    }
      
    [RCVerb ("symbol")]
    public void EvalSymbolString (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCLexer lexer = new RCLexer (new RCArray<RCTokenType> (
        RCTokenType.Number, RCTokenType.Boolean, RCTokenType.Symbol, RCTokenType.Name));
      RCArray<RCSymbolScalar> result = new RCArray<RCSymbolScalar> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        RCSymbolScalar symbol = RCSymbolScalar.From (lexer, right[i]);
        result.Write (symbol);
      }
      runner.Yield (closure, new RCSymbol (result));
    }

    [RCVerb ("reference")]
    public void EvalReferenceString (
      RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCLReference (right.ToArray ()));
    }

    [RCVerb ("reference")]
    public void EvalReferenceString (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("Only one symbol allowed in the right argument.");
      }
      object[] parts = right[0].ToArray ();
      string[] strings = new string[parts.Length];
      for (int i = 0; i < strings.Length; ++i)
      {
        strings[i] = parts[i].ToString ();
      }
      runner.Yield (closure, new RCLReference (strings));
    }
      
    [RCVerb ("in")] [RCVerb ("like")]
    public void EvalRightContextual (
      RCRunner runner, RCClosure closure, object left, object right)
    {
      RCOperator op = (RCOperator) closure.Code;
      runner.Yield (closure, VectorMath.InvokeDyadic (
        closure, op.Name, (RCVectorBase) right, (RCVectorBase) left));
    }
  }
}
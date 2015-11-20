
using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Each : RCOperator
  {
    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCByte right)
    {
      DoEach<byte> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCLong right)
    {
      DoEach<long> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCDouble right)
    {
      DoEach<double> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCDecimal right)
    {
      DoEach<decimal> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      DoEach<string> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCSymbol right)
    {
      DoEach<RCSymbolScalar> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCTime right)
    {
      DoEach<RCTimeScalar> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCBoolean right)
    {
      DoEach<bool> (runner, closure, left, right);
    }

    [RCVerb ("each")]
    public void EvalEach (
      RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      long i = closure.Index - 2;
      if (i < right.Count)
      {
        RCBlock result = new RCBlock ("R", ":", right.Get(i));
        left.Eval (runner, new RCClosure (
          closure, closure.Bot, left, closure.Left, result, 0));
      }
      else
      {
        runner.Yield (closure, closure.Parent.Result);
      }
    }

    protected virtual void DoEach<T> (
      RCRunner runner, RCClosure closure, RCBlock left, RCVector<T> right)
    {
      int i = closure.Index - 2;
      if (i < right.Count)
      {
        RCBlock result = new RCBlock ("R", ":",
                                      RCVectorBase.FromArray (new RCArray<T> (right[i])));
        left.Eval (runner,
                   new RCClosure (closure, closure.Bot, left, closure.Left, result, 0));
      }
      else
      {
        runner.Yield (closure, closure.Parent.Result);
      }
    }

    public override RCClosure Next (
      RCRunner runner, RCClosure tail, RCClosure previous, RCValue result)
    {
      if (previous.Index < 2)
        return base.Next (runner, tail, previous, result);
      
      RCValue right = previous.Result.Get ("1");
      if (previous.Index == 2)
      {
        RCBlock output = new RCBlock (null, "", ":", result);
        RCClosure parent = new RCClosure (
          previous.Parent, previous.Bot, previous.Code, previous.Left,
          output, previous.Index + 1);
        return new RCClosure (
          parent, previous.Bot, previous.Code, previous.Left,
          previous.Result, previous.Index + 1);
      }
      else if (right != null && previous.Index < right.Count + 2)
      {
        RCBlock output = new RCBlock (
          previous.Parent.Result, "", ":", result);
        RCClosure parent = new RCClosure (
          previous.Parent.Parent, previous.Bot, previous.Code, previous.Left,
          output, previous.Index + 1);
        return new RCClosure (
          parent, previous.Bot, previous.Code, previous.Left,
          previous.Result, previous.Index + 1);
      }
      else
      {
        return base.Next (runner, tail, previous, result);
      }
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      RCValue right = closure.Result.Get ("1");
      if (right == null)
        return base.IsLastCall (closure, arg);
      return closure.Index == right.Count + 2;
    }
  }
}

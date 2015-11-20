
using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Switch : RCOperator
  {
    protected delegate RCValue Picker<T>(T val);

    [RCVerb ("switch")]
    public void EvalSwitch (
      RCRunner runner, RCClosure closure, RCBoolean left, RCBlock right)
    {
      Picker<bool> picker = delegate (bool val)
      {
        long i = val ? 0 : 1;
        return i >= right.Count ? RCBlock.Empty : right.Get (i);
      };
      DoSwitch<bool> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (
      RCRunner runner, RCClosure closure, RCByte left, RCBlock right)
    {
      Picker<byte> picker = delegate (byte val)
      {
        long i = val < 0 ? 1 : 0;
        return i >= right.Count ? RCBlock.Empty : right.Get (i);
      };
      DoSwitch<byte> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (
      RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      //What on earth was I thinking... we need to make this work.
      Picker<long> picker = delegate (long val)
      {
        long i = val < 0 ? 1 : 0;
        return i >= right.Count ? RCBlock.Empty : right.Get (i);
      };
      DoSwitch<long> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (
      RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      Picker<RCSymbolScalar> picker = delegate (RCSymbolScalar val)
      {
        if (val.Count > 1)
        {
          throw new Exception (
            "switch only supports block lookups using tuples of count 1.  But this could change.");
        }
        RCValue code = right.Get ((string) val.Key);
        if (code == null) 
        {
          code = RCBlock.Empty;
        }
        return code;
      };
      DoSwitch<RCSymbolScalar> (runner, closure, left, right, picker);
    }

    protected virtual void DoSwitch<T> (
      RCRunner runner, RCClosure closure, RCVector<T> left, RCBlock right, Picker<T> picker)
    {
      int i = closure.Index - 2;
      if (i < left.Count)
      {
        RCValue code = picker (left[i]);
        RCClosure child = new RCClosure (
          closure, closure.Bot, code, closure.Left, RCBlock.Empty, 0);
        code.Eval (runner, child);
      }
      else
      {
        runner.Yield (closure, closure.Parent.Result);
      }
    }

    //This higher order thingy needs to go away it makes no sense.
    public override bool IsHigherOrder ()
    {
      return true;
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      if (arg == null)
        return base.IsLastCall (closure, arg);
      if (!base.IsLastCall (closure, arg))
        return false;
      return arg.Code.IsBeforeLastCall (arg);
    }
  }
}
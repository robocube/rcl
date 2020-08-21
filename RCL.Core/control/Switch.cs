
using System;
using RCL.Kernel;

namespace RCL.Core
{
  public class Switch : RCOperator
  {
    protected delegate RCValue Picker<T> (T val, out bool eval);

    [RCVerb ("switch")]
    public void EvalSwitch (RCRunner runner, RCClosure closure, RCBoolean left, RCBlock right)
    {
      Picker<bool> picker = delegate (bool val, out bool eval)
      {
        long i = val ? 0 : 1;
        RCBlock variable = right.GetName (i);
        eval = !variable.Evaluator.Pass;
        return i >= right.Count ? RCBlock.Empty : variable.Value;
      };
      DoSwitch<bool> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (RCRunner runner, RCClosure closure, RCByte left, RCBlock right)
    {
      Picker<byte> picker = delegate (byte val, out bool eval)
      {
        RCBlock variable = right.GetName (val);
        long i = val < 0 ? 1 : 0;
        eval = !variable.Evaluator.Pass;
        return i >= right.Count ? RCBlock.Empty : variable.Value;
      };
      DoSwitch<byte> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      // What on earth was I thinking... we need to make this work.
      Picker<long> picker = delegate (long val, out bool eval)
      {
        RCBlock variable = right.GetName (val);
        eval = !variable.Evaluator.Pass;
        return variable.Value;
      };
      DoSwitch<long> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      Picker<RCSymbolScalar> picker = delegate (RCSymbolScalar val, out bool eval)
      {
        if (val.Length > 1) {
          throw new Exception (
                  "switch only supports block lookups using tuples of count 1.  But this could change.");
        }
        RCBlock variable = right.GetName ((string) val.Key);
        RCValue code;
        // This behavior is sketchy and should be reevaluated - this should be an
        // exception
        if (variable == null) {
          code = RCBlock.Empty;
          eval = true;
        }
        else {
          code = variable.Value;
          eval = !variable.Evaluator.Pass;
        }
        return code;
      };
      DoSwitch<RCSymbolScalar> (runner, closure, left, right, picker);
    }

    [RCVerb ("switch")]
    public void EvalSwitch (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      Picker<string> picker = delegate (string val, out bool eval)
      {
        RCBlock variable = right.GetName (val);
        RCValue code;
        // This behavior is sketchy and should be reevaluated - this should be an
        // exception
        if (variable == null) {
          code = RCBlock.Empty;
          eval = true;
        }
        else {
          code = variable.Value;
          eval = !variable.Evaluator.Pass;
        }
        return code;
      };
      DoSwitch<string> (runner, closure, left, right, picker);
    }

    [RCVerb ("then")]
    public void EvalThen (RCRunner runner, RCClosure closure, RCBoolean left, RCBlock right)
    {
      if (left[0]) {
        int i = closure.Index - 2;
        if (i < left.Count) {
          RCClosure child = new RCClosure (closure,
                                           closure.Bot,
                                           right,
                                           closure.Left,
                                           RCBlock.Empty,
                                           0);
          right.Eval (runner, child);
        }
        else {
          runner.Yield (closure, closure.Parent.Result);
        }
      }
      else {
        runner.Yield (closure, RCBoolean.False);
      }
    }

    protected virtual void DoSwitch<T> (RCRunner runner,
                                        RCClosure closure,
                                        RCVector<T> left,
                                        RCBlock right,
                                        Picker<T> picker)
    {
      int i = closure.Index - 2;
      if (i < left.Count) {
        bool eval;
        RCValue code = picker (left[i], out eval);
        if (eval) {
          RCClosure child = new RCClosure (closure,
                                           closure.Bot,
                                           code,
                                           closure.Left,
                                           RCBlock.Empty,
                                           0);
          code.Eval (runner, child);
        }
        else {
          // In this case the "code" can be a value, normally part of a generated program
          runner.Yield (closure, code);
        }
      }
      else {
        runner.Yield (closure, closure.Parent.Result);
      }
    }

    // This higher order thingy needs to go away it makes no sense.
    public override bool IsHigherOrder ()
    {
      return false;
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      if (arg == null) {
        return base.IsLastCall (closure, arg);
      }
      if (!base.IsLastCall (closure, arg)) {
        return false;
      }
      bool isBeforeLastCall = arg.Code.IsBeforeLastCall (arg);
      return isBeforeLastCall;
    }
  }
}

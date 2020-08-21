using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Sub
  {
    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoSub<byte> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoSub<long> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoSub<double> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoSub<decimal> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoSub<bool> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoSub<string> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoSub<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("sub")]
    public void EvalSub (RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoSub<RCTimeScalar> (left, right)));
    }

    protected RCArray<T> DoSub<T> (RCVector<T> left, RCVector<T> right)
    {
      Dictionary<T, T> map = new Dictionary<T, T> ();
      for (int i = 0; i < left.Count; ++i, ++i)
      {
        map.Add (left[i], left[i + 1]);
      }
      RCArray<T> result = new RCArray<T> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        T val;
        if (map.TryGetValue (right[i], out val)) {
          result.Write (val);
        }
        else {
          result.Write (right[i]);
        }
      }
      return result;
    }
  }
}
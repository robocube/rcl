
using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Reverse
  {
    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoReverse<byte> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoReverse<long> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoReverse<double> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoReverse<decimal> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoReverse<bool> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoReverse<string> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoReverse<RCSymbolScalar> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoReverse<RCTimeScalar> (right)));
    }

    [RCVerb ("reverse")]
    public void EvalReverse (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCBlock result = RCBlock.Empty;
      RCBlock current = right;
      while (current.Count > 0)
      {
        result = new RCBlock (result, current.Name, current.Evaluator, current.Value);
        current = current.Previous;
      }
      runner.Yield (closure, result);
    }

    protected RCArray<T> DoReverse<T> (RCVector<T> right)
    {
      RCArray<T> results = new RCArray<T> (right.Count);
      for (int i = right.Count - 1; i >= 0; --i)
      {
        results.Write (right[i]);
      }
      return results;
    }
  }
}

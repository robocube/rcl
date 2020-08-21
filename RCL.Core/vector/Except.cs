
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Except
  {
    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoExcept<long> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoExcept<double> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoExcept<decimal> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoExcept<byte> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoExcept<bool> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoExcept<string> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoExcept<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoExcept<RCTimeScalar> (left, right)));
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < left.Count; ++i)
      {
        RCBlock name = left.GetName (i);
        if (!right.Data.Contains (name.Name)) {
          result = new RCBlock (result,
                                name.Name,
                                name.Evaluator,
                                name.Value);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("except")]
    public void EvalExcept (RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      throw new System.NotImplementedException ("Implementation should be similar to merge");
    }

    protected RCArray<T> DoExcept<T> (RCVector<T> left, RCVector<T> right)
    {
      HashSet<T> results = new HashSet<T> (left);
      for (int i = 0; i < right.Count; ++i)
      {
        if (results.Contains (right[i])) {
          results.Remove (right[i]);
        }
      }
      T[] array = new T[results.Count];
      results.CopyTo (array);
      return new RCArray<T> (array);
    }
  }
}

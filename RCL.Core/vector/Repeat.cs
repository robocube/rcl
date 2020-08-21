using System;
using System.Text;
using RCL.Kernel;

namespace RCL.Core
{
  /// <summary>
  /// Operations to create vectors with repeated values.
  /// </summary>
  public class Repeat
  {
    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoRepeat<byte> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoRepeat<long> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoRepeat<double> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoRepeat<decimal> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoRepeat<bool> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      runner.Yield (closure, new RCString (DoRepeat<string> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoRepeat<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoRepeat<RCTimeScalar> (left, right)));
    }

    [RCVerb ("repeat")]
    public void EvalRepeat (RCRunner runner, RCClosure closure, RCLong left, RCIncr right)
    {
      runner.Yield (closure, new RCIncr (DoRepeat<RCIncrScalar> (left, right)));
    }

    protected RCArray<T> DoRepeat<T> (RCVector<long> left, RCVector<T> right)
    {
      long count = left[0];
      RCArray<T> result = new RCArray<T> ((int) count * right.Count);
      for (int i = 0; i < count; ++i)
      {
        result.Write (right.Data);
      }
      return result;
    }
  }
}

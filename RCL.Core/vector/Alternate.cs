using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Alternate
  {
    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoAlternate<byte> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoAlternate<long> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoAlternate<double> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoAlternate<decimal> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoAlternate<bool> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoAlternate<string> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoAlternate<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("alternate")]
    public void EvalAlternate (
      RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoAlternate<RCTimeScalar> (left, right)));
    }

    protected RCArray<T> DoAlternate<T> (RCVector<T> left, RCVector<T> right)
    {
      if (left.Count != right.Count)
      {
        throw new Exception ("alternate requires equal length vectors");
      }
      RCArray<T> result = new RCArray<T> (left.Count * 2);
      for (int i = 0; i < left.Count; ++i)
      {
        result.Write (left[i]);
        result.Write (right[i]);
      }
      return result;
    }
  }
}

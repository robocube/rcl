
using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Union
  {
    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoUnion<long> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoUnion<double> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoUnion<decimal> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoUnion<byte> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoUnion<bool> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoUnion<string> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoUnion<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("union")]
    public void EvalUnion (RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoUnion<RCTimeScalar> (left, right)));
    }

    protected RCArray<T> DoUnion<T> (RCVector<T> left, RCVector<T> right)
    {
      HashSet<T> results = new HashSet<T> (left);
      for (int i = 0; i < right.Count; ++i)
        results.Add (right[i]);
      T[] array = new T[results.Count];
      results.CopyTo (array);
      return new RCArray<T> (array);
    }
  }
}

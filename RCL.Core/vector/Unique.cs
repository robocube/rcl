using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Unique
  {
    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoUnique<byte> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoUnique<long> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoUnique<double> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoUnique<decimal> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoUnique<bool> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoUnique<string> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoUnique<RCSymbolScalar> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (
      RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoUnique<RCTimeScalar> (right)));
    }

    protected RCArray<T> DoUnique<T> (RCVector<T> right)
    {
      RCArray<T> results = new RCArray<T> ();
      HashSet<T> items = new HashSet<T> ();

      for (int i = 0; i < right.Count; ++i)
      {
        if (!items.Contains (right[i]))
        {
          items.Add (right[i]);
          results.Write (right[i]);
        }
      }
      return results;
    }
  }
}
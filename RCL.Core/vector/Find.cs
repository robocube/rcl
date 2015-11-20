using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using RCL.Kernel;

namespace RCL.Core
{
  public class Find
  {
    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, DoFind<bool> (RCBoolean.True, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, DoFind<bool> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, DoFind<byte> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, DoFind<double> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, DoFind<long> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, DoFind<decimal> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, DoFind<string> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, DoFind<RCSymbolScalar> (left, right));
    }

    [RCVerb ("find")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, DoFind<RCTimeScalar> (left, right));
    }

    protected RCLong DoFind<T> (RCVector<T> things, RCVector<T> within)
      where T:IComparable<T>
    {
      RCArray<long> result = new RCArray<long>();
      for (int i = 0; i < within.Count; ++i)
      {
        for (int j = 0; j < things.Count; ++j)
        {
          if (things[j].CompareTo (within[i]) == 0)
          {
            result.Write (i);
          }
        }
      }
      return new RCLong (result);
    }
  }
}
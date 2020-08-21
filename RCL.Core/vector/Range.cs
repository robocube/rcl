
using System;
using RCL.Kernel;

namespace RCL.Core
{
  public class Range
  {
    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCByte right)
    {
      runner.Yield (closure, DoRange<byte> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, DoRange<long> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCDouble right)
    {
      runner.Yield (closure, DoRange<double> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCDecimal right)
    {
      runner.Yield (closure, DoRange<decimal> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      runner.Yield (closure, DoRange<string> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCSymbol right)
    {
      runner.Yield (closure, DoRange<RCSymbolScalar> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCTime right)
    {
      runner.Yield (closure, DoRange<RCTimeScalar> (left, right));
    }

    [RCVerb ("range")]
    public void EvalRange (RCRunner runner, RCClosure closure, RCLong left, RCIncr right)
    {
      runner.Yield (closure, DoRange<RCIncrScalar> (left, right));
    }

    public static RCVector<L> DoRange<L> (RCVector<long> left, RCVector<L> right)
    {
      if (left.Count == 0) {
        return (RCVector<L>)RCVectorBase.FromArray (new RCArray<L> ());
      }
      else if (left.Count == 1) {
        RCArray<L> result = new RCArray<L> ();
        for (int i = (int) left[0]; i < right.Count; ++i)
        {
          result.Write (right[i]);
        }
        return (RCVector<L>)RCVectorBase.FromArray (new RCArray<L> (result));
      }
      else if (left.Count % 2 == 0) {
        RCArray<L> result = new RCArray<L> ();
        int pair = 0;
        while (pair < left.Count / 2)
        {
          int i = (int) left[2 * pair];
          int j = (int) left[2 * pair + 1];
          while (i <= j)
          {
            result.Write (right[i]);
            ++i;
          }
          ++pair;
        }
        return (RCVector<L>)RCVectorBase.FromArray (new RCArray<L> (result));
      }
      else {
        throw new Exception ("count of left argument must be 1 or an even number.");
      }
    }
  }
}

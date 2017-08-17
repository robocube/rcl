
using RCL.Kernel;

namespace RCL.Core
{
  public class Where
  {
    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCByte left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<byte> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCLong left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<long> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCDouble left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<double> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCDecimal left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<decimal> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCString left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<string> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCSymbol left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<RCSymbolScalar> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCTime left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<RCTimeScalar> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<bool> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCIncr left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<RCIncrScalar> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (
      RCRunner runner, RCClosure closure, RCBlock left, RCBoolean right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i])
        {
          RCBlock current = left.GetName (i);
          result = new RCBlock (result, current.Name, current.Evaluator, current.Value);
        }
      }
      runner.Yield (closure, result);
    }

    public static RCVector<L> DoWhere<L> (RCVector<L> left, RCVector<bool> right)
    {
      RCArray<L> result = new RCArray<L> ();
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i])
        {
          result.Write (left[i]);
        }
      }
      return (RCVector<L>) RCVectorBase.FromArray (new RCArray<L> (result));
    }
  }
}


using System;
using RCL.Kernel;

namespace RCL.Core
{
  public class Where
  {
    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCByte left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<byte> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCLong left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<long> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCDouble left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<double> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCDecimal left, RCBoolean right)
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
    public void EvalWhere (RCRunner runner, RCClosure closure, RCSymbol left, RCBoolean right)
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
    public void EvalWhere (RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<bool> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCIncr left, RCBoolean right)
    {
      runner.Yield (closure, DoWhere<RCIncrScalar> (left, right));
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCBlock left, RCBoolean right)
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

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      RCBlock result = RecursiveWhere (left, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("where")]
    public void EvalWhere (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      RCArray<long> result = new RCArray<long> (8);
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i])
        {
          result.Write ((long) i);
        }
      }
      runner.Yield (closure, new RCLong (result));
    }

    protected RCBlock RecursiveWhere (RCBlock left, RCBlock right)
    {
      if (left.Count != right.Count)
      {
        throw new Exception (string.Format ("Left count was {0} but right count was {1}. Counts must match", left.Count, right.Count));
      }
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < left.Count; ++i)
      {
        RCBlock leftName = left.GetName (i);
        RCValue rightValue = right.Get (i);
        if (rightValue is RCBlock)
        {
          RCBlock rightBlock = (RCBlock) rightValue;
          RCBlock leftBlock = left.GetBlock (i);
          RCBlock childResult = RecursiveWhere (leftBlock, rightBlock);
          if (childResult.Count > 0)
          {
            result = new RCBlock (result, leftName.Name, leftName.Evaluator, childResult);
          }
        }
        else if (rightValue is RCBoolean)
        {
          if (rightValue.Count == 1)
          {
            if (right.GetBoolean (i))
            {
              result = new RCBlock (result, leftName.Name, ":", leftName.Value);
            }
          }
          else
          {
            RCBoolean rightVector = (RCBoolean) rightValue;
            RCBlock leftBlock = left.GetBlock (i);
            RCBlock childResult = RCBlock.Empty;
            for (int j = 0; j < rightVector.Count; ++j)
            {
              RCBlock leftVar = leftBlock.GetName (j);
              if (rightVector[j])
              {
                childResult = new RCBlock (childResult, leftVar.Name, ":", leftVar.Value);
              }
            }
            if (childResult.Count > 0)
            {
              result = new RCBlock (result, leftName.Name, leftName.Evaluator, childResult);
            }
          }
        }
      }
      return result;
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

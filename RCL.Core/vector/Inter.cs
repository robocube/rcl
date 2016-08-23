
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Inter
  {
    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoInter<long> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoInter<double> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoInter<decimal> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoInter<byte> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoInter<bool> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoInter<string> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoInter<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoInter<RCTimeScalar> (left, right)));
    }

    [RCVerb ("inter")]
    public void EvalInter (
      RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < left.Count; ++i)
      {
        RCBlock name = left.GetName (i);
        if (right.Data.Contains (name.Name))
        {
          result = new RCBlock (result, 
                                name.Name, 
                                name.Evaluator, 
                                name.Value);
        }
      }
      runner.Yield (closure, result);
    }

    protected RCArray<T> DoInter<T> (RCVector<T> left, RCVector<T> right)
    {
      HashSet<T> lhs = new HashSet<T> (left);
      HashSet<T> rsh = new HashSet<T> (right);
      lhs.IntersectWith (rsh);
      T[] array = new T[lhs.Count];
      lhs.CopyTo (array);
      return new RCArray<T> (array);
    }
  }
}

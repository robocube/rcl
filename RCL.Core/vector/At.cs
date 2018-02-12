
using System;
using System.Text;
using RCL.Kernel;

/// <summary>
/// General purpose operations on vectors.
/// </summary>
namespace RCL.Core
{
  public class At
  {
    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, DoAt<byte> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCByte left, RCLong right)
    {
      runner.Yield (closure, DoAt<byte> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCByte left, RCDouble right)
    {
      runner.Yield (closure, DoAt<byte> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCByte left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<byte> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBoolean left, RCLong right)
    {
      runner.Yield (closure, DoAt<bool> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBoolean left, RCDouble right)
    {
      runner.Yield (closure, DoAt<bool> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBoolean left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<bool> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBoolean left, RCByte right)
    {
      runner.Yield (closure, DoAt<bool> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDouble left, RCLong right)
    {
      runner.Yield (closure, DoAt<double> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, DoAt<double> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDouble left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<double> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDouble left, RCByte right)
    {
      runner.Yield (closure, DoAt<double> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, DoAt<long> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCLong left, RCDouble right)
    {
      runner.Yield (closure, DoAt<long> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCLong left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<long> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCLong left, RCByte right)
    {
      runner.Yield (closure, DoAt<long> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDecimal left, RCLong right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDecimal left, RCDouble right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCDecimal left, RCByte right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCString left, RCLong right)
    {
      runner.Yield (closure, DoAt<string> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCString left, RCDouble right)
    {
      runner.Yield (closure, DoAt<string> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCString left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<string> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCString left, RCByte right)
    {
      runner.Yield (closure, DoAt<string> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCSymbol left, RCDouble right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCSymbol left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCSymbol left, RCByte right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCTime left, RCLong right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCTime left, RCDouble right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCTime left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCTime left, RCByte right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBlock left, RCLong right)
    {
      runner.Yield (closure, DoAt (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBlock left, RCDouble right)
    {
      runner.Yield (closure, DoAt (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBlock left, RCDecimal right)
    {
      runner.Yield (closure, DoAt (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBlock left, RCByte right)
    {
      runner.Yield (closure, DoAt (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      runner.Yield (closure, DoAt (closure, left, right));
    }

    [RCVerb ("at")]
    public void EvalOperator (RCRunner runner, RCClosure closure, RCBlock left, RCSymbol right)
    {
      runner.Yield (closure, DoAt (closure, left, right));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCByte right)
    {
      runner.Yield (closure, DoAt<byte> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCByte right)
    {
      runner.Yield (closure, DoAt<byte> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCByte right)
    {
      runner.Yield (closure, DoAt<byte> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, DoAt<byte> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCBoolean right)
    {
      runner.Yield (closure, DoAt<bool> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCBoolean right)
    {
      runner.Yield (closure, DoAt<bool> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCBoolean right)
    {
      runner.Yield (closure, DoAt<bool> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCBoolean right)
    {
      runner.Yield (closure, DoAt<bool> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCDouble right)
    {
      runner.Yield (closure, DoAt<double> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, DoAt<double> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCDouble right)
    {
      runner.Yield (closure, DoAt<double> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCDouble right)
    {
      runner.Yield (closure, DoAt<double> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, DoAt<long> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCLong right)
    {
      runner.Yield (closure, DoAt<long> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCLong right)
    {
      runner.Yield (closure, DoAt<long> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCLong right)
    {
      runner.Yield (closure, DoAt<long> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCDecimal right)
    {
      runner.Yield (closure, DoAt<decimal> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      runner.Yield (closure, DoAt<string> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCString right)
    {
      runner.Yield (closure, DoAt<string> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCString right)
    {
      runner.Yield (closure, DoAt<string> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCString right)
    {
      runner.Yield (closure, DoAt<string> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCSymbol right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCSymbol right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCSymbol right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCSymbol right)
    {
      runner.Yield (closure, DoAt<RCSymbolScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCTime right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCTime right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCTime right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCTime right)
    {
      runner.Yield (closure, DoAt<RCTimeScalar> (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      runner.Yield (closure, DoAt (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDouble left, RCBlock right)
    {
      runner.Yield (closure, DoAt (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCDecimal left, RCBlock right)
    {
      runner.Yield (closure, DoAt (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCByte left, RCBlock right)
    {
      runner.Yield (closure, DoAt (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      runner.Yield (closure, DoAt (closure, right, left));
    }

    [RCVerb ("from")]
    public void EvalFrom (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      runner.Yield (closure, DoAt (closure, right, left));
    }

    public static RCBlock DoAt (RCClosure closure, RCBlock left, RCLong right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        //O(n) lookup, kinda sucky.
        RCBlock next = left.GetName (right[i]);
        result = new RCBlock (result, next.Name, next.Evaluator, next.Value);
      }
      return result;
    }

    public static RCBlock DoAt (RCClosure closure, RCBlock left, RCDouble right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        //O(n) lookup, kinda sucky.
        RCBlock next = left.GetName ((long)right[i]);
        result = new RCBlock (result, next.Name, next.Evaluator, next.Value);
      }
      return result;
    }

    public static RCBlock DoAt (RCClosure closure, RCBlock left, RCDecimal right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        //O(n) lookup, kinda sucky.
        RCBlock next = left.GetName ((long)right[i]);
        result = new RCBlock (result, next.Name, next.Evaluator, next.Value);
      }
      return result;
    }

    public static RCBlock DoAt (RCClosure closure, RCBlock left, RCByte right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        //O(n) lookup, kinda sucky.
        RCBlock next = left.GetName ((long)right[i]);
        result = new RCBlock (result, next.Name, next.Evaluator, next.Value);
      }
      return result;
    }

    public static RCBlock DoAt (RCClosure closure, RCBlock left, RCString right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        string name = right[i];
        RCBlock next = left.GetName (name);
        if (next == null)
        {
          string message = string.Format ("at: Name '{0}' not found within block", name);
          throw new RCException (closure, RCErrors.Name, message);
        }
        result = new RCBlock (result, next.Name, next.Evaluator, next.Value);
      }
      return result;
    }

    public static RCBlock DoAt (RCClosure closure, RCBlock left, RCSymbol right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        //O(n) lookup, kinda sucky.
        if (right[i].Length > 1) 
        {
          throw new Exception ("at only supports block lookups using tuples of count 1.  But this could change.");
        }
        string name = (string) right[i].Key;
        RCBlock next = left.GetName (name);
        if (next == null)
        {
          string message = string.Format ("at: Name '{0}' not found within block", name);
          throw new RCException (closure, RCErrors.Name, message);
        }
        result = new RCBlock (result, next.Name, next.Evaluator, next.Value);
      }
      return result;
    }

    public static RCVector<L> DoAt<L> (RCClosure closure, RCVector<L> left, RCVector<byte> right)
    {
      L[] result = new L[right.Count];
      for (int i = 0; i < right.Count; ++i)
      {
        int j = right[i];
        if (j < 0 || j >= left.Count)
        {
          throw RCException.Range (closure, right[i], right.Count);
        }
        result[i] = left[j];
      }
      return (RCVector<L>) RCVectorBase.FromArray (new RCArray<L> (result));
    }

    public static RCVector<L> DoAt<L> (RCClosure closure, RCVector<L> left, RCVector<long> right)
    {
      L[] result = new L[right.Count];
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i] < 0)
        {
          int j = (int) (left.Count + right[i]);
          if (j < 0 || j >= left.Count)
          {
            throw RCException.Range (closure, right[i], right.Count);
          }
          result[i] = left[j];
        }
        else
        {
          int j = (int) right[i];
          if (j < 0 || j >= left.Count)
          {
            throw RCException.Range (closure, right[i], right.Count);
          }
          result[i] = left[j];
        }
      }
      return (RCVector<L>) RCVectorBase.FromArray (new RCArray<L> (result));
    }

    public static RCVector<L> DoAt<L> (RCClosure closure, RCVector<L> left, RCVector<double> right)
    {
      L[] result = new L[right.Count];
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i] < 0)
        {
          int j = (int) (left.Count + right[i]);
          if (j < 0 || j >= left.Count)
          {
            throw RCException.Range (closure, (long) right[i], right.Count);
          }
          result[i] = left[j];
        }
        else
        {
          int j = (int) right[i];
          if (j < 0 || j >= left.Count)
          {
            throw RCException.Range (closure, (long) right[i], right.Count);
          }
          result[i] = left[j];
        }
      }
      return (RCVector<L>) RCVectorBase.FromArray (new RCArray<L> (result));
    }

    public static RCVector<L> DoAt<L> (RCClosure closure, RCVector<L> left, RCVector<decimal> right)
    {
      L[] result = new L[right.Count];
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i] < 0)
        {
          int j = (int) (left.Count + right[i]);
          if (j < 0 || j >= left.Count)
          {
            throw RCException.Range (closure, (long) right[i], right.Count);
          }
          result[i] = left[j];
        }
        else
        {
          int j = (int) right[i];
          if (j < 0 || j >= left.Count)
          {
            throw RCException.Range (closure, (long) right[i], right.Count);
          }
          result[i] = left[j];
        }
      }
      return (RCVector<L>) RCVectorBase.FromArray (new RCArray<L> (result));
    }
  }
}

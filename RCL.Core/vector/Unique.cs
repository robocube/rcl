using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Dups
  {
    [RCVerb ("dups")]
    public void EvalDups (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoDups<byte> (right)));
    }

    [RCVerb ("dups")]
    public void EvalDups (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoDups<long> (right)));
    }

    [RCVerb ("dups")]
    public void EvalDups (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoDups<string> (right)));
    }

    protected RCArray<T> DoDups<T> (RCVector<T> right)
    {
      RCArray<T> result = new RCArray<T> ();
      HashSet<T> items = new HashSet<T> ();
      for (int i = 0; i < right.Count; ++i)
      {
        if (!items.Contains (right[i]))
        {
          items.Add (right[i]);
        }
        else
        {
          // Three or more instances of the dup
          // will be represented two or more times
          result.Write (right[i]);
        }
      }
      return result;
    }
  }

  public class Unique
  {
    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoUnique<byte> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoUnique<long> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoUnique<double> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoUnique<decimal> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoUnique<bool> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoUnique<string> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoUnique<RCSymbolScalar> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoUnique<RCTimeScalar> (right)));
    }

    [RCVerb ("unique")]
    public void EvalUnique (RCRunner runner, RCClosure closure, RCBlock right)
    {
      // Preserve the names in the original order
      // Always use the last value for any given name
      RCBlock result = RCBlock.Empty;
      RCArray<string> names = new RCArray<string> ();
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock name = right.GetName (i);
        if (!names.Contains (name.Name))
        {
          names.Write (name.Name);
        }
      }
      for (int i = 0; i < names.Count; ++i)
      {
        RCBlock name = right.GetName (names[i]);
        result = new RCBlock (result, name.Name, name.Evaluator, name.Value);
      }
      runner.Yield (closure, result);
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

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
  public class First
  {
    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, right.Get (0));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCIncr right)
    {
      runner.Yield (closure, new RCIncr (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (right[0]));
    }

    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (right[0]));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 1; i < right.Count; ++i)
      {
        RCBlock variable = right.GetName (i);
        result = new RCBlock (result, variable.Name, variable.Evaluator, variable.Value);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoRest<long> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoRest<double> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoRest<decimal> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoRest<string> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoRest<bool> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoRest<RCSymbolScalar> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCIncr right)
    {
      runner.Yield (closure, new RCIncr (DoRest<RCIncrScalar> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoRest<RCTimeScalar> (right.Data)));
    }

    [RCVerb ("rest")]
    public void EvalRest (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoRest<byte> (right.Data)));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCIncr right)
    {
      runner.Yield (closure, new RCIncr (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (right[right.Count - 1]));
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCBlock last = right.GetName (right.Count - 1);
      RCBlock result = new RCBlock (last.Name, last.Evaluator.Symbol, last.Value);
      runner.Yield (closure, result);
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, right.Previous);
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoPop<long> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoPop<double> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoPop<decimal> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCString (DoPop<string> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoPop<bool> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoPop<RCSymbolScalar> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCIncr right)
    {
      runner.Yield (closure, new RCIncr (DoPop<RCIncrScalar> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoPop<RCTimeScalar> (right.Data)));
    }

    [RCVerb ("pop")]
    public void EvalPop (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoPop<byte> (right.Data)));
    }

    [RCVerb ("unwrap")]
    public void EvalUnwrap (RCRunner runner, RCClosure closure, RCBlock right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("monadic unwrap unwraps a block containing a single element");
      }
      runner.Yield (closure, right.Get (0));
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCBlock left, RCLong right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("right argument may only contain a single index");
      }
      RCValue result = left.Get (right[0]);
      if (result == null)
      {
        throw new RCException (closure, RCErrors.Range, "Index " + right[0] + " is out of range");
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left argument may only contain a single index");
      }
      RCValue result = right.Get (left[0]);
      if (result == null)
      {
        throw new RCException (closure, RCErrors.Range, "Index " + left[0] + " is out of range");
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("right argument may only contain a single name");
      }
      RCValue result = left.Get (right[0]);
      if (result == null)
      {
        throw new RCException (closure, RCErrors.Name, "Unable to resolve name " + right[0]);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left argument may only contain a single name");
      }
      RCValue result = right.Get (left[0]);
      if (result == null)
      {
        throw new RCException (closure, RCErrors.Name, "Unable to resolve name " + left[0]);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCBlock left, RCSymbol right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("right argument may only contain a single name");
      }
      object obj = right[0].Part (0);
      if (obj is long)
      {
        runner.Yield (closure, left.Get ((long) obj));
      }
      else if (obj is string)
      {
        runner.Yield (closure, left.Get ((string) obj));
      }
      else
      {
        throw new Exception ("invalid symbol: " + right[0].ToString ());
      }
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      EvalGet (runner, closure, right, left);
    }

    protected RCArray<T> DoRest<T> (RCArray<T> right)
    {
      RCArray<T> results = new RCArray<T> (right.Count);
      for (int i = 1; i < right.Count; ++i)
      {
        results.Write (right[i]);  
      }
      return results;
    }

    protected RCArray<T> DoPop<T> (RCArray<T> right)
    {
      RCArray<T> results = new RCArray<T> (right.Count);
      for (int i = 0; i < right.Count - 1; ++i)
      {
        results.Write (right[i]);
      }
      return results;
    }
  }
}

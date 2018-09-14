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
      runner.Yield (closure, right.Get (right.Count - 1));
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
  }
}

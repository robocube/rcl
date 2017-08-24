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
      runner.Yield (closure, left.Get (right[0]));
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left argument may only contain a single index");
      }
      runner.Yield (closure, right.Get (left[0]));
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("right argument may only contain a single name");
      }
      runner.Yield (closure, left.Get (right[0]));
    }

    [RCVerb ("get")]
    public void EvalGet (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left argument may only contain a single name");
      }
      runner.Yield (closure, right.Get (left[0]));
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

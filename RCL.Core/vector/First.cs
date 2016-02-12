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
        throw new Exception ("unwrap is for unwrapping a block containing a single element");
      }
      runner.Yield (closure, right.Get (0));
    }
  }
}
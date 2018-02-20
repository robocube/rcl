
using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Stack
  {
    [RCVerb ("stack")]
    public void EvalStack (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCBlock result = new RCBlock (null, "depth", ":", new RCLong (closure.Depth));
      result = new RCBlock (result, "fiber", ":", new RCLong (closure.Fiber));
      runner.Yield (closure, result);
    }

    [RCVerb ("show_stack")]
    public void EvalShowStack (RCRunner runner, RCClosure closure, RCBlock right)
    {
      string.Format ("depth: {0} fiber: {1}\n{2}",
        closure.Depth, closure.Fiber, closure.ToString ());
      runner.Log.Record (runner, closure, "stack", 0, "show", closure);
      runner.Yield (closure, new RCString (closure.ToString ()));
    }
  }
}

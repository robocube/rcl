
using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Stack
  {
    [RCVerb ("stack")]
    public void EvalStack (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCBlock result = new RCBlock (null, "depth", ":", new RCLong (closure.Depth));
      result = new RCBlock (result, "fiber", ":", new RCLong (closure.Fiber));
      runner.Yield (closure, result);
    }
  }
}

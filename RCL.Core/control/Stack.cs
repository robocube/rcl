
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
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

    [RCVerb ("stack_trace")]
    public void EvalStackTrace (RCRunner runner, RCClosure closure, RCBlock right)
    {
      bool firstOnTop = runner.Argv.OutputEnum != RCOutput.Systemd;
      StringBuilder builder = new StringBuilder ();
      closure.ToString (builder:builder, indent:0, firstOnTop:firstOnTop);
      string stack = builder.ToString ();
      runner.Log.Record (runner, closure, "stack", 0, "show", stack);
      runner.Yield (closure, new RCString (stack));
    }
  }
}

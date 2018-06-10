
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
      runner.Yield (closure, closure.Serialize ());
    }

    [RCVerb ("stack_trace")]
    public void EvalStackTrace (RCRunner runner, RCClosure closure, RCBlock right)
    {
      bool firstOnTop = RCSystem.Args.OutputEnum != RCOutput.Systemd;
      StringBuilder builder = new StringBuilder ();
      closure.ToString (builder:builder, indent:0, firstOnTop:firstOnTop);
      string stack = builder.ToString ();
      RCSystem.Log.Record (closure, "stack", 0, "show", stack);
      runner.Yield (closure, new RCString (stack));
    }
  }
}

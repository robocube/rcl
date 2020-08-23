
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Runner : RCOperator
  {
    [RCVerb ("bot")]
    public static void EvalBot (RCRunner runner, RCClosure closure, RCBlock right)
    {
      long bot = runner.Bot (closure, right);
      runner.Yield (closure, new RCLong (bot));
    }

    public override RCClosure Next (RCRunner runner,
                                    RCClosure tail,
                                    RCClosure previous,
                                    RCValue
                                    result)
    {
      bool done = Fiber.CheckFiberDone (previous);
      if (done) {
        return null;
      }
      return base.Next (runner, tail, previous, result);
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      if (arg == null) {
        return base.IsLastCall (closure, arg);
      }
      if (!base.IsLastCall (closure, arg)) {
        return false;
      }
      return arg.Code.IsBeforeLastCall (arg);
    }

    [RCVerb ("dump")]
    public static void EvalOperator (RCRunner runner, RCClosure closure, RCLong bot)
    {
      RCValue state = Dump (runner, closure, (int) bot[0]);
      runner.Yield (closure, state);
    }

    protected static RCValue Dump (RCRunner runner, RCClosure closure, int handle)
    {
      // This requires special access to runner.
      RCBot bot;
      lock (runner._botLock)
      {
        bot = runner._bots[handle];
      }
      Blackboard other = (Blackboard) bot.GetModule (typeof (Blackboard));
      return other.Dump ();
    }
  }
}

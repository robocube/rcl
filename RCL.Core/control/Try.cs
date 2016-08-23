
using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Try : RCOperator
  {
    [RCVerb ("try")]
    public void EvalTry (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      right.Eval (runner,
                  new RCClosure (closure, closure.Bot, right, closure.Left, RCBlock.Empty, 0));
    }

    public override RCClosure Handle (RCRunner runner, 
                                      RCClosure closure, 
                                      Exception exception, 
                                      long status, 
                                      out RCValue result)
    {
      //When the runner finds out about an exception it will try to call this
      //Handle method on all of the parents in the stack until it gets to
      //one that returns a closure to eval next.
      RCBlock wrapper = new RCBlock ("status", ":", new RCLong (status));
      RCException rcex = exception as RCException;
      string message;
      if (rcex != null)
      {
        if (runner.Argv.OutputEnum == RCOutput.Test) 
        {
          message = rcex.ToTestString ();
        } 
        else
        {
          message = rcex.ToString ();
        }
      }
      else
      {
        message = exception.Message;
      }
      //RCBlock report = new RCBlock ("", ":", new RCString (message));
      //wrapper = new RCBlock (wrapper, "data", ":", new RCTemplate (report, 1, true));
      wrapper = new RCBlock (wrapper, "data", ":", new RCString (message));
      result = wrapper;
      return base.Next (runner, closure, closure, result);
    }

    public override RCClosure Next (RCRunner runner, 
		                                RCClosure tail, 
		                                RCClosure previous, 
                                    RCValue result)
    {
      if (previous.Index == 1)
      {
        RCBlock wrapper = new RCBlock ("status", ":", new RCLong (0));
        wrapper = new RCBlock (wrapper, "data", ":", result);
        RCClosure next = base.Next (runner, tail, previous, wrapper);
        return next;
      }
      else return base.Next (runner, tail, previous, result);
    }

    public override RCValue Finish (RCValue result)
    {
      RCBlock wrapper = new RCBlock ("status", ":", new RCLong (0));
      wrapper = new RCBlock (wrapper, "data", ":", result);
      return wrapper;
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      if (arg == null)
      {
        return base.IsLastCall (closure, arg);
      }
      if (!base.IsLastCall (closure, arg))
      {
        return false;
      }
      return arg.Code.IsBeforeLastCall (arg);
    }
  }
}
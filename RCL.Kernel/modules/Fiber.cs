
using System;
using System.Threading;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Fiber : RCOperator
  {
    public class FiberState
    {
      public string State;
    }

    // For fiber module.
    public long _fiber = 0;
    public object _fiberLock = new object ();
    public Dictionary<long, FiberState> _fibers = new Dictionary<long, FiberState> ();
    public Dictionary<long, Queue<RCClosure>> _fiberWaiters =
      new Dictionary<long, Queue<RCClosure>> ();
    public Dictionary<long, RCValue> _fiberResults = new Dictionary<long, RCValue> ();

    [RCVerb ("fiber")]
    public void EvalFiber (RCRunner runner, RCClosure closure, RCBlock right)
    {
      long fiber = DoFiber (runner, closure, right);
      runner.Yield (closure, new RCLong (closure.Bot, fiber));
    }

    protected long DoFiber (RCRunner runner, RCClosure closure, RCValue code)
    {
      long fiber = Interlocked.Increment (ref _fiber);
      RCBot bot = runner.GetBot (closure.Bot);
      RCClosure next = FiberClosure (bot, fiber, closure, code);
      bot.ChangeFiberState (fiber, "start");
      RCSystem.Log.Record (closure, "fiber", fiber, "start", "");

      // This creates a separate stream of execution (fiber) from the
      // one that called this method.
      // When it is done it will naturally try to return its result
      // back to the original calling fiber.  But the Next method on the
      // fiber operator will see this happening and call FiberDone instead.
      runner.Continue (null, next);
      return fiber;
    }

    public static RCClosure FiberClosure (RCBot bot, long fiber, RCClosure closure, RCValue code)
    {
      // First create a clone of the parent closure for the current operator,
      // with the new fiber operator on it.  This ensures that that when tail
      // calls are eliminated the next closure will have the correct child fiber
      // number, not the fiber number of the parent.
      // Update July 12, 2014.
      // This additional clone of the parent closure no longer seems necessary for tail
      // calls.
      // In addition it was found to cause certain fibers to give the wrong result on
      // completion.
      // TestTryWaitKill5 is the repro for this.
      /*
         RCClosure parent = null;
         if (closure.Parent != null)
         {
          parent = new RCClosure (
            //bot,
            //fiber,
            closure.Parent.Bot,
            closure.Parent.Fiber,
            closure.Parent.Locks,
            closure.Parent.Parent,
            closure.Parent.Code,
            closure.Parent.Left,
            closure.Parent.Result,
            closure.Parent.Index);
         }
       */

      RCClosure clone = new RCClosure (bot.Id,
                                       fiber,
                                       closure.Locks,
                                       closure.Parent,
                                       closure.Code,
                                       closure.Left,
                                       closure.Result,
                                       closure.Index,
                                       closure.UserOp,
                                       closure.UserOpContext,
                                       noClimb: false,
                                       noResolve: false);

      RCClosure next = new RCClosure (clone, bot.Id, code, clone.Left, RCBlock.Empty, 0);
      return next;
    }

    [RCVerb ("wait")]
    public void EvalWait (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Wait (closure, right);
    }

    [RCVerb ("wait")]
    public void EvalWait (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Wait (closure, left, right);
    }

    [RCVerb ("done")]
    public void EvalDone (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Done (closure, right);
    }

    [RCVerb ("kill")]
    public void EvalKill (RCRunner runner, RCClosure closure, RCLong right)
    {
      try
      {
        runner.Kill (closure, right);
      }
      catch (Exception)
      {
        throw;
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("watch")]
    public void EvalWatch (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Watch (closure, right[0]);
    }

    public override RCClosure Next (RCRunner runner,
                                    RCClosure tail,
                                    RCClosure previous,
                                    RCValue
                                    result)
    {
      bool done = CheckFiberDone (previous);
      if (done) {
        return null;
      }
      return base.Next (runner, tail, previous, result);
    }

    public static bool CheckFiberDone (RCClosure previous)
    {
      bool done = false;
      if (previous.Parent != null) {
        if (previous.Parent.Parent != null &&
            (previous.Bot != previous.Parent.Parent.Bot ||
             previous.Fiber != previous.Parent.Parent.Fiber)) {
          done = true;
        }
        // Is it wise to rely on the fiber number here?
        // Yup, numbering fibers is going to be an important idea in robocube.
        else if (previous.Parent.Parent == null &&
                 (previous.Bot > 0 || previous.Fiber > 0)) {
          done = true;
        }
      }
      return done;
    }

    public bool IsFiberDone (long fiber)
    {
      lock (_fiberLock)
      {
        return _fiberResults.ContainsKey (fiber);
      }
    }

    public void ChangeFiberState (long handle, string state)
    {
      FiberState fiberState;
      lock (_fiberLock)
      {
        if (!_fibers.TryGetValue (handle, out fiberState)) {
          fiberState = new FiberState ();
          _fibers.Add (handle, fiberState);
        }
        fiberState.State = state;
      }
    }

    public void FiberDone (RCRunner runner, long bot, long fiber, RCValue result)
    {
      Queue<RCClosure> waiters;
      RCValue botResult = null;
      lock (_fiberLock)
      {
        if (_fiberWaiters.TryGetValue (fiber, out waiters)) {
          _fiberWaiters.Remove (fiber);
        }
        if (!_fiberResults.ContainsKey (fiber)) {
          _fiberResults.Add (fiber, result);
        }
        else {
          // I wanted to draw a hard line and ensure this method was
          // called only once for each fiber, by the fiber or bot operator.
          // But it has to also be called from within try when there is nothing
          // to do next.
          if (!_fiberResults[fiber].Equals (result)) {
            throw new Exception ("Conflicting results for fiber " + fiber);
          }
        }
        if (_fiberResults.Count == _fibers.Count) {
          botResult = _fiberResults[0L];
        }
      }
      if (waiters != null) {
        RCNative exValue = result as RCNative;
        if (exValue != null && exValue.Value is Exception) {
          foreach (RCClosure waiter in waiters)
          {
            runner.Finish (waiter, (Exception) exValue.Value, 1);
          }
        }
        else {
          foreach (RCClosure waiter in waiters)
          {
            runner.Yield (waiter, result);
          }
        }
      }
      if (botResult != null) {
        runner.BotDone (bot, botResult);
      }
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
  }
}


using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Sleep
  {
    [RCVerb ("sleep")]
    public void EvalSleep (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      SetTimer (runner, closure, right, (int) right[0]);
    }

    [RCVerb ("sleep")]
    public void EvalSleep (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      SetTimer (runner, closure, right, (int) right[0]);
    }

    [RCVerb ("sleep")]
    public void EvalSleep (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      SetTimer (runner, closure, right, (int) right[0]);
    }

    [RCVerb ("sleep")]
    public void EvalSleep (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      SetTimer (runner, closure, right, (int) right[0]);
    }

    protected virtual void SetTimer (
      RCRunner runner, RCClosure closure, RCValue right, int millis)
    {
      RCAsyncState state = new RCAsyncState (runner, closure, right);
      //Using the single argument ctor causes the the timer to be 
      //passed in the "state" parameter, and the state parameter
      //is always included in the gc handle table, so in this convoluted
      //way, the timer is protected from being gc'd.
      Timer timer = new Timer (new Wakeup (state).Continue);
      timer.Change (millis, Timeout.Infinite);
    }

    public class Wakeup
    {
      protected RCAsyncState m_state;
      public Wakeup (RCAsyncState state)
      {
        m_state = state;
      }

      public virtual void Continue (Object obj)
      {
        Timer timer = (Timer) obj;
        try
        {
          m_state.Runner.Yield (m_state.Closure, (RCValue) m_state.Other);
        }
        catch (Exception ex)
        {
          m_state.Runner.Report (m_state.Closure, ex);
        }
        finally
        {
          timer.Dispose ();
        }
      }
    }
  }
}

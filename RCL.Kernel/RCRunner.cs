using System;
using System.Threading;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCRunner
  {
    //I made these public for snap - but snap is not being used the way I originally thought
    public readonly object m_botLock = new object ();

    /// <summary>
    /// Maintains the existing bots for a running program. Synchronize using m_botLock
    /// </summary>
    public Dictionary<long, RCBot> m_bots = new Dictionary<long, RCBot> ();

    /// <summary>
    /// Closures which are waiting for a bot to finish executing
    /// </summary>
    protected Dictionary<long, Queue<RCClosure>> m_botWaiters = new Dictionary<long, Queue<RCClosure>> ();

    /// <summary>
    /// The series of closures being observed by watch. watch is used to build debugging tools for rcl
    /// </summary>
    protected Dictionary<long, Queue<RCAsyncState>> m_output = new Dictionary<long, Queue<RCAsyncState>> ();

    /// <summary>
    /// Closures which are evaluating the watch operator
    /// </summary>
    protected Dictionary<long, Queue<RCClosure>> m_watchers = new Dictionary<long, Queue<RCClosure>> ();

    /// <summary>
    /// The initial closure used to launch this runner and all of its bots and fibers
    /// </summary>
    protected RCClosure m_root = null;

    /// <summary>
    /// The handle for the next bot created within this runner
    /// </summary>
    protected long m_bot = 1;

    /// <summary>
    /// Counter tracking the number of times that the Reset method is invoked
    /// Note: This tracks calls to ResetCount in addition to Reset
    /// </summary>
    protected long m_reset = 0;

    /// <summary>
    /// Worker threads managed by this runner
    /// </summary>
    protected RCArray<Thread> m_workers = new RCArray<Thread> ();

    /// <summary>
    /// Exit status to be returned via Program.Main
    /// </summary>
    protected int m_exit = 0;

    /// <summary>
    /// The final result of evaluation
    /// </summary>
    protected volatile RCValue m_result = null;

    /// <summary>
    /// If evaluation failed, the exception describing the failure
    /// </summary>
    protected volatile Exception m_exception = null;

    /// <summary>
    /// True if an unhandled exception was rethrown by the runner
    /// </summary>
    protected volatile bool m_runnerUnhandled = false;

    /// <summary>
    /// The closure which threw m_exception
    /// </summary>
    protected volatile RCClosure m_exceptionClosure = null;

    /// <summary>
    /// Keeps a count of exceptions reported, mostly for unit testing purposes
    /// </summary>
    public volatile int m_exceptionCount = 0;

    /// <summary>
    /// The parser used for any parsing operations by this runner
    /// </summary>
    protected RCParser m_parser;

    /// <summary>
    /// Lock object for access to the queue of closures being worked
    /// </summary>
    protected readonly object m_queueLock = new object ();

    /// <summary>
    /// The queue of closures being worked
    /// </summary>
    protected Queue<RCClosure> m_queue = new Queue<RCClosure> ();

    /// <summary>
    /// Tracks closures which have completed initial execution but have not yet invoked Yield.
    /// The closure is working asyncly.
    /// </summary>
    protected Dictionary<long, Dictionary<long, RCClosure>> m_pending = new Dictionary<long, Dictionary<long, RCClosure>> ();

    /// <summary>
    /// Used by worker threads to wait when there are no more closures to execute
    /// </summary>
    protected AutoResetEvent m_wait = new AutoResetEvent (false);

    /// <summary>
    /// Used by worker threads
    /// </summary>
    protected AutoResetEvent m_done = new AutoResetEvent (false);

    /// <summary>
    /// The thread which invoked the runner constructor
    /// </summary>
    protected Thread m_ctorThread;

    public static RCRunner TestRunner ()
    {
      RCSystem.Reconfigure (new RCLArgv ("--output=test", "--show=print"));
      return new RCRunner (workers:1);
    }

    public RCRunner () : this (workers:1) {}
    public RCRunner (long workers)
    {
      m_ctorThread = Thread.CurrentThread;
      m_bots[0] = new RCBot (this, 0);
      m_output[0] = new Queue<RCAsyncState> ();
      m_parser = new RCLParser (RCSystem.Activator);
      Console.CancelKeyPress += HandleConsoleCancelKeyPress;
      for (int i = 0; i < workers; ++i)
      {
        Thread worker = new Thread (Work);
        worker.IsBackground = true;
        m_workers.Write (worker);
        worker.Start ();
      }
    }

    public void Start (RCValue program)
    {
      if (program == null)
      {
        throw new Exception ("program may not be null");
      }
      RCClosure root = null;
      lock (m_queueLock)
      {
        if (m_root != null)
        {
          throw new Exception ("Runner has already started.");
        }
        RCBot rootBot = m_bots[0];
        root = new RCClosure (rootBot.Id, program);
        rootBot.ChangeFiberState (root.Fiber, "start");
        RCSystem.Log.Record (root, "fiber", root.Fiber, "start", root.Code);
        m_root = root;
        m_queue.Enqueue (root);
      }
      m_wait.Set ();
    }

    public RCValue Run (RCValue program)
    {
      return Run (program, restoreStateOnError:false);
    }

    public RCValue Run (RCValue program, bool restoreStateOnError)
    {
      //Shouldn't this be an exception?
      if (program == null)
      {
        return null;
      }
      RCBlock wrapper = new RCBlock (RCBlock.Empty, "", "<-", program);
      RCClosure parent = new RCClosure (m_bots[0].Id, 0, null, null, wrapper, null, m_state, 0, null, null, noClimb:false);
      RCClosure closure = new RCClosure (parent, m_bots[0].Id, program, null, RCBlock.Empty, 0, null, null);
      RCValue result = Run (closure, restoreStateOnError);
      return result;
    }

    protected RCValue Run (RCClosure root, bool restoreStateOnError)
    {
      lock (m_queueLock)
      {
        if (m_root == null)
        {
          m_root = root;
          RCBot rootBot = GetBot (m_root.Bot);
          //keeping this inside the lock because it should happen before the call to Enqueue.
          //But only during the very first call to run for this runner.
          //Log.Record (this, root, root.BotId, "bot", root.BotId, "start", root.Code);
          rootBot.ChangeFiberState (root.Fiber, "start");
          RCSystem.Log.Record (root, "fiber", root.Fiber, "start", root.Code);
        }
        m_queue.Enqueue (root);
      }
      //Trigger a worker (don't care which) to take it.
      m_wait.Set ();
      //Wait for the work to be completed.
      m_done.WaitOne ();
      //If an exception was thrown, rethrow it on this thread.
      if (m_exception != null)
      {
        Exception exception = m_exception;
        m_exception = null;
        if (restoreStateOnError)
        {
          //Make the successfully computed values into the effective state of the environment
          RCClosure top = m_exceptionClosure;
          while (top.Parent != null && top.Parent.Parent != null)
          {
            top = top.Parent;
          }
          m_state = top.Result;
        }
        m_runnerUnhandled = true;
        throw exception;
      }
      //The final result is assigned by the worker in Finish ().
      RCValue result = m_result;
      m_result = null;
      return result;
    }

    public bool RunnerUnhandled
    {
      get { return m_runnerUnhandled; }
    }

    void HandleConsoleCancelKeyPress (object sender, ConsoleCancelEventArgs e)
    {
      //Console.Out.WriteLine ("Cancel Key Press");
      //Interupt ();
      //e.Cancel = true;
    }

    public void Interupt ()
    {
      //Kill all fibers for the 0 bot.
      this.Kill (0, -1, new Exception ("Interupt"), 1);
    }

    //Previous is the closure that will be removed from the pending set.
    //Next is the closure that will be added to the queue.
    //This is done in an atomic fashion so that all fibers will be
    //represented in m_pending or m_queue at all times.
    //previous will be null in cases where Continue is used to retry or fork streams of execution.
    //next will be null in cases where the executing fiber is finished and all
    //that remains is to remove it from m_pending.
    public void Continue (RCClosure previous, RCClosure next)
    {
      bool live = false;
      lock (m_queueLock)
      {
        if (previous != null)
        {
          Dictionary<long, RCClosure> pending = null;
          if (m_pending.TryGetValue (previous.Bot, out pending))
          {
            RCClosure c = null;
            if (pending.TryGetValue (previous.Fiber, out c))
            {
              pending.Remove (previous.Fiber);
              if (pending.Count == 0)
              {
                m_pending.Remove (previous.Bot);
              }
              live = true;
            }
          }
        }
        else
        {
          live = true;
        }
        if (live)
        {
          if (next != null)
          {
            m_queue.Enqueue (next);
            m_wait.Set ();
          }
        }
        else
        {
          //This will internally take the m_botLock.
          //This should be ok but given that it is just a log write I would like to move this outside.
          RCBot bot = GetBot (previous.Bot);
          bot.ChangeFiberState (previous.Fiber, "dead");
          RCSystem.Log.Record (previous, "fiber", previous.Fiber, "dead", "");
        }
      }
    }

    protected RCClosure Assign ()
    {
      lock (m_queueLock)
      {
        RCClosure next;
        if (m_queue.Count > 0)
        {
          next = m_queue.Dequeue ();
        }
        else
        {
          //We were signalled, but it was already gone.
          //Another worker took it out from under us.
          return null;
        }
        Dictionary<long, RCClosure> fibers = null;
        if (!m_pending.TryGetValue (next.Bot, out fibers))
        {
          fibers = new Dictionary<long, RCClosure> ();
          m_pending[next.Bot] = fibers;
        }
        fibers[next.Fiber] = next;
        return next;
      }
    }

    protected void Work ()
    {
      RCValue result = null;
      RCClosure next = null;
      RCClosure prev = null;

      while (true)
      {
        if (result == null)
        {
          m_wait.WaitOne ();
          prev = next;
          next = Assign ();
        }
        else break;

        while (next != null)
        {
          try
          {
            next.Code.Eval (this, next);
          }
          catch (Exception userex)
          {
            ++m_exceptionCount;
            try
            {
              Kill (next.Bot, next.Fiber, userex, 1);
            }
            catch (Exception sysex)
            {
              m_exception = sysex;
              m_exceptionClosure = next;
              ++m_exceptionCount;
              SafeLogRecord (next, "fiber", "killfail", sysex);
              m_done.Set ();
            }
          }
          prev = next;
          next = Assign ();
        }
      }
      Finish (prev, result);
    }

    protected void SafeLogRecord (RCClosure closure, string module, string state, Exception ex)
    {
      try
      {
        RCSystem.Log.Record (closure, module, closure.Fiber, state, ex);
      }
      catch (Exception innerEx)
      {
        RCSystem.Log.Record (closure, module, closure.Fiber, state,
                             "An exception occured while reporting an exception: " + innerEx.ToString ());
      }
    }

    public void Dispose ()
    {
      lock (m_botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in m_bots)
        {
          try
          {
            kv.Value.Dispose ();
          }
          catch (Exception ex)
          {
            Report (ex);
          }
        }
      }
    }

    public void Abort (int status)
    {
      lock (m_botLock)
      {
        m_exit = status;
      }
      m_ctorThread.Abort ();
    }

    public int ExitStatus ()
    {
      lock (m_botLock)
      {
        if (m_exceptionCount > 0)
        {
          return 2;
        }
        else
        {
          return m_exit;
        }
      }
    }

    public RCValue Read (string code)
    {
      bool fragment;
      return m_parser.Parse (m_parser.Lex (code), out fragment, canonical:false);
    }

    public RCValue Read (string code, out bool fragment, bool canonical)
    {
      return m_parser.Parse (m_parser.Lex (code), out fragment, canonical);
    }

    public RCArray<RCToken> Lex (string code)
    {
      return m_parser.Lex (code);
    }

    public void Reset ()
    {
      //Should I take the locks here? One or both? Both.
      lock (m_queueLock)
      {
        lock (m_botLock)
        {
          m_parser = new RCLParser (RCSystem.Activator);
          m_root = null;
          m_result = null;
          m_exception = null;
          m_exceptionClosure = null;
          m_exceptionCount = 0;
          m_queue = new Queue<RCClosure> ();
          m_pending = new Dictionary<long, Dictionary<long, RCClosure>> ();
          m_bot = 1;
          m_bots = new Dictionary<long, RCBot> ();
          m_bots[0] = new RCBot (this, 0);
          m_output[0] = new Queue<RCAsyncState> ();
          ++m_reset;
        }
      }
    }

    public void ResetCount (long botHandle)
    {
      //Should I take the locks here? One or both? Both.
      lock (m_queueLock)
      {
        lock (m_botLock)
        {
          RCBot bot;
          if (!m_bots.TryGetValue (botHandle, out bot))
          {
            throw new Exception (string.Format ("Invalid bot: {0}", botHandle));
          }
          //bot.Reset ();
          m_exceptionCount = 0;
          ++m_reset;
        }
      }
    }

    public RCValue Rep (RCValue program, bool restoreStateOnError)
    {
      RCValue result = Run (program, restoreStateOnError);
      RCBlock state = result as RCBlock;
      if (state != null)
      {
        m_state = state;
      }
      return result;
    }

    public RCValue RepAction (string action)
    {
      if (m_state.Get (action) == null)
      {
        throw new ArgumentException (string.Format ("Unknown action name: {0}", action));
      }
      return Rep (string.Format ("{0} {{}}", action));
    }

    protected RCBlock m_state = RCBlock.Empty;
    public RCValue Rep (string code)
    {
      bool fragment = false;
      RCValue peek = RCSystem.Parse (code, out fragment);
      if (peek == null)
      {
        return null;
      }
      RCBlock variable = peek as RCBlock;
      if (variable != null && fragment)
      {
        if (variable.Count == 0)
        {
          return null;
        }
        if (variable.Value.ArgumentEval)
        {
          RCBlock program = new RCBlock (m_state, "", "<-", variable.Value);
          RCClosure parent = new RCClosure (m_bots[0].Id, 0, null, null, program, null,
                                            m_state, m_state.Count, null, null, noClimb:false);
          RCClosure child = new RCClosure (parent, m_bots[0].Id, variable.Value, null,
                                           RCBlock.Empty, 0, null, null);
          RCValue result = Run (child, restoreStateOnError:false);
          m_state = new RCBlock (m_state, variable.Name, ":", result);
        }
        else
        {
          //What about fragments with multiple parts.
          m_state = new RCBlock (m_state, variable.Name, ":", variable.Value);
        }
        return null;
      }
      else
      {
        RCBlock program = new RCBlock (m_state, "", "<-", peek);
        RCClosure parent = new RCClosure (m_bots[0].Id, 0, null, null, program, null,
                                          m_state, m_state.Count, null, null, noClimb:false);
        RCClosure child = new RCClosure (parent, m_bots[0].Id, peek, null, RCBlock.Empty, 0);
        RCValue result = Run (child, restoreStateOnError:false);
        return result;
      }
    }
   
    public string RepString (string code)
    {
      RCValue result = Rep (code);
      if (result == null)
      {
        return "";
      }
      return result.Format (RCFormat.DefaultNoT);
    }

    public void Finish (RCClosure closure, RCValue result)
    {
      Fiber fiber;
      lock (m_botLock)
      {
        fiber = (Fiber) m_bots[0].GetModule (typeof (Fiber));
      }
      lock (fiber.m_fiberLock)
      {
        RCValue finalResult;
        fiber.m_fiberResults.TryGetValue (0, out finalResult);
        m_result = finalResult;
      }
      if (m_result == null)
      {
        m_result = result;
      }
      m_done.Set ();
    }

    public void Finish (RCClosure closure, Exception exception, long status)
    {
      RCValue result = null;
      RCClosure parent = closure;
      RCBot bot = GetBot (closure.Bot);
      while (parent != null && parent.Bot == closure.Bot && parent.Fiber == closure.Fiber)
      {
        RCClosure next = parent.Code.Handle (this, parent, exception, status, out result);
        if (result != null && next == null)
        {
          string state = status == 1 ? "failed" : "killed";
          bot.ChangeFiberState (closure.Fiber, state);
          RCSystem.Log.Record (closure, "fiber", closure.Fiber, state, exception);
          if (closure.Fiber == 0 && closure.Bot == 0)
          {
            Finish (closure, result);
          }
          else
          {
            bot.FiberDone (this, closure.Bot, closure.Fiber, result);
          }
          return;
        }
        else
        {
          Continue (null, next);
        }
        if (result != null)
        {
          bot.ChangeFiberState (closure.Fiber, "caught");
          RCSystem.Log.Record (closure, "fiber", closure.Fiber, "caught", exception);
          ++m_exceptionCount;
          return;
        }
        parent = parent.Parent;
      }
      //This means it was not handled in the while loop.
      if (result == null)
      {
        string state = status == 1 ? "failed" : "killed";
        bot.ChangeFiberState (closure.Fiber, state);
        SafeLogRecord (closure, "fiber", state, exception);
        ++m_exceptionCount;
        if (closure.Fiber == 0 && closure.Bot == 0)
        {
          m_exception = exception;
          m_exceptionClosure = closure;
          m_done.Set ();
        }
        else
        {
          //I think this is sort of mostly the correct think to do.
          //We need to record the fact that the fiber finished.
          //But stuffing an exception inside a Native to do so seems wrong.
          //Need more work on controlling the lifecycle of fibers.
          //Also I want to get rid of RCNative I think this is the only place
          //where I still need it.
          bot.FiberDone (this, closure.Bot, closure.Fiber, new RCNative (exception));
        }
      }
    }

    public void Yield (RCClosure closure, RCValue result)
    {
      RCL.Kernel.Eval.DoYield (this, closure, result);
    }

    public void YieldCanonical (RCClosure closure, RCValue result)
    {
      RCL.Kernel.Eval.DoYield (this, closure, result, canonical:true);
    }

    public void Output (RCClosure closure, RCSymbolScalar name, RCValue val)
    {
      lock (m_botLock)
      {
        Queue<RCAsyncState> output = m_output[closure.Bot];
        output.Enqueue (new RCAsyncState (this, closure, val));
        Queue<RCClosure> watchers;
        if (m_watchers.TryGetValue (closure.Bot, out watchers))
        {
          while (watchers.Count > 0)
          {
            RCClosure observer = watchers.Dequeue ();
            Continue (null, observer);
          }
        }
        else
        {
          while (output.Count > 500)
          {
            output.Dequeue ();
          }
        }
      }
    }

    //It is possible to have multiple concurrent observers.
    //However each value will only be returned to one of the observers.
    //It is hard to see a reason to create multiple observers but there
    //are multiple potential problems with implementing the constraint that there
    //be only one.
    public void Watch (RCClosure closure, long bot)
    {
      RCBlock result;
      lock (m_botLock)
      {
        Queue<RCAsyncState> output = m_output[bot];
        if (output.Count == 0)
        {
          Queue<RCClosure> watchers;
          if (!m_watchers.TryGetValue (bot, out watchers))
          {
            watchers = new Queue<RCClosure> ();
            m_watchers[bot] = watchers;
          }
          watchers.Enqueue (closure);
          return;
        }
        RCBlock values = RCBlock.Empty;
        Stack<RCClosure> parts = new Stack<RCClosure> ();
        RCArray<RCSymbolScalar> names = new RCArray<RCSymbolScalar> (output.Count);
        while (output.Count > 0)
        {
          RCAsyncState state = output.Dequeue ();
          RCClosure parent = state.Closure;
          RCSymbolScalar name = new RCSymbolScalar (null, parent.Fiber);
          while (parent != null)
          {
            if (parent.Parent.Bot != parent.Bot ||
                parent.Parent.Fiber != parent.Fiber)
            {
              break;
            }
            parts.Push (parent);
            parent = parent.Parent;
          }
          while (parts.Count > 0)
          {
            RCClosure top = parts.Pop ();
            if (top.Code.IsBlock)
            {
              RCBlock code = (RCBlock) top.Code;
              string part = code.GetName (top.Index).Name;
              if (part != "")
              {
                name = new RCSymbolScalar (name, part);
              }
              else
              {
                name = new RCSymbolScalar (name, (long) top.Index);
              }
            }
          }
          if (name != null)
          {
            RCValue val = (RCValue) state.Other;
            values = new RCBlock (values, "", ":", val);
            names.Write (name);
          }
        }
        result = new RCBlock (null, "names", ":", new RCSymbol (names));
        result = new RCBlock (result, "values", ":", values);
      }
      Yield (closure, result);
    }

    public long Bot (RCClosure closure, RCBlock right)
    {
      RCClosure next;
      long id;
      RCBot bot;
      lock (m_botLock)
      {
        id = m_bot++;
        bot = new RCBot (this, id);
        m_output[id] = new Queue<RCAsyncState> ();
        m_bots[id] = bot;
        next = Fiber.FiberClosure (bot, 0, closure, right);
      }
      bot.ChangeFiberState (0, "start");
      RCSystem.Log.Record (next, "fiber", 0, "start", right);
      Continue (null, next);
      return id;
    }

    public RCBot GetBot (long id)
    {
      RCBot result;
      lock (m_botLock)
      {
        if (!m_bots.TryGetValue (id, out result))
        {
          throw new Exception ("Unknown bot id: " + id);
        }
      }
      return result;
    }

    public void Done (RCClosure closure, RCLong fibers)
    {
      if (fibers.Count == 1)
      {
        Fiber fiber;
        lock (m_botLock)
        {
          fiber = (Fiber) m_bots[fibers[0]].GetModule (typeof (Fiber));
        }
        bool val;
        lock (fiber.m_fiberLock)
        {
          val = fiber.m_fibers.Count == fiber.m_fiberResults.Count;
        }
        Yield (closure, new RCBoolean (val));
      }
      else if (fibers.Count == 2)
      {
        bool val;
        Fiber fiber;
        lock (m_botLock)
        {
          fiber = (Fiber) m_bots[fibers[0]].GetModule (typeof (Fiber));
        }
        lock (fiber.m_fiberLock)
        {
          val = fiber.m_fiberResults.ContainsKey (fibers[1]);
        }
        Yield (closure, new RCBoolean (val));
      }
      else
      {
        throw new Exception ();
      }
    }

    public void Wait (RCClosure closure, RCLong fibers)
    {
      //At some point I want this to work for multiple fibers,
      //but the current version will only wait on a single fiber.
      if (fibers.Count == 1)
      {
        Fiber fiber;
        lock (m_botLock)
        {
          fiber = (Fiber) m_bots[fibers[0]].GetModule (typeof (Fiber));
        }
        RCValue result = null;
        lock (fiber.m_fiberLock)
        {
          if (fiber.m_fiberResults.Count == fiber.m_fibers.Count)
          {
            fiber.m_fiberResults.TryGetValue (0, out result);
          }
        }
        if (result == null)
        {
          lock (m_botLock)
          {
            Queue<RCClosure> waiters;
            if (m_botWaiters.TryGetValue (fibers[0], out waiters))
            {
              waiters.Enqueue (closure);
            }
            else
            {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              m_botWaiters.Add (fibers[0], waiters);
            }
          }
        }
        else
        {
          Yield (closure, result);
        }
      }
      else if (fibers.Count == 2)
      {
        RCValue result = null;
        Fiber fiber;
        lock (m_botLock)
        {
          fiber = (Fiber) m_bots[fibers[0]].GetModule (typeof (Fiber));
        }
        lock (fiber.m_fiberLock)
        {
          if (!fiber.m_fiberResults.TryGetValue (fibers[1], out result))
          {
            Queue<RCClosure> waiters;
            if (fiber.m_fiberWaiters.TryGetValue (fibers[1], out waiters))
            {
              waiters.Enqueue (closure);
            }
            else
            {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              fiber.m_fiberWaiters.Add (fibers[1], waiters);
            }
          }
        }
        if (result != null)
        {
          Yield (closure, result);
        }
      }
      else
      {
        throw new Exception ();
      }
    }

    protected class Wakeup
    {
      protected readonly RCAsyncState m_state;
      protected readonly long m_resetCount;

      public Wakeup (RCAsyncState state, long resetCount)
      {
        m_state = state;
        m_resetCount = resetCount;
      }

      public virtual void ContinueBot (Object obj)
      {
        Timer timer = (Timer) obj;
        RCLong fibers = (RCLong) m_state.Other;
        try
        {
          lock (m_state.Runner.m_botLock)
          {
            if (m_state.Runner.m_reset != m_resetCount) return;
            Queue<RCClosure> waiters;
            //Since the bot results are not stored anywhere, we time out if there are waiters
            //But what if there are multiple waiters? This seems like an issue.
            if (m_state.Runner.m_botWaiters.TryGetValue (fibers[0], out waiters))
            {
              Exception ex = new Exception ("Timed out waiting for bot " + fibers[0]);
              m_state.Runner.Kill (fibers[0], -1, ex, 2);
            }
          }
        }
        catch (ThreadAbortException)
        {
          //This often happens as the runtime is shutting down,
          //because this code runs on a thread pool thread.
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

      public virtual void ContinueFiber (Object obj)
      {
        Timer timer = (Timer) obj;
        RCLong fibers = (RCLong) m_state.Other;
        try
        {
          Fiber fiber;
          lock (m_state.Runner.m_botLock)
          {
            if (m_state.Runner.m_reset != m_resetCount)
            {
              return;
            }
            fiber = (Fiber) m_state.Runner.m_bots[fibers[0]].GetModule (typeof (Fiber));
          }
          lock (fiber.m_fiberLock)
          {
            RCValue result = null;
            if (!fiber.m_fiberResults.TryGetValue (fibers[1], out result))
            {
              Exception ex = new Exception ("Timed out waiting for fiber " + fibers[1]);
              m_state.Runner.Kill (fibers[0], fibers[1], ex, 2);
            }
          }
        }
        catch (ThreadAbortException)
        {
          //This often happens as the runtime is shutting down,
          //because this code runs on a thread pool thread.
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

    public void Wait (RCClosure closure, RCLong timeout, RCLong fibers)
    {
      //At some point I want this to work for multiple fibers,
      //but the current version will only wait on a single fiber.
      if (fibers.Count == 1)
      {
        Fiber fiber;
        lock (m_botLock)
        {
          fiber = (Fiber) m_bots[fibers[0]].GetModule (typeof (Fiber));
        }
        RCValue result = null;
        lock (fiber.m_fiberLock)
        {
          if (fiber.m_fiberResults.Count == fiber.m_fibers.Count)
          {
            fiber.m_fiberResults.TryGetValue (0, out result);
          }
        }
        if (result == null)
        {
          lock (m_botLock)
          {
            Queue<RCClosure> waiters;
            if (m_botWaiters.TryGetValue (fibers[0], out waiters))
            {
              waiters.Enqueue (closure);
            }
            else
            {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              m_botWaiters.Add (fibers[0], waiters);
            }
            RCAsyncState state = new RCAsyncState (this, closure, fibers);
            Wakeup wakeup = new Wakeup (state, m_reset);
            Timer timer = new Timer (wakeup.ContinueBot);
            timer.Change (timeout[0], Timeout.Infinite);
          }
        }
        else
        {
          Yield (closure, result);
        }
      }
      else if (fibers.Count == 2)
      {
        RCValue result = null;
        Fiber fiber;
        lock (m_botLock)
        {
          fiber = (Fiber) m_bots[fibers[0]].GetModule (typeof (Fiber));
        }
        lock (fiber.m_fiberLock)
        {
          if (!fiber.m_fiberResults.TryGetValue (fibers[1], out result))
          {
            Queue<RCClosure> waiters;
            if (fiber.m_fiberWaiters.TryGetValue (fibers[1], out waiters))
            {
              waiters.Enqueue (closure);
            }
            else
            {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              fiber.m_fiberWaiters.Add (fibers[1], waiters);
            }
            RCAsyncState state = new RCAsyncState (this, closure, fibers);
            Wakeup wakeup = new Wakeup (state, m_reset);
            Timer timer = new Timer (wakeup.ContinueFiber);
            timer.Change (timeout[0], Timeout.Infinite);
          }
        }
        if (result != null)
        {
          Yield (closure, result);
        }
      }
      else
      {
        throw new Exception ();
      }
    }

    public void BotDone (long bot, RCValue result)
    {
      Queue<RCClosure> waiters = null;
      lock (m_botLock)
      {
        if (m_botWaiters.TryGetValue (bot, out waiters))
        {
          m_botWaiters.Remove (bot);
        }
        m_bots[bot].Dispose ();
      }
      if (waiters != null)
      {
        foreach (RCClosure waiter in waiters)
        {
          Yield (waiter, result);
        }
      }
    }

    public void Kill (RCClosure closure, RCLong fibers)
    {
      if (fibers.Count == 1)
      {
        Kill (fibers[0], -1, new Exception ("fiber killed"), 2);
      }
      else if (fibers.Count == 2)
      {
        Kill (fibers[0], fibers[1], new Exception ("fiber killed"), 2);
      }
      else
      {
        throw new Exception ();
      }
    }

    public void Kill (long bot, long fiber, Exception ex, long status)
    {
      if (fiber < 0)
      {
        lock (m_queueLock)
        {
          Queue<RCClosure> queue = new Queue<RCClosure> ();
          HashSet<long> killed = new HashSet<long> ();
          while (m_queue.Count > 0)
          {
            RCClosure queued = m_queue.Dequeue ();
            if (queued.Bot == bot)
            {
              if (!killed.Contains (queued.Fiber))
              {
                killed.Add (queued.Fiber);
                Finish (queued, ex, status);
              }
            }
            else
            {
              queue.Enqueue (queued);
            }
          }
          m_queue = queue;
          Dictionary<long, RCClosure> pending;
          if (m_pending.TryGetValue (bot, out pending))
          {
            RCClosure[] closures = new RCClosure[pending.Count];
            pending.Values.CopyTo (closures, 0);
            for (int i = 0; i < closures.Length; ++i)
            {
              if (!killed.Contains (closures[i].Fiber))
              {
                killed.Add (closures[i].Fiber);
                Finish (closures[i], ex, status);
              }
            }
            m_pending.Remove (bot);
          }
        }
      }
      else
      {
        //Kill only the fiber on the designated bot.
        //Do we want to allow multiple bot fiber pairs? Yes! No.
        //No. The final answer is no.
        lock (m_queueLock)
        {
          Queue<RCClosure> queue = new Queue<RCClosure> ();
          HashSet<long> killed = new HashSet<long> ();
          while (m_queue.Count > 0)
          {
            RCClosure queued = m_queue.Dequeue ();
            if (queued.Bot == bot)
            {
              if (queued.Fiber == fiber &&
                  !killed.Contains (queued.Fiber))
              {
                killed.Add (queued.Fiber);
                Finish (queued, ex, status);
              }
              else
              {
                queue.Enqueue (queued);
              }
            }
            else
            {
              queue.Enqueue (queued);
            }
          }
          m_queue = queue;
          Dictionary<long, RCClosure> pending;
          if (m_pending.TryGetValue (bot, out pending))
          {
            RCClosure target;
            if (pending.TryGetValue (fiber, out target))
            {
              pending.Remove (fiber);
              if (!killed.Contains (fiber))
              {
                killed.Add (fiber);
                Finish (target, ex, status);
              }
            }
            if (pending.Count == 0)
            {
              m_pending.Remove (bot);
            }
          }
        }
      }
    }

    public void Report (RCClosure closure, Exception ex)
    {
      RCBot bot = GetBot (closure.Bot);
      bot.ChangeFiberState (closure.Fiber, "reported");
      RCSystem.Log.Record (closure, "fiber", closure.Fiber, "reported", ex);
      ++m_exceptionCount;
    }

    public void Report (Exception ex)
    {
      this.Report (ex, "reported");
    }

    public void Report (Exception ex, string status)
    {
      RCSystem.Log.Record (0, 0, "fiber", 0, status, ex);
      ++m_exceptionCount;
    }

    public RCOperator New (string op, RCValue right)
    {
      if (right == null)
      {
        throw new ArgumentNullException ("R");
      }
      return RCSystem.Activator.New (op, right);
    }

    public RCOperator New (string op, RCValue left, RCValue right)
    {
      if (right == null)
      {
        throw new ArgumentNullException ("R");
      }
      return RCSystem.Activator.New (op, left, right);
    }

    public int ExceptionCount
    {
      get { return m_exceptionCount; }
    }
  }
}

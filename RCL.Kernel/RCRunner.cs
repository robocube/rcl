
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCRunner
  {
    //I made this public because parse wants it.
    //It's fine because there are no mutating methods on RCActivator.
    //But it's possible that could change at some point.

    public readonly RCBlock Arguments;
    public readonly RCActivator Activator;
    public readonly RCLog Log;

    //I made these public for snap.
    public readonly object m_botLock = new object ();
    public Dictionary<long, RCBot> m_bots = new Dictionary<long, RCBot> ();
    protected Dictionary<long, Queue<RCClosure>> m_botWaiters = new Dictionary<long, Queue<RCClosure>> ();
    protected Dictionary<long, Queue<RCAsyncState>> m_output = new Dictionary<long, Queue<RCAsyncState>> ();
    protected Dictionary<long, Queue<RCClosure>> m_watchers = new Dictionary<long, Queue<RCClosure>> ();

    protected RCClosure m_root = null;
    protected long m_bot = 1;
    protected RCArray<Thread> m_workers = new RCArray<Thread> ();
    protected int m_exit = -1;
    protected volatile RCValue m_result = null;
    protected volatile Exception m_exception = null;
    protected RCParser m_parser;

    protected readonly object m_queueLock = new object ();
    protected Queue<RCClosure> m_queue = new Queue<RCClosure> ();
    protected Dictionary<long, Dictionary<long, RCClosure>> m_pending = new Dictionary<long, Dictionary<long, RCClosure>> ();

    protected AutoResetEvent m_wait = new AutoResetEvent (false);
    protected AutoResetEvent m_done = new AutoResetEvent (false);
    protected Thread m_ctorThread;

    public RCRunner () : this (RCActivator.Default, new RCLog (), 1, CreateArgs ()) {}

    public RCRunner (RCActivator activator, RCLog log, long workers, RCBlock arguments)
    {
      Arguments = arguments;
      Activator = activator;
      Log = log;
      m_ctorThread = Thread.CurrentThread;
      m_bots[0] = new RCBot (this, 0);
      m_output[0] = new Queue<RCAsyncState> ();
      m_parser = new RCLParser (Activator);
      Console.CancelKeyPress += HandleConsoleCancelKeyPress;
      for (int i = 0; i < workers; ++i)
      {
        Thread worker = new Thread (Work);
        worker.IsBackground = true;
        m_workers.Write (worker);
        worker.Start ();
      }
    }

    public static RCBlock CreateArgs (params string[] argv)
    {
      RCBlock args = RCBlock.Empty;
      for (int i = 0; i < argv.Length; ++i)
      {
        string[] kv = argv[i].Split (':');
        if (kv.Length == 1)
        {
          args = new RCBlock (args, kv[0], ":", RCBoolean.True);
        }
        else
        {
          args = new RCBlock (args, kv[0], ":", new RCString (kv[1]));
        }
      }

      RCString program = (RCString) args.Get ("program", null);
      if (program == null)
      {
        program = new RCString ("");
        args = new RCBlock (args, "program", ":", program);
      }
      RCString action = (RCString) args.Get ("action", null);
      if (action == null)
      {
        action = new RCString ("");
        args = new RCBlock (args, "action", ":", action);
      }
      RCBoolean console = (RCBoolean) args.Get ("console", null);
      if (console == null)
      {
        console = RCBoolean.True;
        args = new RCBlock (args, "console", ":", console);
      }
      //RCBoolean blackboard = (RCBoolean) args.Get ("blackboard", null);
      //if (blackboard == null)
      //{
      //  blackboard = RCBoolean.True;
      //  args = new RCBlock (args, "blackboard", ":", blackboard);
      //}
      RCString output = (RCString) args.Get ("output", null);
      if (output == null)
      {
        output = new RCString ("full");
        args = new RCBlock (args, "output", ":", output);
      }
      return args;
    }

    public void Start (RCValue program)
    {
      if (program == null)
        throw new Exception ("program may not be null");

      RCClosure root = null;
      lock (m_queueLock)
      {
        if (m_root != null)
          throw new Exception ("Runner has already started.");

        root = new RCClosure (m_bots[0], program);
        root.Bot.ChangeFiberState (root.Fiber, "start");
        Log.RecordDoc (this, root, "fiber", root.Fiber, "start", root.Code);
        m_root = root;
        m_queue.Enqueue (root);
      }
      m_wait.Set ();
    }

    public RCValue Run (RCValue program)
    {
      //Shouldn't this be an exception?
      if (program == null)
        return null;

      RCBlock wrapper = new RCBlock (RCBlock.Empty, "", "<-", program);
      RCClosure parent = new RCClosure (
        m_bots[0], 0, null, null, wrapper, null, m_state, 0);
      RCClosure closure = new RCClosure (
        parent, m_bots[0], program, null, RCBlock.Empty, 0);
      
      //RCClosure closure = new RCClosure (m_bots[0], program);
      RCValue result = Run (closure);
      return result;
    }

    protected RCValue Run (RCClosure root)
    {
      lock (m_queueLock)
      {
        if (m_root == null)
        {
          m_root = root;
          //keeping this inside the lock because it should happen before the call to Enqueue.
          //But only during the very first call to run for this runner.
          //Log.Record (this, root, root.Bot.Id, "bot", root.Bot.Id, "start", root.Code);
          root.Bot.ChangeFiberState (root.Fiber, "start");
          Log.RecordDoc (this, root, "fiber", root.Fiber, "start", root.Code);
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
        throw exception;
      }
      //The final result is assigned by the worker in Finish().
      RCValue result = m_result;
      m_result = null;
      return result;
    }

    void HandleConsoleCancelKeyPress (object sender, ConsoleCancelEventArgs e)
    {
      //Kill all fibers for the 0 bot.
      this.Kill (0, -1, new Exception ("Interupt"), 1);
      e.Cancel = true;
    }

    //Previous is the closure that will be removed from the pending set.
    //Next is the closure that will be added to the queue.
    //This is done in an atomic fashion so that all fibers will be
    //represented in m_pending or m_queue at all times.
    //previous will be null in cases where Continue is used to retry or fork streams of execution.
    //next will be null in cases where the executing fiber is finished and all that remains is to remove it from m_pending.
    public void Continue (RCClosure previous, RCClosure next)
    {
      bool live = false;
      lock (m_queueLock)
      {
        if (previous != null)
        {
          Dictionary<long, RCClosure> pending = null;
          if (m_pending.TryGetValue (previous.Bot.Id, out pending))
          {
            RCClosure c = null;
            if (pending.TryGetValue (previous.Fiber, out c))
            {
              pending.Remove (previous.Fiber);
              if (pending.Count == 0)
              {
                m_pending.Remove (previous.Bot.Id);
              }
              live = true;
            }
          }
        }
        else live = true;

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
          previous.Bot.ChangeFiberState (previous.Fiber, "dead");
          Log.Record (this, previous, "fiber", previous.Fiber, "dead", "");
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
        if (!m_pending.TryGetValue (next.Bot.Id, out fibers))
        {
          fibers = new Dictionary<long, RCClosure> ();
          m_pending[next.Bot.Id] = fibers;
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
            try
            {
              Console.Out.Write (next);
              Kill (next.Bot.Id, next.Fiber, userex, 1);
            }
            catch (Exception sysex)
            {
              m_exception = sysex;
              m_done.Set ();
            }
          }
          prev = next;
          next = Assign ();
        }
      }
      Finish (prev, result);
    }

    public void Exit (int status)
    {
      lock (m_botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in m_bots)
        {
          kv.Value.Dispose ();
        }
        m_exit = status;
      }
      m_ctorThread.Abort ();
    }

    public int ExitStatus ()
    {
      lock (m_botLock)
      {
        return m_exit;
      }
    }

    public RCValue Peek (string code, out bool fragment)
    {
      RCParser parser = new RCLParser (Activator);
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      parser.Lex (code, tokens);
      RCValue result = parser.Parse (tokens, out fragment);
      return result;
    }

    public RCValue Read (string code)
    {
      bool fragment;
      return m_parser.Parse (m_parser.Lex (code), out fragment);
    }

    public RCValue Read (string code, out bool fragment)
    {
      return m_parser.Parse (m_parser.Lex (code), out fragment);
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
          m_parser = new RCLParser (Activator);
          m_root = null;
          m_result = null;
          m_exception = null;
          m_queue = new Queue<RCClosure> ();
          m_pending = new Dictionary<long, Dictionary<long, RCClosure>> ();
          m_bot = 1;
          m_bots = new Dictionary<long, RCBot> ();
          m_bots[0] = new RCBot (this, 0);
          m_output[0] = new Queue<RCAsyncState> ();
        }
      }
    }

    public RCValue Rep (RCValue program)
    {
      
      //RCValue result = Run (child);

      //RCClosure closure = new RCClosure (m_bots[0], program);
      RCValue result = Run (program);
      RCBlock state = result as RCBlock;
      if (state != null)
      {
        m_state = state;
      }
      return result;
    }

    protected RCBlock m_state = RCBlock.Empty;
    public RCValue Rep (string code)
    {
      bool fragment = false;
      RCValue peek = Peek (code, out fragment);
      if (peek == null)
      {
        return null;
      }

      RCBlock variable = peek as RCBlock;
      if (variable != null && fragment)
      {
        if (variable.Value.ArgumentEval)
        {
          RCBlock program = new RCBlock (m_state, "", "<-", variable.Value);
          RCClosure parent = new RCClosure (
            m_bots[0], 0, null, null, program, null, m_state, m_state.Count);
          RCClosure child = new RCClosure (
            parent, m_bots[0], variable.Value, null, RCBlock.Empty, 0);
          RCValue result = Run (child);
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
        RCClosure parent = new RCClosure (
          m_bots[0], 0, null, null, program, null, m_state, m_state.Count);
        RCClosure child = new RCClosure (
          parent, m_bots[0], peek, null, RCBlock.Empty, 0);
        RCValue result = Run (child);
        return result;
      }
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
        fiber.m_fiberResults.TryGetValue (0, out m_result);
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
      while (parent != null && parent.Bot.Id == closure.Bot.Id && parent.Fiber == closure.Fiber)
      {
        RCClosure next = parent.Code.Handle (this, parent, exception, status, out result);
        if (result != null && next == null)
        {
          string state = status == 1 ? "failed" : "killed";
          closure.Bot.ChangeFiberState (closure.Fiber, state);
          Log.Record (this, closure, "fiber", closure.Fiber, state, exception);
          if (closure.Fiber == 0 && closure.Bot.Id == 0)
          {
            Finish (closure, result);
          }
          else
          {
            closure.Bot.FiberDone (this, closure.Bot.Id, closure.Fiber, result);
          }
          return;
        }
        else
        {
          Continue (null, next);
        }
        if (result != null)
        {
          closure.Bot.ChangeFiberState (closure.Fiber, "caught");
          Log.Record (this, closure, "fiber", closure.Fiber, "caught", exception);
          return;
        }
        parent = parent.Parent;
      }
      //This means it was not handled in the while loop.
      if (result == null)
      {
        string state = status == 1 ? "failed" : "killed";
        closure.Bot.ChangeFiberState (closure.Fiber, state);
        Log.Record (this, closure, "fiber", closure.Fiber, state, exception);
        if (closure.Fiber == 0 && closure.Bot.Id == 0)
        {
          m_exception = exception;
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
          closure.Bot.FiberDone (this, closure.Bot.Id, closure.Fiber, new RCNative (exception));
        }
      }
    }

    public void Yield (RCClosure closure, RCValue result)
    {
      RCL.Kernel.Eval.DoYield (this, closure, result);
    }

    public void Output (RCClosure closure, RCSymbolScalar name, RCValue val)
    {
      lock (m_botLock)
      {
        Queue<RCAsyncState> output = m_output[closure.Bot.Id];
        output.Enqueue (new RCAsyncState (this, closure, val));
        Queue<RCClosure> watchers;
        if (m_watchers.TryGetValue (closure.Bot.Id, out watchers))
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
            if (parent.Parent.Bot.Id != parent.Bot.Id ||
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
      Log.RecordDoc (this, next, "fiber", 0, "start", right);
      Continue (null, next);
      return id;
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

    public void BotDone (long bot, RCValue result)
    {
      Queue<RCClosure> waiters = null;
      lock (m_botLock)
      {
        if (m_botWaiters.TryGetValue (bot, out waiters))
        {
          m_botWaiters.Remove (bot);
        }
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
            if (queued.Bot.Id == bot)
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
            if (queued.Bot.Id == bot)
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
      closure.Bot.ChangeFiberState (closure.Fiber, "reported");
      Log.RecordDoc (this, closure, "fiber", closure.Fiber, "reported", ex);
    }

    public RCOperator New (string op, RCValue right)
    {
      if (right == null)
        throw new ArgumentNullException ("R");
      return Activator.New (op, right);
    }

    public RCOperator New (string op, RCValue left, RCValue right)
    {
      if (right == null)
        throw new ArgumentNullException ("R");
      return Activator.New (op, left, right);
    }
  }
}
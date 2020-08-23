using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCRunner
  {
    // I made these public for snap - but snap is not being used the way I originally
    // thought
    public readonly object _botLock = new object ();

    /// <summary>
    /// Maintains the existing bots for a running program. Synchronize using _botLock
    /// </summary>
    public Dictionary<long, RCBot> _bots = new Dictionary<long, RCBot> ();

    /// <summary>
    /// Closures which are waiting for a bot to finish executing
    /// </summary>
    protected Dictionary<long, Queue<RCClosure>> _botWaiters =
      new Dictionary<long, Queue<RCClosure>> ();

    /// <summary>
    /// The series of closures being observed by watch.
    /// watch is used to build debugging tools for rcl.
    /// </summary>
    protected Dictionary<long, Queue<RCAsyncState>> _output =
      new Dictionary<long, Queue<RCAsyncState>> ();

    /// <summary>
    /// Closures which are evaluating the watch operator
    /// </summary>
    protected Dictionary<long, Queue<RCClosure>> _watchers =
      new Dictionary<long, Queue<RCClosure>> ();

    /// <summary>
    /// The initial closure used to launch this runner and all of its bots and fibers
    /// </summary>
    protected RCClosure _root = null;

    /// <summary>
    /// The handle for the next bot created within this runner
    /// </summary>
    protected long _bot = 1;

    /// <summary>
    /// Counter tracking the number of times that the Reset method is invoked
    /// Note: This tracks calls to ResetCount in addition to Reset
    /// </summary>
    protected long _reset = 0;

    /// <summary>
    /// Worker threads managed by this runner
    /// </summary>
    protected RCArray<Thread> _workers = new RCArray<Thread> ();

    /// <summary>
    /// Exit status to be returned via Program.Main
    /// </summary>
    protected int _exit = 0;

    /// <summary>
    /// The final result of evaluation
    /// </summary>
    protected volatile RCValue _result = null;

    /// <summary>
    /// If evaluation failed, the exception describing the failure
    /// </summary>
    protected volatile Exception _exception = null;

    /// <summary>
    /// True if an unhandled exception was rethrown by the runner
    /// </summary>
    protected volatile bool _runnerUnhandled = false;

    /// <summary>
    /// The closure which threw _exception
    /// </summary>
    protected volatile RCClosure _exceptionClosure = null;

    /// <summary>
    /// Keeps a count of exceptions reported, mostly for unit testing purposes
    /// </summary>
    public volatile int _exceptionCount = 0;

    /// <summary>
    /// The parser used for any parsing operations by this runner
    /// </summary>
    protected RCParser _parser;

    /// <summary>
    /// Lock object for access to the queue of closures being worked
    /// </summary>
    protected readonly object _queueLock = new object ();

    /// <summary>
    /// The queue of closures being worked
    /// </summary>
    protected Queue<RCClosure> _queue = new Queue<RCClosure> ();

    /// <summary>
    /// Tracks closures which have completed initial execution but have not yet invoked
    /// Yield. The closure is working asyncly.
    /// </summary>
    protected Dictionary<long, Dictionary<long, RCClosure>> _pending =
      new Dictionary<long, Dictionary< long, RCClosure>> ();

    /// <summary>
    /// Used by worker threads to wait when there are no more closures to execute
    /// </summary>
    protected AutoResetEvent _wait = new AutoResetEvent (false);

    /// <summary>
    /// Used by worker threads
    /// </summary>
    protected AutoResetEvent _done = new AutoResetEvent (false);

    /// <summary>
    /// The thread which invoked the runner constructor
    /// </summary>
    protected Thread _ctorThread;

    public static RCRunner TestRunner ()
    {
      RCSystem.Reconfigure (new RCLArgv ("--output=test", "--show=print"));
      return new RCRunner (workers: 1);
    }

    public RCRunner () : this (workers : 1) {}
    public RCRunner (long workers)
    {
      _ctorThread = Thread.CurrentThread;
      _bots[0] = new RCBot (this, 0);
      _output[0] = new Queue<RCAsyncState> ();
      _parser = new RCLParser (RCSystem.Activator);
      Console.CancelKeyPress += HandleConsoleCancelKeyPress;
      for (int i = 0; i < workers; ++i)
      {
        Thread worker = new Thread (Work);
        worker.IsBackground = true;
        _workers.Write (worker);
        worker.Start ();
      }
    }

    public void Start (RCValue program)
    {
      if (program == null) {
        throw new Exception ("program may not be null");
      }
      RCClosure root = null;
      lock (_queueLock)
      {
        if (_root != null) {
          throw new Exception ("Runner has already started.");
        }
        RCBot rootBot = _bots[0];
        root = new RCClosure (rootBot.Id, program);
        rootBot.ChangeFiberState (root.Fiber, "start");
        RCSystem.Log.Record (root, "fiber", root.Fiber, "start", root.Code);
        _root = root;
        _queue.Enqueue (root);
      }
      _wait.Set ();
    }

    public RCValue Run (RCValue program)
    {
      return Run (program, restoreStateOnError: false);
    }

    public RCValue Run (RCValue program, bool restoreStateOnError)
    {
      // Shouldn't this be an exception?
      if (program == null) {
        return null;
      }
      RCBlock wrapper = new RCBlock (RCBlock.Empty, "", "<-", program);
      RCClosure parent = new RCClosure (_bots[0].Id,
                                        0,
                                        null,
                                        null,
                                        wrapper,
                                        null,
                                        _state,
                                        0,
                                        null,
                                        null,
                                        noClimb: false,
                                        noResolve: false);
      RCClosure closure = new RCClosure (parent,
                                         _bots[0].Id,
                                         program,
                                         null,
                                         RCBlock.Empty,
                                         0,
                                         null,
                                         null);
      RCValue result = Run (closure, restoreStateOnError);
      return result;
    }

    protected RCValue Run (RCClosure root, bool restoreStateOnError)
    {
      lock (_queueLock)
      {
        if (_root == null) {
          _root = root;
          RCBot rootBot = GetBot (_root.Bot);
          // keeping this inside the lock because it should happen before the call to
          // Enqueue.
          // But only during the very first call to run for this runner.
          // Log.Record (this, root, root.BotId, "bot", root.BotId, "start", root.Code);
          rootBot.ChangeFiberState (root.Fiber, "start");
          RCSystem.Log.Record (root, "fiber", root.Fiber, "start", root.Code);
        }
        _queue.Enqueue (root);
      }
      // Trigger a worker (don't care which) to take it.
      _wait.Set ();
      // Wait for the work to be completed.
      _done.WaitOne ();
      // If an exception was thrown, rethrow it on this thread.
      if (_exception != null) {
        Exception exception = _exception;
        _exception = null;
        if (restoreStateOnError) {
          // Make the successfully computed values into the effective state of the
          // environment
          RCClosure top = _exceptionClosure;
          RCClosure underTop = null;
          while (top.Parent != null && top.Parent.Parent != null)
          {
            underTop = top;
            top = top.Parent;
          }
          if (underTop != null && top.Code.IsOperator) {
            top = underTop;
          }
          _state = RCBlock.Append (_state, top.Result);
        }
        _runnerUnhandled = true;
        throw exception;
      }
      // The final result is assigned by the worker in Finish ().
      RCValue result = _result;
      _result = null;
      return result;
    }

    public bool RunnerUnhandled
    {
      get { return _runnerUnhandled; }
    }

    void HandleConsoleCancelKeyPress (object sender, ConsoleCancelEventArgs e)
    {
      // Console.Out.WriteLine ("Cancel Key Press");
      // Interupt ();
      // e.Cancel = true;
    }

    public void Interupt ()
    {
      // Kill all fibers for the 0 bot.
      this.Kill (0, -1, new Exception ("Interupt"), 1);
    }

    // Previous is the closure that will be removed from the pending set.
    // Next is the closure that will be added to the queue.
    // This is done in an atomic fashion so that all fibers will be
    // represented in _pending or _queue at all times.
    // previous will be null in cases where Continue is used to retry or fork streams of
    // execution.
    // next will be null in cases where the executing fiber is finished and all
    // that remains is to remove it from _pending.
    public void Continue (RCClosure previous, RCClosure next)
    {
      bool live = false;
      lock (_queueLock)
      {
        if (previous != null) {
          Dictionary<long, RCClosure> pending = null;
          if (_pending.TryGetValue (previous.Bot, out pending)) {
            RCClosure c = null;
            if (pending.TryGetValue (previous.Fiber, out c)) {
              pending.Remove (previous.Fiber);
              if (pending.Count == 0) {
                _pending.Remove (previous.Bot);
              }
              live = true;
            }
          }
        }
        else {
          live = true;
        }
        if (live) {
          if (next != null) {
            _queue.Enqueue (next);
            _wait.Set ();
          }
        }
        else {
          // This will internally take the _botLock.
          // This should be ok but given that it is just a log write
          // I would like to move this outside.
          RCBot bot = GetBot (previous.Bot);
          bot.ChangeFiberState (previous.Fiber, "dead");
          RCSystem.Log.Record (previous, "fiber", previous.Fiber, "dead", "");
        }
      }
    }

    protected RCClosure Assign ()
    {
      lock (_queueLock)
      {
        RCClosure next;
        if (_queue.Count > 0) {
          next = _queue.Dequeue ();
        }
        else {
          // We were signalled, but it was already gone.
          // Another worker took it out from under us.
          return null;
        }
        Dictionary<long, RCClosure> fibers = null;
        if (!_pending.TryGetValue (next.Bot, out fibers)) {
          fibers = new Dictionary<long, RCClosure> ();
          _pending[next.Bot] = fibers;
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
        if (result == null) {
          _wait.WaitOne ();
          prev = next;
          next = Assign ();
        }
        else {
          break;
        }

        while (next != null)
        {
          try
          {
            next.Code.Eval (this, next);
          }
          catch (Exception userex)
          {
            ++_exceptionCount;
            try
            {
              Kill (next.Bot, next.Fiber, userex, 1);
            }
            catch (Exception sysex)
            {
              _exception = sysex;
              _exceptionClosure = next;
              ++_exceptionCount;
              SafeLogRecord (next, "fiber", "killfail", sysex);
              _done.Set ();
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
        RCSystem.Log.Record (closure,
                             module,
                             closure.Fiber,
                             state,
                             "An exception occured while reporting an exception: " +
                             innerEx.ToString ());
      }
    }

    public void Dispose ()
    {
      lock (_botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in _bots)
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
      lock (_botLock)
      {
        _exit = status;
      }
      _ctorThread.Abort ();
    }

    public int ExitStatus ()
    {
      lock (_botLock)
      {
        if (_exceptionCount > 0) {
          return 2;
        }
        else {
          return _exit;
        }
      }
    }

    public RCValue Read (string code)
    {
      bool fragment;
      return _parser.Parse (_parser.Lex (code), out fragment, canonical: false);
    }

    public RCValue Read (string code, out bool fragment, bool canonical)
    {
      return _parser.Parse (_parser.Lex (code), out fragment, canonical);
    }

    public RCArray<RCToken> Lex (string code)
    {
      return _parser.Lex (code);
    }

    public void Reset ()
    {
      // Should I take the locks here? One or both? Both.
      lock (_queueLock)
      {
        lock (_botLock)
        {
          _parser = new RCLParser (RCSystem.Activator);
          _root = null;
          _result = null;
          _exception = null;
          _exceptionClosure = null;
          _exceptionCount = 0;
          _queue = new Queue<RCClosure> ();
          _pending = new Dictionary<long, Dictionary<long, RCClosure>> ();
          _bot = 1;
          _bots = new Dictionary<long, RCBot> ();
          _bots[0] = new RCBot (this, 0);
          _output[0] = new Queue<RCAsyncState> ();
          ++_reset;
        }
      }
    }

    public void ResetCount (long botHandle)
    {
      // Should I take the locks here? One or both? Both.
      lock (_queueLock)
      {
        lock (_botLock)
        {
          RCBot bot;
          if (!_bots.TryGetValue (botHandle, out bot)) {
            throw new Exception (string.Format ("Invalid bot: {0}", botHandle));
          }
          // bot.Reset ();
          _exceptionCount = 0;
          ++_reset;
        }
      }
    }

    public RCValue Rep (RCValue program, bool restoreStateOnError)
    {
      RCValue result = Run (program, restoreStateOnError);
      RCBlock state = result as RCBlock;
      if (state != null) {
        _state = state;
      }
      return result;
    }

    public RCValue RepAction (string action)
    {
      if (_state.Get (action) == null) {
        throw new ArgumentException (string.Format ("Unknown action name: {0}", action));
      }
      RCValue result = Rep (string.Format ("{0} {{}}", action), restoreStateOnError: true);
      RCBlock variables = result as RCBlock;
      if (variables != null) {
        for (int i = 0; i < variables.Count; ++i)
        {
          RCBlock variable = variables.GetName (i);
          _state = new RCBlock (_state, variable.Name, variable.Evaluator, variable.Value);
        }
      }
      return result;
    }

    protected RCBlock _state = RCBlock.Empty;

    public RCValue Rep (string code)
    {
      return Rep (code, restoreStateOnError: false);
    }

    public RCValue Rep (string code, bool restoreStateOnError)
    {
      bool fragment = false;
      RCValue peek = RCSystem.Parse (code, out fragment);
      if (peek == null) {
        return null;
      }
      RCBlock variable = peek as RCBlock;
      if (variable != null && fragment) {
        if (variable.Count == 0) {
          return null;
        }
        if (variable.Value.ArgumentEval) {
          RCBlock program = new RCBlock (_state, "", "<-", variable.Value);
          RCClosure parent = new RCClosure (_bots[0].Id,
                                            0,
                                            null,
                                            null,
                                            program,
                                            null,
                                            _state,
                                            _state.Count,
                                            null,
                                            null,
                                            noClimb: false,
                                            noResolve: false);
          RCClosure child = new RCClosure (parent,
                                           _bots[0].Id,
                                           variable.Value,
                                           null,
                                           RCBlock.Empty,
                                           0,
                                           null,
                                           null);
          RCValue result = Run (child, restoreStateOnError);
          _state = new RCBlock (_state, variable.Name, ":", result);
        }
        else {
          // What about fragments with multiple parts.
          _state = new RCBlock (_state, variable.Name, ":", variable.Value);
        }
        return null;
      }
      else {
        RCBlock program = new RCBlock (_state, "", "<-", peek);
        RCClosure parent = new RCClosure (_bots[0].Id,
                                          0,
                                          null,
                                          null,
                                          program,
                                          null,
                                          _state,
                                          _state.Count,
                                          null,
                                          null,
                                          noClimb: false,
                                          noResolve: false);
        RCClosure child = new RCClosure (parent, _bots[0].Id, peek, null, RCBlock.Empty, 0);
        RCValue result = Run (child, restoreStateOnError);
        return result;
      }
    }

    public string RepString (string code)
    {
      RCValue result = Rep (code, restoreStateOnError: false);
      if (result == null) {
        return "";
      }
      return result.Format (RCFormat.DefaultNoT);
    }

    public void Finish (RCClosure closure, RCValue result)
    {
      Fiber fiber;
      lock (_botLock)
      {
        fiber = (Fiber) _bots[0].GetModule (typeof (Fiber));
      }
      lock (fiber._fiberLock)
      {
        RCValue finalResult;
        fiber._fiberResults.TryGetValue (0, out finalResult);
        _result = finalResult;
      }
      if (_result == null) {
        _result = result;
      }
      _done.Set ();
    }

    public void Finish (RCClosure closure, Exception exception, long status)
    {
      RCValue result = null;
      RCClosure parent = closure;
      RCBot bot = GetBot (closure.Bot);
      while (parent != null && parent.Bot == closure.Bot && parent.Fiber == closure.Fiber)
      {
        if (parent.InCodeEval) {
          RCClosure next = parent.Code.Handle (this, parent, exception, status, out result);
          if (result != null && next == null) {
            string state = status == 1 ? "caught" : "killed";
            bot.ChangeFiberState (closure.Fiber, state);
            RCSystem.Log.Record (closure, "fiber", closure.Fiber, state, exception);
            if (closure.Fiber == 0 && closure.Bot == 0) {
              Finish (closure, result);
            }
            else {
              bot.FiberDone (this, closure.Bot, closure.Fiber, result);
            }
            return;
          }
          else {
            Continue (null, next);
          }
          if (result != null) {
            bot.ChangeFiberState (closure.Fiber, "caught");
            RCSystem.Log.Record (closure, "fiber", closure.Fiber, "caught", exception);
            ++_exceptionCount;
            return;
          }
        }
        parent = parent.Parent;
      }
      // This means it was not handled in the while loop.
      if (result == null) {
        string state = status == 1 ? "failed" : "killed";
        bot.ChangeFiberState (closure.Fiber, state);
        SafeLogRecord (closure, "fiber", state, exception);
        ++_exceptionCount;
        if (closure.Fiber == 0 && closure.Bot == 0) {
          _exception = exception;
          _exceptionClosure = closure;
          _done.Set ();
        }
        else {
          // I think this is sort of mostly the correct think to do.
          // We need to record the fact that the fiber finished.
          // But stuffing an exception inside a Native to do so seems wrong.
          // Need more work on controlling the lifecycle of fibers.
          // Also I want to get rid of RCNative I think this is the only place
          // where I still need it.
          bot.FiberDone (this, closure.Bot, closure.Fiber, new RCNative (exception));
        }
      }
    }

    public void Yield (RCClosure closure, RCValue result)
    {
      RCL.Kernel.Eval.DoYield (this, closure, result);
    }

    public void Output (RCClosure closure, RCSymbolScalar name, RCValue val)
    {
      lock (_botLock)
      {
        Queue<RCAsyncState> output = _output[closure.Bot];
        output.Enqueue (new RCAsyncState (this, closure, val));
        Queue<RCClosure> watchers;
        if (_watchers.TryGetValue (closure.Bot, out watchers)) {
          while (watchers.Count > 0)
          {
            RCClosure observer = watchers.Dequeue ();
            Continue (null, observer);
          }
        }
        else {
          while (output.Count > 500)
          {
            output.Dequeue ();
          }
        }
      }
    }

    // It is possible to have multiple concurrent observers.
    // However each value will only be returned to one of the observers.
    // It is hard to see a reason to create multiple observers but there
    // are multiple potential problems with implementing the constraint that there
    // be only one.
    public void Watch (RCClosure closure, long bot)
    {
      RCBlock result;
      lock (_botLock)
      {
        Queue<RCAsyncState> output = _output[bot];
        if (output.Count == 0) {
          Queue<RCClosure> watchers;
          if (!_watchers.TryGetValue (bot, out watchers)) {
            watchers = new Queue<RCClosure> ();
            _watchers[bot] = watchers;
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
                parent.Parent.Fiber != parent.Fiber) {
              break;
            }
            parts.Push (parent);
            parent = parent.Parent;
          }
          while (parts.Count > 0)
          {
            RCClosure top = parts.Pop ();
            if (top.Code.IsBlock) {
              RCBlock code = (RCBlock) top.Code;
              string part = code.GetName (top.Index).Name;
              if (part != "") {
                name = new RCSymbolScalar (name, part);
              }
              else {
                name = new RCSymbolScalar (name, (long) top.Index);
              }
            }
          }
          if (name != null) {
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
      lock (_botLock)
      {
        id = _bot++;
        bot = new RCBot (this, id);
        _output[id] = new Queue<RCAsyncState> ();
        _bots[id] = bot;
        next = Fiber.FiberClosure (bot, 0, closure, right);
      }
      bot.ChangeFiberState (0, "start");
      RCSystem.Log.Record (next, "fiber", 0, "start", "");
      Continue (null, next);
      return id;
    }

    public RCBot GetBot (long id)
    {
      RCBot result;
      lock (_botLock)
      {
        if (!_bots.TryGetValue (id, out result)) {
          throw new Exception ("Unknown bot id: " + id);
        }
      }
      return result;
    }

    public void Done (RCClosure closure, RCLong fibers)
    {
      if (fibers.Count == 1) {
        Fiber fiber;
        lock (_botLock)
        {
          fiber = (Fiber) _bots[fibers[0]].GetModule (typeof (Fiber));
        }
        bool val;
        lock (fiber._fiberLock)
        {
          val = fiber._fibers.Count == fiber._fiberResults.Count;
        }
        Yield (closure, new RCBoolean (val));
      }
      else if (fibers.Count == 2) {
        bool val;
        Fiber fiber;
        lock (_botLock)
        {
          fiber = (Fiber) _bots[fibers[0]].GetModule (typeof (Fiber));
        }
        lock (fiber._fiberLock)
        {
          val = fiber._fiberResults.ContainsKey (fibers[1]);
        }
        Yield (closure, new RCBoolean (val));
      }
      else {
        throw new Exception ();
      }
    }

    public void Wait (RCClosure closure, RCLong fibers)
    {
      // At some point I want this to work for multiple fibers,
      // but the current version will only wait on a single fiber.
      if (fibers.Count == 1) {
        Fiber fiber;
        lock (_botLock)
        {
          fiber = (Fiber) _bots[fibers[0]].GetModule (typeof (Fiber));
        }
        RCValue result = null;
        lock (fiber._fiberLock)
        {
          if (fiber._fiberResults.Count == fiber._fibers.Count) {
            fiber._fiberResults.TryGetValue (0, out result);
          }
        }
        if (result == null) {
          lock (_botLock)
          {
            Queue<RCClosure> waiters;
            if (_botWaiters.TryGetValue (fibers[0], out waiters)) {
              waiters.Enqueue (closure);
            }
            else {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              _botWaiters.Add (fibers[0], waiters);
            }
          }
        }
        else {
          SafeYieldFromWait (closure, result);
        }
      }
      else if (fibers.Count == 2) {
        RCValue result = null;
        Fiber fiber;
        lock (_botLock)
        {
          fiber = (Fiber) _bots[fibers[0]].GetModule (typeof (Fiber));
        }
        lock (fiber._fiberLock)
        {
          if (!fiber._fiberResults.TryGetValue (fibers[1], out result)) {
            Queue<RCClosure> waiters;
            if (fiber._fiberWaiters.TryGetValue (fibers[1], out waiters)) {
              waiters.Enqueue (closure);
            }
            else {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              fiber._fiberWaiters.Add (fibers[1], waiters);
            }
          }
        }
        if (result != null) {
          SafeYieldFromWait (closure, result);
        }
      }
      else {
        throw new Exception ();
      }
    }

    protected class Wakeup
    {
      protected readonly RCAsyncState _state;
      protected readonly long _resetCount;

      public Wakeup (RCAsyncState state, long resetCount)
      {
        _state = state;
        _resetCount = resetCount;
      }

      public virtual void ContinueBot (Object obj)
      {
        Timer timer = (Timer) obj;
        RCLong fibers = (RCLong) _state.Other;
        try
        {
          lock (_state.Runner._botLock)
          {
            if (_state.Runner._reset != _resetCount) {
              return;
            }
            Queue<RCClosure> waiters;
            // Since the bot results are not stored anywhere, we time out if there are
            // waiters but what if there are multiple waiters? This seems like an issue.
            if (_state.Runner._botWaiters.TryGetValue (fibers[0], out waiters)) {
              Exception ex = new Exception ("Timed out waiting for bot " + fibers[0]);
              _state.Runner.Kill (fibers[0], -1, ex, 2);
            }
          }
        }
        catch (ThreadAbortException)
        {
          // This often happens as the runtime is shutting down,
          // because this code runs on a thread pool thread.
        }
        catch (Exception ex)
        {
          _state.Runner.Report (_state.Closure, ex);
        }
        finally
        {
          timer.Dispose ();
        }
      }

      public virtual void ContinueFiber (Object obj)
      {
        Timer timer = (Timer) obj;
        RCLong fibers = (RCLong) _state.Other;
        try
        {
          Fiber fiber;
          lock (_state.Runner._botLock)
          {
            if (_state.Runner._reset != _resetCount) {
              return;
            }
            fiber = (Fiber) _state.Runner._bots[fibers[0]].GetModule (typeof (Fiber));
          }
          lock (fiber._fiberLock)
          {
            RCValue result = null;
            if (!fiber._fiberResults.TryGetValue (fibers[1], out result)) {
              Exception ex = new Exception ("Timed out waiting for fiber " + fibers[1]);
              _state.Runner.Kill (fibers[0], fibers[1], ex, 2);
            }
          }
        }
        catch (ThreadAbortException)
        {
          // This often happens as the runtime is shutting down,
          // because this code runs on a thread pool thread.
        }
        catch (Exception ex)
        {
          _state.Runner.Report (_state.Closure, ex);
        }
        finally
        {
          timer.Dispose ();
        }
      }
    }

    public void Wait (RCClosure closure, RCLong timeout, RCLong fibers)
    {
      // At some point I want this to work for multiple fibers,
      // but the current version will only wait on a single fiber.
      if (fibers.Count == 1) {
        Fiber fiber;
        lock (_botLock)
        {
          fiber = (Fiber) _bots[fibers[0]].GetModule (typeof (Fiber));
        }
        RCValue result = null;
        lock (fiber._fiberLock)
        {
          if (fiber._fiberResults.Count == fiber._fibers.Count) {
            fiber._fiberResults.TryGetValue (0, out result);
          }
        }
        if (result == null) {
          lock (_botLock)
          {
            Queue<RCClosure> waiters;
            if (_botWaiters.TryGetValue (fibers[0], out waiters)) {
              waiters.Enqueue (closure);
            }
            else {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              _botWaiters.Add (fibers[0], waiters);
            }
            RCAsyncState state = new RCAsyncState (this, closure, fibers);
            Wakeup wakeup = new Wakeup (state, _reset);
            Timer timer = new Timer (wakeup.ContinueBot);
            timer.Change (timeout[0], Timeout.Infinite);
          }
        }
        else {
          SafeYieldFromWait (closure, result);
        }
      }
      else if (fibers.Count == 2) {
        RCValue result = null;
        Fiber fiber;
        lock (_botLock)
        {
          fiber = (Fiber) _bots[fibers[0]].GetModule (typeof (Fiber));
        }
        lock (fiber._fiberLock)
        {
          if (!fiber._fiberResults.TryGetValue (fibers[1], out result)) {
            Queue<RCClosure> waiters;
            if (fiber._fiberWaiters.TryGetValue (fibers[1], out waiters)) {
              waiters.Enqueue (closure);
            }
            else {
              waiters = new Queue<RCClosure> ();
              waiters.Enqueue (closure);
              fiber._fiberWaiters.Add (fibers[1], waiters);
            }
            RCAsyncState state = new RCAsyncState (this, closure, fibers);
            Wakeup wakeup = new Wakeup (state, _reset);
            Timer timer = new Timer (wakeup.ContinueFiber);
            timer.Change (timeout[0], Timeout.Infinite);
          }
        }
        if (result != null) {
          SafeYieldFromWait (closure, result);
        }
      }
      else {
        throw new Exception ();
      }
    }

    protected void SafeYieldFromWait (RCClosure closure, RCValue result)
    {
      RCNative exValue = result as RCNative;
      if (exValue != null && exValue.Value is Exception) {
        Finish (closure, (Exception) exValue.Value, 1);
      }
      else {
        Yield (closure, result);
      }
    }

    public void BotDone (long bot, RCValue result)
    {
      Queue<RCClosure> waiters = null;
      lock (_botLock)
      {
        if (_botWaiters.TryGetValue (bot, out waiters)) {
          _botWaiters.Remove (bot);
        }
        _bots[bot].Dispose ();
      }
      if (waiters != null) {
        foreach (RCClosure waiter in waiters)
        {
          // I think this might be another where place where we need SafeYieldFromWait
          // but I need a repro to prove that. Should be an easy fix then.
          Yield (waiter, result);
        }
      }
    }

    public void Kill (RCClosure closure, RCLong fibers)
    {
      if (fibers.Count == 1) {
        Kill (fibers[0], -1, new Exception ("fiber killed"), 2);
      }
      else if (fibers.Count == 2) {
        Kill (fibers[0], fibers[1], new Exception ("fiber killed"), 2);
      }
      else {
        throw new Exception ();
      }
    }

    public void Kill (long bot, long fiber, Exception ex, long status)
    {
      if (fiber < 0) {
        lock (_queueLock)
        {
          Queue<RCClosure> queue = new Queue<RCClosure> ();
          HashSet<long> killed = new HashSet<long> ();
          while (_queue.Count > 0)
          {
            RCClosure queued = _queue.Dequeue ();
            if (queued.Bot == bot) {
              if (!killed.Contains (queued.Fiber)) {
                killed.Add (queued.Fiber);
                Finish (queued, ex, status);
              }
            }
            else {
              queue.Enqueue (queued);
            }
          }
          _queue = queue;
          Dictionary<long, RCClosure> pending;
          if (_pending.TryGetValue (bot, out pending)) {
            RCClosure[] closures = new RCClosure[pending.Count];
            pending.Values.CopyTo (closures, 0);
            for (int i = 0; i < closures.Length; ++i)
            {
              if (!killed.Contains (closures[i].Fiber)) {
                killed.Add (closures[i].Fiber);
                Finish (closures[i], ex, status);
              }
            }
            _pending.Remove (bot);
          }
        }
      }
      else {
        // Kill only the fiber on the designated bot.
        // Do we want to allow multiple bot fiber pairs? Yes! No.
        // No. The final answer is no.
        lock (_queueLock)
        {
          Queue<RCClosure> queue = new Queue<RCClosure> ();
          HashSet<long> killed = new HashSet<long> ();
          while (_queue.Count > 0)
          {
            RCClosure queued = _queue.Dequeue ();
            if (queued.Bot == bot) {
              if (queued.Fiber == fiber &&
                  !killed.Contains (queued.Fiber)) {
                killed.Add (queued.Fiber);
                Finish (queued, ex, status);
              }
              else {
                queue.Enqueue (queued);
              }
            }
            else {
              queue.Enqueue (queued);
            }
          }
          _queue = queue;
          Dictionary<long, RCClosure> pending;
          if (_pending.TryGetValue (bot, out pending)) {
            RCClosure target;
            if (pending.TryGetValue (fiber, out target)) {
              pending.Remove (fiber);
              if (!killed.Contains (fiber)) {
                killed.Add (fiber);
                Finish (target, ex, status);
              }
            }
            if (pending.Count == 0) {
              _pending.Remove (bot);
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
      ++_exceptionCount;
    }

    public void Report (Exception ex)
    {
      this.Report (ex, "reported");
    }

    public void Report (Exception ex, string status)
    {
      RCSystem.Log.Record (0, 0, "fiber", 0, status, ex);
      ++_exceptionCount;
    }

    public RCOperator New (string op, RCValue right)
    {
      if (right == null) {
        throw new ArgumentNullException ("R");
      }
      return RCSystem.Activator.New (op, right);
    }

    public RCOperator New (string op, RCValue left, RCValue right)
    {
      if (right == null) {
        throw new ArgumentNullException ("R");
      }
      return RCSystem.Activator.New (op, left, right);
    }

    public int ExceptionCount
    {
      get { return _exceptionCount; }
    }
  }
}

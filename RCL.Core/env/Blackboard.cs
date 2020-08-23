
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  // Implements concurrency control and communication between fibers.
  public class Blackboard
  {
    protected readonly object _readWriteLock = new object ();
    protected Dictionary<object, Section> _sections = new Dictionary<object, Section> ();

    public Blackboard ()
    {}

    protected class Section
    {
      internal long _g = 0;
      internal MapQueue _readWaiters = new MapQueue ();
      internal MapQueue _dispatchWaiters = new MapQueue ();
      internal MapQueue _throttleWaiters = new MapQueue ();
      internal Dictionary<RCSymbolScalar, long> _dispatchLines = new Dictionary<RCSymbolScalar,
                                                                                 long> ();
      internal ReadCounter _counter = new ReadCounter ();
      internal RCCube _blackboard = new RCCube (new RCArray<string> ("G", "E", "S"));
      // internal RCCube _blackboard = new RCCube (new RCArray<string> ("G", "S"));

      internal void Clear ()
      {
        // g gets updated...
        _g += _blackboard.Count;
        // blackboard cube is dumped.
        _blackboard = new RCCube (new RCArray<string> ("G", "E", "S"));
        // Uncomment this to disable the E column I think
        // I want it to be G or E in the blackboard, not both
        // _blackboard = new RCCube (new RCArray<string> ("G", "S"));
        // waiters keep on waiting.
        // But what about _counter?
        // Shouldn't we just reset it?
        _counter = new ReadCounter ();
      }
    }

    protected Section GetSection (RCSymbol symbol)
    {
      // This assumes that all symbols in symbol have the same first part!
      object key = symbol[0].Part (0);
      Section s;
      if (!_sections.TryGetValue (key, out s)) {
        s = new Section ();
        _sections[key] = s;
      }
      return s;
    }

    [RCVerb ("read")]
    public void EvalRead (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      try
      {
        Section section = GetSection (left);
        Read (runner,
              closure,
              left,
              new ReadSpec (section._counter,
                            left,
                            right,
                            0,
                            false,
                            false,
                            true,
                            false));
      }
      catch (Exception)
      {
        throw;
      }
    }

    [RCVerb ("read")]
    public void EvalRead (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      try
      {
        RCLong args = new RCLong (0, 0);
        Section section = GetSection (right);
        Read (runner,
              closure,
              right,
              new ReadSpec (section._counter,
                            right,
                            args,
                            0,
                            false,
                            false,
                            true,
                            false));
      }
      catch (Exception)
      {
        throw;
      }
    }

    [RCVerb ("trace")]
    public void EvalTrace (RCRunner runner, RCClosure closure, RCLong left, RCSymbol right)
    {
      try
      {
        // This read is different from all others in that it has force on and fill off.
        // force causes duplicate values to be written.
        // fill causes prior values to be filled into the results.
        // This read gives you exactly what exists in the blackboard.
        Section section = GetSection (right);
        Read (runner,
              closure,
              right,
              new ReadSpec (section._counter,
                            right,
                            left,
                            0,
                            false,
                            true,
                            false,
                            true));
      }
      catch (Exception)
      {
        throw;
      }
    }

    [RCVerb ("trace")]
    public void EvalTrace (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      try
      {
        // This read is different from all others in that it has force on and fill off.
        // force causes duplicate values to be written.
        // fill causes prior values to be filled into the results.
        // This read gives you exactly what exists in the blackboard.
        RCLong args = new RCLong (0, 0);
        Section section = GetSection (right);
        Read (runner,
              closure,
              right,
              new ReadSpec (section._counter,
                            right,
                            args,
                            0,
                            false,
                            true,
                            false,
                            true));
      }
      catch (Exception)
      {
        throw;
      }
    }

    [RCVerb ("select")]
    public void EvalSelect (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      RCLong args = new RCLong (0, 0);
      ReadCounter counter = new ReadCounter (right);
      ReadSpec spec = new ReadSpec (counter, left, args, 0, false, false, true, false);
      RCCube result = right.Read (spec, counter, true, right.Count);
      runner.Yield (closure, result);
    }

    public void Read (RCRunner runner, RCClosure closure, RCSymbol symbol, ReadSpec spec)
    {
      lock (_readWriteLock)
      {
        // Make abstract symbols concrete
        Section section = GetSection (symbol);
        Satisfy canSatisfy = section._counter.CanSatisfy (spec);
        RCCube result = section._blackboard.Read (spec,
                                                   section._counter,
                                                   true,
                                                   section._blackboard.Count);
        if (spec.SymbolUnlimited) {
          if (result.Count > 0) {
            // Notice the canSatisfy constraints here are less strict.
            // If the start point is greater than zero then we have to
            // do the full read and then just see if there were enough
            // rows to satisfy the constraints.
            if (canSatisfy == Satisfy.No) {
              throw new Exception ();
            }
            runner.Yield (closure, result);
          }
          else {
            if (canSatisfy == Satisfy.Yes) {
              throw new Exception ();
            }
            section._readWaiters.Enqueue (symbol, closure);
          }
        }
        else {
          if (result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit)) {
            if (canSatisfy == Satisfy.No) {
              throw new Exception ();
            }
            runner.Yield (closure, result);
          }
          else {
            if (canSatisfy == Satisfy.Yes) {
              throw new Exception ();
            }
            section._readWaiters.Enqueue (symbol, closure);
          }
        }
      }
    }

    [RCVerb ("peek")]
    public void EvalPeek (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      int limit = (int) right[0];
      lock (_readWriteLock)
      {
        Section s = GetSection (left);
        ReadSpec spec = s._counter.GetReadSpec (left, limit, false, true);
        Satisfy canSatisfy = s._counter.CanSatisfy (spec);
        RCCube result = s._blackboard.Read (spec, s._counter, true, s._blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= left.Count * Math.Abs (spec.SymbolLimit))) {
          // If dispatch would yield, return lines.
          if (canSatisfy != Satisfy.Yes) {
            throw new Exception ();
          }
          runner.Yield (closure, RCBoolean.True);
        }
        else {
          if (canSatisfy != Satisfy.No) {
            throw new Exception ();
          }
          runner.Yield (closure, RCBoolean.False);
        }
      }
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      // right now last defaults to -1 and read defaults to long.MaxValue; thats kind of
      // weird.
      // but seems to fit with the way i use it those operators.
      Section section = GetSection (left);
      Read (runner,
            closure,
            left,
            new ReadSpec (section._counter,
                          left,
                          right,
                          -1,
                          false,
                          false,
                          true,
                          false));
    }

    [RCVerb ("gawk")]
    public void EvalGawk (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int limit = (int) right[0];
      lock (_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s._counter.GetReadSpec (symbol, limit, false, true);
        Satisfy canSatisfy = s._counter.CanSatisfy (spec);
        RCCube result = s._blackboard.Read (spec, s._counter, true, s._blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit))) {
          if (canSatisfy != Satisfy.Yes) {
            throw new Exception ();
          }
          runner.Yield (closure, result);
        }
        else {
          if (canSatisfy != Satisfy.No) {
            throw new Exception ();
          }
          s._dispatchWaiters.Enqueue (symbol, closure);
        }
      }
    }

    [RCVerb ("poll")]
    public void EvalPoll (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong starts)
    {
      // Poll is like read but it will never block.
      lock (_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s._counter.GetReadSpec (symbol, starts, false, true);
        RCCube result = s._blackboard.Read (spec, s._counter, true, s._blackboard.Count);
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("snap")]
    public void EvalSnap (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      // With snap, the read instructions apply to each symbol in a family of symbols.
      // Not only to the symbol explcitly noted in the left argument.
      lock (_readWriteLock)
      {
        Section s = GetSection (left);
        // This line with the ConcreteSymbols should be added to other operators
        // RCSymbol concretes = s._counter.ConcreteSymbols (left, false);
        ReadSpec spec = new ReadSpec (s._counter, left, right, -1, false, false, true, false);
        // RCCube result = s._blackboard.Read (spec, s._counter, true,
        // s._blackboard.Count);
        RCCube result = s._blackboard.Read (spec, s._counter, true, s._blackboard.Count);
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("page")]
    public void EvalPage (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int pageNumber = 0;
      if (right.Count > 0) {
        pageNumber = (int) right[0];
      }
      int pageSize = int.MaxValue;
      if (right.Count > 1) {
        pageSize = (int) right[1];
      }

      // Page lets you access the blackboard by page number and page size, rather than row
      // numbers.
      // Good for building tools for looking at blackboard contents.
      lock (_readWriteLock)
      {
        Section s = GetSection (symbol);
        int skipFirst = pageNumber * pageSize;
        int stopAfter = pageSize;
        ReadSpec spec = new ReadSpec (s._counter, symbol, skipFirst, stopAfter, false);
        RCCube result = s._blackboard.Read (spec, s._counter, true, s._blackboard.Count);
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("clear")]
    public void EvalClear (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      lock (_readWriteLock)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          string name = right[i].Part (0).ToString ();
          Section section;
          if (_sections.TryGetValue (name, out section)) {
            section.Clear ();
          }
        }
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("dispatch")]
    public void EvalDispatch (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int limit = (int) right[0];
      lock (_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s._counter.GetReadSpec (symbol, limit, false, true);
        Satisfy canSatisfy = s._counter.CanSatisfy (spec);
        RCCube result = s._blackboard.Read (spec,
                                             s._counter,
                                             true,
                                             s._blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit))) {
          if (canSatisfy != Satisfy.Yes) {
            throw new Exception ();
          }
          s._counter.Dispatch (s._blackboard, result.AcceptedLines);
          runner.Yield (closure, result);
          Dictionary<long, RCClosure> throttlers = null;
          s._throttleWaiters.GetReadersForSymbol (ref throttlers,
                                                   result.AcceptedSymbols);
          ContinueWaiters (runner, throttlers);
        }
        else {
          if (canSatisfy != Satisfy.No) {
            throw new Exception ();
          }
          s._dispatchWaiters.Enqueue (symbol, closure);
        }
      }
    }

    public void ContinueWaiters (RCRunner runner, Dictionary<long, RCClosure> all)
    {
      if (all != null) {
        foreach (RCClosure waiter in all.Values)
        {
          runner.Continue (null, waiter);
        }
      }
    }

    [RCVerb ("throttle")]
    public void EvalThrottle (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int limit = (int) right[0];
      lock (_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s._counter.GetReadSpec (symbol, limit, false, true);
        Satisfy canSatisfy = s._counter.CanSatisfy (spec);
        RCCube result = s._blackboard.Read (spec,
                                             s._counter,
                                             true,
                                             s._blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit))) {
          // If dispatch would yield, suspend.
          if (canSatisfy != Satisfy.Yes) {
            throw new Exception ();
          }
          s._throttleWaiters.Enqueue (symbol, closure);
        }
        else {
          // Return the count of the result set for now.
          // wny not just return the whole thing?
          // Because I want to write a version of this that can
          // know, in near constant time whether dispatch would yield,
          // without actually producing a result set.
          // That method can be used by dispatch, peek, and throttle.
          // So keep the interface locked down.
          if (canSatisfy != Satisfy.No) {
            throw new Exception ();
          }
          runner.Yield (closure, new RCLong (result.Lines));
        }
      }
    }

    [RCVerb ("write")]
    public void EvalWrite (RCRunner runner, RCClosure closure, RCCube right)
    {
      Write (runner, closure, right, false);
    }

    [RCVerb ("force")]
    public void EvalForce (RCRunner runner, RCClosure closure, RCCube right)
    {
      Write (runner, closure, right, true);
    }

    protected void Write (RCRunner runner, RCClosure closure, RCCube right, bool force)
    {
      try
      {
        RCArray<RCSymbolScalar> symbols;
        long line;
        // Merge all waiters into this collection to avoid readers being
        // fired multiple times.
        if (right.Count == 0) {
          runner.Yield (closure, new RCLong (0));
          return;
        }
        RCSymbol symbol = new RCSymbol (right.Axis.Symbol);
        Dictionary<long, RCClosure> all = null;
        lock (_readWriteLock)
        {
          Section s = GetSection (symbol);
          symbols = s._blackboard.Write (s._counter, right, false, force, s._g);
          line = s._g + s._blackboard.Count;
          // write should always return the last G value and that G value needs
          // to be the correct one. This is not the case. Need to fix it.
          s._readWaiters.GetReadersForSymbol (ref all, symbols);
          s._dispatchWaiters.GetReadersForSymbol (ref all, symbols);
        }
        ContinueWaiters (runner, all);
        // I really want to see what was written including G and T and i cols.
        // Not only what was passed in.
        RCSystem.Log.Record (closure, "board", 0, "write", right);
        runner.Yield (closure, new RCLong (line));
      }
      catch (Exception)
      {
        throw;
      }
    }

    [RCVerb ("write")]
    public void EvalWrite (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      long line = Write (runner, left, right, false);
      RCBlock logBlock = new RCBlock (right, "S", ":", left);
      RCSystem.Log.Record (closure, "board", 0, "write", logBlock);
      runner.Yield (closure, new RCLong (line));
    }

    [RCVerb ("force")]
    public void EvalForce (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      long line = Write (runner, left, right, true);
      RCBlock logBlock = new RCBlock (right, "S", ":", left);
      RCSystem.Log.Record (closure, "board", 0, "write", logBlock);
      runner.Yield (closure, new RCLong (line));
    }

    protected long Write (RCRunner runner, RCSymbol symbol, RCBlock block, bool force)
    {
      RCArray<RCSymbolScalar> symbols;
      long result;
      // Merge all waiters into this collection to avoid readers being
      // fired multiple times.  May never be allocated if no one is listening.
      Dictionary<long, RCClosure> all = null;
      lock (_readWriteLock)
      {
        Section s = GetSection (symbol);
        symbols = s._blackboard.Write (s._counter, symbol, block, s._g, force);
        result = s._blackboard.Count;
        // write should always return the last G value and that G value needs
        // to be the correct one. This is not the case. Need to fix it.
        s._readWaiters.GetReadersForSymbol (ref all, symbols);
        s._dispatchWaiters.GetReadersForSymbol (ref all, symbols);
      }
      ContinueWaiters (runner, all);
      return result;
    }

    [RCVerb ("block")]
    public void EvalBlock (RCRunner runner, RCClosure closure, RCCube right)
    {
      BlockWriter writer = new BlockWriter (right);
      RCBlock result = writer.Write ();
      runner.Yield (closure, result);
    }

    [RCVerb ("lines")]
    public void EvalLines (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCLong (right.Lines));
    }

    public RCBlock Dump ()
    {
      RCBlock result = RCBlock.Empty;
      lock (_readWriteLock)
      {
        foreach (KeyValuePair<object, Section> kv in _sections)
        {
          result = new RCBlock (
            result,
            kv.Key.ToString (),
            ":",
            kv.Value._blackboard);
        }
      }
      return result;
    }

    [RCVerb ("blackboard")]
    public void EvalBlackboard (RCRunner runner, RCClosure closure, RCBlock empty)
    {
      // Metadata about the blackboard contents.
      RCBlock result = RCBlock.Empty;
      lock (_readWriteLock)
      {
        RCBlock descriptor = RCBlock.Empty;
        foreach (KeyValuePair<object, Section> kv in _sections)
        {
          descriptor = new RCBlock (descriptor,
                                    "rows",
                                    ":",
                                    new RCLong (kv.Value._blackboard.Axis.Count));
          descriptor = new RCBlock (descriptor,
                                    "cols",
                                    ":",
                                    new RCLong (kv.Value._blackboard.Cols));
          result = new RCBlock (result,
                                kv.Key.ToString (),
                                ":",
                                descriptor);
        }
      }
      runner.Yield (closure, result);
    }

    public class MapQueue
    {
      // The waiting fibers by symbol.
      protected Dictionary<RCSymbolScalar, HashSet<long>> _fibersBySymbol =
        new Dictionary<RCSymbolScalar, HashSet<long>> ();
      // The symbols being waited for by fiber.
      protected Dictionary<long, HashSet<RCSymbolScalar>> _symbolsByFiber =
        new Dictionary<long, HashSet<RCSymbolScalar>> ();
      // The order in which the waiters arrived.
      protected Queue<RCClosure> _waitOrder = new Queue<RCClosure> ();

      public void Abort (RCRunner runner)
      {
        while (_waitOrder.Count > 0)
        {
          RCClosure closure = _waitOrder.Dequeue ();
          runner.Finish (closure, new Exception ("Canceled"), 2);
        }
      }

      public void Enqueue (RCSymbol symbol, RCClosure closure)
      {
        if (_symbolsByFiber.ContainsKey (closure.Fiber)) {
          throw new Exception ("Fiber " + closure.Fiber.ToString () +
                               " is already waiting, something is wrong.");
        }
        RCArray<RCSymbolScalar> stripped = new RCArray<RCSymbolScalar> (symbol.Count);
        for (int i = 0; i < symbol.Count; ++i)
        {
          if (symbol[i].Key.Equals ("*")) {
            stripped.Write (symbol[i].Previous);
          }
          else {
            stripped.Write (symbol[i]);
          }
        }
        _symbolsByFiber.Add (closure.Fiber,
                              new HashSet<RCSymbolScalar> (stripped));
        for (int i = 0; i < stripped.Count; ++i)
        {
          HashSet<long> fibers;
          if (!_fibersBySymbol.TryGetValue (stripped[i], out fibers)) {
            _fibersBySymbol[stripped[i]] = fibers = new HashSet<long> ();
          }
          fibers.Add (closure.Fiber);
        }
        _waitOrder.Enqueue (closure);
      }

      public Queue<RCClosure> Dequeue (RCSymbolScalar scalar)
      {
        Queue<RCClosure> result = null;
        HashSet<long> candidates = null;
        HashSet<long> fibers = null;
        while (scalar != null)
        {
          _fibersBySymbol.TryGetValue (scalar, out fibers);
          if (fibers != null) {
            if (candidates == null) {
              candidates = new HashSet<long> ();
            }
            candidates.UnionWith (fibers);
          }
          scalar = scalar.Previous;
        }
        if (candidates == null) {
          return null;
        }
        int count = _waitOrder.Count;
        for (int i = 0; i < count; ++i)
        {
          RCClosure next = _waitOrder.Dequeue ();
          if (candidates.Contains (next.Fiber)) {
            if (result == null) {
              result = new Queue<RCClosure> ();
            }
            HashSet<RCSymbolScalar> symbols = _symbolsByFiber[next.Fiber];
            _symbolsByFiber.Remove (next.Fiber);
            foreach (RCSymbolScalar symbol in symbols)
            {
              _fibersBySymbol[symbol].Remove (next.Fiber);
            }
            result.Enqueue (next);
          }
          else {
            // Put it back in the queue, this preserves the original priority
            // because we go all the way through the queue each time.
            _waitOrder.Enqueue (next);
          }
        }
        return result;
      }

      public void GetReadersForSymbol (ref Dictionary<long, RCClosure> fibers,
                                       RCArray<RCSymbolScalar> symbol)
      {
        Queue<RCClosure> symbolWaiters;
        for (int i = 0; i < symbol.Count; ++i)
        {
          // It is possible to have the same closure represented more than once if the
          // operator requested more than one symbol.  This is deduping the closures
          // based on the fiber number, if necessary.
          // But maybe this whole thing should be part of MapQueue.Dequeue for clarity.
          if ((symbolWaiters = Dequeue (symbol[i])) != null) {
            while (symbolWaiters.Count > 0)
            {
              RCClosure waiter = symbolWaiters.Dequeue ();
              if (fibers == null) {
                fibers = new Dictionary<long, RCClosure> ();
              }
              fibers[waiter.Fiber] = waiter;
            }
          }
        }
      }
    }
  }
}

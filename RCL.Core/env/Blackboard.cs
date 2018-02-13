
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  //Implements concurrency control and communication between fibers.
  public class Blackboard
  {
    protected readonly object m_readWriteLock = new object ();
    protected Dictionary<object, Section> m_sections = new Dictionary<object, Section> ();

    public Blackboard () {}

    protected class Section
    {
      internal long m_g = 0;
      internal MapQueue m_readWaiters = new MapQueue ();
      internal MapQueue m_dispatchWaiters = new MapQueue ();
      internal MapQueue m_throttleWaiters = new MapQueue ();
      internal Dictionary<RCSymbolScalar, long> m_dispatchLines = new Dictionary<RCSymbolScalar, long> ();
      internal ReadCounter m_counter = new ReadCounter ();
      internal RCCube m_blackboard = new RCCube (new RCArray<string> ("G", "E", "S"));
      //internal RCCube m_blackboard = new RCCube (new RCArray<string> ("G", "S"));

      internal void Clear ()
      {
        //g gets updated...
        m_g += m_blackboard.Count;
        //blackboard cube is dumped.
        m_blackboard = new RCCube (new RCArray<string> ("G", "E", "S"));
        //Uncomment this to disable the E column I think
        //I want it to be G or E in the blackboard, not both
        //m_blackboard = new RCCube (new RCArray<string> ("G", "S"));
        //waiters keep on waiting.
        //But what about m_counter?
        //Shouldn't we just reset it?
        m_counter = new ReadCounter ();
      }
    }

    protected Section GetSection (RCSymbol symbol)
    {
      //This assumes that all symbols in symbol have the same first part!
      object key = symbol[0].Part (0);
      Section s;
      if (!m_sections.TryGetValue (key, out s))
      {
        s = new Section ();
        m_sections[key] = s;
      }
      return s;
    }

    [RCVerb ("read")]
    public void EvalRead (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      try
      {
        Section section = GetSection (left);
        Read (runner, closure, left, new ReadSpec (section.m_counter, left, right, 0, false, false, true, false));
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
        Read (runner, closure, right, new ReadSpec (section.m_counter, right, args, 0, false, false, true, false));
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
        //This read is different from all others in that it has force on and fill off.
        //force causes duplicate values to be written.
        //fill causes prior values to be filled into the results.
        //This read gives you exactly what exists in the blackboard.
        Section section = GetSection (right);
        Read (runner, closure, right, new ReadSpec (section.m_counter, right, left, 0, false, true, false, true));
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
        //This read is different from all others in that it has force on and fill off.
        //force causes duplicate values to be written.
        //fill causes prior values to be filled into the results.
        //This read gives you exactly what exists in the blackboard.
        RCLong args = new RCLong (0, 0);
        Section section = GetSection (right);
        Read (runner, closure, right, new ReadSpec (section.m_counter, right, args, 0, false, true, false, true));
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
      lock (m_readWriteLock)
      {
        //Make abstract symbols concrete
        Section section = GetSection (symbol);
        //Console.Out.WriteLine ("BEFORE SYM: " + symbol);
        symbol = section.m_counter.ConcreteSymbols (symbol, spec.ShowDeleted);
        //Console.Out.WriteLine ("AFTER SYM: " + symbol);
        Satisfy canSatisfy = section.m_counter.CanSatisfy (spec);
        RCCube result = section.m_blackboard.Read (spec,
                                                   section.m_counter,
                                                   true,
                                                   section.m_blackboard.Count);
        if (spec.SymbolUnlimited)
        {
          if (result.Count > 0)
          {
            //Notice the canSatisfy constraints here are less strict.
            //If the start point is greater than zero then we have to
            //do the full read and then just see if there were enough
            //rows to satisfy the constraints.
            if (canSatisfy == Satisfy.No)
            {
              throw new Exception ();
            }
            runner.Yield (closure, result);
          }
          else
          {
            if (canSatisfy == Satisfy.Yes)
            {
              throw new Exception ();
            }
            section.m_readWaiters.Enqueue (symbol, closure);
          }
        }
        else
        {
          if (result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit))
          {
            if (canSatisfy == Satisfy.No)
            {
              throw new Exception ();
            }
            runner.Yield (closure, result);
          }
          else
          {
            if (canSatisfy == Satisfy.Yes)
            {
              throw new Exception ();
            }
            section.m_readWaiters.Enqueue (symbol, closure);
          }
        }
      }
    }

    [RCVerb ("peek")]
    public void EvalPeek (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      int limit = (int) right[0];
      lock (m_readWriteLock)
      {
        Section s = GetSection (left);
        ReadSpec spec = s.m_counter.GetReadSpec (left, limit, false, true);
        Satisfy canSatisfy = s.m_counter.CanSatisfy (spec);
        RCCube result = s.m_blackboard.Read (spec, s.m_counter, true, s.m_blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= left.Count * Math.Abs (spec.SymbolLimit)))
        {
          //If dispatch would yield, return lines.
          if (canSatisfy != Satisfy.Yes)
          {
            throw new Exception ();
          }
          runner.Yield (closure, RCBoolean.True);
        }
        else
        {
          if (canSatisfy != Satisfy.No)
          {
            throw new Exception ();
          }
          runner.Yield (closure, RCBoolean.False);
        }
      }
    }

    [RCVerb ("last")]
    public void EvalLast (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      //right now last defaults to -1 and read defaults to long.MaxValue; thats kind of weird.
      //but seems to fit with the way i use it those operators.
      Section section = GetSection (left);
      Read (runner, closure, left, new ReadSpec (section.m_counter, left, right, -1, false, false, true, false));
    }

    [RCVerb ("gawk")]
    public void EvalGawk (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int limit = (int) right[0];
      lock (m_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s.m_counter.GetReadSpec (symbol, limit, false, true);
        Satisfy canSatisfy = s.m_counter.CanSatisfy (spec);
        RCCube result = s.m_blackboard.Read (spec, s.m_counter, true, s.m_blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit)))
        {
          if (canSatisfy != Satisfy.Yes)
          {
            throw new Exception ();
          }
          runner.Yield (closure, result);
        }
        else
        {
          if (canSatisfy != Satisfy.No)
          {
            throw new Exception ();
          }
          s.m_dispatchWaiters.Enqueue (symbol, closure);
        }
      }
    }

    [RCVerb ("poll")]
    public void EvalPoll (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong starts)
    {
      //Poll is like read but it will never block.
      lock (m_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s.m_counter.GetReadSpec (symbol, starts, false, true);
        RCCube result = s.m_blackboard.Read (spec, s.m_counter, true, s.m_blackboard.Count);
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("snap")]
    public void EvalSnap (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      //With snap, the read instructions apply to each symbol in a family of symbols.
      //Not only to the symbol explcitly noted in the left argument.
      lock (m_readWriteLock)
      {
        Section s = GetSection (left);
        //This line with the ConcreteSymbols should be added to other operators
        //RCSymbol concretes = s.m_counter.ConcreteSymbols (left, false);
        ReadSpec spec = new ReadSpec (s.m_counter, left, right, -1, false, false, true, false);
        //RCCube result = s.m_blackboard.Read (spec, s.m_counter, true, s.m_blackboard.Count);
        RCCube result = s.m_blackboard.Read (spec, s.m_counter, true, s.m_blackboard.Count);
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("page")]
    public void EvalPage (RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int pageNumber = 0;
      if (right.Count > 0)
      {
        pageNumber = (int) right[0];
      }
      int pageSize = int.MaxValue;
      if (right.Count > 1)
      {
        pageSize = (int) right[1];
      }

      //Page let's you access the blackboard by page number and page size, rather than row numbers.
      //Good for building tools for looking at blackboard contents.
      lock (m_readWriteLock)
      {
        Section s = GetSection (symbol);
        int skipFirst = pageNumber * pageSize;
        int stopAfter = pageSize;
        ReadSpec spec = new ReadSpec (s.m_counter, symbol, skipFirst, stopAfter, false);
        RCCube result = s.m_blackboard.Read (spec, s.m_counter, true, s.m_blackboard.Count);
        runner.Yield (closure, result);
      }
    }

    [RCVerb ("clear")]
    public void EvalClear (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      lock (m_readWriteLock)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          string name = right [i].Part (0).ToString ();
          Section section;
          if (m_sections.TryGetValue (name, out section))
          {
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
      lock (m_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s.m_counter.GetReadSpec (symbol, limit, false, true);
        Satisfy canSatisfy = s.m_counter.CanSatisfy (spec);
        RCCube result = s.m_blackboard.Read (spec,
                                             s.m_counter, true, s.m_blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit)))
        {
          if (canSatisfy != Satisfy.Yes)
          {
            throw new Exception ();
          }
          s.m_counter.Dispatch (s.m_blackboard, result.AcceptedLines);
          runner.Yield (closure, result);
          Dictionary<long, RCClosure> throttlers = null;
          s.m_throttleWaiters.GetReadersForSymbol (ref throttlers,
                                                   result.AcceptedSymbols);
          ContinueWaiters (runner, throttlers);
        }
        else
        {
          if (canSatisfy != Satisfy.No)
          {
            throw new Exception ();
          }
          s.m_dispatchWaiters.Enqueue (symbol, closure);
        }
      }
    }

    public void ContinueWaiters (RCRunner runner, Dictionary<long, RCClosure> all)
    {
      if (all != null)
      {
        foreach (RCClosure waiter in all.Values)
        {
          runner.Continue (null, waiter);
        }
      }
    }

    [RCVerb ("throttle")]
    public void EvalThrottle (
      RCRunner runner, RCClosure closure, RCSymbol symbol, RCLong right)
    {
      int limit = (int) right[0];
      lock (m_readWriteLock)
      {
        Section s = GetSection (symbol);
        ReadSpec spec = s.m_counter.GetReadSpec (symbol, limit, false, true);
        Satisfy canSatisfy = s.m_counter.CanSatisfy (spec);
        RCCube result = s.m_blackboard.Read (spec,
                                             s.m_counter, true, s.m_blackboard.Count);

        if ((spec.SymbolUnlimited && result.Count > 0) ||
            (!spec.SymbolUnlimited && result.Count >= symbol.Count * Math.Abs (spec.SymbolLimit)))
        {
          //If dispatch would yield, suspend.
          if (canSatisfy != Satisfy.Yes)
          {
            throw new Exception ();
          }
          s.m_throttleWaiters.Enqueue (symbol, closure);
        }
        else
        {
          //Return the count of the result set for now.
          //wny not just return the whole thing?
          //Because I want to write a version of this that can
          //know, in near constant time whether dispatch would yield,
          //without actually producing a result set.
          //That method can be used by dispatch, peek, and throttle.
          //So keep the interface locked down.
          if (canSatisfy != Satisfy.No)
          {
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
        //Merge all waiters into this collection to avoid readers being
        //fired multiple times.
        if (right.Count == 0)
        {
          runner.Yield (closure, new RCLong (0));
          return;
        }
        RCSymbol symbol = new RCSymbol (right.Axis.Symbol);
        Dictionary<long, RCClosure> all = null;
        lock (m_readWriteLock)
        {
          Section s = GetSection (symbol);
          symbols = s.m_blackboard.Write (s.m_counter, right, false, force, s.m_g);
          line = s.m_g + s.m_blackboard.Count;
          s.m_readWaiters.GetReadersForSymbol (ref all, symbols);
          s.m_dispatchWaiters.GetReadersForSymbol (ref all, symbols);
        }
        ContinueWaiters (runner, all);
        //I really want to see what was written including G and T and i cols.
        //Not only what was passed in.
        runner.Log.Record (runner, closure, "board", 0, "write", right);
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
      runner.Log.Record (runner, closure, "board", 0, "write", logBlock);
      runner.Yield (closure, new RCLong (line));
    }

    [RCVerb ("force")]
    public void EvalForce (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      long line = Write (runner, left, right, true);
      RCBlock logBlock = new RCBlock (right, "S", ":", left);
      runner.Log.Record (runner, closure, "board", 0, "write", logBlock);
      runner.Yield (closure, new RCLong (line));
    }

    protected long Write (RCRunner runner, RCSymbol symbol, RCBlock block, bool force)
    {
      RCArray<RCSymbolScalar> symbols;
      long result;
      //Merge all waiters into this collection to avoid readers being
      //fired multiple times.  May never be allocated if no one is listening.
      Dictionary<long, RCClosure> all = null;
      lock (m_readWriteLock)
      {
        Section s = GetSection (symbol);
        symbols = s.m_blackboard.Write (s.m_counter, symbol, block, s.m_g, force);
        result = s.m_blackboard.Count;
        s.m_readWaiters.GetReadersForSymbol (ref all, symbols);
        s.m_dispatchWaiters.GetReadersForSymbol (ref all, symbols);
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
    public void EvalLines (
      RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCLong (right.Lines));
    }

    public RCBlock Dump ()
    {
      RCBlock result = RCBlock.Empty;
      lock (m_readWriteLock)
      {
        foreach (KeyValuePair<object, Section> kv in m_sections)
        {
          result = new RCBlock (
            result, kv.Key.ToString (), ":", kv.Value.m_blackboard);
        }
      }
      return result;
    }

    [RCVerb ("blackboard")]
    public void EvalBlackboard (RCRunner runner, RCClosure closure, RCBlock empty)
    {
      //Metadata about the blackboard contents.
      RCBlock result = RCBlock.Empty;
      lock (m_readWriteLock)
      {
        RCBlock descriptor = RCBlock.Empty;
        foreach (KeyValuePair<object, Section> kv in m_sections)
        {
          descriptor = new RCBlock (descriptor,
                                    "rows", ":", new RCLong (kv.Value.m_blackboard.Axis.Count));
          descriptor = new RCBlock (descriptor,
                                    "cols", ":", new RCLong (kv.Value.m_blackboard.Cols));
          result = new RCBlock (result,
                                kv.Key.ToString (), ":", descriptor);
        }
      }
      runner.Yield (closure, result);
    }

    public class MapQueue
    {
      //The waiting fibers by symbol.
      protected Dictionary<RCSymbolScalar, HashSet<long>> m_fibersBySymbol =
        new Dictionary<RCSymbolScalar, HashSet<long>>();
      //The symbols being waited for by fiber.
      protected Dictionary<long, HashSet<RCSymbolScalar>> m_symbolsByFiber =
        new Dictionary<long, HashSet<RCSymbolScalar>>();
      //The order in which the waiters arrived.
      protected Queue<RCClosure> m_waitOrder = new Queue<RCClosure> ();

      public void Abort (RCRunner runner)
      {
        while (m_waitOrder.Count > 0)
        {
          RCClosure closure = m_waitOrder.Dequeue ();
          runner.Finish (closure, new Exception ("Canceled"), 2);
        }
      }

      public void Enqueue (RCSymbol symbol, RCClosure closure)
      {
        if (m_symbolsByFiber.ContainsKey(closure.Fiber))
        {
          throw new Exception ("Fiber " + closure.Fiber.ToString () +
                              " is already waiting, something is wrong.");
        }
        RCArray<RCSymbolScalar> stripped = new RCArray<RCSymbolScalar> (symbol.Count);
        for (int i = 0; i < symbol.Count; ++i)
        {
          if (symbol[i].Key.Equals ("*"))
          {
            stripped.Write (symbol[i].Previous);
          }
          else
          {
            stripped.Write (symbol[i]);
          }
        }
        m_symbolsByFiber.Add (closure.Fiber,
                              new HashSet<RCSymbolScalar> (stripped));
        for (int i = 0; i < stripped.Count; ++i)
        {
          HashSet<long> fibers;
          if (!m_fibersBySymbol.TryGetValue (stripped[i], out fibers))
          {
            m_fibersBySymbol[stripped[i]] = fibers = new HashSet<long> ();
          }
          fibers.Add (closure.Fiber);
        }
        m_waitOrder.Enqueue (closure);
      }

      public Queue<RCClosure> Dequeue (RCSymbolScalar scalar)
      {
        Queue<RCClosure> result = null;
        HashSet<long> candidates = null;
        HashSet<long> fibers = null;
        while (scalar != null)
        {
          m_fibersBySymbol.TryGetValue (scalar, out fibers);
          if (fibers != null)
          {
            if (candidates == null)
            {
              candidates = new HashSet<long> ();
            }
            candidates.UnionWith (fibers);
          }
          scalar = scalar.Previous;
        }
        if (candidates == null)
        {
          return null;
        }
        int count = m_waitOrder.Count;
        for (int i = 0; i < count; ++i)
        {
          RCClosure next = m_waitOrder.Dequeue ();
          if (candidates.Contains (next.Fiber))
          {
            if (result == null)
            {
              result = new Queue<RCClosure> ();
            }
            HashSet<RCSymbolScalar> symbols = m_symbolsByFiber[next.Fiber];
            m_symbolsByFiber.Remove (next.Fiber);
            foreach (RCSymbolScalar symbol in symbols)
            {
              m_fibersBySymbol[symbol].Remove (next.Fiber);
            }
            result.Enqueue (next);
          }
          else
          {
            //Put it back in the queue, this preserves the original priority
            //because we go all the way through the queue each time.
            m_waitOrder.Enqueue (next);
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
          //It is possible to have the same closure represented more than once if the
          //operator requested more than one symbol.  This is deduping the closures
          //based on the fiber number, if necessary.
          //But maybe this whole thing should be part of MapQueue.Dequeue for clarity.
          if ((symbolWaiters = Dequeue (symbol[i])) != null)
          {
            while (symbolWaiters.Count > 0)
            {
              RCClosure waiter = symbolWaiters.Dequeue ();
              if (fibers == null)
              {
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

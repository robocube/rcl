
using System;
using System.Threading;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  [RCModule]
  //The RCModule attribute causes a state to be created even though there are no RCVerb attributes on any of the
  //methods in this class.  This makes it possible for TakeOperator to manipulate the state of Take the module.
  //Take is unique in requiring this complicated setup.
  //That is because it is both a control structure (overrides the Next()) and it needs to manipulate mutable
  //state IN the Next().
  //In the simpler cases like Each, we get away with using the same class as the Module and the operator.
  //But it is important to note that Eval will get called on the Module instance and Next() will get called on the operator instance.
  //This matters a lot if you are looking after mutable state in the Module.
  public class Take
  {
    //The fibers waiting to take each symbol.
    public Dictionary<RCSymbolScalar, HashSet<long>> m_takeFibers = new Dictionary<RCSymbolScalar, HashSet<long>> ();
    //The order in which the take commands were issued.
    public List<RCClosure> m_takeOrder = new List<RCClosure> ();
    //Any access to m_takenSymbols, m_takeFibers or m_takeOrder needs m_takeLock.
    public readonly object m_takeLock = new object ();
    //The fibers that have taken each symbol.
    public Dictionary<RCSymbolScalar, long> m_takeSymbols = new Dictionary<RCSymbolScalar, long> ();

    public class TakeOperator : RCOperator
    {
      [RCVerb ("take")]
      public void EvalTake (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
      {
        RCBot bot = runner.GetBot (closure.BotId);
        Take module = (Take) bot.GetModule (typeof (Take));
        module.DoTake (runner, closure, left, right);
      }
  
      public override RCClosure Next (RCRunner runner, RCClosure tail, RCClosure previous, RCValue result)
      {
        if (previous.Index < 2)
        {
          return base.Next (runner, tail, previous, result);
        }
        else
        {
          RCBot bot = runner.GetBot (tail.BotId);
          Take module = (Take) bot.GetModule (typeof (Take));
          module.Untake (runner, tail);
          return base.Next (runner, tail, previous, result);
        }
      }
  
      public override bool IsLastCall (RCClosure closure, RCClosure arg)
      {
        if (arg == null)
        {
          return base.IsLastCall (closure, arg);
        }
        return arg.Code.IsLastCall (arg, null);
      }
    }

    public void DoTake (RCRunner runner, RCClosure closure, RCSymbol symbols, RCValue section)
    {
      RCClosure next = new RCClosure (
        closure.BotId, closure.Fiber, symbols,
        closure, section, closure.Left,
        closure.Parent != null ? closure.Parent.Result : null, 0,
        closure.UserOp, closure.UserOpContext);

      lock (m_takeLock)
      {
        if (TryGetLocks (next))
        {
          //Start evaluating the critical section.
          runner.Continue (null, next);
        }
        else
        {
          //Record the order in which the waiters arrived.
          m_takeOrder.Add (next);
          
          //Remember that we want the lock when the other guy releases it.
          for (int i = 0; i < symbols.Count; ++i)
          {
            HashSet<long> fibers;
            if (!m_takeFibers.TryGetValue (symbols[i], out fibers))
            {
              fibers = new HashSet<long> ();
              m_takeFibers[symbols[i]] = fibers;
            }
            fibers.Add (closure.Fiber);
          }
        }
      }
    }

    public void Untake (RCRunner runner, RCClosure head)
    {
      if (head == null)
        throw new Exception ("Head closure was missing.");

      if (head.Locks == null)
        throw new Exception (
          "There were supposed to be locks on this closure.");

      lock (m_takeLock)
      {
        //Free all of the locks.
        for (int i = 0; i < head.Locks.Count; ++i)
        {
          m_takeSymbols.Remove (head.Locks[i]);
        }

        //List of all fibers waiting for any of the symbols just released.
        HashSet<long> candidates = new HashSet<long> ();
        for (int i = 0; i < head.Locks.Count; ++i)
        {
          HashSet<long> waiters;
          if (m_takeFibers.TryGetValue (head.Locks[i], out waiters))
          {
            candidates.UnionWith (waiters);
          }
        }

        //Go through the waiters in the order they came in.
        //Can any of them go now?
        for (int i = 0; i < m_takeOrder.Count; ++i)
        {
          RCClosure candidate = m_takeOrder[i];
          if (TryGetLocks (candidate))
          {
            //A Little ineffecient but whatevs.
            //In MapQueue I used a queue instead of a list to avoid the O(n) removals.
            //This makes the overall algorithm linear instead of quadratic.
            m_takeOrder.RemoveAt (i);
            runner.Continue (null, candidate);
            break;
          }
        }
      }
    }

    protected bool TryGetLocks (RCClosure closure)
    {
      bool allfree = true;
      for (int i = 0; i < closure.Locks.Count; ++i)
      {
        long holder;
        if (m_takeSymbols.TryGetValue (closure.Locks[i], out holder))
        {
          if (holder != closure.Fiber)
          {
            allfree = false;
            break;
          }
        }
      }
      if (allfree)
      {
        for (int i = 0; i < closure.Locks.Count; ++i)
        {
          m_takeSymbols[closure.Locks[i]] = closure.Fiber;
        }
      }
      return allfree;
    }
  }
}


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class TcpOutBox
  {
    protected object m_lock = new object ();
    protected Queue<RCAsyncState> m_outbox = new Queue<RCAsyncState> ();

    public bool Add (RCAsyncState state)
    {
      //Note the thing stays in the outbox until the object has been sent
      //and OutboxRemove has been called.
      lock (m_lock)
      {
        m_outbox.Enqueue (state);
        //Console.Out.WriteLine ("Outbox has {0}, {1} total", ((SendState)state.Other).Id, m_outbox.Count);
        return m_outbox.Count == 1;
      }
    }

    public RCAsyncState Remove ()
    {
      lock (m_lock)
      {
        //Remove the last item.
        m_outbox.Dequeue ();
        //Console.Out.WriteLine ("Outbox removed {0}, {1} remaining", ((SendState)prior.Other).Id, m_outbox.Count);
        //If not empty then start sending the next one.
        if (m_outbox.Count > 0)
        {
          return m_outbox.Peek ();
        }
        else return null;
      }
    }
  }
}

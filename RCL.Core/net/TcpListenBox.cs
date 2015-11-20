
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
  public class TcpListenBox
  {
    protected object m_lock = new object ();
    protected Queue<RCAsyncState> m_requests = new Queue<RCAsyncState> ();
    protected Queue<RCValue> m_messages = new Queue<RCValue> ();

    public void Add (RCRunner runner, RCValue message)
    {
      RCAsyncState state = null;
      lock (m_lock)
      {
        if (m_requests.Count > 0)
        {
          state = m_requests.Dequeue ();
        }
        else
        {
          m_messages.Enqueue (message);
        }
      }
      if (state != null)
      {
        runner.Yield (state.Closure, message);
      }
    }

    public void Remove (RCRunner runner, RCClosure closure)
    {
      RCValue message = null;
      lock (m_lock)
      {
        if (m_messages.Count > 0)
        {
          message = m_messages.Dequeue ();
        }
        else
        {
          m_requests.Enqueue (new RCAsyncState (runner, closure, null));
        }
      }
      if (message != null)
      {
        runner.Yield (closure, message);
      }
    }
  }
}
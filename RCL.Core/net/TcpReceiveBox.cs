
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
  public class TcpReceiveBox
  {
    protected readonly object m_lock = new object ();
    public readonly Dictionary<RCSymbolScalar, RCValue> m_replies =
      new Dictionary<RCSymbolScalar, RCValue> ();
    public readonly Dictionary<RCSymbolScalar, TcpCollector> m_waiters =
      new Dictionary<RCSymbolScalar, TcpCollector> ();

    public void Add (RCSymbolScalar id, RCValue message)
    {
      lock (m_lock)
      {
        TcpCollector waiter = null;
        // Console.Out.WriteLine ("Client message in:{0}", id);
        if (m_waiters.TryGetValue (id, out waiter)) {
          m_waiters.Remove (id);
          waiter.Accept (id, message);
        }
        else {
          m_replies.Add (id, message);
        }
      }
    }

    public void Remove (RCSymbolScalar id, TcpCollector waiter)
    {
      lock (m_lock)
      {
        RCValue message = null;
        if (m_replies.TryGetValue (id, out message)) {
          m_replies.Remove (id);
          waiter.Accept (id, message);
        }
        else {
          m_waiters.Add (id, waiter);
        }
      }
      // Console.Out.WriteLine ("Client waiting on {0}", new RCSymbol(new
      // RCArray<RCSymbolScalar>(m_waiters.Keys)));
    }
  }
}


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
    protected readonly object _lock = new object ();
    public readonly Dictionary<RCSymbolScalar, RCValue> _replies =
      new Dictionary<RCSymbolScalar, RCValue> ();
    public readonly Dictionary<RCSymbolScalar, TcpCollector> _waiters =
      new Dictionary<RCSymbolScalar, TcpCollector> ();

    public void Add (RCSymbolScalar id, RCValue message)
    {
      lock (_lock)
      {
        TcpCollector waiter = null;
        // Console.Out.WriteLine ("Client message in:{0}", id);
        if (_waiters.TryGetValue (id, out waiter)) {
          _waiters.Remove (id);
          waiter.Accept (id, message);
        }
        else {
          _replies.Add (id, message);
        }
      }
    }

    public void Remove (RCSymbolScalar id, TcpCollector waiter)
    {
      lock (_lock)
      {
        RCValue message = null;
        if (_replies.TryGetValue (id, out message)) {
          _replies.Remove (id);
          waiter.Accept (id, message);
        }
        else {
          _waiters.Add (id, waiter);
        }
      }
      // Console.Out.WriteLine ("Client waiting on {0}", new RCSymbol(new
      // RCArray<RCSymbolScalar>(_waiters.Keys)));
    }
  }
}

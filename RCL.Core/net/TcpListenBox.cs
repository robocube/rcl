
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
    protected object _lock = new object ();
    protected Queue<RCAsyncState> _requests = new Queue<RCAsyncState> ();
    protected Queue<RCValue> _messages = new Queue<RCValue> ();

    public void Add (RCRunner runner, RCValue message)
    {
      RCAsyncState state = null;
      lock (_lock)
      {
        if (_requests.Count > 0) {
          state = _requests.Dequeue ();
        }
        else {
          _messages.Enqueue (message);
        }
      }
      if (state != null) {
        runner.Yield (state.Closure, message);
      }
    }

    public void Remove (RCRunner runner, RCClosure closure)
    {
      RCValue message = null;
      lock (_lock)
      {
        if (_messages.Count > 0) {
          message = _messages.Dequeue ();
        }
        else {
          _requests.Enqueue (new RCAsyncState (runner, closure, null));
        }
      }
      if (message != null) {
        runner.Yield (closure, message);
      }
    }
  }
}
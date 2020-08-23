
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
    protected object _lock = new object ();
    protected Queue<RCAsyncState> _outbox = new Queue<RCAsyncState> ();

    public bool Add (RCAsyncState state)
    {
      // Note the thing stays in the outbox until the object has been sent
      // and OutboxRemove has been called.
      lock (_lock)
      {
        _outbox.Enqueue (state);
        // Console.Out.WriteLine ("Outbox has {0}, {1} total",
        // ((SendState)state.Other).Id,
        // _outbox.Count);
        return _outbox.Count == 1;
      }
    }

    public RCAsyncState Remove ()
    {
      lock (_lock)
      {
        // Remove the last item.
        _outbox.Dequeue ();
        // Console.Out.WriteLine ("Outbox removed {0}, {1} remaining",
        // ((SendState)prior.Other).Id,
        // _outbox.Count);
        // If not empty then start sending the next one.
        if (_outbox.Count > 0) {
          return _outbox.Peek ();
        }
        else {
          return null;
        }
      }
    }
  }
}

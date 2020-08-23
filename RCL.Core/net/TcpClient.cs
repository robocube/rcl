
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
  public class TcpClient : Tcp.Client
  {
    protected RCAsyncState _openState;
    protected string _host;
    protected long _port;
    protected long _handle;
    protected Socket _socket;
    protected IPHostEntry _ip;
    protected TcpMessageBuffer _buffer;
    protected TcpReceiveBox _inbox;
    protected TcpOutBox _outbox;
    protected int _cid = 0;
    protected Tcp.Protocol _protocol;
    protected int _timeout;
    protected Timer _timeoutTimer;

    public TcpClient (long handle, RCSymbolScalar symbol, Tcp.Protocol protocol, int timeout)
    {
      _protocol = protocol;
      _handle = handle;
      _host = (string) symbol.Part (1);
      _port = (long) symbol.Part (2);
      _inbox = new TcpReceiveBox ();
      _outbox = new TcpOutBox ();
      _buffer = new TcpMessageBuffer ();
      _timeout = timeout;
      _timeoutTimer = null;
    }

    public string Host
    {
      get { return _host; }
    }

    public long Port
    {
      get { return _port; }
    }

    public override void Open (RCRunner runner, RCClosure closure)
    {
      Dns.BeginGetHostEntry (_host,
                             new AsyncCallback (GetHostEntryCompleted),
                             new RCAsyncState (runner, closure, null));
    }

    protected void GetHostEntryCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      try
      {
        _ip = Dns.EndGetHostEntry (result);
        TryAddress (state, 0);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    protected void TryAddress (RCAsyncState state, int i)
    {
      IPAddress address = _ip.AddressList[i];
      _socket = new Socket (
        address.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);
      IPEndPoint end = new IPEndPoint (address, (int) _port);
      RCAsyncState statei = new RCAsyncState (state.Runner, state.Closure, i);
      _socket.BeginConnect (end, new AsyncCallback (ConnectCompleted), statei);
      _timeoutTimer = new Timer (CheckConnect, statei, _timeout, Timeout.Infinite);
    }

    protected void CheckConnect (object obj)
    {
      RCAsyncState state = (RCAsyncState) obj;
      try
      {
        if (!_socket.Connected) {
          _socket.Close ();
          int i = (int) state.Other;
          if (i >= _ip.AddressList.Length - 1) {
            state.Runner.Finish (state.Closure,
                                 new RCException (state.Closure,
                                                  RCErrors.Timeout,
                                                  "Giving up after timeout."),
                                 1);
          }
          else {
            _socket.Close ();
            TryAddress (state, i + 1);
          }
        }
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    protected void ConnectCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      int i = (int) state.Other;
      try
      {
        _socket.EndConnect (result);
        _openState = state;
        state.Runner.Yield (state.Closure, new RCLong (_handle));
        _socket.BeginReceive (_buffer.RecvBuffer,
                              _buffer.Read,
                              _buffer.RecvBuffer.Length,
                              SocketFlags.None,
                              new AsyncCallback (ReceiveCompleted),
                              null);
      }
      catch (Exception ex)
      {
        if (i >= _ip.AddressList.Length - 1) {
          state.Runner.Report (state.Closure, ex);
        }
        else {
          _socket.Close ();
          TryAddress (state, i + 1);
        }
      }
    }

    protected void ReceiveCompleted (IAsyncResult result)
    {
      int count = 0;
      try
      {
        count = _socket.EndReceive (result);
        if (count > 0) {
          long sid = -1;
          long ignore;
          RCBlock message = _buffer.CompleteReceive (_openState.Runner,
                                                     count,
                                                     _handle,
                                                     sid,
                                                     out ignore);
          RCSymbol id = (RCSymbol) message.Get ("id");
          // Console.Out.WriteLine ("Client receiving {0}, message:{1}", id, message);
          if (message != null) {
            _inbox.Add (id[0], message);
          }
          _socket.BeginReceive (
            _buffer.RecvBuffer,
            _buffer.Read,
            _buffer.RecvBuffer.Length - _buffer.Read,
            SocketFlags.None,
            new AsyncCallback (ReceiveCompleted),
            null);
        }
      }
      catch (Exception ex)
      {
        _openState.Runner.Report (_openState.Closure, ex);
      }
      finally
      {
        if (count == 0) {
          _socket.Close (1000);
          RCSystem.Log.Record (_openState.Closure, "socket", _handle, "closed", "");
        }
      }
    }

    public override void Close (RCRunner runner, RCClosure closure)
    {
      // Again, wtf is up with this timeout thingy.
      if (_socket != null) {
        _socket.Close (1000);
      }
    }

    public override TcpSendState Send (RCRunner runner, RCClosure closure, RCBlock message)
    {
      long cid = Interlocked.Increment (ref _cid);
      // Console.Out.WriteLine ("New Cid:{0}", cid);
      byte[] payload = _protocol.Serialize (this, message);
      TcpSendState correlation = new TcpSendState (_handle, cid, message);
      RCAsyncState state = new RCAsyncState (runner, closure, correlation);
      if (_outbox.Add (state)) {
        // Send will add the header to the payload.
        // This is something I will probably want to change by adding some kind of
        // serializer/formatter abstraction.
        int size = _buffer.PrepareSend (correlation.Id, payload);
        _socket.BeginSend (
          _buffer.SendBuffer,
          0,
          size,
          SocketFlags.None,
          new AsyncCallback (SendCompleted),
          state);
        // Console.Out.WriteLine ("Client sending {0}", correlation.Id);
      }
      else {
        // Console.Out.WriteLine ("Another message has to go first");
      }
      return correlation;
    }

    protected void SendCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      try
      {
        // Don't know what to do with the count returned... Anything?
        _socket.EndSend (result);
        // Dequeue the next item for sending on this channel.
        RCAsyncState next = _outbox.Remove ();
        if (next != null) {
          // Send will add the header to the payload.
          // This is something I will probably want to change by adding some kind of
          // serializer/formatter abstraction.
          TcpSendState correlation = (TcpSendState) next.Other;
          byte[] payload = _protocol.Serialize (this, correlation.Message);
          int size = _buffer.PrepareSend (correlation.Id, payload);
          _socket.BeginSend (_buffer.SendBuffer,
                              0,
                              size,
                              SocketFlags.None,
                              new AsyncCallback (SendCompleted),
                              state);
          // Console.Out.WriteLine ("Client sending {0}", correlation.Id);
        }
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    public override void Receive (TcpCollector gatherer, RCSymbolScalar id)
    {
      // It's not honoring the id here.
      // We should make sure to return the response to the appropriate
      // request, not rely on the order.
      // _inbox.Remove(runner, closure);
      _inbox.Remove (id, gatherer);
    }
  }
}

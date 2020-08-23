
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
  public class TcpServerSession
  {
    protected TcpServer _server;
    protected Socket _socket;
    protected RCAsyncState _listenState;
    protected long _handle;
    protected TcpMessageBuffer _buffer;
    protected TcpListenBox _inbox;
    protected TcpOutBox _outbox;

    public TcpServerSession (RCAsyncState state,
                             TcpServer server,
                             Socket socket,
                             TcpListenBox inbox,
                             long handle)
    {
      _listenState = state;
      _server = server;
      _socket = socket;
      _handle = handle;
      _buffer = new TcpMessageBuffer ();
      // One outbox per *session*
      _outbox = new TcpOutBox ();
      // One inbox per *server*
      _inbox = inbox;
    }

    public void Start ()
    {
      _socket.BeginReceive (_buffer.RecvBuffer,
                            _buffer.Read,
                            _buffer.RecvBuffer.Length - _buffer.Read,
                            SocketFlags.None,
                            new AsyncCallback (ReceiveCompleted),
                            null);
    }

    protected void ReceiveCompleted (IAsyncResult result)
    {
      int count = 0;
      try
      {
        count = _socket.EndReceive (result);
        if (count > 0) {
          long sid = _server.NextId (this);
          long cid;
          RCValue message = _buffer.CompleteReceive (_listenState.Runner,
                                                     count,
                                                     _server.Handle,
                                                     sid,
                                                     out cid);
          // Console.Out.WriteLine ("Server receiving {0}", cid);
          if (message != null) {
            _inbox.Add (_listenState.Runner, message);
          }
          _socket.BeginReceive (_buffer.RecvBuffer,
                                _buffer.Read,
                                _buffer.RecvBuffer.Length - _buffer.Read,
                                SocketFlags.None,
                                new AsyncCallback (ReceiveCompleted),
                                null);
        }
      }
      catch (Exception ex)
      {
        _listenState.Runner.Report (_listenState.Closure, ex);
        // Make sure we close the socket.
        count = 0;
      }
      finally
      {
        // What is this timeout garbage, shouldn't there be a BeginClose.
        // Do I need to create one?
        if (count == 0) {
          // We need to tell the server to remove this connection from the list.
          // No need to try and close the connection.  That is already done.
          _socket.Close (1000);
          RCSystem.Log.Record (_listenState.Closure, "socket", _handle, "closed", "");
        }
      }
    }

    public TcpSendState Send (RCRunner runner,
                              RCClosure closure,
                              long cid,
                              RCBlock message)
    {
      TcpSendState correlation = new TcpSendState (_handle, cid, message);
      RCAsyncState state = new RCAsyncState (runner, closure, correlation);
      byte[] payload = Encoding.ASCII.GetBytes (message.ToString ());
      // If other items are queued in the outbox Add will return false.
      // This message should be sent after the others.
      if (_outbox.Add (state)) {
        // Send will add the header to the payload.
        // This is something I will probably want to change by adding some kind of
        // serializer/formatter abstraction.
        int size = _buffer.PrepareSend (cid, payload);
        _socket.BeginSend (
          _buffer.SendBuffer,
          0,
          size,
          SocketFlags.None,
          new AsyncCallback (SendCompleted),
          state);
        // Console.Out.WriteLine ("Server sending {0}", correlation.Id);
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
        // if (next != null)
        RCAsyncState next = _outbox.Remove ();
        if (next != null) {
          TcpSendState correlation = (TcpSendState) next.Other;
          byte[] payload = Encoding.ASCII.GetBytes (correlation.Message.ToString ());
          int size = _buffer.PrepareSend (correlation.Id, payload);
          _socket.BeginSend (_buffer.SendBuffer,
                              0,
                              size,
                              SocketFlags.None,
                              new AsyncCallback (SendCompleted),
                              state);
          // Console.Out.WriteLine ("Server sending {0}", correlation.Id);
          // Send (next.Runner, next.Closure, other.Id, other.Message);
        }
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }
  }
}

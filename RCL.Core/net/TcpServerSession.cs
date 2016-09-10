
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
    protected TcpServer m_server;
    protected Socket m_socket;
    protected RCAsyncState m_listenState;
    protected long m_handle;
    protected TcpMessageBuffer m_buffer;
    protected TcpListenBox m_inbox;
    protected TcpOutBox m_outbox;

    public TcpServerSession (RCAsyncState state,
			     TcpServer server,
			     Socket socket,
			     TcpListenBox inbox,
			     long handle)
    {
      m_listenState = state;
      m_server = server;
      m_socket = socket;
      m_handle = handle;
      m_buffer = new TcpMessageBuffer ();
      //One outbox per *session*
      m_outbox = new TcpOutBox ();
      //One inbox per *server*
      m_inbox = inbox;
    }

    public void Start ()
    {
      m_socket.BeginReceive (m_buffer.RecvBuffer,
			     m_buffer.Read,
			     m_buffer.RecvBuffer.Length - m_buffer.Read,
			     SocketFlags.None,
			     new AsyncCallback(ReceiveCompleted),
			     null);
    }

    protected void ReceiveCompleted (IAsyncResult result)
    {
      int count = 0;
      try
      {
        count = m_socket.EndReceive(result);
        if (count > 0)
        {
          long sid = m_server.NextId (this);
          long cid;
          RCValue message = m_buffer.CompleteReceive (
            m_listenState.Runner, count, m_server.Handle, sid, out cid);
          //Console.Out.WriteLine ("Server receiving {0}", cid);
          if (message != null)
          {
            m_inbox.Add (m_listenState.Runner, message);
          }
          m_socket.BeginReceive (m_buffer.RecvBuffer,
                                 m_buffer.Read,
                                 m_buffer.RecvBuffer.Length - m_buffer.Read,
                                 SocketFlags.None,
                                 new AsyncCallback(ReceiveCompleted),
                                 null);
        }
      }
      catch (Exception ex)
      {
        m_listenState.Runner.Report (m_listenState.Closure, ex);
        //Make sure we close the socket.
        count = 0;
      }
      finally
      {
        //What is this timeout garbage, shouldn't there be a BeginClose.
        //Do I need to create one?
        if (count == 0)
        {
          //We need to tell the server to remove this connection from the list.
          //No need to try and close the connection.  That is already done.
          m_socket.Close (1000);
          m_listenState.Runner.Log.RecordDoc (m_listenState.Runner, 
                                              m_listenState.Closure, 
                                              "socket",
                                              m_handle,
                                              "closed",
                                              "");
        }
      }
    }

    public TcpSendState Send (RCRunner runner, 
                           RCClosure closure, 
                           long cid, 
                           RCBlock message)
    {
      TcpSendState correlation = new TcpSendState(m_handle, cid, message);
      RCAsyncState state = new RCAsyncState (runner, closure, correlation);
      byte[] payload = Encoding.ASCII.GetBytes (message.ToString ());
      //If other items are queued in the outbox Add will return false.
      //This message should be sent after the others.
      if (m_outbox.Add (state))
      {
        //Send will add the header to the payload.
        //This is something I will probably want to change by adding some kind of
        //serializer/formatter abstraction.
        int size = m_buffer.PrepareSend (cid, payload);
        m_socket.BeginSend (
          m_buffer.SendBuffer,
          0,
          size,
          SocketFlags.None,
          new AsyncCallback (SendCompleted),
          state);
        //Console.Out.WriteLine ("Server sending {0}", correlation.Id);
      }
      return correlation;
    }

    protected void SendCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      try
      {
        //Don't know what to do with the count returned... Anything?
        m_socket.EndSend (result);
        //Dequeue the next item for sending on this channel.
        //if (next != null)
        RCAsyncState next = m_outbox.Remove ();
        if (next != null)
        {
          TcpSendState correlation = (TcpSendState) next.Other;
          byte[] payload = Encoding.ASCII.GetBytes (correlation.Message.ToString ());
          int size = m_buffer.PrepareSend (correlation.Id, payload);
          m_socket.BeginSend (m_buffer.SendBuffer,
			      0,
			      size,
			      SocketFlags.None,
			      new AsyncCallback (SendCompleted),
			      state);
          //Console.Out.WriteLine ("Server sending {0}", correlation.Id);
          //Send (next.Runner, next.Closure, other.Id, other.Message);
        }
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }
  }
}
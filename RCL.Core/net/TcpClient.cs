
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
    protected RCAsyncState m_openState;
    protected string m_host;
    protected long m_port;
    protected long m_handle;
    protected Socket m_socket;
    protected IPHostEntry m_ip;
    protected TcpMessageBuffer m_buffer;
    protected TcpReceiveBox m_inbox;
    protected TcpOutBox m_outbox;
    protected int m_cid = 0;
    protected Tcp.Protocol m_protocol;

    public TcpClient (long handle, RCSymbolScalar symbol, Tcp.Protocol protocol)
    {
      m_protocol = protocol;
      m_handle = handle;
      m_host = (string) symbol.Part (1);
      m_port = (long) symbol.Part (2);
      m_inbox = new TcpReceiveBox ();
      m_outbox = new TcpOutBox ();
      m_buffer = new TcpMessageBuffer ();
    }

    public string Host
    {
      get { return m_host; }
    }

    public long Port
    {
      get { return m_port; }
    }

    public override void Open (RCRunner runner, RCClosure closure)
    {
      Dns.BeginGetHostEntry (
        m_host,
        new AsyncCallback (GetHostEntryCompleted),
        new RCAsyncState (runner, closure, null));
    }

    protected void GetHostEntryCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      try
      {
        m_ip = Dns.EndGetHostEntry (result);
        TryAddress (state, 0);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    protected void TryAddress (RCAsyncState state, int i)
    {
      IPAddress address = m_ip.AddressList[i];
      m_socket = new Socket (
        address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      IPEndPoint end = new IPEndPoint (address, (int) m_port);
      m_socket.BeginConnect (
        end,
        new AsyncCallback (ConnectCompleted),
        new RCAsyncState (state.Runner, state.Closure, i));
    }

    protected void ConnectCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      int i = (int) state.Other;
      try
      {
        m_socket.EndConnect (result);
        m_openState = state;
        state.Runner.Yield (state.Closure, new RCLong (m_handle));
        m_socket.BeginReceive (
          m_buffer.RecvBuffer,
          m_buffer.Read,
          m_buffer.RecvBuffer.Length,
          SocketFlags.None,
          new AsyncCallback (ReceiveCompleted),
          null);
      }
      catch (Exception ex)
      {
        if (i >= m_ip.AddressList.Length - 1)
        {
          state.Runner.Report (state.Closure, ex);
        }
        else
        {
          m_socket.Close ();
          TryAddress(state, i + 1);
        }
      }
    }

    protected void ReceiveCompleted (IAsyncResult result)
    {
      int count = 0;
      try
      {
        count = m_socket.EndReceive (result);
        if (count > 0)
        {
          long sid = -1;
          long ignore;
          RCBlock message = m_buffer.CompleteReceive (
            m_openState.Runner, count, m_handle, sid, out ignore);
          RCSymbol id = (RCSymbol) message.Get ("id");
          //Console.Out.WriteLine ("Client receiving {0}, message:{1}", id, message);
          if (message != null)
          {
            m_inbox.Add (id[0], message);
          }
          m_socket.BeginReceive (
            m_buffer.RecvBuffer,
            m_buffer.Read,
            m_buffer.RecvBuffer.Length - m_buffer.Read,
            SocketFlags.None,
            new AsyncCallback (ReceiveCompleted),
            null);
        }
      }
      catch (Exception ex)
      {
        m_openState.Runner.Report (m_openState.Closure, ex);
      }
      finally
      {
        if (count == 0)
        {
          Console.Out.WriteLine ("CLOSING SOCKET!?");
          m_socket.Close (1000);
        }
      }
    }

    public override void Close (RCRunner runner, RCClosure closure)
    {
      //Again, wtf is up with this timeout thingy.
      if (m_socket != null)
      {
        m_socket.Close (1000);
      }
    }

    public override TcpSendState Send (
      RCRunner runner, RCClosure closure, RCBlock message)
    {
      long cid = Interlocked.Increment (ref m_cid);
      //Console.Out.WriteLine ("New Cid:{0}", cid);
      byte[] payload = m_protocol.Serialize (this, message);
      TcpSendState correlation = new TcpSendState (m_handle, cid, message);
      RCAsyncState state = new RCAsyncState (runner, closure, correlation);
      if (m_outbox.Add (state))
      {
        //Send will add the header to the payload.
        //This is something I will probably want to change by adding some kind of
        //serializer/formatter abstraction.
        int size = m_buffer.PrepareSend (correlation.Id, payload);
        m_socket.BeginSend (
          m_buffer.SendBuffer, 0, size, SocketFlags.None,
          new AsyncCallback (SendCompleted),
          state);
        //Console.Out.WriteLine ("Client sending {0}", correlation.Id);
      }
      else
      {
        //Console.Out.WriteLine ("Another message has to go first");
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
        RCAsyncState next = m_outbox.Remove ();
        if (next != null)
        {
          //Send will add the header to the payload.
          //This is something I will probably want to change by adding some kind of
          //serializer/formatter abstraction.
          TcpSendState correlation = (TcpSendState) next.Other;
          byte[] payload = m_protocol.Serialize (this, correlation.Message);
          int size = m_buffer.PrepareSend (correlation.Id, payload);
          m_socket.BeginSend (
            m_buffer.SendBuffer, 0, size, SocketFlags.None,
            new AsyncCallback (SendCompleted),
            state);
          //Console.Out.WriteLine ("Client sending {0}", correlation.Id);
        }
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    public override void Receive (TcpCollector gatherer, RCSymbolScalar id)
    {
      //It's not honoring the id here.
      //We should make sure to return the response to the appropriate
      //request, not rely on the order.
      //m_inbox.Remove(runner, closure);
      m_inbox.Remove (id, gatherer);
    }
  }
}
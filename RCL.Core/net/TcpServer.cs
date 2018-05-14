
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
  public class TcpServer : Tcp.Server
  {
    protected long m_handle;
    protected Tcp.Protocol m_protocol;
    protected Socket m_listener;
    protected IPEndPoint m_ip;
    protected object m_lock = new object();
    protected Dictionary<long, TcpServerSession> m_clients;
    protected TcpListenBox m_inbox;
    protected long m_id = 0;
    protected Dictionary<long, TcpServerSession> m_requests;

    public long Handle { get { return m_handle; } }

    public TcpServer (long handle, long port, Tcp.Protocol protocol)
    {
      m_handle = handle;
      m_protocol = protocol;
      m_ip = new IPEndPoint (IPAddress.Any, (int)port);
      m_listener = new Socket (m_ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      m_clients = new Dictionary<long, TcpServerSession> ();
      m_requests = new Dictionary<long, TcpServerSession> ();
      m_inbox = new TcpListenBox ();
    }

    public override void Listen (RCRunner runner, RCClosure closure)
    {
      m_listener.Bind (m_ip);
      m_listener.Listen (int.MaxValue);
      m_listener.BeginAccept (new AsyncCallback (AcceptCompleted),
			      new RCAsyncState (runner, closure, null));
      runner.Yield (closure, new RCLong (m_handle));
    }

    protected void AcceptCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      try
      {
        Socket client = m_listener.EndAccept (result);
        RCBot bot = state.Runner.GetBot (state.Closure.BotId);
        //long handle = state.Closure.Bot.New ();
        long handle = bot.New ();
        TcpServerSession session = new TcpServerSession (state,
							 this,
							 client,
							 m_inbox,
							 handle);
        lock (m_lock)
        {
          m_clients.Add (handle, session);
        }
        session.Start ();
        m_listener.BeginAccept (new AsyncCallback (AcceptCompleted), state);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    public override void Accept (RCRunner runner, 
                                 RCClosure closure, 
                                 long limit)
    {
      if (limit != 1)
      {
        throw new Exception("limit has to be exactly 1 currently, sorry");
      }
      m_inbox.Remove (runner, closure);
    }

    public override TcpSendState Reply (RCRunner runner,
                                     RCClosure closure,
                                     long sid,
                                     long cid,
                                     RCBlock message)
    {
      TcpServerSession session;
      lock (m_lock)
      {
        session = m_requests[sid];
        m_requests.Remove (sid);
      }
      return session.Send (runner, closure, cid, message);
    }

    public long NextId (TcpServerSession session)
    {
      long id;
      lock (m_lock)
      {
        id = m_id++;
        m_requests.Add (id, session);
      }
      return id;
    }

    public override byte[] Serialize (RCValue message)
    {
      return m_protocol.Serialize (this, message);
    }
  }
}

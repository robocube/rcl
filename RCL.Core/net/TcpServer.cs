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
    protected long _handle;
    protected Tcp.Protocol _protocol;
    protected Socket _listener;
    protected IPEndPoint _ip;
    protected object _lock = new object ();
    protected Dictionary<long, TcpServerSession> _clients;
    protected TcpListenBox _inbox;
    protected long _id = 0;
    protected Dictionary<long, TcpServerSession> _requests;

    public long Handle {
      get { return _handle; }
    }

    public TcpServer (long handle, long port, Tcp.Protocol protocol)
    {
      _handle = handle;
      _protocol = protocol;
      _ip = new IPEndPoint (IPAddress.Any, (int) port);
      _listener = new Socket (_ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      _clients = new Dictionary<long, TcpServerSession> ();
      _requests = new Dictionary<long, TcpServerSession> ();
      _inbox = new TcpListenBox ();
    }

    public override void Listen (RCRunner runner, RCClosure closure)
    {
      _listener.Bind (_ip);
      _listener.Listen (int.MaxValue);
      _listener.BeginAccept (new AsyncCallback (AcceptCompleted),
                              new RCAsyncState (runner, closure, null));
      runner.Yield (closure, new RCLong (_handle));
    }

    protected void AcceptCompleted (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      try
      {
        Socket client = _listener.EndAccept (result);
        RCBot bot = state.Runner.GetBot (state.Closure.Bot);
        // long handle = state.Closure.Bot.New ();
        long handle = bot.New ();
        TcpServerSession session = new TcpServerSession (state,
                                                         this,
                                                         client,
                                                         _inbox,
                                                         handle);
        lock (_lock)
        {
          _clients.Add (handle, session);
        }
        session.Start ();
        _listener.BeginAccept (new AsyncCallback (AcceptCompleted), state);
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
      if (limit != 1) {
        throw new Exception ("limit has to be exactly 1 currently, sorry");
      }
      _inbox.Remove (runner, closure);
    }

    public override TcpSendState Reply (RCRunner runner,
                                        RCClosure closure,
                                        long sid,
                                        long cid,
                                        RCBlock message)
    {
      TcpServerSession session;
      lock (_lock)
      {
        session = _requests[sid];
        _requests.Remove (sid);
      }
      return session.Send (runner, closure, cid, message);
    }

    public long NextId (TcpServerSession session)
    {
      long id;
      lock (_lock)
      {
        id = _id++;
        _requests.Add (id, session);
      }
      return id;
    }

    public override byte[] Serialize (RCValue message)
    {
      return _protocol.Serialize (this, message);
    }
  }
}

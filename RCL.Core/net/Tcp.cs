
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
  public class Tcp
  {
    //I don't know what this crap is, it should die.
    //Let's just do binary with tcp always.
    //And text for http always.
    public abstract class Protocol
    {
      public abstract byte[] Serialize (Client client, RCValue message);
      public abstract byte[] Serialize (Server server, RCValue message);
    }

    public abstract class Server
    {
      public abstract void Listen (RCRunner runner, RCClosure closure);
      public abstract void Accept (RCRunner runner, RCClosure closure, long limit);
      public abstract TcpSendState Reply (RCRunner runner, RCClosure closure, long sid, long cid, RCBlock message);
      public abstract byte[] Serialize (RCValue message);
    }

    public abstract class Client
    {
      public abstract void Open (RCRunner runner, RCClosure closure);
      public abstract TcpSendState Send (RCRunner runner, RCClosure closure, RCBlock message);
      public abstract void Receive (TcpCollector gatherer, RCSymbolScalar token);
      public abstract void Close (RCRunner runner, RCClosure closure);
    }

    [RCVerb ("open")]
    public void EvalOpen (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      EvalOpen (runner, closure, new RCLong (5000), right);
    }

    [RCVerb ("open")]
    public void EvalOpen (RCRunner runner, RCClosure closure, RCLong left, RCSymbol right)
    {
      //Implementing multiple would require some annoying scatter gather logic.
      //Plus what if one fails out of the list?
      if (right.Count != 1)
      {
        throw new Exception ("open takes exactly one protocol,host,[port]");
      }
      if (left.Count != 1)
      {
        throw new Exception ("open takes a single timeout value");
      }

      RCSymbolScalar symbol = right[0];
      string protocol = (string) symbol.Part (0);
      int timeout = (int) left[0];
      //string host = (string) symbol[1];
      //long port = -1;
      //if (symbol.Length > 2)
      //  port = (long) symbol[2];
      RCBot bot = runner.GetBot (closure.Bot);
      long handle = bot.New ();
      Client client;
      if (protocol.Equals ("http"))
      {
        client = new TcpHttpClient (handle, symbol);
      }
      else if (protocol.Equals ("tcp"))
      {
        client = new TcpClient (handle, symbol, new TcpProtocol (), timeout);
      }
      else if (protocol.Equals ("udp"))
      {
        throw new NotImplementedException ("No udp sockets yet");
      }
      else if (protocol.Equals ("cube"))
      {
        throw new NotImplementedException ();
        //client = new CubeClient (handle, symbol);
      }
      else
      {
        throw new NotImplementedException ("Unknown protocol: " + protocol);
      }
      bot.Put (handle, client);
      client.Open (runner, closure);
    }

    [RCVerb ("listen")]
    public void EvalListen (RCRunner runner, RCClosure closure, RCLong right)
    {
      RCSymbolScalar symbol = new RCSymbolScalar (new RCSymbolScalar (null, "tcp"), right[0]);
      EvalListen (runner, closure, new RCSymbol(symbol));
    }

    [RCVerb ("listen")]
    public void EvalListen (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      if (right.Count != 1)
      {
        throw new Exception("listen takes exactly one protocol,port");
      }
      string protocol = (string) right[0].Part (0);
      long port = (long) right[0].Part (1);
      RCBot bot = runner.GetBot (closure.Bot);
      long handle = bot.New ();
      Server server;
      if (protocol.Equals ("http"))
      {
        throw new NotImplementedException ("http server not ready yet");
      }
      else if (protocol.Equals ("tcp"))
      {
        server = new TcpServer (handle, port, new TcpProtocol ());
      }
      else if (protocol.Equals ("udp"))
      {
        throw new ArgumentException ("No udp sockets yet");
      }
      else
      {
        throw new ArgumentException ("Unknown protocol: " + protocol);
      }
      bot.Put (handle, server);
      server.Listen (runner, closure);
    }

    [RCVerb ("close")]
    public void EvalClose (RCRunner runner, RCClosure closure, RCLong right)
    {
       //Implementing multiple would require some annoying scatter gather logic.
      //Plus what if one fails out of the list?
      if (right.Count != 1)
      {
        throw new Exception ("open takes exactly one protocol,host,port");
      }
      RCBot bot = runner.GetBot (closure.Bot);
      Client client = (Client) bot.Get (right[0]);
      client.Close (runner, closure);
      runner.Yield (closure, right);
    }

    [RCVerb ("send")]
    public void EvalSend (RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      DoSend (runner, closure, left, right);
    }

    protected void DoSend (RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      RCSymbolScalar[] result = new RCSymbolScalar[left.Count];
      RCBot bot = runner.GetBot (closure.Bot);
      for (int i = 0; i < left.Count; ++i)
      {
        Client client = (Client) bot.Get (left[i]);
        TcpSendState state = client.Send (runner, closure, right);
        RCSymbolScalar handle = new RCSymbolScalar (null, state.Handle);
        result[i] = new RCSymbolScalar (handle, state.Id);
      }
      runner.Yield (closure, new RCSymbol (result));
    }

    [RCVerb ("accept")]
    public void EvalAccept (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      DoAccept (runner, closure, left, right);
    }

    protected void DoAccept (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      RCBot bot = runner.GetBot (closure.Bot);
      for (int i = 0; i < left.Count; ++i)
      {
        Server server = (Server) bot.Get (left[i]);
        server.Accept (runner, closure, right[0]);
      }
    }

    [RCVerb ("reply")]
    public void EvalReply (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      DoReply (runner, closure, left, right);
    }

    protected void DoReply (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      RCBot bot = runner.GetBot (closure.Bot);
      RCSymbolScalar[] result = new RCSymbolScalar[left.Count];
      for (int i = 0; i < left.Count; ++i)
      {
        long channel = (long) left[i].Part (0);
        long sid = (long) left[i].Part (1);
        long cid = (long) left[i].Part (2);
        Server server = (Server) bot.Get (channel);
        TcpSendState state = server.Reply (runner, closure, sid, cid, right);
        RCSymbolScalar handle = new RCSymbolScalar (null, state.Handle);
        result[i] = new RCSymbolScalar (handle, state.Id);
      }
      runner.Yield (closure, new RCSymbol (result));
    }

    [RCVerb ("receive")]
    public void EvalReceive (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      DoReceive (runner, closure, right);
    }

    protected void DoReceive (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCBot bot = runner.GetBot (closure.Bot);
      TcpCollector gatherer = new TcpCollector (runner, closure, right);
      for (int i = 0; i < right.Count; ++i)
      {
        long channel = (long) right[i].Part (0);
        Client client = (Client) bot.Get (channel);
        client.Receive (gatherer, right[i]);
      }
    }
  }
}

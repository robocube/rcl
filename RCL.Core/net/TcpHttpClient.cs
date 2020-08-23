
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  // So now I have two http clients.
  public class TcpHttpClient : Tcp.Client
  {
    public enum HttpVerb
    {
      head, get, post, put, delete
    }

    protected readonly long _handle;
    protected readonly string _host;
    protected readonly long _port;
    protected readonly WebClient _client;
    // protected readonly Uri _uri;
    protected long _cid = 0;
    protected TcpReceiveBox _inbox;

    // I think host and port will get combined with "resource" as an RCSymbolScalar here.
    public TcpHttpClient (long handle, RCSymbolScalar hostandport)
    {
      _handle = handle;
      _host = (string) hostandport.Part (1);
      _port = (long) hostandport.Part (2);
      // _uri = new Uri("http://" + _host + ":" + _port.ToString ());
      _inbox = new TcpReceiveBox ();
      _client = new WebClient ();
      _client.UploadDataCompleted += UploadDataCompleted;
      _client.DownloadDataCompleted += DownloadDataCompleted;
    }

    public long Port
    {
      get { return _port; }
    }

    public string Host
    {
      get { return _host; }
    }

    public override void Open (RCRunner runner, RCClosure closure)
    {
      runner.Yield (closure, new RCLong (_handle));
    }

    public override void Close (RCRunner runner, RCClosure closure)
    {
      _client.Dispose ();
      runner.Yield (closure, new RCLong (_handle));
    }

    public override TcpSendState Send (RCRunner runner, RCClosure closure, RCBlock message)
    {
      long cid = Interlocked.Increment (ref _cid);
      RCSymbolScalar id = new RCSymbolScalar (null, _handle);
      id = new RCSymbolScalar (id, cid);

      StringBuilder address = new StringBuilder ();
      // HttpVerb verb = (HttpVerb) Enum.Parse (
      //  typeof(HttpVerb),
      //  ((RCSymbol) message.Get ("verb"))[0].Part (0).ToString ());
      object[] resource = ((RCSymbol) message.Get ("resource"))[0].ToArray ();

      address.Append ("http://");
      address.Append (_host);
      if (_port > 0) {
        address.Append (":");
        address.Append (_port);
      }
      address.Append ("/");

      for (int i = 0; i < resource.Length; ++i)
      {
        address.Append (resource[i].ToString ());
        if (i < resource.Length - 1) {
          address.Append ("/");
        }
      }

      RCBlock query = (RCBlock) message.Get ("query");
      if (query != null) {
        address.Append ("?");
        for (int i = 0; i < query.Count; ++i)
        {
          RCBlock variable = query.GetName (i);
          address.Append (variable.Name);
          address.Append ("=");
          address.Append (((RCString) variable.Value)[0]);
          if (i < query.Count - 1) {
            address.Append ("&");
          }
        }
      }

      // byte[] payload = _client.Encoding.GetBytes (message.Get ("body").ToString ());
      Uri uri = new Uri (address.ToString ());
      System.Console.Out.WriteLine (address.ToString ());
      _client.DownloadDataAsync (uri, new RCAsyncState (runner, closure, id));
      // runner.Yield (closure, new RCSymbol(id));
      return new TcpSendState (_handle, cid, message);
    }

    protected void UploadDataCompleted (object sender, UploadDataCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState) e.UserState;
      if (e.Error != null) {
        state.Runner.Report (state.Closure, e.Error);
      }
      RCSymbolScalar id = (RCSymbolScalar) state.Other;
      string text = _client.Encoding.GetString (e.Result);
      // TODO: make this into a real object with headers and body and stuff.
      _inbox.Add (id, new RCString (text));
    }

    protected void DownloadDataCompleted (object sender, DownloadDataCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState) e.UserState;
      if (e.Error != null) {
        state.Runner.Report (state.Closure, e.Error);
      }
      else {
        RCSymbolScalar id = (RCSymbolScalar) state.Other;
        string text = _client.Encoding.GetString (e.Result);
        // TODO: make this into a real object with headers and body and stuff.
        _inbox.Add (id, new RCString (text));
      }
    }

    public override void Receive (TcpCollector gatherer, RCSymbolScalar id)
    {
      _inbox.Remove (id, gatherer);
    }
  }
}

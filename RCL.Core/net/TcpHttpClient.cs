
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

    protected readonly long m_handle;
    protected readonly string m_host;
    protected readonly long m_port;
    protected readonly WebClient m_client;
    // protected readonly Uri m_uri;
    protected long m_cid = 0;
    protected TcpReceiveBox m_inbox;

    // I think host and port will get combined with "resource" as an RCSymbolScalar here.
    public TcpHttpClient (long handle, RCSymbolScalar hostandport)
    {
      m_handle = handle;
      m_host = (string) hostandport.Part (1);
      m_port = (long) hostandport.Part (2);
      // m_uri = new Uri("http://" + m_host + ":" + m_port.ToString ());
      m_inbox = new TcpReceiveBox ();
      m_client = new WebClient ();
      m_client.UploadDataCompleted += UploadDataCompleted;
      m_client.DownloadDataCompleted += DownloadDataCompleted;
    }

    public long Port
    {
      get { return m_port; }
    }

    public string Host
    {
      get { return m_host; }
    }

    public override void Open (RCRunner runner, RCClosure closure)
    {
      runner.Yield (closure, new RCLong (m_handle));
    }

    public override void Close (RCRunner runner, RCClosure closure)
    {
      m_client.Dispose ();
      runner.Yield (closure, new RCLong (m_handle));
    }

    public override TcpSendState Send (RCRunner runner, RCClosure closure, RCBlock message)
    {
      long cid = Interlocked.Increment (ref m_cid);
      RCSymbolScalar id = new RCSymbolScalar (null, m_handle);
      id = new RCSymbolScalar (id, cid);

      StringBuilder address = new StringBuilder ();
      // HttpVerb verb = (HttpVerb) Enum.Parse (
      //  typeof(HttpVerb),
      //  ((RCSymbol) message.Get ("verb"))[0].Part (0).ToString ());
      object[] resource = ((RCSymbol) message.Get ("resource"))[0].ToArray ();

      address.Append ("http://");
      address.Append (m_host);
      if (m_port > 0) {
        address.Append (":");
        address.Append (m_port);
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

      // byte[] payload = m_client.Encoding.GetBytes (message.Get ("body").ToString ());
      Uri uri = new Uri (address.ToString ());
      System.Console.Out.WriteLine (address.ToString ());
      m_client.DownloadDataAsync (uri, new RCAsyncState (runner, closure, id));
      // runner.Yield (closure, new RCSymbol(id));
      return new TcpSendState (m_handle, cid, message);
    }

    protected void UploadDataCompleted (object sender, UploadDataCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState) e.UserState;
      if (e.Error != null) {
        state.Runner.Report (state.Closure, e.Error);
      }
      RCSymbolScalar id = (RCSymbolScalar) state.Other;
      string text = m_client.Encoding.GetString (e.Result);
      // TODO: make this into a real object with headers and body and stuff.
      m_inbox.Add (id, new RCString (text));
    }

    protected void DownloadDataCompleted (object sender, DownloadDataCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState) e.UserState;
      if (e.Error != null) {
        state.Runner.Report (state.Closure, e.Error);
      }
      else {
        RCSymbolScalar id = (RCSymbolScalar) state.Other;
        string text = m_client.Encoding.GetString (e.Result);
        // TODO: make this into a real object with headers and body and stuff.
        m_inbox.Add (id, new RCString (text));
      }
    }

    public override void Receive (TcpCollector gatherer, RCSymbolScalar id)
    {
      m_inbox.Remove (id, gatherer);
    }
  }
}

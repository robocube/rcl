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
  public class HttpServer
  {
    protected internal object m_lock = new object ();
    protected int m_listener = 0;
    protected internal Dictionary<int, HttpListener> m_listeners =
      new Dictionary<int, HttpListener> ();

    //I'm sticking to the terminology of the api here but the context
    //is really a request.  And we have to keep them as mutable state
    //in order to respond through the http listener.
    protected int m_context = 0;
    protected readonly Dictionary<int, RequestInfo> m_contexts =
      new Dictionary<int, RequestInfo> ();
    protected int m_client = 0;

    public class RequestInfo
    {
      public readonly HttpListenerContext Context;
      public readonly DateTime Time;

      public RequestInfo (HttpListenerContext context, DateTime time)
      {
        Context = context;
        Time = time;
      }
    }

    [RCVerb ("httpstart")]
    public void EvalHttpStart (
      RCRunner runner, RCClosure closure, RCString right)
    {
      HttpListener listener = new HttpListener ();
      try
      {
        for (int i = 0; i < right.Count; ++i)
        {
          listener.Prefixes.Add (right[i]);
        }
        listener.Start ();
      }
      catch (Exception ex)
      {
        throw;
      }
      int handle;
      lock (m_lock)
      {
        ++m_listener;
        handle = m_listener;
        m_listeners.Add (handle, listener);
        // set these to optimal for MONO and .NET
        //lgsp.Expect100Continue = false;
        //lgsp.UseNagleAlgorithm = true;
        //lgsp.MaxIdleTime = 100000;
        runner.Log.RecordDoc (runner, closure, "http", handle, "start", right);
      }
      runner.Yield (closure, new RCLong (handle));
    }

    [RCVerb ("httpstop")]
    public void EvalHttpStop (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      lock (m_lock)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          HttpListener listener = m_listeners[(int) right[i]];
          //I should be ok calling this in a lock right?
          listener.Close ();
          runner.Log.RecordDoc (runner, closure, "http", right[i], "stop", "");
          //Shouldn't I remove this from the m_listeners?
          //Wait I want to see if retaining it fixes the object disposed exception.
        }
      }
      runner.Yield (closure, new RCBoolean (true));
    }

    [RCVerb ("httprecv")]
    public void EvalHttpRecv (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      HttpListener listener;
      //Let's have only one listener at a time, at least for now.
      if (right.Count > 1)
      {
        throw new Exception ("Can only httprecv from one listener per call");
      }

      lock (m_lock)
      {
        listener = m_listeners[(int) right[0]];
        listener.BeginGetContext (new AsyncCallback (Process),
          new RCAsyncState (runner, closure, listener));
      }
      //These updates were just noise in the log file.
      //runner.Log.Record (runner, closure, closure.Bot.Id, "https", right[0], "recv", "");
    }

    protected void Process (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      HttpListener listener = (HttpListener) state.Other;
      try
      {
        int handle;
        HttpListenerContext context;
        lock (m_lock)
        {
          context = listener.EndGetContext (result);
          m_context++;
          handle = m_context;
          m_contexts.Add (handle, new RequestInfo (context, DateTime.Now));
        }
        state.Runner.Log.RecordDoc (state.Runner, state.Closure,
                                 "http", handle, "proc",
                                 context.Request.HttpMethod + " " + context.Request.RawUrl);
        state.Runner.Yield (state.Closure, new RCLong (handle));
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    [RCVerb ("httpqs")]
    public void EvalHttpQs (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("httpqs only allows one request per call");
      }

      RequestInfo info;
      lock (m_lock)
      {
        info = m_contexts[(int) right[0]];
      }
      RCBlock query = RCBlock.Empty;
      for (int i = 0; i < info.Context.Request.QueryString.Count; ++i)
      {
        query = new RCBlock (query,
                             info.Context.Request.QueryString.Keys[i], ":",
                             new RCString (info.Context.Request.QueryString[i]));
      }
      runner.Yield (closure, query);
    }

    [RCVerb ("httpmethod")]
    public void EvalHttpMethod (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("httpheader only allows one request per call");
      }

      RequestInfo info;
      lock (m_lock)
      {
        info = m_contexts[(int) right[0]];
      }

      RCSymbolScalar method = new RCSymbolScalar (null,
                                                  info.Context.Request.HttpMethod.ToLower ());
      runner.Yield (closure, new RCSymbol (method));
    }

    [RCVerb ("httpheader")]
    public void EvalHttpHeader (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("httpheader only allows one request per call");
      }

      RequestInfo info;
      lock (m_lock)
      {
        info = m_contexts[(int) right[0]];
      }

      NameValueCollection values = info.Context.Request.Headers;
      RCBlock result = RCBlock.Empty;
      result = new RCBlock (result, "Verb", ":", new RCString (info.Context.Request.HttpMethod));
      result = new RCBlock (result, "RawUrl", ":", new RCString (info.Context.Request.RawUrl));
      for (int i = 0; i < values.AllKeys.Length; ++i)
      {
        string key = values.AllKeys[i];
        result = new RCBlock (result, key, ":", new RCString (values[key]));
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("httpbody")]
    public void EvalHttpBody (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("httpbody only allows one request per call");
      }

      RequestInfo info;
      lock (m_lock)
      {
        info = m_contexts[(int) right[0]];
      }
      string body = new StreamReader (info.Context.Request.InputStream).ReadToEnd ();
      //ParseQueryString really means ParseUrlEncodedForm.
      NameValueCollection values = HttpUtility.ParseQueryString (body);
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < values.AllKeys.Length; ++i)
      {
        string key = values.AllKeys[i];
        result = new RCBlock (result, key, ":", new RCString (values[key]));
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("httpraw")]
    public void EvalHttpRaw (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("httpraw only allows one request per call");
      }

      RequestInfo info;
      lock (m_lock)
      {
        info = m_contexts[(int) right[0]];
      }
      string body = new StreamReader (info.Context.Request.InputStream).ReadToEnd ();
      runner.Yield (closure, new RCString (body));
    }

    //This should be called httpreply, not send, for consistency with other apis.
    [RCVerb ("httpsend")]
    public void EvalHttpSend (
      RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      try
      {
        DoHttpSend (runner, closure, left, right);
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    [RCVerb ("httpsend")]
    public void EvalHttpSend (
      RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      try
      {
        //Maybe we should send multiple here?
        DoHttpSend (runner, closure, left, right[0]);
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    [RCVerb ("httpsend")]
    public void EvalHttpSend (
      RCRunner runner, RCClosure closure, RCLong left, RCByte right)
    {
      try
      {
        //Maybe we should send multiple here?
        DoHttpSend (runner, closure, left, right.ToArray ());
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    protected void DoHttpSend (
      RCRunner runner, RCClosure closure, RCLong left, byte[] payload)
    {
      RCBlock result = RCBlock.Empty;
      int total = 0;
      try
      {
        for (int i = 0; i < left.Count; ++i)
        {
          RequestInfo info;
          lock (m_lock)
          {
            info = m_contexts [(int)left [i]];
          }
          try
          {
            byte[] buffer = new byte[1024 * 16];
            MemoryStream stream = new MemoryStream (payload);
            int nbytes;
            while ((nbytes = stream.Read (buffer, 0, buffer.Length)) > 0)
            {
              info.Context.Response.OutputStream.Write (buffer, 0, nbytes);
              total += nbytes;
            }
            result = CreateLogEntry (info, left[i], total);
          }
          catch (Exception ex)
          {
            runner.Report (closure, ex);
            result = CreateLogEntry (info, left[i], 0);
          }
          finally
          {
            info.Context.Response.Close ();
            runner.Log.RecordDoc (runner, closure, "http", left [i], "send", result);
          }
        }
      }
      catch (Exception ex)
      {
        runner.Report (closure, ex);
      }
      runner.Yield (closure, result);
    }

    protected void DoHttpSend (
      RCRunner runner, RCClosure closure, RCLong left, string message)
    {
      RCBlock result = RCBlock.Empty;
      int total = 0;
      try
      {
        for (int i = 0; i < left.Count; ++i)
        {
          RequestInfo info;
          lock (m_lock)
          {
            info = m_contexts [(int)left [i]];
          }
          try
          {
            byte[] payload = Encoding.UTF8.GetBytes (message);
            byte[] buffer = new byte[1024 * 16];
            MemoryStream stream = new MemoryStream (payload);
            int nbytes;
            while ((nbytes = stream.Read (buffer, 0, buffer.Length)) > 0)
            {
              info.Context.Response.OutputStream.Write (buffer, 0, nbytes);
              total += nbytes;
            }
            result = CreateLogEntry (info, left[i], total);
          }
          catch (Exception ex)
          {
            runner.Report (closure, ex);
            result = CreateLogEntry (info, left[i], 0);
          }
          finally
          {
            info.Context.Response.Close ();
            runner.Log.RecordDoc (runner, closure, "http", left [i], "send", result);
          }
        }
      }
      catch (Exception ex)
      {
        runner.Report (closure, ex);
      }
      runner.Yield (closure, result);
    }

    protected RCBlock CreateLogEntry (RequestInfo info, long id, int bytes)
    {
      HttpListenerContext context = info.Context;
      string ip = context.Request.UserHostAddress;
      string user = context.User != null ? info.Context.User.Identity.Name : "";
      string resource = string.Format ("{0} {1} HTTP/{2}", 
                                      context.Request.HttpMethod, 
                                      context.Request.Url.LocalPath, 
                                      context.Request.ProtocolVersion.ToString ());
      string timestamp = string.Format ("[{0:dd/MMM/yyyy:hh:mm:ss zzz}]", info.Time);
      string httpversion = context.Request.ProtocolVersion.ToString ();
      string status = context.Response.StatusCode.ToString ();
      string byteString = bytes.ToString ();
      string referrer = context.Request.UrlReferrer != null ? 
                        context.Request.UrlReferrer.ToString () : "";
      string agent = context.Request.UserAgent != null ? 
                     context.Request.UserAgent : "";
      string cookie = "";
      if (context.Request.Cookies != null &&
          context.Request.Cookies.Count > 0)
      {
        StringBuilder builder = new StringBuilder ();
        for (int i = 0; i < context.Request.Cookies.Count; ++i)
        {
          Cookie c = context.Request.Cookies[i];
          builder.Append (c.Name);
          if (c.Value != null)
          {
            builder.Append ("=");
            builder.Append (c.Value);
          }
          if (i < context.Request.Cookies.Count - 1)
          {
            builder.Append ("; ");
          }
        }
        cookie = builder.ToString ();
      }
      RCBlock result = RCBlock.Empty;
      result = new RCBlock (result, "id", ":", new RCString (id.ToString ()));
      result = new RCBlock (result, "ip", ":", new RCString (ip));
      result = new RCBlock (result, "user", ":", new RCString (user));
      result = new RCBlock (result, "resource", ":", new RCString (resource));
      result = new RCBlock (result, "timestamp", ":", new RCString (timestamp));
      result = new RCBlock (result, "httpversion", ":", new RCString (httpversion));
      result = new RCBlock (result, "status", ":", new RCString (status));
      result = new RCBlock (result, "size", ":", new RCString (byteString));
      result = new RCBlock (result, "referrer", ":", new RCString (referrer));
      result = new RCBlock (result, "agent", ":", new RCString (agent));
      result = new RCBlock (result, "cookie", ":", new RCString (cookie));
      return result;
    }

    protected void DoHttpSend (
      RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      if (left.Count > 1)
      {
        throw new Exception (
          "httpsend only allows one request per call.  Maybe this can change though.");
      }
      RequestInfo info;
      lock (m_lock)
      {
        info = m_contexts[(int) left[0]];
      }
      try
      {
        RCLong status = (RCLong) right.Get ("status");
        RCBlock headers = (RCBlock) right.Get ("headers");
        RCString body = (RCString) right.Get ("body");
        if (status != null)
        {
          if (status[0] == 0)
          {
            info.Context.Response.StatusCode = 200;
          }
          else if (status[0] == 1)
          {
            info.Context.Response.StatusCode = 400;
          }
          else
          {
            info.Context.Response.StatusCode = (int) status[0];
          }
        }
        if (headers != null)
        {
          for (int i = 0; i < headers.Count; ++i)
          {
            RCBlock header = headers.GetName (i);
            RCString val = (RCString) header.Value;
            info.Context.Response.AppendHeader (header.Name, val[0]);
          }
        }
        byte[] bytes = Encoding.UTF8.GetBytes (body[0]);
        byte[] buffer = new byte[1024 * 16];
        MemoryStream stream = new MemoryStream (bytes);
        int nbytes;
        while ((nbytes = stream.Read (buffer, 0, buffer.Length)) > 0)
        {
          info.Context.Response.OutputStream.Write (buffer, 0, nbytes);
        }
      }
      finally
      {
        info.Context.Response.OutputStream.Close ();
        runner.Yield (closure, left);
      }
    }
  }
}
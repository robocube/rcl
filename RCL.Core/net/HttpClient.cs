
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
  public class HttpClient
  {
    protected long m_client = 0;

    public class RestAsyncState : RCAsyncState
    {
      public readonly RCString Body;
      public readonly bool BodyOnly;
      public readonly long Instance;
      public RestAsyncState (RCRunner runner, RCClosure closure,
                             object other, RCString body, bool bodyOnly, long instance)
        :base (runner, closure, other)
      {
        Body = body;
        BodyOnly = bodyOnly;
        Instance = instance;
      }
    }

    [RCVerb ("get")]
    public void Get (RCRunner runner, RCClosure closure, RCString right)
    {
      //This one only returns the body of the response
      //And throws exceptions if the response doesn't have a 200 status.
      if (right.Count != 1)
      {
        throw new Exception ("getw can only get from one resource at a time.");
      }
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (right[0]);
      request.Method = "GET";
      ThreadPool.QueueUserWorkItem (BeginWebRequest,
                                    new RestAsyncState (runner, closure, request, new RCString (), true, Interlocked.Increment (ref m_client)));
    }

    [RCVerb ("getw")]
    public void Getw (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("getw can only get from one resource at a time.");
      }
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (right[0]);
      request.Method = "GET";
      ThreadPool.QueueUserWorkItem (BeginWebRequest,
                                    new RestAsyncState (runner, closure, request, new RCString (), false, Interlocked.Increment (ref m_client)));
    }

    [RCVerb ("putw")]
    public void Putw (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("putw can only put to one resource at a time.");
      }
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (right[0]);
      request.Method = "PUT";
      request.BeginGetRequestStream (FinishGetRequestStream,
                                     new RestAsyncState (runner, closure, request, right, false, Interlocked.Increment (ref m_client)));
    }

    [RCVerb ("putw")]
    public void Putw (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("putw can only put to one resource at a time.");
      }
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (left[0]);
      request.Method = "PUT";
      request.BeginGetRequestStream (FinishGetRequestStream,
                                     new RestAsyncState (runner, closure, request, right, false, Interlocked.Increment (ref m_client)));
    }

    [RCVerb ("postw")]
    public void Postw (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("postw can only put to one resource at a time.");
      }
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (left[0]);
      request.Method = "POST";
      request.BeginGetRequestStream (FinishGetRequestStream,
                                     new RestAsyncState (runner, closure, request, right, false, Interlocked.Increment (ref m_client)));
    }

    [RCVerb ("delw")]
    public void Delw (
      RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("delw can only get from one resource at a time.");
      }
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (right[0]);
      request.Method = "DELETE";
      ThreadPool.QueueUserWorkItem (BeginWebRequest,
                                    new RestAsyncState (runner, closure, request, new RCString (), false, Interlocked.Increment (ref m_client)));
    }

    protected void FinishGetRequestStream (IAsyncResult result)
    {
      RestAsyncState state = (RestAsyncState) result.AsyncState;
      HttpWebRequest request = (HttpWebRequest) state.Other;
      try
      {
        Stream stream = request.EndGetRequestStream (result);
        TextWriter writer = new StreamWriter (stream);
        for (int i = 0; i < state.Body.Count; ++i)
        {
          writer.Write (state.Body[i]);
        }
        writer.Close ();
        //How does the stream get closed in this case?
        request.BeginGetResponse (FinishGetResponse, state);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    protected void BeginWebRequest (object obj)
    {
      RestAsyncState state = (RestAsyncState) obj;
      HttpWebRequest request = (HttpWebRequest) state.Other;
      //This does DNS lookups and some other blocking stuff.
      request.BeginGetResponse (FinishGetResponse, state);
    }

    protected void FinishGetResponse (IAsyncResult result)
    {
      RestAsyncState state = (RestAsyncState) result.AsyncState;
      HttpWebRequest request = (HttpWebRequest) state.Other;
      HttpWebResponse response = null;
      long status;
      try
      {
        try
        {
          response = (HttpWebResponse) request.EndGetResponse (result);
          status = (long) response.StatusCode;
        }
        catch (WebException ex)
        {
          if (ex.Status == WebExceptionStatus.ProtocolError)
          {
            response = ex.Response as HttpWebResponse;
            if (response != null)
            {
              status = (long) response.StatusCode;
            }
            else throw;
          }
          else throw;
        }
        Stream stream = response.GetResponseStream ();
        TextReader reader = new StreamReader (stream);
        string line;
        RCArray<string> body = new RCArray<string> ();
        while (true)
        {
          line = reader.ReadLine ();
          if (line != null)
          {
            body.Write (line);
          }
          else break;
        }
        reader.Close ();

        RCBlock block = new RCBlock (RCBlock.Empty,
                                     "status", ":", new RCLong (status));
        RCBlock head = RCBlock.Empty;
        for (int i = 0; i < response.Headers.Count; ++i)
        {
          head = new RCBlock (head,
                              response.Headers.Keys[i], ":", new RCString (response.Headers[i].Trim ('"')));
        }
        block = new RCBlock (block, "head", ":", head);
        block = new RCBlock (block, "body", ":", new RCString (body));
        state.Runner.Log.Record (state.Runner, state.Closure,
                                 "web", state.Instance, "response", block);
        if (state.BodyOnly)
        {
          if (status == 200)
          {
            state.Runner.Yield (state.Closure, new RCString (body));
          }
          else
          {
            state.Runner.Finish (state.Closure,
                                 new Exception ("http request failed.\n" + response.ToString ()), 1);
          }
        }
        else
        {
          state.Runner.Yield (state.Closure, block);
        }
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
      finally
      {
        if (response != null)
        {
          response.Close ();
        }
      }
    }

    [RCVerb ("httpget")]
    public void EvalHttpGet (
      RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      //You can send the same request to multiple sites.
      //I just had an idea, you can use method to propogate data to
      //other nodes, then consistency becomes the responsiblity of the
      //process that inserts or  produces the data.
      //It's a sort-of/maybe idea.
      string query = ToQueryString (right);
      for (int i = 0; i < left.Count; ++i)
      {
        //Why doesn't this follow the same Begin/End idea as every other API?
        //Is there some other lower level thing I should use?
        WebClient client = new WebClient ();
        long handle = Interlocked.Increment (ref m_client);
        client.DownloadStringCompleted +=
          new DownloadStringCompletedEventHandler (
            client_DownloadStringCompleted);
        Uri uri = new Uri (left[i] + query);
        runner.Log.Record (runner, closure,
                           "httpc", handle, "get", new RCString (left[i]));
        client.DownloadStringAsync (
          uri, new RCAsyncState (runner, closure, handle));
      }
    }

    public void client_DownloadStringCompleted (
      object sender, DownloadStringCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState) e.UserState;
      WebClient client = (WebClient) sender;
      try
      {
        client.DownloadStringCompleted -=
          new DownloadStringCompletedEventHandler(
            client_DownloadStringCompleted);
        if (e.Error != null)
        {
          throw e.Error;
        }
        RCString result = new RCString (e.Result);
        long handle = (long) state.Other;
        state.Runner.Log.Record (state.Runner, state.Closure,
                                 "httpc", handle, "done", result);
        state.Runner.Yield (state.Closure, result);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
      finally
      {
        client.Dispose ();
      }
    }

    /// <summary>
    /// Convert a block into a query string.
    /// Does not handle nested values or vectors longer than one.
    /// </summary>
    public static string ToQueryString (RCBlock block)
    {
      StringBuilder builder = new StringBuilder ();
      builder.Append ("block?");
      for (int i = 0; i < block.Count; ++i)
      {
        RCBlock name = block.GetName (i);
        builder.Append (name.Name);
        builder.Append ("=");
        name.Value.Format (builder, RCFormat.Default, 0);
      }
      return builder.ToString ();
    }
  }

  /*
  public class Http
  {
    [RCVerb ("httpstart")]
    public void EvalHttpStart (
      RCRunner runner, RCClosure closure, RCString right)
    {
      HttpListener listener = new HttpListener ();
      for (int i = 0; i < right.Count; ++i)
      {
        listener.Prefixes.Add (right[i]);
      }
      listener.Start ();
      runner.Yield (closure, new RCNative (listener));
    }

    [RCVerb ("httpstop")]
    public void EvalHttpStop (
      RCRunner runner, RCClosure closure, RCNative right)
    {
      HttpListener listener = (HttpListener) right.Value;
      listener.Close ();
      runner.Yield(closure, new RCBoolean (true));
    }

    [RCVerb ("httprecv")]
    public void EvalHttpRecv (
      RCRunner runner, RCClosure closure, RCNative right)
    {
      HttpListener listener = (HttpListener)right.Value;
      listener.BeginGetContext(new AsyncCallback(Process),
        new RCAsyncState(runner, closure, listener));
    }

    protected void Process (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      HttpListener listener = (HttpListener) state.Other;
      try
      {
        HttpListenerContext context = listener.EndGetContext (result);
        state.Runner.Yield (state.Closure, new RCNative (context));
        listener.BeginGetContext (new AsyncCallback (Process), state);
      }
      catch (Exception ex)
      {
        state.Runner.Report (ex);
      }
    }

    [RCVerb ("httpqs")]
    public void EvalHttpQs (
      RCRunner runner, RCClosure closure, RCNative right)
    {
      HttpListenerContext context = (HttpListenerContext) right.Value;
      RCBlock query = RCBlock.Empty;
      for (int i = 0; i < context.Request.QueryString.Count; ++i)
      {
        query = new RCBlock (
          query, context.Request.QueryString.Keys[i], ":",
          runner.Peek (context.Request.QueryString[i]));
      }
      runner.Yield (closure, query);
    }

    [RCVerb ("httpsend")]
    public void EvalHttpSend (
      RCRunner runner, RCClosure closure, RCNative left, RCBlock right)
    {
      HttpListenerContext context = (HttpListenerContext) left.Value;
      try
      {
        byte[] bytes = Encoding.ASCII.GetBytes(right.ToString());
        byte[] buffer = new byte[1024 * 16];
        MemoryStream stream = new MemoryStream(bytes);
        int nbytes;
        while ((nbytes = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
          context.Response.OutputStream.Write(buffer, 0, nbytes);
        }
      }
      finally
      {
        context.Response.OutputStream.Close();
      }
    }

    [RCVerb ("httpget")]
    public void EvalHttpGet (
      RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      //You can send the same request to multiple sites.
      //I just had an idea, you can use method to propogate data to
      //other nodes, then consistency becomes the responsiblity of the
      //process that inserts or  produces the data.
      //It's a sort-of/maybe idea.
      string query = ToQueryString(right);
      for (int i = 0; i < left.Count; ++i)
      {
        //Why doesn't this follow the same Begin/End idea as every other API?
        //Is there some other lower level thing I should use?
        WebClient client = new WebClient();
        client.DownloadStringCompleted +=
          new DownloadStringCompletedEventHandler(
            client_DownloadStringCompleted);
        Uri uri = new Uri(left[i] + query);
        client.DownloadStringAsync(
          uri, new RCAsyncState(runner, closure, null));
      }
    }

    public void client_DownloadStringCompleted(
      object s, DownloadStringCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState)e.UserState;
      try
      {
        WebClient sender = (WebClient)s;
        sender.DownloadStringCompleted -=
          new DownloadStringCompletedEventHandler(
            client_DownloadStringCompleted);
        string result = e.Result;
        RCValue value = state.Runner.Peek(result);
        state.Runner.Yield(state.Closure, value);
      }
      catch (Exception ex)
      {
        state.Runner.Report(ex);
      }
    }

    /// <summary>
    /// Convert a block into a query string.
    /// Does not handle nested values or vectors longer than one.
    /// </summary>
    public static string ToQueryString (RCBlock block)
    {
      StringBuilder builder = new StringBuilder ();
      builder.Append ("block?");
      for (int i = 0; i < block.Count; ++i)
      {
        RCBlock name = block.GetName (i);
        builder.Append (name.Name);
        builder.Append ("=");
        name.Value.Format (builder, FormatArgs.Default, 0);
      }
      return builder.ToString ();
    }
  }
  */
}

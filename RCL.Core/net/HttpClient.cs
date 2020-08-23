using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using RCL.Kernel;

namespace RCL.Core
{
  public class HttpClient
  {
    protected object _lock = new object ();
    protected long _client = 0;
    protected long _timeout = -1;
    protected long _retry = -1;

    public class RestAsyncState : RCAsyncState
    {
      public static readonly TimeSpan NoPeriodicSignal = new TimeSpan (0, 0, 0, 0, -1);
      public readonly object Lock = new object ();
      protected HttpWebRequest Request = null;
      protected bool Aborting = false;
      public readonly string Url;
      public readonly string Method;
      public readonly RCBlock Head;
      public readonly RCString Body;
      public readonly bool BodyOnly;
      public readonly long Instance;
      public readonly Timer OverallTimer;
      public readonly Timer RetryTimer;
      public readonly DateTime StartTime;
      public readonly TimeSpan Timeout;
      public readonly TimeSpan Retry;
      public readonly bool ShouldRetry;

      public RestAsyncState (RCRunner runner,
                             RCClosure closure,
                             string url,
                             string method,
                             RCBlock head,
                             RCString body,
                             bool bodyOnly,
                             long instance,
                             long timeout,
                             long retry,
                             bool shouldRetry)
        : base (runner, closure, null)
      {
        Url = url;
        Method = method;
        Head = head;
        Body = body;
        BodyOnly = bodyOnly;
        Instance = instance;
        // RequestTimer = new Timer (AbortWebRequest);
        RetryTimer = new Timer (RetryWebRequest);
        OverallTimer = new Timer (AbortWebRequestAndFail);
        StartTime = DateTime.UtcNow;
        Timeout = new TimeSpan (0, 0, 0, 0, (int) timeout);
        Retry = new TimeSpan (0, 0, 0, 0, (int) retry);
        ShouldRetry = shouldRetry;
      }

      protected void RetryOrFail (Exception ex)
      {
        if (!Aborting && ShouldRetry && Retry.Ticks >= 0) {
          RetryTimer.Change (Retry, NoPeriodicSignal);
        }
        else {
          OverallTimer.Dispose ();
          RetryTimer.Dispose ();
          Runner.Finish (Closure, ex, 1);
        }
      }

      public void MakeWebRequest (string logEvent)
      {
        HttpWebRequest request;
        lock (Lock)
        {
          Request = (HttpWebRequest) WebRequest.Create (Url);
          Request.Method = Method;
          SetHeaders (Request, Head);
          request = Request;
        }
        if (Body.Count > 0) {
          request.BeginGetRequestStream (delegate (IAsyncResult result)
          {
            Stream stream = request.EndGetRequestStream (result);
            TextWriter writer = new StreamWriter (stream);
            for (int i = 0; i < Body.Count; ++i)
            {
              writer.Write (Body[i]);
            }
            writer.Close ();
            // How does the stream get closed in this case?
            RCSystem.Log.Record (Closure, "web", Instance, logEvent, Method + " " + Url);
            // This does DNS lookups and some other blocking stuff.
            request.BeginGetResponse (FinishGetResponse, request);
          },
                                         null);
        }
        else {
          RCSystem.Log.Record (Closure, "web", Instance, logEvent, Method + " " + Url);
          // This does DNS lookups and some other blocking stuff.
          request.BeginGetResponse (FinishGetResponse, request);
        }
      }

      public void BeginWebRequest (object ignore)
      {
        try
        {
          MakeWebRequest ("request");
          if (Timeout.Ticks >= 0) {
            OverallTimer.Change (Timeout, NoPeriodicSignal);
          }
        }
        catch (Exception ex)
        {
          Runner.Finish (Closure, ex, 1);
        }
      }

      public void AbortWebRequestAndFail (object ignore)
      {
        HttpWebRequest request = null;
        lock (Lock)
        {
          if (Request != null) {
            request = Request;
            Aborting = true;
          }
        }
        if (request != null) {
          request.Abort ();
        }
      }

      public void RetryWebRequest (object ignore)
      {
        MakeWebRequest ("retry");
      }

      protected void SetHeaders (HttpWebRequest request, RCBlock head)
      {
        if (head != null) {
          for (int i = 0; i < head.Count; ++i)
          {
            RCBlock header = head.GetName (i);
            string name = header.RawName.ToLower ();
            if (name == "content-type") {
              request.ContentType = ((RCString) header.Value)[0];
            }
            else if (name == "accept") {
              request.Accept = ((RCString) header.Value)[0];
            }
            else if (name == "user-agent") {
              request.UserAgent = ((RCString) header.Value)[0];
            }
            else if (name == "referer") {
              request.Referer = ((RCString) header.Value)[0];
            }
            else {
              request.Headers.Set (header.RawName, ((RCString) header.Value)[0]);
            }
          }
        }
      }

      protected void FinishGetResponse (IAsyncResult result)
      {
        HttpWebRequest request = (HttpWebRequest) result.AsyncState;
        HttpWebResponse response = null;
        Exception ex = null;
        long status;
        RCBlock block = null;
        string body = null;
        try
        {
          try
          {
            response = (HttpWebResponse) request.EndGetResponse (result);
            status = (long) response.StatusCode;
            Stream stream = response.GetResponseStream ();
            TextReader reader = new StreamReader (stream);
            body = reader.ReadToEnd ();
            reader.Close ();
            // Should I also call stream.Close ()? I think I should.
            stream.Close ();
            block = new RCBlock (RCBlock.Empty,
                                 "status",
                                 ":",
                                 new RCLong (status));
            RCBlock head = RCBlock.Empty;
            for (int i = 0; i < response.Headers.Count; ++i)
            {
              head = new RCBlock (head,
                                  response.Headers.Keys[i],
                                  ":",
                                  new RCString (response.Headers[i].Trim ('"')));
            }
            block = new RCBlock (block, "head", ":", head);
            block = new RCBlock (block, "body", ":", new RCString (body));
            RCSystem.Log.Record (Closure, "web", Instance, "response", block);
          }
          catch (WebException webEx)
          {
            ex = webEx;
            if (webEx.Status == WebExceptionStatus.ProtocolError) {
              response = webEx.Response as HttpWebResponse;
              if (response != null) {
                status = (long) response.StatusCode;
                body = response.StatusDescription;
              }
              else {
                status = 700;
                body = webEx.Message;
              }
            }
            else {
              status = 700;
              body = webEx.Message;
            }
          }
          catch (Exception socketEx)
          {
            ex = socketEx;
            status = 700;
            body = socketEx.Message;
          }

          if (ex != null) {
            block = new RCBlock (RCBlock.Empty, "status", ":", new RCLong (status));
            block = new RCBlock (block, "head", ":", RCBlock.Empty);
            block = new RCBlock (block, "body", ":", new RCString (body));
          }
          if (status >= 500) {
            RetryOrFail (ex);
          }
          else {
            lock (Lock)
            {
              Request = null;
              OverallTimer.Dispose ();
              RetryTimer.Dispose ();
            }
            if (BodyOnly) {
              Runner.Yield (Closure, new RCString (body));
            }
            else {
              Runner.Yield (Closure, block);
            }
          }
        }
        catch (Exception otherEx)
        {
          Runner.Report (Closure, otherEx);
        }
        finally
        {
          if (response != null) {
            response.Close ();
          }
        }
      }
    }

    [RCVerb ("urlencode")]
    public void UrlEncode (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (HttpUtility.UrlEncode (right[i]));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("urldecode")]
    public void UrlDecode (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (HttpUtility.UrlDecode (right[i]));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("httptimeout")]
    public void HttpWait (RCRunner runner, RCClosure closure, RCLong right)
    {
      lock (_lock)
      {
        this._timeout = (int) right[0];
      }
      RCSystem.Log.Record (closure, "web", 0, "httptimeout", right[0]);
      runner.Yield (closure, right);
    }

    [RCVerb ("httpretry")]
    public void HttpRetry (RCRunner runner, RCClosure closure, RCLong right)
    {
      lock (_lock)
      {
        this._retry = (int) right[0];
      }
      RCSystem.Log.Record (closure, "web", 0, "httpretry", right[0]);
      runner.Yield (closure, right);
    }

    /*
       [RCVerb ("get")]
       public void Get (RCRunner runner, RCClosure closure, RCString right)
       {
       //This one only returns the body of the response
       //And throws exceptions if the response doesn't have a 200 status.
       if (right.Count != 1)
       {
        throw new Exception ("get can only get from one resource at a time.");
       }
       RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 right[0],
                                                 "GET",
                                                 RCBlock.Empty,
                                                 new RCString (),
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
       ThreadPool.QueueUserWorkItem (state.BeginWebRequest, state);
       }
     */

    [RCVerb ("getw")]
    public void Getw (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count != 1) {
        throw new Exception ("getw can only get from one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 right[0],
                                                 "GET",
                                                 RCBlock.Empty,
                                                 new RCString (),
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, state);
    }

    [RCVerb ("getw")]
    public void Getw (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      if (right.Count != 1) {
        throw new Exception ("getw can only get from one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 right[0],
                                                 "GET",
                                                 left,
                                                 new RCString (),
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, state);
    }

    [RCVerb ("putw")]
    public void Putw (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count != 1) {
        throw new Exception ("putw can only put to one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 right[0],
                                                 "PUT",
                                                 RCBlock.Empty,
                                                 right,
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, null);
    }

    [RCVerb ("putw")]
    public void Putw (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1) {
        throw new Exception ("putw can only put to one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 left[0],
                                                 "PUT",
                                                 RCBlock.Empty,
                                                 right,
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, null);
    }

    [RCVerb ("putw")]
    public void Putw (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      if (left.Count != 1) {
        throw new Exception ("putw can only put to one resource at a time.");
      }
      RCString body = (RCString) right.Get ("body");
      RCBlock head = (RCBlock) right.Get ("head", RCBlock.Empty);
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 left[0],
                                                 "PUT",
                                                 head,
                                                 body,
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, null);
    }

    [RCVerb ("postw")]
    public void Postw (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1) {
        throw new Exception ("postw can only put to one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 left[0],
                                                 "POST",
                                                 RCBlock.Empty,
                                                 right,
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, null);
    }

    [RCVerb ("postw")]
    public void Postw (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      if (left.Count != 1) {
        throw new Exception ("postw can only put to one resource at a time.");
      }
      RCString body = (RCString) right.Get ("body");
      RCBlock head = (RCBlock) right.Get ("head", RCBlock.Empty);
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 left[0],
                                                 "POST",
                                                 head,
                                                 body,
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, null);
    }

    [RCVerb ("delw")]
    public void Delw (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count != 1) {
        throw new Exception ("delw can only get from one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 right[0],
                                                 "DELETE",
                                                 RCBlock.Empty,
                                                 new RCString (),
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, state);
    }

    [RCVerb ("delw")]
    public void Delw (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      if (right.Count != 1) {
        throw new Exception ("delw can only get from one resource at a time.");
      }
      RestAsyncState state = new RestAsyncState (runner,
                                                 closure,
                                                 right[0],
                                                 "DELETE",
                                                 left,
                                                 new RCString (),
                                                 false,
                                                 Interlocked.Increment (ref _client),
                                                 _timeout,
                                                 _retry,
                                                 true);
      ThreadPool.QueueUserWorkItem (state.BeginWebRequest, state);
    }

    [RCVerb ("httpget")]
    public void EvalHttpGet (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      // You can send the same request to multiple sites.
      // I just had an idea, you can use method to propogate data to
      // other nodes, then consistency becomes the responsiblity of the
      // process that inserts or  produces the data.
      // It's a sort-of/maybe idea.
      string query = ToQueryString (right);
      for (int i = 0; i < left.Count; ++i)
      {
        // Why doesn't this follow the same Begin/End idea as every other API?
        // Is there some other lower level thing I should use?
        WebClient client = new WebClient ();
        long handle = Interlocked.Increment (ref _client);
        client.DownloadStringCompleted +=
          new DownloadStringCompletedEventHandler (client_DownloadStringCompleted);
        Uri uri = new Uri (left[i] + query);
        RCSystem.Log.Record (closure, "httpc", handle, "get", new RCString (left[i]));
        client.DownloadStringAsync (uri, new RCAsyncState (runner, closure, handle));
      }
    }

    public void client_DownloadStringCompleted (object sender, DownloadStringCompletedEventArgs e)
    {
      RCAsyncState state = (RCAsyncState) e.UserState;
      WebClient client = (WebClient) sender;
      try
      {
        client.DownloadStringCompleted -=
          new DownloadStringCompletedEventHandler (client_DownloadStringCompleted);
        if (e.Error != null) {
          throw e.Error;
        }
        RCString result = new RCString (e.Result);
        long handle = (long) state.Other;
        RCSystem.Log.Record (state.Closure, "httpc", handle, "done", result);
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
}

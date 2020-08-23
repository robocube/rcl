
using System;
using System.Threading.Tasks;
using System.Net.Http;
using RCL.Kernel;

namespace RCL.Core
{
  public class HttpClientAsync
  {
    [RCVerb ("getasync")]
    public void Get (RCRunner runner, RCClosure closure, RCString right)
    {
      // This is where I'm going to do some experiments with async and await,
      // and see how this will interact with the trampoline mechanism.
      // I don't really know where this is going, if it will work, if it matters.
      // I don't know everything about async/await yet, but right now it seems like
      // a perversion of how functions and the call stack are supposed to work.
      if (right.Count != 1) {
        throw new Exception ("get can only get from one resource at a time.");
      }
      // HttpRequestMessage q = new HttpRequestMessage (HttpMethod.Get, right[0]);
      System.Net.Http.HttpClient c = new System.Net.Http.HttpClient ();
      Task<HttpResponseMessage> task = c.GetAsync (right[0]);
      task.Wait ();
      HttpResponseMessage r = task.Result;
      runner.Yield (closure, new RCString (r.Content.ToString ()));

      // HttpWebRequest request = (HttpWebRequest) WebRequest.Create (right[0]);
      // request.ServicePoint.
      // request.Method = "GET";
      // ThreadPool.QueueUserWorkItem (BeginWebRequest,
      //                              new RestAsyncState (runner, closure, request, new
      // RCString (),
      // false, Interlocked.Increment (ref _client)));
    }
  }
}

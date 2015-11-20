
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Web;
using RCL.Kernel;

namespace RCL.Core
{
  public class List
  {
    [RCVerb ("list")]
    public void EvalList (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      string target = (string) right[0].Part (0);
      if (target == "" || target == "files")
      {
        ListFiles (runner, closure);
      }
      else if (target == "fibers")
      {
        ListFibers (runner, closure);
      }
      else if (target == "vars")
      {
        //ListVars (runner, closure);
      }
      else if (target == "urls")
      {
        ListUrls (runner, closure);
      }
      else if (target == "sockets")
      {
        ListSockets (runner, closure);
      }
      else throw new Exception ("Unknown target for list: " + target);
    }

    public static void ListFiles (RCRunner runner, RCClosure closure)
    {
      RCArray<string> paths = new RCArray<string> ();
      RCArray<long> sizes = new RCArray<long> ();
      RCArray<string> exts = new RCArray<string> ();
      RCArray<string> createTimes = new RCArray<string> ();
      RCArray<string> accessTimes = new RCArray<string> ();
      RCArray<string> writeTimes = new RCArray<string> ();
      Queue<string> todo = new Queue<string> ();
      todo.Enqueue (".");
      while (todo.Count > 0)
      {
        string path = todo.Dequeue ();
        string[] files = Directory.GetFiles (path);
        for (int i = 0; i < files.Length; ++i) {
          FileInfo file = new FileInfo (files[i]);
          paths.Write (files[i]);
          sizes.Write (file.Length);
          exts.Write (file.Extension);
          createTimes.Write (file.LastWriteTime.ToString ());
          accessTimes.Write (file.LastAccessTime.ToString ());
          writeTimes.Write (file.LastWriteTime.ToString ());
        }
        RCArray<string> dirs = new RCArray<string> (Directory.GetDirectories (path));
        for (int i = 0; i < dirs.Count; ++i)
          todo.Enqueue (dirs[i]);
      }
      RCBlock result = RCBlock.Empty;
      if (paths.Count > 0)
      {
        result = new RCBlock (result, "path", ":", new RCString (paths));
        result = new RCBlock (result, "size", ":", new RCLong (sizes));
        result = new RCBlock (result, "ext", ":", new RCString (exts));
        result = new RCBlock (result, "create", ":", new RCString (createTimes));
        result = new RCBlock (result, "access", ":", new RCString (accessTimes));
        result = new RCBlock (result, "write", ":", new RCString (writeTimes));
      }
      runner.Yield (closure, result);
    }

    public static void ListFibers (RCRunner runner, RCClosure closure)
    {
      RCArray<Fiber> modules = new RCArray<Fiber> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner.m_botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner.m_bots)
        {
          keys.Write (kv.Key);
          modules.Write ((Fiber) runner.m_bots[kv.Key].GetModule (typeof (Fiber)));
        }
      }
      RCArray<long> bots = new RCArray<long> ();
      RCArray<long> fibers = new RCArray<long> ();
      RCArray<string> states = new RCArray<string> ();
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i].m_fiberLock)
        {
          foreach (KeyValuePair<long, Fiber.FiberState> kv in modules[i].m_fibers)
          {
            bots.Write (keys[i]);
            fibers.Write (kv.Key);
            states.Write (kv.Value.State);
          }
        }
      }
      RCBlock result = RCBlock.Empty;
      if (states.Count > 0)
      {
        result = new RCBlock (result, "bot", ":", new RCLong (bots));
        result = new RCBlock (result, "fiber", ":", new RCLong (fibers));
        result = new RCBlock (result, "state", ":", new RCString (states));
      }
      runner.Yield (closure, result);
    }

    /*
    public static void ListVars (RCRunner runner, RCClosure closure)
    {
      RCArray<MemoryStore> modules = new RCArray<MemoryStore> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner.m_botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner.m_bots)
        {
          keys.Write (kv.Key);
          modules.Write ((MemoryStore) runner.m_bots[kv.Key].GetModule (typeof (MemoryStore)));
        }
      }
      RCArray<long> bots = new RCArray<long> ();
      RCArray<RCSymbolScalar> names = new RCArray<RCSymbolScalar> ();
      RCArray<string> values = new RCArray<string> ();
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i].m_lock)
        {
          foreach (KeyValuePair<RCSymbolScalar, RCValue> kv in modules[i].m_store)
          {
            bots.Write (keys[i]);
            names.Write (kv.Key);
            //Need to make cubes handle columns of block.
            values.Write (kv.Value.ToString ());
          }
        }
      }
      RCBlock result = RCBlock.Empty;
      if (values.Count > 0)
      {
        result = new RCBlock (result, "bot", ":", new RCLong (bots));
        result = new RCBlock (result, "name", ":", new RCSymbol (names));
        result = new RCBlock (result, "value", ":", new RCString (values));
      }
      runner.Yield (closure, result);
    }
    */

    public static void ListSockets (RCRunner runner, RCClosure closure)
    {
      throw new NotImplementedException ();
    }

    public static void ListUrls (RCRunner runner, RCClosure closure)
    {
      RCArray<HttpServer> modules = new RCArray<HttpServer> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner.m_botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner.m_bots)
        {
          keys.Write (kv.Key);
          modules.Write ((HttpServer) runner.m_bots[kv.Key].GetModule (typeof (HttpServer)));
        }
      }
      RCArray<long> bots = new RCArray<long> ();
      RCArray<long> handles = new RCArray<long> ();
      RCArray<string> urls = new RCArray<string> ();
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i].m_lock)
        {
          foreach (KeyValuePair<int, HttpListener> kv in modules[i].m_listeners)
          {
            foreach (string uri in kv.Value.Prefixes)
            {
              bots.Write (keys[i]);
              handles.Write (kv.Key);
              urls.Write (uri);
            }
          }
        }
      }
      RCBlock result = RCBlock.Empty;
      if (urls.Count > 0)
      {
        result = new RCBlock (result, "bot", ":", new RCLong (bots));
        result = new RCBlock (result, "handle", ":", new RCLong (handles));
        result = new RCBlock (result, "url", ":", new RCString (urls));
      }
      runner.Yield (closure, result);
    }
  }
}
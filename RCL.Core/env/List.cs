
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
    protected readonly RCSymbolScalar all = new RCSymbolScalar (null, "all");
    protected readonly RCSymbolScalar deep = new RCSymbolScalar (null, "deep");
    protected readonly RCSymbol DefaultLeft = new RCSymbol ();

    [RCVerb ("list")]
    public void EvalList (RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      HashSet<RCSymbolScalar> options = new HashSet<RCSymbolScalar> (left);
      string target = (string) right[0].Part (0);
      if (target == "" || target == "work")
      {
        ListFilesCube (runner, closure, right[0], options.Contains (all), 
                                                  options.Contains (deep));
      }
      else if (target == "home")
      {
        ListFilesCube (runner, closure, right[0], options.Contains (all), 
                                                  options.Contains (deep));
      }
      else if (target == "fibers")
      {
        ListFibers (runner, closure);
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

    [RCVerb ("show_special_folders")]
    public void ShowSpecialFolders (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      foreach(Environment.SpecialFolder folder in Enum.GetValues(typeof(Environment.SpecialFolder)))
      {
        Console.WriteLine(string.Format("{0}\t\t\t{1}", folder.ToString(), Environment.GetFolderPath(folder)));
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("list")]
    public void EvalList (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      EvalList (runner, closure, DefaultLeft, right);
    }


    public static void ListFilesCube (RCRunner runner, 
                                      RCClosure closure, 
                                      RCSymbolScalar spec,
                                      bool all, 
                                      bool deep)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      Queue<string> todo = new Queue<string> ();
      string top = Command.PathSymbolToString (spec);
      todo.Enqueue (top);
      while (todo.Count > 0)
      {
        string path = todo.Dequeue ();
        //Needs to be async. LIAR!
        string[] files = Directory.GetFiles (path);
        for (int i = 0; i < files.Length; ++i)
        {
          FileInfo file = new FileInfo (files [i]);
          if (all || file.Name[0] != '.')
          {
            string [] parts = files[i].Split (Path.DirectorySeparatorChar);
            RCSymbolScalar symbol = RCSymbolScalar.From (1, parts);
            result.WriteCell ("size", symbol, file.Length);
            result.WriteCell ("ext", symbol, file.Extension);
            result.WriteCell ("access", symbol, new RCTimeScalar (file.LastAccessTime, RCTimeType.Datetime));
            result.WriteCell ("write", symbol, new RCTimeScalar (file.LastWriteTime, RCTimeType.Datetime));
            result.Axis.Write (symbol); 
          }
        }
        //Needs to be async. CHEATER!
        RCArray<string> dirs = new RCArray<string> (Directory.GetDirectories (path));
        for (int i = 0; i < dirs.Count; ++i) 
        {
          DirectoryInfo dir = new DirectoryInfo (dirs [i]);
          if (all || dir.Name [0] != '.') 
          {
            string [] parts = dirs [i].Split (Path.DirectorySeparatorChar);
            RCSymbolScalar symbol = RCSymbolScalar.From (1, parts);
            result.WriteCell ("access", symbol, new RCTimeScalar (dir.LastAccessTime, RCTimeType.Datetime));
            result.WriteCell ("write", symbol, new RCTimeScalar (dir.LastWriteTime, RCTimeType.Datetime));
            result.Axis.Write (symbol);
            if (deep)
            {
              todo.Enqueue (dirs [i]);
            }
          }
        }
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
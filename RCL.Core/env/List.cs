
using System;
using System.IO;
using System.Threading;
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
      if (target == "" || target == "work") {
        BeginListFilesCube (runner,
                            closure,
                            new ListArgs (right[0],
                                          options.Contains (all),
                                          options.Contains (deep)));
      }
      else if (target == "home") {
        BeginListFilesCube (runner,
                            closure,
                            new ListArgs (right[0],
                                          options.Contains (all),
                                          options.Contains (deep)));
      }
      else if (target == "root") {
        BeginListFilesCube (runner,
                            closure,
                            new ListArgs (right[0],
                                          options.Contains (all),
                                          options.Contains (deep)));
      }
      else if (target == "fibers") {
        ListFibers (runner, closure);
      }
      else if (target == "vars") {
        ListVars (runner, closure);
      }
      else if (target == "urls") {
        ListUrls (runner, closure);
      }
      else if (target == "request") {
        ListRequest (runner, closure);
      }
      else if (target == "sockets") {
        ListSockets (runner, closure);
      }
      else if (target == "exec") {
        ListExec (runner, closure);
      }
      else {
        throw new Exception ("Unknown target for list: " + target);
      }
    }

    [RCVerb ("list")]
    public void EvalList (RCRunner runner, RCClosure closure, RCString right)
    {
      ListArgs args = new ListArgs (right[0], all: true, deep: false);
      BeginListFilesCube (runner, closure, args);
    }

    [RCVerb ("show_special_folders")]
    public void ShowSpecialFolders (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      foreach (Environment.SpecialFolder folder in Enum.GetValues (typeof (
                                                                     Environment.SpecialFolder)))
      {
        Console.WriteLine (string.Format ("{0}\t\t\t{1}",
                                          folder.ToString (),
                                          Environment.GetFolderPath (folder)));
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("list")]
    public void EvalList (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      EvalList (runner, closure, DefaultLeft, right);
    }

    protected void BeginListFilesCube (RCRunner runner, RCClosure closure, ListArgs args)
    {
      ThreadPool.QueueUserWorkItem (ListFilesCube,
                                    new RCAsyncState (runner, closure, args));
    }

    public class ListArgs
    {
      public readonly RCSymbolScalar Spec;
      public readonly string Path;
      public readonly bool All;
      public readonly bool Deep;

      public ListArgs (string path, bool all, bool deep)
      {
        Path = path;
        All = all;
        Deep = deep;
      }

      public ListArgs (RCSymbolScalar spec, bool all, bool deep)
      {
        Spec = spec;
        Path = Command.PathSymbolToLocalString (spec);
        All = all;
        Deep = deep;
      }
    }

    protected static void ListFilesCube (object obj)
    {
      RCAsyncState state = (RCAsyncState) obj;
      ListArgs args = (ListArgs) state.Other;
      try
      {
        RCCube result = new RCCube (new RCArray<string> ("S"));
        Queue<string> todo = new Queue<string> ();
        string top = args.Path;
        string[] topParts = top.Split (Path.DirectorySeparatorChar);
        int startPart;
        RCSymbolScalar prefix;
        if (args.Spec != null) {
          startPart = (int) (topParts.Length - args.Spec.Length) + 1;
          prefix = RCSymbolScalar.From (args.Spec.Part (0));
        }
        else {
          startPart = 1;
          prefix = RCSymbolScalar.From (topParts[0]);
        }
        todo.Enqueue (top);
        while (todo.Count > 0)
        {
          string path = todo.Dequeue ();
          string[] files = Directory.GetFiles (path);
          for (int i = 0; i < files.Length; ++i)
          {
            FileInfo file = new FileInfo (files[i]);
            if (args.All || file.Name[0] != '.') {
              string [] parts = files[i].Split (Path.DirectorySeparatorChar);
              RCSymbolScalar symbol = RCSymbolScalar.From (startPart, prefix, parts);
              result.WriteCell ("name", symbol, file.Name);
              result.WriteCell ("size", symbol, file.Length);
              result.WriteCell ("type", symbol, "f");
              result.WriteCell ("ext", symbol, file.Extension);
              result.WriteCell ("access",
                                symbol,
                                new RCTimeScalar (file.LastAccessTime,
                                                  RCTimeType.Datetime));
              result.WriteCell ("write",
                                symbol,
                                new RCTimeScalar (file.LastWriteTime,
                                                  RCTimeType.Datetime));
              result.Axis.Write (symbol);
            }
          }
          RCArray<string> dirs = new RCArray<string> (Directory.GetDirectories (path));
          for (int i = 0; i < dirs.Count; ++i)
          {
            DirectoryInfo dir = new DirectoryInfo (dirs[i]);
            if (args.All || dir.Name[0] != '.') {
              string [] parts = dirs[i].Split (Path.DirectorySeparatorChar);
              RCSymbolScalar symbol = RCSymbolScalar.From (startPart, prefix, parts);
              result.WriteCell ("name", symbol, dir.Name);
              result.WriteCell ("type", symbol, "d");
              result.WriteCell ("access",
                                symbol,
                                new RCTimeScalar (dir.LastAccessTime,
                                                  RCTimeType.Datetime));
              result.WriteCell ("write",
                                symbol,
                                new RCTimeScalar (dir.LastWriteTime,
                                                  RCTimeType.Datetime));
              result.Axis.Write (symbol);
              if (args.Deep) {
                todo.Enqueue (dirs[i]);
              }
            }
          }
        }
        state.Runner.Yield (state.Closure, result);
      }
      catch (DirectoryNotFoundException ex)
      {
        state.Runner.Finish (state.Closure,
                             new RCException (state.Closure, RCErrors.File, ex.Message),
                             1);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    /*
       public static void ListFibers (RCRunner runner, RCClosure closure)
       {
       RCArray<Fiber> modules = new RCArray<Fiber> ();
       RCArray<long> keys = new RCArray<long> ();
       lock (runner._botLock)
       {
        foreach (KeyValuePair<long, RCBot> kv in runner._bots)
        {
          keys.Write (kv.Key);
          modules.Write ((Fiber) runner._bots[kv.Key].GetModule (typeof (Fiber)));
        }
       }
       RCArray<long> bots = new RCArray<long> ();
       RCArray<long> fibers = new RCArray<long> ();
       RCArray<string> states = new RCArray<string> ();
       for (int i = 0; i < modules.Count; ++i)
       {
        lock (modules[i]._fiberLock)
        {
          foreach (KeyValuePair<long, Fiber.FiberState> kv in modules[i]._fibers)
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
     */

    public static void ListFibers (RCRunner runner, RCClosure closure)
    {
      RCArray<Fiber> modules = new RCArray<Fiber> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner._botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner._bots)
        {
          keys.Write (kv.Key);
          modules.Write ((Fiber) runner._bots[kv.Key].GetModule (typeof (Fiber)));
        }
      }
      RCCube result = new RCCube ("S");
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i]._fiberLock)
        {
          foreach (KeyValuePair<long, Fiber.FiberState> kv in modules[i]._fibers)
          {
            RCSymbolScalar sym = RCSymbolScalar.From ("fiber", keys[i], kv.Key);
            result.WriteCell ("bot", sym, keys[i]);
            result.WriteCell ("fiber", sym, kv.Key);
            result.WriteCell ("state", sym, kv.Value.State);
            result.Write (sym);
          }
        }
      }
      runner.Yield (closure, result);
    }

    public static void ListVars (RCRunner runner, RCClosure closure)
    {
      RCArray<Variable> modules = new RCArray<Variable> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner._botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner._bots)
        {
          keys.Write (kv.Key);
          modules.Write ((Variable) runner._bots[kv.Key].GetModule (typeof (Variable)));
        }
      }
      RCCube result = new RCCube ("S");
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i]._lock)
        {
          foreach (KeyValuePair<object, Dictionary<RCSymbolScalar,
                                                   RCValue>> section in modules[i]._sections)
          {
            RCSymbolScalar botsym = RCSymbolScalar.From ("var", keys[i]);
            foreach (KeyValuePair<RCSymbolScalar, RCValue> kv in section.Value)
            {
              RCSymbolScalar varsym = RCSymbolScalar.From (botsym, kv.Key);
              result.WriteCell ("bot", varsym, keys[i]);
              result.WriteCell ("section", varsym, section.Key);
              result.WriteCell ("name", varsym, kv.Key);
              result.WriteCell ("value", varsym, kv.Value.ToString ());
              result.Write (varsym);
            }
          }
        }
      }
      runner.Yield (closure, result);
    }

    public static void ListSockets (RCRunner runner, RCClosure closure)
    {
      throw new NotImplementedException ();
    }

    public static void ListUrls (RCRunner runner, RCClosure closure)
    {
      RCArray<HttpServer> modules = new RCArray<HttpServer> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner._botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner._bots)
        {
          keys.Write (kv.Key);
          modules.Write ((HttpServer) runner._bots[kv.Key].GetModule (typeof (HttpServer)));
        }
      }
      RCCube result = new RCCube ("S");
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i]._lock)
        {
          foreach (KeyValuePair<int, HttpListener> kv in modules[i]._listeners)
          {
            foreach (string uri in kv.Value.Prefixes)
            {
              RCSymbolScalar urlsym = RCSymbolScalar.From ("url", keys[i], (long) kv.Key);
              result.WriteCell ("bot", urlsym, keys[i]);
              result.WriteCell ("handle", urlsym, (long) kv.Key);
              result.WriteCell ("url", urlsym, uri);
              result.Write (urlsym);
            }
          }
        }
      }
      runner.Yield (closure, result);
    }

    public static void ListRequest (RCRunner runner, RCClosure closure)
    {
      RCArray<HttpServer> modules = new RCArray<HttpServer> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner._botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner._bots)
        {
          keys.Write (kv.Key);
          modules.Write ((HttpServer) runner._bots[kv.Key].GetModule (typeof (HttpServer)));
        }
      }
      RCCube result = new RCCube ("S");
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i]._lock)
        {
          foreach (KeyValuePair<int, HttpServer.RequestInfo> kv in modules[i]._contexts)
          {
            RCSymbolScalar urlsym = RCSymbolScalar.From ("request", keys[i], (long) kv.Key);
            result.WriteCell ("bot", urlsym, keys[i]);
            result.WriteCell ("handle", urlsym, (long) kv.Key);
            result.WriteCell ("url", urlsym, kv.Value.Context.Request.RawUrl);
            result.Write (urlsym);
          }
        }
      }
      runner.Yield (closure, result);
    }

    public static void ListExec (RCRunner runner, RCClosure closure)
    {
      RCArray<Exec> modules = new RCArray<Exec> ();
      RCArray<long> keys = new RCArray<long> ();
      lock (runner._botLock)
      {
        foreach (KeyValuePair<long, RCBot> kv in runner._bots)
        {
          keys.Write (kv.Key);
          modules.Write ((Exec) runner._bots[kv.Key].GetModule (typeof (Exec)));
        }
      }
      RCCube result = new RCCube ("S");
      for (int i = 0; i < modules.Count; ++i)
      {
        lock (modules[i]._lock)
        {
          foreach (KeyValuePair<long, Exec.ChildProcess> kv in modules[i]._process)
          {
            RCSymbolScalar sym = RCSymbolScalar.From ("exec", keys[i], kv.Key);
            result.WriteCell ("bot", sym, keys[i]);
            result.WriteCell ("handle", sym, (long) kv.Key);
            result.WriteCell ("pid", sym, (long) kv.Value._pid);
            result.WriteCell ("program", sym, kv.Value._program);
            result.WriteCell ("arguments", sym, kv.Value._arguments);
            result.WriteCell ("exit", sym, kv.Value._exitCode);
            result.Write (sym);
          }
        }
      }
      runner.Yield (closure, result);
    }
  }
}

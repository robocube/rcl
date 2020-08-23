using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using RCL.Kernel;

namespace RCL.Core
{
  public class FileEvents
  {
    protected class RCLFileSystemWatcher : FileSystemWatcher
    {
      public readonly RCRunner Runner;
      public readonly long Handle;
      public RCLFileSystemWatcher (RCRunner runner,
                                   long handle,
                                   string path,
                                   string filter)
        : base (path, filter)
      {
        Runner = runner;
        Handle = handle;
      }
    }

    protected object _lock = new object ();
    protected long _handle = -1;
    protected Dictionary <long, RCLFileSystemWatcher> _watchers =
      new Dictionary<long, RCLFileSystemWatcher> ();
    protected Dictionary <long, Queue<RCBlock>> _output = new Dictionary<long, Queue<RCBlock>> ();
    protected Dictionary <long, RCClosure> _waiters = new Dictionary<long, RCClosure> ();

    [RCVerb ("watchfs")]
    public void EvalWatchd (RCRunner runner, RCClosure closure, RCString right)
    {
      long handle = Interlocked.Increment (ref _handle);
      RCLFileSystemWatcher watcher = new RCLFileSystemWatcher (runner, handle, right[0], "*");
      watcher.InternalBufferSize = 16 * 1024;
      watcher.IncludeSubdirectories = true;
      watcher.NotifyFilter = NotifyFilters.DirectoryName |
                             NotifyFilters.FileName |
                             NotifyFilters.CreationTime |
                             NotifyFilters.Attributes |
                             NotifyFilters.LastWrite |
                             NotifyFilters.Size;
      watcher.Created += watcher_Created;
      watcher.Changed += watcher_Changed;
      watcher.Deleted += watcher_Deleted;
      watcher.Renamed += watcher_Renamed;
      watcher.EnableRaisingEvents = true;
      lock (_lock)
      {
        _output.Add (handle, new Queue<RCBlock> ());
        _watchers.Add (handle, watcher);
      }
      runner.Yield (closure, new RCLong (handle));
    }

    [RCVerb ("watchfs")]
    public void EvalWatchd (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      long handle = Interlocked.Increment (ref _handle);
      RCLFileSystemWatcher watcher = new RCLFileSystemWatcher (runner, handle, right[0], left[0]);
      watcher.InternalBufferSize = 16 * 1024;
      watcher.IncludeSubdirectories = true;
      watcher.NotifyFilter = NotifyFilters.DirectoryName |
                             NotifyFilters.FileName |
                             NotifyFilters.CreationTime |
                             NotifyFilters.Attributes |
                             NotifyFilters.LastWrite |
                             NotifyFilters.Size;
      watcher.Created += watcher_Created;
      watcher.Changed += watcher_Changed;
      watcher.Deleted += watcher_Deleted;
      watcher.Renamed += watcher_Renamed;
      watcher.EnableRaisingEvents = true;
      lock (_lock)
      {
        _output.Add (handle, new Queue<RCBlock> ());
        _watchers.Add (handle, watcher);
      }
      runner.Yield (closure, new RCLong (handle));
    }

    [RCVerb ("waitf")]
    public void EvalWaitf (RCRunner runner, RCClosure closure, RCLong right)
    {
      lock (_lock)
      {
        Queue<RCBlock> queue;
        if (_output.TryGetValue (right[0], out queue) && queue.Count > 0) {
          Drain (_watchers[right[0]], closure, queue);
        }
        else {
          RCClosure waiter;
          if (_waiters.TryGetValue (right[0], out waiter)) {
            throw new Exception ("Please only create one waiter per watcher.");
          }
          _waiters.Add (right[0], closure);
        }
      }
    }

    void Drain (RCLFileSystemWatcher watcher, RCClosure closure, Queue<RCBlock> queue)
    {
      RCBlock result = RCBlock.Empty;
      while (queue.Count > 0)
      {
        result = new RCBlock (result, "", ":", queue.Dequeue ());
      }
      if (result.Count > 0) {
        _waiters.Remove (watcher.Handle);
        watcher.Runner.Yield (closure, result);
      }
    }

    void EnqueueAndDrain (RCLFileSystemWatcher watcher, RCBlock result)
    {
      lock (_lock)
      {
        _output[watcher.Handle].Enqueue (result);
        RCClosure closure;
        if (_waiters.TryGetValue (watcher.Handle, out closure)) {
          Queue<RCBlock> queue;
          if (_output.TryGetValue (watcher.Handle, out queue)) {
            Drain (watcher, closure, queue);
          }
        }
      }
    }

    void watcher_Renamed (object sender, RenamedEventArgs e)
    {
      RCLFileSystemWatcher watcher = (RCLFileSystemWatcher) sender;
      RCBlock result = GetFileEventInfo (e);
      result = new RCBlock (result, "oldname", ":", new RCString (e.OldName));
      result = new RCBlock (result, "oldfullpath", ":", new RCString (e.OldFullPath));
      // RCSystem.Log.Record (watcher.Runner, closure, "fs", watcher.Handle, "rename",
      // result);
      EnqueueAndDrain (watcher, result);
    }

    void watcher_Deleted (object sender, FileSystemEventArgs e)
    {
      RCLFileSystemWatcher watcher = (RCLFileSystemWatcher) sender;
      RCBlock result = GetFileEventInfo (e);
      EnqueueAndDrain (watcher, result);
    }

    void watcher_Created (object sender, FileSystemEventArgs e)
    {
      RCLFileSystemWatcher watcher = (RCLFileSystemWatcher) sender;
      RCBlock result = GetFileEventInfo (e);
      EnqueueAndDrain (watcher, result);
    }

    void watcher_Changed (object sender, FileSystemEventArgs e)
    {
      RCLFileSystemWatcher watcher = (RCLFileSystemWatcher) sender;
      RCBlock result = GetFileEventInfo (e);
      EnqueueAndDrain (watcher, result);
    }

    RCBlock GetFileEventInfo (FileSystemEventArgs e)
    {
      RCBlock result = RCBlock.Empty;
      result = new RCBlock (result,
                            "event",
                            ":",
                            new RCString (e.ChangeType.ToString ().ToLower ()));
      result = new RCBlock (result, "name", ":", new RCString (e.Name));
      result = new RCBlock (result, "fullpath", ":", new RCString (e.FullPath));
      return result;
    }
  }
}


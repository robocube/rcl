
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using RCL.Kernel;

namespace RCL.Core
{
  public class FileIO
  {
    protected object _lock = new object ();
    protected long _handle = -1;
    protected Dictionary<long, FileState> _filesByHandle = new Dictionary<long, FileState> ();
    protected Dictionary<string, FileState> _filesByName = new Dictionary<string, FileState> ();

    public class FileState
    {
      public long _h;
      public FileInfo _f;
      public StreamWriter _w;
      public FileState (long h, FileInfo f, StreamWriter w)
      {
        _h = h;
        _f = f;
        _w = w;
      }
    }

    [RCVerb ("openf")]
    public void EvalOpenf (RCRunner runner, RCClosure closure, RCString right)
    {
      long handle = -1;
      lock (_lock)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          FileState state;
          FileInfo f = new FileInfo (right[0]);
          if (!_filesByName.TryGetValue (f.FullName, out state)) {
            StreamWriter w = f.AppendText ();
            ++_handle;
            handle = _handle;
            state = new FileState (handle, f, w);
            _filesByHandle.Add (handle, state);
            _filesByName.Add (f.FullName, state);
          }
          else {
            handle = state._h;
          }
        }
      }
      runner.Yield (closure, new RCLong (handle));
    }

    [RCVerb ("writef")]
    public void EvalWritef (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      if (left.Count != 1) {
        throw new Exception ("writef requires exactly one handle");
      }
      lock (_lock)
      {
        FileState f;
        if (!_filesByHandle.TryGetValue (left[0], out f)) {
          throw new Exception ("Bad file handle: " + left[0]);
        }
        StringBuilder builder = new StringBuilder ();
        for (int i = 0; i < right.Count; ++i)
        {
          builder.Append (right[i]);
        }
        byte[] message = Encoding.UTF8.GetBytes (builder.ToString ());
        RCAsyncState state = new RCAsyncState (runner, closure, left);
        f._w.BaseStream.BeginWrite (message, 0, message.Length, EndWrite, state);
      }
    }

    protected void EndWrite (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      RCLong left = (RCLong) state.Other;
      try
      {
        lock (_lock)
        {
          FileState f;
          if (!_filesByHandle.TryGetValue (left[0], out f)) {
            throw new Exception ("Bad file handle: " + left[0]);
          }
          f._w.BaseStream.EndWrite (result);
          f._w.BaseStream.Flush ();
        }
        state.Runner.Yield (state.Closure, left);
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }
  }
}
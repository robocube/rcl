
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
    protected object m_lock = new object ();
    protected long m_handle = -1;
    protected Dictionary<long, FileState> m_filesByHandle = new Dictionary<long, FileState> ();
    protected Dictionary<string, FileState> m_filesByName = new Dictionary<string, FileState> ();

    public class FileState
    {
      public long m_h;
      public FileInfo m_f;
      public StreamWriter m_w;
      public FileState (long h, FileInfo f, StreamWriter w)
      {
        m_h = h;
        m_f = f;
        m_w = w;
      }
    }

    [RCVerb ("openf")]
    public void EvalOpenf (
      RCRunner runner, RCClosure closure, RCString right)
    {
      long handle = -1;
      lock (m_lock)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          FileState state;
          FileInfo f = new FileInfo (right[0]);
          if (!m_filesByName.TryGetValue (f.FullName, out state))
          {
            StreamWriter w = f.AppendText ();
            ++m_handle;
            handle = m_handle;
            state = new FileState (handle, f, w);
            m_filesByHandle.Add (handle, state);
            m_filesByName.Add (f.FullName, state);
          }
          else
          {
            handle = state.m_h;
          }
        }
      }
      runner.Yield (closure, new RCLong (handle));
    }

    [RCVerb ("writef")]
    public void EvalWritef (
      RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("writef requires exactly one handle");
      }
      lock (m_lock)
      {
        FileState f;
        if (!m_filesByHandle.TryGetValue (left[0], out f))
        {
          throw new Exception ("Bad file handle: " + left[0]);
        }
        StringBuilder builder = new StringBuilder ();
        for (int i = 0; i < right.Count; ++i)
        {
          builder.Append (right[i]);
        }
        byte[] message = Encoding.UTF8.GetBytes (builder.ToString ());
        RCAsyncState state = new RCAsyncState (runner, closure, left);
        f.m_w.BaseStream.BeginWrite (message, 0, message.Length, EndWrite, state);
      }
    }

    protected void EndWrite (IAsyncResult result)
    {
      RCAsyncState state = (RCAsyncState) result.AsyncState;
      RCLong left = (RCLong) state.Other;
      try
      {
        lock (m_lock)
        {
          FileState f;
          if (!m_filesByHandle.TryGetValue (left[0], out f))
          {
            throw new Exception ("Bad file handle: " + left[0]);
          }
          f.m_w.BaseStream.EndWrite (result);
          f.m_w.BaseStream.Flush ();
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
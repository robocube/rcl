
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using RCL.Kernel;

namespace RCL.Core
{
  public class Exec : IDisposable
  {
    protected object m_lock = new object ();
    protected long m_handle = 0;
    protected Dictionary <long, ChildProcess> m_process = new Dictionary<long, ChildProcess> ();
    protected bool disposed;

    [RCVerb ("exec")]
    public virtual void EvalExec (
      RCRunner runner, RCClosure closure, RCString command)
    {
      RCAsyncState state = new RCAsyncState (runner, closure, command);
      long handle = CreateHandle ();
      ChildProcess process = new ChildProcess (handle, state, true);
      RegisterProcess (process);
      process.Start ();
    }

    [RCVerb ("startx")]
    public virtual void EvalSpawn (
      RCRunner runner, RCClosure closure, RCString command)
    {
      RCAsyncState state = new RCAsyncState (runner, closure, command);
      long handle = CreateHandle ();
      ChildProcess process = new ChildProcess (handle, state, false);
      RegisterProcess (process);
      process.Start ();
      runner.Yield (closure, new RCLong (handle));
    }

    protected long CreateHandle ()
    {
      long handle;
      lock (m_lock)
      {
        handle = m_handle;
        ++m_handle;
      }
      return handle;
    }

    protected void RegisterProcess (ChildProcess process)
    {
      lock (m_lock)
      {
        m_process.Add (process.Handle, process);
      }
    }

    protected void DoExec (object obj)
    {
      RCAsyncState state = (RCAsyncState) obj;
      try
      {
        ChildProcess process = (ChildProcess) state.Other;
        process.Start ();
      }
      catch (Exception ex)
      {
        state.Runner.Report (state.Closure, ex);
      }
    }

    [RCVerb ("writex")]
    public virtual void EvalWritex (
      RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      ChildProcess child;
      lock (m_lock)
      {
        if (!m_process.TryGetValue (left[0], out child))
          throw new Exception ("Unknown child process: " + left[0]);
      }
      ThreadPool.QueueUserWorkItem (child.WriteLineToInput,
                                    new RCAsyncState (runner, closure, right));
    }

    [RCVerb ("readx")]
    public virtual void EvalReadx (
      RCRunner runner, RCClosure closure, RCString left, RCLong right)
    {
      ChildProcess child;
      lock (m_lock)
      {
        if (!m_process.TryGetValue (right[0], out child))
          throw new Exception ("Unknown child process: " + right[0]);
      }
      ThreadPool.QueueUserWorkItem (child.ReadLineFromOutput,
                                    new RCAsyncState (runner, closure, left));
    }

    [RCVerb ("waitx")]
    public virtual void EvalWaitx (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      ChildProcess child;
      lock (m_lock)
      {
        if (!m_process.TryGetValue (right[0], out child))
          throw new Exception ("Unknown child process: " + right[0]);
      }
      ThreadPool.QueueUserWorkItem (child.Wait,
                                    new RCAsyncState (runner, closure, right));
    }

    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    protected virtual void Dispose (bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          foreach (KeyValuePair<long, ChildProcess> kv in m_process)
          {
            kv.Value.Close ();
          }
        }
        disposed = true;
      }
    }

    protected class ChildProcess
    {
      public readonly long Handle;
      protected RCArray<string> m_result = new RCArray<string> ();
      protected Queue<string> m_lines = new Queue<string> ();
      protected Queue<RCAsyncState> m_outputReaders = new Queue<RCAsyncState> ();
      protected Queue<RCAsyncState> m_waiters = new Queue<RCAsyncState> ();
      protected int m_readLines = 0;
      protected int[] m_checkedLines = new int[16];
      protected Timer m_timer;
      protected RCAsyncState m_state;
      protected Process m_process;
      protected bool m_yieldOnExit;
      protected bool m_outputDone = false;
      protected bool m_errorDone = false;
      protected bool m_exited = false;
      protected bool m_finished = false;
      protected string m_program;
      protected string m_arguments;

      public ChildProcess (long handle, RCAsyncState state, bool yieldWhenDone)
      {
        Handle = handle;
        m_state = state;
        m_yieldOnExit = yieldWhenDone;
        RCString command = (RCString) m_state.Other;
        //It seems that this is the one api in the entire bcl that does
        //not offer some way to pass custom arguments asyncly.
        string line = command[0].TrimStart (' ');
        int split = line.IndexOf (' ');
        if (split >= 0)
        {
          m_program = line.Substring (0, split);
          m_arguments = line.Substring (split + 1);
        }
        else
        {
          m_program = line;
          m_arguments = "";
        }

        m_process = new Process ();
        m_process.StartInfo = new ProcessStartInfo (m_program, m_arguments);
        m_process.EnableRaisingEvents = true;
        m_process.StartInfo.CreateNoWindow = true;
        m_process.StartInfo.UseShellExecute = false;
        m_process.StartInfo.ErrorDialog = false;
        m_process.StartInfo.RedirectStandardOutput = true;
        m_process.OutputDataReceived += HandleProcessOutputDataReceived;
        m_process.ErrorDataReceived += HandleProcessErrorDataReceived;
        m_process.StartInfo.RedirectStandardError = true;
        m_process.StartInfo.RedirectStandardInput = true;
        m_process.Exited += process_Exited;
        m_timer = new Timer (TimerCallback, null, 200, Timeout.Infinite);
      }

      void process_Exited (object sender, EventArgs e)
      {
        lock (this)
        {
          m_exited = true;
        }
        Finish (null);
      }

      public void Start ()
      {
        try
        {
          lock (this)
          {
            m_process.Start ();
            m_process.BeginOutputReadLine ();
            m_process.BeginErrorReadLine ();
            //This guys says that if we are not using the standard input it should be closed
            //immediately, but I cannot find any information about why. I introduced this code
            //thinking it might fix a bug but it did not so I'm commenting it out for now.
            //http://csharptest.net/321/how-to-use-systemdiagnosticsprocess-correctly/
            //{:{<-startx "sleep 1"} each 0l to 1000l <-exit 0l}
            //if (!m_yieldOnExit)
            //{
            //  m_process.StandardInput.Close ();
            //}
          }
          m_state.Runner.Log.RecordDoc (m_state.Runner, m_state.Closure,
                                        "exec", Handle, "start",
                                        string.Format ("{0} {1}", m_program, m_arguments));
        }
        catch (Exception ex)
        {
          m_state.Runner.Report (m_state.Closure, ex);
        }
      }

      //The state here is specifically for fibers waiting
      //for the process to be totally done and dusted.
      public void Finish (RCAsyncState waiter)
      {
        long exitCode;
        RCAsyncState[] waiters;
        lock (this)
        {
          if (m_finished)
          {
            waiter.Runner.Yield (waiter.Closure, new RCString (m_result));
            return;
          }
          if (waiter != null)
          {
            m_waiters.Enqueue (waiter);
          }
          if (!(m_exited && m_outputDone && m_errorDone))
          {
            return;
          }
          exitCode = m_process.ExitCode;
          m_timer.Dispose ();
          m_process.Dispose ();
          waiters = m_waiters.ToArray ();
          m_waiters.Clear ();
          m_finished = true;
        }
        //RCString result = new RCString (m_result);
        m_state.Runner.Log.RecordDoc (m_state.Runner, m_state.Closure,
                                      "exec", Handle, "done", exitCode);

        RCString lines = null;
        lock (this)
        {
          if (m_lines.Count > 0)
          {
            lines = new RCString (m_lines.ToArray ());
            m_lines.Clear ();
          }
        }
        if (lines != null)
        {
          m_state.Runner.Log.RecordDoc (m_state.Runner, m_state.Closure,
                                          "exec", Handle, "line", lines);
        }

        RCString result = new RCString (m_result);
        for (int i = 0; i < waiters.Length; ++i)
        {
          if (exitCode != 0)
          {
            m_state.Runner.Finish (m_state.Closure,
                                   new Exception ("Non-zero exit status"), exitCode);
          }
          else
          {
            waiters[i].Runner.Yield (waiters[i].Closure, result);
          }
        }
        if (m_yieldOnExit)
        {
          if (exitCode != 0)
          {
            m_state.Runner.Finish (m_state.Closure,
                                   new Exception ("Non-zero exit status"), exitCode);
          }
          else
          {
            m_state.Runner.Yield (m_state.Closure, result);
          }
        }
      }

      public void Close ()
      {
        lock (this)
        {
          if (!m_finished)
          {
            m_process.Kill ();
          }
        }
        //You understand I don't normally do things like this.
        while (true)
        {
          lock (this)
          {
            if (m_finished)
            {
              return;
            }
          }
        }
      }

      public void Wait (object obj)
      {
        RCAsyncState state = (RCAsyncState) obj;
        Finish (state);
      }

      public void WriteLineToInput (object obj)
      {
        RCAsyncState state = (RCAsyncState) obj;
        RCString right = (RCString) state.Other;
        try
        {
          StringBuilder builder = new StringBuilder ();
          for (int i = 0; i < right.Count; ++i)
          {
            builder.AppendLine (right[i]);
          }
          string text = builder.ToString ();
          byte[] message = Encoding.UTF8.GetBytes (text);
          lock (this)
          {
            if (m_outputDone || m_errorDone || m_exited)
            {
              Exception ex = new Exception ("Cannot write to standard input, process has already exited or is in the process of exiting.");
              state.Runner.Finish (state.Closure, ex, 1);
            }
            m_process.StandardInput.BaseStream.BeginWrite (
              message, 0, message.Length, EndWrite, state);
          }
          //Using right instead of text here ensures that the output is consistent
          //with the usual formatting procedure.
          //example single lines will appear on the header line not their own line.
          //for purposes of writing to stdin it is imperative that every line ends cleanly.
          state.Runner.Log.RecordDoc (state.Runner, state.Closure,
                                      "exec", Handle, "writex", right);
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
        }
      }

      protected void EndWrite (IAsyncResult result)
      {
        RCAsyncState state = (RCAsyncState) result.AsyncState;
        RCString right = (RCString) state.Other;
        try
        {
          lock (this)
          {
            m_process.StandardInput.BaseStream.EndWrite (result);
            m_process.StandardInput.BaseStream.Flush ();
          }
          state.Runner.Yield (state.Closure, new RCLong (right.Count));
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
        }
      }

      //This part still needs a lot of work. In particular identifying the end of output.
      public void ReadLineFromOutput (object obj)
      {
        RCAsyncState state = (RCAsyncState) obj;
        try
        {
          RCString left = (RCString) state.Other;
          string line = null;
          char termChar = left[0][0];
          lock (this)
          {
            if (m_readLines < m_result.Count)
            {
              if (termChar == '\n')
              {
                line = m_result[m_readLines];
                ++m_readLines;
              }
              else
              {
                throw new NotImplementedException ();
                //start checking each time from checked lines.
                for (int i = m_checkedLines[termChar]; i < m_result.Count; ++i)
                {
                  int termPos = m_result[i].IndexOf (termChar);
                  if (termPos >= 0)
                  {
                    //when the term char is found go BACK to read lines to read them.
                    StringBuilder text = new StringBuilder ();
                    for (int j = m_readLines; j < i; ++j)
                    {
                      text.AppendLine (m_result[j]);
                    }
                    line = text.ToString ();
                    //If the term char is midline we need to do some more work here.
                    m_readLines = i;
                  }
                  ++m_checkedLines[termChar];
                }
              }
            }
          }
          if (line != null)
          {
            state.Runner.Log.RecordDoc (state.Runner, state.Closure,
                                        "exec", Handle, "readx", line);
            state.Runner.Yield (state.Closure, new RCString (line));
          }
          else
          {
            m_outputReaders.Enqueue (state);
          }
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
        }
      }

      protected void TimerCallback (object ignore)
      {
        try
        {
          RCString output = null;
          lock (this)
          {
            if (m_lines.Count > 0)
            {
              output = new RCString (m_lines.ToArray ());
              m_lines.Clear ();
            }
          }
          if (output != null)
          {
            m_state.Runner.Log.RecordDoc (m_state.Runner, m_state.Closure,
                                          "exec", Handle, "line", output);
          }
          m_timer.Change (200, Timeout.Infinite);
        }
        catch (Exception ex)
        {
          m_state.Runner.Report (m_state.Closure, ex);
        }
      }

      public void HandleProcessOutputDataReceived (object sender, DataReceivedEventArgs e)
      {
        ReadLine (true, e.Data);
      }

      public void HandleProcessErrorDataReceived (object sender, DataReceivedEventArgs e)
      {
        ReadLine (false, e.Data);
      }

      protected void ReadLine (bool output, string line)
      {
        try
        {
          RCAsyncState[] readers;
          if (line == null)
          {
            lock (this)
            {
              if (output)
              {
                m_outputDone = true;
              }
              else
              {
                m_errorDone = true;
              }
            }
            Finish (null);
          }
          else
          {
            lock (this)
            {
              m_result.Write (line);
              m_lines.Enqueue (line);
              readers = m_outputReaders.ToArray ();
              m_outputReaders.Clear ();
            }
            for (int i = 0; i < readers.Length; i++)
            {
              ReadLineFromOutput (readers[i]);
            }
          }
        }
        catch (Exception ex)
        {
          m_state.Runner.Report (m_state.Closure, ex);
        }
      }
    }
  }
}
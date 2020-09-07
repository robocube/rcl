
using System;
using System.Text;
using System.IO;
#if __MonoCS__
using Mono.Posix;
#endif
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using RCL.Kernel;

namespace RCL.Core
{
  public class Exec : IDisposable
  {
    protected internal object _lock = new object ();
    protected long _handle = 0;
    protected internal Dictionary <long, ChildProcess> _process = new Dictionary<long,
                                                                                  ChildProcess> ();
    protected bool disposed;

    [RCVerb ("exec")]
    public virtual void EvalExec (RCRunner runner, RCClosure closure, RCString command)
    {
      RCAsyncState state = new RCAsyncState (runner, closure, command);
      long handle = CreateHandle ();
      ChildProcess process = new ChildProcess (this, handle, state, true);
      RegisterProcess (process);
      process.Start ();
    }

    [RCVerb ("startx")]
    public virtual void EvalSpawn (RCRunner runner, RCClosure closure, RCString command)
    {
      RCAsyncState state = new RCAsyncState (runner, closure, command);
      long handle = CreateHandle ();
      ChildProcess process = new ChildProcess (this, handle, state, false);
      RegisterProcess (process);
      process.Start ();
      runner.Yield (closure, new RCLong (handle));
    }

    protected long CreateHandle ()
    {
      long handle;
      lock (_lock)
      {
        handle = _handle;
        ++_handle;
      }
      return handle;
    }

    protected void RegisterProcess (ChildProcess process)
    {
      lock (_lock)
      {
        _process.Add (process.Handle, process);
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
    public virtual void EvalWritex (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      ChildProcess child;
      lock (_lock)
      {
        if (!_process.TryGetValue (left[0], out child)) {
          throw new Exception ("Unknown child process: " + left[0]);
        }
      }
      ThreadPool.QueueUserWorkItem (child.WriteLineToInput,
                                    new RCAsyncState (runner, closure, right));
    }

    [RCVerb ("readx")]
    public virtual void EvalReadx (RCRunner runner, RCClosure closure, RCString left, RCLong right)
    {
      ChildProcess child;
      lock (_lock)
      {
        if (!_process.TryGetValue (right[0], out child)) {
          throw new Exception ("Unknown child process: " + right[0]);
        }
      }
      ThreadPool.QueueUserWorkItem (child.ReadLineFromOutput,
                                    new RCAsyncState (runner, closure, left));
    }

    [RCVerb ("waitx")]
    public virtual void EvalWaitx (RCRunner runner, RCClosure closure, RCLong right)
    {
      ChildProcess child;
      lock (_lock)
      {
        if (!_process.TryGetValue (right[0], out child)) {
          throw new Exception ("Unknown child process: " + right[0]);
        }
      }
      ThreadPool.QueueUserWorkItem (child.Wait,
                                    new RCAsyncState (runner, closure, right));
    }

    [RCVerb ("killx")]
    public virtual void EvalKillx (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      ChildProcess child;
      lock (_lock)
      {
        if (!_process.TryGetValue (left[0], out child)) {
          throw new Exception ("Unknown child process: " + left[0]);
        }
      }
      ThreadPool.QueueUserWorkItem (child.Kill,
                                    new RCAsyncState (runner, closure, right));
    }

    [RCVerb ("closex")]
    public virtual void EvalClosex (RCRunner runner, RCClosure closure, RCLong right)
    {
      ChildProcess child;
      lock (_lock)
      {
        if (!_process.TryGetValue (right[0], out child)) {
          throw new Exception ("Unknown child process: " + right[0]);
        }
      }
      ThreadPool.QueueUserWorkItem (child.Close,
                                    new RCAsyncState (runner, closure, right));
    }

    [RCVerb ("delx")]
    public virtual void EvalDelx (RCRunner runner, RCClosure closure, RCLong right)
    {
      ChildProcess child;
      lock (_lock)
      {
        if (!_process.TryGetValue (right[0], out child)) {
          throw new Exception ("Unknown child process: " + right[0]);
        }
        _process.Remove (right[0]);
      }
      runner.Yield (closure, right);
    }

    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    protected virtual void Dispose (bool disposing)
    {
      if (!disposed) {
        if (disposing) {
          lock (_lock)
          {
            foreach (KeyValuePair<long, ChildProcess> kv in _process)
            {
              kv.Value.Close ();
            }
          }
        }
        disposed = true;
      }
    }

    protected internal class ChildProcess
    {
      public readonly long Handle;
      protected readonly Exec Module;
      protected RCArray<string> _result = new RCArray<string> ();
      protected Queue<string> _lines = new Queue<string> ();
      protected Queue<RCAsyncState> _outputReaders = new Queue<RCAsyncState> ();
      protected Queue<RCAsyncState> _waiters = new Queue<RCAsyncState> ();
      protected int _readLines = 0;
      protected int[] _checkedLines = new int[16];
      protected Timer _timer;
      protected RCAsyncState _state;
      protected Process _process;
      protected bool _yieldOnExit;
      protected bool _outputDone = false;
      protected bool _errorDone = false;
      protected bool _exited = false;
      protected bool _finished = false;
      protected bool _killing = false;
      protected internal long _exitCode = -1;
      protected internal string _program;
      protected internal string _arguments;
      protected internal long _pid = -1;

      public ChildProcess (Exec module, long handle, RCAsyncState state, bool yieldWhenDone)
      {
        Handle = handle;
        Module = module;
        _state = state;
        _yieldOnExit = yieldWhenDone;
        RCString command = (RCString) _state.Other;
        // It seems that this is the one api in the entire bcl that does
        // not offer some way to pass custom arguments asyncly.
        string line = command[0].TrimStart (' ');
        int split = line.IndexOf (' ');
        if (split >= 0) {
          _program = line.Substring (0, split);
          _arguments = line.Substring (split + 1);
        }
        else {
          _program = line;
          _arguments = "";
        }

        _process = new Process ();
        _process.StartInfo = new ProcessStartInfo (_program, _arguments);
        _process.EnableRaisingEvents = true;
        _process.StartInfo.CreateNoWindow = true;
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.ErrorDialog = false;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.OutputDataReceived += HandleProcessOutputDataReceived;
        _process.ErrorDataReceived += HandleProcessErrorDataReceived;
        _process.StartInfo.RedirectStandardError = true;
        _process.StartInfo.RedirectStandardInput = true;
        _process.Exited += process_Exited;
        _timer = new Timer (TimerCallback, null, 200, Timeout.Infinite);
      }

      void process_Exited (object sender, EventArgs e)
      {
        lock (this)
        {
          _exited = true;
        }
        Finish (null);
      }

      public void Start ()
      {
        try
        {
          lock (this)
          {
            _process.Start ();
            _pid = _process.Id;
            _process.BeginOutputReadLine ();
            _process.BeginErrorReadLine ();
            // This guys says that if we are not using the standard input it should be
            // closed immediately, but I cannot find any information about why. I
            // introduced this
            // code thinking it might fix a bug but it did not so I'm commenting it out
            // for
            // now.
            // http://csharptest.net/321/how-to-use-systemdiagnosticsprocess-correctly/
            // {:{<-startx "sleep 1"} each 0l to 1000l <-exit 0l}
            // if (!_yieldOnExit)
            // {
            //  _process.StandardInput.Close ();
            // }
          }
          RCSystem.Log.Record (_state.Closure,
                               "exec",
                               Handle,
                               "start",
                               string.Format ("{0} {1}", _program, _arguments));
        }
        catch (Exception ex)
        {
          _state.Runner.Report (_state.Closure, ex);
          throw;
        }
      }

      // The state here is specifically for fibers waiting
      // for the process to be totally done and dusted.
      public void Finish (RCAsyncState waiter)
      {
        long exitCode;
        RCAsyncState[] waiters;
        lock (this)
        {
          if (_finished) {
            if (waiter == null) {
              return;
            }
            else if (_exitCode != 0) {
              StripControlChars (_result);
              _state.Runner.Finish (waiter.Closure,
                                     new RCException (waiter.Closure,
                                                      RCErrors.Exec,
                                                      "exit status " + _exitCode,
                                                      new RCString (_result)),
                                     _exitCode);
            }
            else {
              StripControlChars (_result);
              waiter.Runner.Yield (waiter.Closure, new RCString (_result));
            }
            return;
          }
          if (waiter != null) {
            _waiters.Enqueue (waiter);
          }
          // When _killing is set, do not wait for last null from stdout and stderr.
          if (!_killing && !(_exited && _outputDone && _errorDone)) {
            return;
          }
          exitCode = _process.ExitCode;
          _exitCode = exitCode;
          _timer.Dispose ();
          waiters = _waiters.ToArray ();
          _waiters.Clear ();
          _finished = true;
        }
        RCSystem.Log.Record (_state.Closure, "exec", Handle, "done", exitCode);
        RCString lines = null;
        lock (this)
        {
          if (_lines.Count > 0) {
            lines = new RCString (_lines.ToArray ());
            _lines.Clear ();
          }
        }
        if (lines != null) {
          RCSystem.Log.Record (_state.Closure, "exec", Handle, "line", lines, forceDoc: true);
        }
        StripControlChars (_result);
        RCString result = new RCString (_result);
        for (int i = 0; i < waiters.Length; ++i)
        {
          if (exitCode != 0) {
            _state.Runner.Finish (waiters[i].Closure,
                                   new RCException (waiters[i].Closure,
                                                    RCErrors.Exec,
                                                    "exit status " + exitCode,
                                                    result),
                                   exitCode);
          }
          else {
            waiters[i].Runner.Yield (waiters[i].Closure, result);
          }
        }
        if (_yieldOnExit) {
          if (exitCode != 0) {
            _state.Runner.Finish (_state.Closure,
                                   new RCException (_state.Closure,
                                                    RCErrors.Exec,
                                                    "exit status " + exitCode,
                                                    result),
                                   exitCode);
          }
          else {
            _state.Runner.Yield (_state.Closure, result);
          }
          lock (Module._lock)
          {
            Module._process.Remove ((int) Handle);
          }
          lock (this)
          {
            _process.Dispose ();
          }
        }
      }

      protected void StripControlChars (RCArray<string> result)
      {
        // So far this only comes into play on Windows when using wsl to execute shell scripts.
        // This is the output of the bash 'clear' command.
        // Let's be minimally aggressive to start.
        if (result.Count > 0 && result[result.Count - 1] == "\u001b[H\u001b[2J")
        {
          result.RemoveAt (result.Count - 1);
        }
      }

      public void Kill (object other)
      {
        RCAsyncState state = (RCAsyncState) other;
        try
        {
          RCLong signal = (RCLong) state.Other;
          RCSystem.Log.Record (null, "exec", Handle, "killx", signal[0]);
          lock (this)
          {
      #if __MonoCS__
            Mono.Unix.Native.Syscall.kill (
              (int) _pid,
              (Mono.Unix.Native.Signum)signal[0]);
      #else
            KillById ((int) _pid);
      #endif
          }
          state.Runner.Yield (state.Closure, signal);
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
        }
      }

      protected static bool KillById (int pid)
      {
        Process process = Process.GetProcessById (pid);
        if (process != null) {
          process.Kill ();
          return true;
        }
        else {
          return false;
        }
      }

      public void Close (object other)
      {
        RCAsyncState state = (RCAsyncState) other;
        try
        {
          RCLong signal = (RCLong) state.Other;
          RCSystem.Log.Record (null, "exec", Handle, "closex", signal[0]);
          Close ();
          state.Runner.Yield (state.Closure, signal);
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
        }
      }

      public void Close ()
      {
        // First ask nicely and wait 1000 ms.
        // If it's an rcl process it should cleanly close sockets, files, other procs etc.
        // See signal handling in Process.cs
        string message =
          _program + (_arguments.Length > 0 ? " " : "") + _arguments + " (" + _pid + ")";
        RCSystem.Log.Record (null, "exec", Handle, "closing", message);
        lock (this)
        {
          if (_pid >= 0 && !_finished) {
      #if __MonoCS__
            Mono.Unix.Native.Syscall.kill (
              (int) _pid,
              Mono.Unix.Native.Signum.SIGTERM);
      #else
            // Console.Out.WriteLine ("Closing exec'd processes is not implemented on Windows");
      #endif
          }
          else {
            return;
          }
        }
        DateTime timeout = DateTime.UtcNow + new TimeSpan (0, 0, 0, 0, 2000);
        // You understand I don't normally do things like this.
        while (DateTime.UtcNow < timeout)
        {
          lock (this)
          {
            if (_finished) {
              RCSystem.Log.Record (null, "exec", Handle, "finished", "soft");
              return;
            }
          }
        }
        // Then ask not nicely and wait forever.
        lock (this)
        {
          if (!_finished) {
            _killing = true;
            _process.Kill ();
          }
        }
        while (true)
        {
          lock (this)
          {
            if (_finished) {
              RCSystem.Log.Record (null, "exec", Handle, "finished", "hard");
              return;
            }
          }
        }
      }

      public void Wait (object obj)
      {
        RCAsyncState state = (RCAsyncState) obj;
        try
        {
          Finish (state);
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
          throw;
        }
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
            if (_outputDone || _errorDone || _exited) {
              Exception ex = new Exception (
                "Cannot write to standard input, process has already exited or is in the process of exiting.");
              state.Runner.Finish (state.Closure, ex, 1);
            }
            _process.StandardInput.BaseStream.BeginWrite (message,
                                                           0,
                                                           message.Length,
                                                           EndWrite,
                                                           state);
          }
          // Using right instead of text here ensures that the output is consistent
          // with the usual formatting procedure.
          // example single lines will appear on the header line not their own line.
          // for purposes of writing to stdin it is imperative that every line ends
          // cleanly.
          RCSystem.Log.Record (state.Closure, "exec", Handle, "writex", right);
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
            _process.StandardInput.BaseStream.EndWrite (result);
            _process.StandardInput.BaseStream.Flush ();
          }
          state.Runner.Yield (state.Closure, new RCLong (right.Count));
        }
        catch (Exception ex)
        {
          state.Runner.Report (state.Closure, ex);
        }
      }

      // This part still needs a lot of work. In particular identifying the end of output.
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
            if (_readLines < _result.Count) {
              if (termChar == '\n') {
                line = _result[_readLines];
                ++_readLines;
              }
              else {
                throw new NotImplementedException ();
                // start checking each time from checked lines.
                // I can't really remember what this code was going to do.
                // It has to do with using a termChar other than newline.
                // I think it will come into play when we want more interaction
                // with a process via readx. At present we write input to these
                // processes only once.
                // Commenting this code for now to fix the warning.
                /*
                   for (int i = _checkedLines[termChar]; i < _result.Count; ++i)
                   {
                   int termPos = _result[i].IndexOf (termChar);
                   if (termPos >= 0)
                   {
                    //when the term char is found go BACK to read lines to read them.
                    StringBuilder text = new StringBuilder ();
                    for (int j = _readLines; j < i; ++j)
                    {
                      text.AppendLine (_result[j]);
                    }
                    line = text.ToString ();
                    //If the term char is midline we need to do some more work here.
                    _readLines = i;
                   }
                 ++_checkedLines[termChar];
                   }
                 */
              }
            }
          }
          if (line != null) {
            RCSystem.Log.Record (state.Closure, "exec", Handle, "readx", line);
            state.Runner.Yield (state.Closure, new RCString (line));
          }
          else {
            _outputReaders.Enqueue (state);
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
            if (_lines.Count > 0) {
              output = new RCString (_lines.ToArray ());
              _lines.Clear ();
            }
          }
          if (output != null) {
            RCSystem.Log.Record (_state.Closure, "exec", Handle, "line", output, forceDoc: true);
          }
          try
          {
            _timer.Change (200, Timeout.Infinite);
          }
          catch (ObjectDisposedException) { }
        }
        catch (Exception ex)
        {
          _state.Runner.Report (_state.Closure, ex);
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
          if (line == null) {
            lock (this)
            {
              if (output) {
                _outputDone = true;
              }
              else {
                _errorDone = true;
              }
            }
            Finish (null);
          }
          else {
            lock (this)
            {
              _result.Write (line);
              _lines.Enqueue (line);
              readers = _outputReaders.ToArray ();
              _outputReaders.Clear ();
            }
            for (int i = 0; i < readers.Length; i++)
            {
              ReadLineFromOutput (readers[i]);
            }
          }
        }
        catch (Exception ex)
        {
          _state.Runner.Report (_state.Closure, ex);
        }
      }
    }
  }
}

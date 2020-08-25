
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Mono.Terminal;
#if __MonoCS__
using Mono.Unix;
#endif
using RCL.Kernel;

namespace RCL.Exe
{
  /// <summary>
  /// The entry point for the rcl interpreter
  /// </summary>
  public class Program : MarshalByRefObject
  {
    public string IsolateCode;
    public string IsolateResult;

    /// <summary>
    /// Autocomplete handler for Mono.Terminal (Autocomplete is not implemented yet)
    /// </summary>
    static LineEditor.Completion AutoComplete (string text, int pos)
    {
      return new LineEditor.Completion ("RCL>", new string[] {text, text + "foo"});
    }

    public static void Main (string[] argv)
    {
      Program program = new Program ();
      program.InstanceMain (argv, "You should never see this text");
    }

    /// <summary>
    /// Entry point for the rcl interpreter
    /// </summary>
    public void InstanceMain (string[] argv, string appDomainVersionString)
    {
      string flags = Environment.GetEnvironmentVariable ("RCL_FLAGS");
      RCLArgv cmd;
      if (flags != null) {
        string[] flagsv = flags.Split (' ');
        string[] newArgv = new string[flagsv.Length + argv.Length];
        for (int i = 0; i < flagsv.Length; ++i)
        {
          newArgv[i] = flagsv[i];
        }
        for (int i = 0; i < argv.Length; ++i)
        {
          newArgv[flagsv.Length + i] = argv[i];
        }
        cmd = RCLArgv.Init (newArgv);
      }
      else {
        cmd = RCLArgv.Init (argv);
      }
      // Someday do color output like this
      // string message = "\x1b[0;33mYELLOW\x1b[0;31m RED\x1b[0;34m BLUE\x1b[0;37m";

      // Initialize runner environment
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler (
        UnhandledException);
      string prompt = "RCL>";
      LineEditor editor = new LineEditor ("RCL");
      RCRunner runner = new RCRunner (workers: 1);
      InstallSignalHandler (runner);
      cmd.PrintStartup (appDomainVersionString);

      string line = "";
      if (cmd.Program != "" || IsolateCode != null) {
        int status = 0;
        RCValue codeResult = null;

        try
        {
          RCValue code = null;

          if (IsolateCode != null) {
            code = runner.Read (IsolateCode);
          }
          else if (cmd.Program != "") {
            string file = File.ReadAllText (cmd.Program, Encoding.UTF8);
            code = runner.Read (file);
          }
          codeResult = runner.Rep (code, restoreStateOnError: true);
          if (cmd.Action != "") {
            RCValue result = runner.RepAction (cmd.Action);
            if (cmd.OutputEnum != RCOutput.Clean && !cmd.NoResult) {
              Console.Out.WriteLine (result.Format (RCFormat.Pretty, RCSystem.Log.GetColmap ()));
            }
          }
          if (cmd.Batch && !cmd.Exit) {
            Thread.Sleep (Timeout.Infinite);
          }
          if (cmd.Batch) {
            status = runner.ExitStatus ();
            runner.Dispose ();
            Environment.Exit (status);
          }
          // otherwise go on and keep listening for further commands.
        }
        catch (ThreadAbortException)
        {
          status = runner.ExitStatus ();
          runner.Dispose ();
          Environment.Exit (status);
        }
        catch (ArgumentException ex)
        {
          // This is for when the action name is not in _state.
          RCSystem.Log.Record (0, 0, "runner", 0, "fatal", ex);
          Environment.Exit (1);
        }
        catch (FileNotFoundException ex)
        {
          // This for when the program file cannot be read.
          RCSystem.Log.Record (0, 0, "runner", 0, "fatal", ex);
          Environment.Exit (1);
        }
        catch (RCSyntaxException ex)
        {
          // Program file has bad syntax
          RCSystem.Log.Record (0, 0, "runner", 0, "fatal", ex);
          Environment.Exit (2);
        }
        catch (Exception ex)
        {
          // For all other exceptions keep the process open unless instructed to --exit.
          // This is so you can hack around in the environment.
          // Does this result in duplicate exception reports on the console?
          // I don't want it to, but without this there are errors that do not show up at
          // all.
          RCSystem.Log.Record (0, 0, "fiber", 0, "fatal", ex);
          status = runner.ExitStatus ();
          if (IsolateCode != null) {
            AppDomain.CurrentDomain.SetData ("IsolateException", ex);
          }
        }
        finally
        {
          if (codeResult != null) {
            IsolateResult = codeResult.ToString ();
            AppDomain.CurrentDomain.SetData ("IsolateResult", IsolateResult);
          }

          if (cmd.Exit) {
            runner.Dispose ();
            Environment.Exit (status);
          }
        }

        if (IsolateCode != null) {
          // When running isolated, do not call Environment.Exit because it would close
          // the entire
          // process.
          runner.Dispose ();
          return;
        }
      }
      else if (cmd.Exit && !cmd.Batch) {
        int status = runner.ExitStatus ();

        runner.Dispose ();
        // This means there is no program and no input from stdin.
        // The process simply starts and then stops.
        // There is no way external way to cause an error to be generated,
        // so there is no test for the possible non-zero status result.
        Environment.Exit (status);
      }

      // Process batch (standard input) and interactive commands.
      while (true)
      {
        int status = 0;

        try
        {
          if (cmd.Batch) {
            StringBuilder text = new StringBuilder ();

            // Read all commands from standard input.
            while (true)
            {
              line = Console.ReadLine ();
              if (line == null) {
                break;
              }
              text.AppendLine (line);
            }

            bool fragment;
            RCValue code = RCSystem.Parse (text.ToString (), out fragment);
            RCValue codeResult = runner.Rep (code, restoreStateOnError: true);

            if (cmd.Action != "") {
              RCValue actionResult = runner.RepAction (cmd.Action);
              if (cmd.OutputEnum != RCOutput.Clean && !cmd.NoResult) {
                Console.Out.WriteLine (actionResult.Format (RCFormat.Pretty,
                                                            RCSystem.Log.GetColmap ()));
              }
            }
            else if (codeResult != null && !cmd.NoResult) {
              Console.Out.WriteLine (codeResult.Format (RCFormat.Pretty,
                                                        RCSystem.Log.GetColmap ()));
            }
            if (cmd.Exit) {
              status = runner.ExitStatus ();
              runner.Dispose ();
              Environment.Exit (status);
            }
          }
          else {
            if (cmd.NoKeys) {
              // No read requires nokeys to have an effect, obvs.
              if (cmd.NoRead) {
                Thread.Sleep (Timeout.Infinite);
              }
              else {
                line = Console.ReadLine ();
              }
            }
            else {
              line = editor.Edit (prompt, "");
            }

            _firstSigint = false;

            if (line != null) {
              string trimmed = line.TrimStart (' ').TrimEnd (' ');
              line = Alias (trimmed, runner, cmd);
              RCValue result = runner.Rep (line, restoreStateOnError: false);

              if (result != null) {
                Console.Out.WriteLine (result.Format (RCFormat.Pretty, RCSystem.Log.GetColmap ()));
              }
            }
            else {
              break;
            }
          }
        }
        catch (ThreadAbortException)
        {
          status = runner.ExitStatus ();
          runner.Dispose ();

          // This prevents the last RCL prompt from appearing on the same line as the next
          // bash
          // prompt.
          // I want to do something so that log output *never* appears on the same line as
          // the
          // prompt.
          Console.Out.Flush ();
          Environment.Exit (status);
        }
        catch (Exception ex)
        {
          // Prevent having duplicate output in the log for these.
          // Also allow the runner to report this exception and count it towards
          // determination of
          // exit status.
          if (!runner.RunnerUnhandled) {
            runner.Report (ex, "unhandled");
          }
        }
      }
      runner.Dispose ();
      Environment.Exit (0);
    }

    /// <summary>
    /// True if we have already received one SIGINT.
    /// </summary>
    protected static volatile bool _firstSigint;

    /// <summary>
    /// Launches a background thread to listen for POSIX signals.
    /// Exit on two signal 2's or one signal 15.
    /// </summary>
    static void InstallSignalHandler (RCRunner runner)
    {
#if __MonoCS__
      UnixSignal[] signals = {
        new UnixSignal (Mono.Unix.Native.Signum.SIGTERM),
        new UnixSignal (Mono.Unix.Native.Signum.SIGINT)
      };
      Thread signalThread = new Thread (delegate ()
      {
        while (true)
        {
          int index = UnixSignal.WaitAny (signals, -1);
          if (index < 0) {
            continue;
          }
          Mono.Unix.Native.Signum signal = signals[index].Signum;
          if (signal == Mono.Unix.Native.Signum.SIGTERM) {
            ThreadPool.QueueUserWorkItem (delegate (object state)
            {
              RCSystem.Log.Record (0, 0, "runner", 0, "signal", "SIGTERM");
              runner.Abort (15);
            });
          }
          else if (signal == Mono.Unix.Native.Signum.SIGINT) {
            if (!_firstSigint) {
              _firstSigint = true;
              ThreadPool.QueueUserWorkItem (delegate (object state)
              {
                RCSystem.Log.Record (0, 0, "runner", 0, "signal", "SIGINT");
                runner.Interupt ();
              });
            }
            else {
              ThreadPool.QueueUserWorkItem (delegate (object state)
              {
                RCSystem.Log.Record (0, 0, "runner", 0, "signal", "SIGINT (exiting)");
                runner.Abort (2);
              });
            }
          }
        }
      });
      signalThread.IsBackground = true;
      signalThread.Start ();
#endif
    }

    /// <summary>
    /// Write to the Console. Do not exit.
    /// </summary>
    static void UnhandledException (object sender, UnhandledExceptionEventArgs e)
    {
      Console.Out.WriteLine ("RCL Unhandled:\n" + e.ExceptionObject.ToString ());
    }

    /// <summary>
    /// Some corny bash-like aliases for various things. We should get rid of this or do
    /// something better.
    /// </summary>
    static string Alias (string trimmed, RCRunner runner, RCLArgv cmd)
    {
      string line = trimmed;
      if (trimmed == "exit") {
        line = "exit 0";
      }
      else if (trimmed == "ls") {
        line = "exec \"ls\"";
      }
      else if (trimmed.StartsWith ("ls")) {
        string path = GetPathArgument ("ls", trimmed);
        line = string.Format ("exec \"ls {0}\"", path);
      }
      else if (trimmed.StartsWith ("cd")) {
        string path = GetPathArgument ("cd", trimmed);
        // This prevents conflicting with the syntax for the internal cd command.
        if (path.Length > 0 && path[0] == '"') {
          return trimmed;
        }
        line = string.Format ("cd \"{0}\"", path);
      }
      else if (trimmed == "lsl") {
        line = "cube list #files";
      }
      else if (trimmed == "pwd") {
        line = "pwd {}";
      }
      else if (trimmed == "quiet" ||
               trimmed == "single" ||
               trimmed == "multi" ||
               trimmed == "full" ||
               trimmed == "clean") {
        RCOutput level = (RCOutput) Enum.Parse (typeof (RCOutput), trimmed, true);
        RCSystem.Log.SetVerbosity (level);
      }
      else if (trimmed == "help") {
        Console.WriteLine ("I am trying to be helpful, these are the command line arguments.");
        Console.WriteLine (cmd.Options.Format (RCFormat.Pretty));
      }
      else if (trimmed == "begin") {
        // Is this useful or even correct?
        StringBuilder text = new StringBuilder ();
        string docline = Console.ReadLine ();
        while (docline != "end")
        {
          text.AppendLine (docline);
          docline = Console.ReadLine ();
        }
        line = text.ToString ();
      }
      else if (trimmed.StartsWith ("epl")) {
        string path = GetPathArgument ("epl", trimmed);
        line = string.Format ("eval parse load \"{0}\"", path);
      }
      else if (trimmed.StartsWith ("fepl")) {
        string path = GetPathArgument ("fepl", trimmed);
        line = string.Format ("fiber {{<-eval parse load \"{0}\"}}", path);
      }
      else if (trimmed.StartsWith ("reset")) {
        // This is the one operation that cannot be done with an operator.
        // line = "reset 0l";
        runner.Reset ();
      }
      else if (trimmed.StartsWith (".")) {
        if (trimmed == "..") {
          line = "cd \"..\"";
        }
        else {
          line = string.Format ("cd \"{0}\"", trimmed.Substring (1));
        }
      }
      return line;
    }

    /// <summary>
    /// Handle paths expressed in unescaped syntax.
    /// </summary>
    static string GetPathArgument (string alias, string trimmed)
    {
      for (int i = alias.Length; i < trimmed.Length; ++i)
      {
        if (trimmed[i] != ' ') {
          return trimmed.Substring (i);
        }
      }
      throw new Exception ("Not a valid path command: " + trimmed);
    }
  }
}

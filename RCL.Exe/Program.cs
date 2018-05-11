
using System;
using System.IO;
using System.Text;
using System.Threading;
using Mono.Terminal;
#if __MonoCS__
using Mono.Unix;
#endif
using RCL.Kernel;

namespace RCL.Exe
{
  public class Isolate
  {
    [RCVerb ("isolate")]
    public void EvalIsolate (RCRunner runner, RCClosure closure, RCBlock right)
    {
      DoIsolate (runner, closure, new string[] {}, right);
    }

    [RCVerb ("isolate")]
    public void EvalIsolate (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      DoIsolate (runner, closure, left.ToArray (), right);
    }

    protected void DoIsolate (RCRunner runner, RCClosure closure, string[] argv, RCValue code)
    {
      Thread thread = new Thread (delegate ()
      {
        string result = null;
        AppDomain appDomain = null;
        Program program;
        try
        {
          AppDomainSetup setupInfo = new AppDomainSetup ();
          string build = "dev";
          if (argv.Length > 0)
          {
            string first = argv[0];
            if (first == "dev")
            {
              build = first;
              // It should use the dev build in this case. The current build is not the dev build.
              setupInfo = AppDomain.CurrentDomain.SetupInformation;
            }
            else if (first == "last")
            {
              throw new NotImplementedException ("last option is not yet implemented. Please specify a build number");
            }
            else
            {
              string rclHome = Environment.GetEnvironmentVariable ("RCL_HOME");
              if (rclHome == null)
              {
                throw new Exception ("RCL_HOME was not set. It is needed in order to locate the specified binary: " + first);
              }
              int number;
              if (int.TryParse (first, out number))
              {
                build = number.ToString ();
                setupInfo.ApplicationBase = rclHome + "/bin/rcl_bin/" + number + "/lib";
              }
              else
              {
                setupInfo = AppDomain.CurrentDomain.SetupInformation;
              }
            }
          }
          string appDomainName = "Isolated:" + Guid.NewGuid ();
          appDomain = AppDomain.CreateDomain (appDomainName, null, setupInfo);
          Type type = typeof (Program);
          program = (Program) appDomain.CreateInstanceAndUnwrap (type.Assembly.FullName, type.FullName);
          program.IsolateCode = code.ToString ();
          program.InstanceMain (argv);
          result = (string) appDomain.GetData ("IsolateResult");
        }
        catch (Exception ex)
        {
          runner.Finish (closure, ex, 1);
        }
        finally
        {
          if (appDomain != null)
          {
            AppDomain.Unload (appDomain);
            appDomain = null;
          }
        }
        if (result == null)
        {
          runner.Finish (closure, new Exception ("Missing IsolateResult for program"), 1);
        }
        runner.Yield (closure, runner.Peek (result));
      });
      thread.IsBackground = true;
      thread.Start ();
    }
  }

  /// <summary>
  /// The rcl Interpreter
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
      program.InstanceMain (argv);
    }

    /// <summary>
    /// Entry point for the rcl interpreter
    /// </summary>
    public void InstanceMain (string[] argv)
    {
      string flags = Environment.GetEnvironmentVariable ("RCL_FLAGS");
      RCLArgv cmd;
      if (flags != null)
      {
        string[] flagsv = flags.Split (' ');
        string[] newArgv = new string [flagsv.Length + argv.Length];
        for (int i = 0; i < flagsv.Length; ++i)
        {
          newArgv[i] = flagsv[i];
        }
        for (int i = 0; i < argv.Length; ++i)
        {
          newArgv[flagsv.Length + i] = argv[i];
        }
        cmd = new RCLArgv (newArgv);
      }
      else
      {
        cmd = new RCLArgv (argv);
      }

      //string message = "\x1b[0;33mYELLOW\x1b[0;31m RED\x1b[0;34m BLUE\x1b[0;37m";
      AppDomain.CurrentDomain.UnhandledException += 
        new UnhandledExceptionEventHandler (UnhandledException);

      string prompt = "RCL>";
      LineEditor editor = new LineEditor ("RCL");
      RCLogger consoleLog = new RCLogger (cmd.Nokeys, cmd.Show);
      RCLog log = new RCLog (consoleLog);
      RCRunner runner = new RCRunner (RCActivator.Default, log, 1, cmd);
      InstallSignalHandler (runner);
      cmd.PrintStartup ();

      string line = "";
      if (cmd.Program != "" || IsolateCode != null)
      {
        int status = 0;
        RCValue codeResult = null;
        try
        {
          RCValue code = null;
          if (IsolateCode != null)
          {
            code = runner.Read (IsolateCode);
          }
          else if (cmd.Program != "")
          {
            string file = File.ReadAllText (cmd.Program, Encoding.UTF8);
            code = runner.Read (file);
          }
          codeResult = runner.Rep (code);
          if (cmd.Action != "")
          {
            RCValue result = runner.Rep (string.Format ("{0} #", cmd.Action));
            if (cmd.OutputEnum != RCOutput.Clean)
            {
              string text = result.Format (RCFormat.Pretty, log.GetColmap ());
              Console.Out.WriteLine (text);
            }
          }
          if (cmd.Batch && !cmd.Exit)
          {
            Thread.Sleep (Timeout.Infinite);
          }
          if (cmd.Batch)
          {
            runner.Dispose ();
            Environment.Exit (0);
          }
          //otherwise go on and keep listening for further commands.
        }
        catch (ThreadAbortException)
        {
          status = runner.ExitStatus ();
          runner.Dispose ();
          Environment.Exit (status);
        }
        catch (Exception ex)
        {
          // Does this result in duplicate exception reports on the console?
          // I don't want it to, but without this there are errors that do not show up at all.
          RCLogger.RecordFilter (0, 0, "runner", 0, "fatal", ex);
          status = 1;
        }
        finally
        {
          IsolateResult = codeResult.ToString ();
          AppDomain.CurrentDomain.SetData ("IsolateResult", IsolateResult);
          if (cmd.Exit)
          {
            runner.Dispose ();
            Environment.Exit (status);
          }
        }
        if (IsolateCode != null)
        {
          // When we are running isolated, Environment.Exit would close the entire process.
          // Don't do that.
          runner.Dispose ();
          return;
        }
      }
      else if (cmd.Exit && !cmd.Batch)
      {
        runner.Dispose ();
        Environment.Exit (0);
      }

      while (true)
      {
        int status = 0;
        try
        {
          if (cmd.Batch)
          {
            StringBuilder text = new StringBuilder ();
            while (true)
            {
              line = Console.ReadLine ();
              if (line == null)
              {
                break;
              }
              text.AppendLine (line);
            }
            bool fragment;
            RCValue code = runner.Peek (text.ToString (), out fragment);
            RCValue result = runner.Rep (code);
            if (result != null)
            {
              Console.Out.WriteLine (result.Format (RCFormat.Pretty, log.GetColmap ()));
            }
            if (cmd.Exit)
            {
              runner.Dispose ();
              Environment.Exit (0);
            }
          }
          else
          {
            if (cmd.Nokeys)
            {
              //No read requires nokeys to have an effect, obvs.
              if (cmd.Noread)
              {
                Thread.Sleep (Timeout.Infinite);
              }
              else
              {
                line = Console.ReadLine ();
              }
            }
            else
            {
              line = editor.Edit (prompt, "");
            }
            m_firstSigint = false;
            if (line != null)
            {
              string trimmed = line.TrimStart (' ').TrimEnd (' ');
              line = Alias (trimmed, runner, consoleLog, cmd);
              RCValue result = runner.Rep (line);
              if (result != null)
              {
                Console.Out.WriteLine (result.Format (RCFormat.Pretty, log.GetColmap ()));
              }
            }
            else break;
          }
        }
        catch (ThreadAbortException)
        {
          status = runner.ExitStatus ();
          runner.Dispose ();
          //Prevents last RCL prompt from appearing on the same line as the next bash prompt.
          //But I want to do something so that log output *never* appears on the same line as the prompt.
          Console.Out.Flush ();
          Environment.Exit (status);
        }
        catch (Exception ex)
        {
          RCLogger.RecordFilter (0, 0, "fiber", 0, "reported", ex);
        }
      }
      runner.Dispose ();
      Environment.Exit (0);
    }

    /// <summary>
    /// Remember if we have already received one SIGINT
    /// </summary>
    protected static volatile bool m_firstSigint;

    /// <summary>
    /// Launches a background thread to listen for POSIX signals. Exit on two signal 2's
    /// or one signal 15.
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
          if (index < 0)
          {
            continue;
          }
          Mono.Unix.Native.Signum signal = signals[index].Signum;
          if (signal == Mono.Unix.Native.Signum.SIGTERM)
          {
            ThreadPool.QueueUserWorkItem (delegate (object state) 
            {
              runner.Log.Record (runner, null, "runner", 0, "signal", "SIGTERM");
              runner.Abort (15);
            });
          }
          else if (signal == Mono.Unix.Native.Signum.SIGINT)
          {
            if (!m_firstSigint)
            {
              m_firstSigint = true;
              ThreadPool.QueueUserWorkItem (delegate (object state) 
              {
                runner.Log.Record (runner, null, "runner", 0, "signal", "SIGINT");
                runner.Interupt ();
              });
            }
            else
            {
              ThreadPool.QueueUserWorkItem (delegate (object state) 
              {
                runner.Log.Record (runner, null, "runner", 0, "signal", "SIGINT (exiting)");
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
    static string Alias (string trimmed, RCRunner runner, RCLogger output, RCLArgv cmd)
    {
      string line = trimmed;
      if (trimmed == "exit")
      {
        line = "exit 0";
      }
      else if (trimmed == "ls")
      {
        line = "exec \"ls\"";
      }
      else if (trimmed.StartsWith ("ls"))
      {
        string path = GetPathArgument ("ls", trimmed);
        line = string.Format ("exec \"ls {0}\"", path);
      }
      else if (trimmed.StartsWith ("cd"))
      {
        string path = GetPathArgument ("cd", trimmed);
        //This prevents conflicting with the syntax for the internal cd command.
        if (path.Length > 0 && path [0] == '"')
        {
          return trimmed;
        }
        line = string.Format ("cd \"{0}\"", path);
      }
      else if (trimmed == "lsl")
      {
        line = "cube list #files";
      }
      else if (trimmed == "pwd")
      {
        line = "pwd {}";
      }
      else if (trimmed == "quiet" ||
               trimmed == "single" ||
               trimmed == "multi" ||
               trimmed == "full" ||
               trimmed == "clean")
      {
        RCOutput level = (RCOutput) Enum.Parse (typeof (RCOutput), trimmed, true);
        output.SetVerbosity (level);
      }
      else if (trimmed == "help")
      {
        Console.WriteLine ("I am trying to be helpful, these are the command line arguments.");
        Console.WriteLine (cmd.Options.Format (RCFormat.Pretty));
      }
      else if (trimmed == "begin")
      {
        //Is this useful or even correct?
        StringBuilder text = new StringBuilder ();
        string docline = Console.ReadLine ();
        while (docline != "end")
        {
          text.AppendLine (docline);
          docline = Console.ReadLine ();
        }
        line = text.ToString ();
      }
      else if (trimmed.StartsWith ("epl"))
      {
        string path = GetPathArgument ("epl", trimmed);
        line = string.Format ("eval parse load \"{0}\"", path);
      }
      else if (trimmed.StartsWith ("fepl"))
      {
        string path = GetPathArgument ("fepl", trimmed);
        line = string.Format ("fiber {{<-eval parse load \"{0}\"}}", path);
      }
      else if (trimmed.StartsWith ("reset"))
      {
        //This is the one operation that cannot be done with an operator.
        //line = "reset 0l";
        runner.Reset ();
      }
      else if (trimmed.StartsWith ("."))
      {
        if (trimmed == "..")
        {
          line = "cd \"..\"";
        }
        else
        {
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
        if (trimmed[i] != ' ')
        {
          return trimmed.Substring (i);
        }
      }
      throw new Exception ("Not a valid path command: " + trimmed);
    }
  }
}

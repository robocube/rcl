
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using Mono.Terminal;
using RCL.Kernel;
using RCL.Core;

namespace RCL.Exe
{
  public class Program
  {
    static LineEditor.Completion AutoComplete (string text, int pos)
    {
      return new LineEditor.Completion ("RCL>", new string[] {text, text + "foo"});
    }

    static void Main (string[] argv)
    {
      string flags = Environment.GetEnvironmentVariable ("RCL_FLAGS");
      RCLArgv cmd;
      if (flags != null)
      {
        string[] flagsv = flags.Split (' ');
        string[] newArgv = new string [argv.Length + flagsv.Length];
        for (int i = 0; i < argv.Length; ++i)
        {
          newArgv[i] = argv[i];
        }
        for (int i = 0; i < flagsv.Length; ++i)
        {
          newArgv[argv.Length + i] = flagsv[i];
        }
        cmd = new RCLArgv (newArgv);
      }
      else
      {
        cmd = new RCLArgv (argv);
      }

      //string message = "\x1b[0;33mYELLOW\x1b[0;31m RED\x1b[0;34m BLUE\x1b[0;37m";
      //Console.Out.WriteLine (message);
      AppDomain.CurrentDomain.UnhandledException += 
        new UnhandledExceptionEventHandler(UnhandledException);
      
      string prompt = "RCL>";
      LineEditor editor = new LineEditor ("RCL");
      Output consoleLog = new Output (editor, cmd.Show);
      RCLog log = new RCLog (consoleLog);
      RCRunner runner = new RCRunner (RCActivator.Default, log, 1, cmd);

      cmd.PrintStartup ();
  
      string line = "";
      if (cmd.Program != "")
      {
        int status = 0;
        try
        {
          string file = File.ReadAllText (cmd.Program, Encoding.UTF8);
          RCValue code = runner.Read (file);
          runner.Rep (code);
          if (cmd.Action != "")
          {
            RCValue result = runner.Rep (string.Format ("{0} #", cmd.Action));
            if (cmd.OutputEnum != RCOutput.Clean)
            {
              string text = result.Format (RCFormat.Pretty);
              Console.Out.WriteLine (text);
            }
          }
          if (cmd.Batch && !cmd.Exit)
          {
            Thread.Sleep (Timeout.Infinite);
          }
          if (cmd.Batch)
          {
            Environment.Exit (0);
          }
          //otherwise go on and keep listening for further commands.
        }
        catch (ThreadAbortException)
        {
          status = runner.ExitStatus ();
          Environment.Exit (status);
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine (ex.ToString ());
          status = 1;
        }
        finally
        {
          if (cmd.Exit)
          {
            Environment.Exit (status);
          }
        }
      }
      else if (cmd.Exit && !cmd.Batch)
      {
        Environment.Exit (0);
      }

      while (true)
      {
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
              Console.Out.WriteLine (result.Format (RCFormat.Pretty));
            }
            if (cmd.Exit)
            {
              Environment.Exit (0);
            }
          }
          else
          {
            if (cmd.Nokeys)
            {
              line = Console.ReadLine ();
            }
            else
            {
              line = editor.Edit (prompt, "");
            }
            if (line != null)
            {
              string trimmed = line.TrimStart (' ').TrimEnd (' ');
              line = Alias (trimmed, runner, consoleLog, cmd);
              RCValue result = runner.Rep (line);
              if (result != null)
              {
                Console.Out.WriteLine (result.Format (RCFormat.Pretty));
              }
            }
            else break;
          }
        }
        catch (ThreadAbortException)
        {
          int status = runner.ExitStatus ();
          Environment.Exit (status);
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine (ex.ToString ());
        }
      }
      Environment.Exit (0);
    }

    static void UnhandledException (object sender, UnhandledExceptionEventArgs e)
    {
      Console.Out.WriteLine ("CAUGHT BY RCL:" + e.ExceptionObject.ToString ());
    }

    static string Alias (string trimmed, RCRunner runner, Output output, RCLArgv cmd)
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
      else if (trimmed == "quiet" || trimmed == "single" || trimmed == "multi" || trimmed == "full" || trimmed == "clean")
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

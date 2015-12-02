
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

    static int Main (string[] argv)
    {
      RCBlock arguments = RCRunner.CreateArgs (argv);
      string output = ((RCString) arguments.Get ("output"))[0];
      string program = ((RCString) arguments.Get ("program"))[0];
      string action = ((RCString) arguments.Get ("action"))[0];
      bool batch = arguments.Get ("batch", null) != null;
      bool exit = arguments.Get ("exit", null) != null;

      string prompt = "RCL>";
      LineEditor editor = new LineEditor ("RCL");
      Output consoleLog = new Output (editor);
      RCOutput outputEnum = (RCOutput) Enum.Parse (typeof (RCOutput), output, true);
      consoleLog.SetVerbosity (outputEnum);
      RCLog log = new RCLog (consoleLog);
      RCRunner runner = new RCRunner (RCActivator.Default, log, 1, arguments);

      if (!batch && outputEnum != RCOutput.Clean)
      {
        Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        Console.Out.WriteLine ("RCL Version {0} Copyright Robocube Corporation 2015", version.ToString ());
        Console.Out.WriteLine ("If you need help, type 'help'. To exit RCL, type 'exit'.");
        Console.Out.WriteLine ("arguments:{0}", arguments.Format (RCFormat.Pretty));
      }
      string line = "";
      //editor.AutoCompleteEvent = new LineEditor.AutoCompleteHandler (AutoComplete);
      if (program != "")
      {
        int status = 0;
        try
        {
          string file = File.ReadAllText (program, Encoding.UTF8);
          RCValue code = runner.Read (file);
          runner.Rep (code);
          if (action != "")
          {
            RCValue result = runner.Rep (string.Format ("{0} #", action));
            if (outputEnum != RCOutput.Clean)
            {
              string text = result.Format (RCFormat.Pretty);
              Console.Out.WriteLine (text);
            }
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine (ex.ToString ());
          status = 1;
        }
        finally
        {
          if (exit)
          {
            Environment.Exit (status);
          }
        }
      }

      while (true)
      {
        try
        {
          if (batch)
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
            if (result != null) // && outputEnum != RCOutput.Clean)
            {
              Console.Out.WriteLine (result.Format (RCFormat.Pretty));
            }
            break;
          }
          else
          {
            if (outputEnum == RCOutput.Clean)
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
              line = Alias (trimmed, runner, consoleLog, arguments);
              RCValue result = runner.Rep (line);
              if (result != null) // && outputEnum != RCOutput.Clean)
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
      return 0;
    }

    static string Alias (string trimmed, RCRunner runner, Output output, RCBlock arguments)
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
        Console.WriteLine (arguments.Format (RCFormat.Pretty));
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
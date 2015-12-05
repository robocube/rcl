
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
      //RCBlock arguments = RCRunner.CreateArgs (argv);
      RCBlock arguments = GetOptions (argv);
      string output = ((RCString) arguments.Get ("output"))[0];
      string program = ((RCString) arguments.Get ("program"))[0];
      string action = ((RCString) arguments.Get ("action"))[0];
      bool batch = ((RCBoolean) arguments.Get ("batch"))[0];
      bool exit = ((RCBoolean) arguments.Get ("exit"))[0];

      string prompt = "RCL>";
      LineEditor editor = new LineEditor ("RCL");
      Output consoleLog = new Output (editor);
      RCOutput outputEnum = (RCOutput) Enum.Parse (typeof (RCOutput), output, true);
      consoleLog.SetVerbosity (outputEnum);
      RCLog log = new RCLog (consoleLog);
      RCRunner runner = new RCRunner (RCActivator.Default, log, 1, arguments);

      if (!batch && outputEnum != RCOutput.Clean)
      {
        Console.WriteLine ();
        PrintCopyright ();
        Console.WriteLine ();
        Console.WriteLine ("Options:");
        Console.WriteLine (arguments.Format (RCFormat.Pretty));
        Console.WriteLine ();
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

    static void PrintCopyright ()
    {
      Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
      Console.Out.WriteLine ("RCL Version {0}", version.ToString ());
      Console.Out.WriteLine ("  Copyright (C) 2007-2015 Brian M. Andersen");
      Console.Out.WriteLine ("  Copyright (C) 2015-2016 Robocube Corporation");
    }

    static RCBlock GetOptions (string[] argv)
    {
      bool exit = false;
      bool batch = false;
      string program = "";
      string action = "";
      string output = "full";
      for (int i = 0; i < argv.Length; ++i)
      {
        string[] kv = argv[i].Split ('=');
        if (kv.Length == 1)
        {
          if (kv[0].StartsWith ("--"))
          {
            string option = kv[0].Substring (2);
            if (option.Equals ("exit"))
            {
              exit = true;
            }
            else if (option.Equals ("batch"))
            {
              batch = true;
            }
            else
            {
              Usage ("Unknown option '" + option + "'");
            }
          }
          else if (kv[0].StartsWith ("-"))
          {
            for (int j = 1; j < kv[0].Length; ++j)
            {
              char c;
              switch (c = kv[0][j])
              {
                case 'x': 
                  exit = true;
                  break;
                case 'b':
                  batch = true;
                  break;
                default :
                  Usage ("Unknown option '" + kv[0][j] + "'");
                  break;
              }
            }
          }
          else
          {
            //do position.
            if (program == "")
            {
              program = kv[0];
            }
            else
            {
              action = kv[0];
            }
          }
        }
        else
        {
          //do named arguments.
          if (kv[0].StartsWith ("--"))
          {
            string option = kv[0].Substring (2);
            if (option.Equals ("program"))
            {
              program = kv[1];
            }
            else if (option.Equals ("action"))
            {
              action = kv[1];
            }
            else if (option.Equals ("output"))
            {
              output = kv[1];
            }
            else
            {
              Usage ("Unknown option '" + option + "'");
            }
          }
          else Usage ("Named options start with --");
        }
      }
      RCBlock result = RCBlock.Empty;
      result = new RCBlock (result, "program", ":", new RCString (program));
      result = new RCBlock (result, "action", ":", new RCString (action));
      result = new RCBlock (result, "output", ":", new RCString (output));
      result = new RCBlock (result, "batch", ":", new RCBoolean (batch));
      result = new RCBlock (result, "exit", ":", new RCBoolean (exit));
      return result;
    }

    static void Usage (string message)
    {
      PrintCopyright ();
      Console.WriteLine ();
      Console.WriteLine ("Usage: rcl [OPTIONS] program action");
      Console.WriteLine ("  {0}", message);
      Console.WriteLine ();
      Console.WriteLine ("Options:");
      Console.WriteLine ("  --program         Program file to run on startup");
      Console.WriteLine ("  --action          Operator within program to eval on launch");
      Console.WriteLine ("  --output          Output format [full|multi|single|clean|none]");
      Console.WriteLine ("  --batch, -b       Non-interactive console (use when reading from standard input)");
      Console.WriteLine ("  --exit, -x        Exit after completion of action");
      Console.WriteLine ();
      Environment.Exit (1);
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
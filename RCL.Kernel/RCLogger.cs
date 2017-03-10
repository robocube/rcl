using System;
using System.IO;
using System.Text;

namespace RCL.Kernel
{
  public class RCLogger
  {
    protected static bool m_nokeys = false;
    protected static TextWriter m_output;
    protected static RCArray<string> m_types;
    protected static RCOutput m_level = RCOutput.Full;
    protected static string TimeFormat = "yyyy.MM.dd HH:mm:ss.fffffff";

    public RCLogger () : this (true, "*") {}

    public RCLogger (bool nokeys, params string[] whiteList)
    {
      m_nokeys = nokeys;
      m_types = new RCArray<string> (whiteList);
      if (!m_nokeys)
      {
        m_output = Console.Error;
      }
      else
      {
        m_output = Console.Out;
      }
    }

    public RCArray<string> Types ()
    {
      return m_types;
    }

    public void SetVerbosity (RCOutput level)
    {
      m_level = level;
    }

    public void Record (RCRunner runner,
                        RCClosure closure,
                        string type,
                        long instance,
                        string state,
                        object info)
    {
      long bot = 0;
      long fiber = 0;
      if (closure != null)
      {
        bot = closure.Bot.Id;
        fiber = closure.Fiber;
      }
      Record (bot, fiber, type, instance, state, info);
    }

    public static void Record (long bot,
                               long fiber,
                               string type,
                               long instance,
                               string state,
                               object info)
    {
      if (m_output == null)
      {
        return;
      }
      if (m_level == RCOutput.Quiet)
      {
        return;
      }
      else if (!m_nokeys && bot == 0 && type == "fiber" && instance == 0 && 
               (state == "start" || state == "done" || state == "reported"))
      {
        return;
      }
      else if (m_level == RCOutput.Single)
      {
        string time = DateTime.UtcNow.ToString (TimeFormat);
        m_output.WriteLine ("{0} {1} {2} {3} {4} {5} {6}",
                            time, bot, fiber, type, instance, state, info.ToString ());
      }
      else if (m_level == RCOutput.Multi || m_level == RCOutput.Full)
      {
        string time = DateTime.UtcNow.ToString (TimeFormat);
        bool singleLine;
        string message = IndentMessage (CreateMessage (info), out singleLine);
        string optionalSpace = (singleLine && message.Length > 0) ? ":" : "";
        m_output.WriteLine ("{0} {1} {2} {3} {4} {5}{6}{7}",
                            time, bot, fiber, type, instance, state, optionalSpace, message);
      }
      else if (m_level == RCOutput.Clean && type == "print")
      {
        string message = CreateMessage (info);
        m_output.WriteLine (message);
      }
      else if (m_level == RCOutput.Test)
      {
        bool singleLine;
        string message = IndentMessage (CreateMessage (info), out singleLine);
        string optionalSpace = (singleLine && message.Length > 0) ? ":" : "";
        m_output.WriteLine ("{0} {1} {2} {3} {4}{5}{6}", 
                            bot, fiber, type, instance, state, optionalSpace, message);        
      }
      else if (m_level == RCOutput.Trace)
      {
        throw new NotImplementedException ();
      }
    }

    protected static string CreateMessage (object info)
    {
      //Not the most efficient solution possible, but it looks pretty good on the console.
      string message;
      RCBlock block = info as RCBlock;
      if (block != null)
      {
        if (block.Count == 1 && block.Evaluator == RCEvaluator.Yield)
        {
          message = block.Format (RCFormat.Default);
        }
        else
        {
          message = block.Format (RCFormat.Pretty);
        }
      }
      else if (info is RCCube)
      {
        message = ((RCValue) info).Format (RCFormat.Pretty);
      }
      else if (info is RCException && m_level == RCOutput.Test)
      {
        message = ((RCException) info).ToTestString ();
      }
      else if (info is RCString)
      {
        RCString strings = (RCString) info;
        StringBuilder builder = new StringBuilder ();
        for (int i = 0; i < strings.Count; ++i)
        {
          builder.AppendLine (strings[i]);
        }
        if (builder.Length > 0)
        {
          builder.Remove (builder.Length - 1, 1);
        }
        message = builder.ToString ();
      }
      else
      {
        message = info.ToString ();
      }
      return message;
    }

    protected static string IndentMessage (string message, out bool singleLine)
    {
      string[] lines = message.Split ('\n', '\r');
      int chars = 0;
      if (lines.Length == 1)
      {
        singleLine = true;
        return lines[0];
      }
      singleLine = false;
      StringBuilder output = new StringBuilder ();
      output.AppendLine ();
      for (int i = 0; i < lines.Length; ++i)
      {
        string line = string.Format ("  {0}\n", lines[i]);
        if (m_level == RCOutput.Multi)
        {
          if ((chars + line.Length) > 512)
          {
            output.Append ("................................................................................");
            return output.ToString ();
          }
        }
        output.Append (line);
        chars += line.Length;
      }
      if (output.Length > 0)
      {
        output.Remove (output.Length - 1, 1);
      }
      return output.ToString ();
    }
  }
}

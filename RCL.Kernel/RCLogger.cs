using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace RCL.Kernel
{
  public class RCColmap
  {
    protected class DisplayCol
    {
      public readonly string Display;
      public readonly string Format;

      public DisplayCol (string display, string format)
      {
        Display = display;
        Format = format;
      }
    }

    protected Dictionary<string, DisplayCol> m_displayCols = new Dictionary<string, DisplayCol> ();

    public RCColmap Update (RCArray<string> column, RCArray<string> format)
    {
      RCColmap result = new RCColmap ();
      result.m_displayCols = this.m_displayCols;
      foreach (string key in m_displayCols.Keys)
      {
        result.m_displayCols[key] = m_displayCols[key];
      }
      m_displayCols = new Dictionary<string, DisplayCol> ();
      for (int i = 0; i < column.Count; i++)
      {
        result.m_displayCols[column[i]] = new DisplayCol (column[i], format[i]);
      }
      return result;
    }

    public string GetDisplayFormat (string column)
    {
      if (column == null)
      {
        throw new ArgumentNullException ("column");
      }
      DisplayCol col;
      if (m_displayCols.TryGetValue (column, out col))
      {
        return col.Format;
      }
      return null;
    }
  }

  public class RCLogger
  {
    protected static object m_lock = new object ();
    protected static bool m_nokeys;
    protected static TextWriter m_output;
    protected static RCArray<string> m_types;
    protected static RCOutput m_level = RCOutput.Full;
    protected readonly static string TimeFormat = "yyyy.MM.dd HH:mm:ss.ffffff";
    protected static HashSet<string> m_show;
    protected static RCColmap m_colmap = new RCColmap ();

    public RCLogger () : this (true, "*") {}

    public RCLogger (bool nokeys, params string[] whiteList)
    {
      m_nokeys = nokeys;
      m_show = new HashSet<string> (whiteList);
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

    public void UpdateColmap (RCArray<string> column, RCArray<string> format)
    {
      lock (m_lock)
      {
        m_colmap = m_colmap.Update (column, format);
      }
    }

    /// <summary>
    /// This is threadsafe because the RCColmap instances are immutable
    /// </summary>
    public RCColmap GetColmap ()
    {
      return m_colmap;
    }

    public static void RecordFilter (RCRunner runner,
                                     RCClosure closure,
                                     string type,
                                     long instance,
                                     string state,
                                     object info)
    {
      RecordFilter (runner, closure, type, instance, state, info, false);
    }

    public static void RecordFilter (RCRunner runner,
                                     RCClosure closure,
                                     string type,
                                     long instance,
                                     string state,
                                     object info,
                                     bool forceDoc)
    {
      lock (m_lock)
      {
        if (m_show.Contains ("*"))
        {
          Record (runner, closure, type, instance, state, info, forceDoc);
        }
        else if (m_show.Contains (type))
        {
          Record (runner, closure, type, instance, state, info, forceDoc);
        }
        else if (m_show.Contains (type + ":" + state))
        {
          Record (runner, closure, type, instance, state, info, forceDoc);
        }
      }
    }

    public static void RecordFilter (long bot,
                                     long fiber,
                                     string type,
                                     long instance,
                                     string state,
                                     object info)
    {
      RecordFilter (bot, fiber, type, instance, state, info, false);
    }

    public static void RecordFilter (long bot,
                                     long fiber,
                                     string type,
                                     long instance,
                                     string state,
                                     object info,
                                     bool forceDoc)
    {
      lock (m_lock)
      {
        if (m_show.Contains ("*"))
        {
          Record (bot, fiber, type, instance, state, info, forceDoc);
        }
        else if (m_show.Contains (type))
        {
          Record (bot, fiber, type, instance, state, info, forceDoc);
        }
        else if (m_show.Contains (type + ":" + state))
        {
          Record (bot, fiber, type, instance, state, info, forceDoc);
        }
      }
    }

    protected static void Record (RCRunner runner,
                                  RCClosure closure,
                                  string type,
                                  long instance,
                                  string state,
                                  object info,
                                  bool forceDoc)
    {
      long bot = 0;
      long fiber = 0;
      if (closure != null)
      {
        bot = closure.Bot;
        fiber = closure.Fiber;
      }
      Record (bot, fiber, type, instance, state, info, forceDoc);
    }

    protected static void Record (long bot,
                                  long fiber,
                                  string type,
                                  long instance,
                                  string state,
                                  object info)
    {
      Record (bot, fiber, type, instance, state, info, false);
    }

    protected static void Record (long bot,
                                  long fiber,
                                  string type,
                                  long instance,
                                  string state,
                                  object info,
                                  bool forceDoc)
    {
      if (m_output == null)
      {
        return;
      }
      if (m_level == RCOutput.Quiet)
      {
        return;
      }
      else if (bot == 0 && type == "fiber" && instance == 0 &&
               (state == "start" || state == "done"))
      {
        return;
      }
      else if (m_level == RCOutput.Single)
      {
        string time = DateTime.UtcNow.ToString (TimeFormat);
        if (info is Exception)
        {
          info = "<<Reported>>";
        }
        m_output.WriteLine ("{0} {1} {2} {3} {4} {5} {6}",
                            time, bot, fiber, type, instance, state, info.ToString ());
        Debug.WriteLine ("{0} {1} {2} {3} {4} {5} {6}",
                          time, bot, fiber, type, instance, state, info.ToString ());
      }
      else if (m_level == RCOutput.Systemd)
      {
				// 0 Emergency: system is unusable
				// 1 Alert: action must be taken immediately
				// 2 Critical: critical conditions
				// 3 Error: error conditions
				// 4 Warning: warning conditions
				// 5 Notice: normal but significant condition
				// 6 Informational: informational messages
				// 7 Debug: debug-level messages
        int severity = 6;
        string message;
        if (info is Exception)
        {
          if (type == "fiber" && state == "killed")
          {
            //It's important to use an exception here because this is
            //a non-standard termination; it relies on the regular process
            //for non-standard terminations.
            //But we assume you are killing the fiber because you want it dead,
            //so it shouldn't be treated like an error.
            //An important use case is canceling read commands over http
            severity = 6;
          }
          else
          {
            severity = 3;
          }
          RCException ex = info as RCException;
          if (ex != null)
          {
            message = ex.ToSystemdString ();
          }
          else
          {
            //If you do it like this it shows more stack but it is hard to read
            //message = new RCString (info.ToString ()).ToString ();
            message = info.ToString ();
          }
        }
        else
        {
          bool singleLine;
          message = IndentMessage (CreateMessage (info), false, out singleLine);
        }
        m_output.WriteLine ("<{0}>{1} {2} {3} {4} {5} {6}",
                            severity, bot, fiber, type, instance, state, message);
        Debug.WriteLine ("<{0}>{1} {2} {3} {4} {5} {6}",
                            severity, bot, fiber, type, instance, state, message);
      }
      else if (m_level == RCOutput.Multi || m_level == RCOutput.Full)
      {
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc (DateTime.UtcNow, RCTime.DisplayTimeZone);
        string time = now.ToString (TimeFormat);
        bool singleLine;
        string message = IndentMessage (CreateMessage (info), forceDoc, out singleLine);
        string optionalSpace = (singleLine && message.Length > 0) ? " " : "";
        m_output.WriteLine ("{0} {1} {2} {3} {4} {5}{6}{7}",
                            time, bot, fiber, type, instance, state, optionalSpace, message);
        Debug.WriteLine ("{0} {1} {2} {3} {4} {5}{6}{7}",
                            time, bot, fiber, type, instance, state, optionalSpace, message);
      }
      else if (m_level == RCOutput.Clean) //&& type == "print")
      {
        string message = CreateMessage (info);
        m_output.WriteLine (message);
        Debug.WriteLine (message);
      }
      else if (m_level == RCOutput.Test)
      {
        bool singleLine;
        string message = IndentMessage (CreateMessage (info), forceDoc, out singleLine);
        string optionalSpace = (singleLine && message.Length > 0) ? " " : "";
        m_output.WriteLine ("{0} {1} {2} {3} {4}{5}{6}", 
                            bot, fiber, type, instance, state, optionalSpace, message);        
        Debug.WriteLine ("{0} {1} {2} {3} {4}{5}{6}",
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
          message = block.Format (RCFormat.Default, m_colmap);
        }
        else
        {
          message = block.Format (RCFormat.Pretty, m_colmap);
        }
      }
      else if (info is RCCube)
      {
        message = ((RCValue) info).Format (RCFormat.Pretty, m_colmap);
      }
      else if (info is RCException && (m_level == RCOutput.Test || m_level == RCOutput.Single))
      {
        message = ((RCException) info).ToTestString ();
      }
      else if (info is Exception && (m_level == RCOutput.Test || m_level == RCOutput.Single))
      {
        message = "<<Reported>>";
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

    protected static string IndentMessage (string message, bool forceDoc, out bool singleLine)
    {
      string[] lines = message.Split ('\n', '\r');
      int chars = 0;
      if (lines.Length == 1 && !forceDoc)
      {
        singleLine = true;
        return lines[0];
      }
      singleLine = false;
      StringBuilder output = new StringBuilder ();
      output.AppendLine ();
      int til = lines.Length;
      if (lines.Length > 0 && lines[lines.Length - 1].Length == 0)
      {
        --til;
      }
      for (int i = 0; i < til; ++i)
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
      //Get rid of the final newline because we use AppendLine to do the output
      if (output.Length > 0)
      {
        output.Remove (output.Length - 1, 1);
      }
      return output.ToString ();
    }
  }
}

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace RCL.Kernel
{
  public class RCLogger
  {
    protected static object m_lock = new object ();
    protected static bool m_nokeys;
    protected static TextWriter m_output;
    protected static RCOutput m_level = RCOutput.Full;
    protected readonly static string TimeFormat = "yyyy.MM.dd HH:mm:ss.ffffff";
    protected static HashSet<string> m_show;
    protected static HashSet<string> m_hide;
    protected static RCColmap m_colmap = new RCColmap ();

    public RCLogger () : this (true, new string[] {"*"}, new string[] {}) {}

    public RCLogger (bool nokeys, string[] whiteList, string[] blacklist)
    {
      m_nokeys = nokeys;
      m_show = new HashSet<string> (whiteList);
      m_hide = new HashSet<string> (blacklist);
      if (!m_nokeys) {
        m_output = Console.Error;
      }
      else {
        m_output = Console.Out;
      }
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
    /// This is thread safe because the RCColmap instances are immutable.
    /// </summary>
    public RCColmap GetColmap ()
    {
      return m_colmap;
    }

    protected bool Filter (string type, string state)
    {
      lock (m_lock)
      {
        if (m_hide.Contains (type + ":" + state)) {
          return false;
        }
        else if (m_hide.Contains (type)) {
          return false;
        }
        else if (m_show.Contains ("*")) {
          return true;
        }
        else if (m_show.Contains (type)) {
          return true;
        }
        else if (m_show.Contains (type + ":" + state)) {
          return true;
        }
      }
      return false;
    }

    public void Record (RCClosure closure,
                        string type,
                        long instance,
                        string state,
                        object info)
    {
      Record (closure, type, instance, state, info, false);
    }

    public void Record (RCClosure closure,
                        string type,
                        long instance,
                        string state,
                        object info,
                        bool forceDoc)
    {
      long bot = 0;
      long fiber = 0;
      if (closure != null) {
        bot = closure.Bot;
        fiber = closure.Fiber;
      }
      Record (bot, fiber, type, instance, state, info, forceDoc);
    }

    public void Record (long bot,
                        long fiber,
                        string type,
                        long instance,
                        string state,
                        object info)
    {
      Record (bot, fiber, type, instance, state, info, false);
    }

    public void Record (long bot,
                        long fiber,
                        string type,
                        long instance,
                        string state,
                        object info,
                        bool forceDoc)
    {
      if (!Filter (type, state)) {
        return;
      }
      if (m_output == null) {
        return;
      }
      if (m_level == RCOutput.Quiet) {
        return;
      }
      else if (bot == 0 && instance == 0 && type == "fiber" &&
               (state == "start" || state == "done")) {
        return;
      }
      else if (m_level == RCOutput.Single) {
        string time = DateTime.UtcNow.ToString (TimeFormat);
        if (info is Exception) {
          info = "<<Reported>>";
        }
        m_output.WriteLine ("{0} {1} {2} {3} {4} {5} {6}",
                            time,
                            bot,
                            fiber,
                            type,
                            instance,
                            state,
                            info.ToString ());
        Debug.WriteLine ("{0} {1} {2} {3} {4} {5} {6}",
                         time,
                         bot,
                         fiber,
                         type,
                         instance,
                         state,
                         info.ToString ());
      }
      else if (m_level == RCOutput.Systemd) {
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
        if (info is Exception) {
          if (type == "fiber" && state == "killed") {
            // It's important to use an exception here because this is
            // a non-standard termination; it relies on the regular process
            // for non-standard terminations.
            // But we assume you are killing the fiber because you want it dead,
            // so it shouldn't be treated like an error.
            // An important use case is canceling read commands over http
            severity = 6;
          }
          else {
            severity = 3;
          }
          RCException ex = info as RCException;
          if (ex != null) {
            message = ex.ToSystemdString ();
          }
          else {
            // If you do it like this it shows more stack but it is hard to read
            // message = new RCString (info.ToString ()).ToString ();
            message = info.ToString ();
          }
        }
        else {
          bool singleLine;
          message = IndentMessage (CreateMessage (info), false, out singleLine);
        }
        m_output.WriteLine ("<{0}>{1} {2} {3} {4} {5} {6}",
                            severity,
                            bot,
                            fiber,
                            type,
                            instance,
                            state,
                            message);
        Debug.WriteLine ("<{0}>{1} {2} {3} {4} {5} {6}",
                         severity,
                         bot,
                         fiber,
                         type,
                         instance,
                         state,
                         message);
      }
      else if (m_level == RCOutput.Multi || m_level == RCOutput.Full) {
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc (DateTime.UtcNow, RCTime.DisplayTimeZone);
        string time = now.ToString (TimeFormat);
        bool singleLine;
        string message = IndentMessage (CreateMessage (info), forceDoc, out singleLine);
        string optionalSpace = (singleLine && message.Length > 0) ? " " : "";
        m_output.WriteLine ("{0} {1} {2} {3} {4} {5}{6}{7}",
                            time,
                            bot,
                            fiber,
                            type,
                            instance,
                            state,
                            optionalSpace,
                            message);
        Debug.WriteLine ("{0} {1} {2} {3} {4} {5}{6}{7}",
                         time,
                         bot,
                         fiber,
                         type,
                         instance,
                         state,
                         optionalSpace,
                         message);
      }
      else if (m_level == RCOutput.Clean && type == "print") {
        string message = CreateMessage (info);
        m_output.WriteLine (message);
        Debug.WriteLine (message);
      }
      else if (m_level == RCOutput.Test) {
        bool singleLine;
        string message = IndentMessage (CreateMessage (info), forceDoc, out singleLine);
        string optionalSpace = (singleLine && message.Length > 0) ? " " : "";
        m_output.WriteLine ("{0} {1} {2} {3} {4}{5}{6}",
                            bot,
                            fiber,
                            type,
                            instance,
                            state,
                            optionalSpace,
                            message);
        Debug.WriteLine ("{0} {1} {2} {3} {4}{5}{6}",
                         bot,
                         fiber,
                         type,
                         instance,
                         state,
                         optionalSpace,
                         message);
      }
      else if (m_level == RCOutput.Trace) {
        throw new NotImplementedException ();
      }
    }

    protected static string CreateMessage (object info)
    {
      // Not the most efficient solution possible, but it looks pretty good on the
      // console.
      string message;
      RCBlock block = info as RCBlock;
      if (block != null) {
        if (block.Count == 1 && block.Evaluator == RCEvaluator.Yield) {
          message = block.Format (RCFormat.Default, m_colmap);
        }
        else {
          message = block.Format (RCFormat.Pretty, m_colmap);
        }
      }
      else if (info is RCCube) {
        message = ((RCValue) info).Format (RCFormat.Pretty, m_colmap);
      }
      else if (info is RCException && (m_level == RCOutput.Test || m_level == RCOutput.Single)) {
        message = ((RCException) info).ToTestString ();
      }
      else if (info is Exception && (m_level == RCOutput.Test || m_level == RCOutput.Single)) {
        if (RCSystem.Args.FullStack) {
          message = string.Format ("<<Reported, {0}>>", info.ToString ());
        }
        else {
          message = "<<Reported>>";
        }
      }
      else if (info is RCString) {
        RCString strings = (RCString) info;
        StringBuilder builder = new StringBuilder ();
        for (int i = 0; i < strings.Count; ++i)
        {
          builder.AppendLine (strings[i]);
        }
        if (builder.Length > 0) {
          builder.Remove (builder.Length - 1, 1);
        }
        message = builder.ToString ();
      }
      else {
        message = info.ToString ();
      }
      return message;
    }

    protected static string IndentMessage (string message, bool forceDoc, out bool singleLine)
    {
      string[] lines = message.Split ('\n', '\r');
      int chars = 0;
      if (lines.Length == 1 && !forceDoc) {
        singleLine = true;
        return lines[0];
      }
      singleLine = false;
      StringBuilder output = new StringBuilder ();
      output.AppendLine ();
      int til = lines.Length;
      if (lines.Length > 0 && lines[lines.Length - 1].Length == 0) {
        --til;
      }
      for (int i = 0; i < til; ++i)
      {
        string line = string.Format ("  {0}\n", lines[i]);
        if (m_level == RCOutput.Multi) {
          if ((chars + line.Length) > 512) {
            output.Append (
              "................................................................................");
            return output.ToString ();
          }
        }
        output.Append (line);
        chars += line.Length;
      }
      // Get rid of the final newline because we use AppendLine to do the output
      if (output.Length > 0) {
        output.Remove (output.Length - 1, 1);
      }
      return output.ToString ();
    }
  }
}

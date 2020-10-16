
using System;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Collections.ObjectModel;
using RCL.Kernel;

namespace RCL.Core
{
  public class Command
  {
    [RCVerb ("path")]
    public void EvalPath (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (PathSymbolToLocalString (right[i]));
      }
      runner.Yield (closure, new RCString (result));
    }

    public static string PathSymbolToLocalString (RCSymbolScalar symbol)
    {
      string path = PathSymbolToString (Path.DirectorySeparatorChar, symbol);
      // I think I might need to do something with GetPathRoot
      // string root = Path.GetPathRoot (path);
      // return root + path;
      return path;
    }

    public static string PathSymbolToString (char separator, RCSymbolScalar symbol)
    {
      object[] parts = symbol.ToArray ();
      string path = "";
      string zero = parts[0].ToString ();
      int startIndex = 0;
      if (zero == "home") {
        string home = Environment.GetEnvironmentVariable ("RCL_HOME");
        if (home == null) {
          home = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile) + separator +
                 "dev";
        }
        path += home;
        if (parts.Length > 1) {
          path += separator;
        }
        startIndex = 1;
      }
      else if (zero == "root") {
        path += separator;
        startIndex = 1;
      }
      else if (zero == "work") {
        path += parts.Length == 1 ? "." : "";
        startIndex = 1;
      }
      // Need to handle windows drive letter.
      // There is a function called "GetPathRoot."
      // I think it can be used to obtain the drive letter prefix,
      // but only if you have a path.
      for (int i = startIndex; i < parts.Length; ++i)
      {
        path += parts[i].ToString ();
        if (i < parts.Length - 1) {
          path += separator;
        }
      }
      return path;
    }

    [RCVerb ("readlines")]
    public void EvalReadLines (RCRunner runner, RCClosure closure, RCBlock right)
    {
      StringBuilder result = new StringBuilder ();
      string line;
      while ((line = Console.ReadLine ()) != null)
      {
        result.AppendLine (line);
      }
      runner.Yield (closure, new RCString (result.ToString ()));
    }

    [RCVerb ("file")]
    public void EvalFile (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (File.Exists (PathSymbolToLocalString (right[i])));
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("file")]
    public void EvalFile (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (File.Exists (right[i]));
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("dir")]
    public void EvalDir (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (Directory.Exists (PathSymbolToLocalString (right[i])));
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("dir")]
    public void EvalDir (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (Directory.Exists (right[i]));
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("guid")]
    public void EvalGuid (RCRunner runner, RCClosure closure, RCLong right)
    {
      int guids = (int) right[0];
      RCArray<string> result = new RCArray<string> (guids);
      for (int i = 0; i < guids; ++i)
      {
        Guid guid = Guid.NewGuid ();
        result.Write (guid.ToString ());
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("shortid")]
    public void EvalShortGuid (RCRunner runner, RCClosure closure, RCLong right)
    {
      int guids = (int) right[0];
      RCArray<string> result = new RCArray<string> (guids);
      for (int i = 0; i < guids; ++i)
      {
        string base64Guid = Convert.ToBase64String (Guid.NewGuid ().ToByteArray ());
        base64Guid = base64Guid.Substring (0, 22);
        base64Guid = base64Guid.Replace ('+', '-');
        base64Guid = base64Guid.Replace ('/', '_');
        result.Write (base64Guid);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("ismono")]
    public void IsMono (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCBoolean (RCSystem.IsMono ()));
    }

    [RCVerb ("load")]
    public void EvalLoad (RCRunner runner, RCClosure closure, RCString right)
    {
      string code = File.ReadAllText (right[0], Encoding.UTF8);
      code = code.Replace ("\r", "");
      // I want to change this to split lines.
      runner.Yield (closure, new RCString (code));
    }

    [RCVerb ("load")]
    public void EvalLoad (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      // Need check for windows drive letter
      string path = PathSymbolToLocalString (right[0]);
      string code = File.ReadAllText (path, Encoding.UTF8);
      code = code.Replace ("\r", "");
      runner.Yield (closure, new RCString (code));
    }

    [RCVerb ("rm")]
    public void EvalRm (RCRunner runner, RCClosure closure, RCString right)
    {
      // All of this stuff HASTA HASTA HASTA be ASYNC!
      for (int i = 0; i < right.Count; ++i)
      {
        File.Delete (right[i]);
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("mkdir")]
    public void EvalMkdir (RCRunner runner, RCClosure closure, RCString right)
    {
      // All of this stuff HASTA HASTA HASTA be ASYNC!
      Directory.CreateDirectory (right[0]);
      runner.Yield (closure, right);
    }

    [RCVerb ("rmdir")]
    public void EvalRmDir (RCRunner runner, RCClosure closure, RCString right)
    {
      Directory.Delete (right[0]);
      runner.Yield (closure, right);
    }

    [RCVerb ("mkdir")]
    public void EvalMkdir (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      // All of this stuff HASTA HASTA HASTA be ASYNC!
      string path = PathSymbolToLocalString (right[0]);
      Directory.CreateDirectory (path);
      runner.Yield (closure, right);
    }

    [RCVerb ("rmdir")]
    public void EvalRmdir (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      // All of this stuff HASTA HASTA HASTA be ASYNC!
      string path = PathSymbolToLocalString (right[0]);
      Directory.Delete (path);
      runner.Yield (closure, right);
    }

    protected long _handle = -1;

    [RCVerb ("save")]
    public void EvalSave (RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      Save (runner, closure, PathSymbolToLocalString (left[0]), right.ToArray ());
    }

    [RCVerb ("save")]
    public void EvalSave (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      Save (runner, closure, left[0], right.ToArray ());
    }

    protected void Save (RCRunner runner, RCClosure closure, string path, string[] lines)
    {
      // BRIAN READ THIS WHEN YOU GET BACK HERE.
      // Should not be doing sync io like this.
      // The least I can do is use a thread pool thread.
      // The endgame is for load, save, delete and so on to be replaced with
      // getf, putf, delf and lsf.
      // These operators should be properly async.
      // Also return RCValues just like getm, putm and so on.
      // Also use a symbols system for paths so programs do not contain RC dependent
      // paths.
      // And provide path abstraction which should help with security.
      // getf, putf, delf should always store values in the text files.
      // Then we need some way to store binary files.
      // But I will still need some way to work with raw text files that might not be RC
      // syntax.
      // So I guess I WILL need operators like save and load, huh.
      // And I also guess that those should work on string arrays as lines...
      // Then I could use writeAllLines and readAllLines to get around the terminal line
      // issue.
      // But that would mean changing that parser to interpret string breaks as line
      // breaks.
      // So, not today.
      try
      {
        WriteAllLinesBetter (path, lines);
      }
      catch (UnauthorizedAccessException ex)
      {
        throw new RCException (closure, RCErrors.Access, ex.Message);
      }
      catch (FileNotFoundException ex)
      {
        throw new RCException (closure, RCErrors.File, ex.Message);
      }

      RCSystem.Log.Record (closure,
                           "save",
                           Interlocked.Increment (ref _handle),
                           path,
                           new RCString (lines));
      // ideally this should return a symbol right?
      runner.Yield (closure, new RCString (path));
    }

    // http://stackoverflow.com/questions/11689337/net-file-writealllines-leaves-empty-line-at-the-end-of-file
    public static void WriteAllLinesBetter (string path, params string[] lines)
    {
      if (path == null) {
        throw new ArgumentNullException ("path");
      }
      if (lines == null) {
        throw new ArgumentNullException ("lines");
      }

      using (var stream = File.Open (path, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter writer = new StreamWriter (stream))
        {
          if (lines.Length > 0) {
            for (int i = 0; i < lines.Length - 1; i++)
            {
              writer.WriteLine (lines[i]);
            }
            writer.Write (lines[lines.Length - 1]);
          }
          writer.WriteLine ();
        }
      }
    }

    [RCVerb ("loadbin")]
    public void EvalLoadbin (RCRunner runner, RCClosure closure, RCString right)
    {
      byte[] bytes = File.ReadAllBytes (right[0]);
      // I want to change this to split lines.
      runner.Yield (closure, new RCByte (bytes));
    }

    [RCVerb ("savebin")]
    public void EvalSavebin (RCRunner runner, RCClosure closure, RCString left, RCByte right)
    {
      // BRIAN READ THIS WHEN YOU GET BACK HERE.
      // Should not be doing sync io like this.
      // The least I can do is use a thread pool thread.
      try
      {
        File.WriteAllBytes (left[0], right.ToArray ());
      }
      catch (UnauthorizedAccessException ex)
      {
        throw new RCException (closure, RCErrors.Access, ex.Message);
      }
      catch (FileNotFoundException ex)
      {
        throw new RCException (closure, RCErrors.File, ex.Message);
      }
      RCSystem.Log.Record (closure, "save", Interlocked.Increment (ref _handle), left[0], right);
      runner.Yield (closure, new RCString (left[0]));
    }

    [RCVerb ("delete")]
    public void EvalDelete (RCRunner runner, RCClosure closure, RCString right)
    {
      // It kind of sucks that if one file can not be deleted
      // it could leave the disk in an inconsistent state.
      // Todo: worry about it
      for (int i = 0; i < right.Count; ++i)
      {
        File.Delete (right[i]);
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("delete")]
    public void EvalDelete (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        File.Delete (PathSymbolToLocalString (right[i]));
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("cd")]
    public void EvalCd (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count > 1) {
        throw new Exception ("cd can only change into one directory");
      }
      Environment.CurrentDirectory = right[0];
      runner.Yield (closure, new RCString (Environment.CurrentDirectory));
    }

    [RCVerb ("cd")]
    public void EvalCd (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      if (right.Count > 1) {
        throw new Exception ("cd can only change into one directory");
      }
      Environment.CurrentDirectory = Command.PathSymbolToLocalString (right[0]);
      runner.Yield (closure, right);
    }

    [RCVerb ("pwd")]
    public void EvalPwd (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCString (Environment.CurrentDirectory));
    }

    [RCVerb ("pid")]
    public void EvalPid (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCLong (Process.GetCurrentProcess ().Id));
    }

    [RCVerb ("option")]
    public void EvalOptions (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCValue result = RCSystem.Args.Options.Get (right[0]);
      if (result == null) {
        result = left;
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("flag")]
    public void EvalFlag (RCRunner runner, RCClosure closure, RCBoolean left, RCString right)
    {
      bool result = RCSystem.Args.Options.GetBoolean (right[0], left[0]);
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("flag")]
    public void EvalFlag (RCRunner runner, RCClosure closure, RCString right)
    {
      bool result = RCSystem.Args.Options.GetBoolean (right[0], false);
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("option")]
    public void EvalOptions (RCRunner runner, RCClosure closure, RCString right)
    {
      RCValue result = RCSystem.Args.Options.Get (right[0]);
      if (result == null) {
        throw new Exception ("No such option:" + right[0]);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("info")]
    public void EvalInfo (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      if (right.Count > 1) {
        throw new Exception (
                "info can only provide one value at a time. info #help gives a list of valid values.");
      }
      Info (runner, closure, right[0].Part (0).ToString ());
    }

    [RCVerb ("info")]
    public void EvalInfo (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count > 1) {
        throw new Exception (
                "info can only provide one value at a time. info #help gives a list of valid values.");
      }
      Info (runner, closure, right[0]);
    }

    protected void Info (RCRunner runner, RCClosure closure, string value)
    {
      if (value == "arguments") {
        runner.Yield (closure, RCSystem.Args.Arguments);
      }
      else if (value == "options") {
        runner.Yield (closure, RCSystem.Args.Options);
      }
      else if (value == "directory") {
        runner.Yield (closure, new RCString (Environment.CurrentDirectory));
      }
      else if (value == "drives") {
        runner.Yield (closure, new RCString (Environment.GetLogicalDrives ()));
      }
      else if (value == "host") {
        runner.Yield (closure, new RCString (Environment.MachineName));
      }
      else if (value == "newline") {
        runner.Yield (closure, new RCString (Environment.NewLine));
      }
      else if (value == "osname") {
        OperatingSystem os = Environment.OSVersion;
        runner.Yield (closure, new RCString (os.VersionString));
      }
      else if (value == "platform") {
        PlatformID platform = Environment.OSVersion.Platform;
        string platformName = platform.ToString ();
        runner.Yield (closure, new RCString (platformName));
      }
      else if (value == "help") {
        runner.Yield (closure,
                      new RCString ("arguments",
                                    "options",
                                    "directory",
                                    "drives",
                                    "host",
                                    "newline",
                                    "osname",
                                    "platform",
                                    "help"));
      }
      else {
        throw new Exception (string.Format ("Unsupported argument for info: {0}", value));
      }
    }

    [RCVerb ("codebase")]
    public void EvalCodebase (RCRunner runner, RCClosure closure, RCBlock right)
    {
      Uri codebase = new Uri (Assembly.GetExecutingAssembly ().CodeBase);
      DirectoryInfo dir = new FileInfo (codebase.LocalPath).Directory;
      string unixFullName = dir.FullName.Replace ("\\", "/");
      runner.Yield (closure, new RCString (unixFullName));
    }

    [RCVerb ("getenv")]
    public void EvalGetenv (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        string variable = Environment.GetEnvironmentVariable (right[i]);
        if (variable == null) {
          variable = ConfigurationManager.AppSettings[right[i]];
        }
        if (variable == null) {
          throw new Exception ("No environment variable set: " + right[i]);
        }
        result.Write (variable);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("getenv")]
    public void EvalGetenv (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        string variable = Environment.GetEnvironmentVariable (right[i]);
        if (variable == null) {
          variable = left[i];
        }
        result.Write (variable);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("setenv")]
    public void EvalSetenv (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      for (int i = 0; i < left.Count; ++i)
      {
        Environment.SetEnvironmentVariable (left[i], right[i]);
      }
      runner.Yield (closure, left);
    }

    public class Print
    {
      [RCVerb ("print")]
      public void EvalPrint (RCRunner runner, RCClosure closure, RCString left, object right)
      {
        RCSystem.Log.Record (closure, "print", 0, left[0], right);
        runner.Yield (closure, (RCValue) right);
      }

      [RCVerb ("print")]
      public void EvalPrint (RCRunner runner, RCClosure closure, object right)
      {
        RCSystem.Log.Record (closure, "print", 0, "out", right);
        runner.Yield (closure, (RCValue) right);
      }

      [RCVerb ("print")]
      public void EvalPrint (RCRunner runner, RCClosure closure, RCString right)
      {
        RCSystem.Log.Record (closure, "print", 0, "out", right);
        runner.Yield (closure, right);
      }

      [RCVerb ("print")]
      public void EvalPrint (RCRunner runner, RCClosure closure, RCString left, RCString right)
      {
        RCSystem.Log.Record (closure, "print", 0, left[0], right);
        runner.Yield (closure, right);
      }
    }

    /// <summary>
    /// This is deprecated. DO NOT USE!
    /// </summary>
    [RCVerb ("module")]
    public void EvalModule (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCArray<RCReference> references = new RCArray<RCReference> ();
      RCBlock result = (RCBlock) right.Edit (runner,
                                             delegate (RCValue val)
      {
        RCReference reference = val as RCReference;
        if (reference != null) {
          RCReference r = new RCReference (reference.Name);
          references.Write (r);
          return r;
        }
        RCUserOperator op = val as RCUserOperator;
        if (op != null) {
          RCReference r = new RCReference (op.Name);
          references.Write (r);
          RCUserOperator outop = new RCUserOperator (r);
          outop.Init (op.Name, op.Left, op.Right);
          return outop;
        }
        else {
          return null;
        }
      });
      // Ugh there must be some better way than this.
      // But sometimes you just need to modify a value after allocation.
      // We can use the Lock() mechanism to tighten this up at least.
      // SetStatic will throw an exception if you call it after Lock().
      for (int i = 0; i < references.Count; ++i)
      {
        references[i].SetStatic (result);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("exit")]
    public void EvalExit (RCRunner runner, RCClosure closure, RCLong right)
    {
      RCSystem.Log.Record (closure, "runner", 0, "exit", right);
      runner.Abort ((int) right[0]);
    }

    /// <summary>
    /// colout modifies the format used when printing cubes to customize
    /// the appearance of columns, especially number columns.
    /// It is no fun crunching numbers at the command line if they don't
    /// look right.
    /// </summary>
    [RCVerb ("displayFormat")]
    public void DisplayFormat (RCRunner runner, RCClosure closure, RCCube right)
    {
      RCArray<string> column = right.DoColof<string> ("column", "");
      RCArray<string> format = right.DoColof<string> ("format", "");
      RCSystem.Log.UpdateColmap (column, format);
      // RCSystem.Log.Record (runner, closure, "display", 0, "format", "set to " +
      // right[0]);
      runner.Yield (closure, right);
    }

    [RCVerb ("displayFormat")]
    public void DisplayFormat (RCRunner runner, RCClosure closure, RCBlock right)
    {
      // RCArray<string> column = right.DoColof<string> ("column", "");
      // RCArray<string> format = right.DoColof<string> ("format", "");
      throw new NotImplementedException ("This should yield a cube describing the defined formats");
    }

    [RCVerb ("displayTimezone")]
    public void DisplayTimezone (RCRunner runner, RCClosure closure, RCString right)
    {
      RCTime.DisplayTimeZone = TimeZoneInfo.FindSystemTimeZoneById (right[0]);
      RCSystem.Log.Record (closure, "display", 0, "timezone", "set to " + right[0]);
      runner.Yield (closure, right);
      // TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById (displayTimezone);
      // displayTime = TimeZoneInfo.ConvertTimeFromUtc (new DateTime (scalar.Ticks),
      // DisplayTimeZone);
    }

    [RCVerb ("displayTimezone")]
    public void DisplayTimezone (RCRunner runner, RCClosure closure, RCBlock right)
    {
      string id = RCTime.DisplayTimeZone.Id;
      runner.Yield (closure, new RCString (id));
    }

    [RCVerb ("excount")]
    public void ExceptionCount (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCLong (runner.ExceptionCount));
    }

    [RCVerb ("reset")]
    public void Reset (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.ResetCount (closure.Bot);
      runner.Yield (closure, new RCLong (0));
    }

    [RCVerb ("timezones")]
    public void Timezones (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCCube result = new RCCube ("S");
      DateTime now = DateTime.UtcNow;
      ReadOnlyCollection<TimeZoneInfo> timezones = TimeZoneInfo.GetSystemTimeZones ();
      foreach (TimeZoneInfo timezone in timezones)
      {
        RCSymbolScalar sym = RCSymbolScalar.From ("timezones", timezone.Id);
        result.WriteCell ("displayName", sym, timezone.DisplayName);
        result.WriteCell ("standardName", sym, timezone.StandardName);
        result.WriteCell ("daylightName", sym, timezone.DaylightName);
        result.WriteCell ("utcOffset", sym, new RCTimeScalar (timezone.GetUtcOffset (now)));
        result.WriteCell ("isDaylightSavingTime", sym, timezone.IsDaylightSavingTime (now));
        result.WriteCell ("supportsDaylightSavingTime", sym, timezone.SupportsDaylightSavingTime);
        result.Axis.Write (sym);
      }
      runner.Yield (closure, result);
    }

    private static RCBlock _options = null;
    public static void SetOptions (RCBlock options)
    {
      if (_options != null) {
        throw new Exception ("Set options called more than once.");
      }
      _options = options;
    }

    public void EvalOption (RCRunner runner,
                            RCClosure closure,
                            RCString right)
    {
      runner.Yield (closure, _options.Get (right[0]));
    }
  }
}

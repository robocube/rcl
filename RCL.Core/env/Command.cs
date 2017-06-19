
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
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
        result.Write (PathSymbolToString (right[i]));
      }
      runner.Yield (closure, new RCString (result));
    }

    public static string PathSymbolToString (RCSymbolScalar symbol)
    {
      object[] parts = symbol.ToArray ();
      string path = "";
      string zero = parts[0].ToString ();
      int startIndex = 0;
      if (zero == "home")
      {
        string home = Environment.GetEnvironmentVariable ("RCL_HOME");
        if (home == null) 
        {
          home = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
        }
        path += home;
        if (parts.Length > 1)
        {
          path += Path.DirectorySeparatorChar;
        }
        startIndex = 1;
      }
      else if (zero == "root")
      {         
        path += Path.DirectorySeparatorChar;
        startIndex = 1;
      }
      else if (zero == "work")
      {
        path += parts.Length == 1 ? "." : "";
        startIndex = 1;
      }
      //Need to handle windows drive letter.
      //There is a function called "GetPathRoot."
      //I think it can be used to obtain the drive letter prefix,
      //but only if you have a path.
      for (int i = startIndex; i < parts.Length; ++i)
      {
        path += parts[i].ToString ();
        if (i < parts.Length - 1)
        {
          path += Path.DirectorySeparatorChar;
        }
      }
      return path;
    }

    [RCVerb ("file")]
    public void EvalFile (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (File.Exists (PathSymbolToString (right[i])));
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

    [RCVerb ("load")]
    public void EvalLoad (
      RCRunner runner, RCClosure closure, RCString right)
    {
      string code = File.ReadAllText (right[0], Encoding.UTF8);
      //I want to change this to split lines.
      runner.Yield (closure, new RCString (code));
    }

    [RCVerb ("load")]
    public void EvalLoad (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      //Need check for windows drive letter
      string path = PathSymbolToString (right[0]);
      string code = File.ReadAllText (path, Encoding.UTF8);
      runner.Yield (closure, new RCString (code));
    }

    protected long m_handle = -1;
    [RCVerb ("save")]
    public void EvalSave (RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      Save (runner, closure, PathSymbolToString (left[0]), right.ToArray ());
    }
      
    [RCVerb ("save")]
    public void EvalSave (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      Save (runner, closure, left[0], right.ToArray ());
    }

    protected void Save (RCRunner runner, RCClosure closure, string path, string[] lines)
    {
      //BRIAN READ THIS WHEN YOU GET BACK HERE.
      //Should not be doing sync io like this.
      //The least I can do is use a thread pool thread.
      //The endgame is for load, save, delete and so on to be replaced with
      //getf, putf, delf and lsf.
      //These operators should be properly async.
      //Also return RCValues just like getm, putm and so on.
      //Also use a symbols system for paths so programs do not contain RC dependent paths.
      //And provide path abstraction which should help with security.
      //getf, putf, delf should always store values in the text files.
      //Then we need some way to store binary files.
      //But I will still need some way to work with raw text files that might not be RC syntax.
      //So I guess I WILL need operators like save and load, huh.
      //And I also guess that those should work on string arrays as lines...
      //Then I could use writeAllLines and readAllLines to get around the terminal line issue.
      //But that would mean changing that parser to interpret string breaks as line breaks.
      //So, not today.
      WriteAllLinesBetter (path, lines);

      runner.Log.Record (runner, closure,
                         "save", Interlocked.Increment (ref m_handle), path, lines);
      //ideally this should return a symbol right?
      runner.Yield (closure, new RCString (path));
    }

    //http://stackoverflow.com/questions/11689337/net-file-writealllines-leaves-empty-line-at-the-end-of-file
    public static void WriteAllLinesBetter(string path, params string[] lines)
    {
      if (path == null)
      {
        throw new ArgumentNullException ("path");
      }
      if (lines == null)
      {
        throw new ArgumentNullException ("lines");
      }

      //using (var stream = File.OpenWrite (path))
      using (var stream = File.Open (path, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter (stream))
      {
        if (lines.Length > 0)
        {
          for (int i = 0; i < lines.Length - 1; i++)
          {
            writer.WriteLine (lines[i]);
          }
          writer.Write (lines[lines.Length - 1]);
        }
      }
    }

    [RCVerb ("loadbin")]
    public void EvalLoadbin (
      RCRunner runner, RCClosure closure, RCString right)
    {
      byte[] bytes = File.ReadAllBytes (right[0]);
      //I want to change this to split lines.
      runner.Yield (closure, new RCByte (bytes));
    }

    [RCVerb ("savebin")]
    public void EvalSavebin (RCRunner runner, RCClosure closure, RCString left, RCByte right)
    {
      //BRIAN READ THIS WHEN YOU GET BACK HERE.
      //Should not be doing sync io like this.
      //The least I can do is use a thread pool thread.
      File.WriteAllBytes (left[0], right.ToArray ());
      runner.Log.Record (runner, closure,
                         "save", Interlocked.Increment (ref m_handle), left[0], right);
      runner.Yield (closure, new RCString (left[0]));
    }

    [RCVerb ("delete")]
    public void EvalDelete (
      RCRunner runner, RCClosure closure, RCString right)
    {
      //It kind of sucks that if one file can not be deleted
      //it could leave the disk in an inconsistent state.
      //Todo: worry about it
      for (int i = 0; i < right.Count; ++i)
      {
        File.Delete (right[i]);
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("delete")]
    public void EvalDelete (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        File.Delete (PathSymbolToString (right[i]));
      }
      runner.Yield (closure, right);
    }

    [RCVerb ("cd")]
    public void EvalCd (
      RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count > 1)
        throw new Exception ("cd can only change into one directory");

      Environment.CurrentDirectory = right[0];
      runner.Yield (closure, new RCString (Environment.CurrentDirectory));
    }

    [RCVerb ("pwd")]
    public void EvalPwd (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCString (Environment.CurrentDirectory));
    }

    [RCVerb ("pid")]
    public void EvalPid (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCLong (Process.GetCurrentProcess ().Id));
    }

    //Let's call this getarg to be more like getenv
    [RCVerb ("option")]
    public void EvalOptions (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCValue result = runner.Argv.Options.Get (right[0]);
      if (result == null)
      {
        result = left;
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("option")]
    public void EvalOptions (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCValue result = runner.Argv.Options.Get (right[0]);
      if (result == null)
      {
        throw new Exception ("No such option:" + right[0]);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("info")]
    public void EvalInfo (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("info can only provide one value at a time. info #help gives a list of valid values.");
      }
      Info (runner, closure, right[0].Part(0).ToString ());
    }

    [RCVerb ("info")]
    public void EvalInfo (
      RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count > 1)
      {
        throw new Exception ("info can only provide one value at a time. info #help gives a list of valid values.");
      }
      Info (runner, closure, right[0]);
    }

    protected void Info (RCRunner runner, RCClosure closure, string value)
    {
      if (value == "arguments")
      {
        runner.Yield (closure, runner.Argv.Arguments);
      }
      else if (value == "options")
      {
        runner.Yield (closure, runner.Argv.Options);
      }
      else if (value == "directory")
      {
        runner.Yield (closure, new RCString (Environment.CurrentDirectory));
      }
      else if (value == "drives")
      {
        runner.Yield (closure, new RCString (Environment.GetLogicalDrives ()));
      }
      else if (value == "host")
      {
        runner.Yield (closure, new RCString (Environment.MachineName));
      }
      else if (value == "ending")
      {
        runner.Yield (closure, new RCString (Environment.NewLine));
      }
      else if (value == "os")
      {
        runner.Yield (closure, new RCString (Environment.OSVersion.VersionString));
      }
      else if (value == "help")
      {
        runner.Yield (closure, new RCString ("arguments", "options", "directory", "drives", "host", "end", "os", "help"));
      }
    }

    [RCVerb ("getenv")]
    public void EvalGetenv (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        string variable = Environment.GetEnvironmentVariable (right[i]);
        if (variable == null)
        {
          throw new Exception ("No environment variable set: " + right[i]);
        }
        result.Write (variable);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("getenv")]
    public void EvalGetenv (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        string variable = Environment.GetEnvironmentVariable (right[i]);
        if (variable == null)
        {
          variable = left[i];
        }
        result.Write (variable);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("setenv")]
    public void EvalSetenv (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
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
      public void EvalPrint (
        RCRunner runner, RCClosure closure, RCString right)
      {
        runner.Log.Record (runner, closure, "print", 0, "out", right);
        runner.Yield (closure, RCLong.Zero);
      }

      [RCVerb ("print")]
      public void EvalPrint (
        RCRunner runner, RCClosure closure, RCString left, RCString right)
      {
        runner.Log.Record (runner, closure, "print", 0, left[0], right);
        runner.Yield (closure, RCLong.Zero);
      }
    }

    [RCVerb ("module")]
    public void EvalModule (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCArray<RCReference> references = new RCArray<RCReference> ();
      RCBlock result = (RCBlock) right.Edit (runner, delegate (RCValue val)
      {
        RCReference reference = val as RCReference;
        if (reference != null)
        {
          RCReference r = new RCReference (reference.Name);
          references.Write (r);
          return r;
        }
        UserOperator op = val as UserOperator;
        if (op != null)
        {
          RCReference r = new RCReference (op.Name);
          references.Write (r);
          UserOperator outop = new UserOperator (r);
          outop.Init (op.Name, op.Left, op.Right);
          return outop;
        }
        else return null;
      });

      //Ugh there must be some better way than this.
      //But sometimes you just need to modify a value after allocation.
      //We can use the Lock() mechanism to tighten this up at least.
      //SetStatic will throw an exception if you call it after Lock().
      for (int i = 0; i < references.Count; ++i)
      {
        references[i].SetStatic (result);
      }

      runner.Yield (closure, result);
    }

    [RCVerb ("exit")]
    public void EvalExit (RCRunner runner, 
                          RCClosure closure, 
                          RCLong right)
    {
      runner.Log.Record (runner, closure, "runner", 0, "exit", right);
      runner.Abort ((int) right[0]);
    }

    private static RCBlock m_options = null;
    public static void SetOptions (RCBlock options)
    {
      if (m_options != null)
      {
        throw new Exception ("Set options called more than once.");
      }
      m_options = options;
    }

    public void EvalOption (RCRunner runner, 
                            RCClosure closure, 
                            RCString right)
    {
      runner.Yield (closure, m_options.Get (right[0]));
    }
  }
}

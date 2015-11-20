
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using RCL.Kernel;

namespace RCL.Core
{
  public class Command
  {
    [RCVerb ("load")]
    public void EvalLoad (
      RCRunner runner, RCClosure closure, RCString right)
    {
      string code = File.ReadAllText (right[0], Encoding.UTF8);
      //I want to change this to split lines.
      runner.Yield (closure, new RCString (code));
    }

    protected long m_handle = -1;
    [RCVerb ("save")]
    public void EvalSave (RCRunner runner, RCClosure closure, RCString left, RCString right)
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
      WriteAllLinesBetter (left[0], right.ToArray ());

      runner.Log.RecordDoc (runner, closure,
                            "save", Interlocked.Increment (ref m_handle), left[0], right);
      runner.Yield (closure, new RCString (left[0]));
    }

    //http://stackoverflow.com/questions/11689337/net-file-writealllines-leaves-empty-line-at-the-end-of-file
    public static void WriteAllLinesBetter(string path, params string[] lines)
    {
      if (path == null)
        throw new ArgumentNullException ("path");
      if (lines == null)
        throw new ArgumentNullException ("lines");

      //using (var stream = File.OpenWrite (path))
      using (var stream = File.Open (path, FileMode.Create))
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

    /*
    [RCVerb ("list")]
    public void EvalList (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> ();
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (Directory.GetFiles (right[i]));
        result.Write (Directory.GetDirectories (right[i]));
      }
      runner.Yield (closure, new RCString (result));
    }
    */

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

    /*
    [RCVerb ("output")]
    public void EvalArgument (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCOutput level = (RCOutput) Enum.Parse (typeof (RCOutput), right[0], true);
      runner.Log.Output (level);
      runner.Yield (closure, right);
    }
    */

    [RCVerb ("argument")]
    public void EvalArgument (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCValue result = runner.Arguments.Get (right[0]);
      if (result == null)
      {
        throw new Exception ("No argument given:" + right[0]);
      }
      runner.Yield (closure, result);
    }

    //Let's call this getarg to be more like getenv
    [RCVerb ("argument")]
    public void EvalArgument (
      RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCValue result = runner.Arguments.Get (right[0]);
      if (result == null)
      {
        result = left;
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("getenv")]
    public void EvalGetenv (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (Environment.GetEnvironmentVariable (right[i]));
      }
      runner.Yield (closure, new RCString (result));
    }

    public class Print
    {
      [RCVerb ("print")]
      public void EvalPrint (
        RCRunner runner, RCClosure closure, RCString right)
      {
        runner.Log.RecordDoc (runner, closure, "print", 0, "out", right);
        runner.Yield (closure, RCLong.Zero);
      }

      [RCVerb ("print")]
      public void EvalPrint (
        RCRunner runner, RCClosure closure, RCString left, RCString right)
      {
        runner.Log.RecordDoc (runner, closure, "print", 0, left[0], right);
        runner.Yield (closure, RCLong.Zero);
      }
    }

    [RCVerb ("module")]
    public void EvalModule (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCArray<RCLReference> references = new RCArray<RCLReference> ();
      RCBlock result = (RCBlock) right.Edit (runner, delegate (RCValue val)
      {
        RCLReference reference = val as RCLReference;
        if (reference != null)
        {
          RCLReference r = new RCLReference (reference.Name);
          references.Write (r);
          return r;
        }
        UserOperator op = val as UserOperator;
        if (op != null)
        {
          RCLReference r = new RCLReference (op.Name);
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
    public void EvalExit (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Log.RecordDoc (runner, closure, "runner", 0, "exit", right);
      runner.Exit ((int) right[0]);
    }

    /*
    Making this an operator did not work cause fucked up stuff happens when you yield
    after calling reset on the runner.
    [RCVerb ("reset")]
    public void EvalReset (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Log.RecordDoc (runner, closure,
                            closure.Bot.Id, "runner", 0, "reset", right);
      runner.Reset ();
      //runner.Yield (closure, right);
    }
    */
  }
}
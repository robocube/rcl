
using System;
using DiffMatchPatch;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Text
  {
    /*
    [RCVerb ("diff")]
    public void EvalDiff (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left[0], right[0]);
      List<Patch> patch = dmp.patch_make (diffs);
      string text = dmp.patch_toText (patch);
      runner.Yield (closure, new RCString (text));
    }
    */

    [RCVerb ("diff")]
    public void EvalDiff (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left[0], right[0], true);
      List<Patch> patch = dmp.patch_make (diffs);
      RCCube result = new RCCube (new RCArray<string> ());
      for (int i = 0; i < patch.Count; ++i)
      {
        for (int j = 0; j < patch[i].diffs.Count; ++j)
        {
          string operation = patch[i].diffs[j].operation.ToString ();
          string text = patch[i].diffs[j].text;
          result.WriteCell ("op", null, operation);
          result.WriteCell ("text", null, text);
          result.Axis.Write (null);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("compare")]
    public void EvalCompare (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, Compare (left[0], right[0], true));
    }

    protected RCCube Compare (string left, string right, bool breakLines)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left, right, true);
      List<Patch> patch = dmp.patch_make (diffs);
      RCCube result = new RCCube (new RCArray<string> ());
      int start = 0;
      int end = 0;
      for (int i = 0; i < patch.Count; ++i)
      {
        for (int j = 0; j < patch[i].diffs.Count; ++j)
        {
          Operation op = patch[i].diffs[j].operation;
          string text = patch[i].diffs[j].text;
          if (op == Operation.EQUAL)
          {
            end = left.IndexOf (text, start) + text.Length;
            if (end < 0)
            {
              throw new Exception (
                "cannot find string \"" + text + "\"");
            }
            string section = left.Substring (start, (end - start));
            start = end; // + text.Length;
            string[] lines = section.Split ('\n');
            if (breakLines && lines.Length > 1)
            {
              for (int k = 0; k < lines.Length; ++k)
              {
                if (!(k == lines.Length - 1 && lines[k] == ""))
                {
                  result.WriteCell ("op", null, op.ToString ());
                  result.WriteCell ("old", null, lines[k] + "\n");
                  result.WriteCell ("new", null, lines[k] + "\n");
                  result.Axis.Write (null);
                }
              }
            }
            else
            {
              result.WriteCell ("op", null, op.ToString ());
              result.WriteCell ("old", null, section); 
              result.WriteCell ("new", null, section); 
              result.Axis.Write (null);
            }
          }
          else if (op == Operation.INSERT)
          {
            if (breakLines)
            {
              string[] lines = text.Split ('\n');
              if (lines.Length > 1)
              {
                for (int k = 0; k < lines.Length; ++k)
                {
                  if (!(k == lines.Length - 1 && lines[k] == ""))
                  {
                    result.WriteCell ("op", null, op.ToString ());
                    result.WriteCell ("new", null, lines[k] + "\n");
                    result.Axis.Write (null);
                  }
                }
              }
              else
              {
                result.WriteCell ("op", null, op.ToString ());
                result.WriteCell ("new", null, text);
                result.Axis.Write (null);
              }
            }
            else
            {
              result.WriteCell ("op", null, op.ToString ());
              result.WriteCell ("new", null, text);
              result.Axis.Write (null);
            }
          }
          else //DELETE
          {
            end += patch[i].diffs[j].text.Length;
            start = end;
            if (breakLines)
            {
              string[] lines = text.Split ('\n');
              if (lines.Length > 1)
              {
                for (int k = 0; k < lines.Length; ++k)
                {
                  if (!(k == lines.Length - 1 && lines[k] == ""))
                  {
                    result.WriteCell ("op", null, op.ToString ());
                    result.WriteCell ("old", null, lines[k] + "\n");
                    result.Axis.Write (null);
                  }
                }
              }
              else
              {
                result.WriteCell ("op", null, op.ToString ());
                result.WriteCell ("old", null, text);
                result.Axis.Write (null);
              }
            }
            else
            {
              result.WriteCell ("op", null, op.ToString ());
              result.WriteCell ("old", null, text);
              result.Axis.Write (null);
            }
          }
        }
      }
      return result;
    }

    [RCVerb ("diff1")]
    public void EvalDiff1 (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left[0], right[0]);
      string result = dmp.diff_text1 (diffs);
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("diff2")]
    public void EvalDiff2 (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left[0], right[0]);
      List<Patch> patch = dmp.patch_make (diffs);
      string text = dmp.patch_toText (patch);
      runner.Yield (closure, new RCString (text));
    }

    [RCVerb ("diffhtml")]
    public void EvalDiffHtml (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left[0], right[0]);
      string result = dmp.diff_prettyHtml (diffs);
      runner.Yield (closure, new RCString (result));
    }

  }
}

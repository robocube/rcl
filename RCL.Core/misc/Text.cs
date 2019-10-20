
using System;
using DiffMatchPatch;
using System.Collections.Generic;
using RCL.Kernel;
using System.Text;
using System.Text.RegularExpressions;

namespace RCL.Core
{
  public class Text
  {
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
      runner.Yield (closure, CompareNew (left[0], right[0], true));
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
        //int hunkStart = start;
        for (int j = 0; j < patch[i].diffs.Count; ++j)
        {
          Operation op = patch[i].diffs[j].operation;
          string text = patch[i].diffs[j].text;
          if (op == Operation.EQUAL)
          {
            end = left.IndexOf (text, start);
            if (end < 0)
            {
              throw new Exception (
                "cannot find string \"" + text + "\" after position " + start + " in \"" + left + "\"");
            }
            end += text.Length;
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
            //start = Math.Max (0, start - text.Length);
            //end = start;
            //end = start;
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

    protected RCCube CompareNew (string left, string right, bool breakLines)
    {
      diff_match_patch dmp = new diff_match_patch ();
      List<Diff> diffs = dmp.diff_main (left, right, true);
      RCCube result = new RCCube (new RCArray<string> ());
      int start = 0;
      int end = 0;
      for (int i = 0; i < diffs.Count; ++i)
      {
        Operation op = diffs[i].operation;
        string text = diffs[i].text;
        if (op == Operation.EQUAL)
        {
          int match = left.IndexOf (text, start);
          if (match < 0)
          {      
            throw new Exception (
              "cannot find string \"" + text + "\" after position " + start + " in \"" + left + "\"");
          }
          end = match + text.Length;
          string section = left.Substring (start, end - start);
          start = end;

          if (breakLines)
          {
            int lineStart = 0;
            while (true)
            {
              int lineEnd = section.IndexOf ('\n', lineStart);
              if (lineEnd >= 0)
              {
                result.WriteCell ("op", null, op.ToString ());
                string line = section.Substring (lineStart, 1 + (lineEnd - lineStart));
                result.WriteCell ("old", null, line);
                result.WriteCell ("new", null, line);
                result.Axis.Write (null);
                lineStart = lineEnd + 1;
              }
              else if (lineStart < section.Length)
              {
                result.WriteCell ("op", null, op.ToString ());
                string rest = section.Substring (lineStart, section.Length - lineStart);
                result.WriteCell ("old", null, rest);
                result.WriteCell ("new", null, rest);
                result.Axis.Write (null);
                break;
              }
              else break;
            }
          }
          else
          {
            result.WriteCell ("op", null, op.ToString ());
            result.WriteCell ("old", null, section); 
            result.WriteCell ("new", null, section); 
            result.Axis.Write (null);
          }

          //string[] lines = section.Split ('\n');
          //if (lines.Length > 1 && section.Length == 1) continue;
          /*
          if (breakLines && lines.Length > 1)
          {
            for (int k = 0; k < lines.Length; ++k)
            {
              if (k < lines.Length - 1)
              {
                result.WriteCell ("op", null, op.ToString ());
                result.WriteCell ("old", null, lines[k] + "\n");
                result.WriteCell ("new", null, lines[k] + "\n");
                result.Axis.Write (null);
              }
              else if (lines[k] != "")
              {
                result.WriteCell ("op", null, op.ToString ());
                result.WriteCell ("old", null, lines[k]);
                result.WriteCell ("new", null, lines[k]);
                result.Axis.Write (null);
              }
              else
              {
                result.WriteCell ("op", null, op.ToString ());
                result.WriteCell ("old", null, "\n");
                result.WriteCell ("new", null, "\n");
                result.Axis.Write (null);
              }
              if (lines[k] == "")
              {
                result.WriteCell ("op", null, op.ToString ());
                result.WriteCell ("old", null, "\n");
                result.WriteCell ("new", null, "\n");
                result.Axis.Write (null);
              }
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
          */
        }
        else if (op == Operation.INSERT)
        {
          string section = text;
          string[] lines = section.Split ('\n');
          if (breakLines && lines.Length > 1)
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
            result.WriteCell ("new", null, section); 
            result.Axis.Write (null);
          }
        }
        else if (op == Operation.DELETE)
        {
          start += text.Length;
          string section = text;
          string[] lines = section.Split ('\n');
          if (breakLines && lines.Length > 1)
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
            result.WriteCell ("old", null, section); 
            result.Axis.Write (null);
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

    [RCVerb ("utf8")]
    public void EvalUtf8 (RCRunner runner, RCClosure closure, RCLong right)
    {
      byte[] bytes = new byte[right.Count];
      for (int i = 0; i < bytes.Length; ++i)
      {
        bytes[i] = (byte) right[i];
      }
      string result = Encoding.UTF8.GetString (bytes);
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("utf8")]
    public void EvalUtf8 (RCRunner runner, RCClosure closure, RCByte right)
    {
      string result = right.Utf8String ();
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("utf8")]
    public void EvalUtf8 (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCByte (Encoding.UTF8.GetBytes (right[0])));
    }

    [RCVerb ("ascii")]
    public void EvalAscii (RCRunner runner, RCClosure closure, RCLong right)
    {
      byte[] bytes = new byte[right.Count];
      for (int i = 0; i < bytes.Length; ++i)
      {
        bytes[i] = (byte) right[i];
      }
      string result = Encoding.ASCII.GetString (bytes);
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("ascii")]
    public void EvalAscii (RCRunner runner, RCClosure closure, RCByte right)
    {
      string result = Encoding.ASCII.GetString (right.ToArray ());
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("ascii")]
    public void EvalAscii (RCRunner runner, RCClosure closure, RCString right)
    {
      byte[] result = Encoding.ASCII.GetBytes (right[0]);
      runner.Yield (closure, new RCByte (result));
    }
  }
}

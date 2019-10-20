using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using RCL.Kernel;

namespace RCL.Core
{
  public class String
  {
    [RCVerb ("substring")]
    public void EvalSubstring (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      RCArray<string> result = new RCArray<string> ();
      if (left.Count == 1)
      {
        int start = (int) left[0];
        for (int i = 0; i < right.Count; ++i)
        {
          result.Write (right[i].Substring (start));
        }
      }
      else if (left.Count == 2)
      {
        int start = (int) left[0];
        int length = (int) left[1];
        for (int i = 0; i < right.Count; ++i)
        {
          result.Write (right[i].Substring (start, length));
        }
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("cut")]
    public void EvalCut (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      RCArray<string> result = new RCArray<string> ();
      if (left.Count == 1)
      {
        int point = (int) left[0];
        if (point >= 0)
        {
          for (int i = 0; i < right.Count; ++i)
          {
            result.Write (right[i].Substring (point));
          }
        }
        else
        {
          point = Math.Abs (point);
          for (int i = 0; i < right.Count; ++i)
          {
            result.Write (right[i].Substring (right[i].Length - point));
          }
        }
      }
      else if (left.Count == right.Count)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          if (left[i] >= 0)
          {
            result.Write (right[i].Substring ((int) left[i]));
          }
          else
          {
            int point = Math.Abs ((int) left[i]);
            result.Write (right[i].Substring (right[i].Length - point));
          }
        }
      }
      else
      {
        throw new Exception ("left argument must have count 1 or count equal to right.");
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("cutleft")]
    public void EvalCutleft (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      RCArray<string> result = new RCArray<string> ();
      if (left.Count == 1)
      {
        int point = (int) left[0];
        if (point >= 0)
        {
          for (int i = 0; i < right.Count; ++i)
          {
            result.Write (right[i].Substring (0, point));
          }
        }
        else
        {
          point = Math.Abs (point);
          for (int i = 0; i < right.Count; ++i)
          {
            result.Write (right[i].Substring (0, right[i].Length - point));
          }
        }
      }
      else if (left.Count == right.Count)
      {
        for (int i = 0; i < right.Count; ++i)
        {
          if (left[i] >= 0)
          {
            result.Write (right[i].Substring (0, (int) left[i]));
          }
          else
          {
            int point = Math.Abs ((int) left[i]);
            result.Write (right[i].Substring (0, right[i].Length - point));
          }
        }
      }
      else
      {
        throw new Exception ("left argument must have count 1 or count equal to right.");
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("split")]
    public void EvalSplit (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      //Would be nice to support muliple strings on the left, like "\" "/" split $blah
      RCArray<string> result = new RCArray<string> ();
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].Split (left[0].ToCharArray ()));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("tuple")]
    public void EvalTuple (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCArray<RCSymbolScalar> result = new RCArray<RCSymbolScalar> ();
      for (int i = 0; i < right.Count; ++i)
      {
        string[] parts = right[i].Split (left[0].ToCharArray ());
        RCSymbolScalar sym = RCSymbolScalar.From (parts);
        result.Write (sym);
      }
      runner.Yield (closure, new RCSymbol (result));
    }

    [RCVerb ("splitw")]
    public  void EvalSplitw (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCArray<string> result = new RCArray<string> ();
      for (int i = 0; i < right.Count; ++i)
      {
        int start = 0;
        int end;
        while (true)
        {
          end = right[i].IndexOf (left[0], start);
          if (end >= 0)
          {
            result.Write (right[i].Substring (start, end - start));
            start = end + left[0].Length;
          }
          else
          {
            result.Write (right[i].Substring (start));
            break;
          }
        }
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("pad")]
    public void EvalPad (RCRunner runner, RCClosure closure, RCString left, RCLong right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        string padding = "".PadRight ((int) right [i], left [0][0]);
        result.Write (padding);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("indexof")]
    public void EvalIndexOf (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("indexof can only take one string on the left.");
      }
      RCArray<long> result = new RCArray<long> ();
      string value = left [0];
      int totalIndex = 0;
      for (int i = 0; i < right.Count; ++i)
      {
        int startIndex = 0;
        while (true)
        {
          startIndex = right [i].IndexOf (value, 
                                          startIndex, 
                                          right [i].Length - startIndex, 
                                          StringComparison.InvariantCulture);
          if (startIndex > -1)
          {
            result.Write (totalIndex + startIndex);
            startIndex += value.Length;
          }
          else
          {
            break;
          }
        }
        totalIndex += right [i].Length;
      }
      runner.Yield (closure, new RCLong (result));
    }

    [RCVerb ("indexoflast")]
    public void EvalIndexOfLast (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("indexoflast can only take one string on the left.");
      }
      RCArray<long> result = new RCArray<long> ();
      string value = left [0];
      int totalIndex = 0;
      for (int i = 0; i < right.Count; ++i)
      {
        int startIndex = 0;
        while (true)
        {
          startIndex = right [i].LastIndexOf (value, 
                                              startIndex, 
                                              right [i].Length - startIndex, 
                                              StringComparison.InvariantCulture);
          if (startIndex > -1)
          {
            result.Write (totalIndex + startIndex);
            startIndex += value.Length;
          }
          else
          {
            break;
          }
        }
        totalIndex += right [i].Length;
      }
      runner.Yield (closure, new RCLong (result));
    }

    [RCVerb ("slice")]
    public void EvalSlice (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left argument must contain exactly one delimeter");
      }
      if (left[0].Length != 1)
      {
        throw new Exception ("delimeter must be a single character");
      }

      char delimeter = left[0][0];
      List<List<string>> columns = new List<List<string>>();
      for (int i = 0; i < right.Count; ++i)
      {
        string[] splitted = right[i].Split (delimeter);
        while (columns.Count < splitted.Length)
        {
          List<string> column = new List<string>();
          for (int k = 0; k < i; ++k)
          {
            column.Add ("");
          }
          columns.Add (column);
        }
        for (int j = 0; j < columns.Count; ++j)
        {
          if (j < splitted.Length)
          {
            columns[j].Add (splitted[j]);
          }
          else
          {
            columns[j].Add ("");
          }
        }
      }
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < columns.Count; ++i)
      {
        result = new RCBlock (result,
                              "", ":", new RCString (new RCArray<string> (columns[i])));
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("trim")]
    public void EvalTrim (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].Trim ());
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("trim")]
    public void EvalTrim (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left should contain a single string containing the chars to trim.");
      }
      char[] chars = left[0].ToCharArray ();
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].Trim (chars));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("trimStart")]
    public void EvalTrimStart (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].TrimStart ());
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("trimStart")]
    public void EvalTrimStart (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left should contain a single string containing the chars to trim.");
      }
      char[] chars = left[0].ToCharArray ();
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].TrimStart (chars));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("trimEnd")]
    public void EvalTrimEnd (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].TrimEnd ());
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("trimEnd")]
    public void EvalTrimEnd (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1)
      {
        throw new Exception ("left should contain a single string containing the chars to trim.");
      }
      char[] chars = left[0].ToCharArray ();
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (right[i].TrimEnd (chars));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("delimit")]
    public void EvalDelimit (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      StringBuilder result = new StringBuilder ();
      for (int i = 0; i < right.Count; ++i)
      {
        result.Append (right[i]);
        if (i < right.Count - 1)
        {
          result.Append (left [0]);
        }
      }
      runner.Yield (closure, new RCString (result.ToString ()));
    }

    [RCVerb ("isname")]
    public void EvalIsName (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<bool> result = new RCArray<bool> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (RCTokenType.LengthOfStrictIdentifier (right[i], 0) == right[i].Length);
      }
      runner.Yield (closure, new RCBoolean (result));
    }
  }
}

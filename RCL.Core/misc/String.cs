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
      if (left.Count == 1) {
        int start = (int) left[0];
        for (int i = 0; i < right.Count; ++i)
        {
          result.Write (right[i].Substring (start));
        }
      }
      else if (left.Count == 2) {
        int start = (int) left[0];
        int length = (int) left[1];
        for (int i = 0; i < right.Count; ++i)
        {
          result.Write (right[i].Substring (start, length));
        }
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("split")]
    public void EvalSplit (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      // Would be nice to support muliple strings on the left, like "\" "/" split $blah
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
    public void EvalSplitw (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      RCArray<string> result = new RCArray<string> ();
      for (int i = 0; i < right.Count; ++i)
      {
        int start = 0;
        int end;
        while (true)
        {
          end = right[i].IndexOf (left[0], start);
          if (end >= 0) {
            result.Write (right[i].Substring (start, end - start));
            start = end + left[0].Length;
          }
          else {
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
        string padding = "".PadRight ((int) right[i], left[0][0]);
        result.Write (padding);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("indexof")]
    public void EvalIndexOf (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1) {
        throw new Exception ("indexof can only take one string on the left.");
      }
      RCArray<long> result = new RCArray<long> ();
      string value = left[0];
      int totalIndex = 0;
      for (int i = 0; i < right.Count; ++i)
      {
        int startIndex = 0;
        while (true)
        {
          startIndex = right[i].IndexOf (value,
                                         startIndex,
                                         right[i].Length - startIndex,
                                         StringComparison.InvariantCulture);
          if (startIndex > -1) {
            result.Write (totalIndex + startIndex);
            startIndex += value.Length;
          }
          else {
            break;
          }
        }
        totalIndex += right[i].Length;
      }
      runner.Yield (closure, new RCLong (result));
    }

    [RCVerb ("indexoflast")]
    public void EvalIndexOfLast (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1) {
        throw new Exception ("indexoflast can only take one string on the left.");
      }
      RCArray<long> result = new RCArray<long> ();
      string value = left[0];
      int totalIndex = 0;
      for (int i = 0; i < right.Count; ++i)
      {
        int startIndex = 0;
        while (true)
        {
          startIndex = right[i].LastIndexOf (value,
                                             startIndex,
                                             right[i].Length - startIndex,
                                             StringComparison.InvariantCulture);
          if (startIndex > -1) {
            result.Write (totalIndex + startIndex);
            startIndex += value.Length;
          }
          else {
            break;
          }
        }
        totalIndex += right[i].Length;
      }
      runner.Yield (closure, new RCLong (result));
    }

    [RCVerb ("slice")]
    public void EvalSlice (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      if (left.Count != 1) {
        throw new Exception ("left argument must contain exactly one delimeter");
      }
      if (left[0].Length != 1) {
        throw new Exception ("delimeter must be a single character");
      }

      char delimeter = left[0][0];
      List<List<string>> columns = new List<List<string>> ();
      for (int i = 0; i < right.Count; ++i)
      {
        string[] splitted = right[i].Split (delimeter);
        while (columns.Count < splitted.Length)
        {
          List<string> column = new List<string> ();
          for (int k = 0; k < i; ++k)
          {
            column.Add ("");
          }
          columns.Add (column);
        }
        for (int j = 0; j < columns.Count; ++j)
        {
          if (j < splitted.Length) {
            columns[j].Add (splitted[j]);
          }
          else {
            columns[j].Add ("");
          }
        }
      }
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < columns.Count; ++i)
      {
        result = new RCBlock (result,
                              "",
                              ":",
                              new RCString (new RCArray<string> (columns[i])));
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
      if (left.Count != 1) {
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
      if (left.Count != 1) {
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
      if (left.Count != 1) {
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
        if (i < right.Count - 1) {
          result.Append (left[0]);
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

    /// <summary>
    /// Apply a .net format string to the the right-hand data values.
    /// Given multiple format strings on the left,
    /// this variant of netformat will return multiple formatted result strings.
    /// </summary>
    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoNetFormat<string> (left, right.Data)));
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCLong right)
    {
      runner.Yield (closure, new RCString (DoNetFormat<long> (left, right.Data)));
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCDouble right)
    {
      runner.Yield (closure, new RCString (DoNetFormat<double> (left, right.Data)));
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCDecimal right)
    {
      runner.Yield (closure, new RCString (DoNetFormat<decimal> (left, right.Data)));
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCSymbol right)
    {
      runner.Yield (closure, new RCString (DoNetFormat<RCSymbolScalar> (left, right.Data)));
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      RCVectorBase[] columns = new RCVectorBase[right.Count];
      for (int i = 0; i < right.Count; ++i)
      {
        columns[i] = (RCVectorBase) right.Get (i);
        if (columns[i].Count != columns[0].Count) {
          throw new Exception (string.Format (
                                 "All columns must have the same count. Expected: {0}, Actual: {1}",
                                 columns[0].Count,
                                 columns[i].Count));
        }
      }
      RCArray<object[]> formatParams = new RCArray<object[]> (columns[0].Count);
      RCArray<string> result = new RCArray<string> (columns[0].Count);
      for (int i = 0; i < columns[0].Count; ++i)
      {
        formatParams.Write (new object[right.Count]);
        for (int j = 0; j < right.Count; ++j)
        {
          formatParams[i][j] = columns[j].Child (i);
          RCTimeScalar? time = formatParams[i][j] as RCTimeScalar ?;
          if (time.HasValue) {
            if (time.Value.Type == RCTimeType.Timespan) {
              throw new Exception (
                      "netformat does not handle Timespans, please pass a specific date and time.");
            }
            formatParams[i][j] = new DateTime (time.Value.Ticks);
          }
        }
      }
      for (int i = 0; i < formatParams.Count; ++i)
      {
        result.Write (string.Format (left[0], formatParams[i]));
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCTime right)
    {
      RCArray<DateTime> source = new RCArray<DateTime> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        if (right[i].Type == RCTimeType.Timespan) {
          throw new Exception (
                  "netformat does not handle Timespans, please pass a specific date and time.");
        }
        source.Write (new DateTime (right[i].Ticks));
      }
      runner.Yield (closure, new RCString (DoNetFormat<DateTime> (left, source)));
    }

    protected RCArray<string> DoNetFormat<T> (RCString left, RCArray<T> right)
    {
      RCArray<string> result = new RCArray<string> (left.Count);
      T[] data = right.ToArray ();
      // So fucken tiresome...
      object[] boxed = new object[data.Length];
      for (int i = 0; i < data.Length; ++i)
      {
        boxed[i] = data[i];
      }
      for (int i = 0; i < left.Count; ++i)
      {
        result.Write (string.Format (left[i], boxed));
      }
      return result;
    }

    [RCVerb ("netformat")]
    public void EvalNetformat (RCRunner runner, RCClosure closure, RCString left, RCCube right)
    {
      // Output is a cube with a single column
    }
  }
}

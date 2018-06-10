
using System;
using System.Text;

namespace RCL.Kernel
{
  public class Format
  {
    [RCVerb ("format")]
    public void EvalFormat (RCRunner runner, RCClosure closure, object right)
    {
      EvalFormat (runner, closure,
                  new RCSymbol (new RCSymbolScalar (null, "default")), right);
    }

    [RCVerb ("format")]
    public void EvalFormat (RCRunner runner, RCClosure closure, object left, object right)
    {
      //eventually should support xml and json, maybe csv?
      string which = "default";
      RCSymbol format = (RCSymbol) left;
      RCValue content = (RCValue) right;
      which = format[0].Part (0).ToString ();
      string result = null;
      if (which.Equals ("default"))
      {
        result = content.Format (RCFormat.Default);
      }
      else if (which.Equals ("pretty"))
      {
        result = content.Format (RCFormat.Pretty);
      }
      else if (which.Equals ("html"))
      {
        result = content.Format (RCFormat.Html);
      }
      else if (which.Equals ("csv"))
      {
        result = content.Format (RCFormat.Csv);
      }
      else if (which.Equals ("log"))
      {
        result = content.Format (RCFormat.Log);
      }
      else if (which.Equals ("text"))
      {
        result = DoTextFormat (right);
      }
      else throw new Exception ("Unknown format:" + which);
      runner.Yield (closure, new RCString (result));
    }

    public static string DoTextFormat (object right)
    {
      RCString text = right as RCString;
      if (text == null)
      {
        throw new Exception ("text format can only format strings");
      }
      StringBuilder builder = new StringBuilder ();
      for (int i = 0; i < text.Count; ++i)
      {
        builder.AppendLine (text[i]);
      }
      return builder.ToString ();
    }

    public static void DoFormat<T> (RCVector<T> vector, StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      if (vector.Count == 0)
      {
        builder.Append ("~");
        builder.Append (vector.TypeCode);
      }
      else
      {
        for (int i = 0; i < vector.Count; ++i)
        {
          vector.ScalarToString (builder, vector[i]);
          if (i < vector.Count - 1)
          {
            builder.Append (" ");
          }
          else builder.Append (vector.Suffix);
        }
      }
    }

    public static void DoFormat (RCOperator op, StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      if (op.Left != null)
      {
        if (op.Left.IsOperator)
        {
          builder.Append ("(");
          op.Left.Format (builder, args, colmap, level);
          builder.Append (")");
        }
        else
        {
          op.Left.Format (builder, args, colmap, level);
        }
        builder.Append (" ");
      }
      op.BodyToString (builder, args, level);
      builder.Append (" ");
      //Note Right is not allowed to be null.
      op.Right.Format (builder, args, colmap, level);
    }

    public static void DoFormat (RCBlock block, StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      if (block.Count == 0)
      {
        builder.Append ("{}");
        return;
      }
      builder.Append ("{");
      builder.Append (args.Newline);
      ++level;
      //Note the indexer requires a linear search backwards.
      //Maybe a custom iterator is in order?
      //It would also be useful for evaluation and other algorithms.
      for (int i = 0; i < block.Count; ++i)
      {
        RCBlock child = block.GetName (i);
        for (int tab = 0; tab < level; ++tab)
        {
          builder.Append (args.Indent);
        }
        if (child.EscapeName)
        {
          builder.Append (child.Name);
        }
        else
        {
          builder.Append (child.Name);
        }
        builder.Append (child.Evaluator.Symbol);
        if (child.Value != null)
        {
          child.Value.Format (builder, args, colmap, level);
        }
        else //Only the empty block has no value.
        {
          builder.Append ("{}");
        }
        if (i < block.Count - 1)
        {
          builder.Append (args.RowDelimeter);
        }
      }
      --level;
      builder.Append (args.Newline);
      for (int tab = 0; tab < level; ++tab)
      {
        builder.Append (args.Indent);
      }
      builder.Append ("}");
    }

    public static void DoFormat (RCTemplate template, StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      //templates need to follow the same indenting rules as everyone else please.
      builder.Append ("[");
      builder.Append ('?', template.EscapeCount);
      //Use AppendLine not args.Newline, because the newline is signficant
      //  and needs to be there no matter what.  Otherwise when we parse it again
      ++level;
      if (template.Multiline)
      {
        builder.Append ("\n");
        for (int tab = 0; tab < level; ++tab)
        {
          builder.Append (args.Indent);
        }
      }
      for (int i = 0; i < template.Count - 1; ++i)
      {
        RCValue child = template.Get (i);
        RCString str = child as RCString;
        if (str != null && i % 2 == 0)
        {
          for (int j = 0; j < str.Count; ++j)
          {
            //Now go through str one char at a time to find the newlines.
            int start = 0, end = 0;
            for (;end < str[j].Length; ++end)
            {
              if (str[j][end] == '\n')
              {
                string line = str[j].Substring (start, end - start);
                builder.Append (line);
                builder.Append ("\n");
                if (i < template.Count - 2 || end < str[j].Length - 1)
                {
                  for (int tab = 0; tab < level; ++tab)
                  {
                    builder.Append (args.Indent);
                  }
                }
                start = end + 1;
              }
              else if (end == str[j].Length - 1)
              {
                builder.Append (str[j].Substring (start, 1 + end - start));
              }
            }
          }
        }
        else
        {
          if (template.Multiline)
          {
            //for (int tab = 0; tab < level; ++tab)
            //{
            //  builder.Append (args.Indent);
            //}
            /*
            int k = builder.Length - 1;
            while (k >= 0)
            {
              if (builder[k] == '\n')
              {
                for (int tab = 0; tab < level; ++tab)
                {
                  builder.Append (args.Indent);
                }
                break;
              }
              else if (builder[k] != ' ')
              {
                break;
              }
              --k;
            }
            */
          }
          builder.Append ("[");
          builder.Append ('!', template.EscapeCount);
          builder.Append (' ');
          child.Format (builder, RCFormat.Default, colmap, level);
          builder.Append (' ');
          builder.Append ('!', template.EscapeCount);
          builder.Append ("]");
        }
      }
      --level;
      if (template.Multiline)
      {
        for (int tab = 0; tab < level; ++tab)
        {
          builder.Append (args.Indent);
        }
      }
      builder.Append ('?', template.EscapeCount);
      builder.Append ("]");
    }

    public static void DoFormat (RCReference reference, StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      builder.Append ("$");
      builder.Append (reference.Name);
    }
  }
}

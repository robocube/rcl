
using System;

namespace RCL.Kernel
{
  public class MarkdownContentToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      for (int current = start; current < code.Length; ++current)
      {
        //Console.Out.Write (code[current]);
        if (code[current] == '\r' || code[current] == '\n' ||
            code[current] == '_' || code[current] == '*')
        {
          int length = current - start;
          if (length > 0)
          {
            string text = code.Substring (start, length);
            return new RCToken (text, this, start, index);
          }
          else return null;
        }
      }
      throw new Exception ("No newline at end of content");
    }
    
    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownContent (token);
    }
    
    public override string TypeName
    {
      get { return "MarkdownContentToken"; }
    }
  }

  public class MarkdownBeginBoldToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "**");
      if (length > 0)
      {
        if (previous == null || previous.Text.EndsWith (" "))
        {
          return new RCToken ("**", this, start, index);  
        }
      }
      return null;
    }
    
    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownBeginBold (token);
    }
    
    public override string TypeName
    {
      get { return "MarkdownBeginBoldToken"; }
    }
  }

  public class MarkdownEndBoldToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "**");
      if (length > 0)
      {
        //EndsWith here is an approximation, needs to handle newlines
        //and also bold, italic tokens
        if (previous != null && !previous.Text.EndsWith (" "))
        {
          return new RCToken ("**", this, start, index);  
        }
      }
      return null;
    }
    
    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownEndBold (token);
    }
    
    public override string TypeName
    {
      get { return "MarkdownEndBoldToken"; }
    }
  }

  public class MarkdownBeginItalicToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "_");
      if (length > 0)
      {
        if (previous == null || previous.Text.EndsWith (" "))
        {
          return new RCToken ("_", this, start, index);  
        }
      }
      return null;
    }
    
    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownBeginItalic (token);
    }
    
    public override string TypeName
    {
      get { return "MarkdownBeginItalicToken"; }
    }
  }

  public class MarkdownEndItalicToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "_");
      if (length > 0)
      {
        if (previous != null && !previous.Text.EndsWith (" "))
        {
          return new RCToken ("_", this, start, index);  
        }
      }
      return null;
    }
    
    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownEndItalic (token);
    }
    
    public override string TypeName
    {
      get { return "MarkdownBeginItalicToken"; }
    }
  }
}

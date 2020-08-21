
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class ColumnDataToken : RCTokenType
  {
    protected readonly char[] m_separator;
    public ColumnDataToken (params char[] separator)
    {
      m_separator = separator;
    }

    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int current = start;
      bool quoted = false;
      while (quoted || (current < code.Length && !IsIn (code[current], m_separator)))
      {
        // What if there are an uneven number of "'s?
        // I think this is a bug.
        if (code[current] == '"') {
          quoted = !quoted;
        }
        ++current;
      }
      if (current > start) {
        string token = code.Substring (start, current - start);
        return new RCToken (token, this, start, index, line, 0);
      }
      else {
        return null;
      }
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptColumnData (token);
    }

    public override string TypeName
    {
      get { return "coldata"; }
    }

    public override string ParseString (RCLexer lexer, RCToken token)
    {
      if (token.Text[0] != '"') {
        return token.Text;
      }
      else {
        string undelim = token.Text.Substring (1, token.Text.Length - 2);
        return UnescapeControlChars (undelim, '"');
      }
    }
  }
}

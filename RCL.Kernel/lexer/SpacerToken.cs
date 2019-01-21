
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class SpacerToken : RCTokenType
  {
    public override RCToken TryParseToken (string text, int start, int index, int line, RCToken previous)
    {
      int current = start;
      if (text[current] == '|')
      {
        return new RCToken ("|", this, start, 1, line, 0);
      }
      else
      {
        while (current < text.Length && text[current] == '-') 
          ++current;
      }
      int length = current - start;
      if (length > 1)
      {
        string result = text.Substring (start, length);
        return new RCToken (result, this, start, index, line, 0);
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptSpacer (token);
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return null;
    }

    public override string TypeName
    {
      get { return "spacer"; }
    }
  }
}


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class StringToken : RCTokenType
  {
    public override RCToken TryParseToken (string text, int startPos, int index, RCToken previous)
    {
      int length = LengthOfEnclosedLiteral (text, startPos, '"');
      if (length < 0) return null;
      string result = text.Substring (startPos, length);
      return new RCToken (result, this, startPos, index);
    }

    public override bool IsEnclosedLiteral { get { return true; } }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptString (token);
    }

    public override string TypeName
    {
      get { return "string"; }
    }

    public override string ParseString (RCLexer lexer, RCToken token)
    {
      string undelim = token.Text.Substring (1, token.Text.Length - 2);
      return UnescapeControlChars (undelim, '"');
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseString (lexer, token);
    }
  }
}


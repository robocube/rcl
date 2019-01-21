
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class BooleanToken : RCTokenType
  {
    public override RCToken TryParseToken (string text, int start, int index, int line, RCToken previous)
    {
      int length = LengthOfBool (text, start);
      if (length < 0)
      {
        return null;
      }
      string result = text.Substring (start, length);
      return new RCToken (result, this, start, index, line, 0);
    }

    public static int LengthOfBool (string text, int start)
    {
      for (int i = 0; i < Booleans.Length; ++i)
      {
        int length = LengthOfKeyword (text, start, Booleans[i]);
        if (length > 0)
        {
          return length;
        }
      }
      return -1;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptBoolean (token);
    }

    public override string TypeName
    {
      get { return "boolean"; }
    }

    public override bool ParseBoolean (RCLexer lexer, RCToken token)
    {
      return bool.Parse (token.Text);
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseBoolean (lexer, token);
    }
  }
}

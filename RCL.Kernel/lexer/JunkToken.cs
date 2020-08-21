
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class JunkToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int lines;
      int length = LengthOfJunk (text, start, out lines);
      if (length < 0) {
        return null;
      }
      string token = text.Substring (start, length);
      return new RCToken (token, this, start, index, line, lines);
    }

    public static int LengthOfJunk (string text, int start, out int lines)
    {
      int end = text.IndexOfAny (WhiteChars, start, text.Length - start);
      if (end < 0) {
        end = text.Length;
      }
      lines = ScanForNewlines (text, start, end);
      return end - start;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptJunk (token);
    }

    public override string TypeName
    {
      get { return "junk"; }
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return token.Text;
    }
  }
}

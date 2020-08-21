
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class XMLBracketToken : RCTokenType
  {
    protected static readonly string[] Brackets =
      new string[] {"</", "/>", "<!--", "-->", "<", ">", "="};

    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int length = LengthOfOperator (text, start);
      if (length < 0) {
        return null;
      }
      string opChar = text.Substring (start, length);
      return new RCToken (opChar, this, start, index, line, 0);
    }

    public static int LengthOfOperator (string text, int start)
    {
      for (int i = 0; i < Brackets.Length; ++i)
      {
        int length = LengthOfKeyword (text, start, Brackets[i]);
        // Make sure we don't include == as an evaluator, it should be an operator.
        if (length > 0) {
          return length;
        }
      }
      return -1;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptXmlBracket (token);
    }

    public override string TypeName
    {
      get { return "xmlbracket"; }
    }
  }
}

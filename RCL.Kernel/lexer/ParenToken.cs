
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class ParenToken : RCTokenType
  {
    public static readonly char[] PAREN = new char[] { '(', ')' };
    public override RCToken TryParseToken (
      string code, int start, int index, RCToken previous)
    {
      if (IsIn(code[start], PAREN))
      {
        string token = code.Substring (start, 1);
        return new RCToken (token, this, start, index);
      }
      else return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptParen (token);
    }

    public override string TypeName
    {
      get { return "paren"; }
    }
  }
}
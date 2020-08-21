
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class NameToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      if (start < text.Length && text[start] >= '0' && text[start] <= '9') {
        return null;
      }
      int length = LengthOfDelimitedName (text, start, '.');
      if (length < 0) {
        return null;
      }
      string token = text.Substring (start, length);
      return new RCToken (token, this, start, index, line, 0);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptName (token);
    }

    public override string TypeName
    {
      get { return "name"; }
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return token.Text;
    }
  }

  public class XMLNameToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      if (start < text.Length && text[start] >= '0' && text[start] <= '9') {
        return null;
      }
      int length = LengthOfDelimitedName (text, start, ':');
      if (length < 0) {
        return null;
      }
      string token = text.Substring (start, length);
      return new RCToken (token, this, start, index, line, 0);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptName (token);
    }

    public override string TypeName
    {
      get { return "xmlname"; }
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return token.Text;
    }
  }
}

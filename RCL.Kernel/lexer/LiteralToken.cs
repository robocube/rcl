
using System;

namespace RCL.Kernel
{
  public class LiteralToken : RCTokenType
  {
    public override RCToken TryParseToken (string text, int start, int index, int line, RCToken previous)
    {
      int end = start;
      if (text[end] != '\\')
      {
        return null;
      }
      ++end;
      if (end >= text.Length)
      {
        return null;
      }
      char type = text[end];
      ++end;
      int length = -1;
      switch (type)
      {
        case 'x': length = LengthOfByteLiteral (text, end); break;
        default: throw new Exception ("Unknown literal type:" + type);
      }
      end += length;
      if (length > 0)
      {
        string token = text.Substring (start, end - start);
        return new RCToken (token, this, start, index, line, 0);
      }
      return null;
    }

    protected int LengthOfByteLiteral (string text, int start)
    {
      if (start + 1 < text.Length && IsHexChar (text[start]) && IsHexChar (text[start + 1]))
      {
        return 2;
      }
      else
      {
        return -1;
      }
    }

    protected bool IsHexChar (char c)
    {
      //Numbers.
      if (c >= 48 && c <= 57) return true;
      //Uppercase letters.
      if (c >= 65 && c <= 90) return true;
      //Lowercase letters.
      if (c >= 97 && c <= 122) return true;
      return false;
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseByte (lexer, token);
    }

    public override byte ParseByte (RCLexer lexer, RCToken token)
    {
      int major = HexCharToByte (token.Text[2]);
      int minor = HexCharToByte (token.Text[3]);
      return (byte)((major << 4) | minor);
    }

    protected int HexCharToByte (char c)
    {
      char C = char.ToUpper(c);
      return C < 'A' ? C - '0' : 10 + (C - 'A');
      //return (int)hexChar < (int)'A' ?
      //  ((int)hexChar - (int)'0') :
      //  10 + ((int)hexChar - (int)'A');
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptLiteral (token);
    }

    public override string TypeName
    {
      get { return "literal"; }
    }
  }
}

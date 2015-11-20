
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class NumberToken : RCTokenType
  {
    protected readonly static char[] NONNUMCHARS = new char[] { '-', '.' };
    protected readonly static char[] E = new char[] {'e', 'E'};

    public override RCToken TryParseToken (
      string text, int start, int index, RCToken previous)
    {
      //If the previous token is also a number, and there is no whitespace, the dash
      //should be considered an operator and not a sign on the number itself. ex "1-2"
      bool allowDash;
      if (previous == null)
      {
        allowDash = true;
      }
      else
      {
        allowDash = !(previous == null ||
                      previous.Type == RCTokenType.Number ||
                      previous.Type == RCTokenType.Reference ||
                      previous.Text == ")");
      }
      int length = LengthOfNumber (text, start, allowDash);
      if (length < 0) return null;
      string result = text.Substring (start, length);
      return new RCToken (result, this, start, index);
    }

    public static int LengthOfNumber (string text, int start, bool allowDash)
    {
      //not a perfect number lexer. let's work on it some more.
      //this must be why people use lexer generators.
      //if (start >= text.Length) return -1;
      int current = start;
      if (current < text.Length && text[current] == 'N' && text[current + 1] == 'a' && text[current + 2] == 'N') return 3;
      if (current < text.Length && text[current] == '-' && allowDash) ++current;
      if (current<text.Length && text[current] == '.') ++current;
      while (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current;
      if (current < text.Length && (current > start && text[current] == '.')) ++current;
      while (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current;
      if (current < text.Length && (current > start && IsIn (text[current], E)))
      {
        ++current;
        if (current<text.Length && (current>start && text[current] == '+')) ++current;
      }
      while (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current;
      //the last character of a number may be a letter that signifies the precise
      //type of storage to use for the result.
      if (current < text.Length && (current > start && IsIn(text[current], NumTypes))) ++current;
      int length = current - start;
      if (length == 1 && IsIn(text[current-1], NONNUMCHARS)) return -1;
      return length == 0 ? -1 : length;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptNumber (token);
    }

    public override string TypeName
    {
      get { return "number"; }
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      char last = token.Text[token.Text.Length - 1];
      switch (last)
      {
        case 'N': return double.NaN;
        case 'd': return ParseDouble (lexer, token);
        case 'l': return ParseLong (lexer, token);
        case 'm': return ParseDecimal (lexer, token);
        case 'i': return ParseInt (lexer, token);
        case 'f': return ParseFloat (lexer, token);
        //default: throw new Exception (
        //  "Cannot parse " + token.Text + ": If a number is used as a symbol or in a cube the type must be specified on every row");
      }
      if (token.Text.IndexOf ('.') > -1)
      {
        return ParseDouble (lexer, token);
      }
      else
      {
        return ParseLong (lexer, token);
      }
    }

    public override double ParseDouble (RCLexer lexer, RCToken token)
    {
      return double.Parse (ExtractNumber (token));
    }

    public override float ParseFloat (RCLexer lexer, RCToken token)
    {
      return float.Parse (ExtractNumber (token));
    }

    public override decimal ParseDecimal (RCLexer lexer, RCToken token)
    {
      return decimal.Parse (ExtractNumber (token));
    }

    public override int ParseInt (RCLexer lexer, RCToken token)
    {
      return int.Parse (ExtractNumber (token));
    }

    public override long ParseLong (RCLexer lexer, RCToken token)
    {
      return long.Parse (ExtractNumber (token));
    }

    /// <summary>
    /// Extract the number away from the string which might include the type suffix.
    /// </summary>
    public string ExtractNumber (RCToken token)
    {
      char last = token.Text[token.Text.Length - 1];
      if ((last >= '0' && last <= '9') || last == 'N')
      {
        return token.Text;
      }
      else
      {
        return token.Text.Substring (0, token.Text.Length - 1);
      }
    }
  }
}
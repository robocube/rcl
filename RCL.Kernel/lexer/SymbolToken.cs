
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class SymbolToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      if (start < code.Length && code[start] == '#') {
        int end = start + 1;
        if (end < code.Length) {
          int length = LengthOfDelimitedValue (code, end, ',');
          if (length > 0) {
            end += length;
          }
        }
        string token = code.Substring (start, end - start);
        return new RCToken (token, this, start, index, line, 0);
      }
      else {
        return null;
      }
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptScalar (token);
    }

    public override string TypeName
    {
      get { return "symbol"; }
    }

    public override RCSymbolScalar ParseSymbol (RCLexer lexer, RCToken token)
    {
      if (token.Text == "#") {
        return RCSymbolScalar.Empty;
      }
      string[] parts = token.Text.Split (',');
      string first = parts[0];
      // When "casting" strings into symbols, the initial # may be omitted
      // so that "a" becomes #a.
      if (first[0] == '#') {
        first = first.Substring (1);
      }
      RCToken child = lexer.LexSingle (first);
      object part = child.Parse (lexer);
      RCSymbolScalar result = new RCSymbolScalar (null, part);
      for (int i = 1; i < parts.Length; ++i)
      {
        child = lexer.LexSingle (parts[i]);
        result = new RCSymbolScalar (result, child.Parse (lexer));
      }
      return result;
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseSymbol (lexer, token);
    }
  }
}

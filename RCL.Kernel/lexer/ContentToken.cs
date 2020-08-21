
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class ContentToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      if (previous == null) {
        return null;
      }

      // we have to count the question marks in the previous token.
      if (previous.Type != RCTokenType.Block) {
        return null;
      }

      if (!(previous.Text.StartsWith ("[?") || previous.Text.EndsWith ("!]"))) {
        return null;
      }

      int lines = 0;
      int marks = 0;
      for (int i = 1; i < previous.Text.Length; ++i)
      {
        ++marks;
      }

      for (int end = start; end < code.Length; ++end)
      {
        if (code[end] == '[') {
          int look = end + 1;
          int reps = 0;
          while (code[look] == '!')
          {
            ++reps;
            if (reps == marks) {
              string token = code.Substring (start, end - start);
              return new RCToken (token, this, start, index, line, lines);
            }
            ++look;
          }
        }
        else if (code[end] == '?') {
          int look = end + 1;
          int reps = 1;
          while (code[look] == '?')
          {
            ++reps;
            ++look;
          }
          if (reps == marks && code[look] == ']') {
            string token = code.Substring (start, end - start);
            return new RCToken (token, this, start, index, line, lines);
          }
        }
        else if (code[end] == '\n') {
          ++lines;
        }
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptContent (token);
    }

    public override string ParseString (RCLexer lexer, RCToken token)
    {
      // Need to handle xml escape characters.
      // Need to do some stuff here with the whitespace, could be a little involved...
      return token.Text;
    }

    public override string TypeName
    {
      get { return "content"; }
    }
  }
}

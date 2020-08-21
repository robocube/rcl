
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class BlockToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      string token;
      if (code[start] == '{' || code[start] == '}') {
        token = code.Substring (start, 1);
        return new RCToken (token, this, start, index, line, 0);
      }

      int end = start;
      if (code[end] == '[') {
        ++end;
        while (code[end] == '?')
        {
          ++end;
        }
        if (1 < end - start) {
          // This is needed to handle the special case of [??]
          // I think that should be valid syntax just like [] and {}
          if (code[end] == ']') {
            end = start + ((end - start + 1) / 2);
          }
          goto END;
        }
        while (code[end] == '!')
        {
          ++end;
        }
        if (1 < end - start) {
          goto END;
        }
        return null;
      }
      else if (code[end] == '!') {
        while (code[end] == '!')
        {
          ++end;
        }
        if (code[end] == ']') {
          ++end;
          goto END;
        }
        return null;
      }
      else if (code[end] == '?') {
        while (code[end] == '?')
        {
          ++end;
        }
        if (code[end] == ']') {
          ++end;
          goto END;
        }
        return null;
      }
      else {
        return null;
      }

END:
      token = code.Substring (start, end - start);
      return new RCToken (token, this, start, index, line, 0);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptBlock (token);
    }

    public override string TypeName
    {
      get { return "block"; }
    }
  }
}

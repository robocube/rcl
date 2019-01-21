
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class EmptyVectorToken : RCTokenType
  {
    public override RCToken TryParseToken (string code, int start, int index, int line, RCToken previous)
    {
      if (code[start] == '~')
      {
        int end = start + 1;
        if (end < code.Length)
        {
          if (IsIn (code[end], TypeChars))
          {
            string token = code.Substring (start, 2);
            return new RCToken (token, this, start, index, line, 0);
          }
          return null;
        }
        return null;
      }
      else return null;
    }

    public override string TypeName
    {
      get { return "empty"; }
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptScalar (token);
    }
  }
}

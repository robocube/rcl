
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class NullToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int length = LengthOfKeyword (text, start, "null");
      if (length < 0) {
        return null;
      }
      string result = text.Substring (start, length);
      return new RCToken (result, this, start, index, line, 0);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptNull (token);
    }

    public override string TypeName
    {
      get { return "jsonnull"; }
    }
  }
}

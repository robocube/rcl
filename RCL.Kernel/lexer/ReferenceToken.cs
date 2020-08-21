
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class ReferenceToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int length = LengthOfReference (text, start);
      if (length < 0) {
        return null;
      }
      string tokenString = text.Substring (start, length);
      return new RCToken (tokenString, this, start, index, line, 0);
    }

    public static int LengthOfReference (string text, int start)
    {
      int end = start;
      // if it starts with a dollar make it a reference, not a symbol.
      if (IsIn (text[end], TypeChars)) {
        ++end;
      }
      if (end >= text.Length) {
        return -1;
      }
      if (text[end] != '$') {
        return -1;
      }
      else {
        end++;
      }
      int length = LengthOfDelimitedName (text, end, '.');
      if (length > 0) {
        end += length;
      }
      return end > start ? end - start : -1;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptReference (token);
    }

    public override string TypeName
    {
      get { return "reference"; }
    }
  }
}


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class CubeToken : RCTokenType
  {
    public static readonly char[] CUBE = new char[] { '[', ']' };
    public override RCToken TryParseToken (
      string code, int start, int index, RCToken previous)
    {
      if (IsIn (code[start], CUBE))
      {
        string token = code.Substring (start, 1);
        return new RCToken (token, this, start, index);
      }
      else return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptCube (token);
    }

    public override string TypeName
    {
      get { return "cube"; }
    }
  }
}
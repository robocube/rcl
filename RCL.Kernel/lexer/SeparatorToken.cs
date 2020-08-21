
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class SeparatorToken : KeywordLexer
  {
    public SeparatorToken ()
      : base (",", "\n", "\r\n") {
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptSeparator (token);
    }

    public override string TypeName
    {
      get { return "separator"; }
    }
  }
}
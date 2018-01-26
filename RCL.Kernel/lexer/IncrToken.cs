
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class IncrToken : RCTokenType
  {
    public override RCToken TryParseToken (
      string text, int start, int index, RCToken previous)
    {
      int length = LengthOfKeyword (text, start, "++");
      if (length > 0) return new RCToken ("++", this, start, index);
      length = LengthOfKeyword (text, start, "+-");
      if (length > 0) return new RCToken ("+-", this, start, index);
      length = LengthOfKeyword (text, start, "+~");
      if (length > 0) return new RCToken ("+~", this, start, index);
      //do not allow decrement as of yet, because I will first need to decide on a suitable replacement
      //for null, which is currently --.
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptIncr (token);
    }

    public override string TypeName
    {
      get { return "incr"; }
    }

    public override RCIncrScalar ParseIncr (RCLexer lexer, RCToken token)
    {
      //Assume it is always ++ because we don't do -- yet.
      if (token.Text == "++")
      {
        return RCIncrScalar.Increment;
      }
      else if (token.Text == "+-")
      {
        return RCIncrScalar.Decrement;
      }
      else if (token.Text == "+~")
      {
        return RCIncrScalar.Delete;
      }
      else throw new Exception ("Unrecognized incr token: " + token.Text);
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseIncr (lexer, token);
    }
  }
}

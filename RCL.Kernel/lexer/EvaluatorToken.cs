
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class EvaluatorToken : RCTokenType
  {
    public override RCToken TryParseToken (
      string text, int start, int index, RCToken previous)
    {
      int length = LengthOfOperator (text, start);
      if (length < 0) return null;
      string opChar = text.Substring (start, length);
      return new RCToken (opChar, this, start, index);
    }

    public static int LengthOfOperator (string text, int start)
    {
      for (int i = 0; i < Evaluators.Length; ++i)
      {
        int length = LengthOfKeyword (text, start, Evaluators[i]);
        //Make sure we don't include == as an evaluator, it should be an operator.
        if (length > 0 && text[start+length] != '=') return length;
      }
      return -1;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptEvaluator (token);
    }

    public override string TypeName
    {
      get { return "evaluator"; }
    }
  }
}
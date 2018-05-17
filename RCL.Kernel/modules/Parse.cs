
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Parse
  {
    [RCVerb ("parse")]
    public void EvalParse (
      RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, DoParse (new RCLParser (RCSystem.Activator), right));
    }

    [RCVerb ("parse")]
    public void EvalParse (
      RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      RCParser parser = null;
      string which = left[0].Part (0).ToString ();
      if (which.Equals ("csv"))
      {
        parser = new CSVParser ();
      }
      else if (which.Equals ("xml"))
      {
        parser = new XMLParser ();
      }
      else if (which.Equals ("json"))
      {
        parser = new JSONParser ();
      }
      else if (which.Equals ("rcl"))
      {
        parser = new RCLParser (RCSystem.Activator);
      }
      else if (which.Equals ("log"))
      {
        parser = new LogParser ();
      }
      else if (which.Equals ("md"))
      {
        parser = new MarkdownParser ();
      }
      else throw new Exception ("Unknown parser: " + which);
      runner.Yield (closure, DoParse (parser, right));
    }

    [RCVerb ("lex")]
    public void EvalLex (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<RCToken> output = new RCArray<RCToken> ();
      RCLParser.m_o2Lexer.Lex (right[0], output);
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < output.Count; i++)
      {
        result.Write (output[i].Text);
      }
      runner.Yield (closure, new RCString (result));
    }

    protected RCValue DoParse (RCParser parser, RCString right)
    {
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      for (int i = 0; i < right.Count; ++i)
        parser.Lex (right[i], tokens);
      bool fragment;
      RCValue result = parser.Parse (tokens, out fragment);
      return result;
    }
  }
}

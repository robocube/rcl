
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Parse
  {
    [RCVerb ("parse")]
    public void EvalParse (RCRunner runner, RCClosure closure, RCString right)
    {
      bool fragment;
      runner.Yield (closure, DoParse (new RCLParser (RCSystem.Activator), right, false, out fragment));
    }

    [RCVerb ("parse")]
    public void EvalParse (RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      RCParser parser = null;
      bool canonical = false;
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
      else if (which.Equals ("canonical"))
      {
        parser = new RCLParser (RCSystem.Activator);
        canonical = true;
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
      bool fragment;
      RCValue result = DoParse (parser, right, canonical, out fragment);
      runner.Yield (closure, result);
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

    [RCVerb ("lextype")]
    public void EvalLexType (RCRunner runner, RCClosure closure, RCString right)
    {
      RCArray<RCToken> output = new RCArray<RCToken> ();
      RCLParser.m_o2Lexer.Lex (right[0], output);
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < output.Count; i++)
      {
        result.Write (output[i].Type.TypeName);
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("tryparse")]
    public void EvalTryParse (RCRunner runner, RCClosure closure, RCString right)
    {
      bool fragment;
      RCValue val;
      RCBlock result = RCBlock.Empty;
      try
      {
        val = DoParse (new RCLParser (RCSystem.Activator), right, false, out fragment);
        result = new RCBlock (result, "status", ":", new RCLong (0));
        result = new RCBlock (result, "fragment", ":", new RCBoolean (fragment));
        result = new RCBlock (result, "data", ":", val);
      }
      catch (Exception ex)
      {
        result = new RCBlock (result, "status", ":", new RCLong (1));
        result = new RCBlock (result, "fragment", ":", new RCBoolean (false));
        string message = ex.ToString ();
        RCBlock report = new RCBlock ("", ":", new RCString (message + "\n"));
        int escapeCount = RCTemplate.CalculateReportTemplateEscapeLevel (message);
        result = new RCBlock (result, "error", ":", new RCTemplate (report, escapeCount, true));
      }
      runner.Yield (closure, result);
    }

    protected RCValue DoParse (RCParser parser, RCString right, bool canonical, out bool fragment)
    {
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      for (int i = 0; i < right.Count; ++i)
      {
        parser.Lex (right[i], tokens);
      }
      RCValue result = parser.Parse (tokens, out fragment, canonical);
      return result;
    }
  }
}

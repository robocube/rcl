
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
      runner.Yield (closure, DoParse (new RCLParser (runner.Activator), right));
    }

    [RCVerb ("parse")]
    public void EvalParse (
      RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      RCParser parser = null;
      string which = left[0].Part (0).ToString ();
      if (which.Equals ("csv"))
        parser = new CSVParser ();
      else if (which.Equals ("xml"))
        parser = new XMLParser ();
      else if (which.Equals ("json"))
        parser = new JSONParser ();
      else if (which.Equals ("o2"))
        parser = new RCLParser (runner.Activator);
      else throw new Exception ("Unknown parser: " + which);

      runner.Yield (closure, DoParse (parser, right));
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

using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class JSONParser : RCParser
  {
    protected readonly static RCLexer _jsonLexer;
    static JSONParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.WhiteSpace);
      types.Write (RCTokenType.String);
      types.Write (RCTokenType.Number);
      types.Write (RCTokenType.Boolean);
      types.Write (RCTokenType.Block);
      types.Write (RCTokenType.Cube);
      types.Write (RCTokenType.Null);
      types.Write (RCTokenType.CSVSeparator);
      types.Write (RCTokenType.Evaluator);
      _jsonLexer = new RCLexer (types);
    }

    public JSONParser ()
    {
      _lexer = _jsonLexer;
    }

    protected JSONState _state = JSONState.Default;
    protected Stack<RCBlock> _values = new Stack<RCBlock> ();
    protected Stack<string> _names = new Stack<string> ();
    protected Stack<JSONState> _states = new Stack<JSONState> ();
    protected RCBlock _value = null;
    protected string _name = null;

    protected enum JSONState
    {
      Default = 0,
      Name = 1,
      Value = 2
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      if (tokens.Count == 0) {
        fragment = true;
        return RCBlock.Empty;
      }
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      // TODO: fragment should be interpreted as in rcl.
      fragment = false;
      return _value;
    }

    public override void AcceptWhitespace (RCToken token) {}

    public override void AcceptString (RCToken token)
    {
      if (_state == JSONState.Name) {
        _name = token.ParseString (_lexer);
        _state = JSONState.Value;
      }
      else {
        AppendChild (new RCString (token.ParseString (_lexer)));
      }
    }

    public override void AcceptNumber (RCToken token)
    {
      AppendChild (new RCDouble (token.ParseDouble (_lexer)));
    }

    public override void AcceptBoolean (RCToken token)
    {
      AppendChild (new RCBoolean (token.ParseBoolean (_lexer)));
    }

    public override void AcceptBlock (RCToken token)
    {
      if (token.Text.Equals ("{")) {
        StartBlock ();
        _state = JSONState.Name;
      }
      else if (token.Text.Equals ("}")) {
        EndBlock ();
      }
    }

    public override void AcceptCube (RCToken token)
    {
      if (token.Text.Equals ("[")) {
        StartBlock ();
        _state = JSONState.Default;
      }
      else if (token.Text.Equals ("]")) {
        EndBlock ();
      }
    }

    public override void AcceptEvaluator (RCToken token) {}

    public override void AcceptNull (RCToken token)
    {
      AppendChild (RCBlock.Empty);
    }

    protected void StartBlock ()
    {
      if (_value != null) {
        _values.Push (_value);
        _names.Push (_name);
        _states.Push (_state);
      }
      _value = RCBlock.Empty;
      _name = "";
    }

    protected void EndBlock ()
    {
      if (_values.Count > 0) {
        RCBlock child = _value;
        _name = _names.Pop ();
        _value = _values.Pop ();
        _state = _states.Pop ();
        AppendChild (child);
      }
    }

    protected void AppendChild (RCValue scalar)
    {
      _value = new RCBlock (_value, _name, ":", scalar);
      if (_state == JSONState.Value) {
        _name = "";
        _state = JSONState.Name;
      }
    }
  }
}

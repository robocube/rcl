
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class JSONParser : RCParser
  {
    protected readonly static RCLexer m_jsonLexer;
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
      m_jsonLexer = new RCLexer (types);
    }
  
    public JSONParser ()
    {
      m_lexer = m_jsonLexer;
    }
  
    protected JSONState m_state = JSONState.Default;
    protected Stack<RCBlock> m_values = new Stack<RCBlock> ();
    protected Stack<string> m_names = new Stack<string> ();
    protected Stack<JSONState> m_states = new Stack<JSONState> ();
    protected RCBlock m_value = null;
    protected string m_name = null;
  
    protected enum JSONState
    {
      Default = 0,
      Name = 1,
      Value = 2
    }
  
    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment)
    {
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept(this, tokens[i]);
      }
      //TODO: fragment should be interpreted as in rcl.
      fragment = false;
      return m_value;
    }
  
    public override void AcceptWhitespace (RCToken token) {}
  
    public override void AcceptString (RCToken token)
    {
      if (m_state == JSONState.Name)
      {
        m_name = token.ParseString (m_lexer);
        m_state = JSONState.Value;
      }
      else
      {
        AppendChild (new RCString (token.ParseString (m_lexer)));
      }
    }
  
    public override void AcceptNumber (RCToken token)
    {
      AppendChild (new RCDouble (token.ParseDouble (m_lexer)));
    }
  
    public override void AcceptBoolean (RCToken token)
    {
      AppendChild (new RCBoolean (token.ParseBoolean (m_lexer)));
    }
  
    public override void AcceptBlock (RCToken token)
    {
      if (token.Text.Equals ("{"))
      {
        StartBlock ();
        m_state = JSONState.Name;
      }
      else if (token.Text.Equals ("}"))
      {
        EndBlock ();
      }
    }
  
    public override void AcceptCube (RCToken token)
    {
      if (token.Text.Equals ("["))
      {
        StartBlock ();
        m_state = JSONState.Default;
      }
      else if (token.Text.Equals ("]"))
      {
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
      if (m_value != null)
      {
        m_values.Push (m_value);
        m_names.Push (m_name);
        m_states.Push (m_state);
      }
      m_value = RCBlock.Empty;
      m_name = "";
    }
  
    protected void EndBlock ()
    {
      if (m_values.Count > 0)
      {
        RCBlock child = m_value;
        m_name = m_names.Pop ();
        m_value = m_values.Pop ();
        m_state = m_states.Pop ();
        AppendChild (child);
      }
    }
  
    protected void AppendChild (RCValue scalar)
    {
      m_value = new RCBlock (m_value, m_name, ":", scalar);
      if (m_state == JSONState.Value)
      {
        m_name = "";
        m_state = JSONState.Name;
      }
    }
  }
}

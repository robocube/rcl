
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class MarkdownParser : RCParser
  {
    protected readonly static RCLexer m_markdownLexer;
    static MarkdownParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.EndOfLine);
      types.Write (RCTokenType.MarkdownContentToken);
      types.Write (RCTokenType.MarkdownBeginBoldToken);
      types.Write (RCTokenType.MarkdownEndBoldToken);
      types.Write (RCTokenType.MarkdownBeginItalicToken);
      types.Write (RCTokenType.MarkdownEndItalicToken);
      m_markdownLexer = new RCLexer (types);
    }

    public MarkdownParser ()
    {
      m_lexer = m_markdownLexer;
    }

    protected MarkdownState m_state = MarkdownState.None;
    protected Stack<RCBlock> m_values = new Stack<RCBlock> ();
    protected Stack<string> m_names = new Stack<string> ();
    protected Stack<MarkdownState> m_states = new Stack<MarkdownState> ();
    protected RCBlock m_value = null;
    protected string m_name = null;

    protected enum MarkdownState
    {
      None,
      Paragraph,
      MaybeBR,
      Newline1
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment)
    {
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      fragment = false;
      if (m_state == MarkdownState.Newline1)
      {
        EndBlock ();
      }
      return m_value;
    }

    protected StringBuilder m_run = new StringBuilder ();
    public override void AcceptMarkdownContent (RCToken token) 
    {
      //Console.Out.WriteLine ("AcceptMarkdownContent: '{0}'", token.Text);
      if (m_state == MarkdownState.None)
      {
        m_state = MarkdownState.Paragraph;
        m_name = "p";
        StartBlock ();
        m_name = "";
        m_value = RCBlock.Empty;
      }
      if (m_state == MarkdownState.Paragraph ||
          m_state == MarkdownState.MaybeBR)
      {
        if (token.Text.EndsWith ("  "))
        {
          m_state = MarkdownState.MaybeBR;
        }
        else
        {
          m_state = MarkdownState.Paragraph;
        }
        m_run.Append (token.Text);
      }
      else if (m_state == MarkdownState.Newline1)
      {
        m_run.Append (" ");
        m_run.Append (token.Text);
        m_state = MarkdownState.Paragraph;
      }
      else
      {
        m_run.Append (token.Text);
      }
    }

    public override void AcceptEndOfLine (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptEndOfLine: '{0}'", token.Text);
      if (m_state == MarkdownState.Newline1)
      {
        EndBlock ();
        m_state = MarkdownState.None;
      }
      else if (m_state == MarkdownState.MaybeBR)
      {
        m_run.Remove (m_run.Length - 2, 2);
        m_run.AppendLine ();
      }
      else
      {
        m_state = MarkdownState.Newline1;
      }
    }

    public override void AcceptMarkdownBeginBold (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownBeginBold: '{0}'", token.Text);
      m_value = new RCBlock (m_value, "", ":", new RCString (m_run.ToString ()));
      m_name = "strong";
      StartBlock ();
      m_name = "";
      m_value = RCBlock.Empty;
    }

    public override void AcceptMarkdownEndBold (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownEndBold: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownBeginItalic (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownBeginItalic: '{0}'", token.Text);
      m_value = new RCBlock (m_value, "", ":", new RCString (m_run.ToString ()));
      m_name = "em";
      StartBlock ();
      m_name = "";
      m_value = RCBlock.Empty;
    }

    public override void AcceptMarkdownEndItalic (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownEndItalic: '{0}'", token.Text);
      EndBlock ();
    }

    protected void StartBlock ()
    {
      if (m_value == null)
      {
        m_value = RCBlock.Empty;
      }
      if (m_name == null)
      {
        m_name = "";
      }
      //Console.Out.WriteLine ("PUSH: m_name:{0} m_value:{1} m_state:{2}",
      //                       m_name, m_value.ToString (), m_state);
      m_values.Push (m_value);
      m_names.Push (m_name);
      m_states.Push (m_state);
      m_value = RCBlock.Empty;
      m_name = "";
      AppendRun ();
    }

    protected void EndBlock ()
    {
      AppendRun ();
      if (m_values.Count > 0)
      {
        RCBlock child = m_value;
        m_name = m_names.Pop ();
        m_value = m_values.Pop ();
        m_state = m_states.Pop ();
        m_value = new RCBlock (m_value, m_name, ":", child);
        //Console.Out.WriteLine ("POP: m_name:{0} m_value:{1} m_state:{2}",
        //                       m_name, m_value.ToString (), m_state);
        m_name = "";
      }
    }

    protected void AppendRun ()
    {
      if (m_run.Length == 0)
      {
        return;
      }
      RCString text = new RCString (m_run.ToString ());
      //Console.Out.WriteLine ("Adding run: " + m_run.ToString ());
      m_run.Clear ();
      m_value = new RCBlock (m_value, m_name, ":", text);
      //Console.Out.WriteLine ("New value: " + m_value.ToString ());
      m_name = "";
    }
  }
}

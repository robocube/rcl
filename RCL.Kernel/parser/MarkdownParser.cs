
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
      types.Write (RCTokenType.MarkdownLinkToken);
      types.Write (RCTokenType.MarkdownLiteralLinkToken);
      types.Write (RCTokenType.MarkdownHeaderToken);
      types.Write (RCTokenType.MarkdownBlockquoteToken);
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
      Link,
      Paragraph,
      MaybeBR,
      Newline1,
      Blockquote
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment)
    {
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      fragment = false;

      Console.Out.WriteLine ("Done parsing. Doing cleanup");
      if (m_quoteRun.Length > 0)
      {
        m_value = ParseEmbeddedRun (m_quoteRun.ToString ());
      }
      while (m_values.Count > 0)
      {
        EndBlock ();
      }
      
      if (m_run.Length > 0) //&& m_value == null)
      {
        m_value = new RCBlock (m_value, "", ":", new RCString (m_run.ToString ()));
      }
      else if (m_state == MarkdownState.Link && m_value == null)
      {
        m_value = new RCBlock (null, "", ":", new RCString (""));
      }
      return m_value;
    }

    protected StringBuilder m_run = new StringBuilder ();
    public override void AcceptMarkdownContent (RCToken token) 
    {
      Console.Out.WriteLine ("AcceptMarkdownContent: '{0}'", token.Text);
      Console.Out.WriteLine ("m_state: " + m_state);
      if (m_state == MarkdownState.None)
      {
        m_state = MarkdownState.Paragraph;
        m_name = "p";
        StartBlock ();
        m_name = "";
        m_value = RCBlock.Empty;
      }
      
      UpdateTextRun (m_run, token.Text);
    }

    protected void UpdateTextRun (StringBuilder run, string text)
    {
      if (m_state == MarkdownState.Paragraph ||
          m_state == MarkdownState.Link ||
          m_state == MarkdownState.MaybeBR)
      {
        if (text.EndsWith ("  "))
        {
          Console.Out.WriteLine ("Two spaces... MaybeBR?");
          m_state = MarkdownState.MaybeBR;
        }
        else
        {
          m_state = MarkdownState.Paragraph;
        }
        run.Append (text);
      }
      else if (m_state == MarkdownState.Newline1)
      {
        run.Append (" ");
        run.Append (text);
        m_state = MarkdownState.Paragraph;
      }
      else
      {
        run.Append (text);
      }
    }

    public override void AcceptEndOfLine (RCToken token)
    {
      Console.Out.WriteLine ("AcceptEndOfLine", token.Text);
      if (m_state == MarkdownState.Newline1)
      {
        EndBlock ();
        m_state = MarkdownState.None;
      }
      else if (m_state == MarkdownState.MaybeBR)
      {
        if (m_quoteRun.Length > 0)
        {
          Console.Out.WriteLine ("Inserting a newline");
          m_quoteRun.AppendLine ();
        }
        if (m_run.Length >= 2)
        {
          Console.Out.WriteLine ("Removing last two spaces");
          m_run.Remove (m_run.Length - 2, 2);
          m_run.AppendLine ();
        }
      }
      else
      {
        m_state = MarkdownState.Newline1;
      }
    }

    public override void AcceptMarkdownBeginBold (RCToken token)
    {
      Console.Out.WriteLine ("AcceptMarkdownBeginBold: '{0}'", token.Text);
      AppendRun ();
      m_name = "strong";
      StartBlock ();
      m_name = "";
      m_value = RCBlock.Empty;
    }

    public override void AcceptMarkdownEndBold (RCToken token)
    {
      Console.Out.WriteLine ("AcceptMarkdownEndBold: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownBeginItalic (RCToken token)
    {
      Console.Out.WriteLine ("AcceptMarkdownBeginItalic: '{0}'", token.Text);
      AppendRun ();
      m_name = "em";
      StartBlock ();
      m_name = "";
      m_value = RCBlock.Empty;
    }

    public override void AcceptMarkdownEndItalic (RCToken token)
    {
      Console.Out.WriteLine ("AcceptMarkdownEndItalic: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownLink (RCToken token)
    {
      Console.Out.WriteLine ("AcceptMarkdownLink: '{0}'", token.Text);
      Console.Out.WriteLine ("m_state: " + m_state);
      if (m_state == MarkdownState.Newline1)
      {
        m_run.Append (" ");
      }
      else if (m_state == MarkdownState.None)
      {
        m_name = "p";
        StartBlock ();
      }
      m_state = MarkdownState.Link;
      //[ will be at 1 in the case of ! img syntax
      int openBracket = token.Text.IndexOf ('[');
      int closingBracket = token.Text.IndexOf (']');
      int linkTextStart = openBracket + 1;
      int linkTextLength = closingBracket - linkTextStart;
      string linkText = token.Text.Substring (linkTextStart, linkTextLength);
      Console.Out.WriteLine ("Link Text: '{0}'", linkText);
      int openingParen = closingBracket + 1;
      int closingParen = token.Text.IndexOf (')', openingParen);
      int firstChar = openingParen + 1;
      string href = token.Text.Substring (firstChar, closingParen - firstChar);
      Console.Out.WriteLine ("Href Text: '{0}'", href);
      AppendRun ();
      RCBlock text = ParseEmbeddedRun (linkText);
      if (token.Text[0] == '[')
      {
        m_name = "a";
        StartBlock ();
        m_value = new RCBlock (RCBlock.Empty, "text", ":", text);
        m_value = new RCBlock (m_value, "href", ":", new RCString (href));
        EndBlock ();
      }
      else if (token.Text[0] == '!')
      {
        m_name = "img";
        StartBlock ();
        m_value = new RCBlock (RCBlock.Empty, "src", ":", new RCString (href));
        m_value = new RCBlock (m_value, "alt", ":", text);
        EndBlock ();
      }
      else throw new Exception ("Cannot parse link: " + token.Text);
    }

    public override void AcceptMarkdownLiteralLink (RCToken token)
    {
      if (m_state == MarkdownState.None)
      {
        m_name = "p";
        StartBlock ();
      }
      m_state = MarkdownState.Link;
      int openBracket = 0;
      int closeBracket = token.Text.Length - 1;
      int linkTextStart = openBracket + 1;
      int linkTextLength = closeBracket - linkTextStart;
      string linkText = token.Text.Substring (linkTextStart, linkTextLength);
      m_name = "a";
      StartBlock ();
      RCBlock textBlock = new RCBlock ("", ":", new RCString (linkText));
      m_value = new RCBlock (RCBlock.Empty, "text", ":", textBlock);
      m_value = new RCBlock (m_value, "href", ":", new RCString (linkText));
      EndBlock ();
    }

    public override void AcceptMarkdownHeader (RCToken token)
    {
      int space = token.Text.IndexOf (' ');
      int headerTextStart = space + 1;
      int headerTextLength = token.Text.TrimEnd ().Length - headerTextStart;
      string headerText = token.Text.Substring (headerTextStart, headerTextLength);
      int headerLevel = 0;
      while (token.Text[headerLevel] == '#')
      {
        ++headerLevel;
      }
      AppendRun ();
      RCBlock text = ParseEmbeddedRun (headerText);
      m_name = "h" + headerLevel.ToString ();
      StartBlock ();
      m_value = text;
      EndBlock ();
    }

    protected int m_quoteLevel = 0;
    protected StringBuilder m_quoteRun = new StringBuilder ();
    public override void AcceptMarkdownBlockquote (RCToken token)
    {
      Console.Out.WriteLine ("AcceptMarkdownBlockquote: '{0}'", token.Text);
      Console.Out.WriteLine ("m_state: " + m_state);
      int firstContent = -1;
      int quoteLevel = 0;
      for (int current = 0; current < token.Text.Length; ++current)
      {
        if (token.Text[current] != ' ' && token.Text[current] != '>')
        {
          firstContent = current;
          break;
        }
        if (token.Text[current] == '>')
        {
          ++quoteLevel;
        }
      }
      if (firstContent < 0)
      {
        //It's a blank line - nothing to do
        return;
      }
      int contentLength = token.Text.Length - firstContent;
      string text = token.Text.Substring (firstContent, contentLength);
      int levels = quoteLevel - m_quoteLevel;
      for (int level = 0; level < levels; ++level)
      {
        m_state = MarkdownState.Paragraph;
        m_name = "blockquote";
        StartBlock ();
        m_name = "p";
        StartBlock ();
      }

      if (quoteLevel > m_quoteLevel)
      {
        Console.Out.WriteLine ("quoteLevel increased");
        if (m_quoteRun.Length > 0)
        {
          m_value = ParseEmbeddedRun (m_quoteRun.ToString ());
          m_quoteRun.Clear ();
          EndBlock ();
          EndBlock ();
        }
      }
      else if (quoteLevel < m_quoteLevel)
      {
        Console.Out.WriteLine ("quoteLevel decreased");
        m_value = ParseEmbeddedRun (m_quoteRun.ToString ());
        m_quoteRun.Clear ();
        EndBlock ();
        m_name = "blockquote";
        StartBlock ();
        m_name = "p";
        StartBlock ();
      }
      UpdateTextRun (m_quoteRun, text);
      m_quoteLevel = quoteLevel;
    }

    protected RCBlock ParseEmbeddedRun (string text)
    {
      Console.Out.WriteLine ("Parsing embedded text: '{0}'", text.ToString ());
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      m_lexer.Lex (text, tokens);
      //Don't forget to move this above the call
      //AppendRun ();
      MarkdownParser parser = new MarkdownParser ();
      parser.m_state = MarkdownState.Link;
      bool fragment;
      RCBlock result = (RCBlock) parser.Parse (tokens, out fragment);
      Console.Out.WriteLine ("Reentry result: " + result.ToString ());
      return result;
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
      Console.Out.WriteLine ("PUSH: m_name:{0} m_value:{1} m_state:{2}",
                             m_name, m_value.ToString (), m_state);
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
        Console.Out.WriteLine ("POP: m_name:{0} m_value:{1} m_state:{2}",
                               m_name, m_value.ToString (), m_state);
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

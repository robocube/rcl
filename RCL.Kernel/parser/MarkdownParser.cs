
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
      types.Write (RCTokenType.MarkdownBeginBoldToken);
      types.Write (RCTokenType.MarkdownEndBoldToken);
      types.Write (RCTokenType.MarkdownBeginItalicToken);
      types.Write (RCTokenType.MarkdownEndItalicToken);
      types.Write (RCTokenType.MarkdownULItemToken);
      types.Write (RCTokenType.MarkdownOLItemToken);
      types.Write (RCTokenType.MarkdownLIIndentToken);
      types.Write (RCTokenType.MarkdownContentToken);
      m_markdownLexer = new RCLexer (types);
    }

    public MarkdownParser ()
    {
      m_lexer = m_markdownLexer;
    }

    protected MarkdownState m_state = MarkdownState.None;
    protected MarkdownState m_initialState = MarkdownState.None;
    protected Stack<RCBlock> m_values = new Stack<RCBlock> ();
    protected Stack<string> m_names = new Stack<string> ();
    protected Stack<MarkdownState> m_states = new Stack<MarkdownState> ();
    protected RCBlock m_value = null;
    protected string m_name = null;
    protected internal bool m_reentered = false;
    protected bool m_parsingList = false;
    protected int m_liLength = -1;
    protected bool m_parsingParagraph = false;
    protected bool m_blankLine = false;

    protected enum MarkdownState
    {
      None,
      Link,
      Paragraph,
      MaybeBR,
      Newline1,
      Blockquote,
      ListItem,
      Em,
      Bold
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      m_initialState = m_state;
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      fragment = false;

      //Console.Out.WriteLine ("Done parsing. Doing cleanup ({0})", m_state);
      FinishQuote (true);
      while (m_values.Count > 0)
      {
        EndBlock ();
      }
      if (m_run.Length > 0)
      {
        m_value = new RCBlock (m_value, "", ":", new RCString (m_run.ToString ()));
      }
      else if (m_initialState == MarkdownState.Link && m_value == null)
      {
        m_value = new RCBlock (null, "", ":", new RCString (""));
      }
      return m_value;
    }

    protected StringBuilder m_run = new StringBuilder ();
    public override void AcceptMarkdownContent (RCToken token) 
    {
      //Console.Out.WriteLine ("AcceptMarkdownContent({0}): '{1}'", m_state, token.Text);
      //Console.Out.WriteLine ("m_parsingParagraph: {0}", m_parsingParagraph);
      //Console.Out.WriteLine ("m_parsingList: {0}", m_parsingList);
      //Console.Out.WriteLine ("m_liLength: {0}", m_liLength);
      //Console.Out.WriteLine ("m_blankLine: {0}", m_blankLine);
      string text = token.Text;
      if (m_state == MarkdownState.ListItem)
      {
        WrapLITextIfNeeded (m_state);
      }
      if (m_liLength > -1)
      {
        bool indentedlip = true;
        for (int i = 0; i < m_liLength; ++i)
        {
          if (i < text.Length && text[i] != ' ')
          {
            indentedlip = false;
          }
        }
        if (indentedlip)
        {
          WrapLITextIfNeeded (m_state);
        }
      }
      FinishList ();
      if (m_parsingList && m_parsingParagraph &&
          m_blankLine && m_state == MarkdownState.None)
      {
        EndBlock ();
        m_parsingParagraph = false;
      }
      //m_parsingParagraph should be sufficient, shouldn't need m_run.Length check.
      if ((!m_parsingParagraph && m_blankLine && m_parsingList) ||
          (!m_parsingList && !m_parsingParagraph && m_run.Length == 0 &&
           (m_state == MarkdownState.None || m_state == MarkdownState.Newline1)))
      {
        m_state = MarkdownState.Paragraph;
        m_name = "p";
        StartBlock ();
        m_name = "";
        m_value = RCBlock.Empty;
        text = text.TrimStart ();
        m_parsingParagraph = true;
      }
      else if (m_state == MarkdownState.Newline1 || m_state == MarkdownState.MaybeBR)
      {
        text = text.TrimStart ();
      }
      UpdateTextRun (m_run, text);
    }

    protected void FinishList ()
    {
      if (m_parsingList && m_liLength <= 0 &&
          m_state != MarkdownState.Em &&
          m_state != MarkdownState.Bold &&
          m_state != MarkdownState.ListItem &&
          m_state != MarkdownState.Paragraph &&
          m_state != MarkdownState.Link)
      {
        while (true)
        {
          EndBlock ();
          string tag = m_names.Peek ();
          if (tag == "ol" || tag == "ul")
          {
            EndBlock ();
            break;
          }
        }
        //Console.Out.WriteLine ("Done parsing list");
        m_parsingList = false;
        m_parsingParagraph = false;
        m_state = MarkdownState.None;
      }
    }

    protected void UpdateTextRun (StringBuilder run, string text)
    {
      if (m_state == MarkdownState.Paragraph ||
          m_state == MarkdownState.Em ||
          m_state == MarkdownState.Bold ||
          m_state == MarkdownState.Link ||
          m_state == MarkdownState.Blockquote ||
          m_state == MarkdownState.MaybeBR ||
          m_state == MarkdownState.ListItem)
      {
        run.Append (text);
      }
      else if (m_state == MarkdownState.Newline1)
      {
        text = text.TrimStart ();
        if (m_parsingParagraph || run.Length > 0)
        {
          run.Append (" ");
        }
        run.Append (text);
        m_state = MarkdownState.Paragraph;
      }
      else
      {
        run.Append (text);
      }
      if (text.EndsWith ("  "))
      {
        m_state = MarkdownState.MaybeBR;
      }
      else
      {
        m_state = MarkdownState.Paragraph;
      }
    }

    public override void AcceptEndOfLine (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptEndOfLine({0})", m_state);
      //Console.Out.WriteLine ("  m_parsingParagraph: {0}", m_parsingParagraph);
      //Console.Out.WriteLine ("  m_parsingList: {0}", m_parsingList);
      if (token.Index == 0)
      {
        return;
      }
      if (m_state == MarkdownState.Newline1)
      {
        if (m_parsingList)
        {
          AppendRun ();
          m_blankLine = true;
          m_state = MarkdownState.None;
        }
        else if (m_quoteRun.Length > 0)
        {
          FinishQuote (true);
          EndBlock ();
          m_quoteLevel = 0;
          m_parsingParagraph = false;
          m_state = MarkdownState.None;
        }
        else
        {
          EndBlock ();
          m_parsingParagraph = false;
          m_state = MarkdownState.None;
        }
      }
      else if (m_state == MarkdownState.MaybeBR)
      {
        if (m_quoteRun.Length > 0)
        {
          m_quoteRun.Append ("\n");
        }
        if (m_run.Length >= 2)
        {
          if (m_run[m_run.Length - 1] != ' ') return;
          if (m_run[m_run.Length - 2] != ' ') return;
          m_run.Remove (m_run.Length - 2, 2);
          m_run.Append ("\n");
        }
      }
      // else if (m_state == MarkdownState.Paragraph)
      // {
      //   AppendRun ();
      //   WrapLITextIfNeeded (m_state);
      //   m_state = MarkdownState.Newline1;
      // }
      else
      {
        m_state = MarkdownState.Newline1;
      }
      m_liLength = -1;
    }

    public override void AcceptMarkdownBeginBold (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownBeginBold: '{0}'", token.Text);
      UpdateTextRun (m_run, "");
      AppendRun ();
      if (m_parsingList && m_blankLine)
      {
        m_name = "p";
        StartBlock ();
        m_parsingParagraph = true;
      }
      m_name = "strong";
      StartBlock ();
      m_name = "";
      m_value = RCBlock.Empty;
      m_state = MarkdownState.Bold;
    }

    public override void AcceptMarkdownEndBold (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownEndBold: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownBeginItalic (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownBeginItalic({0}): '{1}'", m_state, token.Text);
      UpdateTextRun (m_run, "");
      AppendRun ();
      if (m_parsingList && m_blankLine)
      {
        m_name = "p";
        StartBlock ();
        m_parsingParagraph = true;
      }
      m_name = "em";
      StartBlock ();
      m_name = "";
      m_value = RCBlock.Empty;
      m_state = MarkdownState.Em;
    }

    public override void AcceptMarkdownEndItalic (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownEndItalic: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownLink (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownLink: '{0}'", token.Text);
      //Console.Out.WriteLine ("m_state: " + m_state);
      if (m_state == MarkdownState.None || m_state == MarkdownState.Blockquote)
      {
        m_name = "p";
        StartBlock ();
        m_parsingParagraph = true;
      }
      FinishRuns (false);
      m_state = MarkdownState.Link;
      //[ will be at 1 in the case of ! img syntax
      int openBracket = token.Text.IndexOf ('[');
      int closingBracket = token.Text.IndexOf (']');
      int linkTextStart = openBracket + 1;
      int linkTextLength = closingBracket - linkTextStart;
      string linkText = token.Text.Substring (linkTextStart, linkTextLength);
      int openingParen = closingBracket + 1;
      int closingParen = token.Text.IndexOf (')', openingParen);
      int firstChar = openingParen + 1;
      string href = token.Text.Substring (firstChar, closingParen - firstChar);
      AppendRun ();
      bool reentered;
      RCBlock text = ParseEmbeddedRun (linkText, MarkdownState.Link, out reentered);
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
      //Do not break out of a p for a link
      FinishRuns (false);
      if (m_state == MarkdownState.None)
      {
        m_name = "p";
        StartBlock ();
        m_parsingParagraph = true;
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
      //Console.Out.WriteLine ("AcceptMarkdownHeader({0}): '{1}'", m_state, token.Text);
      //Console.Out.WriteLine ("m_liLength: " + m_liLength);
      FinishList ();
      FinishRuns (true);
      //Do break out of a p for a header
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
      bool reentered;
      RCBlock text = ParseEmbeddedRun (headerText, MarkdownState.Link, out reentered);
      m_name = "h" + headerLevel.ToString ();
      StartBlock ();
      m_value = text;
      EndBlock ();
    }

    protected int m_quoteLevel = 0;
    protected StringBuilder m_quoteRun = new StringBuilder ();
    public override void AcceptMarkdownBlockquote (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptMarkdownBlockquote({0}): '{1}'", m_state, token.Text);
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

      if (quoteLevel > m_quoteLevel)
      {
        FinishQuote (false);
        int levels = quoteLevel - m_quoteLevel;
        for (int level = 0; level < levels; ++level)
        {
          m_state = MarkdownState.Paragraph;
          m_name = "blockquote";
          StartBlock ();
        }
      }
      else if (quoteLevel < m_quoteLevel)
      {
        FinishQuote (true);
      }
      m_state = MarkdownState.Paragraph;
      UpdateTextRun (m_quoteRun, text);
      m_quoteRun.AppendLine ();
      m_quoteLevel = quoteLevel;
    }

    public override void AcceptMarkdownOLItem (RCToken token)
    {
      AcceptListItem (token, "ol");
    }

    public override void AcceptMarkdownULItem (RCToken token)
    {
      AcceptListItem (token, "ul");
    }

    protected void AcceptListItem (RCToken token, string listTagName)
    {
      //Console.Out.WriteLine ("AcceptListItem({0}): '{1}'", m_state, token.Text);
      //Console.Out.WriteLine ("m_parsingParagraph: {0}", m_parsingParagraph);
      //Console.Out.WriteLine ("m_parsingList: {0}", m_parsingList);
      //Console.Out.WriteLine ("m_blankLine: {0}", m_blankLine);
      MarkdownState oldState = m_state;
      m_state = MarkdownState.ListItem;
      if (m_parsingList)
      {
        FinishRuns (false);
        WrapLITextIfNeeded (oldState);
        while (m_names.Peek () != "li")
        {
          EndBlock ();
        }
        EndBlock ();
        m_parsingParagraph = false;
      }
      if (!m_parsingList && oldState == MarkdownState.None)
      {
        m_parsingList = true;
        m_name = listTagName;
        StartBlock ();
      }
      m_name = "li";
      //This may need to go onto the stack to do nested lists.
      StartBlock ();
    }

    public override void AcceptMarkdownLIIndent (RCToken token)
    {
      //Console.Out.WriteLine ("AcceptLIIndent({0}): '{1}'", m_state, token.Text);
      if (m_parsingList)
      {
        m_liLength = token.Text.Length;
        WrapLITextIfNeeded (m_state);
      }
    }

    protected void WrapLITextIfNeeded (MarkdownState oldState)
    {
      //Console.WriteLine("  WrapLITextIfNeeded ({0})", oldState);
      //Console.Out.WriteLine ("    m_parsingParagraph: {0}", m_parsingParagraph);
      //Console.Out.WriteLine ("    m_parsingList: {0}", m_parsingList);
      //Console.Out.WriteLine ("    m_blankLine: {0}", m_blankLine);
      if (m_parsingList && m_blankLine && !m_parsingParagraph)
      {
        //insert a new item
        //Console.WriteLine("    WrapLITextIfNeeded ITS NEEDED! m_value.Name: {0}", m_value.Name);
        if (m_value.Name == "") //|| m_value.Name == "em" || m_value.Name == "strong")
        {
          //Console.Out.WriteLine ("    INSERTING A P");
          m_value = new RCBlock (RCBlock.Empty, "p", ":", m_value);
        }
      }
    }

    protected void FinishRuns (bool endBlock)
    {
      if (m_run.Length > 0)
      {
        if (!endBlock && m_state == MarkdownState.Newline1)
        {
          m_run.Append (" ");
        }
        AppendRun ();
        if (endBlock)
        {
          EndBlock ();
        }
      }
      else if (m_value != null && m_value.Count > 0)
      {
        if (endBlock)
        {
          EndBlock ();
        }
      }
      if (m_quoteRun.Length > 0)
      {
        FinishQuote (true);
      }
    }

    protected void FinishQuote (bool endBlock)
    {
      if (m_quoteRun.Length > 0)
      {
        bool reentered;
        RCBlock embedded = ParseEmbeddedRun (m_quoteRun.ToString (),
                                             MarkdownState.Blockquote,
                                             out reentered);
        if (!reentered)
        {
          m_value = new RCBlock (m_value, "p", ":", embedded);
          if (endBlock)
          {
            EndBlock (); //Blockquote
          }
        }
        else
        {
          //Not sure if this is right yet.
          m_value = embedded;
        }
        m_quoteRun.Clear ();
      }
    }

    protected RCBlock ParseEmbeddedRun (string text,
                                        MarkdownState state,
                                        out bool reentered)
    {
      m_reentered = true;
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      m_lexer.Lex (text, tokens);
      MarkdownParser parser = new MarkdownParser ();
      parser.m_state = state;
      bool fragment;
      RCBlock result = (RCBlock) parser.Parse (tokens, out fragment, canonical:false);
      reentered = parser.m_reentered;
      return result;
    }

    protected void ShowStack (string location)
    {
      string[] names = m_names.ToArray ();
      Console.Out.Write ("  {0}: names: ", location);
      for (int i = 0; i < names.Length; ++i)
      {
        Console.Out.Write ("{0} ", names[i]);
      }
      RCBlock[] values = m_values.ToArray ();
      Console.Out.Write ("values: ");
      for (int i = 0; i < names.Length; ++i)
      {
        Console.Out.Write ("  {0} ", values[i]);
      }
      Console.WriteLine ();
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
      m_values.Push (m_value);
      m_names.Push (m_name);
      m_states.Push (m_state);
      //ShowStack ("StartBlock");
      m_value = RCBlock.Empty;
      m_name = "";
      AppendRun ();
    }

    protected void EndBlock ()
    {
      AppendRun ();
      if (m_values.Count > 0)
      {
        //ShowStack ("EndBlock");
        RCBlock child = m_value;
        m_name = m_names.Pop ();
        m_value = m_values.Pop ();
        m_state = m_states.Pop ();
        m_value = new RCBlock (m_value, m_name, ":", child);
        m_name = "";
        //Console.Out.WriteLine ("  EndBlock: m_value: {0}", m_value);
      }
    }

    protected void AppendRun ()
    {
      if (m_run.Length == 0)
      {
        return;
      }
      RCString text = new RCString (m_run.ToString ());
      m_run.Clear ();
      m_value = new RCBlock (m_value, m_name, ":", text);
      m_name = "";
    }
  }
}

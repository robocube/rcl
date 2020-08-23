
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class MarkdownParser : RCParser
  {
    protected readonly static RCLexer _markdownLexer;
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
      _markdownLexer = new RCLexer (types);
    }

    public MarkdownParser ()
    {
      _lexer = _markdownLexer;
    }

    protected MarkdownState _state = MarkdownState.None;
    protected MarkdownState _initialState = MarkdownState.None;
    protected Stack<RCBlock> _values = new Stack<RCBlock> ();
    protected Stack<string> _names = new Stack<string> ();
    protected Stack<MarkdownState> _states = new Stack<MarkdownState> ();
    protected RCBlock _value = null;
    protected string _name = null;
    protected internal bool _reentered = false;
    protected bool _parsingList = false;
    protected int _liLength = -1;
    protected bool _parsingParagraph = false;
    protected bool _blankLine = false;

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
      _initialState = _state;
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      fragment = false;

      // Console.Out.WriteLine ("Done parsing. Doing cleanup ({0})", _state);
      FinishQuote (true);
      while (_values.Count > 0)
      {
        EndBlock ();
      }
      if (_run.Length > 0) {
        _value = new RCBlock (_value, "", ":", new RCString (_run.ToString ()));
      }
      else if (_initialState == MarkdownState.Link && _value == null) {
        _value = new RCBlock (null, "", ":", new RCString (""));
      }
      return _value;
    }

    protected StringBuilder _run = new StringBuilder ();
    public override void AcceptMarkdownContent (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownContent({0}): '{1}'", _state, token.Text);
      // Console.Out.WriteLine ("_parsingParagraph: {0}", _parsingParagraph);
      // Console.Out.WriteLine ("_parsingList: {0}", _parsingList);
      // Console.Out.WriteLine ("_liLength: {0}", _liLength);
      // Console.Out.WriteLine ("_blankLine: {0}", _blankLine);
      string text = token.Text;
      if (_state == MarkdownState.ListItem) {
        WrapLITextIfNeeded (_state);
      }
      if (_liLength > -1) {
        bool indentedlip = true;
        for (int i = 0; i < _liLength; ++i)
        {
          if (i < text.Length && text[i] != ' ') {
            indentedlip = false;
          }
        }
        if (indentedlip) {
          WrapLITextIfNeeded (_state);
        }
      }
      FinishList ();
      if (_parsingList && _parsingParagraph &&
          _blankLine && _state == MarkdownState.None) {
        EndBlock ();
        _parsingParagraph = false;
      }
      // _parsingParagraph should be sufficient, shouldn't need _run.Length check.
      if ((!_parsingParagraph && _blankLine && _parsingList) ||
          (!_parsingList && !_parsingParagraph && _run.Length == 0 &&
           (_state == MarkdownState.None || _state == MarkdownState.Newline1))) {
        _state = MarkdownState.Paragraph;
        _name = "p";
        StartBlock ();
        _name = "";
        _value = RCBlock.Empty;
        text = text.TrimStart ();
        _parsingParagraph = true;
      }
      else if (_state == MarkdownState.Newline1 || _state == MarkdownState.MaybeBR) {
        text = text.TrimStart ();
      }
      UpdateTextRun (_run, text);
    }

    protected void FinishList ()
    {
      if (_parsingList && _liLength <= 0 &&
          _state != MarkdownState.Em &&
          _state != MarkdownState.Bold &&
          _state != MarkdownState.ListItem &&
          _state != MarkdownState.Paragraph &&
          _state != MarkdownState.Link) {
        while (true)
        {
          EndBlock ();
          string tag = _names.Peek ();
          if (tag == "ol" || tag == "ul") {
            EndBlock ();
            break;
          }
        }
        // Console.Out.WriteLine ("Done parsing list");
        _parsingList = false;
        _parsingParagraph = false;
        _state = MarkdownState.None;
      }
    }

    protected void UpdateTextRun (StringBuilder run, string text)
    {
      if (_state == MarkdownState.Paragraph ||
          _state == MarkdownState.Em ||
          _state == MarkdownState.Bold ||
          _state == MarkdownState.Link ||
          _state == MarkdownState.Blockquote ||
          _state == MarkdownState.MaybeBR ||
          _state == MarkdownState.ListItem) {
        run.Append (text);
      }
      else if (_state == MarkdownState.Newline1) {
        text = text.TrimStart ();
        if (_parsingParagraph || run.Length > 0) {
          run.Append (" ");
        }
        run.Append (text);
        _state = MarkdownState.Paragraph;
      }
      else {
        run.Append (text);
      }
      if (text.EndsWith ("  ")) {
        _state = MarkdownState.MaybeBR;
      }
      else {
        _state = MarkdownState.Paragraph;
      }
    }

    public override void AcceptEndOfLine (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptEndOfLine({0})", _state);
      // Console.Out.WriteLine ("  _parsingParagraph: {0}", _parsingParagraph);
      // Console.Out.WriteLine ("  _parsingList: {0}", _parsingList);
      if (token.Index == 0) {
        return;
      }
      if (_state == MarkdownState.Newline1) {
        if (_parsingList) {
          AppendRun ();
          _blankLine = true;
          _state = MarkdownState.None;
        }
        else if (_quoteRun.Length > 0) {
          FinishQuote (true);
          EndBlock ();
          _quoteLevel = 0;
          _parsingParagraph = false;
          _state = MarkdownState.None;
        }
        else {
          EndBlock ();
          _parsingParagraph = false;
          _state = MarkdownState.None;
        }
      }
      else if (_state == MarkdownState.MaybeBR) {
        if (_quoteRun.Length > 0) {
          _quoteRun.Append ("\n");
        }
        if (_run.Length >= 2) {
          if (_run[_run.Length - 1] != ' ') {
            return;
          }
          if (_run[_run.Length - 2] != ' ') {
            return;
          }
          _run.Remove (_run.Length - 2, 2);
          _run.Append ("\n");
        }
      }
      // else if (_state == MarkdownState.Paragraph)
      // {
      //   AppendRun ();
      //   WrapLITextIfNeeded (_state);
      //   _state = MarkdownState.Newline1;
      // }
      else {
        _state = MarkdownState.Newline1;
      }
      _liLength = -1;
    }

    public override void AcceptMarkdownBeginBold (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownBeginBold: '{0}'", token.Text);
      UpdateTextRun (_run, "");
      AppendRun ();
      if (_parsingList && _blankLine) {
        _name = "p";
        StartBlock ();
        _parsingParagraph = true;
      }
      _name = "strong";
      StartBlock ();
      _name = "";
      _value = RCBlock.Empty;
      _state = MarkdownState.Bold;
    }

    public override void AcceptMarkdownEndBold (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownEndBold: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownBeginItalic (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownBeginItalic({0}): '{1}'", _state,
      // token.Text);
      UpdateTextRun (_run, "");
      AppendRun ();
      if (_parsingList && _blankLine) {
        _name = "p";
        StartBlock ();
        _parsingParagraph = true;
      }
      _name = "em";
      StartBlock ();
      _name = "";
      _value = RCBlock.Empty;
      _state = MarkdownState.Em;
    }

    public override void AcceptMarkdownEndItalic (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownEndItalic: '{0}'", token.Text);
      EndBlock ();
    }

    public override void AcceptMarkdownLink (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownLink: '{0}'", token.Text);
      // Console.Out.WriteLine ("_state: " + _state);
      if (_state == MarkdownState.None || _state == MarkdownState.Blockquote) {
        _name = "p";
        StartBlock ();
        _parsingParagraph = true;
      }
      FinishRuns (false);
      _state = MarkdownState.Link;
      // [ will be at 1 in the case of ! img syntax
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
      if (token.Text[0] == '[') {
        _name = "a";
        StartBlock ();
        _value = new RCBlock (RCBlock.Empty, "text", ":", text);
        _value = new RCBlock (_value, "href", ":", new RCString (href));
        EndBlock ();
      }
      else if (token.Text[0] == '!') {
        _name = "img";
        StartBlock ();
        _value = new RCBlock (RCBlock.Empty, "src", ":", new RCString (href));
        _value = new RCBlock (_value, "alt", ":", text);
        EndBlock ();
      }
      else {
        throw new Exception ("Cannot parse link: " + token.Text);
      }
    }

    public override void AcceptMarkdownLiteralLink (RCToken token)
    {
      // Do not break out of a p for a link
      FinishRuns (false);
      if (_state == MarkdownState.None) {
        _name = "p";
        StartBlock ();
        _parsingParagraph = true;
      }
      _state = MarkdownState.Link;
      int openBracket = 0;
      int closeBracket = token.Text.Length - 1;
      int linkTextStart = openBracket + 1;
      int linkTextLength = closeBracket - linkTextStart;
      string linkText = token.Text.Substring (linkTextStart, linkTextLength);
      _name = "a";
      StartBlock ();
      RCBlock textBlock = new RCBlock ("", ":", new RCString (linkText));
      _value = new RCBlock (RCBlock.Empty, "text", ":", textBlock);
      _value = new RCBlock (_value, "href", ":", new RCString (linkText));
      EndBlock ();
    }

    public override void AcceptMarkdownHeader (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownHeader({0}): '{1}'", _state, token.Text);
      // Console.Out.WriteLine ("_liLength: " + _liLength);
      FinishList ();
      FinishRuns (true);
      // Do break out of a p for a header
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
      _name = "h" + headerLevel.ToString ();
      StartBlock ();
      _value = text;
      EndBlock ();
    }

    protected int _quoteLevel = 0;
    protected StringBuilder _quoteRun = new StringBuilder ();
    public override void AcceptMarkdownBlockquote (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptMarkdownBlockquote({0}): '{1}'", _state,
      // token.Text);
      int firstContent = -1;
      int quoteLevel = 0;
      for (int current = 0; current < token.Text.Length; ++current)
      {
        if (token.Text[current] != ' ' && token.Text[current] != '>') {
          firstContent = current;
          break;
        }
        if (token.Text[current] == '>') {
          ++quoteLevel;
        }
      }
      if (firstContent < 0) {
        // It's a blank line - nothing to do
        return;
      }
      int contentLength = token.Text.Length - firstContent;
      string text = token.Text.Substring (firstContent, contentLength);

      if (quoteLevel > _quoteLevel) {
        FinishQuote (false);
        int levels = quoteLevel - _quoteLevel;
        for (int level = 0; level < levels; ++level)
        {
          _state = MarkdownState.Paragraph;
          _name = "blockquote";
          StartBlock ();
        }
      }
      else if (quoteLevel < _quoteLevel) {
        FinishQuote (true);
      }
      _state = MarkdownState.Paragraph;
      UpdateTextRun (_quoteRun, text);
      _quoteRun.AppendLine ();
      _quoteLevel = quoteLevel;
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
      // Console.Out.WriteLine ("AcceptListItem({0}): '{1}'", _state, token.Text);
      // Console.Out.WriteLine ("_parsingParagraph: {0}", _parsingParagraph);
      // Console.Out.WriteLine ("_parsingList: {0}", _parsingList);
      // Console.Out.WriteLine ("_blankLine: {0}", _blankLine);
      MarkdownState oldState = _state;
      _state = MarkdownState.ListItem;
      if (_parsingList) {
        FinishRuns (false);
        WrapLITextIfNeeded (oldState);
        while (_names.Peek () != "li")
        {
          EndBlock ();
        }
        EndBlock ();
        _parsingParagraph = false;
      }
      if (!_parsingList && oldState == MarkdownState.None) {
        _parsingList = true;
        _name = listTagName;
        StartBlock ();
      }
      _name = "li";
      // This may need to go onto the stack to do nested lists.
      StartBlock ();
    }

    public override void AcceptMarkdownLIIndent (RCToken token)
    {
      // Console.Out.WriteLine ("AcceptLIIndent({0}): '{1}'", _state, token.Text);
      if (_parsingList) {
        _liLength = token.Text.Length;
        WrapLITextIfNeeded (_state);
      }
    }

    protected void WrapLITextIfNeeded (MarkdownState oldState)
    {
      // Console.WriteLine("  WrapLITextIfNeeded ({0})", oldState);
      // Console.Out.WriteLine ("    _parsingParagraph: {0}", _parsingParagraph);
      // Console.Out.WriteLine ("    _parsingList: {0}", _parsingList);
      // Console.Out.WriteLine ("    _blankLine: {0}", _blankLine);
      if (_parsingList && _blankLine && !_parsingParagraph) {
        // insert a new item
        // Console.WriteLine("    WrapLITextIfNeeded ITS NEEDED! _value.Name: {0}",
        // _value.Name);
        if (_value.Name == "") { // || _value.Name == "em" || _value.Name == "strong")
          // Console.Out.WriteLine ("    INSERTING A P");
          _value = new RCBlock (RCBlock.Empty, "p", ":", _value);
        }
      }
    }

    protected void FinishRuns (bool endBlock)
    {
      if (_run.Length > 0) {
        if (!endBlock && _state == MarkdownState.Newline1) {
          _run.Append (" ");
        }
        AppendRun ();
        if (endBlock) {
          EndBlock ();
        }
      }
      else if (_value != null && _value.Count > 0) {
        if (endBlock) {
          EndBlock ();
        }
      }
      if (_quoteRun.Length > 0) {
        FinishQuote (true);
      }
    }

    protected void FinishQuote (bool endBlock)
    {
      if (_quoteRun.Length > 0) {
        bool reentered;
        RCBlock embedded = ParseEmbeddedRun (_quoteRun.ToString (),
                                             MarkdownState.Blockquote,
                                             out reentered);
        if (!reentered) {
          _value = new RCBlock (_value, "p", ":", embedded);
          if (endBlock) {
            EndBlock (); // Blockquote
          }
        }
        else {
          // Not sure if this is right yet.
          _value = embedded;
        }
        _quoteRun.Clear ();
      }
    }

    protected RCBlock ParseEmbeddedRun (string text,
                                        MarkdownState state,
                                        out bool reentered)
    {
      _reentered = true;
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      _lexer.Lex (text, tokens);
      MarkdownParser parser = new MarkdownParser ();
      parser._state = state;
      bool fragment;
      RCBlock result = (RCBlock) parser.Parse (tokens, out fragment, canonical: false);
      reentered = parser._reentered;
      return result;
    }

    protected void ShowStack (string location)
    {
      string[] names = _names.ToArray ();
      Console.Out.Write ("  {0}: names: ", location);
      for (int i = 0; i < names.Length; ++i)
      {
        Console.Out.Write ("{0} ", names[i]);
      }
      RCBlock[] values = _values.ToArray ();
      Console.Out.Write ("values: ");
      for (int i = 0; i < names.Length; ++i)
      {
        Console.Out.Write ("  {0} ", values[i]);
      }
      Console.WriteLine ();
    }

    protected void StartBlock ()
    {
      if (_value == null) {
        _value = RCBlock.Empty;
      }
      if (_name == null) {
        _name = "";
      }
      _values.Push (_value);
      _names.Push (_name);
      _states.Push (_state);
      // ShowStack ("StartBlock");
      _value = RCBlock.Empty;
      _name = "";
      AppendRun ();
    }

    protected void EndBlock ()
    {
      AppendRun ();
      if (_values.Count > 0) {
        // ShowStack ("EndBlock");
        RCBlock child = _value;
        _name = _names.Pop ();
        _value = _values.Pop ();
        _state = _states.Pop ();
        _value = new RCBlock (_value, _name, ":", child);
        _name = "";
        // Console.Out.WriteLine ("  EndBlock: _value: {0}", _value);
      }
    }

    protected void AppendRun ()
    {
      if (_run.Length == 0) {
        return;
      }
      RCString text = new RCString (_run.ToString ());
      _run.Clear ();
      _value = new RCBlock (_value, _name, ":", text);
      _name = "";
    }
  }
}

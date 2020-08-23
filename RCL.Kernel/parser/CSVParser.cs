
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class CSVParser : RCParser
  {
    protected readonly static RCLexer _csvLexer;
    static CSVParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.CSVSeparator);
      types.Write (RCTokenType.CSVContent);
      _csvLexer = new RCLexer (types);
    }

    public CSVParser ()
    {
      _lexer = _csvLexer;
    }

    protected bool _header = true;
    protected int _column = -1;
    protected RCArray<string> _names = new RCArray<string> ();
    protected RCArray<RCArray<string>> _data = new RCArray<RCArray<string>> ();

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < _names.Count; ++i)
      {
        RCString column = new RCString (_data[i]);
        result = new RCBlock (result, _names[i], ":", column);
      }
      // TODO: Use when dealing with headless csvs.
      fragment = false;
      return result;
    }

    public override void AcceptColumnData (RCToken token)
    {
      if (_header) {
        _names.Write (token.Text);
        _data.Write (new RCArray<string> ());
      }
      else {
        _data[_column].Write (token.ParseString (_lexer));
        _column = (_column + 1) % _data.Count;
      }
    }

    protected RCToken _lastSeparator = null;
    public override void AcceptSeparator (RCToken token)
    {
      if (_header) {
        if (token.Text.Equals ("\n") || token.Text.Equals ("\r\n")) {
          _header = false;
          if (_names.Count > 0) {
            _column = 0;
          }
        }
      }
      // Handle empty strings between separators like ",,"
      // The parser will not see the empty string as a token so it needs a little
      // massaging.
      if (_column >= 0 &&
          _lastSeparator != null &&
          _lastSeparator.Start == (token.Start - _lastSeparator.Text.Length)) {
        _data[_column].Write ("");
        _column = (_column + 1) % _data.Count;
      }
      _lastSeparator = token;
    }
  }
}

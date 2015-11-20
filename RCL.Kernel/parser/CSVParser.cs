
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class CSVParser : RCParser
  {
    protected readonly static RCLexer m_csvLexer;
    static CSVParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.CSVSeparator);
      types.Write (RCTokenType.CSVContent);
      m_csvLexer = new RCLexer (types);
    }
  
    public CSVParser ()
    {
      m_lexer = m_csvLexer;
    }
  
    protected bool m_header = true;
    protected int m_column = 0;
    protected RCArray<string> m_names = new RCArray<string> ();
    protected RCArray<RCArray<string>> m_data = new RCArray<RCArray<string>> ();
  
    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment)
    {
      for (int i = 0; i < tokens.Count; ++i)
        tokens[i].Type.Accept (this, tokens[i]);
  
      RCBlock result = null;
      for (int i = 0; i < m_names.Count; ++i)
      {
        RCString column = new RCString (m_data[i]);
        result = new RCBlock (result, m_names[i], ":", column);
      }
      //TODO: Use when dealing with headless csvs.
      fragment = false;
      return result;
    }
  
    public override void AcceptColumnData (RCToken token)
    {
      if (m_header)
      {
        m_names.Write (token.Text);
        m_data.Write (new RCArray<string> ());
      }
      else
      {
        m_data[m_column].Write (token.ParseString (m_lexer));
        m_column = (m_column + 1) % m_data.Count;
      }
    }
  
    protected RCToken m_lastSeparator = null;
    public override void AcceptSeparator (RCToken token)
    {
      if (m_header)
      {
        if (token.Text.Equals ("\n") || token.Text.Equals ("\r\n"))
          m_header = false;
      }
      //Handle empty strings between separators like ",,"
      //The parser will not see the empty string as a token so it needs a little massaging.
      if (m_lastSeparator != null && m_lastSeparator.Start == (token.Start - m_lastSeparator.Text.Length))
      {
        m_data[m_column].Write ("");
        m_column = (m_column + 1) % m_data.Count;
      }
      m_lastSeparator = token;
    }
  }
}
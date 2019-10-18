
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class XMLParser : RCParser
  {
    protected readonly static RCLexer m_xmlLexer;
    static XMLParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.XmlDeclaration);
      types.Write (RCTokenType.XmlContent);
      types.Write (RCTokenType.WhiteSpace);
      //We need a "XmlString" to support single quoted strings.
      //Also escape chars in xml.
      types.Write (RCTokenType.XmlString);
      types.Write (RCTokenType.XmlBracket);
      types.Write (RCTokenType.XmlName);
      m_xmlLexer = new RCLexer (types);
    }

    public XMLParser ()
    {
      m_lexer = m_xmlLexer;
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      //There is always a root element in the stack.
      //This is to support fragments.
      m_contents.Push (RCBlock.Empty);
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept(this, tokens[i]);
      }
      //TODO: Use for xml fragments.
      fragment = false;
      return m_contents.Pop ();
    }

    public enum XmlState
    {
      None,
      OpenTag,
      Attributes,
      Content,
      CloseTag
    }

    protected XmlState m_state = XmlState.None;
    protected Stack<string> m_tags = new Stack<string> ();
    protected Stack<RCBlock> m_contents = new Stack<RCBlock> ();
    protected Stack<RCBlock> m_attributes = new Stack<RCBlock> ();
    protected RCValue m_default = new RCString ("");
    protected RCValue m_text = new RCString ("");
    protected string m_attribute = null;

    public override void AcceptXmlBracket (RCToken token)
    {
      if (token.Text.Equals ("<"))
      {
        m_contents.Push (RCBlock.Empty);
        m_attributes.Push (RCBlock.Empty);
        m_state = XmlState.OpenTag;
      }
      else if (token.Text.Equals (">"))
      {
        if (m_state == XmlState.Attributes)
          m_state = XmlState.Content;
      }
      else if (token.Text.Equals ("</") || token.Text.Equals ("/>"))
      {
        //The current child is already at the top of the stack see "<"
        RCBlock child = m_contents.Pop ();
        RCBlock attributes = m_attributes.Pop ();
        //null to be replaced with block of attributes
        if (child.Count == 0)
          child = new RCBlock (attributes, "", ":", m_text);
        else
          child = new RCBlock (attributes, "", ":", child);
        RCBlock next = new RCBlock (m_contents.Pop (), m_tags.Pop (), ":", child);
        m_contents.Push (next);
        m_text = m_default;
        m_state = XmlState.CloseTag;
      }
      else if (token.Text.Equals ("=")) {}
    }

    public override void AcceptName (RCToken token)
    {
      if (m_state == XmlState.OpenTag)
      {
        m_tags.Push (token.Text);
        m_state = XmlState.Attributes;
      }
      else if (m_state == XmlState.Attributes)
      {
        m_attribute = token.Text;
      }
    }

    public override void AcceptString (RCToken token)
    {
      m_attributes.Push (new RCBlock (m_attributes.Pop (), m_attribute, ":", new RCString (token.ParseString (m_lexer))));
    }

    public override void AcceptXmlContent (RCToken token)
    {
      m_text = new RCString (token.Text);
    }

    public override void AcceptXmlDeclaration (RCToken token)
    {
    }
  }
}

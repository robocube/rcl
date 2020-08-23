
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class XMLParser : RCParser
  {
    protected readonly static RCLexer _xmlLexer;
    static XMLParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.XmlDeclaration);
      types.Write (RCTokenType.XmlContent);
      types.Write (RCTokenType.WhiteSpace);
      // We need a "XmlString" to support single quoted strings.
      // Also escape chars in xml.
      types.Write (RCTokenType.XmlString);
      types.Write (RCTokenType.XmlBracket);
      types.Write (RCTokenType.XmlName);
      _xmlLexer = new RCLexer (types);
    }

    public XMLParser ()
    {
      _lexer = _xmlLexer;
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      // There is always a root element in the stack.
      // This is to support fragments.
      _contents.Push (RCBlock.Empty);
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      // TODO: Use for xml fragments.
      fragment = false;
      return _contents.Pop ();
    }

    public enum XmlState
    {
      None,
      OpenTag,
      Attributes,
      Content,
      CloseTag
    }

    protected XmlState _state = XmlState.None;
    protected Stack<string> _tags = new Stack<string> ();
    protected Stack<RCBlock> _contents = new Stack<RCBlock> ();
    protected Stack<RCBlock> _attributes = new Stack<RCBlock> ();
    protected RCValue _default = new RCString ("");
    protected RCValue _text = new RCString ("");
    protected string _attribute = null;

    public override void AcceptXmlBracket (RCToken token)
    {
      if (token.Text.Equals ("<")) {
        _contents.Push (RCBlock.Empty);
        _attributes.Push (RCBlock.Empty);
        _state = XmlState.OpenTag;
      }
      else if (token.Text.Equals (">")) {
        if (_state == XmlState.Attributes) {
          _state = XmlState.Content;
        }
      }
      else if (token.Text.Equals ("</") || token.Text.Equals ("/>")) {
        // The current child is already at the top of the stack see "<"
        RCBlock child = _contents.Pop ();
        RCBlock attributes = _attributes.Pop ();
        // null to be replaced with block of attributes
        if (child.Count == 0) {
          child = new RCBlock (attributes, "", ":", _text);
        }
        else {
          child = new RCBlock (attributes, "", ":", child);
        }
        RCBlock next = new RCBlock (_contents.Pop (), _tags.Pop (), ":", child);
        _contents.Push (next);
        _text = _default;
        _state = XmlState.CloseTag;
      }
      else if (token.Text.Equals ("=")) {}
    }

    public override void AcceptName (RCToken token)
    {
      if (_state == XmlState.OpenTag) {
        _tags.Push (token.Text);
        _state = XmlState.Attributes;
      }
      else if (_state == XmlState.Attributes) {
        _attribute = token.Text;
      }
    }

    public override void AcceptString (RCToken token)
    {
      _attributes.Push (new RCBlock (_attributes.Pop (),
                                      _attribute,
                                      ":",
                                      new RCString (token.ParseString (_lexer))));
    }

    public override void AcceptXmlContent (RCToken token)
    {
      _text = new RCString (token.Text);
    }

    public override void AcceptXmlDeclaration (RCToken token) {}
  }
}

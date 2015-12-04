
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public abstract class RCParser
  {
    protected RCLexer m_lexer;
    public virtual void Lex (string code, RCArray<RCToken> tokens)
    {
      m_lexer.Lex (code, tokens);
    }
  
    public virtual RCArray<RCToken> Lex (string code)
    {
      RCArray<RCToken> tokens = new RCArray<RCToken> ();
      Lex (code, tokens);
      return tokens;
    }
  
    public abstract RCValue Parse (RCArray<RCToken> tokens, out bool fragment);
  
    public virtual void AcceptName (RCToken token) {}
    public virtual void AcceptWhitespace (RCToken token) {}
    public virtual void AcceptReference (RCToken token) {}
    public virtual void AcceptParen (RCToken token) {}
    public virtual void AcceptBlock (RCToken token) {}
    public virtual void AcceptContent (RCToken token) {}
    public virtual void AcceptCube (RCToken token) {}
    public virtual void AcceptScalar (RCToken token) {}
    public virtual void AcceptEvaluator (RCToken token) {}
    public virtual void AcceptString (RCToken token) {}
    public virtual void AcceptNumber (RCToken token) {}
    public virtual void AcceptTime (RCToken token) {}
    public virtual void AcceptBoolean (RCToken token) {}
    public virtual void AcceptSpacer (RCToken token) {}
    public virtual void AcceptIncr (RCToken token) {}
    public virtual void AcceptLiteral (RCToken token) {}
    public virtual void AcceptJunk (RCToken token) {}
  
    //For CSV and XML files
    public virtual void AcceptColumnData (RCToken token) {}
    public virtual void AcceptSeparator (RCToken token) {}
    public virtual void AcceptNull (RCToken token) {}
    public virtual void AcceptXmlBracket (RCToken token) {}
    public virtual void AcceptXmlContent (RCToken token) {}
  }
}
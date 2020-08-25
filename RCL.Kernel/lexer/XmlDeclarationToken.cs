
namespace RCL.Kernel
{
  /// <summary>
  /// Recognizer for content between xml brackets.
  /// </summary>
  public class XMLDeclarationToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int startPos,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current = startPos;
      if (code[current] != '<')
      {
        return null;
      }
      ++current;
      if (code[current] != '?')
      {
        return null;
      }
      ++current;
      while (code[current] != '?')
      {
        ++current;
      }
      ++current;
      if (code[current] == '>')
      {
        string result = code.Substring (startPos, current - startPos);
        return new RCToken (result, this, startPos, index, line, 0);
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptXmlDeclaration (token);
    }

    public override string ParseString (RCLexer lexer, RCToken token)
    {
      return token.Text;
    }

    public override string TypeName
    {
      get { return "xmldeclaration"; }
    }
  }
}
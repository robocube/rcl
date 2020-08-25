
namespace RCL.Kernel
{
  public class XMLStringToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int startPos,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current = startPos;
      if (text[current] == '"')
      {
        ++current;
        while (text[current] != '"')
        {
          ++current;
        }
        ++current;
      }
      else if (text[current] == '\'')
      {
        ++current;
        while (text[current] != '\'')
        {
          ++current;
        }
        ++current;
      }
      else
      {
        return null;
      }
      string result = text.Substring (startPos, current - startPos);
      return new RCToken (result, this, startPos, index, line, 0);
    }

    public override bool IsEnclosedLiteral
    {
      get { return true; }
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptString (token);
    }

    public override string TypeName
    {
      get { return "xmlstring"; }
    }

    public override string ParseString (RCLexer lexer, RCToken token)
    {
      string undelim = token.Text.Substring (1, token.Text.Length - 2);
      return undelim;
      // return UnescapeControlChars (undelim, '"');
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseString (lexer, token);
    }
  }
}
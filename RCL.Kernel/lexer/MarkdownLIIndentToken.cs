
namespace RCL.Kernel
{
  public class MarkdownLIIndentToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      if (previous != null && previous.Type != RCTokenType.EndOfLine)
      {
        return null;
      }
      int current = start;
      while (current < code.Length && code[current] == ' ')
      {
        ++current;
      }
      if (current > start)
      {
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index, line, 0);
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownLIIndent (token);
    }

    public override string TypeName
    {
      get { return "MarkdownLIIndent"; }
    }
  }
}

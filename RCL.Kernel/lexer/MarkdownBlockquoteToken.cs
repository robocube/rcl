
namespace RCL.Kernel
{
  public class MarkdownBlockquoteToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      // A single line of a markdown blockquote
      int current = start;
      int quoteLevel = 0;
      while (current < code.Length && (code[current] == '>' || code[current] == ' '))
      {
        if (code[current] == '>')
        {
          ++quoteLevel;
        }
        ++current;
      }
      if (quoteLevel == 0)
      {
        return null;
      }
      for (; current < code.Length; ++current)
      {
        if (code[current] == '\r' || code[current] == '\n')
        {
          string text = code.Substring (start, current - start);
          return new RCToken (text, this, start, index, line, 0);
        }
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownBlockquote (token);
    }

    public override string TypeName
    {
      get { return "MarkdownBlockquoteToken"; }
    }
  }
}

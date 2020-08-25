
namespace RCL.Kernel
{
  public class MarkdownHeaderToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int current = start;
      int hcount = 0;
      while (current < code.Length && code[current] == '#')
      {
        ++hcount;
        ++current;
      }
      if (hcount == 0)
      {
        return null;
      }
      if (hcount > 6)
      {
        return null;
      }
      if (current < code.Length && code[current] == ' ')
      {
        for (; current < code.Length; ++current)
        {
          if (code[current] == '\r' || code[current] == '\n')
          {
            string text = code.Substring (start, current - start);
            return new RCToken (text, this, start, index, line, 1);
          }
        }
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownHeader (token);
    }

    public override string TypeName
    {
      get { return "MarkdownHeaderToken"; }
    }
  }
}
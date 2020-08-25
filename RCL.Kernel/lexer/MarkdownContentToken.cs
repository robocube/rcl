
namespace RCL.Kernel
{
  public class MarkdownContentToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      string text;
      int current = start;
      for (; current < code.Length; ++current)
      {
        if (code[current] == '\r' || code[current] == '\n' ||
            code[current] == '[' || code[current] == '_' || code[current] == '*')
        {
          int length = current - start;
          if (length > 0)
          {
            text = code.Substring (start, length);
            int lines = 0;
            // This token captures only 1 or 0 lines
            if (code[current] == '\r' || code[current] == '\n')
            {
              lines = 1;
            }
            return new RCToken (text, this, start, index, line, lines);
          }
          else
          {
            return null;
          }
        }
      }
      text = code.Substring (start, current - start);
      return new RCToken (text, this, start, index, line, 0);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownContent (token);
    }

    public override string TypeName
    {
      get { return "MarkdownContentToken"; }
    }
  }
}
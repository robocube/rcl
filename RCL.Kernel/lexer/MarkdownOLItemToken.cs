
namespace RCL.Kernel
{
  public class MarkdownOLItemToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current = start;
      while (current < code.Length && code[current] >= '0' && code[current] <= '9')
      {
        ++current;
      }
      if (current > start && current < code.Length && code[current] == '.')
      {
        ++current;
      }
      else
      {
        return null;
      }
      if (current < code.Length && code[current] == ' ')
      {
        ++current;
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index, line, 0);
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownOLItem (token);
    }

    public override string TypeName
    {
      get { return "MarkdownOLItem"; }
    }
  }
}

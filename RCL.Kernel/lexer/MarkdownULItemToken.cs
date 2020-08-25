
namespace RCL.Kernel
{
  public class MarkdownULItemToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "* ");
      if (length < 0)
      {
        return null;
      }
      int current = start + length;
      string text = code.Substring (start, current - start);
      return new RCToken (text, this, start, index, line, 0);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownULItem (token);
    }

    public override string TypeName
    {
      get { return "MarkdownULItem"; }
    }
  }
}
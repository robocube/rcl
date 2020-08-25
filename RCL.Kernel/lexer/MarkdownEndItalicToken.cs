
namespace RCL.Kernel
{
  public class MarkdownEndItalicToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "_");
      if (length > 0)
      {
        if (previous != null && !previous.Text.EndsWith (" "))
        {
          return new RCToken ("_", this, start, index, line, 0);
        }
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownEndItalic (token);
    }

    public override string TypeName
    {
      get { return "MarkdownBeginItalicToken"; }
    }
  }
}

namespace RCL.Kernel
{
  public class MarkdownBeginBoldToken : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int length = LengthOfKeyword (code, start, "**");
      if (length > 0)
      {
        if (previous == null ||
            previous.Text.EndsWith (" ") ||
            previous.Text.EndsWith ("\n"))
        {
          return new RCToken ("**", this, start, index, line, 0);
        }
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownBeginBold (token);
    }

    public override string TypeName
    {
      get { return "MarkdownBeginBoldToken"; }
    }
  }
}
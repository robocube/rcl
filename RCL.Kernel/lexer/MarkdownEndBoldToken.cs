
namespace RCL.Kernel
{
  public class MarkdownEndBoldToken : RCTokenType
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
        // EndsWith here is an approximation, needs to handle newlines
        // and also bold, italic tokens
        if (previous != null && !previous.Text.EndsWith (" "))
        {
          return new RCToken ("**", this, start, index, line, 0);
        }
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptMarkdownEndBold (token);
    }

    public override string TypeName
    {
      get { return "MarkdownEndBoldToken"; }
    }
  }
}
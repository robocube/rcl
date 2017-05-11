
namespace RCL.Kernel
{
  public class WhitespaceToken : RCTokenType
  {
    public override RCToken TryParseToken (
      string text, int start, int index, RCToken previous)
    {
      int length = LengthOfWhitespace (text, start);
      if (length < 0) return null;
      string whiteToken = text.Substring (start, length);
      return new RCToken (whiteToken, this, start, index);
    }

    public static int LengthOfWhitespace (string text, int start)
    {
      int current = start;
      while ((current < text.Length) && (IsIn(text[current], WhiteChars)))
        current++;
      if (current == start) return -1;
      else return current - start;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptWhitespace (token);
    }

    public override string TypeName
    {
      get { return "whitespace"; }
    }
  }
}
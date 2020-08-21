
namespace RCL.Kernel
{
  public class WhitespaceToken : RCTokenType
  {
    public override RCToken TryParseToken (string text,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken
                                           previous)
    {
      int lines;
      int length = LengthOfWhitespace (text, start, out lines);
      if (length < 0) {
        return null;
      }
      string whiteToken = text.Substring (start, length);
      return new RCToken (whiteToken, this, start, index, line, lines);
    }

    public static int LengthOfWhitespace (string text, int start, out int lines)
    {
      lines = 0;
      int current = start;
      while ((current < text.Length) && (IsIn (text[current], WhiteChars)))
      {
        current++;
      }
      if (current == start) {
        return -1;
      }
      else {
        lines = ScanForNewlines (text, start, current);
        return current - start;
      }
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

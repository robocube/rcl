
namespace RCL.Kernel
{
  public class LogEndOfLine : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current = start;
      if (code[current] == '\r')
      {
        ++current;
      }
      if (current < code.Length && code[current] == '\n')
      {
        ++current;
      }
      if (current > start)
      {
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index, line, 1);
      }
      else
      {
        return null;
      }
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptEndOfLine (token);
    }

    public override string TypeName
    {
      get { return "endofline"; }
    }
  }
}
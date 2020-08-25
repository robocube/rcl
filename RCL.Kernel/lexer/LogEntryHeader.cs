
namespace RCL.Kernel
{
  public class LogEntryHeader : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current = start;
      if (code[current] >= '0' && code[current] <= '9')
      {
        // it's either a timestamp or a bot number at the beginning of a line.
        while (current < code.Length && code[current] != '\r' && code[current] != '\n')
        {
          ++current;
        }
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index, line, 0);
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptLogEntryHeader (token);
    }

    public override string TypeName
    {
      get { return "logentryheader"; }
    }
  }
}


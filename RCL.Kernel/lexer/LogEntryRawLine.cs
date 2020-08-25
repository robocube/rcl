
namespace RCL.Kernel
{
  public class LogEntryRawLine : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current;
      int lines = 0;
      for (current = start; start < code.Length; ++current)
      {
        if (code[current] == '\r' || code[current] == '\n')
        {
          lines = 1;
          break;
        }
      }
      string text = code.Substring (start, current - start);
      return new RCToken (text, this, start, index, line, lines);
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptLogEntryRawLine (token);
    }

    public override string TypeName
    {
      get { return "logentryrawline"; }
    }
  }
}
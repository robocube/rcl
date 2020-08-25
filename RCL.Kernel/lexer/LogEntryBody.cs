
namespace RCL.Kernel
{
  public class LogEntryBody : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           int line,
                                           RCToken previous)
    {
      int current = start;
      while (current < code.Length && code[current] == ' ')
      {
        ++current;
        if (!(current < code.Length && code[current] == ' '))
        {
          return null;
        }
        ++current;
        while (current < code.Length && !(code[current] == '\r' || code[current] == '\n'))
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
      parser.AcceptLogEntryBody (token);
    }

    public override string TypeName
    {
      get { return "logentrybody"; }
    }
  }
}
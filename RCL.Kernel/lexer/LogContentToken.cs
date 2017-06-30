
namespace RCL.Kernel
{
  public class EndOfLine : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
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
        return new RCToken (text, this, start, index);
      }
      else return null;
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

  public class LogEntryHeader : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int current = start;
      if (code[current] >= '0' && code[current] <= '9')
      {
        //it's either a timestamp or a bot number at the beginning of a line.
        while (current < code.Length && code[current] != '\r' && code[current] != '\n')
        {
          ++current;
        }
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index);
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

  public class LogEntryRawLine : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int current;
      for (current = start; start < code.Length; ++current)
      {
        if (code[current] == '\r' || code[current] == '\n')
        {
          break;
        }
      }
      string text = code.Substring (start, current - start);
      return new RCToken (text, this, start, index);
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

  /*
  public class LogEntryMessage : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int current = start;
      if (code[current] == ' ')
      {
        while (current < code.Length && code[current] != '\n')
        {
          ++current;
        }
        if (current < code.Length && code[current] == '\n')
        {
          ++current;
        }
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index);
      }
      return null;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptLogEntryMessage (token);
    }

    public override string TypeName
    {
      get { return "logentrymessage"; }
    }
  }
  */

  public class LogEntryBody : RCTokenType
  {
    public override RCToken TryParseToken (string code,
                                           int start,
                                           int index,
                                           RCToken previous)
    {
      int current = start;
      while (current < code.Length && code[current] == ' ')
      {
        ++current;
        if (!(current < code.Length && code[current] == ' ')) return null;
        ++current;
        while (current < code.Length && !(code[current] == '\r' || code[current] == '\n'))
        {
          ++current;
        }
        string text = code.Substring (start, current - start);
        return new RCToken (text, this, start, index);
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

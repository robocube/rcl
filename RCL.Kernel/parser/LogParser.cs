
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class LogParser : RCParser
  {
    protected readonly static RCLexer m_logLexer;

    static LogParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.LogEntryHeader);
      types.Write (RCTokenType.LogEntryMessage);
      types.Write (RCTokenType.LogEntryDocument);
      m_logLexer = new RCLexer (types);
    }

    RCToken m_time = null;
    RCToken m_bot = null;
    RCToken m_fiber = null;
    RCToken m_module = null;
    RCToken m_instance = null;
    RCToken m_event = null;
    string m_message = null;
    string m_document = null;
    RCCube m_result = new RCCube ();
  
    public LogParser ()
    {
      m_lexer = m_logLexer;
    }
  
    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment)
    {
      fragment = false;
      m_result = new RCCube (new Timeline (null, null, null, null));
      m_result.ReserveColumn ("bot");
      m_result.ReserveColumn ("fiber");
      m_result.ReserveColumn ("module");
      m_result.ReserveColumn ("instance");
      m_result.ReserveColumn ("event");
      m_result.ReserveColumn ("message");
      m_result.ReserveColumn ("document");
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      if (m_bot != null)
      {
        AppendEntry ();
      }
      return m_result;
    }

    public override void AcceptLogEntryHeader (RCToken token)
    {
      if (m_bot != null)
      {
        AppendEntry ();
      }
      int current = 0;
      m_time = RCTokenType.Time.TryParseToken (token.Text, current, 0, null);
      if (m_time != null)
      {
        current += m_time.Text.Length;
        //skip the single space. Do not validate.
        //This requires log files to only use single spaces between header values.
        ++current;
      }

      m_bot = RCTokenType.Number.TryParseToken (token.Text, current, 0, null);
      if (m_bot != null)
      {
        current += m_bot.Text.Length;
      }
      ++current;

      m_fiber = RCTokenType.Number.TryParseToken (token.Text, current, 0, null);
      if (m_fiber != null)
      {
        current += m_fiber.Text.Length;
      }
      ++current;

      m_module = RCTokenType.Name.TryParseToken (token.Text, current, 0, null);
      if (m_module != null)
      {
        current += m_module.Text.Length;
      }
      ++current;

      m_instance = RCTokenType.Number.TryParseToken (token.Text, current, 0, null);
      if (m_instance != null)
      {
        current += m_instance.Text.Length;
      }
      ++current;

      m_event = RCTokenType.Name.TryParseToken (token.Text, current, 0, null);
      if (m_event != null)
      {
        current += m_event.Text.Length;
      }
      ++current;
    }

    public override void AcceptLogEntryMessage (RCToken token)
    {
      //This first char is always ':'
      int end = token.Text.Length - 1;
      if (end > 0 && token.Text[end] == '\n')
      {
        --end;
      }
      if (end > 0 && token.Text[end] == '\r')
      {
        --end;
      }
      m_message = token.Text.Substring (1, end);
    }

    protected StringBuilder m_builder = new StringBuilder ();
    public override void AcceptLogEntryDocument (RCToken token)
    {
      m_builder.Clear ();
      //This is not correct. Need to take out the indentation.
      int start = 2;
      int current = 2;
      while (current < token.Text.Length)
      {
        if (token.Text[current] == '\n')
        {
          m_builder.AppendLine (token.Text.Substring (start, current - start));
          start = current + 3;
          current = start;
        }
        else if (token.Text[current] == '\r')
        {
          m_builder.AppendLine (token.Text.Substring (start, current - start));
          start = current + 4;
          current = start;
        }
        else
        {
          ++current;
        }
      }
      m_document = m_builder.ToString ();
    }

    protected void AppendEntry ()
    {
      if (m_time != null)
      {
        m_result.WriteCell ("time", null, m_time.ParseTime (m_lexer));
      }
      m_result.WriteCell ("bot", null, m_bot.ParseLong (m_lexer));
      m_result.WriteCell ("fiber", null, m_fiber.ParseLong (m_lexer));
      m_result.WriteCell ("module", null, m_module.Text);
      m_result.WriteCell ("instance", null, m_instance.ParseLong (m_lexer));
      m_result.WriteCell ("event", null, m_event.Text);
      if (m_message != null)
      {
        m_result.WriteCell ("message", null, m_message);
      }
      if (m_document != null)
      {
        m_result.WriteCell ("document", null, m_document);
      }
      m_result.Axis.Write ();

      //Reset everything for the next log entry.
      m_time = null;
      m_bot = null;
      m_fiber = null;
      m_module = null;
      m_instance = null;
      m_event = null;
      m_message = null;
      m_document = null;
    }
  }
}

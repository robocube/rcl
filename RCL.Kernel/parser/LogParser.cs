
using System.Text;

namespace RCL.Kernel
{
  public class LogParser : RCParser
  {
    protected readonly static RCLexer m_logLexer;

    static LogParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.LogEntryHeader);
      types.Write (RCTokenType.EndOfLine);
      types.Write (RCTokenType.LogEntryBody);
      types.Write (RCTokenType.LogEntryRawLine);
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
    StringBuilder m_builder = new StringBuilder ();
    RCCube m_result = new RCCube ();

    public LogParser ()
    {
      m_lexer = m_logLexer;
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      fragment = false;
      m_result = new RCCube (new Timeline ());
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
        if (m_builder.Length > 0)
        {
          m_document = m_builder.ToString ();
        }
        AppendEntry ();
      }
      return m_result;
    }

    public override void AcceptLogEntryHeader (RCToken token)
    {
      if (m_bot != null)
      {
        if (m_builder.Length > 0)
        {
          m_document = m_builder.ToString ();
        }
        AppendEntry ();
      }

      int current = 0;
      m_time = RCTokenType.Time.TryParseToken (token.Text, current, 0, 0, null);
      if (m_time != null)
      {
        current += m_time.Text.Length;
        //skip the single space. Do not validate.
        //This requires log files to only use single spaces between header values.
        ++current;
      }

      m_bot = RCTokenType.Number.TryParseToken (token.Text, current, 0, 0, null);
      if (m_bot != null)
      {
        current += m_bot.Text.Length;
      }
      ++current;

      m_fiber = RCTokenType.Number.TryParseToken (token.Text, current, 0, 0, null);
      if (m_fiber != null)
      {
        current += m_fiber.Text.Length;
      }
      ++current;

      m_module = RCTokenType.Name.TryParseToken (token.Text, current, 0, 0, null);
      if (m_module != null)
      {
        current += m_module.Text.Length;
      }
      ++current;

      m_instance = RCTokenType.Number.TryParseToken (token.Text, current, 0, 0, null);
      if (m_instance != null)
      {
        current += m_instance.Text.Length;
      }
      ++current;

      m_event = RCTokenType.Name.TryParseToken (token.Text, current, 0, 0, null);
      if (m_event != null)
      {
        current += m_event.Text.Length;
      }
      ++current;

      if (current <= token.Text.Length)
      {
        m_message = token.Text.Substring (current);
      }
      else
      {
        m_message = null;
      }
      m_builder.Clear ();
    }

    public override void AcceptEndOfLine (RCToken token)
    {
      
    }

    public override void AcceptLogEntryBody (RCToken token)
    {
      m_builder.Append (token.Text.Substring (2));
      //Prevent inconsistent string content on Windows.
      //Only write CRLFs when persisting text.
      m_builder.Append ("\n");
    }

    protected readonly static char[] TRIM_CHARS = new char[] { '\r', '\n' };
    public override void AcceptLogEntryRawLine (RCToken token)
    {
      if (m_bot != null)
      {
        if (m_builder.Length > 0)
        {
          m_document = m_builder.ToString ();
        }
        AppendEntry ();
      }

      m_time = null;
      m_bot = null;
      m_fiber = null;
      m_module = null;
      m_instance = null;
      m_event = null;
      m_message = token.Text.TrimEnd (TRIM_CHARS);
      m_document = null;
      AppendEntry ();
    }

    protected void AppendEntry ()
    {
      if (m_time != null)
      {
        m_result.WriteCell ("time", null, m_time.ParseTime (m_lexer));
      }
      if (m_bot != null)
      {
        m_result.WriteCell ("bot", null, m_bot.ParseLong (m_lexer));
      }
      else
      {
        m_result.WriteCell ("bot", null, (long) -1);
      }
      if (m_fiber != null)
      {
        m_result.WriteCell ("fiber", null, m_fiber.ParseLong (m_lexer));
      }
      else
      {
        m_result.WriteCell ("fiber", null, (long) -1);
      }
      if (m_module != null)
      {
        m_result.WriteCell ("module", null, m_module.Text);
      }
      else
      {
        m_result.WriteCell ("module", null, "");
      }
      if (m_instance != null)
      {
        m_result.WriteCell ("instance", null, m_instance.ParseLong (m_lexer));
      }
      else
      {
        m_result.WriteCell ("instance", null, (long) -1);
      }
      if (m_event != null)
      {
        m_result.WriteCell ("event", null, m_event.Text);
      }
      else
      {
        m_result.WriteCell ("event", null, "");
      }
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

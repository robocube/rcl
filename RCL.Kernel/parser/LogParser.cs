
using System.Text;

namespace RCL.Kernel
{
  public class LogParser : RCParser
  {
    protected readonly static RCLexer _logLexer;

    static LogParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.LogEntryHeader);
      types.Write (RCTokenType.EndOfLine);
      types.Write (RCTokenType.LogEntryBody);
      types.Write (RCTokenType.LogEntryRawLine);
      _logLexer = new RCLexer (types);
    }

    RCToken _time = null;
    RCToken _bot = null;
    RCToken _fiber = null;
    RCToken _module = null;
    RCToken _instance = null;
    RCToken _event = null;
    string _message = null;
    string _document = null;
    StringBuilder _builder = new StringBuilder ();
    RCCube _result = new RCCube ();

    public LogParser ()
    {
      _lexer = _logLexer;
    }

    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      fragment = false;
      _result = new RCCube (new Timeline ());
      _result.ReserveColumn ("bot");
      _result.ReserveColumn ("fiber");
      _result.ReserveColumn ("module");
      _result.ReserveColumn ("instance");
      _result.ReserveColumn ("event");
      _result.ReserveColumn ("message");
      _result.ReserveColumn ("document");

      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }

      if (_bot != null) {
        if (_builder.Length > 0) {
          _document = _builder.ToString ();
        }
        AppendEntry ();
      }
      return _result;
    }

    public override void AcceptLogEntryHeader (RCToken token)
    {
      if (_bot != null) {
        if (_builder.Length > 0) {
          _document = _builder.ToString ();
        }
        AppendEntry ();
      }

      int current = 0;
      _time = RCTokenType.Time.TryParseToken (token.Text, current, 0, 0, null);
      if (_time != null) {
        current += _time.Text.Length;
        // skip the single space. Do not validate.
        // This requires log files to only use single spaces between header values.
        ++current;
      }

      _bot = RCTokenType.Number.TryParseToken (token.Text, current, 0, 0, null);
      if (_bot != null) {
        current += _bot.Text.Length;
      }
      ++current;

      _fiber = RCTokenType.Number.TryParseToken (token.Text, current, 0, 0, null);
      if (_fiber != null) {
        current += _fiber.Text.Length;
      }
      ++current;

      _module = RCTokenType.Name.TryParseToken (token.Text, current, 0, 0, null);
      if (_module != null) {
        current += _module.Text.Length;
      }
      ++current;

      _instance = RCTokenType.Number.TryParseToken (token.Text, current, 0, 0, null);
      if (_instance != null) {
        current += _instance.Text.Length;
      }
      ++current;

      _event = RCTokenType.Name.TryParseToken (token.Text, current, 0, 0, null);
      if (_event != null) {
        current += _event.Text.Length;
      }
      ++current;

      if (current <= token.Text.Length) {
        _message = token.Text.Substring (current);
      }
      else {
        _message = null;
      }
      _builder.Clear ();
    }

    public override void AcceptEndOfLine (RCToken token) {}

    public override void AcceptLogEntryBody (RCToken token)
    {
      _builder.Append (token.Text.Substring (2));
      // Prevent inconsistent string content on Windows.
      // Only write CRLFs when persisting text.
      _builder.Append ("\n");
    }

    protected readonly static char[] TRIM_CHARS = new char[] { '\r', '\n' };
    public override void AcceptLogEntryRawLine (RCToken token)
    {
      if (_bot != null) {
        if (_builder.Length > 0) {
          _document = _builder.ToString ();
        }
        AppendEntry ();
      }

      _time = null;
      _bot = null;
      _fiber = null;
      _module = null;
      _instance = null;
      _event = null;
      _message = token.Text.TrimEnd (TRIM_CHARS);
      _document = null;
      AppendEntry ();
    }

    protected void AppendEntry ()
    {
      if (_time != null) {
        _result.WriteCell ("time", null, _time.ParseTime (_lexer));
      }
      if (_bot != null) {
        _result.WriteCell ("bot", null, _bot.ParseLong (_lexer));
      }
      else {
        _result.WriteCell ("bot", null, (long) -1);
      }
      if (_fiber != null) {
        _result.WriteCell ("fiber", null, _fiber.ParseLong (_lexer));
      }
      else {
        _result.WriteCell ("fiber", null, (long) -1);
      }
      if (_module != null) {
        _result.WriteCell ("module", null, _module.Text);
      }
      else {
        _result.WriteCell ("module", null, "");
      }
      if (_instance != null) {
        _result.WriteCell ("instance", null, _instance.ParseLong (_lexer));
      }
      else {
        _result.WriteCell ("instance", null, (long) -1);
      }
      if (_event != null) {
        _result.WriteCell ("event", null, _event.Text);
      }
      else {
        _result.WriteCell ("event", null, "");
      }
      if (_message != null) {
        _result.WriteCell ("message", null, _message);
      }
      if (_document != null) {
        _result.WriteCell ("document", null, _document);
      }
      _result.Axis.Write ();

      // Reset everything for the next log entry.
      _time = null;
      _bot = null;
      _fiber = null;
      _module = null;
      _instance = null;
      _event = null;
      _message = null;
      _document = null;
    }
  }
}

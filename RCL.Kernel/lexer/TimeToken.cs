
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class TimeToken : RCTokenType
  {
    public override RCToken TryParseToken (
      string text, int start, int index, RCToken previous)
    {
      //If the previous token is also a number, and there is no whitespace, the dash
      //should be considered an operator and not a sign on the number itself. ex "1-2"
      int length = LengthOfTime (text, start, false);
      if (length < 0) return null;
      string result = text.Substring (start, length);
      return new RCToken (result, this, start, index);
    }

    public static int LengthOfTime (string text, int start, bool allowDash)
    {
      int timestamp = LengthOfTimestamp (text, start);
      if (timestamp > -1)
      {
        return timestamp;
      }
      int datetime = LengthOfDatetime (text, start);
      if (datetime > -1)
      {
        return datetime;
      }
      int timespan = LengthOfTimespan (text, start);
      if (timespan > -1)
      {
        return timespan;
      }
      int date = LengthOfDate (text, start);
      if (date > -1)
      {
        return date;
      }
      int time = LengthOfTime (text, start);
      if (time > -1)
      {
        return time;
      }
      return -1;
    }

    public static int LengthOfDate (string text, int start)
    {
      int current = start;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      return current - start;
    }

    public static int LengthOfTime (string text, int start)
    {
      int current = start;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ':') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      return current - start;
    }

    public static int LengthOfDatetime (string text, int start)
    {
      int current = start;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ' ') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ':') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      return current - start;
    }

    public static int LengthOfTimespan (string text, int start)
    {
      //Console.WriteLine("LengthOfTimespan");
      int current = start;
      if (current < text.Length && text[current] == '-') ++current;
      while (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ':') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ':') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.')
      {
        ++current;
        while (true)
        {
          if (current < text.Length && (text[current] >= '0' && text[current] <= '9'))
          {
            ++current;
          }
          else
          {
            return current - start;
          }
        }
      }
      else return current - start;
    }

    public static int LengthOfTimestamp (string text, int start)
    {
      int current = start;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ' ') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ':') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == ':') ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && (text[current] >= '0' && text[current] <= '9')) ++current; else return -1;
      if (current < text.Length && text[current] == '.')
      {
        ++current;
        while (true)
        {
          if (current < text.Length && (text[current] >= '0' && text[current] <= '9'))
          {
            ++current;
          }
          else
          {
            return current - start;
          }
        }
      }
      else return current - start;
    }

    public override void Accept (RCParser parser, RCToken token)
    {
      parser.AcceptTime (token);
    }

    public override string TypeName
    {
      get { return "time"; }
    }

    public override object Parse (RCLexer lexer, RCToken token)
    {
      return ParseTime (lexer, token);
    }

    public override RCTimeScalar ParseTime (RCLexer lexer, RCToken token)
    {
      return ParseTime (token.Text);
    }

    public static RCTimeScalar ParseTime (string text)
    {
      DateTime result;
      for (int i = 0; i < RCTime.FORMATS.Length; ++i)
      {
        if (DateTime.TryParseExact (text,
                                    RCTime.FORMATS[i],
                                    CultureInfo.InvariantCulture,
                                    //Without this times will implicity have today's date.
                                    DateTimeStyles.NoCurrentDateDefault,
                                    out result))
        {
          if (i <= (int) RCTimeType.Timestamp)
          {
            return new RCTimeScalar (result, (RCTimeType) i);
          }
          else
          {
            return new RCTimeScalar (result, RCTimeType.Timestamp);
          }
        }
      }
      TimeSpan ts;
      if (TimeSpan.TryParse (text, out ts))
      {
        return new RCTimeScalar (ts);
      }
      throw new Exception ("Unable to parse time " + text);
    }
  }
}

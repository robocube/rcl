
using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCName
  {
    protected static object _lock = new object ();
    protected static Dictionary<string, RCName> _names = new Dictionary<string, RCName> ();
    protected static RCArray<RCName> _index = new RCArray<RCName> ();

    static RCName ()
    {
      RCName empty = new RCName ("", 0, false);
      _names.Add ("", empty);
      _index.Write (empty);
      RCName trueLiteral = new RCName ("'true'", 1, true);
      _names.Add ("true", trueLiteral);
      _index.Write (trueLiteral);
      RCName falseLiteral = new RCName ("'false'", 2, true);
      _names.Add ("false", falseLiteral);
      _index.Write (falseLiteral);
    }

    public static RCArray<string> MultipartName (string text, char delimeter)
    {
      int partStart = 0;
      RCArray<string> result = new RCArray<string> (4);
      for (int i = 0; i < text.Length; ++i)
      {
        if (i == text.Length - 1)
        {
          string partString = text.Substring (partStart);
          RCName part = GetName (partString);
          partStart += partString.Length;
          // Consume the delimeter
          ++partStart;
          result.Write (part.Text);
        }
        else if (text[i] == delimeter)
        {
          string partString = text.Substring (partStart, i - partStart);
          RCName part = GetName (partString);
          partStart += partString.Length;
          // Consume the delimeter
          ++partStart;
          result.Write (part.Text);
        }
        else if (text[i] == '\'')
        {
          int matchingQuote = text.IndexOf ('\'', i + 1);
          if (matchingQuote < 0)
          {
            throw new Exception ("Unmatched single quote in name: " + text);
          }
          else
          {
            while (matchingQuote > 0 && text[matchingQuote - 1] == '\\')
            {
              matchingQuote = text.IndexOf ('\'', matchingQuote + 1);
            }
            if (matchingQuote <= 0 || text[matchingQuote] != '\'')
            {
              throw new Exception ("Unmatched single quote among escaped single quotes in name: " +
                                   text);
            }
            string partString = text.Substring (partStart, 1 + (matchingQuote - partStart));
            RCName part = GetName (partString);
            partStart += partString.Length;
            result.Write (part.Text);
            i = matchingQuote;
          }
        }
      }
      return result;
    }

    public static RCName GetName (string text)
    {
      if (text == null)
      {
        text = "";
      }
      string name = null;
      bool escaped = false;
      RCName result;
      lock (_lock)
      {
        if (!_names.TryGetValue (text, out result))
        {
          if (text[0] == '\'')
          {
            if (text.Length == 1 || text[text.Length - 1] != '\'')
            {
              throw new Exception ("Unmatched single quote in name: " + text);
            }
            // Remove quotes if not necessary
            // They are necessary when the name begins with a number
            if (text.Length > 1 && text[1] >= '0' && text[1] <= '9')
            {
              name = text;
              escaped = true;
            }
            for (int i = 1; i < text.Length - 1; ++i)
            {
              if (!RCTokenType.IsIdentifierChar (text[i]))
              {
                name = text;
                escaped = true;
                break;
              }
            }
            if (name == null)
            {
              name = text.Substring (1, text.Length - 2);
            }
          }
          else if (text[0] >= '0' && text[0] <= '9')
          {
            name = "'" + text + "'";
            escaped = true;
          }
          else
          {
            for (int i = 0; i < text.Length; ++i)
            {
              // add quotes if necessary
              if (!RCTokenType.IsIdentifierChar (text[i]))
              {
                name = "'" + text + "'";
                escaped = true;
                break;
              }
            }
            if (name == null)
            {
              name = text;
            }
          }
          if (_names.TryGetValue (name, out result))
          {
            // this makes it a synonym for next time
            _names.Add (text, result);
            return result;
          }
          else
          {
            result = new RCName (name, _names.Count, escaped);
            _names.Add (result.Text, result);
            _index.Write (result);
            return result;
          }
        }
        else
        {
          return result;
        }
      }
    }

    public static string Get (string name)
    {
      return GetName (name).Text;
    }

    public static long Num (string name)
    {
      return GetName (name).Index;
    }

    public static string RawName (string name)
    {
      if (name == null || name.Length == 0)
      {
        return "";
      }
      else if (name[0] == '\'')
      {
        return name.Substring (1, name.Length - 2);
      }
      else
      {
        return name;
      }
    }

    public readonly string Text;
    public readonly long Index;
    public readonly bool Escaped;
    public RCName (string text, long index, bool escaped)
    {
      Text = text;
      Index = index;
      Escaped = escaped;
    }
  }
}

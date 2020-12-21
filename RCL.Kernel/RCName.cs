
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
      //Timeline literal names
      RCName GLiteral = new RCName ("'G'", 3, true, isMSL:true);
      _names.Add ("'G'", GLiteral);
      _index.Write (GLiteral);
      RCName ELiteral = new RCName ("'E'", 4, true, isMSL:true);
      _names.Add ("'E'", ELiteral);
      _index.Write (ELiteral);
      RCName TLiteral = new RCName ("'T'", 5, true, isMSL:true);
      _names.Add ("'T'", TLiteral);
      _index.Write (TLiteral);
      RCName SLiteral = new RCName ("'S'", 6, true, isMSL:true);
      _names.Add ("'S'", SLiteral);
      _index.Write (SLiteral);
      RCName GLiteralRaw = new RCName ("G", 7, false, isMSL:true);
      _names.Add ("G", GLiteralRaw);
      _index.Write (GLiteralRaw);
      RCName ELiteralRaw = new RCName ("E", 8, false, isMSL:true);
      _names.Add ("E", ELiteralRaw);
      _index.Write (ELiteralRaw);
      RCName TLiteralRaw = new RCName ("T", 9, false, isMSL:true);
      _names.Add ("T", TLiteralRaw);
      _index.Write (TLiteralRaw);
      RCName SLiteralRaw = new RCName ("S", 10, false, isMSL:true);
      _names.Add ("S", SLiteralRaw);
      _index.Write (SLiteralRaw);
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

    public static RCName GetName (string text, bool escapeMSL = false)
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
            if (text.Length > 1 && text[1] >= '0' && text[1] <= '9') {
              name = text;
              escaped = true;
            }
            else if (text.Length == 3 && RCTokenType.IsMagicSingleLetter (text[1])) {
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
          else if (text.Length == 1 && RCTokenType.IsMagicSingleLetter (text[0])) {
            if (escapeMSL) {
              name = "'" + text + "'";
              escaped = true;
            }
            else {
              name = text;
              escaped = false;
            }
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
        else if (result.IsMagicSingleLetter && !result.Escaped && escapeMSL) {
          if (result.Text == "G") {
            return _names["'G'"];
          }
          else if (result.Text == "E") {
            return _names["'E'"];
          }
          else if (result.Text == "T") {
            return _names["'T'"];
          }
          if (result.Text == "S") {
            return _names["'S'"];
          }
          return result;
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
    public readonly bool IsMagicSingleLetter;

    public RCName (string text, long index, bool escaped, bool isMSL = false)
    {
      Text = text;
      Index = index;
      Escaped = escaped;
      IsMagicSingleLetter = isMSL;
    }
  }
}

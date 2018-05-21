
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public abstract class RCTokenType
  {
    internal static readonly char[] WhiteChars = new char[] { ' ', '\t', '\r', '\n', '\a' };
    internal static readonly char[] NumTypes = new char[] { 'd', 'l', 'i', 'f', 'm' };
    internal static readonly char[] TypeChars = new char[] { 'd', 'l', 'm', 'y', 'i', 'f', 's', 'c', 't', 'z', 'n', 'b', 'x' };
    internal static string[] SpecialOperators = { "-", "+", "*", "/", "%", "&", "<=", ">=", "<", ">", "==", "!=", "=", "!" };
    //The order of tests matters here.
    internal static string[] Evaluators = { "::", ":", "<-:", "<--", "<-" };
    
    /// <summary>
    /// Would life be better if i let you use shorthand like tttffftttfff
    /// </summary>
    internal static string[] Booleans = { "true", "false" };

    /// <summary>
    /// The name of an operator or a variable.
    /// </summary>
    public static readonly RCTokenType Name = new NameToken ();

    /// <summary>
    /// Operators control evaluation
    /// </summary>
    public static readonly RCTokenType Evaluator = new EvaluatorToken ();

    /// <summary>
    /// References point to variables in the current or parent block.
    /// </summary>
    public static readonly RCTokenType Reference = new ReferenceToken ();

    /// <summary>
    /// Parens are used to reorder operator evaluation and (possibly) define lists
    /// </summary>
    public static readonly RCTokenType Paren = new ParenToken ();

    /// <summary>
    /// Used to denote and nest code blocks
    /// </summary>
    public static readonly RCTokenType Block = new BlockToken ();

    public static readonly RCTokenType Content = new ContentToken ();

    /// <summary>
    /// Cubes store multidimensional time series data.
    /// </summary>
    public static readonly RCTokenType Cube = new CubeToken ();

    /// <summary>
    /// Symbols are used in the RC tuple space implementation.
    /// </summary>
    public static readonly RCTokenType Symbol = new SymbolToken ();

    /// <summary>
    /// Special incr and decr indicators.
    /// </summary>
    public static readonly RCTokenType Incr = new IncrToken ();

    /// <summary>
    /// Spaces, Tabs, Newlines, etc...
    /// </summary>
    public static readonly RCTokenType WhiteSpace = new WhitespaceToken ();
    
    /// <summary>
    /// C-style comments - I would like to add some introspection on comments
    /// so I am making them a separate type from whitespace
    /// </summary>
    //public static readonly RCTokenType Comment = new RCCommentToken();

    /// <summary>
    /// Spacers are used to format tables
    /// </summary>
    public static readonly RCTokenType Spacer = new SpacerToken ();

    /// <summary>
    /// Used to represent vectors whose count is zero. ~d, ~s etc...
    /// </summary>
    public static readonly RCTokenType EmptyVector = new EmptyVectorToken ();

    /// <summary>
    /// C-style string literals
    /// </summary>
    public static readonly RCTokenType String = new StringToken ();

    /// <summary>
    /// Doubles, Longs, Floats, Ints
    /// </summary>
    public static readonly RCTokenType Number = new NumberToken ();

    /// <summary>
    /// C-style true or false
    /// </summary>
    public static readonly RCTokenType Boolean = new BooleanToken ();

    /// <summary>
    /// Generic literal tokens may contain byte,char and other literal types.
    /// </summary>
    public static readonly RCTokenType Literal = new LiteralToken ();

    /// <summary>
    /// RC's caught exceptions appear as errors
    /// </summary>
    //public static readonly RCTokenType Error = new RCErrorToken();

    public static readonly RCTokenType Time = new TimeToken ();
    /// <summary>
    /// The lexer should be able to allow or deny junk in the output.
    /// Junk is a token that is not valid but may need to be preserved
    /// for analysis or reformatting, manual editing, etc.
    /// You can not have the editor freezing up because of bad syntax.
    /// </summary>
    public static readonly RCTokenType Junk = new JunkToken ();

    //For CSV Files only.
    public static readonly RCTokenType CSVSeparator = new SeparatorToken ();

    public static readonly RCTokenType CSVContent = new ColumnDataToken (',', '\r', '\n');

    //For JSON Files only.
    public static readonly RCTokenType Null = new NullToken ();

    //For XML Files only.
    public static readonly RCTokenType XmlBracket = new XMLBracketToken ();
    
    public static readonly RCTokenType XmlContent = new XMLContentToken ();

    //For RCL log files only.
    public static readonly RCTokenType EndOfLine = new EndOfLine ();
    public static readonly RCTokenType LogEntryRawLine = new LogEntryRawLine ();
    public static readonly RCTokenType LogEntryHeader = new LogEntryHeader ();
    public static readonly RCTokenType LogEntryBody = new LogEntryBody ();

    ///For markdown parsing only.
    public static readonly RCTokenType MarkdownContentToken = new MarkdownContentToken ();
    public static readonly RCTokenType MarkdownBeginItalicToken = new MarkdownBeginItalicToken ();
    public static readonly RCTokenType MarkdownEndItalicToken = new MarkdownEndItalicToken ();
    public static readonly RCTokenType MarkdownBeginBoldToken = new MarkdownBeginBoldToken ();
    public static readonly RCTokenType MarkdownEndBoldToken = new MarkdownEndBoldToken ();
    public static readonly RCTokenType MarkdownLinkToken = new MarkdownLinkToken ();
    public static readonly RCTokenType MarkdownLiteralLinkToken = new MarkdownLiteralLinkToken ();
    public static readonly RCTokenType MarkdownHeaderToken = new MarkdownHeaderToken ();
    public static readonly RCTokenType MarkdownBlockquoteToken = new MarkdownBlockquoteToken ();
    public static readonly RCTokenType MarkdownULItemToken = new MarkdownULItemToken ();
    public static readonly RCTokenType MarkdownOLItemToken = new MarkdownOLItemToken ();
    public static readonly RCTokenType MarkdownLIIndentToken = new MarkdownLIIndentToken ();

    /// <summary>
    /// Is this the right spot for a visitor? not really sure...
    /// </summary>
    public abstract void Accept (RCParser parser, RCToken token);
    
    public abstract RCToken TryParseToken (
      string code, int startPos, int index, RCToken previous);

    public virtual bool IsTerminalOf (RCToken token)
    {
      return false;
    }

    public static bool IsIn (char character, char[] allowed)
    {
      for (int i = 0; i < allowed.Length; ++i)
        if (character == allowed[i]) return true;
      return false;
    }
    
    public static bool IsIdentifierChar (char character)
    {
      if ((character >= 'a' && character <= 'z') ||
          (character >= 'A' && character <= 'Z') ||
          (character >= '0' && character <= '9') ||
          (character == '_'))
          return true;
      else return false;
    }

    public static int LengthOfDelimitedName (string text, int start, char delimiter)
    {
      //Check for one of the special ops like + - * etc...
      for (int i = 0; i < SpecialOperators.Length; ++i)
      {
        int length = LengthOfKeyword (text, start, SpecialOperators[i]);
        if (length > 0) return length;
      }
      int end = start;
      while (true)
      {
        int length = LengthOfIdentifier (text, end);
        if (length > 0)
        {
          end += length;
        }
        if ((end < text.Length - 1) && text[end] == delimiter)
        {
          ++end;
        }
        else break;
      }
      return end > start ? end - start : -1;
    }

    public static int LengthOfIdentifier (string text, int start)
    {
      int length, end = -1;
      //Check for an escaped string
      length = LengthOfEnclosedLiteral (text, start, '\'');
      if (length > 0)
      {
        return length;
      }
      //Check for a regular old vanilla name.
      end = start;
      while (end < text.Length && IsIdentifierChar (text[end]))
      {
        end++;
      }
      return end > start ? end - start : -1;
    }

    public static int LengthOfStrictIdentifier (string text, int start)
    {
      //Check for a regular old vanilla name.
      int end = start;
      if (end < text.Length && text[end] >= '0' && text[end] <= '9')
      {
        return -1;
      }
      while (end < text.Length && IsIdentifierChar (text[end]))
      {
        end++;
      }
      return end > start ? end - start : -1;
    }

    public static int LengthOfDelimitedValue (string text, int start, char delimiter)
    {
      int end = start;
      while (end < text.Length)
      {
        if (IsIdentifierChar (text[end]))
        {
          ++end;
        }
        else if (text[end] == delimiter)
        {
          ++end;
        }
        else if (text[end] == '-' && (end == start || text[end-1] == delimiter))
        {
          ++end;
        }
        else if (text[end] == '\'') 
        {
          ++end;
          while (end < text.Length && text[end] != '\'') 
          {
            ++end;
          }
          ++end;
          if (end < text.Length && IsIn (text[end], WhiteChars))
          {
            break;
          }
        }
        else if (text[end] == '\\')
        {
          ++end;
        }
        else if (text[end] == '*' || text[end] == '.')
        {
          ++end;
        }
        else break;
      }
      return end - start;
    }

    public static int LengthOfEnclosedLiteral (string text, int start, char delimeter)
    {
      if (start >= text.Length)
      {
        return -1;
      }
      if (text[start] != delimeter)
      {
        return -1;
      }
      int current = start + 1;
      while (current < text.Length)
      {
        if (text[current] == delimeter)
        {
          if (text[current - 1] != '\\' ||
              text[current - 1] == '\\' && text[current-2] == '\\')
          {
            break;
          }
        }
        ++current;
      }
      while (current < text.Length && text[current] != delimeter)
      {
        ++current;
      }
      int result = current >= text.Length ? -1 : current - start + 1;
      //Console.WriteLine ("result: {0}", result);
      return result;
    }

    public static int LengthOfKeyword (string text, int start, string word)
    {
      int i = 0;
      while ((start + i) < text.Length && word[i] == text[start + i])
      {
        if (i == word.Length - 1)
        {
          return word.Length;
        }
        ++i;
      }
      return -1;
    }

    /// <summary>
    /// Escape control characters for string literals.
    /// Note this only does the most popular control chars, not all of them.
    /// The idea is for string literals to work just like C.
    /// Another note: use of a StringBuilder for each and every string literal
    /// that is parsed and produced is going to become costly.  I may need to
    /// flag tokens as needing escape work as the lexer is passing over them.
    /// </summary>
    public static string EscapeControlChars (string str, char delimeter)
    {
      StringBuilder builder = new StringBuilder ();
      for (int i = 0; i < str.Length; ++i)
      {
        char current = str[i];
        if (current == '\n') builder.Append ("\\n");
        else if (current == '\r') builder.Append ("\\r");
        else if (current == '\t') builder.Append ("\\t");
        else if (current == '\a') builder.Append ("\\a");
        else if (current == '\\') builder.Append ("\\\\");
        else if (current == delimeter)
        {
          builder.Append("\\");
          builder.Append (delimeter);
        }
        else builder.Append (current);
      }
      return builder.ToString ();
    }

    /// <summary>
    /// Reverse the escaping done above.
    /// </summary>
    public static string UnescapeControlChars (string str, char delimiter)
    {
      StringBuilder builder = new StringBuilder();
      for (int i = 0; i < str.Length; ++i)
      {
        // This code block should be used instead of the one above, 
        // but it needs a test to be written first.
        if (str[i] == '\\')
        {
          if (i < str.Length - 1)
          {
            char next = str[i + 1];
            if (next == '\\') builder.Append('\\');
            else if (next == 'n') builder.Append('\n');
            else if (next == 'r') builder.Append('\r');
            else if (next == 't') builder.Append('\t');
            else if (next == 'a') builder.Append('\a');
            else if (next == 'u')
            {
              builder.Append(
                (char) ushort.Parse (str.Substring (i+2, 4), NumberStyles.AllowHexSpecifier));
              i+=4;
            }
            else if (next == '/') builder.Append ('/');
            else if (next == delimiter) builder.Append(delimiter);
            else throw new Exception(
              "unescaped backslash at position " + i + " within \"" + str + "\"");
            //skip over the additional character.
            ++i;
          }
        }
        else builder.Append(str[i]);
      }
      return builder.ToString();
    }
    
    public virtual bool IsEnclosedLiteral { get { return false; } }

    public abstract string TypeName { get; }

    public virtual object Parse (RCLexer lexer, RCToken token)
    {
      throw new Exception ("cannot use " + Name + " as a symbol or within a cube");
    }

    public virtual string ParseString (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "string"));
    }

    public virtual double ParseDouble (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "double"));
    }

    public virtual long ParseLong (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "long"));
    }

    public virtual decimal ParseDecimal (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "decimal"));
    }

    public virtual float ParseFloat (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "double"));
    }

    public virtual int ParseInt (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "int"));
    }

    public virtual bool ParseBoolean (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "bool"));
    }

    public virtual RCSymbolScalar ParseSymbol (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "symbol"));
    }

    public virtual RCBlock ParseLink (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "link"));
    }

    public virtual RCIncrScalar ParseIncr (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "incr"));
    }

    public virtual byte ParseByte (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "byte"));
    }

    public virtual RCTimeScalar ParseTime (RCLexer lexer, RCToken token)
    {
      throw new Exception (Message (token.Type.ToString (), token.Text, "time"));
    }

    protected string Message (string tokenType, string tokenText, string typeName)
    {
      return "Cannot convert (" + tokenType + ") " + tokenText + " to " + typeName;
    }
  }
}

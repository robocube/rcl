
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{

  /// <summary>
  /// Represents a single token within an RCL source document.
  /// </summary>
  public class RCToken
  {
    /// <summary>
    /// The text of this token.
    /// </summary>
    public readonly string Text;

    /// <summary>
    /// The recognizer which instantiated this token.
    /// </summary>
    public readonly RCTokenType Type;

    /// <summary>
    /// The index of the first character in the source document.
    /// </summary>
    public readonly int Start;

    /// <summary>
    /// The index of this token in the source document.
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// The line number in the source document where this token begins.
    /// </summary>
    public readonly int Line;

    /// <summary>
    /// The number of newline characters found within this token.
    /// </summary>
    public readonly int Lines;

    public RCToken (string text, RCTokenType type, int start, int index, int line, int lines)
    {
      Text = text;
      Type = type;
      Start = start;
      Index = index;
      Line = line;
      Lines = lines;
    }

    public string ParseString (RCLexer lexer)
    {
      return Type.ParseString (lexer, this);
    }

    public virtual double ParseDouble (RCLexer lexer)
    {
      return Type.ParseDouble (lexer, this);
    }

    public virtual long ParseLong (RCLexer lexer)
    {
      return Type.ParseLong (lexer, this);
    }

    public virtual float ParseFloat (RCLexer lexer)
    {
      return Type.ParseFloat (lexer, this);
    }

    public virtual decimal ParseDecimal (RCLexer lexer)
    {
      return Type.ParseDecimal (lexer, this);
    }

    public virtual int ParseInt (RCLexer lexer)
    {
      return Type.ParseInt (lexer, this);
    }

    public virtual bool ParseBoolean (RCLexer lexer)
    {
      return Type.ParseBoolean (lexer, this);
    }

    public virtual RCSymbolScalar ParseSymbol (RCLexer lexer)
    {
      return Type.ParseSymbol (lexer, this);
    }

    public virtual RCIncrScalar ParseIncr (RCLexer lexer)
    {
      return Type.ParseIncr (lexer, this);
    }

    public virtual byte ParseByte (RCLexer lexer)
    {
      return Type.ParseByte (lexer, this);
    }

    public virtual RCTimeScalar ParseTime (RCLexer lexer)
    {
      return Type.ParseTime (lexer, this);
    }

    public virtual object Parse (RCLexer lexer)
    {
      return Type.Parse (lexer, this);
    }

    public override string ToString ()
    {
      //This is for easier debugging.
      return Text + " - " + Type.TypeName;
    }
  }
}

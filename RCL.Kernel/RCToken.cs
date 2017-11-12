
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class RCToken
  {
    public readonly string Text;
    public readonly RCTokenType Type;
    public readonly long Start;
    public readonly long Index;

    public RCToken (string text, RCTokenType type, long start, long index)
    {
      Text = text;
      Type = type;
      Start = start;
      Index = index;
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

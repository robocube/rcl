
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace RCL.Kernel
{
  public class RCLexer
  {
    protected readonly RCArray<RCTokenType> m_types = new RCArray<RCTokenType> ();
    public RCLexer (RCArray<RCTokenType> types)
    {
      types.Lock ();
      m_types = types;
    }

    public void Lex (string input, RCArray<RCToken> output)
    {
      int i = 0;
      int tokenIndex = 0;
      //The only scenario requiring a lookback is for disambiguating
      //negative numbers from a minus (-) operator.
      RCToken previous = null;
      while (i < input.Length)
      {
        RCToken token = null;
        foreach (RCTokenType tokenType in m_types)
        {
          token = tokenType.TryParseToken (input, i, tokenIndex, previous);
          if (null != token)
          {
            output.Write (token);
            previous = token;
            tokenIndex++;
            i += token.Text.Length;
            break;
          }
        }
        if (token == null)
        {
          throw new Exception (
            string.Format ("unable to lex: '{0}', i={1}", input, i));
        }
      }
    }

    /// <summary>
    /// Convert a single token from a string to an RCToken.
    /// </summary>
    public RCToken LexSingle (string code)
    {
      foreach (RCTokenType tokenType in m_types)
      {
        RCToken token = tokenType.TryParseToken (code, 0, 0, null);
        if (token != null && token.Text.Length == code.Length)
        {
          return token;
        }
      }
      return new RCToken (code, RCTokenType.Junk, 0, code.Length);
    }
  }
}

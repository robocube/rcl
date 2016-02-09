
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  //This implementation will be a problem eventually.
  //We will need to intern symbols for one thing.
  //Random access into the parts is going to be slow.
  //Might even want to create a monolithic symbol cache.
  //Or one shared by bot.  Lots of cool ideas.
  //Whatever we will fix it later.
  public class RCSymbolScalar : IComparable, IComparable<RCSymbolScalar>
  {
    public static readonly RCSymbolScalar Empty = new RCSymbolScalar ();
    public readonly RCSymbolScalar Previous;
    public readonly RCVectorBase Type;
    public readonly object Key;
    public readonly long Length;
    protected readonly string m_string;

    protected RCSymbolScalar ()
    {
      m_string = "#";
      Key = "";
    }

    public static RCSymbolScalar From (params object[] key) 
    {
      RCSymbolScalar prev = null;
      for (int i = 0; i < key.Length; ++i) 
      {
        prev = new RCSymbolScalar (prev, key[i]);
      }
      return prev;
    }

    public static RCSymbolScalar From (RCLexer lexer, string part)
    {
      //if (part.Length > 0 && part[0] == '#')
      //{
      //  part = part.Substring (1);
      //}
      RCToken token = lexer.LexSingle (part);
      if (token.Text.Length > 0 && token.Type == RCTokenType.Junk)
      {
        return new RCSymbolScalar (RCSymbolScalar.Empty, 
                                   string.Format ("'{0}'", token.Text));
      }
      else
      {
        object val = token.Parse (lexer);
        RCSymbolScalar result = val as RCSymbolScalar;
        if (result == null)
        {
          return new RCSymbolScalar (RCSymbolScalar.Empty, val);
        }
        else
        {
          return result;
        }
      }
    }

    public RCSymbolScalar (RCSymbolScalar previous, object key)
    {
      if (key == null)
      {
        throw new ArgumentNullException ("key");
      }
      if (key.GetType () == typeof (RCSymbolScalar))
      {
        throw new Exception ("Symbols may not contain other symbols.");
      }
      if (previous == null)
      {
        Previous = RCSymbolScalar.Empty;
      }
      else
      {
        Previous = previous;
      }
      Key = key;
      Type = RCVectorBase.EmptyOf (Key.GetType ());
      if (Previous == RCSymbolScalar.Empty)
      {
        Length = 1;
        string part = Type.Shorthand (Key);
        string prefix = part.Length > 0 && part[0] == '#' ? "" : "#";
        m_string = prefix + part + Type.Suffix;
      }
      else
      {
        Length = previous.Length + 1;
        m_string = previous.ToString () + "," + 
          Type.Shorthand (Key) + Type.Suffix;
      }
    }

    public object[] ToArray ()
    {
      object[] result = new object[Length];
      RCSymbolScalar current = this;
      for(int i = 0; i < result.Length; ++i)
      {
        result[i] = current.Key;
        current = current.Previous;
      }
      Array.Reverse (result);
      return result;
    }

    public object Part (long p)
    {
      long i = Length - 1;
      RCSymbolScalar current = this;
      while (i > p)
      {
        --i;
        current = current.Previous;
      }
      return current.Key;
    }

    public bool IsConcreteOf (RCSymbolScalar scalar)
    {
      return m_string.Length > scalar.m_string.Length && m_string.StartsWith (scalar.m_string);
    }

    public override string ToString ()
    {
      return m_string;
    }

    public void ToByte (RCArray<byte> result)
    {
      Binary.WriteScalarSymbol (result, this);
    }

    /// The "Modified Bernstein Hash"
    /// http://www.eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx
    /// 
    /// Dan Bernstein created this algorithm and posted it in a newsgroup. 
    /// It is known by many as the Chris Torek hash because Chris went a long way toward
    /// popularizing it. Since then it has been used successfully by many, but despite 
    /// that the algorithm itself is not very sound when it comes to avalanche and 
    /// permutation of the internal state. It has proven very good for small 
    /// character keys, where it can outperform algorithms that result in a more random 
    /// distribution.
    /// 
    /// Bernstein's hash should be used with caution. It performs very 
    /// well in practice, for no apparently known reasons (much like how the constant 
    /// 33 does better than more logical constants for no apparent reason), but in theory 
    /// it is not up to snuff. Always test this function with sample data for every 
    /// application to ensure that it does not encounter a degenerate case and cause 
    /// excessive collisions.
    /// 
    /// A minor update to Bernstein's hash replaces addition with XOR for the combining 
    /// step. This change does not appear to be well known or often used, the original 
    /// algorithm is still recommended by nearly everyone, but the new algorithm typically
    /// results in a better distribution.
    /// 
    /// Brian here.  I don't know much about this stuff, I hope it works, someday we
    /// should test it.  Would love to talk to anyone who knows how to do better.
    /// In the case of RC I think that series with more than one symbol column are going 
    /// to be the exception rather than the rule, but I want to make sure that they are 
    /// accomodated in the design from day one.  If they have trouble with uneven
    /// distribution of hashes we can worry about it later.
    //
    //  Older wiser Brian here.  Why still do this silly hashcode thing?
    //  I bit the bullet and gave every symbol a full string a while ago.
    //  Multiple symbol columns are no longer an issue because I added compound symbols (tuples).
    public override int GetHashCode()
    {
      //int h = 0;
      //for (int i = 0; i < Keys.Length; i++)
      //  h = 33 * h ^ Keys[i].GetHashCode();
      //return h;

      /*{
       *  @{type="RCSymbol"} symbol = $this
       *  @{type="int"} h = 0i
       *  :while
       *  {
       *    h = 33i * $h ^ $this.Key.GetHashCode {}
       *    symbol = $symbol.Previous
       *  }
       *  <== $h
       *}
       */

      /* New school - this should translate to the c# block above.
      {<-{<-h:33i * $h ^ GetHashCode $R} over block $this}
      */

      RCSymbolScalar symbol = this;
      int h = 0;
      while (symbol != null)
      {
        h = 33 * h ^ Key.GetHashCode();
        symbol = symbol.Previous;
      }
      return h;
    }
      
    public override bool Equals (object obj)
    {
      RCSymbolScalar other = obj as RCSymbolScalar;
      if (other == null) return false;
      RCSymbolScalar current = this;
      while (current != null)
      {
        if (other == null) return false;
        if (!current.Key.Equals (other.Key))
          return false;
        other = other.Previous;
        current = current.Previous;
      }
      if (other != null) return false;
      return true;
    }

    public int CompareTo (object obj)
    {
      if (obj == null) return 1;
      RCSymbolScalar other = obj as RCSymbolScalar;
      if (other == null) return 1;
      return m_string.CompareTo (other.m_string);
    }

    public int CompareTo (RCSymbolScalar other)
    {
      if (other == null) return 1;
      return m_string.CompareTo (other.m_string);
    }
  }
}
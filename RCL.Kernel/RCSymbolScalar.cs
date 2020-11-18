
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  // This implementation will be a problem eventually.
  // We will need to intern symbols for one thing.
  // Random access into the parts is going to be slow.
  // Might even want to create a monolithic symbol cache.
  // Or one shared by bot.  Lots of cool ideas.
  // Whatever we will fix it later.
  public class RCSymbolScalar : IComparable, IComparable<RCSymbolScalar>
  {
    public static readonly RCSymbolScalar Empty = new RCSymbolScalar ();
    public readonly RCSymbolScalar Previous;
    public readonly RCVectorBase Type;
    public readonly object Key;
    public readonly long Length;
    protected readonly string _string;
    protected readonly bool _leadingStar = false;

    protected RCSymbolScalar ()
    {
      _string = "#";
      Key = "";
    }

    public static RCSymbolScalar From (params object[] key)
    {
      return From (0, key);
    }

    protected static RCSymbolScalar From (int startIndex, params object[] key)
    {
      RCSymbolScalar prev = null;
      for (int i = startIndex; i < key.Length; ++i)
      {
        RCSymbolScalar symKey = key[i] as RCSymbolScalar;
        if (symKey != null) {
          for (int j = 0; j < symKey.Length; ++j)
          {
            prev = new RCSymbolScalar (prev, symKey.Part (j));
          }
        }
        else {
          prev = new RCSymbolScalar (prev, key[i]);
        }
      }
      if (prev == null) {
        prev = RCSymbolScalar.Empty;
      }
      return prev;
    }

    public static RCSymbolScalar From (int startIndex, RCSymbolScalar prefix, params object[] key)
    {
      RCSymbolScalar prev = prefix;
      for (int i = startIndex; i < key.Length; ++i)
      {
        prev = new RCSymbolScalar (prev, key[i]);
      }
      return prev;
    }

    public static RCSymbolScalar From (RCLexer lexer, string part)
    {
      RCToken token = lexer.LexSingle (part);
      if (token.Text.Length > 0 && token.Type == RCTokenType.Junk) {
        return new RCSymbolScalar (RCSymbolScalar.Empty,
                                   string.Format ("'{0}'", token.Text));
      }
      else {
        object val = token.Parse (lexer);
        RCSymbolScalar result = val as RCSymbolScalar;
        if (result == null) {
          return new RCSymbolScalar (RCSymbolScalar.Empty, val);
        }
        else {
          return result;
        }
      }
    }

    public RCSymbolScalar (RCSymbolScalar previous, object key)
    {
      if (key == null) {
        throw new ArgumentNullException ("key");
      }
      if (key.GetType () == typeof (RCSymbolScalar)) {
        throw new Exception ("Symbols may not contain other symbols.");
      }
      if (previous == null) {
        Previous = RCSymbolScalar.Empty;
      }
      else {
        Previous = previous;
      }
      if (key.GetType () == typeof (string)) {
        string str = (string) key;
        if (str.Length > 0) {
          if (str[0] == '\'') {
            if (str[str.Length - 1] == '\'') {
              string keyString = str.Substring (1, str.Length - 2);
              if (keyString[0] >= '0' && keyString[0] <= '9') {
                bool makeNumber = true;
                for (int i = 0; i < keyString.Length; ++i)
                {
                  makeNumber = keyString[i] >= '0' && keyString[i] <= '9';
                  if (!makeNumber) break;
                }
                if (makeNumber) {
                  key = (long) long.Parse (keyString);
                }
                else {
                  key = keyString;
                }
              }
              else key = keyString;
            }
            else {
              throw new Exception ("Invalid symbol part: " + str);
            }
          }
        }
      }
      if (key.GetType () == typeof (int)) {
        key = (long) (int) key;
      }
      Key = key;
      Type = RCVectorBase.EmptyOf (Key.GetType ());
      if (Previous == RCSymbolScalar.Empty) {
        Length = 1;
        string part = Type.IdShorthand (Key);
        string prefix = part.Length > 0 && part[0] == '#' ? "" : "#";
        _string = prefix + part + Type.Suffix;
      }
      else {
        Length = previous.Length + 1;
        _string = previous.ToString () + "," +
                   Type.IdShorthand (Key) + Type.Suffix;
      }
      if (Previous.Key.Equals ("*")) {
        _leadingStar = true;
      }
    }

    public object[] ToArray ()
    {
      object[] result = new object[Length];
      RCSymbolScalar current = this;
      for (int i = 0; i < result.Length; ++i)
      {
        result[i] = current.Key;
        current = current.Previous;
      }
      Array.Reverse (result);
      return result;
    }

    public object Part (long p)
    {
      if (p < 0) {
        long i = -1;
        RCSymbolScalar current = this;
        while (i > p)
        {
          --i;
          current = current.Previous;
        }
        if (current == null) {
          throw new Exception (string.Format ("No part {0} in symbol {1}", p, this));
        }
        return current.Key;
      }
      else {
        long i = Length - 1;
        RCSymbolScalar current = this;
        while (i > p)
        {
          --i;
          current = current.Previous;
        }
        if (current == null) {
          throw new Exception (string.Format ("No part {0} in symbol {1}", p, this));
        }
        return current.Key;
      }
    }

    public RCSymbolScalar Part (long[] p)
    {
      object[] parts = new object[p.Length];
      for (int i = 0; i < p.Length; ++i)
      {
        parts[i] = Part (p[i]);
      }
      return From (parts);
    }

    public RCSymbolScalar PartsAfter (long p)
    {
      object[] parts = new object[(Length - p) - 1];
      for (long i = p + 1; i < Length; ++i)
      {
        parts[i - (p + 1)] = Part (i);
      }
      return From (parts);
    }

    public RCSymbolScalar PartsUntil (long p)
    {
      RCSymbolScalar result = this;
      long currentPart = Length - 1;
      while (currentPart > p)
      {
        --currentPart;
        result = result.Previous;
      }
      return result;
    }

    public bool IsConcreteOf (RCSymbolScalar scalar)
    {
      RCSymbolScalar concrete = this;
      RCSymbolScalar @abstract = scalar;
      if (scalar.Equals (RCSymbolScalar.Empty)) {
        return false;
      }
      while (concrete.Length > @abstract.Length)
      {
        concrete = concrete.Previous;
      }
      while (@abstract != null)
      {
        if (!@abstract.Key.Equals ("*")) {
          if (!@abstract.Key.Equals (concrete.Key)) {
            return false;
          }
        }
        concrete = concrete.Previous;
        @abstract = @abstract.Previous;
      }
      return true;
    }

    public override string ToString ()
    {
      return _string;
    }

    public string ToCsvString ()
    {
      return _string.Substring (1);
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
    //  Multiple symbol columns are no longer an issue because I added compound symbols
    // (tuples).
    public override int GetHashCode ()
    {
      // int h = 0;
      // for (int i = 0; i < Keys.Length; i++)
      //  h = 33 * h ^ Keys[i].GetHashCode();
      // return h;

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
        h = 33 * h ^ Key.GetHashCode ();
        symbol = symbol.Previous;
      }
      return h;
    }

    public override bool Equals (object obj)
    {
      RCSymbolScalar other = obj as RCSymbolScalar;
      if (other == null) {
        return false;
      }
      RCSymbolScalar current = this;
      return current._string == other._string;
    }

    public int CompareTo (object obj)
    {
      if (obj == null) {
        return 1;
      }
      RCSymbolScalar other = obj as RCSymbolScalar;
      if (other == null) {
        return 1;
      }
      return MultiCompareSymbolParts (this.ToArray (), other.ToArray ());
      // return _string.CompareTo (other._string);
    }

    public int CompareTo (RCSymbolScalar other)
    {
      if (other == null) {
        return 1;
      }
      return MultiCompareSymbolParts (this.ToArray (), other.ToArray ());
      // return _string.CompareTo (other._string);
    }

    public static int MultiCompareSymbolParts (object[] left, object[] right)
    {
      if (left.Length == right.Length) {
        for (int i = 0; i < left.Length; ++i)
        {
          IComparable leftValue = (IComparable) left[i];
          IComparable rightValue = (IComparable) right[i];
          int result = leftValue.CompareTo (rightValue);
          if (result != 0) {
            return result;
          }
        }
        return 0;
      }
      else if (left.Length > right.Length) {
        for (int i = 0; i < right.Length; ++i)
        {
          IComparable leftValue = (IComparable) left[i];
          IComparable rightValue = (IComparable) right[i];
          int result = leftValue.CompareTo (rightValue);
          if (result != 0) {
            return result;
          }
        }
        // the (shorter) right argument is greater
        return 1;
      }
      else { // if (left.Length < right.Length)
        for (int i = 0; i < left.Length; ++i)
        {
          IComparable leftValue = (IComparable) left[i];
          IComparable rightValue = (IComparable) right[i];
          int result = leftValue.CompareTo (rightValue);
          if (result != 0) {
            return result;
          }
        }
        // the (shorter) right argument is greater
        return -1;
      }
    }
  }
}


using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class ScalarMath
  {
    [Primitive ("+")]
    public static byte Plus (byte l, byte r) { return (byte) (l + r); }

    [Primitive ("+")]
    public static long Plus (byte l, long r) { return l + r; }

    [Primitive ("+")]
    public static double Plus (byte l, double r) { return l + r; }

    [Primitive ("+")]
    public static decimal Plus (byte l, decimal r) { return l + r; }

    [Primitive ("+")]
    public static long Plus (long l, byte r) { return l + r; }

    [Primitive ("+")]
    public static long Plus (long l, long r) { return l + r; }

    [Primitive ("+")]
    public static double Plus (long l, double r) { return l + r; }

    [Primitive ("+")]
    public static decimal Plus (long l, decimal r) { return l + r; }

    [Primitive ("+")]
    public static double Plus (double l, byte r) { return l + r; }

    [Primitive ("+")]
    public static double Plus (double l, long r) { return l + r; }

    [Primitive ("+")]
    public static double Plus (double l, double r) { return l + r; }

    //[Primitive ("+")]
    //public static decimal Plus (double l, decimal r) { throw new RCRuntimeException ("You cannot add double and decimal."); }

    [Primitive ("+")]
    public static decimal Plus (decimal l, byte r) { return l + r; }

    [Primitive ("+")]
    public static decimal Plus (decimal l, long r) { return l + r; }

    //[RCPrimitive ("+")]
    //public static decimal Plus (decimal l, double r) { throw new RCRuntimeException ("You cannot add decimal and double."); }

    [Primitive ("+")]
    public static decimal Plus (decimal l, decimal r) { return l + r; }

    [Primitive ("+")]
    public static string Plus (string l, string r) { return l + r; }

    [Primitive ("+")]
    public static RCSymbolScalar Plus (RCSymbolScalar l, RCSymbolScalar r)
    {
      RCSymbolScalar result = l;
      object[] keys = r.ToArray ();
      for (int i = 0; i < keys.Length; ++i)
        result = new RCSymbolScalar (result, keys[i]);
      return result;
    }

    [Primitive ("+")]
    public static RCTimeScalar Plus (RCTimeScalar l, RCTimeScalar r)
    {
      if (l.Type == RCTimeType.Timespan && r.Type == RCTimeType.Timespan)
      {
        return new RCTimeScalar (l.Ticks + r.Ticks, RCTimeType.Timespan);
      }
      else
      {
        return new RCTimeScalar (l.Ticks + r.Ticks, RCTimeType.Timestamp);
      }
    }

    [Primitive ("-")]
    public static byte Minus (byte l, byte r) { return (byte) (l - r); }

    [Primitive ("-")]
    public static long Minus (byte l, long r) { return l - r; }

    [Primitive ("-")]
    public static double Minus (byte l, double r) { return l - r; }

    [Primitive ("-")]
    public static decimal Minus (byte l, decimal r) { return l - r; }

    [Primitive ("-")]
    public static long Minus (long l, byte r) { return l - r; }

    [Primitive ("-")]
    public static long Minus (long l, long r) { return l - r; }

    [Primitive ("-")]
    public static double Minus (long l, double r) { return l - r; }

    [Primitive ("-")]
    public static decimal Minus (long l, decimal r) { return l - r; }

    [Primitive ("-")]
    public static double Minus (double l, byte r) { return l - r; }

    [Primitive ("-")]
    public static double Minus (double l, long r) { return l - r; }

    [Primitive ("-")]
    public static double Minus (double l, double r) { return l - r; }

    //[Primitive ("-")]
    //public static decimal Minus (double l, decimal r) { throw new RCRuntimeException ("You cannot add double and decimal."); }

    [Primitive ("-")]
    public static decimal Minus (decimal l, byte r) { return l - r; }

    [Primitive ("-")]
    public static decimal Minus (decimal l, long r) { return l - r; }

    //[Primitive ("-")]
    //public static decimal Minus (decimal l, double r) { throw new RCRuntimeException ("You cannot add decimal and double."); }

    [Primitive ("-")]
    public static decimal Minus (decimal l, decimal r) { return l - r; }

    [Primitive ("-")]
    public static RCTimeScalar Minus (RCTimeScalar l, RCTimeScalar r)
    {
      if (l.Type == RCTimeType.Timespan)
      {
        return new RCTimeScalar (l.Ticks - r.Ticks, RCTimeType.Timespan);
      }
      else if (l.Type != RCTimeType.Timespan && r.Type != RCTimeType.Timespan)
      {
        return new RCTimeScalar (l.Ticks - r.Ticks, RCTimeType.Timespan);
      }
      else
      {
        return new RCTimeScalar (l.Ticks - r.Ticks, RCTimeType.Timestamp);
      }
    }

    [Primitive ("*")]
    public static byte Multiply (byte l, byte r) { return (byte) (l * r); }

    [Primitive ("*")]
    public static long Multiply (byte l, long r) { return l * r; }

    [Primitive ("*")]
    public static double Multiply (byte l, double r) { return l * r; }

    [Primitive ("*")]
    public static decimal Multiply (byte l, decimal r) { return l * r; }

    [Primitive ("*")]
    public static long Multiply (long l, byte r) { return l * r; }

    [Primitive ("*")]
    public static long Multiply (long l, long r) { return l * r; }

    [Primitive ("*")]
    public static double Multiply (long l, double r) { return l * r; }

    [Primitive ("*")]
    public static decimal Multiply (long l, decimal r) { return l * r; }

    [Primitive ("*")]
    public static double Multiply (double l, byte r) { return l * r; }

    [Primitive ("*")]
    public static double Multiply (double l, long r) { return l * r; }

    [Primitive ("*")]
    public static double Multiply (double l, double r) { return l * r; }

    //[Primitive ("*")]
    //public static decimal Multiply (double l, decimal r) { throw new RCRuntimeException ("You cannot add double and decimal."); }

    [Primitive ("*")]
    public static decimal Multiply (decimal l, byte r) { return l * r; }

    [Primitive ("*")]
    public static decimal Multiply (decimal l, long r) { return l * r; }

    //[Primitive ("*")]
    //public static decimal Multiply (decimal l, double r) { throw new RCRuntimeException ("You cannot add decimal and double."); }

    [Primitive ("*")]
    public static decimal Multiply (decimal l, decimal r) { return l * r; }

    [Primitive ("/")]
    public static byte Divide (byte l, byte r) { return (byte) (l / r); }

    [Primitive ("/")]
    public static long Divide (byte l, long r) { return l / r; }

    [Primitive ("/")]
    public static double Divide (byte l, double r) { return l / r; }

    [Primitive ("/")]
    public static decimal Divide (byte l, decimal r) { return l / r; }

    [Primitive ("/")]
    public static long Divide (long l, byte r) { return l / r; }

    [Primitive ("/")]
    public static long Divide (long l, long r) { return l / r; }

    [Primitive ("/")]
    public static double Divide (long l, double r) { return l / r; }

    [Primitive ("/")]
    public static decimal Divide (long l, decimal r) { return l / r; }

    [Primitive ("/")]
    public static double Divide (double l, byte r) { return l / r; }

    [Primitive ("/")]
    public static double Divide (double l, long r) { return l / r; }

    [Primitive ("/")]
    public static double Divide (double l, double r) { return l / r; }

    //[Primitive ("/")]
    //public static decimal Divide (double l, decimal r) { throw new RCRuntimeException ("You cannot add double and decimal."); }

    [Primitive ("/")]
    public static decimal Divide (decimal l, byte r) { return l / r; }

    [Primitive ("/")]
    public static decimal Divide (decimal l, long r) { return l / r; }

    //[Primitive ("/")]
    //public static decimal Divide (decimal l, double r) { throw new RCRuntimeException ("You cannot add decimal and double."); }

    [Primitive ("/")]
    public static decimal Divide (decimal l, decimal r) { return l / r; }

    [Primitive ("not", Profile.Monadic)]
    public static bool Not (bool r) { return !r; }

    [Primitive ("not", Profile.Monadic)]
    public static byte Not (byte r) { return (byte) ~r; }

    [Primitive ("or")]
    public static bool Or (bool l, bool r) { return l || r; }

    [Primitive ("or")]
    public static byte Or (byte l, byte r) { return (byte) (l | r); }

    [Primitive ("and")]
    public static bool And (bool l, bool r) { return l && r; }

    [Primitive ("and")]
    public static byte And (byte l, byte r) { return (byte) (l & r); }

    [Primitive ("==")]
    public static bool Equals (byte l, byte r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (byte l, long r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (byte l, double r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (byte l, decimal r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (double l, double r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (double l, long r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (double l, byte r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (long l, long r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (long l, double r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (long l, decimal r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (long l, byte r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (decimal l, decimal r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (decimal l, long r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (decimal l, byte r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (bool l, bool r) { return l == r; }

    [Primitive ("==")]
    public static bool Equals (RCSymbolScalar l, RCSymbolScalar r) { return l.Equals (r); }

    [Primitive ("==")]
    public static bool Equals (string l, string r) { return l.Equals (r); }

    [Primitive ("!=")]
    public static bool NotEquals (byte l, byte r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (byte l, long r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (byte l, double r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (byte l, decimal r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (double l, double r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (double l, long r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (double l, byte r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (long l, long r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (long l, double r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (long l, decimal r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (long l, byte r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (decimal l, decimal r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (decimal l, long r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (decimal l, byte r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (bool l, bool r) { return l != r; }

    [Primitive ("!=")]
    public static bool NotEquals (RCSymbolScalar l, RCSymbolScalar r) { return !l.Equals (r); }

    [Primitive ("!=")]
    public static bool NotEquals (string l, string r) { return !l.Equals (r); }

    [Primitive ("<")]
    public static bool LessThan (byte l, byte r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (byte l, long r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (byte l, double r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (byte l, decimal r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (double l, double r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (double l, long r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (double l, byte r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (long l, long r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (long l, double r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (long l, byte r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (long l, decimal r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (decimal l, decimal r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (decimal l, long r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (decimal l, byte r) { return l < r; }

    [Primitive ("<")]
    public static bool LessThan (RCTimeScalar l, RCTimeScalar r) { return l.Ticks < r.Ticks; }

    [Primitive (">")]
    public static bool GreaterThan (byte l, byte r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (byte l, long r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (byte l, double r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (byte l, decimal r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (double l, double r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (double l, long r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (double l, byte r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (long l, long r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (long l, double r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (long l, byte r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (long l, decimal r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (decimal l, decimal r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (decimal l, long r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (decimal l, byte r) { return l > r; }

    [Primitive (">")]
    public static bool GreaterThan (RCTimeScalar l, RCTimeScalar r) { return l.Ticks > r.Ticks; }
    
    //BEGIN OR EQUAL
    [Primitive ("<=")]
    public static bool LessThanOrEqual (byte l, byte r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (byte l, long r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (byte l, double r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (byte l, decimal r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (double l, double r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (double l, long r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (double l, byte r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (long l, long r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (long l, double r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (long l, byte r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (long l, decimal r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (decimal l, decimal r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (decimal l, long r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (decimal l, byte r) { return l <= r; }

    [Primitive ("<=")]
    public static bool LessThanOrEqual (RCTimeScalar l, RCTimeScalar r) { return l.Ticks <= r.Ticks; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (byte l, byte r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (byte l, long r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (byte l, double r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (byte l, decimal r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (double l, double r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (double l, long r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (double l, byte r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (long l, long r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (long l, double r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (long l, byte r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (long l, decimal r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (decimal l, decimal r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (decimal l, long r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (decimal l, byte r) { return l >= r; }

    [Primitive (">=")]
    public static bool GreaterThanOrEqual (RCTimeScalar l, RCTimeScalar r) { return l.Ticks >= r.Ticks; }

    //Hello we need these for other data types as well.
    [Primitive ("min")]
    public static long Min (long l, long r) { return Math.Min (l, r); }

    [Primitive ("min")]
    public static double Min (double l, double r) { return Math.Min (l, r); }

    [Primitive ("min")]
    public static decimal Min (decimal l, decimal r) { return Math.Min (l, r); }

    [Primitive ("min")]
    public static byte Min (byte l, byte r) { return Math.Min (l, r); }

    [Primitive ("max")]
    public static long Max (long l, long r) { return Math.Max (l, r); }

    [Primitive ("max")]
    public static double Max (double l, double r) { return Math.Max (l, r); }

    [Primitive ("max")]
    public static decimal Max (decimal l, decimal r) { return Math.Max (l, r); }

    [Primitive ("max")]
    public static byte Max (byte l, byte r) { return Math.Max (l, r); }

    //t total
    public struct PlusState<T> { public T t; }

    [Primitive ("+", Profile.Cumulative)]
    public static byte Plus (ref PlusState<byte> s, byte r) { s.t += r; return s.t; }

    [Primitive ("+", Profile.Cumulative)]
    public static long Plus (ref PlusState<long> s, long r) { s.t += r; return s.t; }

    [Primitive ("+", Profile.Cumulative)]
    public static double Plus (ref PlusState<double> s, double r) { s.t += r; return s.t; }

    [Primitive ("+", Profile.Cumulative)]
    public static decimal Plus (ref PlusState<decimal> s, decimal r) { s.t += r; return s.t; }

    [Primitive ("-", Profile.Cumulative)]
    public static byte Minus (ref PlusState<byte> s, byte r) { byte o = (byte) (r - s.t); s.t = r; return o; }

    [Primitive ("-", Profile.Cumulative)]
    public static long Minus (ref PlusState<long> s, long r) { long o = r - s.t; s.t = r; return o; }

    [Primitive ("-", Profile.Cumulative)]
    public static double Minus (ref PlusState<double> s, double r) { double o = r - s.t; s.t = r; return o; }

    [Primitive ("-", Profile.Cumulative)]
    public static decimal Minus (ref PlusState<decimal> s, decimal r) { decimal o = r - s.t; s.t = r; return o; }

    //t total
    public struct SumState<T> { public T t; }

    [Primitive ("sum", Profile.Sequential)]
    public static byte Sum (ref SumState<byte> s, byte r) { s.t += r; return s.t; }

    [Primitive ("sum", Profile.Sequential)]
    public static long Sum (ref SumState<long> s, long r) { s.t += r; return s.t; }

    [Primitive ("sum", Profile.Sequential)]
    public static double Sum (ref SumState<double> s, double r) { s.t += r; return s.t; }

    [Primitive ("sum", Profile.Sequential)]
    public static decimal Sum (ref SumState<decimal> s, decimal r) { s.t += r; return s.t; }

    //n numerator, d denominator
    public struct AvgState<T> { public T n; public T d; }

    [Primitive ("avg", Profile.Sequential)]
    public static double Average (ref AvgState<byte> s, byte r)
      { s.n += r; s.d += 1; return ((double) s.n) / ((double) s.d); }

    [Primitive ("avg", Profile.Sequential)]
    public static double Average (ref AvgState<long> s, long r)
      { s.n += r; s.d += 1; return ((double) s.n) / ((double) s.d); }

    [Primitive ("avg", Profile.Sequential)]
    public static double Average (ref AvgState<double> s, double r)
      { s.n += r; s.d += 1; return s.n / s.d;}

    [Primitive ("avg", Profile.Sequential)]
    public static decimal Average (ref AvgState<decimal> s, decimal r)
      { s.n += r; s.d += 1; return s.n / s.d; }

    //Track the index so that the initial value of 0 will not be
    //confused for the high/low value in the series.
    //i index, h high
    public struct HighState<T> { public int i; public T h; }

    [Primitive ("high", Profile.Sequential)]
    public static byte High (ref HighState<byte> s, byte r)
      { s.h = s.i == 0 || r > s.h ? r : s.h; ++s.i; return s.h; }

    [Primitive ("high", Profile.Sequential)]
    public static long High (ref HighState<long> s, long r)
      { s.h = s.i == 0 || r > s.h ? r : s.h; ++s.i; return s.h; }

    [Primitive ("high", Profile.Sequential)]
    public static double High (ref HighState<double> s, double r)
      { s.h = s.i == 0 || r > s.h ? r : s.h; ++s.i; return s.h; }

    [Primitive ("high", Profile.Sequential)]
    public static decimal High (ref HighState<decimal> s, decimal r)
      { s.h = s.i == 0 || r > s.h ? r : s.h; ++s.i; return s.h; }

    //i index, l low
    public struct LowState<T> { public int i; public T l; }

    [Primitive ("low", Profile.Sequential)]
    public static byte Low (ref LowState<byte> s, byte r)
      { s.l = s.i == 0 || r < s.l ? r : s.l; ++s.i; return s.l; }

    [Primitive ("low", Profile.Sequential)]
    public static long Low (ref LowState<long> s, long r)
      { s.l = s.i == 0 || r < s.l ? r : s.l; ++s.i; return s.l; }

    [Primitive ("low", Profile.Sequential)]
    public static double Low (ref LowState<double> s, double r)
      { s.l = s.i == 0 || r < s.l ? r : s.l; ++s.i; return s.l; }

    [Primitive ("low", Profile.Sequential)]
    public static decimal Low (ref LowState<decimal> s, decimal r)
      { s.l = s.i == 0 || r < s.l ? r : s.l; ++s.i; return s.l; }

    //Coercion operators
    [Primitive ("string", Profile.Monadic)]
    public static string String (string r) { return r; }

    [Primitive ("string", Profile.Monadic)]
    public static string String (byte r) { return RCByte.HexChars (r); }

    [Primitive ("string", Profile.Monadic)]
    public static string String (bool r) { return r ? "true" : "false"; }

    [Primitive ("string", Profile.Monadic)]
    public static string String (long r) { return r.ToString (); }

    [Primitive ("string", Profile.Monadic)]
    public static string String (double r) { return r.ToString (); }

    [Primitive ("string", Profile.Monadic)]
    public static string String (decimal r) { return r.ToString (); }

    [Primitive ("string", Profile.Monadic)]
    public static string String (RCSymbolScalar r) { return r.ToString (); }

    [Primitive ("string", Profile.Monadic)]
    public static string String (RCTimeScalar r) { return r.ToString (); }

    [Primitive ("length", Profile.Monadic)]
    public static long Length (string r) { return r.Length; }

    [Primitive ("length", Profile.Monadic)]
    public static long Length (RCSymbolScalar r) { return r.Length; }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (long r) { return r; }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (byte r) { return (long) r; }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (double r) { return (long) r; }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (decimal r) { return (long) r; }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (bool r) { return r ? 1 : 0; }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (string r) { return long.Parse (r); }

    public class ParseContext<T> : Context<T>
    {
      public T Def;
      public override void Init (RCArray<T> def)
      {
        Def = def[0];
      }
    }
    [Primitive ("long", Profile.Contextual)]
    public static long Long (ParseContext<long> c, string r) { return r.Length == 0 ? c.Def : long.Parse (r); }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (RCSymbolScalar r) { return (long) r.Part (0); }

    [Primitive ("long", Profile.Monadic)]
    public static long Long (RCTimeScalar r) { return r.Ticks; }

    [Primitive ("double", Profile.Monadic)]
    public static double Double (double r) { return r; }

    [Primitive ("double", Profile.Monadic)]
    public static double Double (byte r) { return (double) r; }

    [Primitive ("double", Profile.Monadic)]
    public static double Double (long r) { return (double) r; }

    [Primitive ("double", Profile.Monadic)]
    public static double Double (decimal r) { return (double) r; }

    [Primitive ("double", Profile.Monadic)]
    public static double Double (bool r) { return r ? 1d : 0d; }

    [Primitive ("double", Profile.Monadic)]
    public static double Double (string r)
    {
      try
      {
        return double.Parse (r);
      }
      catch (Exception)
      {
        return double.NaN;
      }
    }


    [Primitive ("byte", Profile.Monadic)]
    public static byte Byte (byte r) { return r; }

    [Primitive ("byte", Profile.Monadic)]
    public static byte Byte (double r) { return (byte) r; }

    [Primitive ("byte", Profile.Monadic)]
    public static byte Byte (long r) { return (byte) r; }

    [Primitive ("byte", Profile.Monadic)]
    public static byte Byte (decimal r) { return (byte) r; }

    [Primitive ("byte", Profile.Monadic)]
    public static byte Byte (bool r) { return r ? (byte) 0x01 : (byte) 0x00; }

    [Primitive ("byte", Profile.Monadic)]
    public static byte Byte (string r) { return byte.Parse (r); }

    [Primitive ("boolean", Profile.Monadic)]
    public static bool Boolean (bool r) { return r; }

    [Primitive ("boolean", Profile.Monadic)]
    public static bool Boolean (byte r) { return r == 0 ? false : true; }

    [Primitive ("boolean", Profile.Monadic)]
    public static bool Boolean (long r) { return r == 0 ? false : true; }

    [Primitive ("boolean", Profile.Monadic)]
    public static bool Boolean (double r) { return r == 0 ? false : true; }

    [Primitive ("boolean", Profile.Monadic)]
    public static bool Boolean (decimal r) { return r == 0 ? false : true; }

    [Primitive ("boolean", Profile.Monadic)]
    public static bool Boolean (string r) { return bool.Parse (r); }

    [Primitive ("boolean", Profile.Contextual)]
    public static bool Boolean (ParseContext<bool> c, string r) { return r.Length == 0 ? c.Def : bool.Parse (r); }

    [Primitive ("decimal", Profile.Monadic)]
    public static decimal Decimal (decimal r) { return r; }

    [Primitive ("decimal", Profile.Monadic)]
    public static decimal Decimal (byte r) { return (decimal) r; }

    [Primitive ("decimal", Profile.Monadic)]
    public static decimal Decimal (long r) { return (decimal) r; }

    [Primitive ("decimal", Profile.Monadic)]
    public static decimal Decimal (double r) { return (decimal) r; }

    [Primitive ("decimal", Profile.Monadic)]
    public static decimal Decimal (bool r) { return r ? 1 : 0; }

    [Primitive ("decimal", Profile.Monadic)]
    public static decimal Decimal (string r) { return decimal.Parse (r); }

    [Primitive ("decimal", Profile.Contextual)]
    public static decimal Decimal (ParseContext<decimal> c, string r) { return r.Length == 0 ? c.Def : decimal.Parse (r); }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (RCSymbolScalar r) { return r; }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (byte r) { return new RCSymbolScalar(null, r); }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (long r) { return new RCSymbolScalar(null, r); }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (double r) { return new RCSymbolScalar(null, r); }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (decimal r) { return new RCSymbolScalar(null, r); }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (bool r) { return new RCSymbolScalar(null, r); }

    [Primitive ("symbol", Profile.Monadic)]
    public static RCSymbolScalar Symbol (string r) {
      if (r.Length > 0 && r[0] == '#') {
        r = r.Substring (1);
      }
      return new RCSymbolScalar (null, r);
    }

    [Primitive ("symbol", Profile.Contextual)]
    public static RCSymbolScalar Symbol (ParseContext<RCSymbolScalar> c, string r) { 
      if (r.Length == 0) return c.Def;
      return Symbol (r);
    }

    [Primitive ("time", Profile.Monadic)]
    public static RCTimeScalar Time (long r) { return new RCTimeScalar (new DateTime (r), RCTimeType.Timestamp); }

    [Primitive ("time", Profile.Monadic)]
    public static RCTimeScalar Time (string r) { return TimeToken.ParseTime (r); }

    [Primitive ("time", Profile.Contextual)]
    public static RCTimeScalar Time (ParseContext<RCTimeScalar> c, string r) { 
      if (r.Length == 0) return c.Def;
      return TimeToken.ParseTime (r);
    }

    [Primitive ("time", Profile.Monadic)]
    public static RCTimeScalar Time (RCTimeScalar r) { return r; }

    [Primitive ("day", Profile.Monadic)]
    public static long Day (RCTimeScalar t)
    {
      return (t.Type == RCTimeType.Timespan) ? 
        new TimeSpan (t.Ticks).Days : new DateTime (t.Ticks).Day;
    }

    [Primitive ("hour", Profile.Monadic)]
    public static long Hour (RCTimeScalar t)
    {
      return (t.Type == RCTimeType.Timespan) ? 
        new TimeSpan (t.Ticks).Hours : new DateTime (t.Ticks).Hour;
    }

    [Primitive ("minute", Profile.Monadic)]
    public static long Minute (RCTimeScalar t)
    {
      return (t.Type == RCTimeType.Timespan) ? 
        new TimeSpan (t.Ticks).Minutes : new DateTime (t.Ticks).Minute;
    }

    [Primitive ("second", Profile.Monadic)]
    public static long Second (RCTimeScalar t)
    {
      return (t.Type == RCTimeType.Timespan) ? 
        new TimeSpan (t.Ticks).Seconds : new DateTime (t.Ticks).Second;
    }

    [Primitive ("nano", Profile.Monadic)]
    public static long Nano (RCTimeScalar t)
    {
      if (t.Type == RCTimeType.Timespan)
      {
        long nanos = t.Ticks;
        TimeSpan span = new TimeSpan (t.Ticks);
        nanos -= span.Days * TimeSpan.TicksPerDay;
        nanos -= span.Hours * TimeSpan.TicksPerHour;
        nanos -= span.Minutes * TimeSpan.TicksPerMinute;
        nanos -= span.Seconds * TimeSpan.TicksPerSecond;
        return nanos * 100;
      }
      else
      {
        long nanos = t.Ticks;
        DateTime time = new DateTime (t.Ticks);
        nanos -= time.Day * TimeSpan.TicksPerDay;
        nanos -= time.Hour * TimeSpan.TicksPerHour;
        nanos -= time.Minute * TimeSpan.TicksPerMinute;
        nanos -= time.Second * TimeSpan.TicksPerSecond;
        return nanos * 100;
      }
    }

    [Primitive ("date", Profile.Monadic)]
    public static RCTimeScalar Date (RCTimeScalar t)
    {
      DateTime old = new DateTime (t.Ticks);
      DateTime result = new DateTime (old.Year, old.Month, old.Day);
      return new RCTimeScalar (result, RCTimeType.Date);
    }

    [Primitive ("daytime", Profile.Monadic)]
    public static RCTimeScalar Daytime (RCTimeScalar t)
    {
      DateTime old = new DateTime (t.Ticks);
      DateTime result = new DateTime (1, 1, 1, old.Hour, old.Minute, 0);
      return new RCTimeScalar (result, RCTimeType.Daytime);
    }

    [Primitive ("datetime", Profile.Monadic)]
    public static RCTimeScalar Datetime (RCTimeScalar t)
    {
      DateTime old = new DateTime (t.Ticks);
      DateTime result = new DateTime (old.Year, old.Month, old.Day, old.Hour, old.Minute, 0);
      return new RCTimeScalar (result, RCTimeType.Datetime);
    }

    [Primitive ("timestamp", Profile.Monadic)]
    public static RCTimeScalar Timestamp (RCTimeScalar t)
    {
      return new RCTimeScalar (t.Ticks, RCTimeType.Timestamp);
    }

    [Primitive ("timespan", Profile.Monadic)]
    public static RCTimeScalar Timespan (RCTimeScalar t)
    {
      return new RCTimeScalar (t.Ticks, RCTimeType.Timespan);
    }

    [Primitive ("upper", Profile.Monadic)]
    public static string Upper (string r) { return r.ToUpper (); }

    [Primitive ("lower", Profile.Monadic)]
    public static string Lower (string r) { return r.ToLower (); }

    [Primitive ("sqrt", Profile.Monadic)]
    public static double Sqrt (double r) { return Math.Sqrt (r); }

    [Primitive ("abs", Profile.Monadic)]
    public static long Abs (long r) { return Math.Abs (r); }

    [Primitive ("abs", Profile.Monadic)]
    public static double Abs (double r) { return Math.Abs (r); }

    [Primitive ("abs", Profile.Monadic)]
    public static decimal Abs (decimal r) { return Math.Abs (r); }

    public class MapContext<T> : Context<T>
    {
      public Dictionary<T,T> m;
      public override void Init (RCArray<T> map)
      {
        m = new Dictionary<T, T> ();
        for (int i = 0; i < map.Count; i += 2)
          m[map[i]] = map[i + 1];
      }
    }

    [Primitive ("map", Profile.Contextual)]
    public static long Map (MapContext<long> s, long r)
      { long o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    [Primitive ("map", Profile.Contextual)]
    public static bool Map (MapContext<bool> s, bool r)
      { bool o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    [Primitive ("map", Profile.Contextual)]
    public static double Map (MapContext<double> s, double r)
      { double o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    [Primitive ("map", Profile.Contextual)]
    public static decimal Map (MapContext<decimal> s, decimal r)
      { decimal o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    [Primitive ("map", Profile.Contextual)]
    public static string Map (MapContext<string> s, string r)
      { string o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    [Primitive ("map", Profile.Contextual)]
    public static byte Map (MapContext<byte> s, byte r)
      { byte o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    [Primitive ("map", Profile.Contextual)]
    public static RCSymbolScalar Map (MapContext<RCSymbolScalar> s, RCSymbolScalar r)
      { RCSymbolScalar o; if (s.m.TryGetValue (r, out o)) return o; else return r; }

    //What is wrong with this?!?!
    [Primitive ("map", Profile.Contextual)]
    public static RCTimeScalar Map (MapContext<RCTimeScalar> s, RCTimeScalar r)
    {
      RCTimeScalar o; 
      if (s.m.TryGetValue (r, out o)) 
        return o; 
      else return r; 
    }
     
    public class ReplaceContext<T> : Context<T>
    {
      //f from, t to
      public T f,t;
      public override void Init (RCArray<T> map)
        { f = map[0]; t = map[1]; }
    }

    [Primitive ("replace", Profile.Contextual)]
    public static string Replace (ReplaceContext<string> c, string r)
      { return r.Replace (c.f, c.t); }

    public class FillContext<T> : Context<T>
    {
      //f from, t to
      public T empty;
      public T last;
      public override void Init (RCArray<T> left)
      { 
        empty = left[0];
        last = empty;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static string Fill (FillContext<string> c, string r)
    {
      if (r.Equals (c.empty))
      {
        return c.last;
      }
      else
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static long Fill (FillContext<long> c, long r)
    {
      if (r == c.empty) 
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static double Fill (FillContext<double> c, double r)
    {
      if (r == c.empty) 
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static decimal Fill (FillContext<decimal> c, decimal r)
    {
      if (r == c.empty) 
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static bool Fill (FillContext<bool> c, bool r)
    {
      if (r == c.empty) 
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static byte Fill (FillContext<byte> c, byte r)
    {
      if (r == c.empty) 
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static RCSymbolScalar Fill (FillContext<RCSymbolScalar> c, RCSymbolScalar r)
    {
      if (r.Equals (c.empty))
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    [Primitive ("fill", Profile.Contextual)]
    public static RCTimeScalar Fill (FillContext<RCTimeScalar> c, RCTimeScalar r)
    {
      if (r.Equals (c.empty))
      { 
        return c.last;
      } 
      else 
      {
        c.last = r;
        return r;
      }
    }

    public class PartContext<T> : Context<T>
    {
      //i index
      public RCArray<T> i;
      public override void Init (RCArray<T> index) { i = index; }
    }

    [Primitive ("part", Profile.Contextual)]
    public static RCSymbolScalar Part (PartContext<long> c, RCSymbolScalar r)
    {
      RCSymbolScalar o = null;
      for (int i = 0; i < c.i.Count; ++i)
        o = new RCSymbolScalar (o, r.Part (c.i[i]));
      return o;
    }

    public class ContainsContext<T> : Context<T>
    {
      public HashSet<T> values;
      public override void Init (RCArray<T> right)
      {
        values = new HashSet<T> (right);
      }
    }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<bool> c, bool r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<byte> c, byte r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<long> c, long r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<double> c, double r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<decimal> c, decimal r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<string> c, string r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<RCSymbolScalar> c, RCSymbolScalar r)
      { return c.values.Contains (r); }

    [Primitive ("in", Profile.Contextual)]
    public static bool Contains (ContainsContext<RCTimeScalar> c, RCTimeScalar r)
    { return c.values.Contains (r); }

    public class LikeContext<T> : Context<T>
    {
      public string[] m_parts;
      public string m_expression;
      public override void Init (RCArray<T> right)
      {
        m_expression = right[0].ToString ();
        m_parts = m_expression.Split ('*');
      }
    }

    [Primitive ("like", Profile.Contextual)]
    public static bool Like (LikeContext<string> c, string r)
    {
      int start = 0;
      int part = 0;
      if (r.Length == 0)
      {
        if (c.m_expression == "*" || c.m_expression == "") 
        {
          return true;
        }
        return false;
      }
      while (start < r.Length && part < c.m_parts.Length)
      {
        if (c.m_parts[part] != "")
        {
          int index = r.IndexOf (c.m_parts[part], start);
          if (index >= 0)
          {
            start += c.m_parts[part].Length;
          }
          else
          {
            return false;
          }
        }
        ++part;
      }
      return true;
    }
  }
}

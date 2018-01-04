
using System;

namespace RCL.Kernel
{
  /// <summary>
  /// Represents dates, times, and quantities of time in RCL.
  /// </summary>
  public class RCTime : RCVector<RCTimeScalar>
  {
    /// <summary>
    /// The zero-count vector.
    /// </summary>
    public static RCTime Empty = new RCTime ();

    /// <summary>
    /// Defines formats for each type of RCTimeScalar.
    /// </summary>
    public static readonly string[] FORMATS = new string[] {
      //date
      "yyyy.MM.dd",
      //daytime
      "HH:mm",
      //datetime
      "yyyy.MM.dd HH:mm",
      //timestamp
      "yyyy.MM.dd HH:mm:ss.fffffff"
    };

    /// <summary>
    /// Constructor for inline use.
    /// </summary>
    public RCTime (params RCTimeScalar[] data) : base (data) {}

    /// <summary>
    /// Constructor for programmatic use.
    /// </summary>
    public RCTime (RCArray<RCTimeScalar> data) : base (data) {}

    /// <summary>
    /// The ScalarType of the vector is used for operator dispatch.
    /// </summary>
    public override Type ScalarType { get { return typeof (RCTimeScalar); } }

    /// <summary>
    /// Equality for time data.
    /// </summary>
    public override bool ScalarEquals (RCTimeScalar x, RCTimeScalar y)
    {
      return x.Ticks == y.Ticks && x.Type == y.Type;
    }

    /// <summary>
    /// Some scalar types can be identified by a suffix. Not this one.
    /// </summary>
    public override string Suffix
    {
      get { return ""; }
    }

    /// <summary>
    /// Single character mnemonic for the time type.
    /// </summary>
    public override char TypeCode
    {
      get { return 't'; }
    }

    /// <summary>
    /// In-language name for the type.
    /// </summary>
    public override string TypeName
    {
      get { return "time"; }
    }

    /// <summary>
    /// Why is this set to -1? Did we forget to implement binary for the time type?
    /// </summary>
    public override int SizeOfScalar
    {
      get { return -1; }
    }

    /// <summary>
    /// Conversion of time scalars to strings.
    /// </summary>
    public override string ScalarToString (RCTimeScalar scalar)
    {
      return FormatScalar (scalar);
    }

    /// <summary>
    /// Standard time formatting method.
    /// </summary>
    public static string FormatScalar (RCTimeScalar scalar)
    {
      if (scalar.Type == RCTimeType.Timespan)
      {
        TimeSpan ts = new TimeSpan (scalar.Ticks);
        int fraction = (int) (scalar.Ticks % TimeSpan.TicksPerSecond);
        return string.Format ("{0}.{1:00}:{2:00}:{3:00}.{4:0000000}", 
                              ts.Days,
                              Math.Abs (ts.Hours),
                              Math.Abs (ts.Minutes),
                              Math.Abs (ts.Seconds),
                              Math.Abs (fraction));
      }
      return new DateTime (scalar.Ticks).ToString (FORMATS[(int) scalar.Type]);
    }

    /// <summary>
    /// Serialize the time data.
    /// </summary>
    public override void ToByte (RCArray<byte> result)
    {
      Binary.WriteVectorTime (result, this);
    }

    /// <summary>
    /// Extend the vector with boxing (expensive).
    /// </summary>
    public override void Write (object box)
    {
      m_data.Write ((RCTimeScalar) box);
    }
  }
}

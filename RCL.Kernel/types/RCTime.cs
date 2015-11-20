
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCTime : RCVector<RCTimeScalar>
  {
    public static RCTime Empty = new RCTime ();

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

    public RCTime (params RCTimeScalar[] data) : base (data) {}
    public RCTime (RCArray<RCTimeScalar> data) : base (data) {}

    public override Type ScalarType { get { return typeof (RCTimeScalar); } }

    public override bool ScalarEquals (RCTimeScalar x, RCTimeScalar y)
    {
      return x.Ticks == y.Ticks && x.Type == y.Type;
    }

    public override string Suffix
    {
      get { return ""; }
    }

    public override char TypeCode
    {
      get { return 't'; }
    }

    public override string TypeName
    {
      get { return "time"; }
    }

    public override int SizeOfScalar
    {
      get { return -1; }
    }

    public override string ScalarToString (RCTimeScalar scalar)
    {
      return FormatScalar (scalar);
    }

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

    public override void ToByte (RCArray<byte> result)
    {
      Binary.WriteVectorTime (result, this);
    }

    public override void Write (object box)
    {
      m_data.Write ((RCTimeScalar) box);
    }
  }
}
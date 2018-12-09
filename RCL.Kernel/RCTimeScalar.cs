
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public enum RCTimeType
  {
    Date = 0,
    Daytime = 1,
    Datetime = 2,
    Timestamp = 3,
    Timespan = 4
  }

  public struct RCTimeScalar : IComparable<RCTimeScalar>, IComparable
  {
    public static RCTimeScalar Empty = new RCTimeScalar (0, RCTimeType.Date);
    public readonly long Ticks;
    public readonly RCTimeType Type;

    public static RCTimeScalar Now ()
    {
      return new RCTimeScalar (DateTime.UtcNow, RCTimeType.Timestamp);
    }

    public RCTimeScalar (long ticks, RCTimeType type)
    {
      Ticks = ticks;
      Type = type;
    }

    public RCTimeScalar (DateTime time, RCTimeType type)
    {
      Ticks = time.Ticks;
      Type = type;
    }

    public RCTimeScalar (TimeSpan span)
    {
      Ticks = span.Ticks;
      Type = RCTimeType.Timespan;
    }

    public int CompareTo (RCTimeScalar other)
    {
      return Ticks.CompareTo (other.Ticks);
    }

    public int CompareTo (object other)
    {
      return CompareTo ((RCTimeScalar) other);
    }

    public override string ToString ()
    {
      return RCTime.FormatScalar (null, this);
    }
  }
}

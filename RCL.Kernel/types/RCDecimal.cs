
using System;
using System.Globalization;

namespace RCL.Kernel
{
  public class RCDecimal : RCVector<decimal>
  {
    public static readonly RCDecimal Empty = new RCDecimal ();
    public static readonly CultureInfo Culture;
    protected static readonly NumberFormatInfo FormatProvider;

    static RCDecimal ()
    {
      FormatProvider = new NumberFormatInfo ();
      FormatProvider.NumberGroupSeparator = "";
      Culture = CultureInfo.InvariantCulture;
    }

    public RCDecimal (params decimal[] data) : base (data) { }
    public RCDecimal (RCArray<decimal> data) : base (data) { }

    public override bool ScalarEquals (decimal x, decimal y)
    {
      return x == y;
    }

    public override string Suffix
    {
      get { return "m"; }
    }

    public override char TypeCode
    {
      get { return 'm'; }
    }

    public override string TypeName
    {
      get { return DECIMAL_TYPENAME; }
    }

    public override int SizeOfScalar
    {
      get { return 16; }
    }

    public override Type ScalarType
    {
      get { return typeof (decimal); }
    }

    public override string ScalarToString (string format, decimal scalar)
    {
      return FormatScalar (format, scalar);
    }

    public static string FormatScalar (string format, decimal scalar)
    {
      if (format == null)
      {
        if ((scalar % 1) == 0)
        {
          return scalar.ToString ("N0", FormatProvider);
        }
        else
        {
          return scalar.ToString (FormatProvider);
        }
      }
      else
      {
        return scalar.ToString (format, FormatProvider);
      }
    }

    public override void ToByte (RCArray<byte> result)
    {
      Binary.WriteVectorDecimal (result, this);
    }

    public override void Write (object box)
    {
      m_data.Write ((decimal) box);
    }
  }
}


using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCDouble : RCVector<double>
  {
    public static readonly RCDouble Empty = new RCDouble ();
    protected static readonly NumberFormatInfo CanonicalFormatProvider;
    static RCDouble ()
    {
      CanonicalFormatProvider = new NumberFormatInfo ();
      CanonicalFormatProvider.NumberGroupSeparator = "";
    }

    public RCDouble (params double[] data) : base (data) {}
    public RCDouble (RCArray<double> data) : base (data) {}

    public override Type ScalarType {
      get { return typeof (double); }
    }

    public static bool DoubleScalarEquals (double x, double y, double threshold)
    {
      if (Math.Abs (x - y) < threshold) {
        return true;
      }
      else if (double.IsNaN (x) && double.IsNaN (y)) {
        return true;
      }
      else {
        return false;
      }
    }

    public static bool DoubleScalarEquals (double x, double y)
    {
      return DoubleScalarEquals (x, y, 0.000001);
    }

    public override bool ScalarEquals (double x, double y)
    {
      return DoubleScalarEquals (x, y);
    }

    public override string Suffix
    {
      get { return ""; }
    }

    public override char TypeCode
    {
      get { return 'd'; }
    }

    public override string TypeName
    {
      get { return RCValue.DOUBLE_TYPENAME; }
    }

    public override int SizeOfScalar
    {
      get { return 8; }
    }

    public override string ScalarToString (string format, double scalar)
    {
      return FormatScalar (format, scalar);
    }

    public static string FormatScalar (string format, double scalar)
    {
      if (format == null) {
        // https://stackoverflow.com/questions/8184068/decimal-tostring-formatting-which-gives-at-least-1-digit-no-upper-limit
        return scalar.ToString ("0.0###########################", CanonicalFormatProvider);
      }
      else {
        return scalar.ToString (format);
      }
    }

    public override void Write (object box)
    {
      m_data.Write ((double) box);
    }
  }
}

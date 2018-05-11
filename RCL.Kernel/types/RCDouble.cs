
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCDouble : RCVector<double>
  {
    public static readonly RCDouble Empty = new RCDouble ();
    public static readonly CultureInfo Culture;
    protected static readonly NumberFormatInfo FormatProvider;
    static RCDouble ()
    {
      FormatProvider = new NumberFormatInfo ();
      FormatProvider.NumberGroupSeparator = "";
      Culture = CultureInfo.InvariantCulture;
    }

    public RCDouble (params double[] data) : base (data) { }
    public RCDouble (RCArray<double> data) : base (data) { }

    public override Type ScalarType { get { return typeof (double); } }

    public override bool ScalarEquals (double x, double y)
    {
      return x == y;
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
      get { return "double"; }
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
      if (format == null)
      {
        // https://stackoverflow.com/questions/8184068/decimal-tostring-formatting-which-gives-at-least-1-digit-no-upper-limit
        return scalar.ToString ("0.0###########################", FormatProvider);
      }
      else
      {
        return scalar.ToString (format, FormatProvider);
      }
    }

    public override void Write (object box)
    {
      m_data.Write ((double) box);
    }
  }
}

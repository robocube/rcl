
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCDouble : RCVector<double>
  {
    public static readonly RCDouble Empty = new RCDouble ();
    protected static readonly NumberFormatInfo IntegerFormat;
    static RCDouble ()
    {
      IntegerFormat = new NumberFormatInfo ();
      IntegerFormat.NumberGroupSeparator = "";
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

    public override string ScalarToString (double scalar)
    {
      return FormatScalar (scalar);
    }

    public static string FormatScalar (double scalar)
    {
      if ((scalar % 1) == 0)
      {
        return scalar.ToString ("N1", IntegerFormat);
      }
      else
      {
        return scalar.ToString ();
      }
    }

    public override void Write (object box)
    {
      m_data.Write ((double) box);
    }
  }
}
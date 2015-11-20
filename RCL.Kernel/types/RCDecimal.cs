
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCDecimal : RCVector<decimal>
  {
    public static readonly RCDecimal Empty = new RCDecimal ();
    public RCDecimal(params decimal[] data) : base(data) { }
    public RCDecimal(RCArray<decimal> data) : base(data) { }

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
      get { return "decimal"; }
    }

    public override int SizeOfScalar
    {
      get { return 16; }
    }

    public override Type ScalarType
    {
      get { return typeof (decimal); }
    }

    public static string FormatScalar (decimal scalar)
    {
      return scalar.ToString ();
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
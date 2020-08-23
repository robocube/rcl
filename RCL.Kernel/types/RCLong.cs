
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCLong : RCVector<long>
  {
    public static readonly RCLong Empty = new RCLong ();
    public readonly static RCLong Zero = new RCLong (0);
    public RCLong (params long[] data) : base (data) {}
    public RCLong (RCArray<long> data) : base (data) {}

    public override bool ScalarEquals (long x, long y)
    {
      return x == y;
    }

    public override string Suffix
    {
      get { return ""; }
    }

    public override char TypeCode
    {
      get { return 'l'; }
    }

    public override string TypeName
    {
      get { return RCValue.LONG_TYPENAME; }
    }

    public override int SizeOfScalar
    {
      get { return 8; }
    }

    public override Type ScalarType
    {
      get { return typeof (long); }
    }

    public static string FormatScalar (long scalar)
    {
      return scalar.ToString ();
    }

    public static RCArray<int> ReadVector4 (RCArray<byte> data, ref int start)
    {
      return RCVector<int>.ReadVector (data, ref start, sizeof (int));
    }

    public override void Write (object box)
    {
      _data.Write ((long) box);
    }
  }
}

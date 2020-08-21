
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCBoolean : RCVector<bool>
  {
    public static readonly RCBoolean Empty = new RCBoolean ();
    public static readonly RCBoolean True = new RCBoolean (true);
    public static readonly RCBoolean False = new RCBoolean (false);

    public RCBoolean (params bool[] data) : base (data) {}
    public RCBoolean (RCArray<bool> data) : base (data) {}

    public override char TypeCode
    {
      get { return 'b'; }
    }

    public override string TypeName
    {
      get { return RCValue.BOOLEAN_TYPENAME; }
    }

    public override int SizeOfScalar
    {
      get { return 1; }
    }

    public override Type ScalarType
    {
      get { return typeof (bool); }
    }

    public override bool ScalarEquals (bool x, bool y)
    {
      return x == y;
    }

    public override void ScalarToString (StringBuilder builder, bool scalar)
    {
      builder.Append (ScalarToString (scalar));
    }

    public static string FormatScalar (string format, bool scalar)
    {
      return scalar ? "true" : "false";
    }

    public override string ScalarToString (string format, bool scalar)
    {
      return FormatScalar (format, scalar);
    }

    public override void Write (object box)
    {
      m_data.Write ((bool) box);
    }
  }
}

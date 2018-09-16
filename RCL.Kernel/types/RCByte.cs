
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCByte : RCVector<byte>
  {
    public static readonly RCByte Empty = new RCByte ();
    public RCByte (params byte[] data) : base (data) { }
    public RCByte (RCArray<byte> data) : base (data) { }

    public override bool ScalarEquals (byte x, byte y)
    {
      return x == y;
    }

    protected static string HEXCHARS = "0123456789ABCDEF";
    public override string ScalarToString (string format, byte scalar)
    {
      return FormatScalar (scalar);
    }

    public static string HexChars (byte scalar)
    {
      return "" + HEXCHARS[scalar >> 4] + HEXCHARS[scalar & 0x0000000F];
    }

    public static string FormatScalar (byte scalar)
    {
      return "\\x" + HexChars (scalar);
    }

    public override string Suffix
    {
      get { return ""; }
    }

    public override char TypeCode
    {
      get { return 'x'; }
    }

    public override string TypeName
    {
      get { return RCValue.BYTE_TYPENAME; }
    }

    public override int SizeOfScalar
    {
      get { return 1; }
    }

    public override Type ScalarType
    {
      get { return typeof (byte); }
    }

    public override void Write (object box)
    {
      m_data.Write ((byte) box);
    }

    public string Utf8String ()
    {
      return Encoding.UTF8.GetString (m_data.m_source);
    }
  }
}

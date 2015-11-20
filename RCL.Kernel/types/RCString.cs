
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCString : RCVector<string>
  {
    public static readonly RCString Empty = new RCString ();
    public RCString (params string[] data) : base (data) { }
    public RCString (RCArray<string> data) : base (data) { }

    public override char TypeCode
    {
      get { return 's'; }
    }

    public override string TypeName
    {
      get { return "string"; }
    }

    public override int SizeOfScalar
    {
      get { return -1;}
    }

    public override Type ScalarType
    {
      get { return typeof (string); }
    }
    
    public override void ScalarToString (StringBuilder builder, string scalar)
    {
      builder.Append ("\"");
      builder.Append (RCTokenType.EscapeControlChars(scalar.ToString(), '"'));
      builder.Append ("\"");
    }

    public override string ScalarToString (string scalar)
    {
      return FormatScalar (scalar);
    }

    public static string FormatScalar (string scalar)
    {
      return "\"" + RCTokenType.EscapeControlChars (scalar.ToString(), '"') + "\"";
    }

    public override string Shorthand (object scalar)
    {
      return scalar.ToString ();
    }
    
    public override bool ScalarEquals (string x, string y)
    {
      return x == y;
    }

    public override void ToByte (RCArray<byte> result)
    {
      Binary.WriteVectorString (result, this);
    }

    public override void Write (object box)
    {
      m_data.Write ((string) box);
    }
  }
}
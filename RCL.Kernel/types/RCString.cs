
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCString : RCVector<string>
  {
    public static readonly RCString Empty = new RCString ();
    public RCString (params string[] data) : base (data)
    {
      for (int i = 0; i < data.Length; ++i)
      {
        if (m_data[i] == null)
        {
          m_data.Write (i, "");
        }
      }
    }
    public RCString (RCArray<string> data) : base (data) { }

    public override char TypeCode
    {
      get { return 's'; }
    }

    public override string TypeName
    {
      get { return RCValue.STRING_TYPENAME; }
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
      builder.Append (RCTokenType.EscapeControlChars (scalar.ToString (), '"'));
      builder.Append ("\"");
    }

    public override string ScalarToString (string format, string scalar)
    {
      return FormatScalar (format, scalar);
    }

    public static string FormatScalar (string format, string scalar)
    {
      return "\"" + RCTokenType.EscapeControlChars (scalar.ToString (), '"') + "\"";
    }

    public override string Shorthand (object scalar)
    {
      return scalar.ToString ();
    }

    public override string IdShorthand (object scalar)
    {
      string id = scalar.ToString ();
      if (id.Length > 0)
      {
        if (id[0] == '\'')
        {
          if (id[id.Length - 1] == '\'')
          {
            return id;
          }
          else throw new Exception ("Invalid id: " + id);
        }
      }
      else
      {
        return id;
      }

      if ((id[0] >= '0') && (id[0] <= '9'))
      {
        return "'" + id + "'";
      } 
      else
      {
        for (int i = 0; i < id.Length; ++i)
        {
          if (!RCTokenType.IsIdentifierChar (id[i]))      
          {
            return "'" + id + "'";
          }
        }
      }
      return id;
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

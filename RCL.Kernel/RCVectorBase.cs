
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public abstract class RCVectorBase : RCValue
  {
    protected readonly static Dictionary<string, RCVectorBase> m_types = new Dictionary<string, RCVectorBase> ();
    static RCVectorBase ()
    {
      m_types.Add (typeof(byte).Name, new RCByte ());
      m_types.Add (typeof(double).Name, new RCDouble ());
      m_types.Add (typeof(long).Name, new RCLong ());
      m_types.Add (typeof(decimal).Name, new RCDecimal ());
      m_types.Add (typeof(string).Name, new RCString ());
      m_types.Add (typeof(bool).Name, new RCBoolean ());
      m_types.Add (typeof(RCSymbolScalar).Name, new RCSymbol ());
      m_types.Add (typeof(RCTimeScalar).Name, new RCTime ());
      m_types.Add (typeof(RCIncrScalar).Name, new RCIncr ());
    }

    public static RCVectorBase FromArray (object array)
    {
      Type arrayType = array.GetType ();
      if (arrayType == typeof (RCArray<byte>))
        return new RCByte ((RCArray<byte>) array);
      else if (arrayType == typeof (RCArray<long>))
        return new RCLong ((RCArray<long>) array);
      //else if (arrayType == typeof (int))
      //  return new RCLong ((RCArray<long>) array);
      else if (arrayType == typeof (RCArray<double>))
        return new RCDouble ((RCArray<double>) array);
      else if (arrayType == typeof (RCArray<decimal>))
        return new RCDecimal ((RCArray<decimal>) array);
      else if (arrayType == typeof (RCArray<string>))
        return new RCString ((RCArray<string>) array);
      else if (arrayType == typeof (RCArray<bool>))
        return new RCBoolean ((RCArray<bool>) array);
      else if (arrayType == typeof (RCArray<RCSymbolScalar>))
        return new RCSymbol ((RCArray<RCSymbolScalar>) array);
      else if (arrayType == typeof (RCArray<RCTimeScalar>))
        return new RCTime ((RCArray<RCTimeScalar>) array);
      else if (arrayType == typeof (RCArray<RCIncrScalar>))
        return new RCIncr ((RCArray<RCIncrScalar>) array);
      else throw new Exception ("Return values of type: " + arrayType + " are not supported.");
    }

    public static RCVectorBase FromScalar (object scalar)
    {
      Type scalarType = scalar.GetType ();
      if (scalarType == typeof (byte))
        return new RCByte ((byte) scalar);
      else if (scalarType == typeof (long))
        return new RCLong ((long) scalar);
      //else if (scalarType == typeof (int))
      //  return new RCLong ((long) (int) scalar);
      else if (scalarType == typeof (double))
        return new RCDouble ((double) scalar);
      else if (scalarType == typeof (decimal))
        return new RCDecimal ((decimal) scalar);
      else if (scalarType == typeof (string))
        return new RCString ((string) scalar);
      else if (scalarType == typeof (bool))
        return new RCBoolean ((bool) scalar);
      else if (scalarType == typeof (RCSymbolScalar))
        return new RCSymbol ((RCSymbolScalar) scalar);
      else if (scalarType == typeof (RCTimeScalar))
        return new RCTime ((RCTimeScalar) scalar);
      else if (scalarType == typeof (RCIncrScalar))
        return new RCIncr ((RCIncrScalar) scalar);
      else throw new Exception ("Return values of type: " + scalarType + " are not supported.");
    }

    public static RCVectorBase EmptyOf (Type type)
    {
      return m_types[type.Name];
    }

    public abstract void Write (object box);
    public abstract string Suffix { get; }
    public abstract Type ScalarType { get; }
    public abstract int SizeOfScalar { get; }
    public abstract string ScalarToString (object scalar);
    public abstract string Shorthand (object scalar);
    public abstract string IdShorthand (object scalar);
    public abstract object Array { get; }
  }
}

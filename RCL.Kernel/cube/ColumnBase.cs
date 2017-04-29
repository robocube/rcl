
using System;

namespace RCL.Kernel
{
  public abstract class ColumnBase
  {
    public static ColumnBase FromArray (Timeline timeline, RCArray<int> index, object data)
    {
      Type type = data.GetType ();
      if (type == typeof (RCArray<byte>))
        return new RCCube.ColumnOfByte (timeline, index, data);
      else if (type == typeof (RCArray<long>))
        return new RCCube.ColumnOfLong (timeline, index, data);
      else if (type == typeof (RCArray<double>))
        return new RCCube.ColumnOfDouble (timeline, index, data);
      else if (type == typeof (RCArray<decimal>))
        return new RCCube.ColumnOfDecimal (timeline, index, data);
      else if (type == typeof (RCArray<string>))
        return new RCCube.ColumnOfString (timeline, index, data);
      else if (type == typeof (RCArray<bool>))
        return new RCCube.ColumnOfBool (timeline, index, data);
      else if (type == typeof (RCArray<RCSymbolScalar>))
        return new RCCube.ColumnOfSymbol (timeline, index, data);
      else if (type == typeof (RCArray<RCTimeScalar>))
        return new RCCube.ColumnOfTime (timeline, index, data);
      else if (type == typeof (RCArray<RCIncrScalar>))
        return new RCCube.ColumnOfIncr (timeline, index, data);
      else throw new Exception ("unsupported type: " + type);
    }

    public abstract bool Write (RCSymbolScalar key, int index, object val, bool force);
    public abstract object Array {get;}
    public abstract RCArray<int> Index {get;}
    public abstract char TypeCode {get;}
    public abstract int Count {get;}
    public abstract Type GetElementType ();
    public abstract void Lock ();
    public abstract void ReverseInPlace (int tlcount);

    public abstract void Accept (string name, Visitor visitor, int i);
    public abstract void AcceptNull (string name, Visitor visitor, int i);
    public abstract string ScalarToString (int vrow);
    public abstract string ScalarToCsvString (int vrow);
    public abstract object BoxCell (int vrow);
    public abstract bool BoxLast (RCSymbolScalar key, out object box);

    public int CountBefore (int tlcount)
    {
      return Index.BinarySearch (tlcount);
    }
  }
}
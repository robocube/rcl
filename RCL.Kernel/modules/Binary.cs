
using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Binary
  {
    [RCVerb ("binary")]
    public void EvalBinary (RCRunner runner, RCClosure closure, object right)
    {
      RCValue val = (RCValue) right;
      RCArray<byte> result = new RCArray<byte> ();
      val.ToByte (result);
      runner.Yield (closure, new RCByte (result));
    }

    public static void WriteBlock (RCArray<byte> result, RCBlock block)
    {
      //Pre-order traversal of the expression tree.
      result.Write ((byte) 'k');
      WriteScalarInt (result, block.Count);
      for (int i = 0; i < block.Count; ++i)
      {
        RCBlock name = block.GetName (i);
        Binary.WriteScalarString (name.Name, result);
        //Binary.WriteScalarString (RCEvaluator.For (name.Evaluator), result);
        Binary.WriteScalarString (name.Evaluator.Symbol, result);
        name.Value.ToByte (result);
      }
    }

    public static void WriteScalarInt (RCArray<byte> result, int val)
    {
      result.Write ((byte) val);
      result.Write ((byte) (val << 8));
      result.Write ((byte) (val << 16));
      result.Write ((byte) (val << 24));
    }

    public static void WriteScalarString (string scalar, RCArray<byte> result)
    {
      WriteScalarInt (result, scalar.Length);
      for (int j = 0; j < scalar.Length; ++j)
      {
        result.Write ((byte) scalar [j]);
        result.Write ((byte) (scalar[j] << 8));
      }
    }

    public static void WriteOperator (RCArray<byte> result, RCOperator op)
    {
      //Pre-order traversal of the expression tree.
      result.Write ((byte) 'o');
      int count = op.Left == null ? 2 : 3;
      WriteScalarInt (result, count);
      if (op.Left != null)
        op.Left.ToByte (result);
      Binary.WriteScalarString (op.Name, result);
      op.Right.ToByte (result);
    }

    public static void WriteReference (RCArray<byte> result, RCReference reference)
    {
      result.Write ((byte) 'r');
      WriteScalarInt (result, reference.Parts.Count);
      for (int i = 0; i < reference.Parts.Count; ++i)
        Binary.WriteScalarString (reference.Parts[i], result);
    }

    //This works for vectors of types with fixed size.
    //decimal is an exception because it is not supported by Buffer.BlockCopy.
    public static void WriteVector<T> (RCArray<byte> result, RCArray<T> array, int size)
    {
      int length = (int) array.Count * size;
      WriteScalarInt (result, array.Count);
      result.Resize (length, 0);
      Buffer.BlockCopy (array.m_source, 0, result.m_source, (int) result.m_count, length);
      result.m_count += length;
    }

    public static void WriteVectorDecimal (RCArray<byte> result, RCVector<decimal> vector)
    {
      result.Write ((byte) vector.TypeCode);
      WriteVectorDecimal (result, vector.Data);
    }

    public static void WriteVectorDecimal (RCArray<byte> result, RCArray<decimal> data)
    {
      WriteScalarInt (result, data.Count);
      result.Resize (data.Count * sizeof(decimal), 0);
      for (int i = 0; i < data.Count; ++i)
      {
        decimal val = data[i];
        //This is horrible because a new byte[8] gets allocated for every
        //element in this vector.  But Buffer.BlockCopy does not work on
        //decimal and I spent a silly amount of time googling for efficient
        //ways to get the bits out one byte at a time, but that effort failed.
        //I still think there is hope though.
        int[] bits = decimal.GetBits (val);
        Buffer.BlockCopy (
          bits, 0, result.m_source,
          (int) result.m_count + i * sizeof (decimal), sizeof (decimal));
      }
      result.m_count += data.Count * sizeof (decimal);
    }

    public static void WriteScalarDecimal (decimal val, RCArray<byte> result)
    {
      int[] bits = decimal.GetBits (val);
      for (int i = 0; i < bits.Length; ++i)
        WriteScalarInt (result, bits[i]);
    }

    public static void WriteVectorString (RCArray<byte> result, RCVector<string> vector)
    {
      result.Write ((byte) vector.TypeCode);
      WriteVectorString (result, vector.Data);
    }

    public static void WriteVectorString (RCArray<byte> result, RCArray<string> data)
    {
      Dictionary<string, int> lookup = new Dictionary<string, int> ();
      List<string> unique = new List<string> ();
      int index = 0;
      for (int i = 0; i < data.Count; ++i)
      {
        if (!lookup.ContainsKey (data[i]))
        {
          lookup.Add (data[i], index++);
          unique.Add (data[i]);
        }
      }

      int uniques = unique.Count;
      WriteScalarInt (result, uniques);
      for (int i = 0; i < unique.Count; ++i)
        WriteScalarString (unique[i], result);
      WriteScalarInt (result, data.Count);
      for (int i = 0; i < data.Count; ++i)
      {
        index = lookup[data[i]];
        WriteScalarInt (result, index);
      }
    }

    public static void WriteVectorSymbol (RCArray<byte> result, RCVector<RCSymbolScalar> vector)
    {
      result.Write ((byte) vector.TypeCode);
      WriteVectorSymbol (result, vector.Data);
    }

    public static void WriteVectorSymbol (RCArray<byte> result, RCArray<RCSymbolScalar> data)
    {
      Dictionary<RCSymbolScalar, int> lookup = new Dictionary<RCSymbolScalar, int> ();
      RCArray<RCSymbolScalar> unique = new RCArray<RCSymbolScalar> ();
      int index = 0;
      for (int i = 0; i < data.Count; ++i)
      {
        if (!lookup.ContainsKey (data[i]))
        {
          lookup.Add (data[i], index++);
          unique.Write (data[i]);
        }
      }
      WriteScalarInt (result, unique.Count);
      for (int i = 0; i < unique.Count; ++i)
      {
        int length = (int) unique[i].Length;
        WriteScalarInt (result, length);
        data[i].ToByte (result);
      }
      WriteScalarInt (result, data.Count);
      for (int i = 0; i < data.Count; ++i)
      {
        index = lookup[data[i]];
        WriteScalarInt (result, index);
      }
    }

    public static void WriteScalarSymbol (RCArray<byte> result, RCSymbolScalar scalar)
    {
      //This is not going to be a triumph of efficiency.
      //I am considering rewriting RCSymbolScalar so that it stores all of its
      //data as an array of bytes.  Then the ToByte operation would just
      //return the internal representation.
      for (int i = 0; i < scalar.Length; ++i)
      {
        object part = scalar.Part (i);
        char type = RCVectorBase.EmptyOf (part.GetType ()).TypeCode;
        result.Write ((byte) type);
        switch (type)
        {
          case 'l' :
          result.Write (BitConverter.GetBytes ((long) part));
          break;
          case 'd' :
          result.Write (BitConverter.GetBytes ((double) part));
          break;
          case 'm' :
          Binary.WriteScalarDecimal ((decimal) part, result);
          break;
          case 'b' :
          result.Write (BitConverter.GetBytes ((bool) part));
          break;
          case 's' :
          Binary.WriteScalarString ((string) part, result);
          break;
          default: throw new Exception ("Unknown type:" + type + " found in symbol");
        }
      }
    }

    public static void WriteVectorTime (RCArray<byte> result, RCVector<RCTimeScalar> vector)
    {
      result.Write ((byte) vector.TypeCode);
      WriteVectorTime (result, vector.Data);
    }

    public static void WriteVectorTime (RCArray<byte> result, RCArray<RCTimeScalar> data)
    {
      WriteScalarInt (result, data.Count);
      for (int i = 0; i < data.Count; ++i)
      {
        result.Write (BitConverter.GetBytes (data[i].Ticks));
        result.Write (BitConverter.GetBytes ((int) data[i].Type));
      }
    }

    public static void WriteVectorIncr (RCArray<byte> result, RCVector<RCIncrScalar> vector)
    {
      result.Write ((byte) vector.TypeCode);
      WriteVectorIncr (result, vector.Data);
    }

    public static void WriteVectorIncr (RCArray<byte> result, RCArray<RCIncrScalar> data)
    {
      WriteScalarInt (result, data.Count);
      for (int i = 0; i < data.Count; ++i)
      {
        switch (data[i])
        {
          case RCIncrScalar.Increment: result.Write (0x00); break;
          case RCIncrScalar.Decrement: result.Write (0x01); break;
          case RCIncrScalar.Delete:    result.Write (0x02); break;
        }
      }
    }

    [RCVerb ("parse")]
    public void EvalParse (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      int start = 0;
      RCValue result = DoParse (RCSystem.Activator, right.Data, ref start);
      runner.Yield (closure, result);
    }

    public static RCValue DoParse (
      RCActivator activator, RCArray<byte> array, ref int start)
    {
      //The type of the data to be parsed.
      char type = (char) array[start];
      start += 1;
      switch (type)
      {
        case 'x': return new RCByte (ReadVector<byte> (array, ref start, sizeof (byte)));
        case 'l': return new RCLong (ReadVector<long> (array, ref start, sizeof (long)));
        case 'd': return new RCDouble (ReadVector<double> (array, ref start, sizeof (double)));
        case 'b': return new RCBoolean (ReadVector<bool> (array, ref start, sizeof (bool)));
        case 'm': return new RCDecimal (ReadVectorDecimal (array, ref start));
        case 's': return new RCString (ReadVectorString (array, ref start));
        case 'y': return new RCSymbol (ReadVectorSymbol (array, ref start));
        case 't': return new RCTime (ReadVectorTime (array, ref start));
        case 'n': return new RCIncr (ReadVectorIncr (array, ref start));
        case 'r': return ReadReference (activator, array, ref start);
        case 'o': return ReadOperator (activator, array, ref start);
        case 'k': return ReadBlock (activator, array, ref start);
        //Again need some kind of "parser extension" or "operator extension" framework
        //to get this to work, but I want to keep the implementation with RCCube.
        //case 'u': return RCCube.ReadCube (activator, array, ref start);
        //default : throw new Exception ("Unsupported type code:" + type);
        default : return activator.ExtensionFor (type).BinaryParse (activator, array, ref start);
      }
    }

    public static RCBlock ReadBlock (
      RCActivator activator, RCArray<byte> data, ref int start)
    {
      int count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);
      RCBlock result = null;
      for (int i = 0; i < count; ++i)
      {
        string name = Binary.ReadScalarString (data, ref start);
        string evaluator = Binary.ReadScalarString (data, ref start);
        RCValue val = Binary.DoParse (activator, data, ref start);
        result = new RCBlock (result, name, evaluator, val);
      }
      return result;
    }

    public static RCOperator ReadOperator (
      RCActivator activator, RCArray<byte> data, ref int start)
    {
      int count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);

      RCValue left = null;
      RCValue right = null;
      string name = null;

      if (count == 2)
      {
        name = Binary.ReadScalarString (data, ref start);
        right = Binary.DoParse (activator, data, ref start);
      }
      else if (count == 3)
      {
        left = Binary.DoParse (activator, data, ref start);
        name = Binary.ReadScalarString (data, ref start);
        right = Binary.DoParse (activator, data, ref start);
      }
      else throw new Exception ("count of an operator must always be 2 or 3.");

      return activator.New (name, left, right);
    }

    public static RCReference ReadReference (
      RCActivator activator, RCArray<byte> array, ref int start)
    {
      int count = BitConverter.ToInt32 (array.m_source, start);
      start += sizeof (int);
      string[] parts = new string[count];
      for (int i = 0; i < count; ++i)
        parts[i] = ReadScalarString (array, ref start);
      return new RCReference (parts);
    }

    public static RCArray<T> ReadVector<T> (RCArray<byte> array, ref int start, int size)
    {
      int count = BitConverter.ToInt32 (array.m_source, start);
      int length = count * size;
      start += sizeof (int);
      T[] result = new T[count];
      Buffer.BlockCopy (array.m_source, start, result, 0, length);
      start += length;
      return new RCArray<T> (result);
    }

    public static RCArray<decimal> ReadVectorDecimal (RCArray<byte> data, ref int start)
    {
      int count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);

      decimal[] result = new decimal[count];
      for (int i = 0; i < result.Length; ++i)
      {
        decimal val = ReadScalarDecimal (data, start);
        result[i] = val;
        start += sizeof (decimal);
      }
      return new RCArray<decimal> (result);
    }

    public static decimal ReadScalarDecimal (RCArray<byte> array, int start)
    {
      int lo = BitConverter.ToInt32 (array.m_source, start);
      int mid = BitConverter.ToInt32 (array.m_source, start + 4);
      int hi = BitConverter.ToInt32 (array.m_source, start + 8);
      byte scale = array[start + 14];
      bool negative = BitConverter.ToBoolean (array.m_source, start + 15);
      decimal val = new decimal (lo, mid, hi, negative, scale);
      return val;
    }

    public static RCArray<RCTimeScalar> ReadVectorTime (RCArray<byte> data, ref int start)
    {
      int count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);

      RCTimeScalar[] result = new RCTimeScalar[count];
      for (int i = 0; i < result.Length; ++i)
      {
        RCTimeScalar val = ReadScalarTime (data, start);
        result[i] = val;
        start += 12;
      }
      return new RCArray<RCTimeScalar> (result);
    }

    public static RCTimeScalar ReadScalarTime (RCArray<byte> array, int start)
    {
      long ticks = BitConverter.ToInt64 (array.m_source, start);
      int type = BitConverter.ToInt32 (array.m_source, start + 8);
      return new RCTimeScalar (new DateTime (ticks), (RCTimeType) type);
    }

    public static RCArray<string> ReadVectorString (RCArray<byte> array, ref int start)
    {
      //This is the count of unique elements.
      int count = BitConverter.ToInt32 (array.m_source, start);
      start += sizeof (int);

      List<string> unique = new List<string> ();
      //In the case of strings the length contains the number of unique values.
      for (int i = 0; i < count; ++i)
      {
        string scalar = ReadScalarString (array, ref start);
        unique.Add (scalar);
      }

      //This is the count of actual elements.
      count = BitConverter.ToInt32 (array.m_source, start);
      start += sizeof (int);
      string[] result = new string [count];
      for (int i = 0; i < count; ++i)
      {
        int index = BitConverter.ToInt32 (array.m_source, start);
        result[i] = unique[index];
        start += sizeof (int);
      }
      return new RCArray<string> (result);
    }

    public static string ReadScalarString (RCArray<byte> data, ref int start)
    {
      int length = BitConverter.ToInt32 (data.m_source, start);
      char[] chars = new char[length];
      start += sizeof (int);
      for (int j = 0; j < length; ++j)
      {
        char c = BitConverter.ToChar (data.m_source, start);
        chars[j] = c;
        start += sizeof (char);
      }
      return new string (chars);
    }

    public static RCArray<RCSymbolScalar> ReadVectorSymbol (RCArray<byte> data, ref int start)
    {
      //count of unique elements in the vector.
      int count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);
      RCArray<RCSymbolScalar> unique = new RCArray<RCSymbolScalar> ();
      for (int i = 0; i < count; ++i)
      {
        RCSymbolScalar scalar = ReadScalarSymbol (data, ref start);
        unique.Write (scalar);
      }

      //count of actual elements in the vector.
      count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);
      RCSymbolScalar[] result = new RCSymbolScalar [count];
      for (int i = 0; i < count; ++i)
      {
        int index = BitConverter.ToInt32 (data.m_source, start);
        result[i] = unique[index];
        start += sizeof (int);
      }
      return new RCArray<RCSymbolScalar> (result);
    }

    public static RCSymbolScalar ReadScalarSymbol (RCArray<byte> data, ref int start)
    {
      RCSymbolScalar result = null;
      int count = BitConverter.ToInt32 (data.m_source, start);
      start += sizeof (int);
      for (int i = 0; i < count; ++i)
      {
        char type = (char) data[start];
        start += 1;
        switch (type)
        {
          case 'l' :
          result = new RCSymbolScalar (result, BitConverter.ToInt64 (data.m_source, start));
          start += sizeof (long);
          break;
          case 'd' :
          result = new RCSymbolScalar (result, BitConverter.ToDouble (data.m_source, start));
          start += sizeof (double);
          break;
          case 'm' :
          result = new RCSymbolScalar (result, Binary.ReadScalarDecimal (data, start));
          start += sizeof (decimal);
          break;
          case 'b' :
          result = new RCSymbolScalar (result, BitConverter.ToBoolean (data.m_source, start));
          start += sizeof (bool);
          break;
          case 's' :
          result = new RCSymbolScalar (result, Binary.ReadScalarString (data, ref start));
          break;
          default : throw new Exception (
            "Unknown or unsupported type code in symbol:" + type);
        }
      }
      return result;
    }

    public static RCArray<RCIncrScalar> ReadVectorIncr (RCArray<byte> array, ref int start)
    {
      int count = BitConverter.ToInt32 (array.m_source, start);
      start += sizeof (int);
      RCIncrScalar[] result = new RCIncrScalar[count];
      for (int i = 0; i < result.Length; ++i)
      {
        byte val = array[start];
        switch (val)
        {
          case 0x00: result[i] = RCIncrScalar.Increment; break;
          case 0x01: result[i] = RCIncrScalar.Decrement; break;
          case 0x02: result[i] = RCIncrScalar.Delete; break;
        }
        start += sizeof (byte);
      }
      return new RCArray<RCIncrScalar> (result);
    }
  }
}

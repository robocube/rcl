
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public abstract class RCVector<T> : RCVectorBase, IEnumerable<T>
  {
    /// <summary>
    /// The data storage for the vector.
    /// </summary>
    protected RCArray<T> m_data;
    
    public RCVector (params T[] data)
    {
      m_data = new RCArray<T> (data);
    }

    public RCVector (RCArray<T> data)
    {
      m_data = data;
    }

    public RCArray<T> Data
    {
      get { return m_data; }
    }

    public override object Array
    {
      get { return m_data; }
    }

    public T this[int index]
    {
      get { return m_data[index]; }
    }

    public override int Count
    {
      get { return m_data.Count; }
    }

    /// <summary>
    /// For numeric types, the character indicating the concrete type.
    /// </summary>
    public override string Suffix
    {
      get { return ""; }
    }

    public override object Child (int i)
    {
      return m_data[i];
    }

    public override bool Equals (object obj)
    {
      if (obj == null) return false;
      RCVector<T> cobj = obj as RCVector<T>;
      if (cobj == null) return false;
      if (cobj.Count != Count) return false;
      for (int i = 0; i < Count; ++i)
      {
        if (!ScalarEquals (this[i], cobj[i])) return false;
      }
      return true;
    }

    public override int GetHashCode ()
    {
      return ToString ().GetHashCode ();
    }

    public override void Format (
      StringBuilder builder, RCFormat args, int level)
    {
      RCL.Kernel.Format.DoFormat<T> (this, builder, args, level);
    }

    public virtual T[] ToArray ()
    {
      return m_data.ToArray ();
    }

    /// <summary>
    /// Implement this for every type otherwise there will
    /// be boxing caused by Equals()
    /// </summary>
    public abstract bool ScalarEquals (T x, T y);

    public virtual void ScalarToString (StringBuilder builder, T scalar)
    {
      builder.Append (ScalarToString (scalar));
    }

    public override string Shorthand (object scalar)
    {
      return ScalarToString (scalar);
    }

    public override string IdShorthand (object scalar)
    {
      return Shorthand (scalar);
    }

    public override string ScalarToString (object scalar)
    {
      return ScalarToString ((T) scalar);
    }

    public virtual string ScalarToString (T scalar)
    {
      return scalar.ToString ();
    }

    public IEnumerator<T> GetEnumerator ()
    {
      return new RCVectorEnumerator<T> (this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
    {
      return new RCVectorEnumerator<T> (this);
    }

    public override void ToByte (RCArray<byte> result)
    {
      result.Write ((byte) TypeCode);
      Binary.WriteVector (result, this.Data, SizeOfScalar);
    }

    protected static RCArray<T> ReadVector (RCArray<byte> array, ref int start, int size)
    {
      int count = BitConverter.ToInt32 (array.m_source, start);
      int length = count * size;
      start += sizeof (int);
      T[] result = new T[count];
      Buffer.BlockCopy (array.m_source, start, result, 0, length);
      start += length;
      return new RCArray<T> (result);
    }

    public override void Cubify (RCCube target, Stack<object> names)
    {
      object[] array = names.ToArray ();
      System.Array.Reverse (array);
      RCSymbolScalar symbol = RCSymbolScalar.From (array);
      for (int i = 0; i < Count; ++i)
      {
        T val = this[i];
        RCSymbolScalar symboli = new RCSymbolScalar (symbol, (long) i);
        target.WriteCell (this.TypeCode.ToString (), symboli, val);
        target.Write (0, symboli);
      }
    }
      
    public class RCVectorEnumerator<T> : IEnumerator<T>
    {
      protected int i = -1;
      protected RCVector<T> m_vector;

      public RCVectorEnumerator (RCVector<T> array)
      {
        m_vector = array;
      }

      public T Current
      {
        get { return m_vector[i]; }
      }

      public void Dispose () { }

      object System.Collections.IEnumerator.Current
      {
        get { return m_vector[i]; }
      }

      public bool MoveNext ()
      {
        ++i;
        return i < m_vector.Count;
      }

      public void Reset ()
      {
        i = -1;
      }
    }
  }
}

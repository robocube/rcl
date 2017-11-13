
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCArrayStorage<T>
  {
    public T[] m_source;
    public int m_count;
    public bool m_lock = false;

    public RCArrayStorage (T[] source, int count)
    {
      if (source == null)
      {
        throw new ArgumentNullException ("source");
      }
      m_source = source;
      m_count = count;
    }
  }
  
  public class RCArray<T> : IEnumerable<T>
  {
    public static readonly RCArray<T> Empty = new RCArray<T> (0);
    protected internal bool m_lock = false;
    protected internal T[] m_source;
    protected internal int m_count;
    //protected RCArrayStorage<T> m_storage;

    public RCArray (int capacity)
    {
      m_source = new T[capacity];
      m_count = 0;
      //m_storage = new RCArrayStorage<T> (new T[capacity], 0);
    }

    public RCArray (params T[] source)
    {
      if (source == null)
      {
        throw new ArgumentNullException ("source");
      }
      m_source = source;
      m_count = source.Length;
      //m_storage = new RCArrayStorage<T> (source, source.Length);
    }

    public RCArray (ICollection<T> source)
    {
      m_source = new T[source.Count];
      m_count = source.Count;
      //m_storage = new RCArrayStorage<T> (new T[source.Count],
      //                                   source.Count);
      int i = 0;
      foreach (T val in source)
      {
        //m_storage.m_source[i] = val;
        m_source[i] = val;
        ++i;
      }
    }

    /*
    internal RCArray (RCArrayStorage<T> storage)
    {
      m_storage = storage;
      m_count = count;  
    }
    */

    public RCArray (RCArray<T> source)
      : this (source.ToArray ()) {}

    public int Count
    {
      get { return m_count; }
    }

    public T this[int i]
    {
      get { return m_source[i]; }
    }

    public T[] ToArray ()
    {
      T[] result = new T[Count];
      for (int i = 0; i < Count; ++i)
      {
        result[i] = this[i];
      }
      return result;
    }

    public IEnumerator<T> GetEnumerator ()
    {
      return new RCArrayEnumerator<T> (this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
    {
      return new RCArrayEnumerator<T> (this);
    }

    public int IndexOf (T value)
    {
      return Array.IndexOf<T> (m_source, value, 0, m_count);
    }

    public override string ToString ()
    {
      StringBuilder builder = new StringBuilder ();
      builder.Append ("(");
      builder.Append (typeof(T).Name);
      builder.Append (" ");
      builder.Append (Count);
      builder.Append ("/");
      builder.Append (m_source.Length);
      builder.Append (")");
      builder.Append ("[");
      for (int i = 0; i < Count; ++i)
      {
        builder.Append (this[i].ToString ());
        if (i < Count - 1)
          builder.Append (" ");
      }
      builder.Append ("]");
      return builder.ToString ();
    }

    /// <summary>
    /// http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
    /// </summary>
    public static long NextPowerOf2 (long n)
    {
      n--;
      n |= n >> 1;
      n |= n >> 2;
      n |= n >> 4;
      n |= n >> 8;
      n |= n >> 16;
      n |= n >> 32;
      n++;
      return n;
    }

    public void Write (T[] values)
    {
      if (m_lock)
        throw new InvalidOperationException (
          "Attempted to Write to an RCArray after it was locked.");

      Resize (values.Length, 0);
      for (int i = 0; i < values.Length; ++i)
      {
        m_source[m_count] = values[i];
        ++m_count;
      }
    }

    public void Write (RCArray<T> values)
    {
      if (m_lock)
        throw new InvalidOperationException (
          "Attempted to Write to an RCArray after it was locked.");

      Resize (values.Count, 0);
      for (int i = 0; i < values.Count; ++i)
      {
        m_source[m_count] = values[i];
        ++m_count;
      }
    }

    public void Write (T value)
    {
      if (m_lock)
      {
        throw new Exception (
          "Cannot write to an RCArray after it is locked.");
      }

      Resize (1, 0);
      m_source[m_count] = value;
      ++m_count;
    }

    /// <summary>
    /// WITH GREAT POWER COMES GREAT RESPONSIBILITY
    /// </summary>
    public void Write (int i, T value)
    {
      if (m_lock)
      {
        throw new Exception (
          "Cannot write to an RCArray after it is locked.");
      }
      Resize (1, 0);
      m_source[i] = value;
    }

    /*
    protected RCArray<T> Bust (RCArray<T> values)
    {
      T[] source = new T[NextPowerOf2 (m_source.Length + values.Count)];
      Write (source, m_count, values);        
      return new RCArray<T> (source, m_source.Length + values.Count);
    }    

    protected void Write (T[] source, int count, RCArray<T> values)
    {
      for (int i = 0; i < values.Count; ++i)
      {
        source[count] = values[i];
        ++count;
      }
    }

    public RCArray<T> CopyWrite (RCArray<T> values)
    {
      if (!m_lock)
      {
        throw new Exception (
          "Cannot CopyWrite to an RCArray until it is locked.");  
      }

      if (m_ccount > m_count || !NeedResize (values.Count, 0))
      {
        RCArray<T> result = new RCArray<T> (m_source, count);
       
      }
      else 
      {
        return Bust (values);  
      }
      throw new NotImplementedException ();
    }

    public RCArray<T> CopyWrite (T[] values)
    {
      throw new NotImplementedException ();
    }

    public RCArray<T> CopyWrite (T value)
    {
      throw new NotImplementedException ();
    }

    public RCArray<T> CopyWrite (int i, T value)
    {
      throw new NotImplementedException ();
    }
    */

    public void RemoveAt (int i)
    {
      for (;i < Count - 1; ++i)
        m_source[i] = m_source[i+1];
      --m_count;
    }

    public void Lock ()
    {
      m_lock = true;
    }
   
    public bool Locked ()
    {
      return m_lock;
    }

    public void Resize (long size, long dups)
    {
      long count = m_count + (size - dups);
      if (m_source.Length < count)
      {
        //At some size I want it to switch from exponential to 
        //linear growth.
        long length = NextPowerOf2 (count);
        T[] source = new T[length];
        for (long i = 0; i < m_count; ++i)
          source[i] = m_source[i];
        m_source = source;
      }
    }

    public void ReverseInPlace ()
    {
      if (m_lock)
        throw new Exception (
          "Cannot write to an RCArray after it is locked.");
      Array.Reverse (m_source, 0, (int) m_count);
    }

    public int BinarySearch (T val)
    {
      int index = Array.BinarySearch<T> (m_source, 0, (int) m_count, val);
      if (index < 0)
        return ~index;
      else return index;
    }

    public bool Contains (T val)
    {
      return Array.IndexOf (m_source, val) > -1;
    }
  }

  public class RCArrayEnumerator<T> : IEnumerator<T>
  {
    protected int i = -1;
    protected RCArray<T> m_array;

    public RCArrayEnumerator (RCArray<T> array)
    {
      m_array = array;
    }

    public T Current
    {
      get { return (T) m_array[i]; }
    }

    public void Dispose () { }

    object System.Collections.IEnumerator.Current
    {
      get { return m_array[i]; }
    }

    public bool MoveNext ()
    {
      ++i;
      return i < m_array.Count;
    }

    public void Reset ()
    {
      i = -1;
    }
  }
}

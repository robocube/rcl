
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCArray<T> : IEnumerable<T>
  {
    public static readonly RCArray<T> Empty = new RCArray<T> (0);
    protected internal bool _lock = false;
    protected internal T[] _source;
    protected internal int _count;

    public RCArray (int capacity)
    {
      _source = new T[capacity];
      _count = 0;
    }

    public RCArray (params T[] source)
    {
      if (source == null) {
        throw new ArgumentNullException ("source");
      }
      _source = source;
      _count = source.Length;
    }

    public RCArray (ICollection<T> source)
    {
      _source = new T[source.Count];
      _count = source.Count;
      int i = 0;
      foreach (T val in source)
      {
        _source[i] = val;
        ++i;
      }
    }

    public RCArray (RCArray<T> source)
      : this (source.ToArray ()) {}

    public int Count
    {
      get { return _count; }
    }

    public T this[int i]
    {
      get { return _source[i]; }
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
      return Array.IndexOf<T> (_source, value, 0, _count);
    }

    public override string ToString ()
    {
      StringBuilder builder = new StringBuilder ();
      builder.Append ("(");
      builder.Append (typeof (T).Name);
      builder.Append (" ");
      builder.Append (Count);
      builder.Append ("/");
      builder.Append (_source.Length);
      builder.Append (")");
      builder.Append ("[");
      for (int i = 0; i < Count; ++i)
      {
        if (this[i] == null) {
          // The only time this should ever appear is when printing the context of a debug
          // exception.
          // It is not parsable. RCArrays with nulls should be caught in Lock (while
          // running the
          // debug build).
          builder.Append ("null");
        }
        else {
          builder.Append (this[i].ToString ());
        }
        if (i < Count - 1) {
          builder.Append (" ");
        }
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
      if (_lock) {
        throw new InvalidOperationException (
                "Attempted to Write to an RCArray after it was locked.");
      }
      Resize (values.Length, 0);
      for (int i = 0; i < values.Length; ++i)
      {
        _source[_count] = values[i];
        ++_count;
      }
    }

    public void Write (RCArray<T> values)
    {
      if (_lock) {
        throw new InvalidOperationException (
                "Attempted to Write to an RCArray after it was locked.");
      }
      Resize (values.Count, 0);
      for (int i = 0; i < values.Count; ++i)
      {
        _source[_count] = values[i];
        ++_count;
      }
    }

    public void Write (T value)
    {
      if (_lock) {
        throw new Exception ("Cannot write to an RCArray after it is locked.");
      }
      Resize (1, 0);
      _source[_count] = value;
      ++_count;
    }

    /// <summary>
    /// WITH GREAT POWER COMES GREAT RESPONSIBILITY
    /// </summary>
    public void Write (int i, T value)
    {
      if (_lock) {
        throw new Exception ("Cannot write to an RCArray after it is locked.");
      }
      Resize (1, 0);
      _source[i] = value;
    }

    public void RemoveAt (int i)
    {
      if (_lock) {
        throw new Exception ("Cannot write to an RCArray after it is locked");
      }
      for (; i < Count - 1; ++i)
      {
        _source[i] = _source[i + 1];
      }
      --_count;
    }

    public void Clear ()
    {
      if (_lock) {
        throw new Exception ("Cannot write to an RCArray after it is locked");
      }
      _count = 0;
    }

    public void Lock ()
    {
      RCAssert.ArrayHasNoNulls<T> (this);
      _lock = true;
    }

    public bool Locked ()
    {
      return _lock;
    }

    public void Resize (long size, long dups)
    {
      long count = _count + (size - dups);
      if (_source.Length < count) {
        // At some size I want it to switch from exponential to
        // linear growth.
        long length = NextPowerOf2 (count);
        T[] source = new T[length];
        for (long i = 0; i < _count; ++i) {
          source[i] = _source[i];
        }
        _source = source;
      }
    }

    public void ReverseInPlace ()
    {
      if (_lock) {
        throw new Exception (
                "Cannot use ReverseInPlace on an RCArray after it has been locked.");
      }
      Array.Reverse (_source, 0, (int) _count);
    }

    public void SortInPlace ()
    {
      if (_lock) {
        throw new Exception (
                "Cannot use SortInPlace on an RCArray<T> after it has been locked.");
      }
      Array.Sort (_source, 0, (int) _count);
    }

    public int BinarySearch (T val, out bool found)
    {
      // The index of the specified value in the specified array, if value is found;
      // otherwise, a negative number.If value is not found and value is less than one or
      // more
      // elements in array,
      // the negative number returned is the bitwise complement of the index of the first
      // element
      // that is larger than value.If
      // value is not found and value is greater than all elements in array,
      // the negative number returned is the bitwise complement of (the index of the last
      // element
      // plus 1).If this method is
      // called with a non - sorted array, the return value can be incorrect and a
      // negative number
      // could be returned,
      // even if value is present in array .
      // https://docs.microsoft.com/en-us/dotnet/api/system.array.binarysearch?view=netframework-4.7.2
      int index = Array.BinarySearch<T> (_source, 0, (int) _count, val);
      if (index < 0) {
        found = false;
        return ~index;
      }
      else {
        found = true;
        return index;
      }
    }

    public bool Contains (T val)
    {
      return Array.IndexOf (_source, val, 0, (int) _count) > -1;
    }
  }
}

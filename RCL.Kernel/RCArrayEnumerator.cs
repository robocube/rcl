
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCArrayEnumerator<T> : IEnumerator<T>
  {
    protected int i = -1;
    protected RCArray<T> _array;

    public RCArrayEnumerator (RCArray<T> array)
    {
      _array = array;
    }

    public T Current
    {
      get { return (T) _array[i]; }
    }

    public void Dispose () { }

    object System.Collections.IEnumerator.Current
    {
      get { return _array[i]; }
    }

    public bool MoveNext ()
    {
      ++i;
      return i < _array.Count;
    }

    public void Reset ()
    {
      i = -1;
    }
  }
}

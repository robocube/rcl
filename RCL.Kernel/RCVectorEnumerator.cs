
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCVectorEnumerator<T> : IEnumerator<T>
  {
    protected int i = -1;
    protected RCVector<T> _vector;

    public RCVectorEnumerator (RCVector<T> array)
    {
      _vector = array;
    }

    public T Current
    {
      get { return (T) _vector[i]; }
    }

    public void Dispose () { }

    object System.Collections.IEnumerator.Current
    {
      get { return _vector[i]; }
    }

    public bool MoveNext ()
    {
      ++i;
      return i < _vector.Count;
    }

    public void Reset ()
    {
      i = -1;
    }
  }
}

using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RankState<T> where T : IComparable<T>
  {
    protected static readonly Dictionary<char, object> _absmap = new Dictionary<char, object> ();
    protected RCVector<T> _data;
    protected AbsoluteValue<T> _abs;

    static RankState ()
    {
      _absmap['l'] = new LongAbs ();
      _absmap['d'] = new DoubleAbs ();
      _absmap['m'] = new DecimalAbs ();
    }

    public RankState (RCVector<T> data)
    {
      _data = data;
      object abs = null;
      _absmap.TryGetValue (data.TypeCode, out abs);
      if (abs == null)
      {
        _abs = new AbsoluteValue<T> ();
      }
      else
      {
        _abs = (AbsoluteValue<T>) abs;
      }
    }

    public virtual int Asc (long x, long y)
    {
      return _data[(int) x].CompareTo (_data[(int) y]);
    }

    public virtual int Desc (long x, long y)
    {
      return _data[(int) y].CompareTo (_data[(int) x]);
    }

    public virtual int AbsAsc (long x, long y)
    {
      return _abs.Abs (_data[(int) x]).CompareTo (_abs.Abs (_data[(int) y]));
    }

    public virtual int AbsDesc (long x, long y)
    {
      return _abs.Abs (_data[(int) y]).CompareTo (_abs.Abs (_data[(int) x]));
    }
  }
}
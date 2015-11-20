
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public abstract class Column<T> : ColumnBase
  {
    protected RCArray<T> m_data;
    protected RCArray<int> m_index;
    protected Dictionary<RCSymbolScalar, T> m_last;

    protected Column (Timeline timeline,
                      RCArray<int> index,
                      object data)
    {
      RCArray<T> original = (RCArray<T>) data;
      if (original.Locked ())
      {
        m_data = new RCArray<T> (original.Count);
        m_index = new RCArray<int> (original.Count);
        for (int i = 0; i < original.Count; ++i)
        {
          m_data.Write (original[i]);
          m_index.Write (index[i]);
        }
      }
      else
      {
        m_data = (RCArray<T>) data;
        m_index = index;
      }

      if (timeline.Has ("S"))
      {
        m_last = new Dictionary<RCSymbolScalar, T> ();
        for (int i = 0; i < m_data.Count; ++i)
        {
          RCSymbolScalar key = timeline.Symbol[index[i]];
          T val = m_data[i];
          if (m_last != null)
            m_last[key] = val;
        }
      }
    }

    protected Column (Timeline timeline)
    {
      //This ctor is used while writing data into the blackboard.
      //Always instantiate m_last because these cubes will always have a symbol column.
      m_data = new RCArray<T> ();
      m_index = new RCArray<int> ();
      if (timeline.Has ("S"))
        m_last = new Dictionary<RCSymbolScalar, T> ();
    }

    public override bool Write (RCSymbolScalar key, int index, object box, bool force)
    {
      T val = (T) box;
      T old;
      //The key may be null if there is no timeline.
      if (m_last != null && key != null)
      {
        if (!force && m_last.TryGetValue (key, out old))
        {
          if (Comparer<T>.Default.Compare (val, old) == 0)
          {
            return false;
          }
        }
        m_last[key] = val;
      }
      m_data.Write (val);
      m_index.Write (index);
      return true;
    }

    public override Type GetElementType ()
    {
      return typeof (T);
    }

    public override void Lock ()
    {
      m_data.Lock ();
      m_index.Lock ();
    }

    public override void ReverseInPlace (int tlcount)
    {
      m_data.ReverseInPlace ();
      m_index.ReverseInPlace ();
      int last = tlcount - 1;
      for (int i = 0; i < m_index.Count; ++i)
      {
        m_index.Write (i, Math.Abs (m_index[i] - last));
      }
    }

    public override void Accept (string name, Visitor visitor, int i)
    {
      visitor.VisitScalar<T> (name, this, i);
    }

    public override void AcceptNull (string name, Visitor visitor, int i)
    {
      visitor.VisitNull<T> (name, this, i);
    }

    public override bool BoxLast (RCSymbolScalar key, out object box)
    {
      T val;
      if (Last (key, out val))
      {
        box = val;
        return true;
      }
      else
      {
        box = null;
        return false;
      }
    }

    public bool Last (RCSymbolScalar key, out T val)
    {
      return m_last.TryGetValue (key, out val);
    }

    public override object BoxCell (int i) { return m_data[i]; }
    public override object Array { get { return m_data; } }
    public override RCArray<int> Index { get { return m_index; } }
    public RCArray<T> Data { get { return m_data; } }
    public override int Count { get { return m_data.Count; } }
  }
}
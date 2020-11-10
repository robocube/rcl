
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public abstract class Column<T> : ColumnBase
  {
    protected RCArray<T> _data;
    protected RCArray<int> _index;
    protected Dictionary<RCSymbolScalar, T> _last;
    protected int _tlcount;

    protected Column (Timeline timeline, RCArray<int> index, object data)
    {
      _tlcount = timeline.Count;
      RCArray<T> original = (RCArray<T>)data;
      if (original.Locked ()) {
        _data = new RCArray<T> (original.Count);
        _index = new RCArray<int> (original.Count);
        for (int i = 0; i < original.Count; ++i)
        {
          _data.Write (original[i]);
          _index.Write (index[i]);
        }
      }
      else {
        _data = (RCArray<T>)data;
        _index = index;
      }
      if (timeline.Has ("S")) {
        _last = new Dictionary<RCSymbolScalar, T> ();
        for (int i = 0; i < _data.Count; ++i)
        {
          RCSymbolScalar key = timeline.Symbol[index[i]];
          T val = _data[i];
          if (_last != null) {
            _last[key] = val;
          }
        }
      }
    }

    protected Column (Timeline timeline)
    {
      _tlcount = timeline.Count;
      // This ctor is used while writing data into the blackboard.
      // Always instantiate _last because these cubes will always have a symbol column.
      _data = new RCArray<T> ();
      _index = new RCArray<int> ();
      if (timeline.Has ("S")) {
        _last = new Dictionary<RCSymbolScalar, T> ();
      }
    }

    public override bool Write (RCSymbolScalar key, int index, object box, bool force)
    {
      T val = (T) box;
      T old;
      // The key may be null if there is no timeline.
      if (_last != null && key != null) {
        if (!force && _last.TryGetValue (key, out old)) {
          if (Comparer<T>.Default.Compare (val, old) == 0) {
            return false;
          }
        }
        _last[key] = val;
      }
      if (_index.Count == 0 || _index[_index.Count - 1] < index) {
        _data.Write (val);
        _index.Write (index);
      }
      else {
        // Do we need to look for this?
        bool found;
        int existing = _index.BinarySearch (index, out found);
        if (existing < 0) {
          throw new Exception ("Invalid index " + index);
        }
        _data.Write (existing, val);
      }
      return true;
    }

    public override Type GetElementType ()
    {
      return typeof (T);
    }

    public override void Lock ()
    {
      _data.Lock ();
      _index.Lock ();
    }

    public override void ReverseInPlace (int tlcount)
    {
      _data.ReverseInPlace ();
      _index.ReverseInPlace ();
      int last = tlcount - 1;
      for (int i = 0; i < _index.Count; ++i)
      {
        _index.Write (i, Math.Abs (_index[i] - last));
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
      if (Last (key, out val)) {
        box = val;
        return true;
      }
      else {
        box = null;
        return false;
      }
    }

    public bool Last (RCSymbolScalar key, out T val)
    {
      bool result = _last.TryGetValue (key, out val);
      return result;
    }

    public override RCBlock Flatpack ()
    {
      RCBlock column = RCBlock.Empty;
      column = new RCBlock (column, "index", ":", RCVectorBase.FromArray (Index));
      column = new RCBlock (column, "array", ":", RCVectorBase.FromArray (Array));
      RCArray<RCSymbolScalar> keys = new RCArray<RCSymbolScalar> ();
      RCArray<T> vals = new RCArray<T> ();
      if (_last != null) {
        foreach (KeyValuePair<RCSymbolScalar, T> kv in _last) {
          keys.Write (kv.Key);
          vals.Write (kv.Value);
        }
      }
      column = new RCBlock (column, "keys", ":", RCVectorBase.FromArray (keys));
      column = new RCBlock (column, "vals", ":", RCVectorBase.FromArray (vals));
      return column;
    }

    public override bool Delete (RCSymbolScalar key)
    {
      return _last.Remove (key);
    }

    public override object BoxCell (int i) {
      return _data[i];
    }
    public override object Array {
      get { return _data; }
    }
    public override RCArray<int> Index {
      get { return _index; }
    }
    public RCArray<T> Data {
      get { return _data; }
    }
    public override int Count {
      get { return _data.Count; }
    }
  }
}


using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  /// <summary>
  /// A timeline is a recording of symbols and times where events have
  /// occured, without any reference to the events themselves.
  /// </summary>
  public class Timeline
  {
    /// <summary>
    /// (G)lobal provides the index of the row that this cube was sourced from.
    /// This allows you to read a small slice from a big cube and then find the
    /// source rows again.
    /// </summary>
    public readonly RCArray<long> Global;

    /// <summary>
    /// The (T)ime at the point the event was recorded.
    /// Mostly useful for display.
    /// It is also used when merging two cubes that don't share a common source.
    /// </summary>
    public readonly RCArray<long> Event;

    /// <summary>
    /// Multiple rows in the timeline can be grouped together to represent a single,
    /// discrete event.  Consecutive rows having the same value in E will be treated
    /// like a single event.
    /// </summary>
    public readonly RCArray<RCTimeScalar> Time;
  
    /// <summary>
    /// The (S)ymbol on the event.
    /// Symbols are intended to represent entities in your application.
    /// Including symbols in the timeline allows you perform the calcs for many
    /// different entities at once.
    /// </summary>
    public readonly RCArray<RCSymbolScalar> Symbol;

    //The list of columns on the axis.
    public readonly RCArray<string> Colset;

    protected int m_count = 0;

    public Timeline (RCArray<long> g, 
                     RCArray<long> e,
                     RCArray<RCTimeScalar> t,
                     RCArray<RCSymbolScalar> s)
    {
      Colset = new RCArray<string> (4);
      if (g != null)
      {
        Colset.Write ("G");
        Global = g;
        m_count = Global.Count;
      }
      if (e != null)
      {
        Colset.Write ("E");
        Event = e;
        m_count = Event.Count;
      }
      if (t != null)
      {
        Colset.Write ("T");
        Time = t;
        m_count = Time.Count;
      }
      if (s != null)
      {
        Colset.Write ("S");
        Symbol = s;
        m_count = Symbol.Count;
      }
    }

    public Timeline (RCArray<RCSymbolScalar> s)
    {
      Colset = new RCArray<string> (1);
      Colset.Write ("S");
      Symbol = s;
      m_count = Symbol.Count;
    }

    public Timeline (params string[] cols) 
      :this (new RCArray<string> (cols)) {}

    public Timeline (RCArray<string> cols)
    {
      Colset = cols;
      if (Colset.Contains ("G"))
      {
        Global = new RCArray<long> ();
      }
      if (Colset.Contains ("E"))
      {
        Event = new RCArray<long> ();
      }
      if (Colset.Contains ("T"))
      {
        Time = new RCArray<RCTimeScalar> ();
      }
      if (Colset.Contains ("S"))
      {
        Symbol = new RCArray<RCSymbolScalar> ();
      }
    }

    public Timeline (int count)
    {
      Colset = new RCArray<string> (0);
      m_count = count;
    }

    public long TimeAt (int i)
    {
      if (Event == null)
      {
        return i;
      }
      return Event[i];
    }

    public RCSymbolScalar SymbolAt (int i)
    {
      if (Symbol == null)
      {
        return null;
      }
      return Symbol[i];
    }
  
    public int Count
    {
      get
      {
        return m_count;
      }
      set
      {
        if (Colset.Locked ())
        {
          throw new Exception ("Tried to modify Count after cube was locked.");
        }
        m_count = value;
      }
    }

    public void Lock ()
    {
      if (Event != null)
      {
        Event.Lock ();
      }
      if (Time != null)
      {
        Time.Lock (); 
      }
      if (Symbol != null)
      {
        Symbol.Lock ();
      }
      if (Global != null)
      {
        Global.Lock ();
      }
      Colset.Lock ();
    }

    public bool Has (string name)
    {
      return Colset.Contains (name);
    }

    public bool Exists
    {
      get { return Colset.Count > 0; }
    }

    public void Write ()
    {
      ++m_count;
    }

    public void Write (RCSymbolScalar s)
    {
      Write (-1, -1, new RCTimeScalar (new DateTime (0), RCTimeType.Timestamp), s);
    }

    public void Write (long e, RCSymbolScalar s)
    {
      //You will get an exception if these arrays have been locked from writing.
      if (Event != null)
      {
        Event.Write (e);
      }
      if (Symbol != null)
      {
        Symbol.Write (s);
      }
      ++m_count;
    }

    public void Write (RCTimeScalar t, RCSymbolScalar s)
    {
      //You will get an exception if these arrays have been locked from writing.
      if (Time != null)
      {
        Time.Write (t);
      }
      if (Symbol != null)
      {
        Symbol.Write (s);
      }
      ++m_count;
    }

    public void Write (long g, long e, RCTimeScalar t, RCSymbolScalar s)
    {
      if (Global != null)
      {
        Global.Write (g);
      }
      if (Event != null)
      {
        Event.Write (e);
      }
      if (Time != null)
      {
        Time.Write (t);
      }
      if (Symbol != null)
      {
        Symbol.Write (s);
      }
      ++m_count;
    }

    public Timeline Match ()
    {
      return new Timeline (new RCArray<string> (Colset));
    }

    internal void ReverseInPlace ()
    {
      if (Global != null)
      {
        Global.ReverseInPlace ();
      }
      if (Event != null)
      {
        Event.ReverseInPlace ();
      }
      if (Time != null)
      {
        Time.ReverseInPlace ();
      }
      if (Symbol != null)
      {
        Symbol.ReverseInPlace ();
      }
    }

    public int ColCount
    {
      get { return Colset.Count; }
    }
  }
}
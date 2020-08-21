using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Variable
  {
    protected internal object m_lock = new object ();
    protected internal Dictionary<object, Dictionary<RCSymbolScalar, RCValue>> m_sections;

    public Variable ()
    {
      m_sections = new Dictionary<object, Dictionary<RCSymbolScalar, RCValue>> ();
    }

    protected Dictionary<RCSymbolScalar, RCValue> GetSection (RCSymbol symbol)
    {
      // This assumes that all symbols in symbol have the same first part!
      object key = symbol[0].Part (0);
      Dictionary<RCSymbolScalar, RCValue> section;
      if (!m_sections.TryGetValue (key, out section)) {
        section = new Dictionary<RCSymbolScalar, RCValue> ();
        m_sections[key] = section;
      }
      return section;
    }

    [RCVerb ("getm")]
    public void Getm (RCRunner runner, RCClosure closure, RCSymbol key)
    {
      RCValue result;
      lock (m_lock)
      {
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (key);
        if (!store.TryGetValue (key[0], out result)) {
          // result = RCBlock.Empty;
          throw new RCException (closure,
                                 RCErrors.Varname,
                                 "No such variable: " + key.ToString ());
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("hasm")]
    public void Hasm (RCRunner runner, RCClosure closure, RCSymbol key)
    {
      RCBoolean result;
      lock (m_lock)
      {
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (key);
        if (!store.ContainsKey (key[0])) {
          result = RCBoolean.False;
        }
        else {
          result = RCBoolean.True;
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("getms")]
    public void Getms (RCRunner runner, RCClosure closure, RCSymbol key)
    {
      RCBlock result = RCBlock.Empty;
      lock (m_lock)
      {
        // Stick to the rule that all keys must start on the same page.
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (key);
        for (int i = 0; i < key.Count; ++i)
        {
          RCValue val;
          if (!store.TryGetValue (key[i], out val)) {
            val = RCBlock.Empty;
          }
          result = new RCBlock (result, "", ":", val);
        }
      }
      runner.Yield (closure, result);
    }

    /*
       [RCVerb ("getms")]
       public void Getms (RCRunner runner, RCClosure closure, RCCube right)
       {
       if (right.Count == 0)
       {
        runner.Yield (closure, right);
        return;
       }
       RCBlock result = RCBlock.Empty;
       lock (m_lock)
       {
        RCSymbol vars = (RCSymbol) right.GetSimpleVector (0);
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (vars);
        for (int i = 0; i < vars.Count; ++i)
        {
          RCValue val;
          if (!store.TryGetValue (vars[i], out val))
            val = RCBlock.Empty;
          result = new RCBlock (result, "", ":", val);
        }
       }
       runner.Yield (closure, result);
       }
     */

    [RCVerb ("putm")]
    public void Putm (RCRunner runner, RCClosure closure, RCSymbol key, object val)
    {
      lock (m_lock)
      {
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (key);
        store[key[0]] = (RCValue) val;
      }
      runner.Yield (closure, key);
    }

    [RCVerb ("putms")]
    public void Putms (RCRunner runner, RCClosure closure, RCSymbol keys, RCBlock values)
    {
      lock (m_lock)
      {
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (keys);
        for (int i = 0; i < values.Count; ++i)
        {
          store[keys[i]] = values.Get (i);
        }
      }
      runner.Yield (closure, keys);
    }

    [RCVerb ("delm")]
    public void Deletem (RCRunner runner, RCClosure closure, RCSymbol key)
    {
      lock (m_lock)
      {
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (key);
        store.Remove (key[0]);
      }
      runner.Yield (closure, new RCSymbol (key[0]));
    }

    [RCVerb ("listm")]
    public void List (RCRunner runner, RCClosure closure, RCSymbol key)
    {
      RCSymbolScalar[] array;
      lock (m_lock)
      {
        Dictionary<RCSymbolScalar, RCValue> store = GetSection (key);
        array = new RCSymbolScalar[store.Keys.Count];
        store.Keys.CopyTo (array, 0);
      }
      runner.Yield (closure, new RCSymbol (array));
    }

    [RCVerb ("clearm")]
    public void Clear (RCRunner runner, RCClosure closure, RCSymbol key)
    {
      lock (m_lock)
      {
        for (int i = 0; i < key.Count; ++i)
        {
          m_sections.Remove (key[i].Part (0));
        }
      }
      runner.Yield (closure, key);
    }
  }
}

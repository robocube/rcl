
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCBot
  {
    public readonly long Id;
    protected long m_handle = 0;
    protected Dictionary<long, object> m_descriptors = new Dictionary<long, object> ();
    protected Dictionary<Type, object> m_modules = new Dictionary<Type, object> ();

    public RCBot (RCRunner runner, long id)
    {
      Id = id;
      runner.Activator.InjectState (this);
    }

    //Deprecated: Only used by Tcp api.
    public long New ()
    {
      return m_handle = Interlocked.Increment (ref m_handle);
    }

    //Deprecated: Only used by Tcp api.
    public object Get (long handle)
    {
      return m_descriptors[handle];
    }

    //Deprecated: Only used by Tcp api.
    public void Put (long handle, object descriptor)
    {
      m_descriptors[handle] = descriptor;
    }

    //Deprecated: Only used by Tcp api.
    public void Delete (long handle)
    {
      m_descriptors.Remove (handle);
    }

    public void PutModule (Type type)
    {
      ConstructorInfo ctor = type.GetConstructor (new Type[]{});
      object module = ctor.Invoke (new object[]{});
      m_modules.Add (type, module);
    }

    public object GetModule (Type type)
    {
      return m_modules[type];
    }

    public void FiberDone (RCRunner runner, long bot, long fiber, RCValue result)
    {
      Fiber module = (Fiber) GetModule (typeof (Fiber));
      module.FiberDone (runner, bot, fiber, result);
    }

    public void ChangeFiberState (long fiber, string state)
    {
      Fiber module = (Fiber) GetModule (typeof (Fiber));
      module.ChangeFiberState (fiber, state);
    }

    //This does not really implement the dispose pattern it just
    //calls dispose on the objects that do.
    public void Dispose ()
    {
      foreach (KeyValuePair <Type, object> kv in m_modules)
      {
        IDisposable module = kv.Value as IDisposable;
        if (module != null)
        {
          module.Dispose ();
        }
      }
    }
  }
}
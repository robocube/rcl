
using System.Diagnostics;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCLog
  {
    protected List<RCLogger> m_all = new List<RCLogger> ();
    protected Dictionary<string, List<RCLogger>> m_loggers = new Dictionary<string, List<RCLogger>> ();
    protected List<RCLogger> m_wild = new List<RCLogger> ();
    protected object m_lock = new object ();

    public RCLog (params RCLogger[] loggers)
    {
      for (int i = 0; i < loggers.Length; ++i)
      {
        m_all.Add (loggers[i]);
        //This is to support the idea of making certain loggers optional 
        //as you are constructing the RCLog. See Program.cs.
        if (loggers[i] == null) 
        {
          continue;
        }
        RCArray<string> type = loggers[i].Types ();
        for (int j = 0; j < type.Count; ++j)
        {
          if (type[j] == "*")
          {
            m_wild.Add (loggers[i]);
          }
          else
          {
            List<RCLogger> typeList;
            if (!m_loggers.TryGetValue (type[j], out typeList))
            {
              typeList = new List<RCLogger> ();
              m_loggers.Add (type[j], typeList);
            }
            typeList.Add (loggers[i]);
          }
        }
      }
    }

    public void SetVerbosity (RCOutput output)
    {
      lock (m_lock)
      {
        foreach (RCLogger logger in m_all)
        {
          logger.SetVerbosity (output);
        }
      }
    }

    public virtual void Record (RCRunner runner, 
                                RCClosure closure, 
                                string type, 
                                long instance, 
                                string state, 
                                object info)
    {
      lock (m_lock)
      {
        List<RCLogger> typeList;
        if (m_loggers.TryGetValue (type, out typeList))
        {
          for (int i = 0; i < typeList.Count; ++i)
          {
            typeList[i].Record (runner, closure, type, instance, state, info);
          }
        }
        else if (m_loggers.TryGetValue (type + ":" + state, out typeList))
        {
          for (int i = 0; i < typeList.Count; ++i)
          {
            typeList[i].Record (runner, closure, type, instance, state, info);
          }
        }
        for (int i = 0; i < m_wild.Count; ++i)
        {
          m_wild[i].Record (runner, closure, type, instance, state, info);
        }
      }
    }

    public virtual void RecordDoc (RCRunner runner, 
                                   RCClosure closure, 
                                   string type, 
                                   long instance, 
                                   string state, 
                                   object info)
    {
      lock (m_lock)
      {
        List<RCLogger> typeList;
        if (m_loggers.TryGetValue (type, out typeList))
        {
          for (int i = 0; i < typeList.Count; ++i)
          {
            typeList[i].RecordDoc (runner, closure, type, instance, state, info);
          }
        }
        else if (m_loggers.TryGetValue (type + ":" + state, out typeList))
        {
          for (int i = 0; i < typeList.Count; ++i)
          {
            typeList[i].Record (runner, closure, type, instance, state, info);
          }
        }
        for (int i = 0; i < m_wild.Count; ++i)
        {
          m_wild[i].RecordDoc (runner, closure, type, instance, state, info);
        }
      }
    }

    /*
    public virtual void Write (string type, string message)
    {
      lock (m_lock)
      {
        List<RCLogger> typeList;
        if (m_loggers.TryGetValue (type, out typeList))
        {
          for (int i = 0; i < typeList.Count; ++i)
          {
            typeList[i].Write (type, message);
          }
        }
        for (int i = 0; i < m_wild.Count; ++i)
        {
          m_wild[i].Write (type, message);
        }
      }
    }

    public virtual void WriteLine (string type, string line)
    {
      lock (m_lock)
      {
        List<RCLogger> typeList;
        if (m_loggers.TryGetValue (type, out typeList))
        {
          for (int i = 0; i < typeList.Count; ++i)
          {
            typeList[i].WriteLine (type, line);
          }
        }
        for (int i = 0; i < m_wild.Count; ++i)
        {
          m_wild[i].WriteLine (type, line);
        }
      }
    }
    */
  }
}

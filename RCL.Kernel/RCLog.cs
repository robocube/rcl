
using System.Diagnostics;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCLog
  {
    protected RCLogger m_logger;

    public RCLog ()
    {
      m_logger = new RCLogger (true);
    }

    public RCLog (RCLogger logger)
    {
      m_logger = logger;
    }

    public void SetVerbosity (RCOutput output)
    {
      m_logger.SetVerbosity (output);
    }

    public virtual void Record (RCRunner runner, 
                                RCClosure closure, 
                                string type, 
                                long instance, 
                                string state, 
                                object info)
    {
      RCLogger.RecordFilter (runner, closure, type, instance, state, info, false);
    }

    public virtual void RecordDoc (RCRunner runner,
                                   RCClosure closure,
                                   string type,
                                   long instance,
                                   string state,
                                   object info)
    {
      RCLogger.RecordFilter (runner, closure, type, instance, state, info, true);
    }

    public virtual void Record (RCRunner runner,
                                RCClosure closure,
                                string type,
                                long instance,
                                string state,
                                object info,
                                bool forceDoc)
    {
      RCLogger.RecordFilter (runner, closure, type, instance, state, info, forceDoc);
    }
  }
}


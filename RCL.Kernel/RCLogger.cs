namespace RCL.Kernel
{
  public class RCLogger
  {
    public virtual void Record (RCRunner runner, 
                                RCClosure closure, 
                                string type, 
                                long instance, 
                                string state, 
                                object info) {}

    public virtual void Output (RCOutput level) {}

    public virtual void SetVerbosity (RCOutput level) {}

    public virtual RCArray<string> Types ()
    {
      return new RCArray<string> ("*");
    }
  }
}


namespace RCL.Kernel
{
  public class RCLogger
  {
    public virtual void Record (
      RCRunner runner, RCClosure closure, string type, long instance, string state, object info)
    {

    }

    public virtual void RecordDoc (
      RCRunner runner, RCClosure closure, string type, long instance, string state, object info)
    {

    }

    public virtual void Write (string type, string text)
    {
      
    }

    public virtual void WriteLine (string type, string line)
    {
      
    }

    public virtual void Output (RCOutput level)
    {

    }

    public virtual RCArray<string> Types ()
    {
      return new RCArray<string> ("*");
    }
  }
}


namespace RCL.Kernel
{
  public interface RCRefable
  {
    RCValue Get (RCArray<string> name, RCArray<RCBlock> context);
    RCValue Get (string[] name, RCArray<RCBlock> context);
    RCValue Get (string name);
    RCValue Get (long index);
  }
}
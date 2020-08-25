
using System;

namespace RCL.Kernel
{
  public class LongAbs : AbsoluteValue<long>
  {
    public override long Abs (long val)
    {
      return Math.Abs (val);
    }
  }
}
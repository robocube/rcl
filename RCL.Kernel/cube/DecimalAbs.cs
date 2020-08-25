
using System;

namespace RCL.Kernel
{
  public class DecimalAbs : AbsoluteValue<decimal>
  {
    public override decimal Abs (decimal val)
    {
      return Math.Abs (val);
    }
  }
}

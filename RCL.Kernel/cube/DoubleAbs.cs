
using System;

namespace RCL.Kernel
{
  public class DoubleAbs : AbsoluteValue<double>
  {
    public override double Abs (double val)
    {
      return Math.Abs (val);
    }
  }
}

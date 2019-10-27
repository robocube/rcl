using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public abstract class Context<T>
  {
    public abstract void Init (RCArray<T> left);
  }
}

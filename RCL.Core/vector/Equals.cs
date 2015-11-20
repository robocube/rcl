
using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Equals
  {
    [RCVerb ("=")]
    public void EvalEquals (RCRunner runner, RCClosure closure, object left, object right)
    {
      runner.Yield (closure, new RCBoolean (left.Equals (right)));
    }
  }
}

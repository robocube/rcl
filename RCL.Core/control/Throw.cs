
using System;
using System.Collections.Generic;
using System.Threading;
using RCL.Kernel;

namespace RCL.Core
{
  public class Throw
  {
    [RCVerb ("assert")]
    public void EvalAssert (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        if (!right[i])
        {
          throw new Exception ("Element " + i.ToString () + " was false");
        }
      }
      runner.Yield (closure, new RCBoolean (true));
    }

    [RCVerb ("assert")]
    public void EvalAssert (RCRunner runner, RCClosure closure, object left, object right)
    {
      if (!left.Equals (right))
      {
        throw new Exception ("Expected: " + right.ToString () +
                             ", Actual: " + left.ToString ());
      }
      runner.Yield (closure, new RCBoolean (true));
    }

    [RCVerb ("fail")]
    public void EvalFail (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      runner.Finish (closure, new Exception (right[0]), left[0]);
    }
  }
}

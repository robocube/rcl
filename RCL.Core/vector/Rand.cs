using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using RCL.Kernel;

namespace RCL.Core
{
  public class Rand
  {
    protected static Random _random = new Random ();

    [RCVerb ("randomd")]
    public void EvalRandomd (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      int seed = (int) left[0];
      Random random = new Random (seed);
      long count = right[0];
      double[] result = new double[count];
      for (long i = 0; i < count; ++i)
      {
        result[i] = random.NextDouble ();
      }
      runner.Yield (closure, new RCDouble (result));
    }

    [RCVerb ("randomd")]
    public void EvalRandomd (RCRunner runner, RCClosure closure, RCLong right)
    {
      long count = right[0];
      double[] result = new double[count];
      lock (_random)
      {
        for (long i = 0; i < count; ++i)
        {
          result[i] = _random.NextDouble ();
        }
      }
      runner.Yield (closure, new RCDouble (result));
    }

    [RCVerb ("randoml")]
    public void EvalRandoml (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      int seed = (int) left[0];
      Random random = new Random (seed);
      long count = right[0];
      int min = (int) right[1];
      int max = (int) right[2];
      long[] result = new long[count];
      for (long i = 0; i < count; ++i)
      {
        result[i] = random.Next (min, max);
      }
      runner.Yield (closure, new RCLong (result));
    }

    [RCVerb ("randoml")]
    public void EvalRandoml (RCRunner runner, RCClosure closure, RCLong right)
    {
      // Make sure we use a different seed each time this operator is called.
      // int seed = Interlocked.Increment(ref _seed);
      // Random random = new Random(seed);

      long count = right[0];
      int min = (int) right[1];
      int max = (int) right[2];
      long[] result = new long[count];
      lock (_random)
      {
        for (long i = 0; i < count; ++i)
        {
          result[i] = _random.Next (min, max);
        }
      }
      runner.Yield (closure, new RCLong (result));
    }
  }
}

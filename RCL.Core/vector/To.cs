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
  public class To
  {
    [RCVerb ("to")]
    public void EvalTo (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      byte[] result = new byte[right[0] - left[0] + 1];
      for (byte i = 0; i < result.Length; ++i)
      {
        result[i] = (byte) (left[0] + i);
      }
      runner.Yield (closure, new RCByte (result));
    }

    [RCVerb ("to")]
    public void EvalTo (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      long[] result = new long[right[0] - left[0] + 1];
      for (long i = 0; i < result.Length; ++i)
      {
        result[i] = left[0] + i;
      }
      runner.Yield (closure, new RCLong (result));
    }

    [RCVerb ("to")]
    public void EvalTo (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      double[] result = new double[(int) right[0] - (int) left[0] + 1];
      for (double i = 0; i < result.Length; ++i)
      {
        result[(int) i] = left[0] + i;
      }
      runner.Yield (closure, new RCDouble (result));
    }

    [RCVerb ("to")]
    public void EvalTo (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      decimal[] result = new decimal[(int) right[0] - (int) left[0] + 1];
      for (decimal i = 0; i < result.Length; ++i)
      {
        result[(int) i] = left[0] + i;
      }
      runner.Yield (closure, new RCDecimal (result));
    }
  }
}
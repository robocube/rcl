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
  public class Now
  {
    [RCVerb ("now")]
    public void EvalNow (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCLong (DateTime.Now.Ticks));
    }
  }
}
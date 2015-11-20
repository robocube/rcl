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
  public class First
  {
    [RCVerb ("first")]
    public void EvalFirst (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, right.Get (0));
    }
  }
}
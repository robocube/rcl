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
  public class Set
  {
    [RCVerb ("set")]
    public virtual void EvalSet (
      RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      //We are going to need a faster version of this at some point.
      RCBlock result = RCBlock.Empty;
      HashSet<string> handled = new HashSet<string> ();
      for (int i = 0; i < left.Count; ++i)
      {
        RCBlock lname = left.GetName (i);
        RCBlock rname;
        if (lname.Name == "")
        {
          rname = right.GetName (i);
        }
        else
        {
          rname = right.GetName (lname.Name);
        }
        if (rname != null)
        {
          result = new RCBlock (
            result, lname.Name, lname.Evaluator, rname.Value);
          handled.Add (rname.Name);
        }
        else
        {
          result = new RCBlock (
            result, lname.Name, lname.Evaluator, lname.Value);
        }
      }
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock rname = right.GetName (i);
        if (!handled.Contains (rname.Name))
        {
          result = new RCBlock (
            result, rname.Name, rname.Evaluator, rname.Value);
        }
      }
      runner.Yield (closure, result);
    }
  }
}

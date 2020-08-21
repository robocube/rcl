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
    public virtual void EvalSet (RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      // We are going to need a faster version of this at some point.
      RCBlock result = RCBlock.Empty;
      HashSet<string> handled = new HashSet<string> ();
      bool leftHasNames = left.HasNamedVariables ();
      for (int i = 0; i < left.Count; ++i)
      {
        RCBlock lname = left.GetName (i);
        RCBlock rname;
        if (!leftHasNames) {
          // In this case left and right must have equal count or else BOOM.
          rname = right.GetName (i);
        }
        else {
          rname = right.GetName (lname.Name);
        }
        // rname.Value is null in the case of the empty block
        if (rname != null && rname.Value != null) {
          result = new RCBlock (result, rname.Name, rname.Evaluator, rname.Value);
          handled.Add (rname.Name);
        }
        else {
          result = new RCBlock (result, lname.Name, lname.Evaluator, lname.Value);
        }
      }
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock rname = right.GetName (i);
        if (!handled.Contains (rname.Name)) {
          result = new RCBlock (result, rname.Name, rname.Evaluator, rname.Value);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("merge")]
    public virtual void EvalMerge (RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      runner.Yield (closure, InnerMerge (left, right));
    }

    protected RCBlock InnerMerge (RCBlock left, RCBlock right)
    {
      RCBlock result = RCBlock.Empty;
      HashSet<string> handled = new HashSet<string> ();
      for (int i = 0; i < left.Count; ++i)
      {
        RCBlock lname = left.GetName (i);
        RCBlock rname;
        if (lname.Name == "") {
          rname = right.GetName (i);
        }
        else {
          rname = right.GetName (lname.Name);
        }
        // rname.Value is null in the case of the empty block
        if (rname != null && rname.Value != null) {
          if (rname.Value is RCBlock) {
            if (lname.Value is RCBlock) {
              RCBlock lblock = (RCBlock) lname.Value;
              RCBlock rblock = (RCBlock) rname.Value;
              result = new RCBlock (result,
                                    rname.Name,
                                    rname.Evaluator,
                                    InnerMerge (lblock, rblock));
              handled.Add (rname.Name);
            }
            else if (lname.Value is RCOperator) {
              result = new RCBlock (result, rname.Name, rname.Evaluator, rname.Value);
              handled.Add (rname.Name);
            }
            else {
              throw new Exception (rname.Name + " must be a block. Was type " +
                                   lname.Value.TypeName + ".");
            }
          }
          else {
            result = new RCBlock (result, rname.Name, rname.Evaluator, rname.Value);
            handled.Add (rname.Name);
          }
        }
        else {
          result = new RCBlock (result, lname.Name, lname.Evaluator, lname.Value);
        }
      }
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock rname = right.GetName (i);
        if (!handled.Contains (rname.Name)) {
          result = new RCBlock (result, rname.Name, rname.Evaluator, rname.Value);
        }
      }
      return result;
    }
  }
}

using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Overload
  {
    public readonly Delegate VectorOp;
    public readonly Delegate ScalarOp;

    public Overload (Delegate vectorop, Delegate scalarop)
    {
      VectorOp = vectorop;
      ScalarOp = scalarop;
    }

    public object Invoke (object left, object right)
    {
      //I think we can do some more magic to make this a static call.
      return VectorOp.DynamicInvoke (left, right, ScalarOp);
    }

    public object Invoke (object right)
    {
      //I think we can do some more magic to make this a static call.
      return VectorOp.DynamicInvoke (right, ScalarOp);
    }
  }
}
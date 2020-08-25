
using System;

namespace RCL.Kernel
{
  /// <summary>
  /// A RCUserOperator is sort of a cross between a reference and an operator.
  /// It is a reference to an expression or value on the stack that
  /// will be treated just like an operator.
  /// </summary>
  public class RCUserOperator : RCOperator
  {
    protected internal RCReference _reference;

    public RCUserOperator (RCReference reference)
    {
      if (reference == null)
      {
        throw new ArgumentNullException ("reference");
      }
      _reference = reference;
    }

    public override void EvalOperator (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEvalUserOp (runner, closure, this);
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      return RCL.Kernel.Eval.DoIsLastCall (closure, arg, this);
    }

    public override bool IsBeforeLastCall (RCClosure closure)
    {
      return RCL.Kernel.Eval.DoIsBeforeLastCall (closure, this);
    }
  }
}
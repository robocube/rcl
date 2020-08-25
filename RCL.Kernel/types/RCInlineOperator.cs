
using System;
using System.Text;

namespace RCL.Kernel
{
  /// <summary>
  /// An inline operator is basically a lambda expression; an operator embedded in a
  /// larger expression without being given a name. For the time being this capability is
  /// restricted to embedded blocks (by the parser). But we might open it up to
  /// expressions or other values if the need arises.
  /// I am thinking of getting rid of these.
  /// </summary>
  public class RCInlineOperator : RCOperator
  {
    protected internal RCValue _code;

    public RCInlineOperator (RCValue code)
    {
      if (code == null)
      {
        throw new ArgumentNullException ("code");
      }
      _code = code;
    }

    public override void EvalOperator (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEvalInline (runner, closure, this);
    }

    public override void BodyToString (StringBuilder builder, RCFormat args, int level)
    {
      _code.Format (builder, args, level);
    }
  }
}

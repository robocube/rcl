
using System;
using System.Reflection;

namespace RCL.Kernel
{
  //I think creating a comprehensive set of exceptions are error messages is appropriate.
  public class RCException : Exception
  {
    public static RCException Overload (
      RCClosure closure, RCOperator op, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " + right.TypeName + ".";
      message += "(" + right.ToString () + ")";
      return new RCException (closure, message);
    }

    public static RCException Overload (
      RCClosure closure, RCOperator op, RCValue left, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " +
        left.TypeName + " and " + right.TypeName + ".";
      message += "(" + left.ToString () + "," + right.ToString () + ")";
      return new RCException (closure, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object right)
    {
      string message = "Operator " + op +
        " can not receive arguments of type " + right.GetType ().Name + ".";
      message += "(" + right.ToString () + ")";
      return new RCException (closure, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object left, object right)
    {
      string message = "Operator " + op +
        " can not receive arguments of type " +
        left.GetType ().Name + " and " + right.GetType ().Name + ".";
      message += "(" + left.ToString () + "," + right.ToString () + ")";
      return new RCException (closure, message);
    }

    public static RCException LockViolation (RCClosure closure)
    {
      return new RCException (
        closure, "Attempted to mutate a value after it was locked.");
    }

    public readonly RCClosure Closure;
    public readonly TargetInvocationException Exception;

    public RCException (RCClosure closure, string message)
      : base (message)
    {
      Closure = closure;
    }

    public RCException (RCClosure closure, TargetInvocationException ex, string message)
      : base (message)
    {
      Closure = closure;
      Exception = ex;
    }

    public override string ToString ()
    {
      if (Exception == null)
      {
        return base.ToString ();
      }
      else

      {
        return string.Format ("{0}\n--------------------------------------------------------------------------------\n{1}",
                                Message, Exception.GetBaseException ().ToString ());
      }
    }
  }
}

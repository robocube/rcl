
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
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, RCOperator op, RCValue left, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " +
        left.TypeName + " and " + right.TypeName + ".";
      message += "(" + left.ToString () + "," + right.ToString () + ")";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object right)
    {
      string message = "Operator " + op +
        " can not receive arguments of type " + right.GetType ().Name + ".";
      message += "(" + right.ToString () + ")";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object left, object right)
    {
      string message = "Operator " + op +
        " can not receive arguments of type " +
        left.GetType ().Name + " and " + right.GetType ().Name + ".";
      message += "(" + left.ToString () + "," + right.ToString () + ")";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException LockViolation (RCClosure closure)
    {
      return new RCException (
        closure, RCErrors.Lock, "Attempted to mutate a value after it was locked.");
    }

    public readonly RCClosure Closure;
    public readonly TargetInvocationException Exception;
    public readonly RCErrors Error;

    public RCException (RCClosure closure, RCErrors error, string message)
      : base (message)
    {
      Closure = closure;
      Error = error;
    }

    public RCException (RCClosure closure, TargetInvocationException ex, RCErrors error, string message)
      : base (message)
    {
      Closure = closure;
      Exception = ex;
      Error = error;
    }

    public override string ToString ()
    {
      if (Exception == null)
      {
        return base.ToString ();
      }
      else
      {
        return string.Format ("{0}\n--------------------------------------------------------------------------------\n{1}\n--------------------------------------------------------------------------------\n{2}",
                              Message, 
                              Exception.GetBaseException ().ToString (),
                              Closure.ToString ());
      }
    }

    public string ToTestString ()
    {
      return string.Format ("<<{0}>>", Error.ToString ());
    }
  }
}

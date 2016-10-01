
using System;
using System.Text;
using System.Reflection;

namespace RCL.Kernel
{
  //I think creating a comprehensive set of exceptions and error messages is appropriate.
  public class RCException : Exception
  {
    public static RCException Overload (
      RCClosure closure, RCOperator op, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " + right.TypeName + ".";
      //message += " (" + right.ToString () + ")";
      message += "\n---- RCL Stack ----\n";
      message += closure.ToString ();
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, RCOperator op, RCValue left, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " +
        left.TypeName + " and " + right.TypeName + ".";
      //message += " (" + left.ToString () + ", " + right.ToString () + ")";
      message += "\n---- RCL Stack ----\n";
      message += closure.ToString ();
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object right)
    {
      string message = "Operator " + op +
        " can not receive arguments of type " + right.GetType ().Name + ".";
      //message += " (" + right.ToString () + ")";
      message += "\n---- RCL Stack ----\n";
      message += closure.ToString ();
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object left, object right)
    {
      string message = "Operator " + op +
        " can not receive arguments of type " +
        left.GetType ().Name + " and " + right.GetType ().Name + ".";
      //message += " (" + left.ToString () + ", " + right.ToString () + ")";
      message += "\n---- RCL Stack ----\n";
      message += closure.ToString ();
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
    public readonly RCString Output;

    public RCException (RCClosure closure,
                        RCErrors error,
                        string message)
      : base (message)
    {
      Closure = closure;
      Error = error;
    }

    public RCException (RCClosure closure, 
                        RCErrors error,
                        string message,
                        RCString output)
      : base (message)
    {
      Closure = closure;
      Error = error;
      Output = output;
    }

    public RCException (RCClosure closure, 
                        TargetInvocationException ex,
                        RCErrors error,
                        string message)
      : base (message)
    {
      Closure = closure;
      Exception = ex;
      Error = error;
    }

    public override string ToString ()
    {
      StringBuilder builder = new StringBuilder ();
      if (Error != RCErrors.Native)
      {
        if (Output != null)
        {
          for (int i = 0; i < Output.Count; ++i)
          {
            builder.AppendLine (Output[i]);
          }
        }
        builder.AppendFormat ("<<{0},{1}>>", Error, Message);
        return builder.ToString ();
      }
      string br = new String ('-', 80);
      builder.AppendLine (br);
      builder.AppendLine (Closure.ToString ());
      builder.AppendLine (br);
      builder.AppendLine (Message);
      builder.AppendLine (br);
      if (Exception != null)
      {
        builder.AppendLine (Exception.GetBaseException ().ToString ());
        builder.AppendLine (br);
      }
      return builder.ToString ();
    }
      
      /*
      if (Exception == null)
      {
        StringBuilder builder = new StringBuilder ();
        if (Output != null)
        {
          for (int i = 0; i < Output.Count; ++i)
          {
            builder.AppendLine (Output[i]);
          }
        }
        builder.AppendFormat ("<<{0},{1}>>", Error, Message);
        return builder.ToString ();
      }
      else
      {
        return string.Format ("{3}\n{0}{3}\n{1}\n{3}\n{2}\n{3}",
                              Closure.ToString (),
                              Message,
                              Exception.GetBaseException ().ToString (),
                              new String ('-', 80));
      }
    }
    */

    public string ToTestString ()
    {
      return string.Format ("<<{0}>>", Error.ToString ());
    }
  }
}

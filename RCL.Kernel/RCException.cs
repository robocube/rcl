
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
      string message = closure.ToString ();
      message += "\n---- RCL Stack ----\n";
      message += "Operator " + op.Name +
        " can not receive arguments of type " + right.TypeName + ".";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, RCOperator op, RCValue left, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " +
        left.TypeName + " and " + right.TypeName + ".";
      message += "\n---- RCL Stack ----\n";
      message += closure.ToString ();
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object right)
    {
      string message = closure.ToString ();
      message += "\n---- RCL Stack ----\n";
      message += "Operator " + op +
        " can not receive arguments of type " + right.GetType ().Name + ".";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (
      RCClosure closure, string op, object left, object right)
    {
      string message = closure.ToString ();
      message += "\n---- RCL Stack ----\n";
      message += "Operator " + op +
        " can not receive arguments of type " +
        left.GetType ().Name + " and " + right.GetType ().Name + ".";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Range (RCClosure closure, long i, long count)
    {
      return new RCException (closure,
                              RCErrors.Range,
                              string.Format ("Index {0} is out of range. Count is {1}", i, count));
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
      return ToStringInner (messageOnTop:false, noStackOnNonNativeErrors:false);
    }

    public string ToSystemdString ()
    {
      return ToStringInner (messageOnTop:true, noStackOnNonNativeErrors:false);
    }

    public string ToTestString ()
    {
      return string.Format ("<<{0}>>", Error.ToString ());
    }

    public string ToStringInner (bool messageOnTop, bool noStackOnNonNativeErrors)
    {
      StringBuilder builder = new StringBuilder ();
      //Never show stack on asserts
      //Always show stack on native exceptions
      if (Error == RCErrors.Assert ||
          Error == RCErrors.Exec ||
          Error == RCErrors.File ||
          (noStackOnNonNativeErrors && Error != RCErrors.Native))
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
      if (messageOnTop)
      {
        builder.AppendLine (br);
        builder.AppendLine (Message);
      }
      builder.AppendLine (br);
      if (Exception != null)
      {
        builder.AppendLine (Exception.GetBaseException ().ToString ());
        builder.AppendLine (br);
      }
      builder.AppendLine (Closure.ToString ());
      builder.AppendLine (br);
      if (!messageOnTop)
      {
        builder.AppendLine (Message);
        builder.AppendLine (br);
      }
      return builder.ToString ();
    }

  }
}

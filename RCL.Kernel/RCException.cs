
using System;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

namespace RCL.Kernel
{
  //I think creating a comprehensive set of exceptions and error messages is appropriate.
  [Serializable]
  public class RCException : Exception
  {
    public static RCException Overload (RCClosure closure, RCOperator op, RCValue right)
    {
      string message = closure.ToString ();
      message += "\n---- RCL Stack ----\n";
      message += "Operator " + op.Name +
        " can not receive arguments of type " + right.TypeName + ".";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (RCClosure closure, RCOperator op, RCValue left, RCValue right)
    {
      string message = "Operator " + op.Name +
        " can not receive arguments of type " +
        left.TypeName + " and " + right.TypeName + ".";
      message += "\n---- RCL Stack ----\n";
      message += closure.ToString ();
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (RCClosure closure, string op, object right)
    {
      string message = closure.ToString ();
      message += "\n---- RCL Stack ----\n";
      message += "Operator " + op +
        " can not receive arguments of type " + right.GetType ().Name + ".";
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (RCClosure closure, string op, object left, object right)
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
      return new RCException (closure,
                              RCErrors.Lock,
                              "Attempted to mutate a value after it was locked.");
    }

    public RCException () {}

    //Without this constructor, deserialization will fail
    protected RCException (SerializationInfo info, StreamingContext context)
      :base (info, context)
    {
      string closureString = info.GetString ("Closure");
      bool fragment;
      RCBlock closureBlock = (RCBlock) RCSystem.Parse (closureString, out fragment);
      Closure = RCClosure.Deserialize (closureBlock);
      Error = (RCErrors) info.GetValue ("Error", typeof (RCErrors));
      string[] outputStrings = (string[]) info.GetValue ("Output", typeof (string[]));
      if (outputStrings != null)
      {
        Output = new RCString (outputStrings);
      }
    }

    [SecurityPermissionAttribute (SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      if (info == null)
      {
        throw new ArgumentNullException ("info");
      }
      info.AddValue ("Closure", this.Closure.Serialize ().ToString ());
      info.AddValue ("Error", this.Error, typeof (RCErrors));
      if (this.Output != null)
      {
        info.AddValue ("Output", this.Output.ToArray (), typeof (string[]));
      }
      else
      {
        info.AddValue ("Output", new string[]{}, typeof (string[]));
      }
      // MUST call through to the base class to let it save its own state
      base.GetObjectData (info, context);
    }

    public readonly RCClosure Closure;
    public readonly TargetInvocationException Exception;
    public readonly RCErrors Error;
    public readonly RCString Output;

    public RCException (RCClosure closure,
                        RCErrors error,
                        string message)
      :base (message)
    {
      Closure = closure;
      Error = error;
    }

    public RCException (RCClosure closure, 
                        RCErrors error,
                        string message,
                        RCString output)
      :base (message)
    {
      Closure = closure;
      Error = error;
      Output = output;
    }

    public RCException (RCClosure closure, 
                        TargetInvocationException ex,
                        RCErrors error,
                        string message)
      :base (message)
    {
      Closure = closure;
      Exception = ex;
      Error = error;
    }

    public override string ToString ()
    {
      return ToStringInner (testString:false, messageOnTop:false, noStackOnNonNativeErrors:false, firstOnTop:true);
    }

    public string ToSystemdString ()
    {
      return ToStringInner (testString:false, messageOnTop:true, noStackOnNonNativeErrors:false, firstOnTop:false);
    }

    public string ToTestString ()
    {
      //return ToString ();
      //return string.Format ("<<{0}>>", Error.ToString ());
      return ToStringInner (testString:true, messageOnTop:false, noStackOnNonNativeErrors:false, firstOnTop:true);
    }

    public string ToStringInner (bool testString, bool messageOnTop, bool noStackOnNonNativeErrors, bool firstOnTop)
    {
      StringBuilder builder = new StringBuilder ();
      //Never show stack on asserts
      //Always show stack on native exceptions
      if (testString ||
          Error == RCErrors.Assert ||
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
        builder.AppendLine (Message);
      }
      if (Exception != null)
      {
        builder.AppendLine (Exception.GetBaseException ().ToString ());
        builder.AppendLine (br);
      }
      //Most recent stack frames should appear on top, not on bottom
      Closure.ToString (builder:builder, indent:0, firstOnTop:firstOnTop);
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

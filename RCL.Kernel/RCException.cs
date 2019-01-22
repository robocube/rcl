
using System;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

namespace RCL.Kernel
{
  /// <summary>
  /// A runtime exception in an RCL user program. RCExceptions occur in both debug and release builds.
  /// Additionally, all RCExceptions have closures attached, whereas RCDebugExceptions do not.
  /// </summary>
  [Serializable]
  public class RCException : Exception
  {
    protected static string MONADIC_OVERLOAD_FORMAT = "Operator {0} cannot receive a single argument of type {1}";
    protected static string DYADIC_OVERLOAD_FORMAT = "Operator {0} cannot receive arguments of type {2} and {1}";
    public static RCException Overload (RCClosure closure, RCOperator op, RCValue right)
    {
      string message = string.Format (MONADIC_OVERLOAD_FORMAT, op.Name, right.TypeName);
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (RCClosure closure, RCOperator op, RCValue left, RCValue right)
    {
      string message = string.Format (DYADIC_OVERLOAD_FORMAT, op.Name, left.TypeName, right.TypeName);
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (RCClosure closure, string op, object right)
    {
      string message = string.Format (MONADIC_OVERLOAD_FORMAT, op, RCValue.TypeNameForType (right.GetType ()));
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Overload (RCClosure closure, string op, object left, object right)
    {
      string message = string.Format (DYADIC_OVERLOAD_FORMAT, op,
                                      RCValue.TypeNameForType (right.GetType ()),
                                      RCValue.TypeNameForType (left.GetType()));
      return new RCException (closure, RCErrors.Type, message);
    }

    public static RCException Range (RCClosure closure, long i, long count)
    {
      return new RCException (closure, RCErrors.Range,
                              string.Format ("Index {0} is out of range. Count is {1}", i, count));
    }

    public static RCException LockViolation (RCClosure closure)
    {
      return new RCException (closure, RCErrors.Lock,
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
    public readonly Exception Exception;
    public readonly RCErrors Error;
    public readonly RCString Output;

    public RCException (RCClosure closure, RCErrors error, string message)
      :base (message)
    {
      Closure = closure;
      Error = error;
    }

    public RCException (RCClosure closure, RCErrors error, string message, RCString output)
      :base (message)
    {
      Closure = closure;
      Error = error;
      Output = output;
    }

    public RCException (RCClosure closure, Exception ex, RCErrors error, string message)
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
      return ToStringInner (testString:!RCSystem.Args.FullStack, messageOnTop:false, noStackOnNonNativeErrors:false, firstOnTop:true);
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
            builder.Append (Output[i]);
            builder.Append ("\n");
          }
        }
        builder.AppendFormat ("<<{0},{1}>>", Error, Message);
        return builder.ToString ();
      }
      string br = new String ('-', 80);
      if (messageOnTop)
      {
        builder.Append (Message);
        builder.Append ("\n");
      }
      if (Exception != null)
      {
        builder.Append (Exception.GetBaseException ().ToString ());
        builder.Append ("\n");
        builder.Append (br);
        builder.Append ("\n");
      }
      //Most recent stack frames should appear on top, not on bottom
      Closure.ToString (builder:builder, indent:0, firstOnTop:firstOnTop);
      builder.Append (br);
      builder.Append ("\n");
      if (!messageOnTop)
      {
        builder.Append (Message);
        builder.Append ("\n");
        builder.Append (br);
        builder.Append ("\n");
      }
      builder.Remove (builder.Length - 1, 1);
      return builder.ToString ();
    }
  }

  /// <summary>
  /// An exception caused by malformed RCL syntax.
  /// </summary>
  [Serializable]
  public class RCLSyntaxException : Exception
  {
    public readonly RCToken Token;
    public readonly Exception Exception;

    protected static string MakeMessage (RCToken token, string details)
    {
      if (details != null && details != "")
      {
        return string.Format ("Invalid syntax around line {0} near the text '{1}'. {2}",
                              token.Line, RCTokenType.EscapeControlChars (token.Text, '"'), details);
      }
      else
      {
        return string.Format ("Invalid syntax around line {0} near the text '{1}'.",
                              token.Line, RCTokenType.EscapeControlChars (token.Text, '"'));
      }
    }

    public RCLSyntaxException (RCToken token, string details)
      :base (MakeMessage (token, details))
    {
      RCAssert.ArgumentIsNotNull (token, "token");
      Token = token;
    }

    public RCLSyntaxException (RCToken token, Exception exception)
      :base (MakeMessage (token, null))
    {
      RCAssert.ArgumentIsNotNull (token, "token");
      RCAssert.ArgumentIsNotNull (exception, "exception");
      Token = token;
      Exception = exception;
    }

    public override string ToString ()
    {
      StringBuilder builder = new StringBuilder ();
      builder.AppendLine (Message);
      string br = new String ('-', 80);
      builder.Append (StackTrace);
      if (RCSystem.Args.OutputEnum != RCOutput.Test)
      {
        if (Exception != null)
        {
          builder.AppendLine ();
          builder.AppendLine (Exception.ToString ());
          builder.Append (br);
        }
      }
      return builder.ToString ();
    }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    protected RCLSyntaxException (SerializationInfo info, StreamingContext context)
      :base (info, context)
    {
    }

    /// <summary>
    /// Serialization method.
    /// </summary>
    [SecurityPermissionAttribute (SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData (SerializationInfo info, StreamingContext context)
    {
    }
  }

  /// <summary>
  /// An exception resulting from a failed assertion in the RCL runtime.
  /// RCDebugExceptions only occur in debug builds.
  /// </summary>
  [Serializable]
  public class RCDebugException : Exception
  {
    public RCDebugException (string format, params object[] args)
      :base (string.Format (format, args)) {}

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    protected RCDebugException (SerializationInfo info, StreamingContext context)
      :base (info, context)
    {
      /*
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
      */
    }

    /// <summary>
    /// Serialization method.
    /// </summary>
    [SecurityPermissionAttribute (SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      /*
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
      */
    }
  }
}

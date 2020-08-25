
using System;
using System.Text;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace RCL.Kernel
{
  /// <summary>
  /// An exception caused by malformed RCL syntax.
  /// </summary>
  [Serializable]
  public class RCSyntaxException : Exception
  {
    public readonly RCToken Token;
    public readonly Exception Exception;

    protected static string MakeMessage (RCToken token, string details)
    {
      if (details != null && details != "")
      {
        return string.Format ("Invalid syntax around line {0} near the text '{1}'. {2}",
                              token.Line,
                              RCTokenType.EscapeControlChars (token.Text, '"'),
                              details);
      }
      else
      {
        return string.Format ("Invalid syntax around line {0} near the text '{1}'.",
                              token.Line,
                              RCTokenType.EscapeControlChars (token.Text, '"'));
      }
    }

    public RCSyntaxException (RCToken token, string details)
      : base (MakeMessage (token, details))
    {
      RCAssert.ArgumentIsNotNull (token, "token");
      Token = token;
    }

    public RCSyntaxException (RCToken token, Exception exception)
      : base (MakeMessage (token, null))
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
    protected RCSyntaxException (SerializationInfo info, StreamingContext context)
      : base (info, context) { }

    /// <summary>
    /// Serialization method.
    /// </summary>
    [SecurityPermissionAttribute (SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData (SerializationInfo info, StreamingContext context) { }
  }
}

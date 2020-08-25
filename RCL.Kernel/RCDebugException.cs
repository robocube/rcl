
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RCL.Kernel
{
  /// <summary>
  /// An exception resulting from a failed assertion in the RCL runtime.
  /// RCDebugExceptions only occur in debug builds.
  /// </summary>
  [Serializable]
  public class RCDebugException : Exception
  {
    public RCDebugException (string format, params object[] args)
      : base (string.Format (format, args)) { }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    protected RCDebugException (SerializationInfo info, StreamingContext context)
      : base (info, context)
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

using System;
using System.Text;

namespace RCL.Kernel
{
  //Not certain I am going to need this in the end.
  public class RCNative : RCValue
  {
    public readonly object Value;
    public RCNative (object value)
    {
      Value = value;
    }

    public override void Format (
      StringBuilder builder, RCFormat args, int level)
    {
      //There should be some clean way to identify a native value,
      //if just for the purpose of raising an error.
      //This will make it look like an operator to the parser.
      builder.Append (Value.ToString ());
    }

    public override string TypeName
    {
      get { return "native"; }
    }

    public override char TypeCode
    {
      get { return 'v'; }
    }
  }
}
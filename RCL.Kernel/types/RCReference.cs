
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCLReference : RCValue
  {
    protected static readonly Dictionary<string, Type> m_types = new Dictionary<string, Type> ();
    protected static readonly Dictionary<Type, string> m_codes = new Dictionary<Type, string> ();

    public readonly string Name;
    public readonly RCArray<string> Parts;

    protected internal RCBlock m_static;

    public RCLReference (string name)
    {
      Name = name;
      Parts = new RCArray<string> (name.Split ('.'));
      Parts.Lock ();
    }

    public RCLReference (string[] parts)
    {
      Parts = new RCArray<string> (parts);
      Parts.Lock ();
      Name = "";
      if (parts.Length > 1)
      {
        for (int i = 0; i < parts.Length; ++i)
        {
          Name += parts[i];
          if (i < parts.Length - 1)
            Name += '.';
        }
      }
      else Name = parts[0];
    }

    public override bool IsReference
    {
      get { return true; }
    }

    public override bool ArgumentEval
    {
      get { return true; }
    }

    public override string TypeName
    {
      get { return "reference"; }
    }

    public override void Eval (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEval (runner, closure, this);
    }

    public static string Delimit (RCArray<string> parts, string delimeter)
    {
      StringBuilder builder = new StringBuilder ();
      for(int i = 0; i < parts.Count; ++i)
      {
        builder.Append (parts[i]);
        if (i < parts.Count - 1)
          builder.Append (delimeter);
      }
      return builder.ToString ();
    }

    public override void Format (
      StringBuilder builder, RCFormat args, int level)
    {
      RCL.Kernel.Format.DoFormat (this, builder, args, level);
    }

    public void SetStatic (RCBlock context)
    {
      if (IsLocked)
        throw new Exception (
          "Attempted to modify a locked instance of RCReference.");

      m_static = context;
    }

    public override RCOperator AsOperator (
      RCActivator activator, RCValue left, RCValue right)
    {
      return activator.New (Name, left, right);
    }

    public override void ToByte (RCArray<byte> result)
    {
      Binary.WriteReference (result, this);
    }
  }
}
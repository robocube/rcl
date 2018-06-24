
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace RCL.Kernel
{
  public abstract class RCValue
  {
    public static readonly string REFERENCE_TYPENAME = "reference";
    public static readonly string BYTE_TYPENAME = "byte";
    public static readonly string TIME_TYPENAME = "time";
    public static readonly string BLOCK_TYPENAME = "block";
    public static readonly string CUBE_TYPENAME = "cube";
    public static readonly string OPERATOR_TYPENAME = "operator";
    public static readonly string TEMPLATE_TYPENAME = "template";
    public static readonly string BOOLEAN_TYPENAME = "boolean";
    public static readonly string LONG_TYPENAME = "long";
    public static readonly string DOUBLE_TYPENAME = "double";
    public static readonly string DECIMAL_TYPENAME = "decimal";
    public static readonly string SYMBOL_TYPENAME = "symbol";
    public static readonly string STRING_TYPENAME = "string";
    public static readonly string INCR_TYPENAME = "incr";

    public static string TypeNameForType (Type type)
    {
      switch (type.Name)
      {
        case "RCBlock": return BLOCK_TYPENAME;
        case "RCCube": return CUBE_TYPENAME;
        case "RCOperator": return OPERATOR_TYPENAME;
        case "RCReference": return REFERENCE_TYPENAME;
        case "RCTemplate": return TEMPLATE_TYPENAME;
        case "RCByte": return BYTE_TYPENAME;
        case "RCTime": return TIME_TYPENAME;
        case "RCBoolean": return BOOLEAN_TYPENAME;
        case "RCLong": return LONG_TYPENAME;
        case "RCDouble": return DOUBLE_TYPENAME;
        case "RCDecimal": return DECIMAL_TYPENAME;
        case "RCSymbol": return SYMBOL_TYPENAME;
        case "RCString": return STRING_TYPENAME;
        case "RCIncr": return INCR_TYPENAME;
        default: return type.Name;
      }
    }

    protected bool m_lock = false;

    public RCValue () {}

    /// <summary>
    /// Operators are important parts of speech in RC and there are several
    /// places where the parser and printer need to know if something is 
    /// an operator.  A hardcoded check like this is preferable to an is cast
    /// in these situations.
    /// </summary>
    public virtual bool IsOperator
    {
      get { return false; }
    }

    public virtual bool IsReference
    {
      get { return false; }
    }

    public virtual bool IsBlock
    {
      get { return false; }
    }

    public virtual bool IsCube
    {
      get { return false; }
    }

    public virtual bool IsTemplate
    {
      get { return false; }
    }

    public virtual bool IsVector
    {
      get { return false; }
    }

    public virtual bool ArgumentEval
    {
      get { return false; }
    }

    public virtual void Eval (RCRunner runner, RCClosure closure)
    {
      runner.Yield (closure, this);
    }

    public virtual RCClosure Next (RCRunner runner, RCClosure tail, RCClosure previous, RCValue result)
    {
      return RCL.Kernel.Eval.DoNext (this, runner, tail, previous, result);
    }

    public virtual RCValue Finish (RCRunner runner, RCClosure closure, RCValue result)
    {
      return RCL.Kernel.Eval.DoFinish (runner, closure, result);
    }

    public virtual RCClosure Handle (RCRunner runner, RCClosure closure, Exception exception, long status, out RCValue handled)
    {
      handled = null;
      return null;
    }

    public virtual RCValue Finish (RCValue result)
    {
      return result;
    }

    /// <summary>
    /// The default implementation of ToString creates its own
    /// builder.  But the real work is done inside of the overload.
    /// StringBuilders have extra space allocated for new data to 
    /// be appended so its better to only create one for a given 
    /// Object.ToString() call.
    /// </summary>
    public override string ToString ()
    {
      return Format (RCL.Kernel.RCFormat.Default);
    }

    public virtual string Format (RCFormat args, RCColmap colmap)
    {
      StringBuilder builder = new StringBuilder ();
      Format (builder, args, colmap, 0);
      return builder.ToString ();
    }

    public virtual string Format (RCFormat args)
    {
      StringBuilder builder = new StringBuilder ();
      Format (builder, args, null, 0);
      return builder.ToString ();
    }

    public abstract void Format (StringBuilder builder, RCFormat args, int level);
    public abstract void Format (StringBuilder builder, RCFormat args, RCColmap colmap, int level);

    public virtual void ToByte (RCArray<byte> result)
    {
      throw new Exception ("Cannot serialize " + TypeName);
    }

    public virtual object Child (int i)
    {
      throw new NotImplementedException ();
    }

    public virtual int Count
    {
      get { return 1; }
    }

    public virtual bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      return true;
    }

    public virtual bool IsBeforeLastCall (RCClosure closure)
    {
      //Yes it matters that this one defaults to false and the other one defaults to true.
      return false;
    }

    //Need to get rid of this method.
    public virtual bool IsHigherOrder ()
    {
      return false;
    }

    public override bool Equals (object obj)
    {
      //This should be overridden by all the child classes to avoid
      //the ToString() operation.  This is a stopgap solution only.
      if (obj == null) return false;
      if (obj == this) return true;
      RCValue other = obj as RCValue;
      if (other == null) return false;
      string thisText = this.ToString ();
      string otherText = other.ToString ();
      return thisText.Equals (otherText);
    }

    public override int GetHashCode ()
    {
      return base.GetHashCode ();
    }

    public virtual RCOperator AsOperator (RCActivator activator, RCValue left, RCValue right)
    {
      RCOperator result = new InlineOperator (this);
      result.Init ("", left, right);
      return result;
    }

    public delegate RCValue RCValueDelegate (RCValue val);
    public virtual RCValue Edit (RCRunner runner, RCValueDelegate editor)
    {
      RCValue edit = editor (this);
      if (edit != null)
        return edit;
      return null;
    }

    //Types that have child objects need to override this and lock the children.
    public virtual void Lock (bool canonical)
    {
      m_lock = true;
    }

    public virtual void Cubify (RCCube target, Stack<object> names) {}

    public bool IsLocked { get { return m_lock; } }
    public abstract string TypeName { get; }
    public abstract char TypeCode { get; }
  }
}

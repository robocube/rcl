
using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  /// <summary>
  /// When building async operators you always need the runner and
  /// closure in order to report back later on.  Often you also 
  /// need to hold onto some "other" api object as well.
  /// </summary>
  public class RCAsyncState
  {
    public readonly RCRunner Runner;
    public readonly RCClosure Closure;
    public readonly object Other;

    public RCAsyncState (RCRunner runner, RCClosure closure, object other)
    {
      Runner = runner;
      Closure = closure;
      Other = other;
    }
  }

  public class RCOperator : RCValue
  {
    protected RCValue m_left;
    protected RCValue m_right;
    protected string m_name;
    protected int m_count = 0;

    //for future implementation
    //protected RCBlock m_impl;
    
    public virtual void Init (string name, RCValue left, RCValue right)
    {
      if (IsLocked)
        throw new Exception (
          "Attempted to modify a locked instance of RCOperator");

      if (right == null)
        throw new ArgumentNullException ("right");

      /*
      if (left == null)
      {
        m_impl =
          new RCBlock (
            new RCBlock ("0", ":", right),
                          "", "~", new RCString (name));
      }
      else
      {
        m_impl =
          new RCBlock (
            new RCBlock (
              new RCBlock ("0", ":", left),
                           "1", ":", right),
                            "", "~", new RCString (name));
      }
      */

      m_name = name;
      m_left = left;
      m_right = right;
      m_count += m_right.IsOperator ? m_right.Count : 1;
      m_count += m_left != null && m_left.IsOperator ? m_left.Count : 1;
    }

    public override void Lock ()
    {
      if (m_left != null)
        m_left.Lock ();
      m_right.Lock ();
    }

    public RCValue Left
    {
      get { return m_left; }
    }

    public RCValue Right
    {
      get { return m_right; }
    }

    public string Name
    {
      get { return m_name; }
    }

    public override string TypeName
    {
      get { return RCValue.OPERATOR_TYPENAME; }
    }

    public override bool IsOperator
    {
      get { return true; }
    }

    public override char TypeCode
    {
      get { return 'o'; }
    }

    public override bool ArgumentEval
    {
      get { return true; }
    }

    public override int Count
    {
      get { return m_count; }
    }

    protected static readonly Type[] CTOR = new Type[]{};
    public override RCValue Edit (RCRunner runner, RCValueDelegate editor)
    {
      RCOperator result = (RCOperator) base.Edit (runner, editor);
      if (result != null)
      {
        return result;
      }
      RCValue left = null;
      if (m_left != null)
      {
        left = m_left.Edit (runner, editor);
      }
      RCValue right = m_right.Edit (runner, editor);
      if (left != null || right != null)
      {
        if (left == null)
        {
          left = m_left;
        }
        if (right == null)
        {
          right = m_right;
        }
        result = runner.New (m_name, left, right);
        return result;
      }
      else return null;
    }

    public override void Eval (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEval (runner, closure, this);
    }

    public virtual void EvalOperator (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEvalOperator (runner, closure, this);
    }

    public override RCClosure Next (RCRunner runner, RCClosure head, RCClosure previous, RCValue result)
    {
      return RCL.Kernel.Eval.DoNext (this, runner, head, previous, result);
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      return RCL.Kernel.Eval.DoIsLastCall (closure, arg, this);
    }

    public override bool IsBeforeLastCall (RCClosure closure)
    {
      return RCL.Kernel.Eval.DoIsBeforeLastCall (closure, this);
    }

    public override void Format (StringBuilder builder, RCFormat args, int level)
    {
      RCL.Kernel.Format.DoFormat (this, builder, args, null, level);
    }

    public override void Format (StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      RCL.Kernel.Format.DoFormat (this, builder, args, colmap, level);
    }

    public virtual void BodyToString (StringBuilder builder, RCFormat args, int level)
    {
      builder.Append  (Name);
    }

    public override void ToByte (RCArray<byte> result)
    {
      RCL.Kernel.Binary.WriteOperator (result, this);
    }

    public override void Cubify (RCCube target, Stack<object> names)
    {
      if (this.Left != null)
      {
        names.Push ("L");
        this.Left.Cubify (target, names);
        names.Pop ();
      }
      names.Push ("R");
      this.Right.Cubify (target, names);
      names.Pop ();
      object[] array = names.ToArray ();
      System.Array.Reverse (array);
      RCSymbolScalar symbol = RCSymbolScalar.From (array);
      target.WriteCell ("o", symbol, Name);
      target.Write (symbol);
    }
  }

  /// <summary>
  /// A UserOperator is sort of a cross between a reference and an operator.
  /// It is a reference to an expression or value on the stack that
  /// will be treated just like an operator.
  /// </summary>
  public class UserOperator : RCOperator
  {
    protected internal RCReference m_reference;
    
    public UserOperator (RCReference reference)
    {
      if (reference == null)
      {
        throw new ArgumentNullException ("reference");
      }
      m_reference = reference;
    }

    public override void EvalOperator (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEvalUserOp (runner, closure, this);
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      return RCL.Kernel.Eval.DoIsLastCall (closure, arg, this);
    }

    public override bool IsBeforeLastCall (RCClosure closure)
    {
      return RCL.Kernel.Eval.DoIsBeforeLastCall (closure, this);
    }
  }

  /// <summary>
  /// An inline operator is basically a lambda expression; an operator embedded in a larger
  /// expression without being given a name.  For the time being this capability is 
  /// restricted to embedded blocks (by the parser).  But we might open it up to 
  /// expressions or other values if the need arises.
  //  I am thinking of getting rid of these.
  /// </summary>
  public class InlineOperator : RCOperator
  {
    protected internal RCValue m_code;

    public InlineOperator (RCValue code)
    {
      if (code == null)
      {
        throw new ArgumentNullException ("code");
      }
      m_code = code;
    }

    public override void EvalOperator (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEvalInline (runner, closure, this);
    }

    public override void BodyToString (StringBuilder builder, RCFormat args, int level)
    {
      m_code.Format (builder, args, level);
    }
  }

  public class RCTemplate : RCBlock
  {
    //I am going to need to store this number but I would like
    //to move it into the block somehow.
    protected readonly int m_escapeCount;
    protected readonly bool m_multiline;
    protected readonly bool m_cr;

    public RCTemplate (RCBlock definition, int escapeCount, bool multiline)
      :base (definition, "", RCEvaluator.Expand, new RCLong (escapeCount))
    {
      m_escapeCount = escapeCount;
      m_multiline = multiline;
    }

    public override void Format (StringBuilder builder, RCFormat args, int level)
    {
      RCFormat format = new RCFormat (args.Syntax, "  ", args.Newline, args.Delimeter,
                                      args.RowDelimeter, args.Align, args.Showt, args.ParsableScalars, args.CanonicalCubes);
      RCL.Kernel.Format.DoFormat (this, builder, format, null, level);
    }

    public override void Format (StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      RCFormat format = new RCFormat (args.Syntax, "  ", args.Newline, args.Delimeter,
                                      args.RowDelimeter, args.Align, args.Showt, args.ParsableScalars, args.CanonicalCubes);
      RCL.Kernel.Format.DoFormat (this, builder, format, colmap, level);
    }

    public override string TypeName
    {
      get { return RCValue.TEMPLATE_TYPENAME; }
    }

    public int EscapeCount
    {
      get { return m_escapeCount; }
    }

    public bool Multiline
    {
      get { return m_multiline; }
    }

    public override bool IsTemplate
    {
      get { return true; }
    }
  }
}

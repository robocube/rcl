
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCOperator : RCValue
  {
    protected RCValue _left;
    protected RCValue _right;
    protected string _name;
    protected int _count = 0;

    public virtual void Init (string name, RCValue left, RCValue right)
    {
      if (IsLocked) {
        throw new Exception (
                "Attempted to modify a locked instance of RCOperator");
      }

      if (right == null) {
        throw new ArgumentNullException ("right");
      }

      /*
         if (left == null)
         {
         _impl =
          new RCBlock (
            new RCBlock ("0", ":", right),
                          "", "~", new RCString (name));
         }
         else
         {
         _impl =
          new RCBlock (
            new RCBlock (
              new RCBlock ("0", ":", left),
                           "1", ":", right),
                            "", "~", new RCString (name));
         }
       */

      _name = name;
      _left = left;
      _right = right;
      _count += _right.IsOperator ? _right.Count : 1;
      if (_left != null) {
        _count += _left.IsOperator ? _left.Count : 1;
      }
    }

    public override void Lock ()
    {
      if (_left != null) {
        _left.Lock ();
      }
      _right.Lock ();
    }

    public RCValue Left
    {
      get { return _left; }
    }

    public RCValue Right
    {
      get { return _right; }
    }

    public string Name
    {
      get { return _name; }
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
      get { return _count; }
    }

    protected static readonly Type[] CTOR = new Type[] {};
    public override RCValue Edit (RCRunner runner, RCValueDelegate editor)
    {
      RCOperator result = (RCOperator) base.Edit (runner, editor);
      if (result != null) {
        return result;
      }
      RCValue left = null;
      if (_left != null) {
        left = _left.Edit (runner, editor);
      }
      RCValue right = _right.Edit (runner, editor);
      if (left != null || right != null) {
        if (left == null) {
          left = _left;
        }
        if (right == null) {
          right = _right;
        }
        result = runner.New (_name, left, right);
        return result;
      }
      else {
        return null;
      }
    }

    public override void Eval (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEval (runner, closure, this);
    }

    public virtual void EvalOperator (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEvalOperator (runner, closure, this);
    }

    public override RCClosure Next (RCRunner runner,
                                    RCClosure head,
                                    RCClosure previous,
                                    RCValue
                                    result)
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
      if (this.Left != null) {
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
}

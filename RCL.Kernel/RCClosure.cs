using System;
using System.Collections.Generic;
using System.Text;

namespace RCL.Kernel
{
  public class RCClosure
  {
    /// <summary>
    /// The bot, sort of like the this pointer.
    /// </summary>
    public readonly RCBot Bot;

    /// <summary>
    /// The fiber, within bot.
    /// </summary>
    public readonly long Fiber;

    /// <summary>
    /// The closure for the expression prior to this one.
    /// </summary>
    public readonly RCClosure Parent;

    /// <summary>
    /// The value being Eval'd
    /// </summary>
    public readonly RCValue Code;

    /// <summary>
    /// While evaluting dyadic expressions, the result of the left
    /// hand side evaluation is stored in this "pocket" so that it
    /// will not interfere with the interpretation of the right-hand
    /// argument.
    /// </summary>
    public readonly RCValue Left;

    /// <summary>
    /// The result of the current execution.
    /// </summary>
    public readonly RCBlock Result;

    /// <summary>
    /// The statement being executed within Code.
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// Symbols being held in a mutually exclusive way for this
    /// closure and all of its children.  This is only to know
    /// which symbols should be released when execution is done.
    /// </summary>
    public readonly RCSymbol Locks;

    /// <summary>
    /// Not sure this is useful or necessary.
    /// Mostly a debugging aid.
    /// </summary>
    public readonly long Depth = 0;

    public readonly RCValue UserOp;

    public readonly RCArray<RCBlock> UserOpContext;

    public RCClosure (RCBot bot, RCValue code)
      :this (bot, 0, null, null, code, null, null, 0) {}

    public RCClosure (
      RCClosure parent,
      RCBot bot,
      RCValue code,
      RCValue left,
      RCBlock result,
      int index)
      :this (parent, bot, code, left, result, index, null, null) {}

    public RCClosure (
      RCClosure parent,
      RCBot bot,
      RCValue code,
      RCValue left,
      RCBlock result,
      int index,
      RCValue userOp,
      RCArray<RCBlock> userOpContext)
    {
      if (parent != null)
      {
        Fiber = parent.Fiber;
        Locks = parent.Locks;
        Depth = parent.Depth + 1;
      }

      if (bot == null)
        throw new Exception ("bot may not be null.");
      if (code == null)
        throw new Exception ("code may not be null.");

      Bot = bot;
      Parent = parent;
      Code = code;
      Left = left;
      Result = result != null ? result : RCBlock.Empty;
      Index = index;
      UserOp = userOp;
      UserOpContext = userOpContext;
    }

    public RCClosure (
      RCBot bot,
      long fiber,
      RCSymbol locks,
      RCClosure parent,
      RCValue code,
      RCValue left,
      RCBlock result,
      int index)
    {
      if (parent != null)
        Depth = parent.Depth + 1;

      if (bot == null)
        throw new Exception ("bot may not be null.");
      if (code == null)
        throw new Exception ("code may not be null.");

      Bot = bot;
      Fiber = fiber;
      Locks = locks;
      Parent = parent;
      Code = code;
      Left = left;
      Result = result != null ? result : RCBlock.Empty;
      Index = index;
    }

    public override string ToString ()
    {
      //This is the old way to ToString() a closure, it might still be useful.
      //return "Code:" + Code.ToString() + ", Depth:" + Depth + ", Result:" + Result.ToString();
      StringBuilder builder = new StringBuilder ();
      ToString (builder, 0);
      return builder.ToString ();
    }
      
    /*
    protected void ToStringOld (StringBuilder builder, int indent)
    {
      RCClosure closure = this;
      while (closure != null)
      {
        RCOperator op = closure.Code as RCOperator;
        if (op != null)
        {
          builder.Append (op.Name);
          builder.Append (" ");
          //closure.Result.Format (builder, RCFormat.Default, indent);
          builder.AppendLine ();
        }

        RCBlock block = closure.Code as RCBlock;
        if (block != null)
        {
          RCBlock line = block.GetName (closure.Index);
          if (line.Name != "")
          {
            builder.AppendFormat ("{0} (@ {1}) ", line.Name, closure.Index);
            //closure.Code.Format (builder, RCFormat.Default, indent);
            //closure.Result.Format (builder, RCFormat.Default, indent);
          }
          else
          {
            //builder.AppendFormat ("{0}", "program...");
            line.Value.Format (builder, RCFormat.Pretty, indent);
          }
          builder.AppendLine ();
        }
        closure = closure.Parent;
      }
    }
    */

    protected void ToString (StringBuilder builder, int indent)
    {
      RCClosure closure = this;
      Stack<string> lines = new Stack<string> ();
      //Do not include the global namespace in the stack trace.
      while (closure != null && closure.Parent != null)
      { 
        if (closure.Code != null)
        {
          RCOperator op = closure.Code as RCOperator;
          if (op != null)
          {
            lines.Push (string.Format ("-- {0}", op.ToString ()));
          }
        }
        RCBlock result = closure.Result;
        while (result != null)
        {
          if (result.Value != null)
          {
            string value = result.Value.Format (RCFormat.Default);
            value = value.Substring (0, Math.Min (100, value.Length));
            lines.Push (string.Format ("{0}:{1}", result.Name, value));
          }
          result = result.Previous;
        }
        closure = closure.Parent;
      }
      while (lines.Count > 0)
      {
        builder.AppendLine (lines.Pop ());
      }
    }
  }
}

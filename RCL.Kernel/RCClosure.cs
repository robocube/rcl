using System;
using System.Collections.Generic;
using System.Text;

namespace RCL.Kernel
{
  public class RCClosure
  {
    /// <summary>
    /// The number for the bot that created this closure.
    /// </summary>
    public readonly long BotId;

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

    /// <summary>
    /// If a UserOperator is currently being evaluated,
    /// the body of the operation can be obtained here.
    /// </summary>
    public readonly RCValue UserOp;

    /// <summary>
    /// An array of blocks that contain any variables available to UserOp.
    /// </summary>
    public readonly RCArray<RCBlock> UserOpContext;

    public RCClosure (long bot, RCValue code)
      :this (bot, 0, null, null, code, null, null, 0, null, null) {}

    public RCClosure (RCClosure parent,
                      long bot,
                      RCValue code,
                      RCValue left,
                      RCBlock result,
                      int index)
      :this (parent, bot, code, left, result, index, null, null) {}

    public RCClosure (RCClosure parent,
                      long bot,
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
        UserOp = parent.UserOp;
        UserOpContext = parent.UserOpContext;
      }
      if (code == null)
      {
        throw new Exception ("code may not be null.");
      }
      //Bot = bot;
      BotId = bot;
      Parent = parent;
      Code = code;
      Left = left;
      Result = result != null ? result : RCBlock.Empty;
      Index = index;
      if (userOp != null)
      {
        UserOp = userOp;
      }
      if (userOpContext != null)
      {
        UserOpContext = userOpContext;
      }
    }

    public RCClosure (
      long bot,
      long fiber,
      RCSymbol locks,
      RCClosure parent,
      RCValue code,
      RCValue left,
      RCBlock result,
      int index,
      RCValue userOp,
      RCArray<RCBlock> userOpContext)
    {
      if (parent != null)
      {
        Depth = parent.Depth + 1;
      }
      if (code == null)
      {
        throw new Exception ("code may not be null.");
      }
      BotId = bot;
      Fiber = fiber;
      Locks = locks;
      Parent = parent;
      Code = code;
      Left = left;
      Result = result != null ? result : RCBlock.Empty;
      Index = index;
      if (userOp != null)
      {
        UserOp = userOp;
      }
      if (userOpContext != null)
      {
        UserOpContext = userOpContext;
      }
    }

    public override string ToString ()
    {
      StringBuilder builder = new StringBuilder ();
      ToString (builder, indent:0, firstOnTop:true);
      return builder.ToString ();
    }

    public void ToString (StringBuilder builder, int indent, bool firstOnTop)
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
            value = string.Format ("{0}:{1}", result.Name, value);
            value = value.Substring (0, Math.Min (80, value.Length));
            lines.Push (value);
          }
          result = result.Previous;
        }
        closure = closure.Parent;
      }
      if (firstOnTop)
      {
        builder.AppendFormat ("--- BEGIN STACK (bot:{0},fiber:{1},lines:{2}) ---\n",
                              closure.BotId, closure.Fiber, lines.Count);
        while (lines.Count > 0)
        {
          builder.AppendLine (lines.Pop ());
        }
        builder.AppendFormat ("--- END STACK ---\n");
      }
      else
      {
        builder.AppendFormat ("--- END STACK (bot:{0},fiber:{1},lines:{2}) ---\n",
                              closure.BotId, closure.Fiber, lines.Count);
        string[] linesInOrder = lines.ToArray ();
        for (int i = linesInOrder.Length - 1; i >= 0; --i)
        {
          builder.AppendLine (linesInOrder[i]);
        }
        builder.AppendFormat ("--- BEGIN STACK ---\n");
      }
    }

    public RCBlock Serialize ()
    {
      RCBlock result = RCBlock.Empty;
      result = new RCBlock (result, "bot", ":", BotId);
      result = new RCBlock (result, "code", ":", this.Code);
      result = new RCBlock (result, "depth", ":", this.Depth);
      result = new RCBlock (result, "fiber", ":", this.Fiber);
      result = new RCBlock (result, "index", ":", this.Index);
      result = new RCBlock (result, "left", ":", this.Left);
      result = new RCBlock (result, "locks", ":", this.Locks);
      result = new RCBlock (result, "result", ":", this.Result);
      result = new RCBlock (result, "parent", ":", this.Parent.Serialize ());
      return result;
    }

    public static RCClosure Deserialize (RCRunner runner, RCBlock right)
    {
      long botId = right.GetLong ("bot");
      long fiber = right.GetLong ("fiber");
      RCSymbol locks = (RCSymbol) right.Get ("locks");
      RCClosure parent = Deserialize (runner, right.GetBlock ("parent"));
      RCValue code = right.Get ("code");
      RCValue left = right.Get ("left");
      RCBlock result = right.GetBlock ("result");
      int index = (int) right.GetLong ("index");
      RCValue userOp = right.Get ("userOp");
      RCBlock userOpContextBlock = right.GetBlock ("userOpContext");
      RCArray<RCBlock> userOpContext = new RCArray<RCBlock> ();
      for (int i = 0; i < userOpContextBlock.Count; ++i)
      {
        userOpContext.Write ((RCBlock) userOpContextBlock.Get (i));
      }
      return new RCClosure (botId, fiber, locks, parent, code, left, result, index, userOp, userOpContext);
    }
  }
}

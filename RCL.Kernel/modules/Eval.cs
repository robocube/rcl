using System;
using System.Text;

namespace RCL.Kernel
{
  public class Eval : RCOperator
  {
    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCClosure parent = new RCClosure (closure.Parent, 
                                        closure.Bot,
                                        right, 
                                        closure.Left, 
                                        RCBlock.Empty, 
                                        0);
      DoEval (runner, parent, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      RCClosure parent = UserOpClosure (closure, right, new RCArray<RCBlock> (left), noClimb:true);
      DoEval (runner, parent, right);
    }

    [RCVerb ("eval")]
    public void EvalTemplate (RCRunner runner, RCClosure closure, RCBlock left, RCTemplate right)
    {
      RCClosure parent = UserOpClosure (closure, right, new RCArray<RCBlock> (left), noClimb:true);
      DoEval (runner, parent, right);
    }

    [RCVerb ("eval")]
    public void EvalTemplate (RCRunner runner, RCClosure closure, RCTemplate right)
    {
      RCClosure parent = new RCClosure (closure.Parent, 
                                        closure.Bot,
                                        right, 
                                        closure.Left, 
                                        RCBlock.Empty, 
                                        0);
      DoEval (runner, parent, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCOperator right)
    {
      RCClosure parent = new RCClosure (closure.Parent, 
                                        closure.Bot,
                                        right, 
                                        closure.Left, 
                                        RCBlock.Empty, 
                                        0);
      DoEval (runner, parent, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCOperator right)
    {
      RCClosure parent = UserOpClosure (closure, right, new RCArray<RCBlock> (left), noClimb:true);
      DoEval (runner, parent, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, UserOperator right)
    {
      //Invocation using the activator requires a perfect match on the argument type.
      EvalEval (runner, closure, left, (RCOperator) right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCReference right)
    {
      DoEval (runner, closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCReference right)
    {
      RCClosure parent = UserOpClosure (closure, right, new RCArray<RCBlock> (left), noClimb:true);
      DoEval (runner, parent, right);
    }
    
    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCByte right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCBoolean right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCLong right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCDouble right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCDecimal right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCSymbol right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("eval")]
    public void EvalEval (RCRunner runner, RCClosure closure, RCBlock left, RCTime right)
    {
      runner.Yield (closure, right);
    }

    [RCVerb ("apply")]
    public void EvalApply (RCRunner runner, RCClosure closure, RCBlock left, object right)
    {
      RCClosure parent = UserOpClosure (closure, left, null, null, (RCValue) right);
      DoEval (runner, parent, left);
    }

    [RCVerb ("apply")]
    public void EvalApply (RCRunner runner, RCClosure closure, RCTemplate left, object right)
    {
      RCClosure parent = UserOpClosure (closure, left, null, null, (RCValue) right);
      DoEval (runner, parent, left);
    }

    [RCVerb ("apply")]
    public void EvalApply (RCRunner runner, RCClosure closure, RCOperator left, object right)
    {
      RCClosure parent = UserOpClosure (closure, left, null, null, (RCValue) right);
      DoEval (runner, parent, left);
    }

    [RCVerb ("apply")]
    public void EvalApply (RCRunner runner, RCClosure closure, RCReference left, object right)
    {
      RCClosure parent = UserOpClosure (closure, left, null, null, (RCValue) right);
      DoEval (runner, parent, left);
    }

    //This higher order thingy needs to go away it makes no sense.
    /*
    public override bool IsHigherOrder ()
    {
      return false;
    }
    */

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      if (arg == null)
      {
        return base.IsLastCall (closure, arg);
      }
      if (!base.IsLastCall (closure, arg))
      {
        return false;
      }
      return arg.Code.IsBeforeLastCall (arg);
    }

    //Kicks off evaluation for an operator and its arguments.
    public static void DoEval (RCRunner runner, RCClosure closure, RCOperator op)
    {
      if (op.Left == null)
      {
        if (closure.Index == 0)
        {
          EvalArgument (runner, closure, op.Right);
        }
        else
        {
          op.EvalOperator (runner, closure);
        }
      }
      else
      {
        if (closure.Index == 0)
        {
          EvalArgument (runner, closure, op.Left);
        }
        else if (closure.Index == 1)
        {
          EvalArgument (runner, closure, op.Right);
        }
        else
        {
          op.EvalOperator (runner, closure);
        }
      }
    }

    //Evaluates the argument if the argument is another operator or a reference.
    protected static void EvalArgument (RCRunner runner, RCClosure closure, RCValue argument)
    {
      if (argument.ArgumentEval)
      {
        RCClosure child = new RCClosure (closure,
                                         closure.Bot,
                                         argument,
                                         closure.Left,
                                         null,
                                         0);
        argument.Eval (runner, child);
      }
      else
      {
        //pretty clear scope for optimization here, why go back to the runner in this case?
        //runner calls have a lot of overhead.
        //It was easier to write this way early on.
        DoYield (runner, closure, argument);
      }
    }

    public static void DoEvalOperator (RCRunner runner, RCClosure closure, RCOperator op)
    {
      RCValue left = closure.Result.Get ("0");
      RCValue right = closure.Result.Get ("1");
      RCValue virtop = Resolve (null, closure, new RCArray<string> (op.Name), null, true);
      if (virtop != null)
      {
        RCClosure parent = UserOpClosure (closure, virtop, null, noClimb:false);
        virtop.Eval (runner, parent);
        return;
      }
      if (left == null)
      {
        RCSystem.Activator.Invoke (runner, closure, op.Name, right);
      }
      else
      {
        RCSystem.Activator.Invoke (runner, closure, op.Name, left, right);
      }
      //A lot of good men died to bring us this one line of code...
      //Let's keep this around as a memorial.
      //right.BindRight (runner, closure, this, left);
    }

    //Kicks off evaluation for a block.
    public static void DoEval (RCRunner runner, RCClosure closure, RCBlock block)
    {
      if (block.Count == 0)
      {
        DoYield (runner, closure, block);
      }
      else
      {
        RCBlock current = block.GetName (closure.Index);
        if (current.Evaluator.Invoke)
        {
          string op = ((RCString) current.Value)[0];
          RCSystem.Activator.Invoke (runner, closure, op, closure.Result);
        }
        else if (current.Evaluator.Template)
        {
          try
          {
            RCString result = ExpandTemplate (new StringBuilder (),
                                              (RCTemplate) current,
                                              closure.Result,
                                              0, "");
            runner.Yield (closure, result);
          }
          catch (Exception)
          {
            //Maybe we should fold the original exception into the output here?
            RCException rcex = new RCException (closure,
                                                RCErrors.Native,
                                                "An exception was thrown by the template.");
            runner.Finish (closure, rcex, (int) RCErrors.Native);
          }
        }
        else if (current.Evaluator.Pass)
        {
          DoYield (runner, closure, current.Value);
        }
        //This means that Value is an operator or a reference.
        else if (current.Value.ArgumentEval)
        {
          current.Value.Eval (runner, new RCClosure (closure,
                                                     closure.Bot,
                                                     current.Value,
                                                     closure.Left,
                                                     closure.Result, 0));
        }
        else if (current.Evaluator.Return)
        {
          DoYield (runner, closure, current.Value);
        }
        else
        {
          //I need something different to happen when we are at the top level already.
          //Or maybe I need to inject a wrapper closure when I do Rep this way?
          if ((closure.Index < block.Count - 1) || (closure.Parent != null))
          {
            DoYield (runner, closure, current.Value);
          }
          else
          {
            DoYield (runner, closure, current);
          }
        }
      }
    }

    protected static RCString ExpandTemplate (StringBuilder builder, 
                                              RCTemplate template, 
                                              RCBlock right,
                                              int I,
                                              string parentIndent)
    {
      string indent = parentIndent;
      for (int i = 0; i < right.Count; ++i)
      {
        RCValue child = right.Get (i);
        RCVector<string> text = child as RCVector<string>;
        if (text == null)
        {
          ExpandTemplate (builder, template, (RCBlock) child, I + i, indent);
        }
        else
        {
          bool somethingAdded = false;
          for (int j = 0; j < text.Count; ++j)
          {
            string section = text [j];
            int start = 0;
            for (int k = 0; k < section.Length; ++k)
            {
              if (section [k] == '\n')
              {
                string line;
                if (i % 2 == 1)
                {
                  if (k > 0 && section.Length > 0 && section [k - 1] == '\r')
                  {
                    line = section.Substring (start, k - start - 1);
                  }
                  else
                  {
                    line = section.Substring (start, k - start);
                  }
                  //if (j > 0 || start > 0)
                  // Using j and start here didn't work because sometimes empty strings
                  // are added. Instead keep track of whether a line has been added.
                  // We may need this variable to handle other cases as well, but
                  // they haven't cropped yet.
                  if (somethingAdded)
                  {
                    //Notice below in the section with w. If there is extra content
                    //before the code section on the same line, it will have been 
                    //inserted/indented already.
                    builder.Append (indent);
                  }
                  builder.Append (line);
                  builder.Append ("\n");
                  somethingAdded = true;
                }
                else
                {
                  //In content sections after the first one,
                  //skip newlines if they are the first thing in the section.
                  line = section.Substring (start, k - start);
                  if (I + i == 0)
                  {
                    builder.Append (line);
                    builder.Append ("\n");
                  }
                  else if (line != "")
                  {
                    if (builder [builder.Length - 1] == '\n')
                    {
                      if (start == 0 && (k < section.Length - 1 || i == right.Count - 1))
                      {
                        builder.Append (indent);
                      }
                      else if (k == section.Length - 1 && i < right.Count - 1)
                      {
                        builder.Append (indent);
                      }
                      else if (start > 1 && i < right.Count - 1)
                      {
                        builder.Append (indent);
                      }
                    }
                    builder.Append (line);
                    builder.Append ("\n");
                  }
                  else if (k > 0 || 
                           (builder.Length > 0 && builder [builder.Length - 1] != '\n'))
                  {
                    builder.Append (line);
                    builder.Append ("\n");
                  }
                }
                start = k + 1;
              }
            }
            if (template.Multiline)
            {
              //If this is a code section, the lastPiece is just the last line of the template.
              //There is no newline at the end.
              //If this is a text section, the lastPiece is a prefix for the next code section.
              string lastPiece = section.Substring (start, section.Length - start);
              if (i % 2 == 1)
              {
                //Odd sections are always code sections.
                //Code sections don't have a newline at the end.
                if (j == 0)
                {
                  //This means there was a newline at the end of section.
                  if (start > 0 && lastPiece != "")
                  {
                    builder.Append (indent);
                  }
                }
                else if (j == text.Count - 1)
                {
                  indent = parentIndent;
                }
                builder.Append (lastPiece);
              }
              else
              {
                int w;
                for (w = 0; w < lastPiece.Length; ++w)
                {
                  if (lastPiece [w] != ' ')
                  {
                    break;
                  }
                }
                //indent only includes spaces before the first non-space character.
                //The non-space part of the text is only inserted once. 
                //\t not spoken here.
                indent = parentIndent + lastPiece.Substring (0, w);
                if (i < right.Count - 1)
                {
                  if (section.Length > 0)
                  {
                    builder.Append (indent);
                  }
                }
                string end = lastPiece.Substring (w, lastPiece.Length - w);
                builder.Append (end);
              }
            }
            else
            {
              //If there are no newlines in the template then just drop the whole thing in as is.
              builder.Append (text [j]);
            }
          }
        }
      }
      //Go back and remove the final newline now.
      //Let the enclosing template decide how to finish off.
      if (template.Multiline)
      {
        if (builder.Length > 0 && builder [builder.Length - 1] != '\n')
        {
          builder.Append ("\n");
        }
      }
      return new RCString (builder.ToString ());
    }

    public static void DoEval (RCRunner runner, 
                               RCClosure closure, 
                               RCReference reference)
    {
      runner.Yield (closure, 
                    Resolve (reference.m_static, closure, reference.Parts, null));
    }

    /// <summary>
    /// Find and return the value referenced by name. Throw if not found.
    /// </summary>
    protected static RCValue Resolve (RCBlock context, 
                                      RCClosure closure, 
                                      RCArray<string> name, 
                                      RCArray<RCBlock> @this)
    {
      return Resolve (context, closure, name, @this, false);
    }

    /// <summary>
    /// Find and return the value referenced by name. Return null if not found.
    /// </summary>
    protected static RCValue Resolve (RCBlock context, 
                                      RCClosure closure, 
                                      RCArray<string> name, 
                                      RCArray<RCBlock> @this,
                                      bool returnNull)
    {
      if (context != null)
      {
        RCValue result = context.Get (name, @this);
        if (result != null)
        {
          return result;
        }
      }
      RCClosure parent = closure;
      RCValue val = null;
      while (parent != null)
      {
        IRefable result = parent.Result;
        if (result != null)
        {
          val = result.Get (name, @this);
        }
        if (val != null)
        {
          break;
        }
        if (!parent.NoClimb)
        {
          parent = parent.Parent;
        }
        else
        {
          break;
        }
      }
      if (val == null && !returnNull)
      {
        //Delimit thing is annoying.
        throw new RCException (closure, RCErrors.Name,
                               "Unable to resolve name " + RCReference.Delimit (name, "."));
      }
      return val;
    }

    /// <summary>
    /// Start evaluation for a user-defined operator.
    /// </summary>
    public static void DoEvalUserOp (RCRunner runner, RCClosure closure, UserOperator op)
    {
      RCValue code = closure.UserOp;
      RCArray<RCBlock> @this = closure.UserOpContext;
      if (code == null)
      {
        if (op.m_reference.Parts.Count > 1)
        {
          @this = new RCArray<RCBlock> ();
        }
        code = Resolve (op.m_reference.m_static, closure, op.m_reference.Parts, @this);
      }
      if (code == null)
      {
        throw new Exception ("Cannot find definition for operator: " + op.m_reference.Name);
      }
      //RCSystem.Log.Record (closure, "invoke", 0, op.m_reference.Name, code);
      code.Eval (runner, UserOpClosure (closure, code, @this, noClimb:false));
    }

    public static RCClosure UserOpClosure (RCClosure previous, RCValue code, RCArray<RCBlock> @this, bool noClimb)
    {
      return UserOpClosure (previous, code, @this, null, null, noClimb);
    }

    public static RCClosure UserOpClosure (RCClosure previous,
                                           RCValue code,
                                           RCArray<RCBlock> @this,
                                           RCValue left,
                                           RCValue right)
    {
      return UserOpClosure (previous, code, @this, left, right, noClimb:false);
    }

    /// <summary>
    /// This method creates an identical closure where the left and right
    /// arguments can be accessed in user space.
    /// This has to be done by operators that evaluate user provided code.
    /// </summary>
    public static RCClosure UserOpClosure (RCClosure previous,
                                           RCValue code,
                                           RCArray<RCBlock> @this,
                                           RCValue left,
                                           RCValue right, bool noClimb)
    {
      left = left != null ? left : previous.Result.Get ("0");
      right = right != null ? right : previous.Result.Get ("1");
      RCBlock result = null;
      //But what if Parent is null? Can that happen?
      if (@this != null && @this.Count > 0)
      {
        result = @this [0];
        //This is only for when the this context contains more than one object.
        //I'm not even sure whether to support this, I guess I should.
        //But this is not going to be the fastest solution possible.
        for (int i = 1; i < @this.Count; ++i)
        {
          for (int j = 0; j < @this [i].Count; ++j)
          {
            RCBlock block = @this [i].GetName (j);
            result = new RCBlock (result, block.Name, ":", block.Value);
          }
        }
      }
      if (left == null)
      {
        result = new RCBlock (result, "R", ":", right);
      }
      else
      {
        result = new RCBlock (result, "L", ":", left);
        result = new RCBlock (result, "R", ":", right);
      }
      //Passing code and @this here is important. NextParentOf will look
      //for the body of the executing function in that variable to detect tail calls.
      RCClosure replacement = new RCClosure (previous.Bot,
                                             previous.Fiber,
                                             previous.Locks,
                                             previous.Parent,
                                             previous.Code,
                                             previous.Left,
                                             result,
                                             previous.Index, code, @this, noClimb);
      RCClosure child = new RCClosure (replacement,
                                       previous.Bot,
                                       code,
                                       previous.Left,
                                       RCBlock.Empty,
                                       0, code, @this);
      return child;
    }

    public static void DoEvalInline (RCRunner runner, RCClosure closure, InlineOperator op)
    {
      op.m_code.Eval (runner, UserOpClosure (closure, op.m_code, null, noClimb:false));
    }

    public static void DoEvalTemplate (RCRunner runner, RCClosure closure, RCTemplate template)
    {
      throw new Exception ("Not implemented");
    }

    public static void DoYield (RCRunner runner, RCClosure closure, RCValue result)
    {
      DoYield (runner, closure, result, canonical:false);
    }

    public static void DoYield (RCRunner runner, RCClosure closure, RCValue result, bool canonical)
    {
      if (result == null)
      {
        throw new ArgumentNullException ("result");
      }
      //Check to see if this fiber was killed before moving on
      RCBot bot = runner.GetBot (closure.Bot);
      if (bot.IsFiberDone (closure.Fiber))
      {
        runner.Continue (closure, null);
        return;
      }
      //Do not permit any further changes to result or its children values.
      result.Lock (canonical);
      RCClosure next = closure.Code.Next (runner, closure, closure, result);
      if (next == null)
      {
        result = closure.Code.Finish (runner, closure, result);
        bot.ChangeFiberState (closure.Fiber, "done");
        RCSystem.Log.Record (closure, "fiber", closure.Fiber, "done", result);
        if (closure.Fiber == 0 && closure.Bot == 0)
        {
          runner.Finish (closure, result);
        }
        //This will handle fibers that wake up from some suspended state
        //without realizing that they have been killed.
        else
        {
          bot.FiberDone (runner, closure.Bot, closure.Fiber, result);
        }
        //Remove closure from the pending queue.
        runner.Continue (closure, null);
      }
      else
      {
        runner.Continue (closure, next);
      }
    }

    //Construct the next closure, default case.
    public static RCClosure DoNext (RCValue val, 
                                    RCRunner runner, 
                                    RCClosure tail, 
                                    RCClosure previous, 
                                    RCValue result)
    {
      if (previous.Parent != null)
      {
        return previous.Parent.Code.Next (runner,
                                          tail == null ? previous : tail,
                                          previous.Parent,
                                          result);
      }
      else return null;
    }

    //Construct the next closure for a block.
    public static RCClosure DoNext (RCBlock block, 
                                    RCRunner runner, 
                                    RCClosure tail, 
                                    RCClosure previous, 
                                    RCValue result)
    {
      if (previous.Index < block.Count - 1)
      {
        return new RCClosure (previous.Bot,
                              previous.Fiber, previous.Locks,
                              previous.Parent, block, previous.Left,
                              NextBlock (runner, block, previous, result),
                              previous.Index + 1,
                              previous.UserOp, previous.UserOpContext, noClimb:false);
      }
      else if (previous.Parent != null)
      {
        if (block.Count == 0)
        {
          return previous.Parent.Code.Next (runner,
                                            tail,
                                            previous.Parent,
                                            result);
        }
        else if (block.Evaluator.Return && previous.Index == block.Count - 1)
        {
          return previous.Parent.Code.Next (runner,
                                            tail,
                                            previous.Parent,
                                            result);
        }
        else
        {
          return previous.Parent.Code.Next (runner,
                                            tail,
                                            previous.Parent,
                                            NextBlock (runner, block, previous, result));
        }
      }
      else return null;
    }

    protected static RCBlock NextBlock (RCRunner runner, 
                                        RCBlock block, 
                                        RCClosure previous, 
                                        RCValue val)
    {
      RCBlock code = block.GetName (previous.Index);
      RCBlock result = new RCBlock (previous.Result,
                                    code.Name,
                                    code.Evaluator.Next,
                                    val);
      runner.Output (previous,
                     new RCSymbolScalar (null, code.Name), val);
      return result;
    }

    //Construct the next closure for an operator.
    public static RCClosure DoNext (RCOperator op, 
                                    RCRunner runner, 
                                    RCClosure head, 
                                    RCClosure previous, 
                                    RCValue result)
    {
      if (op.Left == null)
      {
        if (previous.Index == 0)
        {
          RCValue userop;
          RCArray<RCBlock> useropContext;
          RCClosure nextParentOf = NextParentOf (op, previous, out userop, out useropContext);
          RCClosure next = new RCClosure (nextParentOf,
                                          head.Bot,
                                          op,
                                          previous.Left,
                                          new RCBlock (null, "1", ":", result),
                                          previous.Index + 1,
                                          userop, useropContext);
          return next;
        }
        else if (previous.Index == 1 && previous.Parent != null)
        {
          return previous.Parent.Code.Next (runner,
                                            head == null ? previous : head,
                                            previous.Parent,
                                            result);
        }
        else return null;
      }
      else
      {
        if (previous.Index == 0)
        {
          return new RCClosure (previous.Parent,
                                head.Bot,
                                op,
                                result,
                                previous.Result,
                                previous.Index + 1, previous.UserOp, previous.UserOpContext);
        }
        else if (previous.Index == 1)
        {
          RCValue userop;
          RCArray<RCBlock> useropContext;
          RCClosure next = new RCClosure (NextParentOf (op, previous, out userop, out useropContext),
                                          head.Bot,
                                          op,
                                          //reset "pocket" left to null.
                                          null,
                                          //fold it into the current context for the final eval.
                                          new RCBlock (new RCBlock (null, "0", ":", previous.Left), "1", ":", result),
                                          previous.Index + 1,
                                          userop, useropContext);
          return next;
        }
        else if (previous.Index == 2 && previous.Parent != null)
        {
          return previous.Parent.Code.Next (runner,
                                            head == null ? previous : head,
                                            previous.Parent,
                                            result);
        }
        else if (previous.Parent != null && previous.Parent.Parent != null)
        {
          return previous.Parent.Parent.Code.Next (runner,
                                                   head == null ? previous : head,
                                                   previous.Parent.Parent,
                                                   result);
        }
        else return null;
      }
    }

    protected static RCClosure NextParentOf (RCOperator op, 
                                             RCClosure previous, 
                                             out RCValue userop,
                                             out RCArray<RCBlock> useropContext)
    {
      //The only operator with IsHigherOrder set is switch.
      //Why is switch magical, why not each, take, fiber, etc...
      //Am I just missing tests for those? 
      userop = null;
      useropContext = null;
      RCClosure argument0, argument1;
      bool recursion = false;
      if (previous.Parent == null)
      {
        return previous.Parent;
      }
      recursion = CheckForRecursion (op, previous, ref userop, ref useropContext);
      if (!recursion)
      {
        return previous.Parent;
      }
      /*
      else if (!previous.Parent.Code.IsLastCall (previous.Parent, previous))
      {
        if (!recursion)
        {
          return previous.Parent;
        }
      }
      */
      RCClosure parent0 = OwnerOpOf (op, previous, out argument0);
      if (parent0 == null)
      {
        return previous.Parent;
      }
      if (!parent0.Code.IsLastCall (parent0, argument0))
      {
        if (!recursion)
        {
          return previous.Parent;
        }
      }
      RCClosure parent1 = OwnerOpOf (op, parent0, out argument1);
      if (parent1 == null)
      {
        //parent1 is null when there is no switch in the last line.
        return previous.Parent;
      }
      if (!parent1.Code.IsLastCall (parent1, argument1))
      {
        return previous.Parent;
      }
      return parent1.Parent;
    }

    public static void PreresolveUserOp (RCClosure closure,
                                         RCValue op,
                                         ref RCValue resolved,
                                         ref RCArray<RCBlock> context)
    {
      //Now we just did the this context work so you know what that means.
      //We have to pass in this.
      UserOperator name = op as UserOperator;
      if (name == null)
      {
        return;
      }
      if (name.m_reference.Parts.Count > 1)
      {
        context = new RCArray<RCBlock> ();
      }
      resolved = Resolve (null, closure.Parent, name.m_reference.Parts, context);
    }

    public static bool CheckForRecursion (RCValue op,
                                          RCClosure previous,
                                          ref RCValue userop,
                                          ref RCArray<RCBlock> useropContext)
    {
      UserOperator name = op as UserOperator;
      if (name != null)
      {
        //Console.Out.WriteLine ("CHECKING FOR TAIL RECURSION");
        PreresolveUserOp (previous, op, ref userop, ref useropContext);
        RCClosure current = previous.Parent;
        while (current != null)
        {
          UserOperator code = current.Code as UserOperator;
          if (code != null)
          {
            if (userop == current.UserOp)
            {
              //Console.Out.WriteLine ("TAIL RECURSION DETECTED");
              return true;
            }
          }
          current = current.Parent;
        }
      }
      //Console.Out.WriteLine ("NO TAIL RECURSION HERE BUB");
      return false;
    }

    public static bool DoIsBeforeLastCall (RCClosure closure, RCOperator op)
    {
      return false;
      // We now check explicitly for the tail recursive case
      // This means less need to worry about "last call" status
      // UserOperator is the only place where this operation might return true now.
      // There is never a need to eliminate the tail stack frame in the case of a built-in.
      // Still I don't feel quite ready to excise all of this "last call" code.
      // We will save that until the production system is ready.
      // So that we can do a full and proper regression.
      // It's possible there is something I'm missing.
      /*
      if (closure.Index == 0)
      {
        return op.Left == null;
      }
      else
      {
        return closure.Index == 1;
      }
      */
    }

    public static bool DoIsLastCall (RCClosure closure, 
                                     RCClosure arg, 
                                     RCBlock block)
    {
      //Costly call to GetName, will want to address this at some point.
      //return block.Evaluator.Return;
      return block.GetName (closure.Index).Evaluator.Return;
    }

    public static bool DoIsLastCall (RCClosure closure, 
                                     RCClosure arg, 
                                     RCOperator op)
    {
      if (closure.Index == 1)
      {
        return op.Left == null;
      }
      else
      {
        return closure.Index == 2;
      }
    }

    public static bool DoIsLastCall (RCClosure closure, 
                                     RCClosure arg, 
                                     UserOperator op)
    {
      //Commenting these lines out breaks the case where the last
      //op is not really the last op. But why?
      if (arg != null)
      {
        bool result = arg.Code.IsLastCall (arg, null);
        if (result)
        {
          result = DoIsLastCall (closure, arg, (RCOperator) op);
        }
        return result;
      }
      else
      {
        bool result = DoIsLastCall (closure, arg, (RCOperator) op);
        return result;
      }
    }

    protected static RCClosure OwnerOpOf (RCOperator op, 
                                          RCClosure closure, 
                                          out RCClosure child)
    {
      child = closure;
      RCClosure current = closure.Parent;
      while (current != null && !current.Code.ArgumentEval)
      {
        child = current;
        current = current.Parent;
      }
      return current;
    }

    public static RCValue DoFinish (RCRunner runner, 
                                    RCClosure closure, 
                                    RCValue result)
    {
      while (closure != null)
      {
        RCBlock obj = closure.Code as RCBlock;
        if (obj != null)
        {
          if (obj.Count > 0 && obj.Evaluator.FinishBlock && result != closure.Code)
          {
            result = NextBlock (runner, obj, closure, result);
          }
        }
        RCOperator op = closure.Code as RCOperator;
        if (op != null)
        {
          result = op.Finish (result);
        }
        if (closure.Parent != null &&
            (closure.Parent.Bot != closure.Bot ||
             closure.Parent.Fiber != closure.Fiber))
        {
          break;
        }
        closure = closure.Parent;
      }
      return result;
    }
  }
}

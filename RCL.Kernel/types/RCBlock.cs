using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  //What is this garbage we should get rid of this.
  public interface IRefable
  {
    RCValue Get (RCArray<string> name, RCArray<RCBlock> context);
    RCValue Get (string[] name, RCArray<RCBlock> context);
    RCValue Get (string name);
  }

  public class RCName
  {
    protected static object m_lock = new object ();
    protected static Dictionary<string, RCName> m_names = new Dictionary<string, RCName> ();
    protected static RCArray<RCName> m_index = new RCArray<RCName> ();

    static RCName ()
    {
      RCName empty = new RCName ("", 0);
      m_names.Add ("", empty);
      m_index.Write (empty);
    }

    static RCName GetName (string text)
    {
      if (text == null) 
      {
        text = "";    
      }
      string name = null;
      RCName result;
      lock (m_lock)
      {
        if (!m_names.TryGetValue (text, out result))
        {
          if (text[0] == '\'')
          {
            if (text.Length == 1 || text[text.Length - 1] != '\'')
            {
              throw new Exception ("Unmatched single quote in name: " + text);
            }
            //remove quotes if not necessary          
            for (int i = 1; i < text.Length - 1; ++i)
            {
              if (!RCTokenType.IsIdentifierChar (text[i]))
              { 
                name = text;
                break;  
              }
            }
            if (name == null)
            {
              name = text.Substring (1, text.Length - 2);
            }
          }
          else
          {
            for (int i = 0; i < text.Length; ++i)
            {
              //add quotes if necessary
              if (!RCTokenType.IsIdentifierChar (text[i]))
              {
                name = "'" + text + "'";
                break;
              }
            }
            if (name == null)
            {
              name = text;
            }
          }
          if (m_names.TryGetValue (name, out result))
          {
            //this makes it a synonym for next time
            m_names.Add (text, result);
            return result;
          }
          else
          {
            result = new RCName (name, m_names.Count);
            m_names.Add (result.Text, result);
            m_index.Write (result);
            return result;
          }
        }
        else 
        {
          return result;
        }
      }
    }

    public static string Get (string name)
    {
      return GetName (name).Text;
    }

    public static long Num (string name)
    {
      return GetName (name).Index; 
    }

    public static string RawName (string name)
    {
      if (name == null || name.Length == 0)
      {
        return "";
      }
      else if (name[0] == '\'')
      {
        return name.Substring (1, name.Length - 2);
      }
      else 
      {
        return name;
      }
    }

    protected readonly string Text;
    protected readonly long Index;
    public RCName (string text, long index)
    {
      Text = text;
      Index = index;
    }
  }

  public class RCEvaluator
  {
    public static readonly RCEvaluator Let, Quote, Yield, Yiote, Yiyi, Apply, Expand;

    public readonly string Symbol;
    public readonly bool Return;
    public readonly bool Invoke;
    public readonly bool Pass;
    public readonly bool Template;
    public readonly bool FinishBlock;
    public readonly RCEvaluator Next;

    static RCEvaluator ()
    {
      Let = new RCEvaluator (":", true, false, false, false, false, true, Let);
      Quote = new RCEvaluator ("::", false, false, false, false, true, true, Let);
      Yield = new RCEvaluator ("<-", true, true, false, false, false, false, Yield);
      Yiote = new RCEvaluator ("<-:", false, false, false, false, true, true, Yield);
      Yiyi = new RCEvaluator ("<--", true, false, false, false, false, true, Yield);  
      Apply = new RCEvaluator ("<+", false, false, true, false, false, false, Apply);
      Expand = new RCEvaluator ("<&", false, true, false, true, false, false, Expand);
    }

    public RCEvaluator (string symbol,
                        bool evaluate,
                        bool @return,
                        bool invoke,
                        bool template,
                        bool pass,
                        bool finishBlock,
                        RCEvaluator next)
    {
      Symbol = symbol;
      Pass = pass;
      Return = @return;
      Invoke = invoke;
      Template = template;
      FinishBlock = finishBlock;
      Next = next == null ? this : next;
    }

    public static RCEvaluator For (string symbol)
    {
      if (symbol.Equals (":"))
      {
        return Let;
      }
      else if (symbol.Equals ("::"))
      {
        return Quote;
      }
      else if (symbol.Equals ("<-"))
      {
        return Yield;
      }
      else if (symbol.Equals ("<-:"))
      {
        return Yiote;
      }
      else if (symbol.Equals ("<--"))
      {
        return Yiyi;
      }
      else if (symbol.Equals ("<+"))
      {
        return Apply;
      }
      else if (symbol.Equals ("<&"))
      {
        return Expand;
      }
      throw new Exception ("Unknown evaluator: " + symbol);
    }
  }

  public class RCBlock : RCValue, IRefable
  {
    public readonly static RCBlock Empty = new RCBlock ();
    public readonly RCBlock Previous;
    public readonly RCValue Value;
    public readonly bool EscapeName = false;

    public readonly string Name;
    public readonly RCEvaluator Evaluator;
    protected readonly int m_count;

    protected RCBlock ()
    {
      //this is the empty block.
      //all blocks are eventually rooted to the empty block.
      m_count = 0;
    }

    public RCBlock (RCBlock previous, string name, RCEvaluator evaluator, RCValue val)
    {
      if (val == null)
      {
        throw new ArgumentNullException ("value");
      }

      Previous = previous != null ? previous : Empty;
      Name = RCName.Get (name);
      Evaluator = evaluator;
      Value = val;
      m_count = Previous.Count + 1;
    }

    public RCBlock (RCBlock previous, string name, string instr, RCValue val)
      : this (previous, name, RCEvaluator.For (instr), val) {}

    public RCBlock (string name, string op, RCValue value)
      : this (null, name, op, value) {}

    public override string TypeName
    {
      get { return "block"; }
    }

    public override char TypeCode
    {
      get { return 'k'; }
    }

    public RCBlock GetName (string name)
    {
      name = RCName.Get (name);
      RCBlock current = this;
      while (current != null && current.Count > 0)
      {
        //The empty block has a null name.
        //This may not be for the best.
        if (current.Name != null && current.Name.Equals (name))
        {
          return current;
        }
        current = current.Previous;
      }
      return null;
    }

    public RCValue Get (string[] name, RCArray<RCBlock> @this)
    {
      IRefable block = this;
      RCValue value = null;
      for (int i = 0; i < name.Length; ++i)
      {
        value = block.Get (name[i]);
        block = value as IRefable;
        if (block == null)
        {
          return value;
        }
        else if (@this != null)
        {
          @this.Write ((RCBlock) block);
        }
      }
      return value;
    }

    public RCValue Get (RCArray<string> name, RCArray<RCBlock> @this)
    {
      IRefable block = this;
      RCValue value = null;
      for (int i = 0; i < name.Count; ++i)
      {
        value = block.Get (name[i]);
        block = value as IRefable;
        if (block == null)
        {
          //if it is the last value return it
          if (i == name.Count - 1)
          {
            return value;
          }
          //if not, something is wrong
          else return null;
        }
        if (@this != null && i < name.Count - 1)
        {
          @this.Write ((RCBlock) block);
        }
      }
      return value;
    }

    public RCValue Get (string name)
    {
      RCBlock obj = GetName (name);
      if (obj == null)
      {
        return null;
      }
      return obj.Value;
    }

    public RCValue Get (string name, RCValue @default)
    {
      RCValue actual = Get (name);
      if (actual == null)
      {
        actual = @default;
      }
      return actual;
    }

    public RCBlock GetName (long index)
    {
      RCBlock current = this;
      if (index < 0)
      {
        for (long i = 1; i < -index; ++i)
        {
          current = current.Previous;
        }
      }
      else
      {
        for (long i = Count - 1; i > index; --i)
        {
          current = current.Previous;
        }
      }
      return current;
    }

    public string RawName
    {
      get
      {
        return RCName.RawName (Name);
      }
    }

    public RCValue Get (long index)
    {
      RCBlock obj = GetName (index);
      if (obj == null)
      {
        return null;
      }
      return obj.Value;
    }

    public override RCValue Edit (RCRunner runner, RCValueDelegate editor)
    {
      RCValue val = base.Edit (runner, editor);
      if (val == null)
      {
        RCBlock result = null;
        for (int i = 0; i < Count; ++i)
        {
          RCBlock name = GetName (i);
          val = name.Value.Edit (runner, editor);
          if (val != null)
          {
            if (result == null)
            {
              result = name.Previous;
              if (result == null)
              {
                result = RCBlock.Empty;
              }
            }
            result = new RCBlock (result, name.Name, Evaluator, val);
          }
          else
          {
            //This avoids creating an identical copy if nothing is to be changed
            //on any of the child values.
            if (result == null)
            {
              result = name;
            }
            else
            {
              result = new RCBlock (result, name.Name, name.Evaluator, name.Value);
            }
          }
        }
        return result;
      }
      else return val;
    }

    public override void Eval (RCRunner runner, RCClosure closure)
    {
      RCL.Kernel.Eval.DoEval (runner, closure, this);
    }

    public override RCClosure Next (RCRunner runner,
                                    RCClosure tail,
                                    RCClosure previous,
                                    RCValue result)
    {
      return RCL.Kernel.Eval.DoNext (this, runner, tail, previous, result);
    }

    public override bool IsLastCall (RCClosure closure, RCClosure arg)
    {
      return RCL.Kernel.Eval.DoIsLastCall (closure, arg, this);
    }

    public override void Format (StringBuilder builder, 
                                 RCFormat args, 
                                 int level)
    {
      RCL.Kernel.Format.DoFormat (this, builder, args, level);
    }

    public override int Count
    {
      get { return m_count; }
    }

    public override void Lock ()
    {
      base.Lock ();
      RCBlock current = this;
      while (current.Count > 0)
      {
        current.Value.Lock ();
        current = current.Previous;
      }
    }

    public override bool IsBlock
    {
      get { return true; }
    }

    public override void ToByte (RCArray<byte> result)
    {
      Binary.WriteBlock (result, this);
    }

    public override void Cubify (RCCube target, Stack<object> names)
    {
      for (int i = 0; i < this.Count; ++i)
      {
        RCBlock child = this.GetName (i);
        if (child.Name == "")
        {
          names.Push ((long) i);
        }
        else
        {
          names.Push (child.Name);
        }
        child.Value.Cubify (target, names);
        names.Pop ();
      }
    }
  }
}

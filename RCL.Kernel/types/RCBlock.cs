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
      RCName empty = new RCName ("", 0, false);
      m_names.Add ("", empty);
      m_index.Write (empty);
    }

    public static RCName GetName (string text)
    {
      if (text == null) 
      {
        text = "";    
      }
      string name = null;
      bool escaped = false;
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
            // Remove quotes if not necessary
            // They are necessary when the name begins with a number
            if (text.Length > 1 && text[1] >= '0' && text[1] <= '9')
            {
              name = text;
            }
            for (int i = 1; i < text.Length - 1; ++i)
            {
              if (!RCTokenType.IsIdentifierChar (text[i]))
              { 
                name = text;
                escaped = true;
                break;
              }
            }
            if (name == null)
            {
              name = text.Substring (1, text.Length - 2);
            }
          }
          else if (text[0] >= '0' && text[0] <= '9')
          {
            name = "'" + text + "'";
            escaped = true;
          }
          else
          {
            for (int i = 0; i < text.Length; ++i)
            {
              //add quotes if necessary
              if (!RCTokenType.IsIdentifierChar (text[i]))
              {
                name = "'" + text + "'";
                escaped = true;
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
            result = new RCName (name, m_names.Count, escaped);
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

    public readonly string Text;
    public readonly long Index;
    public readonly bool Escaped;
    public RCName (string text, long index, bool escaped)
    {
      Text = text;
      Index = index;
      Escaped = escaped;
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
      RCName nameInfo = RCName.GetName (name);
      Name = nameInfo.Text;
      EscapeName = nameInfo.Escaped;
      Evaluator = evaluator;
      Value = val;
      m_count = Previous.Count + 1;
    }

    public RCBlock (RCBlock previous, string name, string instr, params long[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCLong (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params string[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCString (val)) { }

    public RCBlock (RCBlock previous, string name, string instr, params double[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCDouble (val)) { }

    public RCBlock (RCBlock previous, string name, string instr, params decimal[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCDecimal (val)) { }

    public RCBlock (RCBlock previous, string name, string instr, params bool[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCBoolean (val)) { }

    public RCBlock (RCBlock previous, string name, string instr, params RCIncrScalar[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCIncr (val)) { }

    public RCBlock (RCBlock previous, string name, string instr, params RCTimeScalar[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCTime (val)) { }

    public RCBlock (RCBlock previous, string name, string instr, params RCSymbolScalar[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCSymbol (val)) { }

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
      RCL.Kernel.Format.DoFormat (this, builder, args, null, level);
    }

    public override void Format (StringBuilder builder, 
                                 RCFormat args,
                                 RCColmap colmap,
                                 int level)
    {
      RCL.Kernel.Format.DoFormat (this, builder, args, colmap, level);
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

    public void CheckString (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 's')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a string. Type is {0}.", val.TypeName));
      }
    }

    public void CheckStringIsIn (string name, params string[] values)
    {
      string val = GetString (name);
      for (int i = 0; i < values.Length; ++i)
      {
        if (values[i] == val)
        {
          return;
        }
      }
      StringBuilder message = new StringBuilder ();
      message.AppendFormat ("The variable '{0}' had value \"{1}\". Valid values are ", name, val);
      for (int i = 0; i < values.Length; ++i)
      {
        message.Append (values[i]);
        if (i < values.Length -1)
        {
          message.Append (", ");
        }
      }
      throw new Exception (message.ToString ());
    }

    public void CheckBoolean (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 'b')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a boolean. Type is {0}.", val.TypeName));
      }
    }

    public void CheckLong (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 'l')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a long. Type is {0}.", val.TypeName));
      }
    }

    public void CheckLongIsBetween (string name, long min, long max)
    {
      long val = GetLong (name);
      if (val < min || val > max)
      {
        throw new Exception (string.Format ("The variable '{0}' is not between {1} and {2}. Value is {3}.", name, min, max, val));
      }
    }

    public void CheckDouble (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 'd')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a double. Type is {0}.", val.TypeName));
      }
    }

    public void CheckDoubleIsBetween (string name, double min, double max)
    {
      double val = GetDouble (name);
      if (val < min || val > max)
      {
        throw new Exception (string.Format ("The variable '{0}' is not between {1} and {2}. Value is {3}.", name, min, max, val));
      }
    }

    public void CheckDecimal (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 'm')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a decimal. Type is {0}.", val.TypeName));
      }
    }

    public void CheckDecimalIsBetween (string name, decimal min, decimal max)
    {
      decimal val = GetDecimal (name);
      if (val < min || val > max)
      {
        throw new Exception (string.Format ("The variable '{0}' is not between {1} and {2}. Value is {3}.", name, min, max, val));
      }
    }

    public void CheckTime (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 't')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a time. Type is {0}.", val.TypeName));
      }
    }

    public void CheckTimeIsBetween (string name, RCTimeScalar min, RCTimeScalar max)
    {
      RCTimeScalar val = GetTime (name);
      if (val.Ticks < min.Ticks || val.Ticks > max.Ticks)
      {
        throw new Exception (string.Format ("The variable '{0}' is not between {1} and {2}. Value is {3}.", name, min, max, val));
      }
    }

    public void CheckSymbol (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 'y')
      {
        throw new Exception (string.Format ("The variable '{0}' is not a symbol. Type is {0}.", val.TypeName));
      }
    }

    public void CheckIncr (string name)
    {
      RCValue val = Get (name);
      if (val == null)
      {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.", name));
      }
      else if (val.TypeCode != 'n')
      {
        throw new Exception (string.Format ("The variable '{0}' is not an incr op. Type is {0}.", val.TypeName));
      }
    }

    public RCBlock GetBlock (string name, RCBlock def)
    {
      RCBlock val = (RCBlock) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val;
    }

    public RCBlock GetBlock (string name)
    {
      RCBlock val = (RCBlock) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val;
    }

    // Convenience functions for extracting individual values
    // in ordinary cases.
    // Useful for integrating with external apis and data.
    public string GetString (string name, string def)
    {
      RCString val = (RCString) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public string GetString (string name)
    {
      RCString val = (RCString) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public bool GetBoolean (string name, bool def)
    {
      RCBoolean val = (RCBoolean) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public bool GetBoolean (string name)
    {
      RCBoolean val = (RCBoolean) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public long GetLong (string name, long def)
    {
      RCLong val = (RCLong) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public long GetLong (string name)
    {
      RCLong val = (RCLong) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public double GetDouble (string name, double def)
    {
      RCDouble val = (RCDouble) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public double GetDouble (string name)
    {
      RCDouble val = (RCDouble) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public decimal GetDecimal (string name, decimal def)
    {
      RCDecimal val = (RCDecimal) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public decimal GetDecimal (string name)
    {
      RCDecimal val = (RCDecimal) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public RCTimeScalar GetTime (string name, RCTimeScalar def)
    {
      RCTime val = (RCTime) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public RCTimeScalar GetTime (string name)
    {
      RCTime val = (RCTime) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public RCSymbolScalar GetSymbol (string name, RCSymbolScalar def)
    {
      RCSymbol val = (RCSymbol) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public RCSymbolScalar GetSymbol (string name)
    {
      RCSymbol val = (RCSymbol) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }

    public RCIncrScalar GetIncr (string name, RCIncrScalar def)
    {
      RCIncr val = (RCIncr) Get (name);
      if (val == null)
      {
        return def;
      }
      else return val[0];
    }

    public RCIncrScalar GetIncr (string name)
    {
      RCIncr val = (RCIncr) Get (name);
      if (val == null)
      {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else return val[0];
    }
  }
}

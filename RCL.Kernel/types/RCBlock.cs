using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  
  public class RCBlock : RCValue, RCRefable
  {
    public readonly static RCBlock Empty = new RCBlock ();
    public readonly RCBlock Previous;
    public readonly RCValue Value;
    public readonly bool EscapeName = false;

    public readonly string Name;
    public readonly RCEvaluator Evaluator;
    protected readonly int _count;

    protected RCBlock ()
    {
      // this is the empty block.
      // all blocks are eventually rooted to the empty block.
      _count = 0;
    }

    public static RCBlock Append (RCBlock left, RCBlock right)
    {
      RCBlock result = left;
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock current = right.GetName (i);
        result = new RCBlock (result, current.Name, current.Evaluator, current.Value);
      }
      return result;
    }

    public RCBlock (RCBlock previous, string name, RCEvaluator evaluator, RCValue val)
    {
      if (val == null) {
        throw new ArgumentNullException ("value");
      }
      Previous = previous != null ? previous : Empty;
      RCName nameInfo = RCName.GetName (name);
      Name = nameInfo.Text;
      EscapeName = nameInfo.Escaped;
      Evaluator = evaluator;
      Value = val;
      _count = Previous.Count + 1;
    }

    public RCBlock (RCBlock previous, string name, string instr, params long[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCLong (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params string[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCString (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params double[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCDouble (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params decimal[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCDecimal (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params bool[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCBoolean (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params RCIncrScalar[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCIncr (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params RCTimeScalar[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCTime (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, params RCSymbolScalar[] val)
      : this (previous, name, RCEvaluator.For (instr), new RCSymbol (val)) {}

    public RCBlock (RCBlock previous, string name, string instr, RCValue val)
      : this (previous, name, RCEvaluator.For (instr), val) {}

    public RCBlock (string name, string op, RCValue value)
      : this (null, name, op, value) {}

    public override string TypeName
    {
      get { return RCValue.BLOCK_TYPENAME; }
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
        // The empty block has a null name.
        // This may not be for the best.
        if (current.Name != null && current.Name.Equals (name)) {
          return current;
        }
        current = current.Previous;
      }
      return null;
    }

    public RCValue Get (string[] name, RCArray<RCBlock> @this)
    {
      RCRefable block = this;
      RCValue value = null;
      for (int i = 0; i < name.Length; ++i)
      {
        value = block.Get (name[i]);
        block = value as RCRefable;
        if (block == null) {
          return value;
        }
        else if (@this != null) {
          @this.Write ((RCBlock) block);
        }
      }
      return value;
    }

    public RCValue Get (RCArray<string> name, RCArray<RCBlock> @this)
    {
      RCRefable block = this;
      RCValue value = null;
      for (int i = 0; i < name.Count; ++i)
      {
        value = block.Get (name[i]);
        block = value as RCRefable;
        if (block == null) {
          // if it is the last value return it
          if (i == name.Count - 1) {
            return value;
          }
          // if not, something is wrong
          else {
            return null;
          }
        }
        if (@this != null && i < name.Count - 1) {
          @this.Write ((RCBlock) block);
        }
      }
      return value;
    }

    public RCValue Get (RCSymbolScalar name, RCArray<RCBlock> @this)
    {
      RCRefable block = this;
      RCValue value = null;
      object[] array = name.ToArray ();
      for (int i = 0; i < array.Length; ++i)
      {
        if (array[i] is long) {
          long index = (long) array[i];
          value = block.Get (index);
        }
        else if (array[i] is string) {
          value = block.Get ((string) array[i]);
        }
        block = value as RCRefable;
        if (block == null) {
          // if it is the last value return it
          if (i == name.Length - 1) {
            return value;
          }
          // if not, something is wrong
          else {
            return null;
          }
        }
        if (@this != null && i < array.Length - 1) {
          @this.Write ((RCBlock) block);
        }
      }
      return value;
    }

    public RCValue Get (string name)
    {
      RCBlock obj = GetName (name);
      if (obj == null) {
        return null;
      }
      return obj.Value;
    }

    public RCValue Get (string name, RCValue @default)
    {
      RCValue actual = Get (name);
      if (actual == null) {
        actual = @default;
      }
      return actual;
    }

    public RCBlock GetName (long index)
    {
      RCBlock current = this;
      if (index < 0) {
        for (long i = 1; i < -index; ++i)
        {
          current = current.Previous;
        }
      }
      else {
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
      if (obj == null) {
        return null;
      }
      return obj.Value;
    }

    public Type GetType (int index)
    {
      RCVectorBase vector = this.Get (index) as RCVectorBase;
      if (vector != null) {
        return vector.GetElementType ();
      }
      else {
        throw new Exception ("GetType (index) assumes a vector");
      }
    }

    public bool HasNamedVariables ()
    {
      RCBlock current = this;
      while (current != null)
      {
        if (current.Name != "" && current.Name != null) {
          return true;
        }
        current = current.Previous;
      }
      return false;
    }

    /// <summary>
    /// DO NOT USE!!!
    /// </summary>
    public override RCValue Edit (RCRunner runner, RCValueDelegate editor)
    {
      RCValue val = base.Edit (runner, editor);
      if (val == null) {
        RCBlock result = null;
        for (int i = 0; i < Count; ++i)
        {
          RCBlock name = GetName (i);
          val = name.Value.Edit (runner, editor);
          if (val != null) {
            if (result == null) {
              result = name.Previous;
              if (result == null) {
                result = RCBlock.Empty;
              }
            }
            result = new RCBlock (result, name.Name, Evaluator, val);
          }
          else {
            // This avoids creating an identical copy if nothing is to be changed
            // on any of the child values.
            if (result == null) {
              result = name;
            }
            else {
              result = new RCBlock (result, name.Name, name.Evaluator, name.Value);
            }
          }
        }
        return result;
      }
      else {
        return val;
      }
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
      get { return _count; }
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
        if (child.Name == "") {
          names.Push ((long) i);
        }
        else {
          names.Push (child.Name);
        }
        child.Value.Cubify (target, names);
        names.Pop ();
      }
    }

    public void CheckName (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
    }

    public void CheckString (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 's') {
        throw new Exception (string.Format ("The variable '{0}' is not a string. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckStringIsIn (string name, params string[] values)
    {
      string val = GetString (name);
      for (int i = 0; i < values.Length; ++i)
      {
        if (values[i] == val) {
          return;
        }
      }
      StringBuilder message = new StringBuilder ();
      message.AppendFormat ("The variable '{0}' had value \"{1}\". Valid values are ", name, val);
      for (int i = 0; i < values.Length; ++i)
      {
        message.Append (values[i]);
        if (i < values.Length - 1) {
          message.Append (", ");
        }
      }
      throw new Exception (message.ToString ());
    }

    public void CheckBoolean (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 'b') {
        throw new Exception (string.Format ("The variable '{0}' is not a boolean. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckLong (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 'l') {
        throw new Exception (string.Format ("The variable '{0}' is not a long. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckLongIsBetween (string name, long min, long max)
    {
      long val = GetLong (name);
      if (val < min || val > max) {
        throw new Exception (string.Format (
                               "The variable '{0}' is not between {1} and {2}. Value is {3}.",
                               name,
                               min,
                               max,
                               val));
      }
    }

    public void CheckDouble (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 'd') {
        throw new Exception (string.Format ("The variable '{0}' is not a double. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckDoubleIsBetween (string name, double min, double max)
    {
      double val = GetDouble (name);
      if (val < min || val > max) {
        throw new Exception (string.Format (
                               "The variable '{0}' is not between {1} and {2}. Value is {3}.",
                               name,
                               min,
                               max,
                               val));
      }
    }

    public void CheckDecimal (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 'm') {
        throw new Exception (string.Format ("The variable '{0}' is not a decimal. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckDecimalIsBetween (string name, decimal min, decimal max)
    {
      decimal val = GetDecimal (name);
      if (val < min || val > max) {
        throw new Exception (string.Format (
                               "The variable '{0}' is not between {1} and {2}. Value is {3}.",
                               name,
                               min,
                               max,
                               val));
      }
    }

    public void CheckTime (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 't') {
        throw new Exception (string.Format ("The variable '{0}' is not a time. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckTimeIsBetween (string name, RCTimeScalar min, RCTimeScalar max)
    {
      RCTimeScalar val = GetTime (name);
      if (val.Ticks < min.Ticks || val.Ticks > max.Ticks) {
        throw new Exception (string.Format (
                               "The variable '{0}' is not between {1} and {2}. Value is {3}.",
                               name,
                               min,
                               max,
                               val));
      }
    }

    public void CheckSymbol (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 'y') {
        throw new Exception (string.Format ("The variable '{0}' is not a symbol. Type is {0}.",
                                            val.TypeName));
      }
    }

    public void CheckIncr (string name)
    {
      RCValue val = Get (name);
      if (val == null) {
        throw new Exception (string.Format ("The variable '{0}' was not found in this block.",
                                            name));
      }
      else if (val.TypeCode != 'n') {
        throw new Exception (string.Format ("The variable '{0}' is not an incr op. Type is {0}.",
                                            val.TypeName));
      }
    }

    public RCBlock GetBlock (string name, RCBlock def)
    {
      RCBlock val = (RCBlock) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val;
      }
    }

    public RCBlock GetBlock (string name)
    {
      RCBlock val = (RCBlock) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val;
      }
    }

    public RCBlock GetBlock (long i)
    {
      // Don't forget to update the rest of these
      RCBlock block = Get (i) as RCBlock;
      if (block == null) {
        RCValue val = Get (i);
        if (val != null) {
          throw new Exception (string.Format ("Value at index {0} was not a block. Type was {1}",
                                              i,
                                              val.TypeName));
        }
        throw new Exception (string.Format ("No value at index {0} within block (count {1})",
                                            i,
                                            Count));
      }
      else {
        return block;
      }
    }

    public string GetString (long i)
    {
      RCString val = (RCString) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block. Count: {1}",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public bool GetBoolean (long i)
    {
      RCBoolean val = (RCBoolean) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public long GetLong (long i)
    {
      RCLong val = (RCLong) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public double GetDouble (long i)
    {
      RCDouble val = (RCDouble) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public decimal GetDecimal (long i)
    {
      RCDecimal val = (RCDecimal) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public RCTimeScalar GetTime (long i)
    {
      RCTime val = (RCTime) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public RCSymbolScalar GetSymbol (long i)
    {
      RCSymbol val = (RCSymbol) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    public RCIncrScalar GetIncr (long i)
    {
      RCIncr val = (RCIncr) Get (i);
      if (val == null) {
        throw new Exception (string.Format ("No value at index {0} within block (count:{1})",
                                            i,
                                            Count));
      }
      return val[0];
    }

    // Convenience functions for extracting individual values
    // in ordinary cases.
    // Useful for integrating with external apis and data.
    public string GetString (string name, string def)
    {
      RCString val = (RCString) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public string GetString (string name)
    {
      RCString val = (RCString) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public bool GetBoolean (string name, bool def)
    {
      RCBoolean val = (RCBoolean) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public bool GetBoolean (string name)
    {
      RCBoolean val = (RCBoolean) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public long GetLong (string name, long def)
    {
      RCLong val = (RCLong) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public long GetLong (string name)
    {
      RCLong val = (RCLong) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public double GetDouble (string name, double def)
    {
      RCDouble val = (RCDouble) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public double GetDouble (string name)
    {
      RCDouble val = (RCDouble) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public decimal GetDecimal (string name, decimal def)
    {
      RCDecimal val = (RCDecimal) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public decimal GetDecimal (string name)
    {
      RCDecimal val = (RCDecimal) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public RCTimeScalar GetTime (string name, RCTimeScalar def)
    {
      RCTime val = (RCTime) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public RCTimeScalar GetTime (string name)
    {
      RCTime val = (RCTime) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public RCSymbolScalar GetSymbol (string name, RCSymbolScalar def)
    {
      RCSymbol val = (RCSymbol) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public RCSymbolScalar GetSymbol (string name)
    {
      RCSymbol val = (RCSymbol) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }

    public RCIncrScalar GetIncr (string name, RCIncrScalar def)
    {
      RCIncr val = (RCIncr) Get (name);
      if (val == null) {
        return def;
      }
      else {
        return val[0];
      }
    }

    public RCIncrScalar GetIncr (string name)
    {
      RCIncr val = (RCIncr) Get (name);
      if (val == null) {
        throw new Exception ("Required value " + name + " not found in block");
      }
      else {
        return val[0];
      }
    }
  }
}

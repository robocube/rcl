
using System;
using RCL.Kernel;

namespace RCL.Core
{
  public class Block
  {
    [RCVerb ("name")]
    public void EvalName (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      if (left.Count != 1) {
        throw new Exception ("left argument must have count 1");
      }
      RCBlock result = Name (left[0], right);
      runner.Yield (closure, result);
    }

    [RCVerb ("name")]
    public void EvalName (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      if (left.Count != 1) {
        throw new Exception ("left argument must have count 1");
      }
      string nameProperty = (string) left[0].Part (0);
      RCBlock result = Name (nameProperty, right);
      runner.Yield (closure, result);
    }

    protected RCBlock Name (string nameProperty, RCBlock right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock name = right.GetName (i);
        RCBlock val;
        if (name.Value is RCBlock) {
          val = (RCBlock) name.Value;
        }
        else {
          throw new Exception (string.Format ("Expected a block. Actual type was {0}",
                                              name.Value.TypeName));
        }
        RCValue newName = val.Get (nameProperty);
        string nameStr;
        RCString str = newName as RCString;
        if (str != null) {
          nameStr = str[0];
        }
        else {
          RCSymbol sym = newName as RCSymbol;
          if (sym != null) {
            nameStr = (string) sym[0].Part (0);
          }
          else {
            throw new Exception (
                    "The name must be a string or a symbol with a name in the first part");
          }
        }
        result = new RCBlock (result, nameStr, name.Evaluator, val);
      }
      return result;
    }

    [RCVerb ("names")]
    public void EvalNames (RCRunner runner, RCClosure closure, RCBlock right)
    {
      string[] result = new string[right.Count];
      for (int i = 0; i < result.Length; ++i)
      {
        RCBlock name = right.GetName (i);
        if (name.EscapeName) {
          result[i] = RCName.RawName (name.Name);
        }
        else {
          result[i] = name.Name;
        }
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("rename")]
    public void EvalRename (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      if (left.Count != right.Count && left.Count != 1) {
        throw new Exception ("left and right arguments must have the same length");
      }

      RCBlock result = RCBlock.Empty;
      if (left.Count == 1) {
        for (int i = 0; i < right.Count; ++i)
        {
          RCBlock name = right.GetName (i);
          result = new RCBlock (result, left[0], name.Evaluator, name.Value);
        }
      }
      else {
        for (int i = 0; i < left.Count; ++i)
        {
          RCBlock name = right.GetName (i);
          result = new RCBlock (result, left[i], name.Evaluator, name.Value);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("rename")]
    public void EvalRename (RCRunner runner, RCClosure closure, RCSymbol left, RCBlock right)
    {
      if (left.Count != right.Count && left.Count != 1) {
        throw new Exception ("left and right arguments must have the same length");
      }

      RCBlock result = RCBlock.Empty;
      if (left.Count == 1) {
        for (int i = 0; i < right.Count; ++i)
        {
          RCBlock name = right.GetName (i);
          string field = left[0].Part (0).ToString ();
          result = new RCBlock (result, field, name.Evaluator, name.Value);
        }
      }
      else {
        for (int i = 0; i < left.Count; ++i)
        {
          RCBlock name = right.GetName (i);
          string field = left[i].Part (0).ToString ();
          result = new RCBlock (result, field, name.Evaluator, name.Value);
        }
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("has")]
    public virtual void EvalHas (RCRunner runner, RCClosure closure, RCBlock left, RCSymbol right)
    {
      RCArray<bool> result = new RCArray<bool> ();
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (left.Get ((string) right[i].Key) != null);
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("has")]
    public virtual void EvalHas (RCRunner runner, RCClosure closure, RCBlock left, RCString right)
    {
      RCArray<bool> result = new RCArray<bool> ();
      for (int i = 0; i < right.Count; ++i)
      {
        result.Write (left.Get (right[i]) != null);
      }
      runner.Yield (closure, new RCBoolean (result));
    }

    [RCVerb ("unflip")]
    public void EvalUnflip (RCRunner runner, RCClosure closure, RCBlock right)
    {
      // Take a block of arrays and turn them into a block of rows.
      RCBlock[] blocks = new RCBlock[right.Get (0).Count];
      for (int i = 0; i < blocks.Length; ++i)
      {
        blocks[i] = RCBlock.Empty;
      }
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock name = right.GetName (i);
        RCVectorBase vector = (RCVectorBase) name.Value;
        for (int j = 0; j < vector.Count; ++j)
        {
          RCValue box = RCVectorBase.FromScalar (vector.Child (j));
          blocks[j] = new RCBlock (blocks[j], name.Name, ":", box);
        }
      }
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < blocks.Length; ++i)
      {
        result = new RCBlock (result, "", ":", blocks[i]);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("flip")]
    public void EvalFlip (RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCBlock prototype = (RCBlock) right.Get (0);
      string[] names = new string[prototype.Count];
      RCVectorBase[] columns = new RCVectorBase[prototype.Count];
      for (int i = 0; i < prototype.Count; ++i)
      {
        RCBlock name = prototype.GetName (i);
        RCVectorBase scalar = (RCVectorBase) name.Value;
        columns[i] = RCVectorBase.FromScalar (scalar.Child (0));
        names[i] = name.Name;
      }
      for (int i = 1; i < right.Count; ++i)
      {
        RCBlock row = (RCBlock) right.Get (i);
        for (int j = 0; j < row.Count; ++j)
        {
          RCBlock name = (RCBlock) row.GetName (j);
          int n = Array.IndexOf (names, name.Name);
          if (n >= 0) {
            RCVectorBase scalar = (RCVectorBase) name.Value;
            columns[n].Write (scalar.Child (0));
          }
        }
      }
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < names.Length; ++i)
      {
        result = new RCBlock (result, names[i], ":", columns[i]);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("string")]
    public void EvalString (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCString (CoerceBlock<string> (right)));
    }

    [RCVerb ("long")]
    public void EvalLong (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCLong (CoerceBlock<long> (right)));
    }

    [RCVerb ("double")]
    public void EvalDouble (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCDouble (CoerceBlock<double> (right)));
    }

    [RCVerb ("byte")]
    public void EvalByte (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCByte (CoerceBlock<byte> (right)));
    }

    [RCVerb ("boolean")]
    public void EvalBoolean (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCBoolean (CoerceBlock<bool> (right)));
    }

    [RCVerb ("decimal")]
    public void EvalDecimal (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCDecimal (CoerceBlock<decimal> (right)));
    }

    [RCVerb ("symbol")]
    public void EvalSymbol (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCSymbol (CoerceBlock<RCSymbolScalar> (right)));
    }

    [RCVerb ("block")]
    public void EvalBlock (RCRunner runner, RCClosure closure, RCTemplate right)
    {
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < right.Count - 1; ++i)
      {
        RCBlock current = right.GetName (i);
        result = new RCBlock (result, current.Name, current.Evaluator, current.Value);
      }
      runner.Yield (closure, result);
    }

    [RCVerb ("template")]
    public void EvalTemplate (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      if (right.Count > 1) {
        throw new RCException (closure,
                               RCErrors.Count,
                               "template only takes a single string (use & first).");
      }
      if (left.Count != 1) {
        throw new RCException (closure,
                               RCErrors.Count,
                               "escapeCount can only contain a single number");
      }
      runner.Yield (closure, CreateTemplate (right, left[0]));
    }

    [RCVerb ("template")]
    public void EvalTemplate (RCRunner runner, RCClosure closure, RCString right)
    {
      if (right.Count > 1) {
        throw new RCException (closure,
                               RCErrors.Count,
                               "template only takes a single string (use & first).");
      }
      runner.Yield (closure, CreateTemplate (right, 1));
    }

    protected RCTemplate CreateTemplate (RCString right, long escapeCount)
    {
      bool multiline = right[0].IndexOf ('\n') > -1;
      RCBlock def = new RCBlock ("", ":", right);
      return new RCTemplate (def, (int) escapeCount, multiline);
    }

    protected virtual RCArray<T> CoerceBlock<T> (RCBlock right)
    {
      RCArray<T> result = new RCArray<T> ();
      for (int i = 0; i < right.Count; ++i)
      {
        RCVector<T> value = (RCVector<T>)right.Get (i);
        for (int j = 0; j < value.Count; ++j)
        {
          result.Write (value[j]);
        }
      }
      return result;
    }
  }
}

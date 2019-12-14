using System;
using System.Text;
using RCL.Kernel;

namespace RCL.Core
{
  /// <summary>
  /// General purpose operations on vectors.
  /// </summary>
  public class Append
  {
    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCByte left, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoAppend<byte> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCDouble left, RCDouble right)
    {
      runner.Yield (closure, new RCDouble (DoAppend<double> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield (closure, new RCLong (DoAppend<long> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCString left, RCString right)
    {
      runner.Yield (closure, new RCString (DoAppend<string> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCDecimal left, RCDecimal right)
    {
      runner.Yield (closure, new RCDecimal (DoAppend<decimal> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCBoolean left, RCBoolean right)
    {
      runner.Yield (closure, new RCBoolean (DoAppend<bool> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCSymbol left, RCSymbol right)
    {
      runner.Yield (closure, new RCSymbol (DoAppend<RCSymbolScalar> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCTime left, RCTime right)
    {
      runner.Yield (closure, new RCTime (DoAppend<RCTimeScalar> (left, right)));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCBlock left, RCBlock right)
    {
      RCBlock result = RCBlock.Append (left, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCCube left, RCCube right)
    {
      RCCube result;
      if (left.Count > 0)
      {
        result = new RCCube (left);
      }
      else
      {
        result = new RCCube (right.Axis.Match ());
      }
      Writer writer = new Writer (result, null, true, true, 0);
      writer.Write (right);
      runner.Yield (closure, result);
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCString right)
    {
      StringBuilder builder = new StringBuilder ();
      for (int i = 0; i < right.Count; ++i)
      {
        builder.Append (right[i]);
      }
      runner.Yield (closure, new RCString (builder.ToString ()));
    }

    [RCVerb ("&")]
    public void EvalAppend (RCRunner runner, RCClosure closure, RCBlock right)
    {
      if (right.Count == 0)
      {
        runner.Yield (closure, RCBlock.Empty);
      }
      RCValue first = right.Get (0);
      RCVectorBase vector = first as RCVectorBase;
      if (vector != null)
      {
        RCVectorBase result;
        switch (vector.TypeCode)
        {
          case 'x' : result = new RCByte (DoAppend<byte> (right)); break;
          case 'l' : result = new RCLong (DoAppend<long> (right)); break;
          case 'd' : result = new RCDouble (DoAppend<double> (right)); break;
          case 'm' : result = new RCDecimal (DoAppend<decimal> (right)); break;
          case 's' : result = new RCString (DoAppend<string> (right)); break;
          case 'b' : result = new RCBoolean (DoAppend<bool> (right)); break;
          case 'y' : result = new RCSymbol (DoAppend<RCSymbolScalar> (right)); break;
          case 't' : result = new RCTime (DoAppend<RCTimeScalar> (right)); break;
          default: throw new Exception ("Type:" + vector.TypeCode + " is not supported by sort");
        }
        runner.Yield (closure, result);
        return;
      }
      RCCube cube = first as RCCube;
      if (cube != null)
      {
        RCCube result = new RCCube (cube);
        for (int i = 1; i < right.Count; ++i)
        {
          Writer writer = new Writer (result, null, true, true, 0);
          writer.Write ((RCCube) right.Get (i));
        }
        runner.Yield (closure, result);
        return;
      }
      // Individual values are now appended to the result just like the children of blocks.
      {
        RCBlock result = RCBlock.Empty;
        for (int i = 0; i < right.Count; ++i)
        {
          RCBlock top = right.GetName (i);
          if (top.Value is RCBlock)
          {
            RCBlock list = (RCBlock) right.Get (i);
            for (int j = 0; j < list.Count; ++j)
            {
              RCBlock item = list.GetName (j);
              result = new RCBlock (result, item.Name, ":", item.Value);
            }
          }
          else
          {
            result = new RCBlock (result, top.Name, ":", top.Value);
          }
        }
        runner.Yield (closure, result);
      }
    }

    protected RCArray<T> DoAppend<T> (RCBlock right)
    {
      RCVector<T> current = (RCVector<T>) right.Get (0);
      RCArray<T> result = new RCArray<T> (current.Count * 3);
      result.Write (current.Data);
      for (int i = 1; i < right.Count; ++i)
      {
        current = (RCVector<T>) right.Get (i);
        result.Write (current.Data);
      }
      return result;
    }

    protected T[] DoAppend<T> (RCVector<T> left, RCVector<T> right)
    {
      //Why did I do it this way instead of using write on RCArray?
      //I think it came before that, should change it.
      int length = left.Count + right.Count;
      T[] result = new T[length];
      int i = 0;
      for (; i < left.Count; ++i)
      {
        result[i] = left[i];
      }
      for (; i < length; ++i)
      {
        result[i] = right[i - left.Count];
      }
      return result;
    }
  }
}

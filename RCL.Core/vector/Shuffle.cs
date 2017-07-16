using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using RCL.Kernel;

namespace RCL.Core
{
  /// <summary>
  /// http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
  /// http://www.codinghorror.com/blog/2007/12/the-danger-of-naivete.html
  /// </summary>
  public class Shuffle
  {
    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCByte (DoShuffle<byte> (new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCByte right)
    {
      runner.Yield(closure, new RCByte(DoShuffle<byte>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield(closure, new RCLong(DoShuffle<long>(new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCLong right)
    {
      runner.Yield(closure, new RCLong(DoShuffle<long>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield(closure, new RCDouble(DoShuffle<double>(new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCDouble right)
    {
      runner.Yield(closure, new RCDouble(DoShuffle<double>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield(closure, new RCDecimal(DoShuffle<decimal>(new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCDecimal right)
    {
      runner.Yield(closure, new RCDecimal(DoShuffle<decimal>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield(closure, new RCString(DoShuffle<string>(new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      runner.Yield(closure, new RCString(DoShuffle<string>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield(closure, new RCBoolean(DoShuffle<bool>(new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCBoolean right)
    {
      runner.Yield(closure, new RCBoolean(DoShuffle<bool>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield(closure, new RCSymbol(DoShuffle<RCSymbolScalar>(new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCSymbol right)
    {
      runner.Yield(closure, new RCSymbol(DoShuffle<RCSymbolScalar>(new Random ((int)left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield(closure, new RCTime (DoShuffle<RCTimeScalar> (new Random (), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCTime right)
    {
      runner.Yield(closure, new RCTime (DoShuffle<RCTimeScalar> (new Random ((int) left[0]), right)));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield(closure, DoShuffle(new Random (), right));
    }

    [RCVerb ("shuffle")]
    public void EvalShuffle (
      RCRunner runner, RCClosure closure, RCLong left, RCBlock right)
    {
      runner.Yield(closure, DoShuffle(new Random((int)left[0]), right));
    }

    protected T[] DoShuffle<T> (Random random, RCVector<T> right)
    {
      //wikipedia discusses a variant of this algorithm that allows you to
      //initialize the array and shuffle it in a single operation.
      //It would be cool to implement that.
      T[] result = new T[right.Count];
      for(int i = 0; i < result.Length; ++i)
        result[i] = right[i];

      for (int i = result.Length - 1; i > 0; i--)
      {
        int n = random.Next(i + 1);
        T temp = result[i];
        result[i] = result[n];
        result[n] = temp;
      }
      return result;
    }

    protected RCBlock DoShuffle (Random random, RCBlock right)
    {
      //wikipedia discusses a variant of this algorithm that allows you to
      //initialize the array and shuffle it in a single operation.
      //It would be cool to implement that.
      string[] names = new string[right.Count];
      for(int i = 0; i < names.Length; ++i)
        names[i] = right.GetName (i).Name;

      for (int i = names.Length - 1; i > 0; i--)
      {
        int n = random.Next(i + 1);
        string temp = names[i];
        names[i] = names[n];
        names[n] = temp;
      }

      //Not close to optimal.  Help.
      RCBlock result = null;
      for(int i = 0; i < names.Length; ++i)
      {
        RCBlock name = right.GetName (names[i]);
        result = new RCBlock (result, name.Name, name.Evaluator, name.Value);
      }
      return result;
    }
  }
}

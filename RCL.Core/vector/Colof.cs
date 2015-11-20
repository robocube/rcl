
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Colof
  {
    [RCVerb ("colofx")]
    public void EvalColofx (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCByte (DoColof<byte> (0, right, false)));
    }

    [RCVerb ("colofx")]
    public void EvalColofx (RCRunner runner, RCClosure closure, RCByte left, RCCube right)
    {
      runner.Yield (closure, new RCByte (DoColof<byte> (left[0], right, true)));
    }

    [RCVerb ("colofd")]
    public void EvalColofd (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCDouble (DoColof<double> (0.0, right, false)));
    }

    [RCVerb ("colofd")]
    public void EvalColofd (RCRunner runner, RCClosure closure, RCDouble left, RCCube right)
    {
      runner.Yield (closure, new RCDouble (DoColof<double> (left[0], right, true)));
    }

    [RCVerb ("colofl")]
    public void EvalColofl (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCLong (DoColof<long> (0L, right, false)));
    }

    [RCVerb ("colofl")]
    public void EvalColofl (RCRunner runner, RCClosure closure, RCLong left, RCCube right)
    {
      runner.Yield (closure, new RCLong (DoColof<long> (left[0], right, true)));
    }

    [RCVerb ("colofs")]
    public void EvalColofs (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCString (DoColof<string> ("", right, false)));
    }

    [RCVerb ("colofs")]
    public void EvalColofs (RCRunner runner, RCClosure closure, RCString left, RCCube right)
    {
      runner.Yield (closure, new RCString (DoColof<string> (left[0], right, true)));
    }

    [RCVerb ("colofm")]
    public void EvalColofm (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCDecimal (DoColof<decimal> (0, right, false)));
    }

    [RCVerb ("colofm")]
    public void EvalColofm (RCRunner runner, RCClosure closure, RCDecimal left, RCCube right)
    {
      runner.Yield (closure, new RCDecimal (DoColof<decimal> (left[0], right, true)));
    }

    [RCVerb ("colofb")]
    public void EvalColofb (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCBoolean (DoColof<bool> (false, right, false)));
    }

    [RCVerb ("colofb")]
    public void EvalColofb (RCRunner runner, RCClosure closure, RCBoolean left, RCCube right)
    {
      runner.Yield (closure, new RCBoolean (DoColof<bool> (left[0], right, true)));
    }

    [RCVerb ("colofy")]
    public void EvalColofy (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCSymbol (DoColof<RCSymbolScalar> (RCSymbolScalar.Empty, right, false)));
    }

    [RCVerb ("colofy")]
    public void EvalColofy (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      runner.Yield (closure, new RCSymbol (DoColof<RCSymbolScalar> (left[0], right, true)));
    }

    [RCVerb ("coloft")]
    public void EvalColoft (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCTime (DoColof<RCTimeScalar> (RCTimeScalar.Empty, right, true)));
    }

    [RCVerb ("coloft")]
    public void EvalColoft (RCRunner runner, RCClosure closure, RCTime left, RCCube right)
    {
      runner.Yield (closure, new RCTime (DoColof<RCTimeScalar> (left[0], right, true)));
    }

    protected RCArray<T> DoColof<T> (T def, RCCube right, bool allowSparse)
    {
      if (right.Count == 0)
      {
        return new RCArray<T> (1);
      }
      RCArray<T> data = right.GetData<T> (0);
      RCArray<T> result;
      if (data.Count < right.Count)
      {
        if (!allowSparse)
        {
          throw new Exception ("There were missing values. Please specify a default value.");
        }
        result = new RCArray<T> (right.Count);
        RCArray<int> index = right.GetIndex<T> (0);
        for (int i = 0; i < index.Count; ++i)
        {
          while (result.Count < index[i])
          {
            result.Write (def);
          }
          result.Write (data [i]);
        }
      }
      else 
      {
        result = data;
      }
      return result;
    }
  }
}
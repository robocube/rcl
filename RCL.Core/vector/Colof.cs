
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
    [RCVerb ("gcol")]
    public void EvalGCol (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, (RCLong) right.Get ("G"));
    }

    [RCVerb ("ecol")]
    public void EvalECol (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, (RCLong) right.Get ("E"));
    }

    [RCVerb ("tcol")]
    public void EvalTCol (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, (RCTime) right.Get ("T"));
    }

    [RCVerb ("scol")]
    public void EvalSCol (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, (RCSymbol) right.Get ("S"));
    }

    [RCVerb ("colofx")]
    public void EvalColofx (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCByte (right.DoColof<byte> (0, 0, false)));
    }

    [RCVerb ("colofx")]
    public void EvalColofx (RCRunner runner, RCClosure closure, RCByte left, RCCube right)
    {
      runner.Yield (closure, new RCByte (right.DoColof<byte> (0, left[0], true)));
    }

    [RCVerb ("colofd")]
    public void EvalColofd (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCDouble (right.DoColof<double> (0, 0.0, false)));
    }

    [RCVerb ("colofd")]
    public void EvalColofd (RCRunner runner, RCClosure closure, RCDouble left, RCCube right)
    {
      runner.Yield (closure, new RCDouble (right.DoColof<double> (0, left[0], true)));
    }

    [RCVerb ("colofl")]
    public void EvalColofl (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCLong (right.DoColof<long> (0, 0L, false)));
    }

    [RCVerb ("colofl")]
    public void EvalColofl (RCRunner runner, RCClosure closure, RCLong left, RCCube right)
    {
      runner.Yield (closure, new RCLong (right.DoColof<long> (0, left[0], true)));
    }

    [RCVerb ("colofs")]
    public void EvalColofs (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCString (right.DoColof<string> (0, "", false)));
    }

    [RCVerb ("colofs")]
    public void EvalColofs (RCRunner runner, RCClosure closure, RCString left, RCCube right)
    {
      runner.Yield (closure, new RCString (right.DoColof<string> (0, left[0], true)));
    }

    [RCVerb ("colofm")]
    public void EvalColofm (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCDecimal (right.DoColof<decimal> (0, 0, false)));
    }

    [RCVerb ("colofm")]
    public void EvalColofm (RCRunner runner, RCClosure closure, RCDecimal left, RCCube right)
    {
      runner.Yield (closure, new RCDecimal (right.DoColof<decimal> (0, left[0], true)));
    }

    [RCVerb ("colofb")]
    public void EvalColofb (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCBoolean (right.DoColof<bool> (0, false, false)));
    }

    [RCVerb ("colofb")]
    public void EvalColofb (RCRunner runner, RCClosure closure, RCBoolean left, RCCube right)
    {
      runner.Yield (closure, new RCBoolean (right.DoColof<bool> (0, left[0], true)));
    }

    [RCVerb ("colofy")]
    public void EvalColofy (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCSymbol (right.DoColof<RCSymbolScalar> (0, RCSymbolScalar.Empty, false)));
    }

    [RCVerb ("colofy")]
    public void EvalColofy (RCRunner runner, RCClosure closure, RCSymbol left, RCCube right)
    {
      runner.Yield (closure, new RCSymbol (right.DoColof<RCSymbolScalar> (0, left[0], true)));
    }

    [RCVerb ("coloft")]
    public void EvalColoft (RCRunner runner, RCClosure closure, RCCube right)
    {
      runner.Yield (closure, new RCTime (right.DoColof<RCTimeScalar> (0, RCTimeScalar.Empty, true)));
    }

    [RCVerb ("coloft")]
    public void EvalColoft (RCRunner runner, RCClosure closure, RCTime left, RCCube right)
    {
      runner.Yield (closure, new RCTime (right.DoColof<RCTimeScalar> (0, left[0], true)));
    }

    /*
    public static RCArray<T> DoColof<T> (T def, RCCube right, bool allowSparse)
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
    */
  }
}

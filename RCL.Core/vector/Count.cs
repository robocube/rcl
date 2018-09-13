using System;
using System.Text;
using RCL.Kernel;

/// <summary>
/// General purpose operations on vectors.
/// </summary>
namespace RCL.Core
{
  public class Count
  {
    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCTime right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCIncr right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCReference right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCTemplate right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }

    [RCVerb ("count")]
    public void EvalCount (RCRunner runner, RCClosure closure, RCOperator right)
    {
      runner.Yield (closure, new RCLong (right.Count));
    }
  }
}

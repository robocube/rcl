
using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Rank
  {
    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCByte right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<byte> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCByte right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<byte> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCLong right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<long> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCLong right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<long> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCDouble right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<double> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCDouble right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<double> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCDecimal right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<decimal> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCDecimal right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<decimal> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<bool> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCBoolean right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<bool> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<string> (SortDirection.asc, right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCString right)
    {
      runner.Yield (closure, new RCLong (RankUtils.DoRank<string> (Sort.ToDir (left), right)));
    }

    [RCVerb ("rank")]
    public void EvalRank (RCRunner runner, RCClosure closure, RCSymbol left, RCTime right)
    {
      runner.Yield (closure,
                    new RCLong (RankUtils.DoRank<RCTimeScalar> (Sort.ToDir (left),
                                                                right)));
    }
  }
}


namespace RCL.Kernel
{
  /// <summary>
  /// When building async operators you always need the runner and
  /// closure in order to report back later on.  Often you also
  /// need to hold onto some "other" api object as well.
  /// </summary>
  public class RCAsyncState
  {
    public readonly RCRunner Runner;
    public readonly RCClosure Closure;
    public readonly object Other;

    public RCAsyncState (RCRunner runner, RCClosure closure, object other)
    {
      Runner = runner;
      Closure = closure;
      Other = other;
    }
  }
}
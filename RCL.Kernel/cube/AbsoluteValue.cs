
namespace RCL.Kernel
{
  // I wanted rank to support the same set of operators as sort.
  // But in practice you could just take the abs of your column
  // before ranking it, which doesn't work for sorting.
  // So I'm not certain bending over backwards for absolute ranking makes sense.
  // On the other hand the work is already done and this should be a little more
  // performant;
  // no additional vector created for the abs.
  public class AbsoluteValue<T>
  {
    public virtual T Abs (T val)
    {
      return val;
    }
  }
}

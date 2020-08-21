using System;
using System.Diagnostics;

namespace RCL.Kernel
{
  public class RCAssert
  {
    [Conditional ("DEBUG")]
    public static void AxisHasG (Timeline axis, string message)
    {
      if (axis.Global == null) {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void AxisHasE (Timeline axis, string message)
    {
      if (axis.Event == null) {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void AxisHasT (Timeline axis, string message)
    {
      if (axis.Time == null) {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void AxisHasS (Timeline axis, string message)
    {
      if (axis.Symbol == null) {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void ArgumentIsNotNull (object obj, string name)
    {
      if (obj == null) {
        throw new ArgumentNullException (name);
      }
    }

    [Conditional ("DEBUG")]
    public static void IsNotNull (object obj, string message)
    {
      if (obj == null) {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void ArrayHasNoNulls<T> (RCArray<T> array)
    {
      for (int i = 0; i < array.Count; ++i)
      {
        if (array[i] == null) {
          throw new RCDebugException (
                  "The array may not contain nulls: Element {0} was null in the array {1}",
                  i,
                  array);
        }
      }
    }

    [Conditional ("DEBUG")]
    public static void ArrayHasOneElement<T> (RCArray<T> array)
    {
      if (array.Count != 1) {
        throw new RCDebugException ("The array must contain exactly one element. Array was {0}",
                                    array);
      }
    }
  }
}

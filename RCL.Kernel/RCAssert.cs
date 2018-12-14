using System;
using System.Diagnostics;

namespace RCL.Kernel
{
  public class RCAssert
  {
    [Conditional ("DEBUG")]
    public static void AxisHasG (Timeline axis, string message)
    {
      if (axis.Global == null)
      {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void AxisHasE (Timeline axis, string message)
    {
      if (axis.Event == null)
      {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void AxisHasT (Timeline axis, string message)
    {
      if (axis.Time == null)
      {
        throw new Exception (message);
      }
    }

    [Conditional ("DEBUG")]
    public static void AxisHasS (Timeline axis, string message)
    {
      if (axis.Symbol == null)
      {
        throw new Exception (message);
      }
    }
  }
}

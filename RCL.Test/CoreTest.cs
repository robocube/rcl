
using System;
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  public class CoreTest
  {
    protected RCRunner runner = RCRunner.TestRunner ();

    public static void DoTest (RCRunner runner, RCFormat args, string code, string expected)
    {
      try
      {
        DoRawTest (runner, args, code, expected);
      }
      catch (RCException ex)
      {
        Console.Out.WriteLine ("F");
        if (ex.Exception != null) {
          throw ex.Exception;
        }
        else {
          throw;
        }
      }
      catch (Exception ex)
      {
        Console.Out.WriteLine ("F");
        NUnit.Framework.Assert.Fail (ex.ToString ());
      }
    }

    public void DoTest (string code, string expected)
    {
      DoTest (runner, RCFormat.Default, code, expected);
    }

    public static void DoRawTest (RCRunner runner, RCFormat args, string code, string expected)
    {
      runner.Reset ();
      string method = new System.Diagnostics.StackFrame (3).GetMethod ().Name;
      Console.Out.Write (method + ": ");
      RCValue program = runner.Read (code);
      RCValue result = runner.Run (program);
      NUnit.Framework.Assert.IsNotNull (result, "RCRunner.Run result was null");
      string actual = result.Format (args);
      NUnit.Framework.Assert.AreEqual (expected, actual);
      Console.Out.WriteLine ("P");
    }
  }

  
}

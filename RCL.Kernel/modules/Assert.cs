
using System;
using System.Text;

namespace RCL.Kernel
{
  public class Assert
  {
    [RCVerb ("assert")]
    public void EvalAssert (RCRunner runner, RCClosure closure, RCBoolean right)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        if (!right[i])
        {
          string expression = closure.Code.ToString ();
          throw new RCException (closure,
                                 RCErrors.Assert,
                                 "Failed: " + expression);
        }
      }
      runner.Yield (closure, new RCBoolean (true));
    }

    [RCVerb ("assert")]
    public void EvalAssert (RCRunner runner, RCClosure closure, object left, object right)
    {
      if (!left.Equals (right))
      {
        string expression = closure.Code.ToString ();
        throw new RCException (closure, 
                               RCErrors.Assert, "" +
                               "Expected: " + right.ToString () +
                                  ", Actual: " + left.ToString ());
      }
      runner.Yield (closure, new RCBoolean (true));
    }


    [RCVerb ("fail")]
    public void EvalFail (RCRunner runner, RCClosure closure, RCLong left, RCString right)
    {
      runner.Finish (closure, 
                     new RCException (closure, RCErrors.Custom, right[0]), 
                     left[0]);
    }

    [RCVerb ("fail")]
    public void EvalFail (RCRunner runner, RCClosure closure, RCString right)
    {
      runner.Finish (closure, 
                     new RCException (closure, RCErrors.Custom, right[0]), 
                     (int) RCErrors.Custom);
    }

    public static void IsNull (object obj)
    {
      if (obj != null)
      {
        throw new Exception (string.Format ("A null value was expected. Actual: {0}", obj.ToString ()));
      }
    }

    public static void IsNotNull (object obj)
    {
      if (obj == null)
      {
        throw new Exception (string.Format ("A non-null value was expected."));
      }
    }

    public static void AreSame (object expected, object actual)
    {
      if (expected == null)
      {
        throw new Exception ("Expected string was null.");
      }
      else if (actual == null)
      {
        throw new Exception ("Actual string was null.");
      }
      else if (expected != actual)
      {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ("Object references do not reference the same object.");
        builder.AppendFormat ("Expected: {0}\n", expected);
        builder.AppendFormat ("Actual:   {0}", actual);
        throw new Exception (builder.ToString ());
      }
    }

    public static void AreEqual (string expected, string actual)
    {
      if (expected == null)
      {
        throw new Exception ("Expected string was null.");
      }
      else if (actual == null)
      {
        throw new Exception ("Actual string was null.");
      }
      else if (expected != actual)
      {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ("Strings are not equal.");
        builder.AppendFormat ("Expected: {0}\n", expected);
        builder.AppendFormat ("Actual:   {0}", actual);
        throw new Exception (builder.ToString ());
      }
    }

    public static void AreEqual (RCValue expected, RCValue actual)
    {
      if (expected == null)
      {
        throw new Exception ("Expected string was null.");
      }
      else if (actual == null)
      {
        throw new Exception ("Actual string was null.");
      }
      else if (!expected.Equals (actual))
      {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ("Values are not equal.");
        builder.AppendFormat ("Expected: {0}\n", expected);
        builder.AppendFormat ("Actual:   {0}", actual);
        throw new Exception (builder.ToString ());
      }
    }

    public static void AreEqual (int expected, int actual)
    {
      if (expected != actual)
      {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ("Longs are not equal.");
        builder.AppendFormat ("Expected: {0}\n", expected);
        builder.AppendFormat ("Actual:   {0}", actual);
        throw new Exception (builder.ToString ());
      }
    }

    public static void AreEqual (double expected, double actual)
    {
      if (!RCDouble.DoubleScalarEquals (expected, actual, 0.0001))
      {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ("Doubles are not equal.");
        builder.AppendFormat ("Expected: {0}\n", expected);
        builder.AppendFormat ("Actual:   {0}", actual);
        throw new Exception (builder.ToString ());
      }
    }

    public static void IsTrue (bool val)
    {
      if (!val)
      {
        throw new Exception ("Expected value was true.");
      }
    }
  }
}


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
        if (ex.Exception != null)
        {
          throw ex.Exception;
        }
        else
        {
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

  /// <summary>
  /// This test is similar in structure to RCEvalTest.
  /// But the purpose of this test is to check every single overload
  /// of every single operator, not to test the infrastructure of 
  /// evaluation proper.
  /// </summary>
  /// 
  [TestFixture]
  public class CoreTest1 : CoreTest
  {
    //Plus
    [Test]
    public void TestPlusXX() { DoTest("\\x01 + \\x02", "\\x03"); }
    [Test]
    public void TestPlusXL() { DoTest("\\x01 + 2", "3"); }
    [Test]
    public void TestPlusXD() { DoTest("\\x01 + 2.0", "3.0"); }
    [Test]
    public void TestPlusXM() { DoTest("\\x01 + 2m", "3m"); }
    [Test]
    public void TestPlusDD() { DoTest("1.0+2.0", "3.0"); }
    [Test]
    public void TestPlusDL() { DoTest("1.0+2.0", "3.0"); }
    [Test]
    //[ExpectedException(typeof(RCException))]
    public void TestPlusDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1.0+2m", "3.0"); }); }
    [Test]
    public void TestPlusDX() { DoTest("1.0 + \\x02", "3.0"); }
    [Test]
    public void TestPlusLL() { DoTest("1+2", "3"); }
    [Test]
    public void TestPlusLD() { DoTest("1+2.0", "3.0"); }
    [Test]
    public void TestPlusLM() { DoTest("1+2m", "3m"); }
    [Test]
    public void TestPlusLX() { DoTest("1 + \\x02", "3"); }
    [Test]
    public void TestPlusMM() { DoTest("1m+2m", "3m"); }
    [Test]
    public void TestPlusMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1m+2.0", "3m"); }); }
    [Test]
    public void TestPlusML() { DoTest("1m+2", "3m"); }
    [Test]
    public void TestPlusMX() { DoTest("1m + \\x02", "3m"); }
    [Test]
    public void TestPlusSS() { DoTest("\"x\"+\"y\"", "\"xy\""); }
    [Test]
    public void TestPlusYY() { DoTest("#x+#y", "#x,y"); }
    [Test]
    public void TestPlusTT() { DoTest ("2015.05.31 + 1.00:00:00.000000", "2015.06.01 00:00:00.000000"); }
    [Test]
    public void TestPlusTT1() { DoTest ("1.00:00:00.000000 + 2015.05.31", "2015.06.01 00:00:00.000000"); }
    [Test]
    public void TestPlusTT2() { DoTest ("1.00:00:00.000000 + 2.00:00:00.000000", "3.00:00:00.000000"); }
    [Test]
    public void TestPlusYY1() { DoTest("#+#y", "#y"); }
    [Test]
    public void TestPlusD0 () { DoTest ("+ ~d", "~d"); }
    [Test]
    public void TestPlusD1 () { DoTest ("+ 1.0 2.0 3.0", "1.0 3.0 6.0"); }
    [Test]
    public void TestPlusL0 () { DoTest ("+ ~l", "~l"); }
    [Test]
    public void TestPlusL1 () { DoTest ("+ 1 2 3", "1 3 6"); }
    [Test]
    public void TestPlusM0 () { DoTest ("+ ~m", "~m"); }
    [Test]
    public void TestPlusM1 () { DoTest ("+ 1 2 3m", "1 3 6m"); }
    [Test]
    public void TestPlusX0 () { DoTest ("+ ~x", "~x"); }
    [Test]
    public void TestPlusX1 () { DoTest ("+ \\x01 \\x02 \\x03", "\\x01 \\x03 \\x06"); }

    //Minus
    [Test]
    public void TestMinusXX() { DoTest("\\x03 - \\x02", "\\x01"); }
    [Test]
    public void TestMinusXXWrap() { DoTest("\\x01 - \\x02", "\\xFF"); }
    [Test]
    public void TestMinusXL() { DoTest("\\x03 - 2", "1"); }
    [Test]
    public void TestMinusXD() { DoTest("\\x03 - 2.0", "1.0"); }
    [Test]
    public void TestMinusXM() { DoTest("\\x03 - 2m", "1m"); }
    [Test]
    public void TestMinusDD() { DoTest("1.0-2.0", "-1.0"); }
    [Test]
    public void TestMinusDL() { DoTest("1.0-2", "-1.0"); }
    [Test]
    public void TestMinusDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1.0-2m", "-1.0"); }); }
    [Test]
    public void TestMinusDX() { DoTest("1.0-\\x02", "-1.0"); }
    [Test]
    public void TestMinusLL() { DoTest("1-2", "-1"); }
    [Test]
    public void TestMinusLD() { DoTest("1-2.0", "-1.0"); }
    [Test]
    public void TestMinusLM() { DoTest("1-2m", "-1m"); }
    [Test]
    public void TestMinusLX() { DoTest("1-\\x02", "-1"); }
    [Test]
    public void TestMinusMM() { DoTest("1m-2m", "-1m"); }
    [Test]
    public void TestMinusMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1m-2d", "-1m"); }); }
    [Test]
    public void TestMinusML() { DoTest("1m-2", "-1m"); }
    [Test]
    public void TestMinusMX() { DoTest("1m-\\x02", "-1m"); }
    [Test]
    public void TestMinusTT0() { DoTest ("2015.05.31 - 1.00:00:00.000000", "2015.05.30 00:00:00.000000"); }
    [Test]
    public void TestMinusTT1() { DoTest ("2015.05.31 - 2015.05.10", "21.00:00:00.000000"); }
    [Test]
    public void TestMinusTT2() { DoTest ("1.00:00:00.000000 - 2.00:00:00.000000", "-1.00:00:00.000000"); }

    [Test]
    public void TestMinusL () { DoTest ("- 1 3 5 8 12", "1 2 2 3 4"); }
    [Test]
    public void TestMinusD () { DoTest ("- 1.0 3.0 5.0 8.0 12.0", "1.0 2.0 2.0 3.0 4.0"); }
    [Test]
    public void TestMinusM () { DoTest ("- 1 3 5 8 12m", "1 2 2 3 4m"); }
    [Test]
    public void TestMinusX () { DoTest ("- \\x01 \\x03 \\x05 \\x08 \\x0c", "\\x01 \\x02 \\x02 \\x03 \\x04"); }

    //Multiply
    [Test]
    public void TestMultiplyXX() { DoTest ("\\x01*\\x02", "\\x02"); }
    [Test]
    public void TestMultiplyXL() { DoTest ("\\x01*2", "2"); }
    [Test]
    public void TestMultiplyXD() { DoTest ("\\x01*2.0", "2.0"); }
    [Test]
    public void TestMultiplyXM() { DoTest ("\\x01*2m", "2m"); }
    [Test]
    public void TestMultiplyDD() { DoTest ("1.0*2.0", "2.0"); }
    [Test]
    public void TestMultiplyDL() { DoTest ("1.0*2", "2.0"); }
    [Test]
    public void TestMultiplyDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("1.0*2m", "2.0"); }); }
    [Test]
    public void TestMultiplyDX() { DoTest ("1.0*\\x02", "2.0"); }
    [Test]
    public void TestMultiplyLL() { DoTest ("1*2", "2"); }
    [Test]
    public void TestMultiplyLD() { DoTest ("1*2.0", "2.0"); }
    [Test]
    public void TestMultiplyLM() { DoTest ("1*2m", "2m"); }
    [Test]
    public void TestMultiplyLX() { DoTest ("1*\\x02", "2"); }
    [Test]
    public void TestMultiplyMM() { DoTest ("1m*2m", "2m"); }
    [Test]
    public void TestMultiplyMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("1m*2.0", "2.0"); }); }
    [Test]
    public void TestMultiplyML() { DoTest ("1m*2", "2m"); }
    [Test]
    public void TestMultiplyMX() { DoTest ("1m*\\x02", "2m"); }

    //Divide
    [Test]
    public void TestDivideXX() { DoTest("\\x04/\\x02", "\\x02"); }
    [Test]
    public void TestDivideXL() { DoTest("\\x04/2", "2"); }
    [Test]
    public void TestDivideXD() { DoTest("\\x04/2.0", "2.0"); }
    [Test]
    public void TestDivideXM() { DoTest("\\x04/2m", "2m"); }
    [Test]
    public void TestDivideDD() { DoTest("4.0/2.0", "2.0"); }
    [Test]
    public void TestDivideDL() { DoTest("4.0/2", "2.0"); }
    [Test]
    public void TestDivideDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("4.0/2m", "2.0"); }); }
    [Test]
    public void TestDivideDX() { DoTest("4.0/\\x02", "2.0"); }
    [Test]
    public void TestDivideLL() { DoTest("4/2", "2"); }
    [Test]
    public void TestDivideLD() { DoTest("4/2.0", "2.0"); }
    [Test]
    public void TestDivideLM() { DoTest("4/2m", "2m"); }
    [Test]
    public void TestDivideLX() { DoTest("4/\\x02", "2"); }
    [Test]
    public void TestDivideMM() { DoTest("4m/2m", "2m"); }
    [Test]
    public void TestDivideMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("4m/2.0", "2.0"); }); }
    [Test]
    public void TestDivideML() { DoTest("4m/2", "2m"); }
    [Test]
    public void TestDivideMX() { DoTest("4m/\\x02", "2m"); }

    //Modulo division
    [Test]
    public void TestModulo() { DoTest ("10 % 3", "1"); }

    //Logic Operators
    [Test]
    public void TestAndXX() { DoTest("\\xF0 and \\xAA", "\\xA0"); }
    [Test]
    public void TestAndBB() { DoTest("true true false false and true false true false", "true false false false"); }
    [Test]
    public void TestOrBB() { DoTest("true true false false or true false true false", "true true true false"); }
    [Test]
    public void TestOrXX() { DoTest("\\xF0 or \\xAA", "\\xFA"); }
    [Test]
    public void TestNotBB() { DoTest("not true false", "false true"); }
    [Test]
    public void TestNotX() { DoTest("not \\xF0", "\\x0F"); }

    //Greater Than
    [Test]
    public void TestGtXX() { DoTest("\\x01>\\x02", "false"); }
    [Test]
    public void TestGtXL() { DoTest("\\x01>2", "false"); }
    [Test]
    public void TestGtXD() { DoTest("\\x01>2.0", "false"); }
    [Test]
    public void TestGtXM() { DoTest("\\x01>2m", "false"); }
    [Test]
    public void TestGtDD() { DoTest("1.0>2.0", "false"); }
    [Test]
    public void TestGtDL() { DoTest("1.0>2", "false"); }
    //.Net doesn't allow comparing decimal and double.
    //I think this is too conservative, but I will tow the line for now.
    [Test]
    public void TestGtDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1.0>2m", "false"); }); }
    [Test]
    public void TestGtDX() { DoTest("1.0>\\x02", "false"); }
    [Test]
    public void TestGtLL() { DoTest("1>2", "false"); }
    [Test]
    public void TestGtLD() { DoTest("1>2.0", "false"); }
    [Test]
    public void TestGtLM() { DoTest("1>2m", "false"); }
    [Test]
    public void TestGtLX() { DoTest("1>\\x02", "false"); }
    [Test]
    public void TestGtMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1m>2.0", "false"); }); }
    [Test]
    public void TestGtML() { DoTest("1m>2", "false"); }
    [Test]
    public void TestGtMM() { DoTest("1m>2m", "false"); }
    [Test]
    public void TestGtMX() { DoTest("1m>\\x02", "false"); }
    [Test]
    public void TestGtTT() { DoTest ("2015.05.31 > 2015.06.01", "false"); }
    [Test]
    public void TestGtTT1() { DoTest ("2015.05.31 08:00 > 2015.05.31 08:01", "false"); }
    [Test]
    public void TestGtTT2() { DoTest ("1.00:00:00.000000 > 2.00:00:00.000000", "false"); }
    //Greater Than or Equal To
    [Test]
    public void TestGteXX() { DoTest("\\x01>=\\x02", "false"); }
    [Test]
    public void TestGteXL() { DoTest("\\x01>=2", "false"); }
    [Test]
    public void TestGteXD() { DoTest("\\x01>=2.0", "false"); }
    [Test]
    public void TestGteXM() { DoTest("\\x01>=2m", "false"); }
    [Test]
    public void TestGteDD() { DoTest("1.0>=2.0", "false"); }
    [Test]
    public void TestGteDL() { DoTest("1.0>=2", "false"); }
    //.Net doesn't allow comparing decimal and double.
    //I think this is too conservative, but I will tow the line for now.
    [Test]
    public void TestGteDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1.0>=2m", "false"); }); }
    [Test]
    public void TestGteDX() { DoTest("1.0>=\\x02", "false"); }
    [Test]
    public void TestGteLL() { DoTest("1>=2", "false"); }
    [Test]
    public void TestGteLD() { DoTest("1>=2.0", "false"); }
    [Test]
    public void TestGteLM() { DoTest("1>=2m", "false"); }
    [Test]
    public void TestGteLX() { DoTest("1>=\\x02", "false"); }
    [Test]
    public void TestGteMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1m>=2.0", "false"); }); }
    [Test]
    public void TestGteML() { DoTest("1m>=2", "false"); }
    [Test]
    public void TestGteMM() { DoTest("1m>=2m", "false"); }
    [Test]
    public void TestGteMX() { DoTest("1m>=\\x02", "false"); }
    [Test]
    public void TestGteTT() { DoTest ("2015.05.31 >= 2015.06.01", "false"); }
    [Test]
    public void TestGteTT1() { DoTest ("2015.05.31 08:00 >= 2015.05.31 08:00", "true"); }
    [Test]
    public void TestGteTT2() { DoTest ("1.00:00:00.000000 >= 2.00:00:00.000000", "false"); }

    //Less Than
    [Test]
    public void TestLtXX() { DoTest("\\x01<\\x02", "true"); }
    [Test]
    public void TestLTXL() { DoTest("\\x01<2", "true"); }
    [Test]
    public void TestLtXD() { DoTest("\\x01<2.0", "true"); }
    [Test]
    public void TestLtXM() { DoTest("\\x01<2m", "true"); }
    [Test]
    public void TestLtDD() { DoTest("1.0<2.0", "true"); }
    [Test]
    public void TestLtDL() { DoTest("1.0<2", "true"); }
    [Test]
    public void TestLtDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1.0<2m", "true"); }); }
    [Test]
    public void TestLtDX() { DoTest ("1.0<\\x02", "true"); }
    [Test]
    public void TestLtLL() { DoTest("1<2", "true"); }
    [Test]
    public void TestLtLD() { DoTest("1<2.0", "true"); }
    [Test]
    public void TestLtLM() { DoTest("1<2m", "true"); }
    [Test]
    public void TestLtLX() { DoTest ("1<\\x02", "true"); }
    [Test]
    public void TestLtMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1m<2.0", "true"); }); }
    [Test]
    public void TestLtML() { DoTest("1m<2", "true"); }
    [Test]
    public void TestLtMM() { DoTest("1m<2m", "true"); }
    [Test]
    public void TestLtMX() { DoTest ("1m<\\x02", "true"); }
    [Test]
    public void TestLtTT() { DoTest ("2015.05.31 < 2015.06.01", "true"); }
    [Test]
    public void TestLtTT1() { DoTest ("2015.05.31 08:00 < 2015.05.31 08:01", "true"); }
    [Test]
    public void TestLtTT2() { DoTest ("1.00:00:00.000000 < 2.00:00:00.000000", "true"); }

    //Less Than or Equal To
    [Test]
    public void TestLteXX() { DoTest("\\x01<=\\x02", "true"); }
    [Test]
    public void TestLteXL() { DoTest("\\x01<=2", "true"); }
    [Test]
    public void TestLteXD() { DoTest("\\x01<=2.0", "true"); }
    [Test]
    public void TestLteXM() { DoTest("\\x01<=2m", "true"); }
    [Test]
    public void TestLteDD() { DoTest("1.0<=2.0", "true"); }
    [Test]
    public void TestLteDL() { DoTest("1.0<=1", "true"); }
    //.Net doesn't allow comparing decimal and double.
    //I think this is too conservative, but I will tow the line for now.
    [Test]
    public void TestLteDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1.0<=2m", "true"); }); }
    [Test]
    public void TestLteDX() { DoTest ("1.0<=\\x02", "true"); }
    [Test]
    public void TestLteLL() { DoTest("1<=1", "true"); }
    [Test]
    public void TestLteLD() { DoTest("1<=2.0", "true"); }
    [Test]
    public void TestLteLM() { DoTest("1<=1m", "true"); }
    [Test]
    public void TestLteLX() { DoTest("1<=\\x02", "true"); }
    [Test]
    public void TestLteMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest("1m<=2.0", "true"); }); }
    [Test]
    public void TestLteML() { DoTest("1m<=1", "true"); }
    [Test]
    public void TestLteMM() { DoTest("1m<=2m", "true"); }
    [Test]
    public void TestLteMX() { DoTest("1m<=\\x02", "true"); }
    [Test]
    public void TestLteTT() { DoTest ("2015.05.31 <= 2015.06.01", "true"); }
    [Test]
    public void TestLteTT1() { DoTest ("2015.05.31 08:00 <= 2015.05.31 08:00", "true"); }
    [Test]
    public void TestLteTT2() { DoTest ("1.00:00:00.000000 <= 2.00:00:00.000000", "true"); }

    //Vector Equals
    [Test]
    public void TestEqXX() { DoTest ("\\x01==\\x02", "false"); }
    [Test]
    public void TestEqXL() { DoTest ("\\x01==2", "false"); }
    [Test]
    public void TestEqXD() { DoTest ("\\x01==2.0", "false"); }
    [Test]
    public void TestEqXM() { DoTest ("\\x01==2m", "false"); }
    [Test]
    public void TestEqDD () { DoTest("1.0==2.0", "false"); }
    [Test]
    public void TestEqDD1 () { DoTest ("1.1==1.1", "true"); }
    [Test]
    public void TestEqDD2 () { DoTest ("NaN==NaN", "true"); }
    [Test]
    public void TestEqDL() { DoTest("1.0==2", "false"); }
    [Test]
    public void TestEqDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("1.0==2m", "false"); }); }
    [Test]
    public void TestEqDX() { DoTest("1.0==\\x02", "false"); }
    [Test]
    public void TestEqLL() { DoTest("1==2", "false"); }
    [Test]
    public void TestEqLD() { DoTest("1==2.0", "false"); }
    [Test]
    public void TestEqLM() { DoTest("1==2m", "false"); }
    [Test]
    public void TestEqLX() { DoTest("1==\\x02", "false"); }
    [Test]
    public void TestEqMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("1m==2.0", "false"); }); }
    [Test]
    public void TestEqML() { DoTest("1m==2", "false"); }
    [Test]
    public void TestEqMM() { DoTest("1m==2m", "false"); }
    [Test]
    public void TestEqMX() { DoTest("1m==\\x02", "false"); }
    [Test]
    public void TestEqBB() { DoTest("true==false", "false"); }
    [Test]
    public void TestEqYY() { DoTest("#x==#y", "false"); }
    [Test]
    public void TestEqYY1() { DoTest("#x==#x", "true"); }
    [Test]
    public void TestEqYY2 () { DoTest ("#==symbol \"\"", "true"); }
    [Test]
    public void TestEqSS() { DoTest ("\"x\"==\"x\"", "true"); }
    [Test]
    public void TestEqTT() { DoTest ("2018.05.04 == 2018.05.04", "true"); }
    [Test]
    public void TestEqTTWithTime() { DoTest ("2018.05.04 11:21 == 2018.05.04", "false"); }

    //Vector Not Equals
    [Test]
    public void TestNeqXX() { DoTest ("\\x01!=\\x02", "true"); }
    [Test]
    public void TestNeqXL() { DoTest ("\\x01!=2", "true"); }
    [Test]
    public void TestNeqXD() { DoTest ("\\x01!=2.0", "true"); }
    [Test]
    public void TestNeqXM() { DoTest ("\\x01!=2m", "true"); }
    [Test]
    public void TestNeqDD() { DoTest("1.0!=2.0", "true"); }
    [Test]
    public void TestNeqDL() { DoTest("1.0!=2", "true"); }
    [Test]
    public void TestNeqDM() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("1.0!=2m", "true"); }); }
    [Test]
    public void TestNeqDX() { DoTest("1.0!=\\x02", "true"); }
    [Test]
    public void TestNeqLL() { DoTest("1!=2", "true"); }
    [Test]
    public void TestNeqLD() { DoTest("1!=2.0", "true"); }
    [Test]
    public void TestNeqLM() { DoTest("1!=2m", "true"); }
    [Test]
    public void TestNeqLX() { DoTest("1!=\\x02", "true"); }
    [Test]
    public void TestNeqMD() { NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("1m!=2.0", "true"); }); }
    [Test]
    public void TestNeqML() { DoTest("1m!=2", "true"); }
    [Test]
    public void TestNeqMM() { DoTest("1m!=2m", "true"); }
    [Test]
    public void TestNeqMX() { DoTest("1m!=\\x02", "true"); }
    [Test]
    public void TestNeqBB() { DoTest("true!=false", "true"); }
    [Test]
    public void TestNeqYY() { DoTest("#x!=#y", "true"); }
    [Test]
    public void TestNeqYY1() { DoTest("#x!=#x", "false"); }
    [Test]
    public void TestNeqSS()  { DoTest ("\"x\"!=\"x\"", "false"); }

    [Test]
    public void TestEqualsYY() { DoTest("#x = #x", "true"); }
    [Test]
    public void TestEqualsYY1() { DoTest("#x #x = #x", "false"); }
    [Test]
    public void TestEqualsDD () { DoTest ("NaN 1.0 2.34 = NaN 1.0 2.34", "true"); }

    //Type coercion for vector types - we are not trying to round trip here.
    //Also not to be used for parsing and serializing object either.
    //What about coercion for blocks? {}
    [Test]
    public void TestStringX () { DoTest("string \\xF0 \\xE1 \\xD2 \\xC3", "\"F0\" \"E1\" \"D2\" \"C3\""); }
    [Test]
    public void TestStringD () { DoTest("string 1.0", "\"1\""); }
    [Test]
    public void TestStringL () { DoTest("string 1", "\"1\""); }
    [Test]
    public void TestStringM () { DoTest("string 1m", "\"1\""); }
    [Test]
    public void TestStringB () { DoTest("string true", "\"true\""); }
    [Test]
    public void TestStringS0 () { DoTest("string \"string\"", "\"string\""); }
    [Test]
    public void TestStringS1 () { DoTest("{u:[E|S|x 0 #a \"string\"] <-string $u.x}", "[E|S|x 0 #a \"string\"]"); }
    [Test]
    public void TestStringY () { DoTest("string #this,is,a,sym", "\"#this,is,a,sym\""); }
    [Test]
    public void TestStringK () { DoTest("string {x:\"a\" y:\"b\" z:\"c\"}", "\"a\" \"b\" \"c\""); }
    [Test]
    public void TestStringT0 () { DoTest ("string 2015.05.26", "\"2015.05.26\""); }
    [Test]
    public void TestStringT1 () { DoTest ("string 08:26", "\"08:26:00\""); }
    [Test]
    public void TestStringT2 () { DoTest ("string 2015.05.26 08:26", "\"2015.05.26 08:26:00\""); }
    [Test]
    public void TestStringT3 () { DoTest ("string 2015.05.26 08:26:00.123456", "\"2015.05.26 08:26:00.123456\""); }

    [Test]
    public void TestSymbolX () { DoTest ("symbol \\xFF", "#\\xFF"); }
    [Test]
    public void TestSymbolD () { DoTest ("symbol 1.0", "#1.0"); }
    [Test]
    public void TestSymbolL () { DoTest ("symbol 1", "#1"); }
    [Test]
    public void TestSymbolM () { DoTest ("symbol 1m", "#1m"); }
    [Test]
    public void TestSymbolB () { DoTest ("symbol true", "#true"); }
    [Test]
    public void TestSymbolS () { DoTest ("symbol \"string\"", "#string"); }
    [Test]
    public void TestSymbolS0 () { DoTest ("symbol \"#string\"", "#string"); }
    [Test]
    public void TestSymbolS01 () { DoTest ("(symbol \"#string\") switch {string:11}", "11"); }
    [Test]
    public void TestSymbolS1 () { DoTest ("symbol \"a\"", "#a"); }
    [Test]
    public void TestSymbolS2 () { DoTest ("symbol \" a \"", "#' a '"); }
    [Test]
    public void TestSymbolS3 () { DoTest ("symbol \"2a\"", "#'2a'"); }
    [Test]
    public void TestSymbolS4 () { DoTest ("symbol \"\"", "#"); }
    [Test]
    public void TestSymbolS5 () { DoTest ("symbol \"#a,0\"", "#a,0"); }
    [Test]
    public void TestSymbolS6 () { DoTest ("#a symbol \"\"", "#a"); }
    [Test]
    public void TestSymbolY0 () { DoTest ("symbol #this,is,a,sym", "#this,is,a,sym"); }
    [Test]
    public void TestSymbolY1 () { DoTest ("{u:[E|S|x 0 #a #symbol] <-symbol $u.x}", "[E|S|x 0 #a #symbol]"); }
    [Test]
    public void TestSymbolK () { DoTest ("symbol {x:#a y:#b z:#c}", "#a #b #c"); }

    [Test]
    public void TestLongX() { DoTest("long \\xFF", "255"); }
    [Test]
    public void TestLongD() { DoTest("long 1.0", "1"); }
    [Test]
    public void TestLongL0() { DoTest("long 1", "1"); }
    [Test]
    public void TestLongL1() { DoTest("{u:[E|S|x 0 #a 0] <-long $u.x}", "[E|S|x 0 #a 0]"); }
    [Test]
    public void TestLongM() { DoTest("long 1m", "1"); }
    [Test]
    public void TestLongB() { DoTest("long true", "1"); }
    [Test]
    public void TestLongS() { DoTest("long \"1\"", "1"); }
    [Test]
    public void TestLongS1 () { DoTest ("100 long \"\"", "100"); }
    [Test]
    public void TestLongK() { DoTest("long {x:0 y:1 z:2}", "0 1 2"); }
    [Test]
    public void TestLongY() { DoTest("long #0 #1 #2", "0 1 2"); }
    [Test]
    public void TestLongT0 () { DoTest ("long 0001.01.01", "0"); }
    [Test]
    public void TestLongT1 () { DoTest ("long 00:00", "0"); }
    [Test]
    public void TestLongT2 () { DoTest ("long 0001.01.01 00:00", "0"); }
    [Test]
    public void TestLongT3 () { DoTest ("long 0001.01.01 00:00:00.000000", "0"); }

    [Test]
    public void TestByteX() { DoTest("byte \\xFF", "\\xFF"); }
    [Test]
    public void TestByteX1() { DoTest("{u:[E|S|x 0 #a \\x00] <-byte $u.x}", "[E|S|x 0 #a \\x00]"); }
    [Test]
    public void TestByteD() { DoTest("byte 1.0", "\\x01"); }
    [Test]
    public void TestByteL0() { DoTest("byte 1", "\\x01"); }
    [Test]
    public void TestByteM() { DoTest("byte 1m", "\\x01"); }
    [Test]
    public void TestByteB() { DoTest("byte true", "\\x01"); }
    [Test]
    public void TestByteS() { DoTest("byte \"255\"", "\\xFF"); }
    [Test]
    public void TestByteK() { DoTest("byte {x:\\x00 y:\\x01 z:\\x02}", "\\x00 \\x01 \\x02"); }

    [Test]
    public void TestDoubleX() { DoTest("double \\xFF", "255.0"); }
    [Test]
    public void TestDoubleD0() { DoTest("double 1.0", "1.0"); }
    [Test]
    public void TestDoubleD1() { DoTest("{u:[E|S|x 0 #a 0.0] <-double $u.x}", "[E|S|x 0 #a 0.0]"); }
    [Test]
    public void TestDoubleL() { DoTest("double 1", "1.0"); }
    [Test]
    public void TestDoubleM() { DoTest("double 1m", "1.0"); }
    [Test]
    public void TestDoubleB() { DoTest("double true", "1.0"); }
    [Test]
    public void TestDoubleS() { DoTest("double \"1\"", "1.0"); }
    [Test]
    public void TestDoubleS1() { DoTest ("double \"foo\" \"1\" \"2.34\"", "NaN 1.0 2.34"); }
    [Test]
    public void TestDoubleS2() { DoTest ("0.0 double \"\" \"1\" \"2.34\"", "0.0 1.0 2.34"); }
    [Test]
    public void TestDoubleK() { DoTest("double {x:0.0 y:1.0 z:2.0}", "0.0 1.0 2.0"); }

    [Test]
    public void TestBooleanX() { DoTest("boolean \\x00 \\x01 \\x02", "false true true"); }
    [Test]
    public void TestBooleanD() { DoTest("boolean 0.0", "false"); }
    [Test]
    public void TestBooleanL() { DoTest("boolean 0", "false"); }
    [Test]
    public void TestBooleanM() { DoTest("boolean 0m", "false"); }
    [Test]
    public void TestBooleanB0() { DoTest("boolean false", "false"); }
    [Test]
    public void TestBooleanB1() { DoTest("{u:[E|S|x 0 #a true] <-boolean $u.x}", "[E|S|x 0 #a true]"); }
    [Test]
    public void TestBooleanS() { DoTest("boolean \"true\"", "true"); }
    [Test]
    public void TestBooleanS1() { DoTest ("false boolean \"\"", "false"); }
    [Test]
    public void TestBooleanK() { DoTest("boolean {x:true y:false z:true}", "true false true"); }

    [Test]
    public void TestDecimalX() { DoTest("decimal \\x00", "0m"); }
    [Test]
    public void TestDecimalD() { DoTest("decimal 0.0", "0m"); }
    [Test]
    public void TestDecimalL() { DoTest("decimal 0", "0m"); }
    [Test]
    public void TestDecimalM0() { DoTest("decimal 0m", "0m"); }
    [Test]
    public void TestDecimalM1() { DoTest("{u:[E|S|x 0 #a 0m] <-decimal $u.x}", "[E|S|x 0 #a 0m]"); }
    [Test]
    public void TestDecimalB() { DoTest("decimal true", "1m"); }
    [Test]
    public void TestDecimalS() { DoTest("decimal \"1\"", "1m"); }
    [Test]
    public void TestDecimalS1() { DoTest("1m decimal \"\"", "1m"); }
    [Test]
    public void TestDecimalK() { DoTest("decimal {x:0m y:1m z:2m}", "0 1 2m"); }

    [Test]
    public void TestTimeL () { DoTest ("time 0", "0001.01.01 00:00:00.000000"); }
    [Test]
    public void TestTimeT0 () { DoTest ("time 0001.01.01 00:00:00.000000", "0001.01.01 00:00:00.000000"); }
    [Test]
    public void TestTimeS () { DoTest ("time \"0001.01.01 00:00:00.000000\"", "0001.01.01 00:00:00.000000"); }
    [Test]
    public void TestTimeTS () { DoTest ("0001.01.01 00:00:00.000000 time \"\"", "0001.01.01 00:00:00.000000"); }
    [Test]
    public void TestTimeYT1 () { DoTest ("#date time 0001.01.01 00:00:00.000000", "0001.01.01"); }
    [Test]
    public void TestTimeYT2 () { DoTest ("#daytime time 0001.01.01 00:00:00.000000", "00:00:00"); }
    [Test]
    public void TestTimeYT3 () { DoTest ("#datetime time 0001.01.01 00:00:00.000000", "0001.01.01 00:00:00"); }
    [Test]
    public void TestTimeYT4 () { DoTest ("#timestamp time 0001.01.01 00:00:00.000000", "0001.01.01 00:00:00.000000"); }
    [Test]
    public void TestTimeYL0 () { DoTest ("#date time 0", "0001.01.01"); }
    [Test]
    public void TestTimeYL1 () { DoTest ("#daytime time 0", "00:00:00"); }
    [Test]
    public void TestTimeYL2 () { DoTest ("#datetime time 0", "0001.01.01 00:00:00"); }
    [Test]
    public void TestTimeYL3 () { DoTest ("#timestamp time 0", "0001.01.01 00:00:00.000000"); }

    [Test]
    public void TestDayT () { DoTest ("day 0.12:34:56.789123", "0"); }
    [Test]
    public void TestDayTU () { DoTest ("day [x 0.12:34:56.789123]", "[x 0]"); }
    [Test]
    public void TestHourT () { DoTest ("hour 0.12:34:56.789123", "12"); }
    [Test]
    public void TestHourTU () { DoTest ("hour [x 0.12:34:56.789123]", "[x 12]"); }
    [Test]
    public void TestMinuteT () { DoTest ("minute 0.12:34:56.789123", "34"); }
    [Test]
    public void TestMinuteTU () { DoTest ("minute [x 0.12:34:56.789123]", "[x 34]"); }
    [Test]
    public void TestSecondT () { DoTest ("second 0.12:34:56.789123", "56"); }
    [Test]
    public void TestSecondTU () { DoTest ("second [x 0.12:34:56.789123]", "[x 56]"); }
    [Test]
    public void TestNanoT () { DoTest ("nano 0.12:34:56.789123", "789123000"); }
    [Test]
    public void TestNanoTU () { DoTest ("nano [x 0.12:34:56.789123]", "[x 789123000]"); }

    [Test]
    public void TestDayL () { DoTest ("day 0", "0.00:00:00.000000"); }
    [Test]
    public void TestDayLU () { DoTest ("day [x 0]", "[x 0.00:00:00.000000]"); }
    [Test]
    public void TestHourL () { DoTest ("hour 12", "0.12:00:00.000000"); }
    [Test]
    public void TestHourLU () { DoTest ("hour [x 12]", "[x 0.12:00:00.000000]"); }
    [Test]
    public void TestMinuteL () { DoTest ("minute 34", "0.00:34:00.000000"); }
    [Test]
    public void TestMinuteLU () { DoTest ("minute [x 34]", "[x 0.00:34:00.000000]"); }
    [Test]
    public void TestSecondL () { DoTest ("second 56", "0.00:00:56.000000"); }
    [Test]
    public void TestSecondLU () { DoTest ("second [x 56]", "[x 0.00:00:56.000000]"); }
    [Test]
    public void TestNanoL () { DoTest ("nano 0.12:34:56.789123", "789123000"); }
    [Test]
    public void TestNanoLU () { DoTest ("nano [x 0.12:34:56.789123]", "[x 789123000]"); }

    [Test]
    public void TestDateT () { DoTest ("timestamp date 1979.09.04 12:34:56.789101", "1979.09.04 00:00:00.000000"); }
    [Test]
    public void TestDateTU () { DoTest ("timestamp date [x 1979.09.04 12:34:56.789101]", "[x 1979.09.04 00:00:00.000000]"); }
    [Test]
    public void TestDaytimeT () { DoTest ("timestamp daytime 1979.09.04 12:34:56.789101", "0001.01.01 12:34:56.000000"); }
    [Test]
    public void TestDaytimeTU () { DoTest ("timestamp daytime [x 1979.09.04 12:34:56.789101]", "[x 0001.01.01 12:34:56.000000]"); }
    [Test]
    public void TestDatetimeT () { DoTest ("timestamp datetime 1979.09.04 12:34:56.789101", "1979.09.04 12:34:56.000000"); }
    [Test]
    public void TestDatetimeTU () { DoTest ("timestamp datetime [x 1979.09.04 12:34:56.789101]", "[x 1979.09.04 12:34:56.000000]"); }
    [Test]
    public void TestTimestampT () { DoTest ("timestamp timestamp 1979.09.04 12:34:56.789101", "1979.09.04 12:34:56.789101"); }
    [Test]
    public void TestTimestampTU () { DoTest ("timestamp timestamp [x 1979.09.04 12:34:56.789101]", "[x 1979.09.04 12:34:56.789101]"); }
    [Test]
    public void TestTimespanT () { DoTest ("timespan 1979.09.04 12:34:56.789101", "722695.12:34:56.789101"); }
    [Test]
    public void TestTimespanTU () { DoTest ("timespan [x 1979.09.04 12:34:56.789101]", "[x 722695.12:34:56.789101]"); }

    [Test]
    public void TestNextDayOfWeek1 () { DoTest ("2018.10.07 nextDayOfWeek \"Friday\"", "2018.10.12"); }
    [Test]
    public void TestNextDayOfWeek2 () { DoTest ("2018.10.05 nextDayOfWeek \"Friday\"", "2018.10.05"); }
    [Test]
    public void TestNextDayOfWeek3 () { DoTest ("2018.10.04 nextDayOfWeek \"Friday\"", "2018.10.05"); }
    [Test]
    public void TestToDisplayTime () { DoTest ("{tzid:(\"Unix\" = info \"platform\") switch {:\"America/Chicago\" :\"Central Standard Time\"} :displayTimezone $tzid result:date toDisplayTime 2018.11.22 02:04:11.871303 :displayTimezone \"UTC\" <-$result}", "2018.11.21"); }
    [Test]
    public void TestReferenceS () { DoTest ("reference \"x\" \"y\" \"z\"", "$x.y.z"); }
    [Test]
    public void TestReferenceY () { DoTest ("reference #x,y,z", "$x.y.z"); }

    //Count for blocks and vectors
    [Test]
    public void TestCountK0() { DoTest("count {}", "0"); }
    [Test]
    public void TestCountK1() { DoTest("count {a:1 b:2}", "2"); }
    [Test]
    public void TestCountX() { DoTest("count \\xFF", "1"); }
    [Test]
    public void TestCountD() { DoTest("count 1.0", "1"); }
    [Test]
    public void TestCountL() { DoTest("count 1", "1"); }
    [Test]
    public void TestCountM() { DoTest("count 1m", "1"); }
    [Test]
    public void TestCountB() { DoTest("count false", "1"); }
    [Test]
    public void TestCountS() { DoTest("count \"x\"", "1"); }
    [Test]
    public void TestCountY() { DoTest("count #x", "1"); }
    [Test]
    public void TestCountT () { DoTest ("count 2015.05.26", "1"); }
    [Test]
    public void TestCountN () { DoTest ("count ++", "1"); }
    [Test]
    public void TestCountU() { DoTest ("{u:[S|x #a 0] <-count #b cube $u}", "0"); }
    [Test]
    public void TestCountR() { DoTest ("count reference \"x.y.z\"", "1"); }
    [Test]
    public void TestCountP() { DoTest ("count [? foo bar baz ?]", "2"); }
    [Test]
    public void TestCountOMonad() { DoTest ("{op::not true <-count $op}", "1"); }
    [Test]
    public void TestCountODyad() { DoTest ("{op::1 + 2 <-count $op}", "2"); }
    [Test]
    public void TestCountOChain() { DoTest ("{op::1 + 2 + 3 <-count $op}", "3"); }

    //Length for strings
    [Test]
    public void TestLengthS () { DoTest ("length \"aaa\" \"bb\" \"a\" \"\"", "3 2 1 0"); }
    [Test]
    public void TestLengthY () { DoTest ("length # #a #aaa #a,b #a,b,c", "0 1 1 2 3"); }

    //Repeat
    [Test]
    public void TestRepeatLX() { DoTest("3 repeat \\x01", "\\x01 \\x01 \\x01"); }
    [Test]
    public void TestRepeatLD() { DoTest("3 repeat 1.0", "1.0 1.0 1.0"); }
    [Test]
    public void TestRepeatLL() { DoTest("3 repeat 1", "1 1 1"); }
    [Test]
    public void TestRepeatLM() { DoTest("3 repeat 1m", "1 1 1m"); }
    [Test]
    public void TestRepeatLB() { DoTest("3 repeat false", "false false false"); }
    [Test]
    public void TestRepeatLS() { DoTest("3 repeat \"x\"", "\"x\" \"x\" \"x\""); }
    [Test]
    public void TestRepeatLY() { DoTest("3 repeat #x", "#x #x #x"); }
    [Test]
    public void TestRepeatLT() { DoTest("3 repeat 2015.05.26", "2015.05.26 2015.05.26 2015.05.26"); }
    [Test]
    public void TestRepeatLN() { DoTest("3 repeat ++", "++ ++ ++"); }
    [Test]
    public void TestRepeatMulti () { DoTest ("3 repeat #x #y #z", "#x #y #z #x #y #z #x #y #z"); }

    //[Test]
    //public void TestRepeatLY() { DoTest("3l repeat #x", "#x #x #x"); }

    //Sequential Aggregations - avg, sum, max, min, med, dev, first, last, count 
    [Test]
    public void TestSumD() { DoTest("sum 1.0 2.0 3.0", "6.0"); }
    [Test]
    public void TestSumL() { DoTest("sum 1 2 3", "6"); }
    [Test]
    public void TestSumM() { DoTest("sum 1 2 3m", "6m"); }
    [Test]
    public void TestSumX() { DoTest("sum \\x01 \\x02 \\x03", "\\x06"); }
    [Test]
    public void TestAvgD() { DoTest("avg 1.0 2.0 3.0", "2.0"); }
    [Test]
    public void TestAvgL() { DoTest("avg 1 2 3", "2.0"); }
    [Test]
    public void TestAvgM() { DoTest("avg 1 2 3m", "2m"); }
    [Test]
    public void TestAvgX() { DoTest("avg \\x01 \\x02 \\x03", "2.0"); }
    [Test]
    public void TestLowD() { DoTest("low 1.0 2.0 3.0", "1.0"); }
    [Test]
    public void TestLowL() { DoTest("low 1.0 2.0 3.0", "1.0"); }
    [Test]
    public void TestLowM() { DoTest("low 1 2 3m", "1m"); }
    [Test]
    public void TestLowX() { DoTest("low \\x01 \\x02 \\x03", "\\x01"); }
    [Test]
    public void TestHighD() { DoTest("high -1.0 -2.0 -3.0", "-1.0"); }
    [Test]
    public void TestHighL() { DoTest("high -1 -2 -3", "-1"); }
    [Test]
    public void TestHighM() { DoTest("high -1 -2 -3m", "-1m"); }
    [Test]
    public void TestHighX() { DoTest("high \\x01 \\x02 \\x03", "\\x03"); }
    [Test]
    public void TestAny1 () { DoTest ("any false true false", "true"); }
    [Test]
    public void TestAny2 () { DoTest ("any false false false", "false"); }
    [Test]
    public void TestAll1 () { DoTest ("all true true true", "true"); }
    [Test]
    public void TestAll2 () { DoTest ("all false false false", "false"); }
    [Test]
    public void TestNone1 () { DoTest ("none false false false", "true"); }
    [Test]
    public void TestNone2 () { DoTest ("none false false true", "false"); }

    [Test]
    public void TestSumKD() { DoTest("sum {x:1.0 2.0 3.0 y:4.0 5.0 6.0}", "5.0 7.0 9.0"); }
    [Test]
    public void TestSumKL() { DoTest("sum {x:1 2 3 y:4 5 6}", "5 7 9"); }
    [Test]
    public void TestSumKM() { DoTest("sum {x:1 2 3m y:4 5 6m}", "5 7 9m"); }
    [Test]
    public void TestSumKX() { DoTest("sum {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "\\x05 \\x07 \\x09"); }
    [Test]
    public void TestAvgKD() { DoTest("avg {x:1.0 2.0 3.0 y:4.0 5.0 6.0}", "2.5 3.5 4.5"); }
    [Test]
    public void TestAvgKL() { DoTest("avg {x:1 2 3 y:4 5 6}", "2.5 3.5 4.5"); }
    [Test]
    public void TestAvgKM() { DoTest("avg {x:1 2 3m y:4 5 6m}", "2.5 3.5 4.5m"); }
    [Test]
    public void TestAvgKX() { DoTest("avg {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "2.5 3.5 4.5"); }
    [Test]
    public void TestLowKD() { DoTest("low {x:1.0 2.0 3.0 y:4.0 5.0 6.0}", "1.0 2.0 3.0"); }
    [Test]
    public void TestLowKL() { DoTest("low {x:1 2 3 y:4 5 6}", "1 2 3"); }
    [Test]
    public void TestLowKM() { DoTest("low {x:1 2 3m y:4 5 6m}", "1 2 3m"); }
    [Test]
    public void TestLowKX() { DoTest("low {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "\\x01 \\x02 \\x03"); }
    [Test]
    public void TestHighKD() { DoTest("high {x:-1.0 -2.0 -3.0 y:-4.0 -5.0 -6.0}", "-1.0 -2.0 -3.0"); }
    [Test]
    public void TestHighKL() { DoTest("high {x:-1 -2 -3 y:-4 -5 -6}", "-1 -2 -3"); }
    [Test]
    public void TestHighKM() { DoTest("high {x:-1 -2 -3m y:-4 -5 -6m}", "-1 -2 -3m"); }
    [Test]
    public void TestHighKX() { DoTest("high {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "\\x04 \\x05 \\x06"); }
    [Test]
    public void TestAnyKB1 () { DoTest ("any {x:false true false y:true true false}", "true true false"); }
    [Test]
    public void TestAnyKB2 () { DoTest ("any {x:false false false y:true true true}", "true true true"); }
    [Test]
    public void TestAllKB1 () { DoTest ("all {x:true false true y:false true true}", "false false true"); }
    [Test]
    public void TestAllKB2 () { DoTest ("all {x:false false false y:true true true}", "false false false"); }
    [Test]
    public void TestNoneKB1 () { DoTest ("none {x:false false true y:false true false}", "true false false"); }
    [Test]
    public void TestNoneKB2 () { DoTest ("none {x:false false true y:false true false}", "true false false"); }

    [Test]
    public void TestMinDD() { DoTest("3.0 2.0 1.0 min 1.0 2.0 3.0", "1.0 2.0 1.0"); }
    [Test]
    public void TestMinLL() { DoTest("3 2 1 min 1 2 3", "1 2 1"); }
    [Test]
    public void TestMinMM() { DoTest("3 2 1m min 1 2 3m", "1 2 1m"); }
    [Test]
    public void TestMinXX() { DoTest("\\x03 \\x02 \\x01 min \\x01 \\x02 \\x03", "\\x01 \\x02 \\x01"); }
    [Test]
    public void TestMaxDD() { DoTest("-3.0 -2.0 -1.0 max -1.0 -2.0 -3.0", "-1.0 -2.0 -1.0"); }
    [Test]
    public void TestMaxLL() { DoTest("-3 -2 -1 max -1 -2 -3", "-1 -2 -1"); }
    [Test]
    public void TestMaxMM() { DoTest("-3 -2 -1m max -1 -2 -3m", "-1 -2 -1m"); }
    [Test]
    public void TestMaxXX() { DoTest("\\x03 \\x02 \\x01 max \\x01 \\x02 \\x03", "\\x03 \\x02 \\x03"); }
  }

  [TestFixture]
  public class CoreTest2 : CoreTest
  {
    //Symbular Aggregations - xavg, xcount, xsum, xmax, xmed, 

    //RCL read and write Operators

    //Generic Operators
    //Append
    [Test]
    public void TestAppendXX() { DoTest("\\x01 & \\x02", "\\x01 \\x02"); }
    [Test]
    public void TestAppendLL() { DoTest("1 & 2", "1 2"); }
    [Test]
    public void TestAppendDD() { DoTest("1.0 & 2.0", "1.0 2.0"); }
    [Test]
    public void TestAppendMM() { DoTest("1m & 2m", "1 2m"); }
    [Test]
    public void TestAppendBB() { DoTest("true & false", "true false"); }
    [Test]
    public void TestAppendSS() { DoTest("\"x\" & \"y\"", "\"x\" \"y\""); }
    [Test]
    public void TestAppendYY() { DoTest("#x & #y", "#x #y"); }
    [Test]
    public void TestAppendKK() { DoTest("{x:1.0} & {y:2.0}", "{x:1.0 y:2.0}"); }
    [Test]
    public void TestAppendK0 () { DoTest ("& {x:1.0 y:2.0 z:3.0}", "1.0 2.0 3.0"); }
    [Test]
    public void TestAppendK1 () { DoTest ("& {x:{a:1 b:2 c:3} y:{d:4 e:5 g:6}}", "{a:1 b:2 c:3 d:4 e:5 g:6}"); }
    [Test]
    public void TestAppendK2 () { DoTest ("& {:[x 1] :[x 2] :[x 3]}", "[x 1 2 3]"); }
    [Test]
    public void TestAppendK3 () { DoTest ("& {x:2015.05.22 y:2015.05.23 z:2015.05.24}", "2015.05.22 2015.05.23 2015.05.24"); }
    [Test]
    public void TestAppendS () { DoTest ("& \"a\" \"b\" \"c\"", "\"abc\""); }
    [Test]
    public void TestAppendT () { DoTest ("2015.05.22 & 2015.05.23", "2015.05.22 2015.05.23"); } 
    
    [Test]
    public void TestPartY0 () { DoTest ("0 part #a,b,c #d,e,f #g,h,i", "#a #d #g"); }
    [Test]
    public void TestPartY1 () { DoTest ("2 1 part #a,b,c #d,e,f #g,h,i", "#c,b #f,e #i,h"); }
    [Test]
    public void TestPartY2 () { DoTest ("0 -1 part #a,b,c #d,e,f #g,h,i", "#a,c #d,f #g,i"); }
    [Test]
    public void TestPartY3 () { DoTest ("-2 part #a,b,c #d,e,f #g,h,i", "#b #e #h"); }

    //Thru
    [Test]
    public void TestToX() { DoTest("\\x00 to \\x02", "\\x00 \\x01 \\x02"); }
    [Test]
    public void TestToL() { DoTest("0 to 2", "0 1 2"); }
    [Test]
    public void TestToD() { DoTest("0.0 to 2.0", "0.0 1.0 2.0"); }
    [Test]
    public void TestToM() { DoTest("0m to 2m", "0 1 2m"); }

    //[Test]
    //public void TestAtCube() { DoTest ("{t:[S|i a #x 0l 10l #x 1l 20l #x 2l 30l] <-$t.a at $t.i}", "10 20 30l"); }
    [Test]
    public void TestAtXX() { DoTest ("\\x00 \\x01 \\x02 at \\x01", "\\x01"); }
    [Test]
    public void TestAtXL() { DoTest ("\\x00 \\x01 \\x02 at 1", "\\x01"); }
    [Test]
    public void TestAtXD() { DoTest ("\\x00 \\x01 \\x02 at 1.0", "\\x01"); }
    [Test]
    public void TestAtXM() { DoTest ("\\x00 \\x01 \\x02 at 1m", "\\x01"); }
    [Test]
    public void TestAtLL() { DoTest ("0 1 2 at 1", "1"); }
    [Test]
    public void TestAtLD() { DoTest ("0 1 2 at 1.5", "1"); }
    [Test]
    public void TestAtLM() { DoTest ("0 1 2 at 1.5m", "1"); }
    [Test]
    public void TestAtLX() { DoTest ("0 1 2 at \\x01", "1"); }
    [Test]
    public void TestAtLNeg() { DoTest ("0 1 2 at 0 -1", "0 2"); }
    [Test]
    public void TestAtDNeg() { DoTest ("0.0 1.0 2.0 at 0.0 -1.0", "0.0 2.0"); }
    [Test]
    public void TestAtMNeg() { DoTest ("0 1 2m at 0 -1m", "0 2m"); }
    [Test]
    public void TestAtDL() { DoTest ("0.0 1.0 2.0 at 1", "1.0"); }
    [Test]
    public void TestAtDD() { DoTest ("0.0 1.0 2.0 at 1.5", "1.0"); }
    [Test]
    public void TestAtDM() { DoTest ("0.0 1.0 2.0 at 1.5m", "1.0"); }
    [Test]
    public void TestAtDX() { DoTest ("0.0 1.0 2.0 at \\x01", "1.0"); }
    [Test]
    public void TestAtML() { DoTest ("0 1 2m at 1", "1m"); }
    [Test]
    public void TestAtMD() { DoTest ("0 1 2m at 1.5", "1m"); }
    [Test]
    public void TestAtMM() { DoTest ("0 1 2m at 1.5m", "1m"); }
    [Test]
    public void TestAtMX() { DoTest ("0 1 2m at \\x01", "1m"); }
    [Test]
    public void TestAtBL() { DoTest ("true false true at 1", "false"); }
    [Test]
    public void TestAtBD() { DoTest ("true false true at 1.0", "false"); }
    [Test]
    public void TestAtBM() { DoTest ("true false true at 1m", "false"); }
    [Test]
    public void TestAtBX() { DoTest ("true false true at \\x01", "false"); }
    [Test]
    public void TestAtSL() { DoTest ("\"x\" \"y\" \"z\" at 1", "\"y\""); }
    [Test]
    public void TestAtSD() { DoTest ("\"x\" \"y\" \"z\" at 1.0", "\"y\""); }
    [Test]
    public void TestAtSM() { DoTest ("\"x\" \"y\" \"z\" at 1m", "\"y\""); }
    [Test]
    public void TestAtSX() { DoTest ("\"x\" \"y\" \"z\" at \\x01", "\"y\""); }
    [Test]
    public void TestAtYL() { DoTest ("#x #y #z at 1", "#y"); }
    [Test]
    public void TestAtYD() { DoTest ("#x #y #z at 1.0", "#y"); }
    [Test]
    public void TestAtYM() { DoTest ("#x #y #z at 1m", "#y"); }
    [Test]
    public void TestAtYX() { DoTest ("#x #y #z at \\x01", "#y"); }
    [Test]
    public void TestAtTL() { DoTest ("01:00 02:00 03:00 at 1", "02:00:00"); }
    [Test]
    public void TestAtTD() { DoTest ("01:00 02:00 03:00 at 1.0", "02:00:00"); }
    [Test]
    public void TestAtTM() { DoTest ("01:00 02:00 03:00 at 1m", "02:00:00"); }
    [Test]
    public void TestAtTX() { DoTest ("01:00 02:00 03:00 at \\x01", "02:00:00"); }
    [Test]
    public void TestAtKL() { DoTest ("{x:0 y:1 z:2} at 1", "{y:1}"); }
    [Test]
    public void TestAtKD() { DoTest ("{x:0 y:1 z:2} at 1.0", "{y:1}"); }
    [Test]
    public void TestAtKM() { DoTest ("{x:0 y:1 z:2} at 1m", "{y:1}"); }
    [Test]
    public void TestAtKLNeg() { DoTest ("{x:0 y:1 z:2} at -2", "{y:1}"); }
    [Test]
    public void TestAtKDNeg() { DoTest ("{x:0 y:1 z:2} at -2.0", "{y:1}"); }
    [Test]
    public void TestAtKMNeg() { DoTest ("{x:0 y:1 z:2} at -2m", "{y:1}"); }
    [Test]
    public void TestAtKX() { DoTest ("{x:0 y:1 z:2} at \\x01", "{y:1}"); }
    [Test]
    public void TestAtKS() { DoTest ("{x:0 y:1 z:2} at \"y\"", "{y:1}"); }
    [Test]
    public void TestAtKY() { DoTest ("{x:0 y:1 z:2} at #y", "{y:1}"); }
    [Test]
    [Ignore ("because")]
    public void TestAtUL() { DoTest ("[S|x y z #a 0 10 100] at 1", "10"); }
    [Test]
    [Ignore ("because")]
    public void TestAtUD() { DoTest ("[S|x y z #a 0 10 100] at 1.0", "10"); }
    [Test]
    [Ignore ("because")]
    public void TestAtUM() { DoTest ("[S|x y z #a 0 10 100] at 1m", "10"); }
    [Test]
    [Ignore ("because")]
    public void TestAtUX() { DoTest ("[S|x y z #a 0 10 100] at \\x01", "10"); }
    [Test]
    [Ignore ("because")]
    public void TestAtUS() { DoTest ("[S|x y z #a 0 10 100] at \"y\"", "10"); }
    [Test]
    [Ignore ("because")]
    public void TestAtUY() { DoTest ("[S|x y z #a 0 10 100] at #y", "10"); }

    //From is just at with the indices on the left side.
    //[Test]
    //public void TestFromCube() { DoTest ("{t:[S|i a #x 0 10 #x 1 20 #x 2 30] <-$t.i from $t.a}", "10 20 30"); }
    [Test]
    public void TestFromXX() { DoTest ("\\x01 from \\x00 \\x01 \\x02", "\\x01"); }
    [Test]
    public void TestFromXL() { DoTest ("\\x01 from 0 1 2", "1"); }
    [Test]
    public void TestFromXD() { DoTest ("\\x01 from 0.0 1.0 2.0", "1.0"); }
    [Test]
    public void TestFromXM() { DoTest ("\\x01 from 0 1 2m", "1m"); }
    [Test]
    public void TestFromLL() { DoTest ("1 from 0 1 2", "1"); }
    [Test]
    public void TestFromLD() { DoTest ("1 from 0.0 1.0 2.0", "1.0"); }
    [Test]
    public void TestFromLM() { DoTest ("1 from 0 1 2m", "1m"); }

    [Test]
    public void TestFromLX() { DoTest ("1 from \\x00 \\x01 \\x02", "\\x01"); }
    [Test]
    public void TestFromLNeg() { DoTest ("0 -1 from 0 1 2", "0 2"); }
    [Test]
    public void TestFromDNeg() { DoTest ("0.0 -1.0 from 0.0 1.0 2.0", "0.0 2.0"); }
    [Test]
    public void TestFromMNeg() { DoTest ("0 -1m from 0.0 1.0 2.0", "0.0 2.0"); }
    [Test]
    public void TestFromDL() { DoTest ("1.5 from 0 1 2", "1"); }
    [Test]
    public void TestFromDD() { DoTest ("1.5 from 0.0 1.0 2.0", "1.0"); }
    [Test]
    public void TestFromDM() { DoTest ("1.5 from 0 1 2m", "1m"); }
    [Test]
    public void TestFromDX() { DoTest ("1.5 from \\x00 \\x01 \\x02", "\\x01"); }
    [Test]
    public void TestFromML() { DoTest ("1.5m from 0 1 2", "1"); }
    [Test]
    public void TestFromMD() { DoTest ("1.5m from 0.0 1.0 2.0", "1.0"); }
    [Test]
    public void TestFromMM() { DoTest ("1.5m from 0 1 2m", "1m"); }
    [Test]
    public void TestFromMX() { DoTest ("1.5m from \\x00 \\x01 \\x02", "\\x01"); }
    [Test]
    public void TestFromLB() { DoTest ("1 from true false true", "false"); }
    [Test]
    public void TestFromDB() { DoTest ("1.0 from true false true", "false"); }
    [Test]
    public void TestFromMB() { DoTest ("1m from true false true", "false"); }
    [Test]
    public void TestFromXB() { DoTest ("\\x01 from true false true", "false"); }
    [Test]
    public void TestFromLS() { DoTest ("1 from \"x\" \"y\" \"z\"", "\"y\""); }
    [Test]
    public void TestFromDS() { DoTest ("1.5 from \"x\" \"y\" \"z\"", "\"y\""); }
    [Test]
    public void TestFromMS() { DoTest ("1.5m from \"x\" \"y\" \"z\"", "\"y\""); }
    [Test]
    public void TestFromXS() { DoTest ("\\x01 from \"x\" \"y\" \"z\"", "\"y\""); }
    [Test]
    public void TestFromLY() { DoTest ("1 from #x #y #z", "#y"); }
    [Test]
    public void TestFromDY() { DoTest ("1.0 from #x #y #z", "#y"); }
    [Test]
    public void TestFromMY() { DoTest ("1m from #x #y #z", "#y"); }
    [Test]
    public void TestFromXY() { DoTest ("\\x01 from #x #y #z", "#y"); }
    [Test]
    public void TestFromTL() { DoTest ("1 from 01:00 02:00 03:00", "02:00:00"); }
    [Test]
    public void TestFromTD() { DoTest ("1.0 from 01:00 02:00 03:00", "02:00:00"); }
    [Test]
    public void TestFromTM() { DoTest ("1m from 01:00 02:00 03:00", "02:00:00"); }
    [Test]
    public void TestFromTX() { DoTest ("\\x01 from 01:00 02:00 03:00", "02:00:00"); }
    [Test]
    public void TestFromLK() { DoTest ("1 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromDK() { DoTest ("1.0 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromMK() { DoTest ("1m from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromLKNeg () { DoTest ("-2 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromDKNeg () { DoTest ("-2.0 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromMKNeg () { DoTest ("-2m from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromXK () { DoTest ("\\x01 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromYK () { DoTest ("#y from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromYK1 () { DoTest ("#'a b c' from {'a b c':1}", "{'a b c':1}"); }
    [Test]
    public void TestFromYK2 () { DoTest ("#a,c #e,f from {a:{b:1 c:2 d:3} e:{f:4 g:5}}", "{c:2 f:4}"); }
    [Test]
    public void TestFromSK () { DoTest ("\"y\" from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromSK1 () { DoTest ("\"a b c\" from {'a b c':1}", "{'a b c':1}"); }
    [Test]
    public void TestFromSK2 () { DoTest ("\"'a b c'\" from {'a b c':1}", "{'a b c':1}"); }
    [Test]
    public void TestFromSKNameError ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("\"a\" \"b\" \"c\" from {a:1 b:2}", "{}"); });
    }
    [Test]
    public void TestFromYKNameError ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("#a #b #c from {a:1 b:2}", "{}"); });
    }

    [Test]
    public void TestFirstK() { DoTest ("first {:0 :1 :2}", "0"); }
    [Test]
    public void TestFirstX() { DoTest ("first \\x00 \\x01 \\x02", "\\x00"); }
    [Test]
    public void TestFirstL() { DoTest ("first 0 1 2", "0"); }
    [Test]
    public void TestFirstD() { DoTest ("first 0.0 1.0 2.0", "0.0"); }
    [Test]
    public void TestFirstM() { DoTest ("first 0 1 2m", "0m"); }
    [Test]
    public void TestFirstB() { DoTest ("first true false true", "true"); }
    [Test]
    public void TestFirstY() { DoTest ("first #a #b #c", "#a"); }
    [Test]
    public void TestFirstS() { DoTest ("first \"a\" \"b\" \"c\"", "\"a\""); }
    [Test]
    public void TestFirstI() { DoTest ("first ++ ++ ++", "++"); }
    [Test]
    public void TestFirstT() { DoTest ("first 2018.09.04 2018.09.05 2018.09.06", "2018.09.04"); }

    [Test]
    public void TestLastX() { DoTest ("last \\x00 \\x01 \\x02", "\\x02"); }
    [Test]
    public void TestLastL() { DoTest ("last 0 1 2", "2"); }
    [Test]
    public void TestLastD() { DoTest ("last 0.0 1.0 2.0", "2.0"); }
    [Test]
    public void TestLastM() { DoTest ("last 0m 1m 2m", "2m"); }
    [Test]
    public void TestLastB() { DoTest ("last true false false", "false"); }
    [Test]
    public void TestLastY() { DoTest ("last #a #b #c", "#c"); }
    [Test]
    public void TestLastS() { DoTest ("last \"a\" \"b\" \"c\"", "\"c\""); }
    [Test]
    public void TestLastI() { DoTest ("last ++ ++ +-", "+-"); }
    [Test]
    public void TestLastT() { DoTest ("last 2018.09.04 2018.09.05 2018.09.06", "2018.09.06"); }

    [Test]
    public void TestLast() { DoTest ("last {:0 :1 :2}", "2"); }
    [Test]
    public void TestUnwrap() { DoTest ("unwrap {:0}", "0"); }
    [Test]
    public void TestUnwrapEx ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate () { DoRawTest (runner, RCFormat.DefaultNoT, "unwrap {:0 :1 :2}", "{}"); });
    }

    //Oh no! What if find doesn't find anything.  How do I represent the result?
    //I have been avoiding this problem for a while... Let's think...
    //How about ~l ~d ~s, ok. That's not bad.
    //But the same problem exists with cubes, what can I do there?
    //[S|x~l y~s z~m]
    //Really not that bad, I kind of like it.  Shouldn't be too hard to implement.
    //Update: an empty cube with no observations is just []
    //so the new syntax is only required for the vector case.
    [Test]
    public void TestFindB() { DoTest ("find true false true", "0 2"); }
    [Test]
    public void TestFindBB() { DoTest ("false find true false true", "1"); }
    [Test]
    public void TestFindLL() { DoTest ("0 find 0 1 2", "0"); }
    [Test]
    public void TestFindDD() { DoTest ("0.0 find 0.0 1.0 2.0", "0"); }
    [Test]
    public void TestFindMM() { DoTest ("0m find 0 1 2m", "0"); }
    [Test]
    public void TestFindXX() { DoTest ("\\x00 find \\x00 \\x01 \\x02", "0"); }
    [Test]
    public void TestFindSS() { DoTest ("\"y\" find \"x\" \"y\" \"z\"", "1"); }
    [Test]
    public void TestFindYY() { DoTest ("#y find #x #y #z", "1"); }
    [Test]
    public void TestFindTT() { DoTest ("2015.05.27 find 2015.05.26 2015.05.27 2015.05.28", "1"); }

    [Test]
    public void TestFindBEmpty ()  { DoTest ("find false false false", "~l"); }
    [Test]
    public void TestFindBBEmpty () { DoTest ("false find true true true", "~l"); }
    [Test]
    public void TestFindLLEmpty () { DoTest ("3 find 0 1 2", "~l"); }
    [Test]
    public void TestFindDDEmpty () { DoTest ("3.0 find 0.0 1.0 2.0", "~l"); }
    [Test]
    public void TestFindMMEmpty () { DoTest ("3m find 0 1 2m", "~l"); }
    [Test]
    public void TestFindXXEmpty () { DoTest ("\\x03 find \\x00 \\x01 \\x02", "~l"); }
    [Test]
    public void TestFindSSEmpty () { DoTest ("\"a\" find \"x\" \"y\" \"z\"", "~l"); }
    [Test]
    public void TestFindYYEmpty () { DoTest ("#a find #x #y #z", "~l"); }
    [Test]
    public void TestFindTTEmpty () { DoTest ("2015.05.27 find 2015.05.26 2015.05.28 2015.05.29", "~l"); }

    [Test]
    public void TestWhereLB () { DoTest ("1 2 3 where true false true", "1 3"); }
    [Test]
    public void TestWhereDB () { DoTest ("1.0 2.0 3.0 where true false true", "1.0 3.0"); }
    [Test]
    public void TestWhereMB () { DoTest ("1.0 2.0 3.0m where true false true", "1 3m"); }
    [Test]
    public void TestWhereXB () { DoTest ("\\x01 \\x02 \\x03 where true false true", "\\x01 \\x03"); }
    [Test]
    public void TestWhereYB () { DoTest ("#x #y #z where true false true", "#x #z"); }
    [Test]
    public void TestWhereNB () { DoTest ("++ ++ ++ where true false true", "++ ++"); }
    [Test]
    public void TestWhereSB () { DoTest ("\"x\" \"y\" \"z\" where true false true", "\"x\" \"z\""); }
    [Test]
    public void TestWhereTB () { DoTest ("2015.05.27 2015.05.28 2015.05.29 where true false true", "2015.05.27 2015.05.29"); }
    [Test]
    public void TestWhereKB () { DoTest ("{a:{x:1} b:{y:1} c:{z:1}} where true false true", "{a:{x:1} c:{z:1}}"); }
    [Test]
    public void TestWhereKK () { DoTest ("{a:{x:1} b:{x:2} c:{x:3} d:{x:4}} where {a:{x:true} b:{x:true} c:{x:false} d:{x:false}}", "{a:{x:1} b:{x:2}}"); }
    [Test]
    public void TestWhereKK1 () { DoTest ("{a:{x:1 y:2 z:3}} where {a:false true false}", "{a:{y:2}}"); }
    [Test]
    public void TestWhereKK2 () { DoTest ("{k:{a:{x:1 y:2 z:3}} <-$k where {<-\"y\" == names $R} each $k}", "{a:{y:2}}"); }
    [Test]
    public void TestWhereKK3 () { DoTest ("{a:{x:1 y:2 z:3}} where {a:{x:false y:true z:false}}", "{a:{y:2}}"); }
    [Test]
    public void TestWhereKK4 () { DoTest ("{k:{a:{x:1 y:2 z:3}} <-$k where {<-{<-($L == \"y\")} each $R} each $k}", "{a:{y:2}}"); }
    [Test]
    public void TestWhereB () { DoTest ("where true true false true false true", "0 1 3 5"); }

    [Test]
    public void TestSortAscL() { DoTest ("#asc sort 2 0 1", "0 1 2"); }
    [Test]
    public void TestSortAscD() { DoTest ("#asc sort 2.0 0.0 1.0", "0.0 1.0 2.0"); }
    [Test]
    public void TestSortAscM() { DoTest ("#asc sort 2 0 1m", "0 1 2m"); }
    [Test]
    public void TestSortAscX() { DoTest ("#asc sort \\x02 \\x00 \\x01", "\\x00 \\x01 \\x02"); }
    [Test]
    public void TestSortAscB() { DoTest ("#asc sort true false true", "false true true"); }
    [Test]
    public void TestSortAscS() { DoTest ("#asc sort \"c\" \"b\" \"a\"", "\"a\" \"b\" \"c\""); }
    [Test][Ignore ("because")]
    public void TestSortAscY() { DoTest ("#asc sort #b #c #a", "#a #b #c"); }
    [Test]
    public void TestSortAscT() { DoTest ("#asc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.24 2015.05.25 2015.05.26"); }
    [Test]
    public void TestSortAscKX() { DoTest ("#asc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}", "{x:\\x00 \\x01 \\x02 y:#c #a #b}"); }
    [Test]
    public void TestSortAscKL() { DoTest ("#asc,x sort {x:1 2 0 y:#a #b #c}", "{x:0 1 2 y:#c #a #b}"); }

    [Test]
    public void TestSortAbsAscL() { DoTest ("#absasc sort 1 -2 0", "0 1 -2"); }
    [Test]
    public void TestSortAbsAscD() { DoTest ("#absasc sort 1.0 -2.0 0.0", "0.0 1.0 -2.0"); }
    [Test]
    public void TestSortAbsAscM() { DoTest ("#absasc sort 1 -2 0m", "0 1 -2m"); }
    [Test]
    public void TestSortAbsAscX() { DoTest ("#absasc sort \\x01 \\x02 \\x00", "\\x00 \\x01 \\x02"); }
    [Test]
    public void TestSortAbsAscB() { DoTest ("#absasc sort true false true", "false true true"); }
    [Test]
    public void TestSortAbsAscS() { DoTest ("#absasc sort \"c\" \"b\" \"a\"", "\"a\" \"b\" \"c\""); }
    [Test][Ignore ("because")]
    public void TestSortAbsAscY() { DoTest ("#absasc sort #b #c #a", "#a #b #c"); }
    [Test]
    public void TestSortAbsAscT() { DoTest ("#absasc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.24 2015.05.25 2015.05.26"); }
    [Test]
    public void TestSortAbsAscKX() { DoTest ("#absasc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}", "{x:\\x00 \\x01 \\x02 y:#c #a #b}"); }
    [Test]
    public void TestSortAbsAscKL() { DoTest ("#absasc,x sort {x:1 -2 0 y:#a #b #c}", "{x:0 1 -2 y:#c #a #b}"); }

    [Test]
    public void TestSortDescL() { DoTest ("#desc sort 2 0 1", "2 1 0"); }
    [Test]
    public void TestSortDescD() { DoTest ("#desc sort 2.0 0.0 1.0", "2.0 1.0 0.0"); }
    [Test]
    public void TestSortDescM() { DoTest ("#desc sort 2 0 1m", "2 1 0m"); }
    [Test]
    public void TestSortDescX() { DoTest ("#desc sort \\x02 \\x00 \\x01", "\\x02 \\x01 \\x00"); }
    [Test]
    public void TestSortDescB() { DoTest ("#desc sort true false true", "true true false"); }
    [Test]
    public void TestSortDescS() { DoTest ("#desc sort \"c\" \"b\" \"a\"", "\"c\" \"b\" \"a\""); }
    [Test] [Ignore ("because")]
    public void TestSortDescY() { DoTest ("#desc sort #b #c #a", "#c #b #a"); }
    [Test]
    public void TestSortDescT() { DoTest ("#desc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.26 2015.05.25 2015.05.24"); }
    [Test]
    public void TestSortDescKX() { DoTest ("#desc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}", "{x:\\x02 \\x01 \\x00 y:#b #a #c}"); }
    [Test]
    public void TestSortDescKL() { DoTest ("#desc,x sort {x:2 0 1 y:#a #b #c}", "{x:2 1 0 y:#a #c #b}"); }
    [Test]
    public void TestSortDescKT() { DoTest ("#desc,x sort {x:2 0 1 y:2015.05.25 2015.05.26 2015.05.27}", "{x:2 1 0 y:2015.05.25 2015.05.27 2015.05.26}"); }

    [Test]
    public void TestSortAbsDescL() { DoTest ("#absdesc sort 1 -2 0", "-2 1 0"); }
    [Test]
    public void TestSortAbsDescD() { DoTest ("#absdesc sort 1.0 -2.0 0.0", "-2.0 1.0 0.0"); }
    [Test]
    public void TestSortAbsDescM() { DoTest ("#absdesc sort 1 -2 0m", "-2 1 0m"); }
    [Test]
    public void TestSortAbsDescX() { DoTest ("#absdesc sort \\x01 \\x02 \\x00", "\\x02 \\x01 \\x00"); }
    [Test]
    public void TestSortAbsDescB() { DoTest ("#absdesc sort true false true", "true true false"); }
    [Test]
    public void TestSortAbsDescS() { DoTest ("#absdesc sort \"c\" \"b\" \"a\"", "\"c\" \"b\" \"a\""); }
    [Test][Ignore ("because")]
    public void TestSortAbsDescY() { DoTest ("#absdesc sort #b #c #a", "#a #b #c"); }
    [Test]
    public void TestSortAbsDescT() { DoTest ("#absdesc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.26 2015.05.25 2015.05.24"); }
    [Test]
    public void TestSortAbsDescKX() { DoTest ("#absdesc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}", "{x:\\x02 \\x01 \\x00 y:#b #a #c}"); }
    [Test]
    public void TestSortAbsDescKL() { DoTest ("#absdesc,x sort {x:1 -2 0 y:#a #b #c}", "{x:-2 1 0 y:#b #a #c}"); }

    [Test]
    public void TestRankAscL() { DoTest ("#asc rank 2 0 1", "1 2 0"); }
    [Test]
    public void TestRankAscD() { DoTest ("#asc rank 2.0 0.0 1.0", "1 2 0"); }
    [Test]
    public void TestRankAscM() { DoTest ("#asc rank 2 0 1m", "1 2 0"); }
    [Test]
    public void TestRankAscX() { DoTest ("#asc rank \\x02 \\x00 \\x01", "1 2 0"); }
    [Test]
    public void TestRankAscB() { DoTest ("#asc rank true false true", "1 0 2"); }
    [Test]
    public void TestRankAscS() { DoTest ("#asc rank \"c\" \"b\" \"a\"", "2 1 0"); }
    [Test][Ignore ("because")]
    public void TestRankAscY() { DoTest ("#asc rank #b #c #a", "2 0 1"); }
    [Test]
    public void TestRankAscT() { DoTest ("#asc rank 2015.05.25 2015.05.26 2015.05.24", "2 0 1"); }

    [Test]
    public void TestRankAbsAscL() { DoTest ("#absasc rank 1 -2 0", "2 0 1"); }
    [Test]
    public void TestRankAbsAscD() { DoTest ("#absasc rank 1.0 -2.0 0.0", "2 0 1"); }
    [Test]
    public void TestRankAbsAscM() { DoTest ("#absasc rank 1 -2 0m", "2 0 1"); }
    [Test]
    public void TestRankAbsAscX() { DoTest ("#absasc rank \\x01 \\x02 \\x00", "2 0 1"); }
    [Test]
    public void TestRankAbsAscB() { DoTest ("#absasc rank true false true", "1 0 2"); }
    [Test]
    public void TestRankAbsAscS() { DoTest ("#absasc rank \"c\" \"b\" \"a\"", "2 1 0"); }
    [Test][Ignore ("because")]
    public void TestRankAbsAscY() { DoTest ("#absasc rank #b #c #a", "2 0 1"); }
    [Test]
    public void TestRankAbsAscT() { DoTest ("#absasc rank 2015.05.25 2015.05.26 2015.05.24", "2 0 1"); }

    [Test]
    public void TestRankDescL() { DoTest ("#desc rank 2 0 1", "0 2 1"); }
    [Test]
    public void TestRankDescD() { DoTest ("#desc rank 2.0 0.0 1.0", "0 2 1"); }
    [Test]
    public void TestRankDescM() { DoTest ("#desc rank 2 0 1m", "0 2 1"); }
    [Test]
    public void TestRankDescX() { DoTest ("#desc rank \\x02 \\x00 \\x01", "0 2 1"); }
    [Test]
    public void TestRankDescB() { DoTest ("#desc rank true false true", "0 2 1"); }
    [Test]
    public void TestRankDescS() { DoTest ("#desc rank \"c\" \"b\" \"a\"", "0 1 2"); }
    [Test][Ignore ("because")]
    public void TestRankDescY() { DoTest ("#desc rank #b #c #a", "1 0 2"); }
    [Test]
    public void TestRankDescT() { DoTest ("#desc rank 2015.05.25 2015.05.26 2015.05.24", "1 0 2"); }

    [Test]
    public void TestRankAbsDescL() { DoTest ("#absdesc rank 1 -2 0", "1 0 2"); }
    [Test]
    public void TestRankAbsDescD() { DoTest ("#absdesc rank 1.0 -2.0 0.0", "1 0 2"); }
    [Test]
    public void TestRankAbsDescM() { DoTest ("#absdesc rank 1 -2 0m", "1 0 2"); }
    [Test]
    public void TestRankAbsDescX() { DoTest ("#absdesc rank \\x01 \\x02 \\x00", "1 0 2"); }
    [Test]
    public void TestRankAbsDescB() { DoTest ("#absdesc rank true false true", "0 2 1"); }
    [Test]
    public void TestRankAbsDescS() { DoTest ("#absdesc rank \"c\" \"b\" \"a\"", "0 1 2"); }
    [Test][Ignore ("because")]
    public void TestRankAbsDescY() { DoTest ("#absdesc rank #b #c #a", "1 0 2"); }
    [Test]
    public void TestRankAbsDescT() { DoTest ("#absdesc rank 2015.05.25 2015.05.26 2015.05.24", "1 0 2"); }
    //Identicalness - this is not like the others and needs good tests.
    
    //Math Operators - Should do a statistical test.
#if (!__MonoCS__)
    [Test]
    public void TestRandomdSeed() { DoTest("0 randomd 3", "0.72624326996796 0.817325359590969 0.768022689394663"); }
#endif    
    [Test]
    public void TestRandomD() { DoTest("3==count randomd 3", "true"); }
#if (!__MonoCS__)    
    [Test]
    public void TestRandomlSeed() { DoTest("0 randoml 3 0 10", "7 8 7"); }
#endif    
    [Test]
    public void TestRandomL() { DoTest("{v:randoml 3 0 10 <-(3 == count $v) and (0 <= low $v) and 10 >= high $v}", "true"); }
    
    //These tests only check that each overload works and that the count is correct.
    //We should have tests for the statistical distribution of the results as well.
    [Test]
    public void TestShuffleL() { DoTest ("count shuffle 0 1 2", "3" ); }
    [Test]
    public void TestShuffleLL() { DoTest ("count 0 shuffle 0 1 2", "3" ); }
    [Test]
    public void TestShuffleD() { DoTest ("count shuffle 0.0 1.0 2.0", "3" ); }
    [Test]
    public void TestShuffleLD() { DoTest ("count 0 shuffle 0.0 1.0 2.0", "3" ); }
    [Test]
    public void TestShuffleM() { DoTest ("count shuffle 0 1 2m", "3" ); }
    [Test]
    public void TestShuffleLM() { DoTest ("count 0 shuffle 0 1 2m", "3" ); }
    [Test]
    public void TestShuffleX() { DoTest ("count shuffle \\x00 \\x01 \\x02", "3" ); }
    [Test]
    public void TestShuffleLX() { DoTest ("count 0 shuffle \\x00 \\x01 \\x02", "3" ); }
    [Test]
    public void TestShuffleS() { DoTest ("count shuffle \"x\" \"y\" \"z\"", "3" ); }
    [Test]
    public void TestShuffleLS() { DoTest ("count 0 shuffle \"x\" \"y\" \"z\"", "3" ); }
    [Test]
    public void TestShuffleB() { DoTest ("count shuffle true false true", "3" ); }
    [Test]
    public void TestShuffleLB() { DoTest ("count 0 shuffle true false true", "3" ); }
    [Test]
    public void TestShuffleY() { DoTest ("count shuffle #x #y #z", "3" ); }
    [Test]
    public void TestShuffleT() { DoTest ("count shuffle 2015.05.25 2015.05.26 2015.05.27", "3" ); }
    [Test]
    public void TestShuffleLT() { DoTest ("count 0 shuffle 2015.05.25 2015.05.26 2015.05.27", "3" ); }
    [Test]
    public void TestShuffleK() { DoTest ("count shuffle {x:0 y:1 z:2}", "3" ); }
    [Test]
    public void TestShuffleLK() { DoTest ("count 0 shuffle {x:0 y:1 z:2}", "3" ); }

    [Test]
    public void TestAbsL () { DoTest ("abs -1 0 1", "1 0 1"); }
    [Test]
    public void TestAbsD () { DoTest ("abs -1.0 0.0 1.0", "1.0 0.0 1.0"); }
    [Test]
    public void TestAbsM () { DoTest ("abs -1 0 1m", "1 0 1m"); }
    //No byte overload cause there are no signed bytes.

    [Test]
    public void TestUniqueL () { DoTest ("unique 3 2 1 3 2 4", "3 2 1 4"); }
    [Test]
    public void TestUniqueD () { DoTest ("unique 3.0 2.0 1.0 3.0 2.0 4.0", "3.0 2.0 1.0 4.0"); }
    [Test]
    public void TestUniqueM () { DoTest ("unique 3 2 1 3 2 4m", "3 2 1 4m"); }
    [Test]
    public void TestUniqueX () { DoTest ("unique \\x03 \\x02 \\x01 \\x03 \\x02 \\x04", "\\x03 \\x02 \\x01 \\x04"); }
    [Test]
    public void TestUniqueS () { DoTest ("unique \"3\" \"2\" \"1\" \"4\"", "\"3\" \"2\" \"1\" \"4\""); }
    [Test]
    public void TestUniqueB () { DoTest ("unique true false true true", "true false"); }
    [Test]
    public void TestUniqueY () { DoTest ("unique #x #y #z #y", "#x #y #z"); }
    [Test]
    public void TestUniqueT () { DoTest ("unique 2015.05.25 2015.05.26 2015.05.27 2015.05.26", "2015.05.25 2015.05.26 2015.05.27"); }
    [Test]
    public void TestUniqueK () { DoTest ("unique {a:{x:1 y:2} b:{x:10 y:20} a:{x:100 y:200}}", "{a:{x:100 y:200} b:{x:10 y:20}}"); }

    [Test]
    public void TestMapL () { DoTest ("1 10 2 20 map 1 1 2 2 1 3", "10 10 20 20 10 3"); }
    [Test]
    public void TestMapD () { DoTest ("1.0 10.0 2.0 20.0 map 1.0 1.0 2.0 2.0 1.0 3.0", "10.0 10.0 20.0 20.0 10.0 3.0"); }
    [Test]
    public void TestMapM () { DoTest ("1 10 2 20m map 1 1 2 2 1 3m", "10 10 20 20 10 3m"); }
    [Test]
    public void TestMapX () { DoTest ("\\x01 \\x10 \\x02 \\x20 map \\x01 \\x01 \\x02 \\x02 \\x01 \\x03", "\\x10 \\x10 \\x20 \\x20 \\x10 \\x03"); }
    [Test]
    public void TestMapS () { DoTest ("\"1\" \"10\" \"2\" \"20\" map \"1\" \"1\" \"2\" \"2\" \"1\" \"3\"", "\"10\" \"10\" \"20\" \"20\" \"10\" \"3\""); }
    [Test]
    public void TestMapB () { DoTest ("true false false true map true true false false true", "false false true true false"); }
    [Test]
    public void TestMapY () { DoTest ("#x #y #1 #10 map #x #x #y #10 #1 #1", "#y #y #y #10 #10 #10"); }
    [Test]
    public void TestMapT () { DoTest ("2015.05.25 2015.06.25 2015.05.26 2015.06.26 map 2015.05.25 2015.05.25 2015.06.25 2015.06.26 2015.05.26 2015.05.26", "2015.06.25 2015.06.25 2015.06.25 2015.06.26 2015.06.26 2015.06.26"); }

    //Flow Control Operators
    [Test]
    public void TestSleepL() { DoTest("{t0:now{} :sleep 100 t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true"); }
    [Test]
    public void TestSleepD() { DoTest("{t0:now{} :sleep 100.0 t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true"); }
    [Test]
    public void TestSleepM() { DoTest("{t0:now{} :sleep 100m t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true"); }
    [Test]
    public void TestSleepX() { DoTest("{t0:now{} :sleep \\x64 t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true"); }

    //The defining characteristic of sweach is that it takes a bunch of code blocks on the right
    //and evaluates the correct ones based on the argument on the left.  Other than that it is much
    //like from.
    //Switch is like sweach except that you promise to only have one element on the left
    //and gives you a single value back rather than a block of results.
    [Test]
    public void TestSwitchBK0() { DoTest("false switch {:1+2 :1+3}", "4"); }
    [Test]
    public void TestSwitchBK1() { DoTest("false switch {:1+2}", "{}"); }
    [Test]
    public void TestSwitchLK0() { DoTest("-1 switch {:1+2 :1+3}", "4"); }
    [Test]
    public void TestSwitchLK1() { DoTest("0 switch {:1+2}", "3"); }

    [Test]
    public void TestSwitchLK2() { DoTest("-1 switch {:1+2}", "3"); }
    [Test]
    public void TestSwitchLK3() { DoTest("2 switch {:0 :1 :2 :3}", "2"); }
    [Test]
    public void TestSwitchXK() { DoTest("\\x00 switch {:1+2}", "3"); }
    [Test]
    public void TestSwitchYK0() { DoTest("#b switch {c:#x b:#y a:#z}", "#y"); }
    [Test]
    public void TestSwitchYK1() { DoTest("#c switch {b:#y a:#z}", "{}"); }
    //[Test]
    //public void TestSweachInTake() { DoTest("{<-#lock take {<-true sweach {:0l :1l}}}", "{:0l}"); }
    [Test]
    public void TestSwitchYKWithQuote () { DoTest ("#a switch {a::1 + 1 b::2 + 2}", "1 + 1"); }
    [Test]
    public void TestSwitchSKWithQuote () { DoTest ("\"b\" switch {a::1 + 1 b::2 + 2}", "2 + 2"); }

    //Each for blocks
    [Test]
    public void TestEachL() { DoTest ("{<-$R} each 0 to 4", "{:0 :1 :2 :3 :4}"); }
    [Test]
    public void TestEachD() { DoTest ("{<-$R} each 0.0 to 4.0", "{:0.0 :1.0 :2.0 :3.0 :4.0}"); }
    [Test]
    public void TestEachM() { DoTest ("{<-$R} each 0m to 4m", "{:0m :1m :2m :3m :4m}"); }
    [Test]
    public void TestEachX() { DoTest ("{<-$R} each \\x00 to \\x04", "{:\\x00 :\\x01 :\\x02 :\\x03 :\\x04}"); }
    [Test]
    public void TestEachS() { DoTest ("{<-$R} each string 0 to 4", "{:\"0\" :\"1\" :\"2\" :\"3\" :\"4\"}"); }
    [Test]
    public void TestEachB() { DoTest ("{<-$R} each boolean 0 to 4", "{:false :true :true :true :true}"); }
    [Test]
    public void TestEachY() { DoTest ("{<-$R} each #x #y #z", "{:#x :#y :#z}"); }
    [Test]
    public void TestEachT() { DoTest ("{<-$R} each 2015.05.27 2015.05.28 2015.05.29", "{:2015.05.27 :2015.05.28 :2015.05.29}"); }
    [Test]
    public void TestEachK() { DoTest ("{<-eval {x:$R.x-1.0}} each {:{x:1.0} :{x:2.0} :{x:3.0}}", "{:{x:0.0} :{x:1.0} :{x:2.0}}"); }
    [Test] [Ignore ("because")]
    public void TestEachKeY() { DoTest ("{<-[?<a>[!$R!]</a>?] $R} each #a #b #c #d", "\"<a>#a</a>\" \"<a>#b</a>\" \"<a>#c</a>\" \"<a>#d</a>\""); }
    [Test] [Ignore ("because")]
    public void TestEachEY() { DoTest ("[?<a>[!$R!]</a>?] each #a #b #c #d", "\"<a>#a</a>\" \"<a>#b</a>\" \"<a>#c</a>\" \"<a>#d</a>\""); }

    //[Test]
    //public void TestEachE() { DoTest ("[?<a href=\"query?symbol=%22[!$R!]%22\">#[!$R!]</a>?] each #a #b #c #d",

    [Test]
    public void TestUnionL () { DoTest ("1 2 3 union 2 3 4", "1 2 3 4"); }
    [Test]
    public void TestUnionD () { DoTest ("1.0 2.0 3.0 union 2.0 3.0 4.0", "1.0 2.0 3.0 4.0"); }
    [Test]
    public void TestUnionM () { DoTest ("1 2 3m union 2 3 4m", "1 2 3 4m"); }
    [Test]
    public void TestUnionX () { DoTest ("\\x01 \\x02 \\x03 union \\x02 \\x03 \\x04", "\\x01 \\x02 \\x03 \\x04"); }
    [Test]
    public void TestUnionB () { DoTest ("true union false", "true false"); }
    [Test]
    public void TestUnionS () { DoTest ("\"a\" \"b\" \"c\" union \"b\" \"c\" \"d\"", "\"a\" \"b\" \"c\" \"d\""); }
    [Test]
    public void TestUnionY () { DoTest ("#a #b #c union #b #c #d", "#a #b #c #d"); }
    [Test]
    public void TestUnionT () { DoTest ("2015.05.27 2015.05.28 2015.05.29 union 2015.05.28 2015.05.29 2015.05.30", "2015.05.27 2015.05.28 2015.05.29 2015.05.30"); }

    [Test]
    public void TestExceptL () { DoTest ("1 2 3 except 2 3 4", "1"); }
    [Test]
    public void TestExceptD () { DoTest ("1.0 2.0 3.0 except 2.0 3.0 4.0", "1.0"); }
    [Test]
    public void TestExceptM () { DoTest ("1 2 3m except 2 3 4m", "1m"); }
    [Test]
    public void TestExceptX () { DoTest ("\\x01 \\x02 \\x03 except \\x02 \\x03 \\x04", "\\x01"); }
    [Test]
    public void TestExceptB () { DoTest ("true except false", "true"); }
    [Test]
    public void TestExceptS () { DoTest ("\"a\" \"b\" \"c\" except \"b\" \"c\" \"d\"", "\"a\""); }
    [Test]
    public void TestExceptY () { DoTest ("#a #b #c except #b #c #d", "#a"); }
    [Test]
    public void TestExceptT () { DoTest ("2015.05.27 2015.05.28 2015.05.29 except 2015.05.28 2015.05.29", "2015.05.27"); }
    [Test]
    public void TestExceptK () { DoTest ("{a:1 b:2 c:3 d:4} except \"b\" \"c\"", "{a:1 d:4}"); }
    [Test]
    [Ignore ("Should work but I want it to do nesting")]
    public void TestExceptKY () { DoTest ("{a:1 b:2 c:3 d:4} except #b #c", "{a:1 d:4}"); }

    [Test]
    public void TestInterL () { DoTest ("1 2 3 inter 2 3 4", "2 3"); }
    [Test]
    public void TestInterD () { DoTest ("1.0 2.0 3.0 inter 2.0 3.0 4.0", "2.0 3.0"); }
    [Test]
    public void TestInterM () { DoTest ("1 2 3m inter 2 3 4m", "2 3m"); }
    [Test]
    public void TestInterX () { DoTest ("\\x01 \\x02 \\x03 inter \\x02 \\x03 \\x04", "\\x02 \\x03"); }
    [Test]
    public void TestInterB () { DoTest ("true inter false", "~b"); }
    [Test]
    public void TestInterS () { DoTest ("\"a\" \"b\" \"c\" inter \"b\" \"c\" \"d\"", "\"b\" \"c\""); }
    [Test]
    public void TestInterY () { DoTest ("#a #b #c inter #b #c #d", "#b #c"); }
    [Test]
    public void TestInterT () { DoTest ("2015.05.27 2015.05.28 2015.05.29 inter 2015.05.28 2015.05.29 2015.05.30", "2015.05.28 2015.05.29"); }
    [Test]
    public void TestInterK () { DoTest ("{a:1 b:2 c:3 d:4} inter \"b\" \"c\"", "{b:2 c:3}"); }
    [Test]
    [Ignore ("Should work but I want it to do nesting")]
    public void TestInterKY () { DoTest ("{a:1 b:2 c:3 d:4} inter #b #c", "{b:2 c:3}"); }

    [Test]
    public void TestInL () { DoTest ("0 1 2 3 4 in 1 3", "false true false true false"); }
    [Test]
    public void TestInD () { DoTest ("0.0 1.0 2.0 3.0 4.0 in 1.0 3.0", "false true false true false"); }
    [Test]
    public void TestInM () { DoTest ("0 1 2 3 4m in 1 3m", "false true false true false"); }
    [Test]
    public void TestInX () { DoTest ("\\x00 \\x01 \\x02 \\x03 \\x04 in \\x01 \\x03", "false true false true false"); }
    [Test]
    public void TestInB () { DoTest ("false true false true false in true", "false true false true false"); }
    [Test]
    public void TestInY () { DoTest ("#0 #1 #2 #3 #4 in #1 #3", "false true false true false"); }
    [Test]
    public void TestInT () { DoTest ("2015.05.26 2015.05.27 2015.05.28 2015.05.29 2015.05.30 in 2015.05.28 2015.05.30", "false false true false true"); }

    [Test]
    public void TestWithinL () { DoTest ("1 2 3 4 5 6 7 8 9 10 within 2 4 6 8", "false true true true false true true true false false"); }
    [Test]
    public void TestWithinD () { DoTest ("1.0 2.0 3.0 4.0 5.0 6.0 7.0 8.0 9.0 10.0 within 2.0 4.0 6.0 8.0", "false true true true false true true true false false"); }
    [Test]
    public void TestWithinM () { DoTest ("1 2 3 4 5 6 7 8 9 10m within 2 4 6 8m", "false true true true false true true true false false"); }
    [Test]
    public void TestWithinS () { DoTest ("\"a\" \"b\" \"c\" \"d\" \"e\" \"f\" \"g\" \"h\" \"i\" \"j\" within \"b\" \"d\" \"f\" \"h\"", "false true true true false true true true false false"); }
    [Test]
    public void TestWithinY () { DoTest ("#a #b #c #d #e #f #g #h #i #j within #b #d #f #h", "false true true true false true true true false false"); }
    [Test]
    public void TestWithinX () { DoTest ("\\x01 \\x02 \\x03 \\x04 \\x05 \\x06 \\x07 \\x08 \\x09 \\x0a within \\x02 \\x04 \\x06 \\x08", "false true true true false true true true false false"); }
    [Test]
    public void TestReverseL () { DoTest ("reverse 1 2 3", "3 2 1"); }
    [Test]
    public void TestReverseD () { DoTest ("reverse 1.0 2.0 3.0", "3.0 2.0 1.0"); }
    [Test]
    public void TestReverseM () { DoTest ("reverse 1.0 2.0 3.0m", "3 2 1m"); }
    [Test]
    public void TestReverseX () { DoTest ("reverse \\x00 \\x01 \\x02", "\\x02 \\x01 \\x00"); }
    [Test]
    public void TestReverseB () { DoTest ("reverse true false true false", "false true false true"); }
    [Test]
    public void TestReverseY () { DoTest ("reverse #a #b #c", "#c #b #a"); }
    [Test]
    public void TestReverseS () { DoTest ("reverse \"a\" \"b\" \"c\"", "\"c\" \"b\" \"a\""); }
    [Test]
    public void TestReverseT () { DoTest ("reverse 2017.08.24 2017.08.25 2017.08.26", "2017.08.26 2017.08.25 2017.08.24"); }
    [Test]
    public void TestReverseK () { DoTest ("reverse {a:1 b:2 c:3}", "{c:3 b:2 a:1}"); }

    [Test]
    public void TestPrint() { DoTest ("print \"this\" \"is\" \"some\" \"output\"", "\"this\" \"is\" \"some\" \"output\""); }
    
    [Test]
    public void TestSubX  () { DoTest ("\\x00 \\x01 \\x01 \\x02 \\x02 \\x03 sub \\x00 \\x01 \\x02", "\\x01 \\x02 \\x03"); }
    [Test]
    public void TestSubD  () { DoTest ("0.0 1.0 1.0 2.0 2.0 3.0 sub 0.0 1.0 2.0", "1.0 2.0 3.0"); }
    [Test]
    public void TestSubD0 () { DoTest ("0.0 -1.0 sub 0.0 NaN 1.0", "-1.0 NaN 1.0"); }
    [Test]
    public void TestSubD1 () { DoTest ("NaN 0 sub NaN 1.0 2.0", "0.0 1.0 2.0"); }
    [Test]
    public void TestSubL  () { DoTest ("0 1 1 2 2 3 sub 0 1 2", "1 2 3"); }
    [Test]
    public void TestSubM  () { DoTest ("0 1 1 2 2 3m sub 0 1 2m", "1 2 3m"); }
    [Test]
    public void TestSubS  () { DoTest ("\"0\" \"1\" \"1\" \"2\" \"2\" \"3\" sub \"0\" \"1\" \"2\"", "\"1\" \"2\" \"3\""); }
    [Test]
    public void TestSubY  () { DoTest ("#0 #1 #1 #2 #2 #3 sub #0 #1 #2", "#1 #2 #3"); }
    [Test]
    public void TestSubB  () { DoTest ("true false false true sub true false", "false true"); }
    [Test]
    public void TestSubT  () { DoTest ("08:00 08:10 08:10 08:20 08:20 08:30 sub 08:00 08:10 08:20", "08:10:00 08:20:00 08:30:00"); }

    [Test]
    public void TestFillL () { DoTest ("0 fill 1 0 2 0 0 3 0 0 0", "1 1 2 2 2 3 3 3 3"); }
    [Test]
    public void TestFillD () { DoTest ("0.0 fill 1.0 0.0 2.0 0.0 0.0 3.0 0.0 0.0 0.0", "1.0 1.0 2.0 2.0 2.0 3.0 3.0 3.0 3.0"); }
    [Test]
    public void TestFillM () { DoTest ("0m fill 1 0 2 0 0 3 0 0 0m", "1 1 2 2 2 3 3 3 3m"); }
    [Test]
    public void TestFillY () { DoTest ("#0 fill #1 #0 #2 #0 #0 #3 #0 #0 #0", "#1 #1 #2 #2 #2 #3 #3 #3 #3"); }
    [Test]
    public void TestFillS () { DoTest ("\"0\" fill \"1\" \"0\" \"2\" \"0\" \"0\" \"3\" \"0\" \"0\" \"0\"", "\"1\" \"1\" \"2\" \"2\" \"2\" \"3\" \"3\" \"3\" \"3\""); }
    [Test]
    public void TestFillX () { DoTest ("\\x00 fill \\x01 \\x00 \\x02 \\x00 \\x00 \\x03 \\x00 \\x00 \\x00", "\\x01 \\x01 \\x02 \\x02 \\x02 \\x03 \\x03 \\x03 \\x03"); }
    [Test]
    public void TestFillB () { DoTest ("false fill false false true false false", "false false true true true"); }
    [Test]
    public void TestFillT () { DoTest ("00:00 fill 08:00 00:00 00:00 09:00 00:00 00:00 10:00 00:00 00:00", "08:00:00 08:00:00 08:00:00 09:00:00 09:00:00 09:00:00 10:00:00 10:00:00 10:00:00"); }
    //Eval

    //Range
    [Test]
    public void TestRangeL () { DoTest ("3 range 1 2 3 4 5 6", "4 5 6"); }
    [Test]
    public void TestRangeL1 () { DoTest ("1 4 range 1 2 3 4 5 6", "2 3 4 5"); }
    [Test]
    public void TestRangeL2 () { DoTest ("0 1 3 5 range 1 2 3 4 5 6", "1 2 4 5 6"); }
    [Test]
    public void TestRangeL3 () { DoTest ("0 1 2 5 range 1 2 3 4 5 6", "1 2 3 4 5 6"); }

    [Test]
    public void TestRangeD () { DoTest ("3 range 1.0 2.0 3.0 4.0 5.0 6.0", "4.0 5.0 6.0"); }
    [Test]
    public void TestRangeD1 () { DoTest ("1 4 range 1.0 2.0 3.0 4.0 5.0 6.0", "2.0 3.0 4.0 5.0"); }
    [Test]
    public void TestRangeD2 () { DoTest ("0 1 3 5 range 1.0 2.0 3.0 4.0 5.0 6.0", "1.0 2.0 4.0 5.0 6.0"); }
    [Test]
    public void TestRangeD3 () { DoTest ("0 1 2 5 range 1.0 2.0 3.0 4.0 5.0 6.0", "1.0 2.0 3.0 4.0 5.0 6.0"); }

    [Test]
    public void TestRangeM () { DoTest ("3 range 1 2 3 4 5 6m", "4 5 6m"); }
    [Test]
    public void TestRangeM1 () { DoTest ("1 4 range 1 2 3 4 5 6m", "2 3 4 5m"); }
    [Test]
    public void TestRangeM2 () { DoTest ("0 1 3 5 range 1 2 3 4 5 6m", "1 2 4 5 6m"); }
    [Test]
    public void TestRangeM3 () { DoTest ("0 1 2 5 range 1 2 3 4 5 6m", "1 2 3 4 5 6m"); }

    [Test]
    public void TestRangeY () { DoTest ("3 range #1 #2 #3 #4 #5 #6", "#4 #5 #6"); }
    [Test]
    public void TestRangeY1 () { DoTest ("1 4 range #1 #2 #3 #4 #5 #6", "#2 #3 #4 #5"); }
    [Test]
    public void TestRangeY2 () { DoTest ("0 1 3 5 range #1 #2 #3 #4 #5 #6", "#1 #2 #4 #5 #6"); }
    [Test]
    public void TestRangeY3 () { DoTest ("0 1 2 5 range #1 #2 #3 #4 #5 #6", "#1 #2 #3 #4 #5 #6"); }

    [Test]
    public void TestRangeS () { DoTest ("3 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"4\" \"5\" \"6\""); }
    [Test]
    public void TestRangeS1 () { DoTest ("1 4 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"2\" \"3\" \"4\" \"5\""); }
    [Test]
    public void TestRangeS2 () { DoTest ("0 1 3 5 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"1\" \"2\" \"4\" \"5\" \"6\""); }
    [Test]
    public void TestRangeS3 () { DoTest ("0 1 2 5 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"1\" \"2\" \"3\" \"4\" \"5\" \"6\""); }

    [Test]
    public void TestRangeX () { DoTest ("3 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x04 \\x05 \\x06"); }
    [Test]
    public void TestRangeX1 () { DoTest ("1 4 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x02 \\x03 \\x04 \\x05"); }
    [Test]
    public void TestRangeX2 () { DoTest ("0 1 3 5 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x01 \\x02 \\x04 \\x05 \\x06"); }
    [Test]
    public void TestRangeX3 () { DoTest ("0 1 2 5 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x01 \\x02 \\x03 \\x04 \\x05 \\x06"); }

    [Test]
    public void TestRangeT () { DoTest ("3 range 00:01 00:02 00:03 00:04 00:05 00:06", "00:04:00 00:05:00 00:06:00"); }
    [Test]
    public void TestRangeT1 () { DoTest ("1 4 range 00:01 00:02 00:03 00:04 00:05 00:06", "00:02:00 00:03:00 00:04:00 00:05:00"); }
    [Test]
    public void TestRangeT2 () { DoTest ("0 1 3 5 range 00:01 00:02 00:03 00:04 00:05 00:06", "00:01:00 00:02:00 00:04:00 00:05:00 00:06:00"); }
    [Test]
    public void TestRangeT3 () { DoTest ("0 1 2 5 range 00:01 00:02 00:03 00:04 00:05 00:06", "00:01:00 00:02:00 00:03:00 00:04:00 00:05:00 00:06:00"); }

    //String/Text Operators
    [Test]
    public void TestDelimit() { DoTest ("\",\" delimit \"x\" \"y\" \"z\"", "\"x,y,z\""); }
    [Test]
    public void TestSplit () { DoTest ("\",:\" split \"x,y,z\" \"a:b:c\"", "\"x\" \"y\" \"z\" \"a\" \"b\" \"c\""); }
    [Test]
    public void TestSplitw0 () { DoTest ("\"  \" splitw \"x  y z\"", "\"x\" \"y z\""); }
    [Test]
    public void TestSplitw1 () { DoTest ("\"  \" splitw \"  x  y z\"", "\"\" \"x\" \"y z\""); }
    [Test]
    public void TestTuple () { DoTest ("\":\" tuple \"a\" \"b:c\" \"d:e:f\"", "#a #b,c #d,e,f"); }
    [Test]
    public void TestSlice0() { DoTest ("\":\" slice \"a:b\" \"c:d\" \"e:f\"", "{:\"a\" \"c\" \"e\" :\"b\" \"d\" \"f\"}"); }
    [Test]
    public void TestSlice1() { DoTest ("\",\" slice \"a\" \"b,c\" \"d,e,f\"", "{:\"a\" \"b\" \"d\" :\"\" \"c\" \"e\" :\"\" \"\" \"f\"}"); }
    [Test]
    public void TestReplace() { DoTest ("\"\\\"\" \"\" replace \"\\\"a\\\"\" \"b\"", "\"a\" \"b\""); }
    [Test]
    public void TestUpperS() { DoTest ("upper \"aAa\" \"AAA\" \"aaa\"", "\"AAA\" \"AAA\" \"AAA\""); }
    [Test]
    public void TestLowerS() { DoTest ("lower \"aAa\" \"AAA\" \"aaa\"", "\"aaa\" \"aaa\" \"aaa\""); }
    [Test]
    public void TestSubstring0 () { DoTest ("1 substring \"/foo\" \"/bar\"", "\"foo\" \"bar\""); }
    [Test]
    public void TestSubstring1 () { DoTest ("1 2 substring \"/foo\" \"/bar\"", "\"fo\" \"ba\""); }
    [Test]
    public void TestTrim () { DoTest ("trim \"a\" \" b\" \"c \" \" d \"", "\"a\" \"b\" \"c\" \"d\""); }
    [Test]
    public void TestTrim1 () { DoTest ("trim \"a\" \"\nb\" \"c\n\" \"\nd\n\"", "\"a\" \"b\" \"c\" \"d\""); }
    [Test]
    public void TestTrim2 () { DoTest ("trim \"a\" \"\r\nb\" \"c\r\n\" \"\r\nd\r\n\"", "\"a\" \"b\" \"c\" \"d\""); }
    [Test]
    public void TestTrim3 () { DoTest ("\"/\" trim \"a\" \"/b\" \"c/\" \"/d/\"", "\"a\" \"b\" \"c\" \"d\""); }
    [Test]
    public void TestTrimStart () { DoTest ("trimStart \"a\" \" b\" \"c \" \" d \"", "\"a\" \"b\" \"c \" \"d \""); }
    [Test]
    public void TestTrimEnd () { DoTest ("trimEnd \"a\" \" b\" \"c \" \" d \"", "\"a\" \" b\" \"c\" \" d\""); }
    [Test]
    public void TestTrimStartSS () { DoTest ("\"/\" trimStart \"a\" \"/b\" \"c/\" \"d/\"", "\"a\" \"b\" \"c/\" \"d/\""); }
    [Test]
    public void TestTrimEndSS () { DoTest ("\"/\" trimEnd \"a\" \"/b\" \"c/\" \"/d/\"", "\"a\" \"/b\" \"c\" \"/d\""); }
    [Test]
    public void TestIndexOf () { DoTest ("\"foo\" indexof \"abcfoodef\" \"abcdef\" \"foo\" \"fooabcdef\" \"abcdeffoo\"", "3 15 18 33"); }
    [Test]
    public void TestPad () { DoTest (" \"-\" pad 1 2 3", "\"-\" \"--\" \"---\""); }
    [Test]
    public void TestStartsWith () { DoTest ("\"aaa\" \"foobar\" \"fozbaz\" \"foo\" \"fo\" startswith \"foo\"", "false true false true false"); }
    [Test]
    public void TestStartsWith1 () { DoTest ("\"x 1\" \"y 2\" \"z 3\" \"x 4\" \"y 5\" startswith \"x\" \"y\"", "true true false true true"); }
    [Test]
    public void TestCut () { DoTest ("0 1 2 cut \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"abcdef\" \"hijkl\" \"opqr\""); }
    [Test]
    public void TestCut1 () { DoTest ("0 -1 -2 cut \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"abcdef\" \"l\" \"qr\""); }
    [Test]
    public void TestCutleft () { DoTest ("0 1 2 cutleft \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"\" \"g\" \"mn\""); }
    [Test]
    public void TestCutleft1 () { DoTest ("0 -1 -2 cutleft \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"\" \"ghijk\" \"mnop\""); }
    [Test]
    public void TestLike1 () { DoTest ("\"foobar\" \"foobaz\" \"foo\" \"fazbar\" like \"foo*\"", "true true true false"); }
    [Test]
    public void TestLike2 () { DoTest ("\"foobar\" \"fazbar\" \"bar\" \"foobaz\" like \"*bar\"", "true true true false"); }
    [Test]
    public void TestLike3 () { DoTest ("\"foozbar\" \"foolsbar\" \"foobar\" \"foobaz\" like \"foo*bar\"", "true true true false"); }
    [Test]
    public void TestLike4 () { DoTest ("\"\" like \"xyz*\"", "false"); }
    [Test]
    public void TestLike5 () { DoTest ("\"\" like \"*\"", "true"); }
    [Test]
    public void TestLike6 () { DoTest ("\"foo bar baz\" like \"bar*\"", "false"); }
    [Test]
    public void TestLike7 () { DoTest ("\"foo bar baz\" like \"bar\"", "false"); }
    [Test]
    public void TestLike8 () { DoTest ("\"foo bar baz\" like \"foo\"", "false"); }
    [Test]
    public void TestLike1U () { DoTest ("[x \"foobar\" \"foobaz\" \"foo\" \"fazbar\"] like \"foo*\"", "[x true true true false]"); }
    [Test]
    public void TestLike2U () { DoTest ("[S|x #a \"foobar\" #b \"fazbar\" #c \"bar\" #d \"foobaz\"] like \"*bar\"", "[S|x #a true #b true #c true #d false]"); }
    [Test]
    public void TestLike3U () { DoTest ("[x \"foozbar\" \"foolsbar\" \"foobar\" \"foobaz\"] like \"foo*bar\"", "[x true true true false]"); }
    [Test]
    public void TestIsName () { DoTest ("isname \"aaa\" \"a a\" \"'foo'\" \"1foo\" \"foo1\" \"foo_bar\"", "true false false false true true"); }

    [Test]
    public void TestUtf8SX () { DoTest ("utf8 utf8 \"foobarbaz\"", "\"foobarbaz\""); }
    [Test]
    public void TestAsciiSX () { DoTest ("ascii ascii \"foobarbaz\"", "\"foobarbaz\""); }

    //Block
    [Test]
    public void TestNamesK1 () { DoTest ("names {a:1 b:2 c:3}", "\"a\" \"b\" \"c\""); }
    [Test]
    public void TestNamesK2 () { DoTest ("names {abc:1 'name-with-hyphens':2 '2':3 'true':4 '1-2-3':5}",
                                         "\"abc\" \"name-with-hyphens\" \"2\" \"true\" \"1-2-3\""); }
    [Test]
    public void TestRenameS () { DoTest ("\"x\" \"y\" \"z\" rename {a:1 b:2 c:3}", "{x:1 y:2 z:3}"); }
    [Test]
    public void TestRenameY () { DoTest ("#x #y #z rename {a:1 b:2 c:3}", "{x:1 y:2 z:3}"); }
    [Test]
    public void TestRenameS1 () { DoTest ("\"x\" rename {a:1 b:2 c:3}", "{x:1 x:2 x:3}"); }
    [Test]
    public void TestRenameY1 () { DoTest ("#x rename {a:1 b:2 c:3}", "{x:1 x:2 x:3}"); }
    [Test]
    public void TestNameKSS () {
      DoTest ("\"name\" name {:{name:\"a\" value:1} :{name:\"b\" value:2} :{name:\"c\" value:3}}",
          "{a:{name:\"a\" value:1} b:{name:\"b\" value:2} c:{name:\"c\" value:3}}");
    }
    [Test]
    public void TestNameKSY () {
      DoTest ("\"name\" name {:{name:#a value:1} :{name:#b value:2} :{name:#c value:3}}",
        "{a:{name:#a value:1} b:{name:#b value:2} c:{name:#c value:3}}");
    }
    [Test]
    public void TestNameKYS () {
      DoTest ("#name name {:{name:\"a\" value:1} :{name:\"b\" value:2} :{name:\"c\" value:3}}",
          "{a:{name:\"a\" value:1} b:{name:\"b\" value:2} c:{name:\"c\" value:3}}");
    }
    [Test]
    public void TestNameKYY () {
      DoTest ("#name name {:{name:#a value:1} :{name:#b value:2} :{name:#c value:3}}",
          "{a:{name:#a value:1} b:{name:#b value:2} c:{name:#c value:3}}");
    }
    [Test]
    public void TestSetK0 () { DoTest ("{x:1 y:2} set {y:3 z:4}", "{x:1 y:3 z:4}"); }
    [Test]
    public void TestSetK1 () { DoTest ("{:~s :~s :~s} set {:\"a0\" \"a1\" :\"b0\" \"b1\" :\"c0\" \"c1\"}", "{:\"a0\" \"a1\" :\"b0\" \"b1\" :\"c0\" \"c1\"}"); }
    [Test]
    public void TestSetK2 () { DoTest ("{:~s :~s :~s} set {}", "{:~s :~s :~s}"); }
    [Test]
    public void TestGetLK () { DoTest ("1 get {:#x :#y :#z}", "#y"); }
    [Test]
    public void TestGetKL () { DoTest ("{:#x :#y :#z} get 1", "#y"); }
    [Test]
    public void TestGetSK () { DoTest ("\"b\" get {a:#x b:#y c:#z}", "#y"); }
    [Test]
    public void TestGetKS () { DoTest ("{a:#x b:#y c:#z} get \"b\"", "#y"); }
    [Test]
    public void TestGetYK () { DoTest ("#b get {a:#x b:#y c:#z}", "#y"); }
    [Test]
    public void TestGetKY () { DoTest ("{a:#x b:#y c:#z} get #b", "#y"); }
    [Test]
    public void TestGetYK1 () { DoTest ("#1 get {a:#x b:#y c:#z}", "#y"); }
    [Test]
    public void TestGetKY1 () { DoTest ("{a:#x b:#y c:#z} get #1", "#y"); }

    [Test]
    public void TestMergeK0 () { DoTest ("{a:3 b:4} merge {a:2}", "{a:2 b:4}"); }
    [Test]
    public void TestMergeK1 () { DoTest ("{x:1 y:2 z:{a:3 b:4}} merge {z:{a:2}}", "{x:1 y:2 z:{a:2 b:4}}"); }
    [Test]
    public void TestMergeK2 () { DoTest ("{x:1 y:2 z:{a:3 b:4}} merge {z:{}}", "{x:1 y:2 z:{a:3 b:4}}"); }
    [Test]
    public void TestMergeK3 () { DoTest ("{x:1 y:{a:3 b:4} z:2} merge {y:{b:5}}", "{x:1 y:{a:3 b:5} z:2}"); }
    [Test]
    public void TestMergeK4 () { DoTest ("{x:1 y:{a:3 b:5 + 1} z:2} merge {y:{b:{<-5 + 1}}}", "{x:1 y:{a:3 b:{<-5 + 1}} z:2}"); }
    [Test]
    public void TestMergeK5 () { DoTest ("{x:1 y:{a:3 b:{<-5 + 1}} z:2} merge {y:{b:5 + 1}}", "{x:1 y:{a:3 b:5 + 1} z:2}"); }

    //Still need to think about how set should work in the case of a timeline.
    [Test]
    public void TestHasY () { DoTest ("{x:1 y:2 z:3} has #a #y #z", "false true true"); }
    [Test]
    public void TestHasS () { DoTest ("{x:1 y:2 z:3} has \"a\" \"y\" \"z\"", "false true true"); }
    [Test]
    [Ignore ("reason")]
    public void TestHasL () { DoTest ("{x:1 y:2 z:3} has 0 1 4", "true true false"); }
    [Test]
    public void TestHasUS () { DoTest ("[S|x y #a 1 10 #b 2 20] has \"S\" \"x\" \"z\"", "true true false"); }
    [Test]
    public void TestHasUY () { DoTest ("[S|x y #a 1 10 #b 2 20] has #S #x #z", "true true false"); }

    [Test]
    public void TestUnflipK () { DoTest ("unflip {x:1 2 3 4 y:5 6 7 8 z:9 10 11 12}", "{:{x:1 y:5 z:9} :{x:2 y:6 z:10} :{x:3 y:7 z:11} :{x:4 y:8 z:12}}"); }
    [Test]
    public void TestFlipK0 () { DoTest ("flip {:{x:1 y:4 z:7} :{x:2 y:5 z:8} :{x:3 y:6 z:9}}", "{x:1 2 3 y:4 5 6 z:7 8 9}"); }
    [Test]
    public void TestFlipK1 () { DoTest ("flip {:{x:1 y:5 z:9} :{x:2 y:6 z:10} :{x:3 y:7 z:11} :{x:4 y:8 z:12}}", "{x:1 2 3 4 y:5 6 7 8 z:9 10 11 12}"); }

    [Test]
    public void TestTypecodeL () { DoTest ("typecode 1 2 3", "\"l\""); }
    [Test]
    public void TestTypecodeD () { DoTest ("typecode 1.0 2.0 3.0", "\"d\""); }
    [Test]
    public void TestTypecodeM () { DoTest ("typecode 1.0 2.0 3.0m", "\"m\""); }
    [Test]
    public void TestTypecodeY () { DoTest ("typecode #a #b #c", "\"y\""); }
    [Test]
    public void TestTypecodeB () { DoTest ("typecode true false true", "\"b\""); }
    [Test]
    public void TestTypecodeX () { DoTest ("typecode \\x00 \\x01 \\x02", "\"x\""); }
    [Test]
    public void TestTypecodeS () { DoTest ("typecode \"a\" \"b\" \"c\"", "\"s\""); }
    [Test]
    public void TestTypecodeT () { DoTest ("typecode 2016.05.16 2016.05.17 2016.05.18", "\"t\""); }
    [Test]
    public void TestTypecodeU () { DoTest ("typecode [x 0 1 2]", "\"u\""); }
    [Test]
    public void TestTypecodeK () { DoTest ("typecode {a:1 b:2 c:3}", "\"k\""); }

    [Test]
    public void TestTypenameL () { DoTest ("typename 1 2 3", "\"long\""); }
    [Test]
    public void TestTypenameD () { DoTest ("typename 1.0 2.0 3.0", "\"double\""); }
    [Test]
    public void TestTypenameM () { DoTest ("typename 1.0 2.0 3.0m", "\"decimal\""); }
    [Test]
    public void TestTypenameY () { DoTest ("typename #a #b #c", "\"symbol\""); }
    [Test]
    public void TestTypenameB () { DoTest ("typename true false true", "\"boolean\""); }
    [Test]
    public void TestTypenameX () { DoTest ("typename \\x00 \\x01 \\x02", "\"byte\""); }
    [Test]
    public void TestTypenameS () { DoTest ("typename \"a\" \"b\" \"c\"", "\"string\""); }
    [Test]
    public void TestTypenameT () { DoTest ("typename 2016.05.16 2016.05.17 2016.05.18", "\"time\""); }
    [Test]
    public void TestTypenameU () { DoTest ("typename [x 0 1 2]", "\"cube\""); }
    [Test]
    public void TestTypenameK () { DoTest ("typename {a:1 b:2 c:3}", "\"block\""); }
  }

  [TestFixture]
  public class CoreTest3 : CoreTest
  {
#if !__MonoCS__
    [SetUp]
    public void Setup ()
    {
      // This is to make tests involving file access work on Windows (under Parallels)
      // It would be better to do this by controlling the working directory from the .runsettings
      // file but I have yet to see any evidence that the runsettings file is honored at all by the Test Explorer.
      Environment.SetEnvironmentVariable ("RCL_HOME", "Y:\\dev");
      // This operator alters the runtime environment, not just the current runner state.
      runner.Run (RCSystem.Parse ("cd #home,src,rcl,RCL.Test,bin,Debug"));
    }
#endif

    //Messaging/Stream Operators
    [Test]
    [Ignore ("because")]
    public void TestOpenSendReceiveCloseCube ()
    {
      DoTest ("{handle:open #cube,'..','..',data,test :$handle receive $handle send {verb:#write symbol:#a data:{a:1 b:2.0 c:3m d:\"x\" e:#x f:true}} <-$handle receive $handle send {verb:#read symbol:#a rows:0}}", "[S|a b c d e f #a 1 2.0 3m \"x\" #x true]");
    }

    //Persistence Operators
    [Test]
    public void TestSaveLoadDelete ()
    {
      DoTest ("{x:parse load \"file\" save format {a:1.0 b:2.0 c:3.0} :delete \"file\" <-$x}", "{a:1.0 b:2.0 c:3.0}");
    }

    [Test]
    public void TestSaveLoadExtraLine ()
    {
      DoTest ("{:\"file\" save \"line0\" \"line1\" \"line2\" text:load \"file\" :delete \"file\" <-$text}", "\"line0\\nline1\\nline2\"");
    }

    [Test]
    public void TestSaveLoadUnderline ()
    {
      DoTest ("{:\"file\" save \"line0\" \"line1\" \"line2\" :\"file\" save \"line0\" \"line1\" text:load \"file\" :delete \"file\" <-$text}", "\"line0\\nline1\"");
    }

    [Test]
    public void TestSavebinLoadbinDelete ()
    {
      DoTest ("{x:parse loadbin \"file\" savebin binary {a:1.0 b:2.0 c:3.0} :delete \"file\" <-$x}", "{a:1.0 b:2.0 c:3.0}");
    }

    [Test]
    public void TestPath ()
    {
      DoTest ("(path #home,env,env.rclb) like \"*/env/env.rclb\"", "true");
    }

    [Test]
    public void TestFlagDefault ()
    {
      DoTest ("false flag \"not-a-flag\"", "false");
    }

    [Test]
    public void TestFile ()
    {
      DoTest ("{before:file \"file\" :\"file\" save #pretty format {a:1 b:2 c:3} after:file \"file\" :delete \"file\" <-$before & $after}", "false true");
    }

    [Test]
    public void TestFileAndPath ()
    {
      DoTest ("{before:file #work,file :#work,file save #pretty format {a:1 b:2 c:3} after:file #work,file :delete #work,file <-$before & $after}", "false true"); 
    }

    [Test]
    public void TestTryFail ()
    {
      DoTest ("#status #data from try {<-900 fail \"fail with status 900\"}", "{status:900 data:[?\n    <<Custom,fail with status 900>>\n  ?]}");
    }

    [Test]
    public void TestTryFail1 ()
    {
      DoTest ("#status #data from try {<-fail \"rando failure message\"}", "{status:8 data:[?\n    <<Custom,rando failure message>>\n  ?]}");
    }

    [Test]
    [Ignore ("This test fails to expose the bugs in watchf all the time. Need a better test then implement buffer fixes.")]
    public void TestWatchf ()
    {
      DoTest ("{h:exec \"mkdir test\" :cd \"test\" fsw:watchf pwd {} :exec \"touch foo\" create:waitf $fsw :\"foo\" save \"bar\" update:waitf $fsw :exec \"rm foo\" delete:waitf $fsw :cd \"..\" :exec \"rmdir test\" <-eval {create:#event #name from last $create update:#event #name from last $update delete:#event #name from last $delete}}", "{create:{event:\"created\" name:\"foo\"} update:{event:\"changed\" name:\"foo\"} delete:{event:\"deleted\" name:\"foo\"}}");
    }

    [Test]
    public void TestTryFailAssert ()
    {
      DoTest ("#status from try {<-#x assert #y}", "{status:1}");
    }

    [Test]
    public void TestTryOk ()
    {
      DoTest ("#status #data from try {<-#x assert #x}", "{status:0 data:true}");
    }

    [Test]
    public void TestTryEval ()
    {
      DoTest ("try {<-eval {a:1}}", "{status:0 data:{a:1}}");
    }

    [Test]
    public void TestTryEval1 ()
    {
      DoTest ("try try {<-eval {a:1}}", "{status:0 data:{status:0 data:{a:1}}}");
    }

    [Test]
    public void TestTryEval2 ()
    {
      DoTest ("try try try {<-eval {a:1}}", "{status:0 data:{status:0 data:{status:0 data:{a:1}}}}");
    }

    [Test]
    public void TestTrySwitchLast ()
    {
      DoTest ("#status from try {<-true switch {:load \"/not/a/file\"}}", "{status:1}");
    }

    [Test]
    public void TestTryBadReference ()
    {
      DoTest ("#status from try {<-$foo}", "{status:1}");
    }

    [Test]
    public void TestTryAsync0 ()
    {
      DoTest ("{b:bot {<-try {<-#x assert #y}} <-0 < $b}", "true");
    }

    [Test]
    public void TestTryAsync1 ()
    {
      //The bot operator would continue from the exception and print twice.
      //This is not a good test because we can't check to see whether it printed once or twice.
      DoTest ("{b:bot {<-try {<-#x assert #y}} :print \"async try\" <-#status from wait $b}", "{status:1}");
    }

    [Test]
    public void TestTryAsync2 ()
    {
      DoTest ("{b:bot {<-try {<-#x assert #x}} <-#status from wait $b}", "{status:0}");
    }

    [Test]
    public void TestTryAsync3 ()
    {
      DoTest ("{b:bot {<-try {<-#foo read 0}} :kill $b <-#status from wait $b}", "{status:2}");
    }

    [Test]
    public void TestTryAsync4 ()
    {
      DoTest ("{b:bot {<-try {<-#foo read 0}} :kill $b f:fiber {<-#status from wait $b} <-wait $f}", "{status:2}");
    }

    [Test]
    public void TestTryAssert ()
    {
      DoTest ("#status #data from try {<-assert false}", "{status:1 data:[?\n    <<Assert,Failed: assert false>>\n  ?]}");
    }

    [Test]
    public void TestBotWithChildFibers ()
    {
      //Would not be able to find f in the child fiber.
      //I should be able to wait on $b, and not have a sleep here.
      //Then the result of the test should be 11.
      //DoTest ("{b:bot {add_one:{<-$R + 1} f:fiber {<-add_one 10} :wait $f <-0} <-wait $b}", "11");
      DoTest ("{b:bot {add_one:{<-$R + 1} f:fiber {<-add_one 10} <-wait $f} <-wait $b}", "11");
    }

    [Test]
    public void TestFiberWithChildFibers ()
    {
      //Would not be able to find f in the child fiber.
      //I should be able to wait on $b, and not have a sleep here.
      //Then the result of the test should be 11.
      DoTest ("{p:fiber {<-eval {add_one:{<-$R + 1} c:fiber {<-add_one 10} <-wait $c}} <-wait $p}", "11");
      //DoTest ("{p:fiber {<-eval {add_one:{<-$R + 1} :fiber {<-add_one 10} <-wait 0}} :sleep 1000 <-$p}", "1");
      //DoTest ("{add_ten:{<-$R + 10} :fiber {<-eval {add_one:{<-$R + 1} :fiber {<-add_one 10} <-wait 0}} :fiber {<-add_ten 10} :sleep 1000l <-0}", "0");
      //DoTest ("{serve_page:{request:httprecv $R header:httpheader $request args:eval {file:(1l substring $header.RawUrl) + \".o2\"} body:httpbody $request :bot {<-eval parse load $args.file} :$request httpsend \"<pre>\" + (#pretty format eval {header:$header body:$body args:$args}) + \"</pre>\" <-serve_page $R} :fiber {<-eval {add_one:{<-$R + 1} :fiber {<-add_one 10} <-wait 0}} :fiber {<-serve_page httpstart \"http://*:8080/\"} <-wait 0}", "0");
    }

    [Test]
    public void TestTryWaitKill ()
    {
      //Note use of sleep 0l to expose the m_queue bug.
      DoTest ("{b:bot {<-try {w:{:#x write {i:++} <-w $R} :fiber {<-w 0} <-wait 0}} :sleep 0 :fiber {:#y dispatch 1 <-kill $b} :#y write {i:++} r:wait $b <-#status from $r}", "{status:2}");
    }

    [Test]
    public void TestTryWaitKill1 ()
    {
      //In this case the 0 fiber for the bot will return immediately resulting in a zero status and an actual result for the zero fiber of the child bot.
      DoTest ("{b:bot {<-try {w:{:#x write {i:++} <-w $R} :fiber {<-w 0} <-123}} :sleep 0 :fiber {:#y dispatch 1 <-kill $b} :#y write {i:++} <-wait $b}", "{status:0 data:123}");
    }

    [Test]
    public void TestTryWaitKill2 ()
    {
      //This is like the test above but with more fibers on the child bot.
      //Some of the kill bugs do not reproduce unless there are more than two child fibers.
      DoTest ("{b:bot {<-try {w:{:#x write {i:++} <-w $R} :fiber {<-w 0} :fiber {<-w 0} :fiber {<-w 0} <-123}} :wait $b & 0 :kill $b <-wait $b}", "{status:0 data:123}");
    }

    [Test]
    public void TestTryWaitKill3 ()
    {
      DoTest ("{p:{serve:{b:bot {<-try {<-eval {<-2 + 3}}} f1:fiber {br:wait $b <-$br} <-wait $f1} r:wait fiber {<-serve #}} <-first #r from eval $p}", "{status:0 data:5}");
    }

    [Test]
    public void TestTryWaitKill4 ()
    {
      //This one exposed a bug in the bot number on the trailing closure.
      DoTest ("{p:{b:bot {<-try {<-eval {:12 :13}}} f:fiber {br:wait $b <-$br} r:wait $f} <-first #r from eval $p}", "{status:0 data:{:12 :13}}");
    }

    [Test]
    public void TestTryWaitKill5 ()
    {
      //Kill was yanking queued items that it should not.
      //We kill fiber f and then check to see that the others are still writing.
      DoTest ("{w:{:#x write {i:++} <-w $R} f0:fiber {<-w 0} f1:fiber {<-w 0} f2:fiber {<-w 0} :kill $f0 :#x dispatch 0 :#x dispatch 10 :kill $f1 :kill $f2 <-0}", "0");
    }

    [Test]
    public void TestTryWaitKill6 ()
    {
      //Much like test 5 except that f2 throws an exception rather than f0 getting killed.
      DoTest ("{w:{:#x write {i:++} <-w $R} f0:fiber {<-w 0} f1:fiber {<-w 0} f2:fiber {<-assert false} :wait $f2 :#x dispatch 0 :#x dispatch 10 :kill $f0 :kill $f1 <-0}", "0");
    }

    [Test]
    public void TestKillAfterThrow ()
    {
      DoTest ("{b:bot {:assert false} :wait $b :kill $b <-0}", "0");
    }

    [Test]
    public void TestWaitFiberTimeout ()
    {
      DoTest ("#status get try {<-500 wait fiber {<-read #a}}", "1");
    }

    [Test]
    public void TestWaitBotTimeout ()
    {
      DoTest ("#status get try {<-500 wait bot {<-read #a}}", "1");
    }

    [Test]
    public void TestWaitFiberNoTimeout ()
    {
      DoTest ("{f:fiber {<-try {<-500 wait fiber {<-read #a}}} :#a write {x:1} <-#status get wait $f}", "0");
    }

    [Test]
    public void TestWaitBotNoTimeout ()
    {
      DoTest ("#status get try {<-500 wait bot {f:fiber {<-read #a} :#a write {x:1} <-wait $f}}", "0");
    }

    [Test]
    public void TestWaitWithConflictingResult1 ()
    {
      DoTest ("{f1:fiber {:read #a} f2:fiber {:try {<-200 wait $f1} :try {<-kill $f1}} :wait $f2 <-0}", "0");
      // The single exception is for the killed fiber
      NUnit.Framework.Assert.AreEqual (1, runner.ExceptionCount);
    }

    [Test]
    public void TestWaitWithConflictingResult2 ()
    {
      DoTest ("{p:{f1:fiber {:read #a} f2:fiber {:try {<-200 wait $f1} :try {<-kill $f1}} :wait $f2} :p {} :p {} <-0}", "0");
      NUnit.Framework.Assert.AreEqual (2, runner.ExceptionCount);
    }

    [Test]
    public void TestWaitWithConflictingResult3 ()
    {
      DoTest ("{p:{f1:fiber {out:#a read 0 <-$out} f2:fiber {:kill $f1 :wait $f1 :#a write {x:0}} :wait $f2 <-0} :p {} :clear #a :p {} <-0}", "0");
      NUnit.Framework.Assert.AreEqual (2, runner.ExceptionCount);
    }

    [Test]
    public void TestWaitWithConflictingResult4 ()
    {
      DoTest ("{f:fiber {:#a read 0 :#m putm 1} :200 wait $f :kill $f :#a write {x:1} :wait $f :assert not hasm #m <-0}", "0");
    }

    //These three tests are still important but the module operator itself
    //can be removed since it was an abomination. Should work "out of the box" now.
    [Test]
    public void TestModule ()
    {
      DoTest ("{lib:{f1:{<-$R * $R} f2:{<-f1 $R}} <-lib.f2 3}", "9");
    }

    [Test]
    public void TestModule1 ()
    {
      DoTest ("{lib:{f1:{<-$R * $R} f2:{<-f1 $R} f3:{<-f2 $R}} <-lib.f3 3}", "9");
    }

    [Test]
    public void TestModule2 ()
    {
      DoTest ("{lib:{f1:{<-1 + 2}} <-lib.f1 {}}", "3");
    }

    [Test]
    public void TestParseDefault ()
    {
      DoTest ("parse \"{a:1 b:2.0 c:\\\"3\\\"}\"", "{a:1 b:2.0 c:\"3\"}");
    }

    [Test]
    public void TestParseDefault1 ()
    {
      DoTest ("#rcl parse \"{a:1 b:2.0 c:\\\"3\\\"}\"", "{a:1 b:2.0 c:\"3\"}");
    }

    [Test]
    public void TestTryParse ()
    {
      DoTest ("tryparse \"{a:1 b:2 c:3}\"", "{status:0 fragment:false data:{a:1 b:2 c:3}}");
    }

    [Test]
    public void TestTryParseFragment ()
    {
      DoTest ("tryparse \"a:1 b:2 c:3\"", "{status:0 fragment:true data:{a:1 b:2 c:3}}");
    }

    [Test]
    public void TestParseCSV ()
    {
      //Example from http://en.wikipedia.org/wiki/Comma-separated_values
      //I think I am missing a test for an single double quote escaped between two other double quotes.
      //DoTest ("#csv parse \"Year,Make,Model,Description,Price\\n1997,Ford,E350,\\\"ac, abs, moon\\\",3000.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended Edition\\\"\\\"\\\",\\\"\\\",4900.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended Edition, Very Large\\\"\\\"\\\",\\\"\\\",5000.00\\n1996,Jeep,Grand Cherokee,\\\"MUST SELL!\\nair, moon roof, loaded\\\",4799.00\"",
      //        "{Year:\"1997\" \"1999\" \"1999\" \"1996\" Make:\"Ford\" \"Chevy\" \"Chevy\" \"Jeep\" Model:\"E350\" \"\\\"Venture \\\"\\\"Extended Edition\\\"\\\"\\\"\" \"\\\"Venture \\\"\\\"Extended Edition, Very Large\\\"\\\"\\\"\" \"Grand Cherokee\" Description:\"\\\"ac, abs, moon\\\"\" \"\\\"\\\"\" \"\\\"\\\"\" \"\\\"MUST SELL!\\nair, moon roof, loaded\\\"\" Price:\"3000.00\" \"4900.00\" \"5000.00\" \"4799.00\"}");
      DoTest ("#csv parse \"Year,Make,Model,Description,Price\\n1997,Ford,E350,\\\"ac, abs, moon\\\",3000.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended Edition\\\"\\\"\\\",\\\"\\\",4900.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended Edition, Very Large\\\"\\\"\\\",\\\"\\\",5000.00\\n1996,Jeep,Grand Cherokee,\\\"MUST SELL!\\nair, moon roof, loaded\\\",4799.00\"",
              "{Year:\"1997\" \"1999\" \"1999\" \"1996\" Make:\"Ford\" \"Chevy\" \"Chevy\" \"Jeep\" Model:\"E350\" \"Venture \\\"\\\"Extended Edition\\\"\\\"\" \"Venture \\\"\\\"Extended Edition, Very Large\\\"\\\"\" \"Grand Cherokee\" Description:\"ac, abs, moon\" \"\" \"\" \"MUST SELL!\\nair, moon roof, loaded\" Price:\"3000.00\" \"4900.00\" \"5000.00\" \"4799.00\"}");

      //No terminal newline - this is not really valid csv, let's not worry about it.
      //DoTest ("#csv parse \"x,y,z\\nX,Y,\"", "{x:\"X\" y:\"Y\" z:\"\"}");
    }

    [Test]
    public void TestParseCSV1 ()
    {
      //Empty string at the end of a line.
      //\r\n for newlines.
      DoTest ("#csv parse \"a,b,c\\r\\nA,B,\\r\\n\"", "{a:\"A\" b:\"B\" c:\"\"}");
    }

    [Test]
    public void TestParseCSV2 ()
    {
      //Empty string at the beginning of a line.
      DoTest ("#csv parse \"a,b,c\\r\\n,B,C\\r\\n\"", "{a:\"\" b:\"B\" c:\"C\"}");
    }

    [Test]
    public void TestParseCSV3 ()
    {
      DoTest ("#csv parse \"\"", "{}");
    }

    [Test]
    public void TestParseCSV4 ()
    {
      DoTest ("#csv parse \"\n\"", "{}");
    }

    [Test]
    public void TestParseCSV5 ()
    {
      DoTest ("#csv parse \"\n\n\"", "{}");
    }

    [Test]
    public void TestParseCSV6 ()
    {
      DoTest ("#csv parse \"\n\n\n\"", "{}");
    }

    //JSON parsing test cases are from:
    //http://code.google.com/p/json-ply/source/browse/trunk/jsonply_test.py

    [Test]
    public void TestParseJSONEmptyArray ()
    {
      //Empty array
      DoTest ("#json parse \"[]\"", "{}");
    }

    [Test]
    public void TestParseJSONStringArray ()
    {
      //Array of strings
      DoTest ("#json parse \"[\\\"a\\\",\\\"b\\\",\\\"c\\\"]\"", "{:\"a\" :\"b\" :\"c\"}");
    }

    [Test]
    public void TestParseJSONNumberArray ()
    {
      //Array of numbers
      DoTest ("#json parse \"[1, 2, 3.4e5]\"", "{:1.0 :2.0 :340000.0}");
    }

    [Test]
    public void TestParseJSONArrayOfArrays ()
    {
      //Array of arrays
      DoTest ("#json parse \"[[\\\"a\\\",\\\"b\\\",\\\"c\\\"]]\"", "{:{:\"a\" :\"b\" :\"c\"}}");
    }

    [Test]
    public void TestParseJSONArrayOfDicts ()
    {
      //Array of dicts
      DoTest ("#json parse \"[{\\\"a\\\":\\\"b\\\"},{\\\"c\\\":\\\"d\\\"}]\"", "{:{a:\"b\"} :{c:\"d\"}}");
    }

    [Test]
    public void TestParseJSONMixedArray ()
    {
      //Array of mixed itmems
      DoTest ("#json parse \"[1, true, {\\\"a\\\": \\\"b\\\"}, [\\\"c\\\"]]\"", "{:1.0 :true :{a:\"b\"} :{:\"c\"}}");
    }

    [Test]
    public void TestParseJSONEmptyString ()
    {
      //Empty dict
      DoTest ("#json parse \"\"", "{}");
    }

    [Test]
    public void TestParseJSONEmptyDict ()
    {
      //Empty dict
      DoTest ("#json parse \"{}\"", "{}");
    }

    [Test]
    public void TestParseJSONStringDict ()
    {
      //Dict of strings
      DoTest ("#json parse \"{\\\"a\\\":\\\"b\\\" \\\"c\\\":\\\"d\\\"}\"", "{a:\"b\" c:\"d\"}");
    }

    [Test]
    public void TestParseJSONNumberDict ()
    {
      //Dict of numbers
      DoTest ("#json parse \"{\\\"a\\\":1.0 \\\"b\\\":2.3}\"", "{a:1.0 b:2.3}");
    }

    [Test]
    public void TestParseJSONArrayDict ()
    {
      //Dict of arrays
      DoTest ("#json parse \"{\\\"a\\\": [\\\"b\\\", \\\"c\\\"], \\\"d\\\":[1.0, 2.3]}\"", "{a:{:\"b\" :\"c\"} d:{:1.0 :2.3}}");
    }

    [Test]
    public void TestParseJSONDictDict ()
    {
      //Dict of dicts
      DoTest ("#json parse \"{\\\"a\\\": {\\\"b\\\":\\\"c\\\"} \\\"d\\\": {\\\"e\\\":\\\"f\\\"}}\"", "{a:{b:\"c\"} d:{e:\"f\"}}");
    }

    [Test]
    public void TestParseJSONMixedDict ()
    {
      //Dict of mixed items
      DoTest ("#json parse \"{\\\"a\\\": true, \\\"b\\\": [1.0, 2.3], \\\"c\\\": {\\\"d\\\":null}}\"", "{a:true b:{:1.0 :2.3} c:{d:{}}}");
    }

    [Test]
    public void TestParseXML1 ()
    {
      //Single empty element
      DoTest ("#xml parse \"<a></a>\"", "{a:{:\"\"}}");
    }

    [Test]
    public void TestParseXML2 ()
    {
      //Empty element single tag
      DoTest ("#xml parse \"<a/>\"", "{a:{:\"\"}}");
    }

    [Test]
    public void TestParseXML3 ()
    {
      //Single element with content
      DoTest ("#xml parse \"<a>x</a>\"", "{a:{:\"x\"}}");
    }

    [Test]
    public void TestParseXML4 ()
    {
      //Single element with attribute
      DoTest ("#xml parse \"<a b=\\\"x\\\"></a>\"", "{a:{b:\"x\" :\"\"}}");
    }

    [Test]
    public void TestParseXML5 ()
    {
      //Multiple attributes in an element
      DoTest ("#xml parse \"<a b=\\\"x\\\" c=\\\"y\\\" d=\\\"z\\\">w</a>\"", "{a:{b:\"x\" c:\"y\" d:\"z\" :\"w\"}}");
    }

    [Test]
    public void TestParseXML6 ()
    {
      //Multiple elements in a document
      DoTest ("#xml parse \"<a>x</a><b>y</b><c>z</c>\"", "{a:{:\"x\"} b:{:\"y\"} c:{:\"z\"}}");
    }

    [Test]
    public void TestParseXML7 ()
    {
      //Multiple elements with different content types.
      DoTest ("#xml parse \"<a>x</a><b></b><c/>\"", "{a:{:\"x\"} b:{:\"\"} c:{:\"\"}}");
    }

    [Test]
    public void TestParseXML8 ()
    {
      //Nested elements
      DoTest ("#xml parse \"<a><b>x</b><c>y</c><d>z</d></a>\"", "{a:{:{b:{:\"x\"} c:{:\"y\"} d:{:\"z\"}}}}");
    }

    //Single empty element
    [Test]
    public void TestParseXml9 ()
    {
      DoTest ("#xml parse \"<a></a>\"", "{a:{:\"\"}}");
    }

    //Empty element single tag
    [Test]
    public void TestParseXml10 ()
    {
      DoTest ("#xml parse \"<a/>\"", "{a:{:\"\"}}");
    }

    //Single element with content
    [Test]
    public void TestParseXml11 ()
    {
      DoTest ("#xml parse \"<a>x</a>\"", "{a:{:\"x\"}}");
    }

    //Multiple elements in a document
    [Test]
    public void TestParseXml12 ()
    {
      DoTest ("#xml parse \"<a>x</a><b>y</b><c>z</c>\"", "{a:{:\"x\"} b:{:\"y\"} c:{:\"z\"}}");
    }

    //Multiple elements with different content types.
    [Test]
    public void TestParseXml13 ()
    {
      DoTest ("#xml parse \"<a>x</a><b></b><c/>\"", "{a:{:\"x\"} b:{:\"\"} c:{:\"\"}}");
    }

    //Single nested multiple elements
    [Test]
    public void TestParseXml14 ()
    {
      DoTest ("#xml parse \"<a><b>x</b><c>y</c><d>z</d></a>\"", "{a:{:{b:{:\"x\"} c:{:\"y\"} d:{:\"z\"}}}}");
    }

    //Multiple nested single elements
    [Test]
    public void TestParseXml15 ()
    {
      DoTest ("#xml parse \"<a><b><c>x</c></b></a>\"", "{a:{:{b:{:{c:{:\"x\"}}}}}}");
    }

    //Multiple nested multiple elements
    [Test]
    public void TestParseXml16 ()
    {
      DoTest ("#xml parse \"<a><b><c>x</c><d>y</d></b><e/></a>\"", "{a:{:{b:{:{c:{:\"x\"} d:{:\"y\"}}} e:{:\"\"}}}}");
    }

    //Single empty element
    [Test]
    public void TestParseXml17 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"></a>\"", "{a:{f:\"i\" :\"\"}}");
    }

    //Empty element single tag
    [Test]
    public void TestParseXml18 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"/>\"", "{a:{f:\"i\" :\"\"}}");
    }

    //Single element with content
    [Test]
    public void TestParseXml19 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a>\"", "{a:{f:\"i\" :\"x\"}}");
    }

    //Multiple elements in a document
    [Test]
    public void TestParseXml20 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a><b g=\\\"j\\\">y</b><c h=\\\"k\\\">z</c>\"", "{a:{f:\"i\" :\"x\"} b:{g:\"j\" :\"y\"} c:{h:\"k\" :\"z\"}}");
    }

    //Multiple elements with different content types.
    [Test]
    public void TestParseXml21 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a><b g=\\\"j\\\"></b><c h=\\\"k\\\"/>\"", "{a:{f:\"i\" :\"x\"} b:{g:\"j\" :\"\"} c:{h:\"k\" :\"\"}}");
    }

    //Single nested multiple elements
    [Test]
    public void TestParseXml22 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b g=\\\"j\\\">x</b><c>y</c><d h=\\\"k\\\">z</d></a>\"", "{a:{f:\"i\" :{b:{g:\"j\" :\"x\"} c:{:\"y\"} d:{h:\"k\" :\"z\"}}}}");
    }

    //Multiple nested single elements
    [Test]
    public void TestParseXml23 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b g=\\\"j\\\"><c h=\\\"k\\\">x</c></b></a>\"", "{a:{f:\"i\" :{b:{g:\"j\" :{c:{h:\"k\" :\"x\"}}}}}}");
    }

    //Multiple nested multiple elements
    [Test]
    public void TestParseXml24 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b><c g=\\\"j\\\">x</c><d>y</d></b><e h=\\\"k\\\"/></a>\"", "{a:{f:\"i\" :{b:{:{c:{g:\"j\" :\"x\"} d:{:\"y\"}}} e:{h:\"k\" :\"\"}}}}");
    }

    [Test]
    public void TestFormatPretty1 ()
    {
      DoTest ("#pretty format {}", "\"{}\"");
    }

    [Test]
    public void TestFormatPretty2 ()
    {
      DoTest ("#pretty format {a:1.0 b:2.0 c:3.0}", "\"{\\n  a:1.0\\n  b:2.0\\n  c:3.0\\n}\"");
    }

    [Test]
    public void TestFormatPretty3 ()
    {
      DoTest ("#pretty format {:{a:1.0 b:2.0 c:3.0}}", "\"{\\n  :{\\n    a:1.0\\n    b:2.0\\n    c:3.0\\n  }\\n}\"");
    }

    [Test]
    public void TestFormatPretty4 ()
    {
      DoTest ("#pretty format {:{a:1.0 1.0 1.0 b:2.0 2.0 2.0 c:3.0 3.0 3.0}}", "\"{\\n  :{\\n    a:1.0 1.0 1.0\\n    b:2.0 2.0 2.0\\n    c:3.0 3.0 3.0\\n  }\\n}\"");
    }

    [Test]
    public void TestFormatPretty5 ()
    {
      //[
      //  T | S|   a b
      //  0l #x   1l "2"
      //  1l #x  10l "20"
      //  2l #x 100l "200"
      //]
      DoTest ("#pretty format [E|S|a b 0 #x 1 \"2\" 1 #x 10 \"20\" 2 #x 100 \"200\"]", "\"[\\n   E|S |  a b\\n   0 #x   1 \\\"2\\\"\\n   1 #x  10 \\\"20\\\"\\n   2 #x 100 \\\"200\\\"\\n]\"");
    }

    [Test]
    public void TestFormatPretty6 ()
    {
      DoTest ("#pretty format {u:[S|x #a 0]}", "\"{\\n  u:[\\n    S | x\\n    #a  0\\n  ]\\n}\"");
    }

    [Test]
    public void TestFormatText ()
    {
      DoTest ("\"\\r\" \"\" replace #text format \"line one\" \"line two\" \"line three\"", "\"line one\\nline two\\nline three\\n\"");
    }

    [Test]
    public void TestFormatFragment ()
    {
      DoTest ("#fragment format {:[S|x #a 1 #b 2 #c 3]}", "\":[\\n  S | x\\n  #a  1\\n  #b  2\\n  #c  3\\n]\\n\"");
    }

    [Test]
    public void TestFormatTextCrlf ()
    {
      DoTest ("#textcrlf format \"line one\" \"line two\" \"line three\"", "\"line one\\r\\nline two\\r\\nline three\\r\\n\"");
    }

    [Test]
    public void TestFormatTextLf ()
    {
      DoTest ("#textlf format \"line one\" \"line two\" \"line three\"", "\"line one\\nline two\\nline three\\n\"");
    }

    [Test]
    public void TestFormatLog ()
    {
      //The log format option omits the header and uses spaces instead of commas for the delimiter.
      DoTest ("#log format [a b c 0 \"x y z\" #foo 1 \"u v w\" #bar 2 \"s t u\" #baz]", 
              "\"0 \\\"x y z\\\" foo\\n1 \\\"u v w\\\" bar\\n2 \\\"s t u\\\" baz\\n\"");
    }

    //I was going to make binary a variant of format.
    //But binary will return a byte vector and format returns a string vector.
    //I do not want the output from an operator to vary based on the arguments.
    //(varying based on the TYPE of the arguments is ok).
    //So use a different operator name to format binary data.
    [Test]
    public void TestBinaryFixed1 ()
    {
      DoTest ("parse binary -1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000", "-1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000");
    }

    [Test]
    public void TestBinaryFixed2 ()
    {
      DoTest ("parse binary -1000000000.0 -2000000.0 -3000.0 -4.0 0.0 5.0 6000.0 7000000.0 8000000000.0", "-1000000000.0 -2000000.0 -3000.0 -4.0 0.0 5.0 6000.0 7000000.0 8000000000.0");
    }

    [Test]
    public void TestBinaryFixed3 ()
    {
      DoTest ("parse binary -1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000m", "-1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000m");
    }

    [Test]
    public void TestBinaryFixed4 ()
    {
      DoTest ("parse binary -12345678.9 -234567.89 -3.456 -0.4 0 5 6.789 789.1234 8912.345678m", "-12345678.9 -234567.89 -3.456 -0.4 0 5 6.789 789.1234 8912.345678m");
    }

    [Test]
    public void TestBinaryFixed5 ()
    {
      DoTest ("parse binary true false true", "true false true");
    }

    [Test]
    public void TestBinaryFixed6 ()
    {
      DoTest ("parse binary \\x00 \\x01 \\x02", "\\x00 \\x01 \\x02");
    }

    [Test]
    public void TestBinaryFixed7 ()
    {
      DoTest ("parse binary ++ ++ ++", "++ ++ ++");
    }

    [Test]
    public void TestBinaryFixed8 ()
    {
      DoTest ("parse binary 2015.05.25 2015.05.26 2015.05.27", "2015.05.25 2015.05.26 2015.05.27");
    }

    [Test]
    public void TestBinaryFixed9 ()
    {
      DoTest ("parse binary {<-$R * $R}", "{<-$R * $R}");
    }

    [Test]
    public void TestBinaryFixed10 ()
    {
      DoTest ("parse binary {<-$R.x * $R.y}", "{<-$R.x * $R.y}");
    }

    [Test]
    public void TestBinaryFixed11 ()
    {
      DoTest ("parse binary {<-$L lib.f $R}", "{<-$L lib.f $R}");
    }

    //Vectors of variable length types like string and reference.
    [Test]
    public void TestBinaryString1 ()
    {
      DoTest ("parse binary \"a\" \"ba\" \"cba\" \"dcba\"", "\"a\" \"ba\" \"cba\" \"dcba\"");
    }

    //test with repeated values
    [Test]
    public void TestBinaryString2 ()
    {
      DoTest ("parse binary \"x\" \"yy\" \"zzz\" \"yy\" \"x\" \"yy\" \"zzz\"", "\"x\" \"yy\" \"zzz\" \"yy\" \"x\" \"yy\" \"zzz\"");
    }

    //With simple fixed types only.
    [Test]
    public void TestBinarySymbol1 ()
    {
      DoTest ("parse binary #1,2.0,true", "#1,2.0,true");
    }

    //With fixed types including decimal
    [Test]
    public void TestBinarySymbol2 ()
    {
      DoTest ("parse binary #1,2.0,3m,true", "#1,2.0,3m,true");
    }
    //With more than one value
    [Test]
    public void TestBinarySymbol3 ()
    {
      DoTest ("parse binary #1,2.0,3m #4,5.0,6m #7,8.0,9m", "#1,2.0,3m #4,5.0,6m #7,8.0,9m");
    }
    //With repeating values
    [Test]
    public void TestBinarySymbol4 ()
    {
      DoTest ("parse binary #1,2.0,3m #4,5.0,6m #1,2.0,3m", "#1,2.0,3m #4,5.0,6m #1,2.0,3m");
    }
    //With strings (variable length)
    [Test]
    public void TestBinarySymbol5 ()
    {
      DoTest ("parse binary #1,2.0,3m,true,abcdef", "#1,2.0,3m,true,abcdef");
    }

    //Mixing strings and repeated values.
    //DoTest ("parse binary #abc #de,f #abc", "#abc #de,f #abc");

    //block as expression holder.
    [Test]
    public void TestBinaryContainers1 ()
    {
      DoTest ("parse binary {<-1 + 2.0}", "{<-1 + 2.0}");
    }

    //block with mixed variables.
    [Test]
    public void TestBinaryContainer2 ()
    {
      DoTest ("parse binary {a:1 2 3 b:4.0 5.0 6.0 c:7 8 9m d:true false e:\"this\" \"a\" \"string\" f:#x,y,z #u,v,w #r,s,t}", "{a:1 2 3 b:4.0 5.0 6.0 c:7 8 9m d:true false e:\"this\" \"a\" \"string\" f:#x,y,z #u,v,w #r,s,t}");
    }

    //block with complex expression.
    [Test]
    public void TestBinaryContainers3 ()
    {
      DoTest ("parse binary {<-(1 - 2 * 4) - 3}", "{<-(1 - 2 * 4) - 3}");
    }

    //nested blocks
    [Test]
    public void TestBinaryContainers4 ()
    {
      DoTest ("parse binary {l:{a:0 b:1 c:2} d:{d:3.0 e:4.0 f:5.0} m:{g:6m h:7m i:8m}}", "{l:{a:0 b:1 c:2} d:{d:3.0 e:4.0 f:5.0} m:{g:6m h:7m i:8m}}");
    }

    //cubes
    [Test]
    public void TestBinaryCube1 ()
    {
      DoTest ("parse binary [S|l d m #a 10 -- -- #a 11 21.0 -- #a 12 22.0 32m]", "[S|l d m #a 10 -- -- #a 11 21.0 -- #a 12 22.0 32m]");
    }

    [Test]
    public void TestBinaryCube2 ()
    {
      DoTest ("parse binary [E|S|l d m 0 #a 10 -- -- 1 #a 11 21.0 -- 2l #a 12 22.0 32m]", "[E|S|l d m 0 #a 10 -- -- 1 #a 11 21.0 -- 2 #a 12 22.0 32m]");
    }

    [Test]
    public void TestCompare0 ()
    {
      DoTest ("\"aaa\\n\" compare \"bbb\\n\"", "[op old new \"DELETE\" \"aaa\" -- \"INSERT\" -- \"bbb\" \"EQUAL\" \"\\n\" \"\\n\"]");
    }

    [Test]
    public void TestCompare1 ()
    {
      DoTest ("\"aba\" compare \"abc\"", "[op old new \"EQUAL\" \"ab\" \"ab\" \"DELETE\" \"a\" -- \"INSERT\" -- \"c\"]");
    }

    [Test]
    public void TestCompare2 ()
    {
      DoTest ("\"a\nx\nb\n\" compare \"a\nb\nc\n\"", "[op old new \"EQUAL\" \"a\\n\" \"a\\n\" \"DELETE\" \"x\\n\" -- \"EQUAL\" \"b\\n\" \"b\\n\" \"INSERT\" -- \"c\\n\"]");
    }

    [Test]
    public void TestCompare3 ()
    {
      DoTest ("\"aaa\nddd\n\" compare \"aaa\nbbb\nccc\nddd\n\"", "[op old new \"EQUAL\" \"aaa\\n\" \"aaa\\n\" \"INSERT\" -- \"bbb\\n\" \"INSERT\" -- \"ccc\\n\" \"EQUAL\" \"ddd\\n\" \"ddd\\n\"]");
    }

    [Test]
    public void TestCompare4 ()
    {
      DoTest ("\"aaa\\nbbb\\nccc\\nddd\\n\" compare \"aaa\\nddd\\n\"", "[op old new \"EQUAL\" \"aaa\\n\" \"aaa\\n\" \"DELETE\" \"bbb\\n\" -- \"DELETE\" \"ccc\\n\" -- \"EQUAL\" \"ddd\\n\" \"ddd\\n\"]");
    }

    [Test]
    public void TestCompare5 ()
    {
      DoTest ("\"aaaaaaaa0\" compare \"1aaaaaaaa2\"", "[op new old \"INSERT\" \"1\" -- \"EQUAL\" \"aaaaaaaa\" \"aaaaaaaa\" \"DELETE\" -- \"0\" \"INSERT\" \"2\" --]");
    }

    [Test]
    public void TestCompare6 ()
    {
      DoTest ("\"aaaaaaaa0\" compare \"aaaaaaaa2\"", "[op old new \"EQUAL\" \"aaaaaaaa\" \"aaaaaaaa\" \"DELETE\" \"0\" -- \"INSERT\" -- \"2\"]");
    }

    [Test]
    public void TestCompare7 ()
    {
      DoTest ("\"[?\n  first line\n  second line\n\" compare \"[?\n  first line \n  second line\n\"", "[op old new \"EQUAL\" \"[?\\n\" \"[?\\n\" \"EQUAL\" \"  first line\" \"  first line\" \"INSERT\" -- \" \" \"EQUAL\" \"\\n\" \"\\n\" \"EQUAL\" \"  second line\\n\" \"  second line\\n\"]");
    }

    [Test]
    public void TestGetm ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate () { DoTest ("getm #foo", "{}"); });
    }

    [Test]
    public void TestGetm1 ()
    {
      DoTest ("{:#foo putm 1 <-getm #foo}", "1");
    }

    [Test]
    public void TestGetm2 ()
    {
      DoTest ("{:#foo putm 1 :assert hasm #foo :delm #foo :assert not hasm #foo <-0}", "0");
    }

#if __MonoCS__
    [Test]
    public void TestMonoTryWaitx ()
    {
      //This was to expose a race condition in exec.
      //It would return zero if the process exited before waitx registered.
      for (int i = 0; i < 10; ++i)
      {
        DoTest ("{sh:startx \"bash\" :$sh writex \"exit 1\n\" <-#status #data from try {<-waitx $sh}}", "{status:1 data:[?\n    <<Exec,exit status 1>>\n  ?]}");
      }
    }

    [Test]
    [Ignore ("problem with bash shell under debugger")]
    public void TestExec1 ()
    {
      DoTest ("{:try {<-exec \"rm foo/bar\"} :try {<-exec \"rmdir foo\"} <-0}", "0");
    }

    [Test]
    [Ignore ("problem with bash shell under debugger")]
    public void TestExec2 ()
    {
      DoTest ("try {<-exec \"ls foo\"}", "{status:1 data:\"Non-zero exit status\"}");
    }

    [Test]
    [Ignore ("problem with bash shell under debugger")]
    public void TestExec3 ()
    {
      DoTest ("{:exec \"mkdir foo\" <-count exec \"ls foo\"}", "0");
    }

    [Test]
    [Ignore ("problem with bash shell under debugger")]
    public void TestExec4 ()
    {
      DoTest ("{:exec \"touch foo/bar\" <-exec \"ls foo\"}", "\"bar\"");
    }

    [Test]
    [Ignore ("problem with bash shell under debugger")]
    public void TestExec5 ()
    {
      DoTest ("{:exec \"rm foo/bar\" :exec \"rmdir foo\" <-0}", "0");
    }

    [Test]
    [Ignore ("There some issue spawning a bash shell under the debugger env. Would like to know why.")]
    public void TestExecBash ()
    {
      DoTest ("{sh:startx \"sh\" :$sh writex \"set\\\\nmkdir foo\" <-try {<-waitx $sh}}", "{status:1 data:~s}");
    }
#endif
  }
}

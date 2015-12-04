
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  public class CoreTest
  {
    //Gui Operators? Are gui operators like gui controls?
    //We could have operators that yield strings and in the 
    //end you just traverse the document and cat all the strings
    //together.
    protected RCRunner runner = new RCRunner (RCActivator.Default, new RCLog (new RCL.Core.Output ()), 1, RCRunner.CreateArgs ());
    public void DoTest (string code, string expected)
    {
      try
      {
        runner.Reset ();
        RCValue program = runner.Read (code);
        RCValue result = runner.Run (program);
        System.Console.Out.WriteLine ("code:");
        System.Console.Out.WriteLine (code);
        System.Console.Out.WriteLine ("expected:");
        System.Console.Out.WriteLine (expected);
        System.Console.Out.WriteLine ("actual:");
        Assert.IsNotNull (result, "RCRunner.Run result was null");
        System.Console.Out.WriteLine (result.ToString ());
        Assert.AreEqual (expected, result.ToString ());
      }
      catch (RCException ex)
      {
        System.Console.Out.WriteLine (ex.ToString ());
        throw;
      }
      catch (System.Exception ex)
      {
        Assert.Fail (ex.ToString ());
      }
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
    [ExpectedException(typeof(RCException))]
    public void TestPlusDM() { DoTest("1.0+2m", "3.0"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestPlusMD() { DoTest("1m+2.0", "3m"); }
    [Test]
    public void TestPlusML() { DoTest("1m+2", "3m"); }
    [Test]
    public void TestPlusMX() { DoTest("1m + \\x02", "3m"); }
    [Test]
    public void TestPlusSS() { DoTest("\"x\"+\"y\"", "\"xy\""); }
    [Test]
    public void TestPlusYY() { DoTest("#x+#y", "#x,y"); }
    [Test]
    public void TestPlusTT() { DoTest ("2015.05.31 + 1.00:00:00.0000000", "2015.06.01 00:00:00.0000000"); }
    [Test]
    public void TestPlusTT1() { DoTest ("1.00:00:00.0000000 + 2015.05.31", "2015.06.01 00:00:00.0000000"); }
    [Test]
    public void TestPlusTT2() { DoTest ("1.00:00:00.0000000 + 2.00:00:00.0000000", "3.00:00:00.0000000"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestMinusDM() { DoTest("1.0-2m", "-1.0"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestMinusMD() { DoTest("1m-2d", "-1m"); }
    [Test]
    public void TestMinusML() { DoTest("1m-2", "-1m"); }
    [Test]
    public void TestMinusMX() { DoTest("1m-\\x02", "-1m"); }
    [Test]
    public void TestMinusTT0() { DoTest ("2015.05.31 - 1.00:00:00.0000000", "2015.05.30 00:00:00.0000000"); }
    [Test]
    public void TestMinusTT1() { DoTest ("2015.05.31 - 2015.05.10", "21.00:00:00.0000000"); }
    [Test]
    public void TestMinusTT2() { DoTest ("1.00:00:00.0000000 - 2.00:00:00.0000000", "-1.00:00:00.0000000"); }

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
    [ExpectedException(typeof(RCException))]
    public void TestMultiplyDM() { DoTest ("1.0*2m", "2.0"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestMultiplyMD() { DoTest ("1m*2.0", "2.0"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestDivideDM() { DoTest("4.0/2m", "2.0"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestDivideMD() { DoTest("4m/2.0", "2.0"); }
    [Test]
    public void TestDivideML() { DoTest("4m/2", "2m"); }
    [Test]
    public void TestDivideMX() { DoTest("4m/\\x02", "2m"); }

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
    [ExpectedException(typeof(RCException))]
    public void TestGtDM() { DoTest("1.0>2m", "false"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestGtMD() { DoTest("1m>2.0", "false"); }
    [Test]
    public void TestGtML() { DoTest("1m>2", "false"); }
    [Test]
    public void TestGtMM() { DoTest("1m>2m", "false"); }
    [Test]
    public void TestGtMX() { DoTest("1m>\\x02", "false"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestGteDM() { DoTest("1.0>=2m", "false"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestGteMD() { DoTest("1m>=2.0", "false"); }
    [Test]
    public void TestGteML() { DoTest("1m>=2", "false"); }
    [Test]
    public void TestGteMM() { DoTest("1m>=2m", "false"); }
    [Test]
    public void TestGteMX() { DoTest("1m>=\\x02", "false"); }

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
    [ExpectedException(typeof(RCException))]
    public void TestLtDM() { DoTest("1.0<2m", "true");}
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
    [ExpectedException(typeof(RCException))]
    public void TestLtMD() { DoTest("1m<2.0", "true"); }
    [Test]
    public void TestLtML() { DoTest("1m<2", "true"); }
    [Test]
    public void TestLtMM() { DoTest("1m<2m", "true"); }
    [Test]
    public void TestLtMX() { DoTest ("1m<\\x02", "true"); }

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
    [ExpectedException(typeof(RCException))]
    public void TestLteDM() { DoTest("1.0<=2m", "true"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestLteMD() { DoTest("1m<=2.0", "true"); }
    [Test]
    public void TestLteML() { DoTest("1m<=1", "true"); }
    [Test]
    public void TestLteMM() { DoTest("1m<=2m", "true"); }
    [Test]
    public void TestLteMX() { DoTest("1m<=\\x02", "true"); }

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
    public void TestEqDD() { DoTest("1.0==2.0", "false"); }
    [Test]
    public void TestEqDL() { DoTest("1.0==2", "false"); }
    [Test]
    [ExpectedException(typeof(RCException))]
    public void TestEqDM() { DoTest("1.0==2m", "false"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestEqMD() { DoTest("1m==2.0", "false"); }
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
    public void TestEqSS() { DoTest ("\"x\"==\"x\"", "true"); }

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
    [ExpectedException(typeof(RCException))]
    public void TestNeqDM() { DoTest("1.0!=2m", "true"); }
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
    [ExpectedException(typeof(RCException))]
    public void TestNeqMD() { DoTest("1m!=2.0", "true"); }
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
    public void TestStringT1 () { DoTest ("string 08:26", "\"08:26\""); }
    [Test]
    public void TestStringT2 () { DoTest ("string 2015.05.26 08:26", "\"2015.05.26 08:26\""); }
    [Test]
    public void TestStringT3 () { DoTest ("string 2015.05.26 08:26:00.1234567", "\"2015.05.26 08:26:00.1234567\""); }

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
    public void TestLongT3 () { DoTest ("long 0001.01.01 00:00:00.0000000", "0"); }

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
    public void TestDecimalK() { DoTest("decimal {x:0m y:1m z:2m}", "0 1 2m"); }

    [Test]
    public void TestTimeL () { DoTest ("time 0", "0001.01.01 00:00:00.0000000"); }
    [Test]
    public void TestTimeYL0 () { DoTest ("#date time 0", "0001.01.01"); }
    [Test]
    public void TestTimeYL1 () { DoTest ("#daytime time 0", "00:00"); }
    [Test]
    public void TestTimeYL2 () { DoTest ("#datetime time 0", "0001.01.01 00:00"); }
    [Test]
    public void TestTimeYL3 () { DoTest ("#timestamp time 0", "0001.01.01 00:00:00.0000000"); }

    [Test]
    public void TestDayT () { DoTest ("day 0.12:34:56.7890123", "0"); }
    [Test]
    public void TestDayTU () { DoTest ("day [x 0.12:34:56.7890123]", "[x 0]"); }
    [Test]
    public void TestHourT () { DoTest ("hour 0.12:34:56.7890123", "12"); }
    [Test]
    public void TestHourTU () { DoTest ("hour [x 0.12:34:56.7890123]", "[x 12]"); }
    [Test]
    public void TestMinuteT () { DoTest ("minute 0.12:34:56.7890123", "34"); }
    [Test]
    public void TestMinuteTU () { DoTest ("minute [x 0.12:34:56.7890123]", "[x 34]"); }
    [Test]
    public void TestSecondT () { DoTest ("second 0.12:34:56.7890123", "56"); }
    [Test]
    public void TestSecondTU () { DoTest ("second [x 0.12:34:56.7890123]", "[x 56]"); }
    [Test]
    public void TestNanoT () { DoTest ("nano 0.12:34:56.7890123", "789012300"); }
    [Test]
    public void TestNanoTU () { DoTest ("nano [x 0.12:34:56.7890123]", "[x 789012300]"); }

    [Test]
    public void TestDateT () { DoTest ("timestamp date 1979.09.04 12:34:56.7891011", "1979.09.04 00:00:00.0000000"); }
    [Test]
    public void TestDateTU () { DoTest ("timestamp date [x 1979.09.04 12:34:56.7891011]", "[x 1979.09.04 00:00:00.0000000]"); }
    [Test]
    public void TestDaytimeT () { DoTest ("timestamp daytime 1979.09.04 12:34:56.7891011", "0001.01.01 12:34:00.0000000"); }
    [Test]
    public void TestDaytimeTU () { DoTest ("timestamp daytime [x 1979.09.04 12:34:56.7891011]", "[x 0001.01.01 12:34:00.0000000]"); }
    [Test]
    public void TestDatetimeT () { DoTest ("timestamp datetime 1979.09.04 12:34:56.7891011", "1979.09.04 12:34:00.0000000"); }
    [Test]
    public void TestDatetimeTU () { DoTest ("timestamp datetime [x 1979.09.04 12:34:56.7891011]", "[x 1979.09.04 12:34:00.0000000]"); }
    [Test]
    public void TestTimestampT () { DoTest ("timestamp timestamp 1979.09.04 12:34:56.7891011", "1979.09.04 12:34:56.7891011"); }
    [Test]
    public void TestTimestampTU () { DoTest ("timestamp timestamp [x 1979.09.04 12:34:56.7891011]", "[x 1979.09.04 12:34:56.7891011]"); }
    [Test]
    public void TestTimespanT () { DoTest ("timespan 1979.09.04 12:34:56.7891011", "722695.12:34:56.7891011"); }
    [Test]
    public void TestTimespanTU () { DoTest ("timespan [x 1979.09.04 12:34:56.7891011]", "[x 722695.12:34:56.7891011]"); }

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


    //Length for strings
    [Test]
    public void TestLengthS () { DoTest ("length \"aaa\" \"bb\" \"a\" \"\"", "3 2 1 0"); }

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

    //RC Reader and Writer Operators

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
    public void TestAtTL() { DoTest ("01:00 02:00 03:00 at 1", "02:00"); }
    [Test]
    public void TestAtTD() { DoTest ("01:00 02:00 03:00 at 1.0", "02:00"); }
    [Test]
    public void TestAtTM() { DoTest ("01:00 02:00 03:00 at 1m", "02:00"); }
    [Test]
    public void TestAtTX() { DoTest ("01:00 02:00 03:00 at \\x01", "02:00"); }
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
    [Ignore]
    public void TestAtUL() { DoTest ("[S|x y z 0 10 100] at 1", "10"); }
    [Test]
    [Ignore]
    public void TestAtUD() { DoTest ("[S|x y z 0 10 100] at 1.0", "10"); }
    [Test]
    [Ignore]
    public void TestAtUM() { DoTest ("[S|x y z 0 10 100] at 1m", "10"); }
    [Test]
    [Ignore]
    public void TestAtUX() { DoTest ("[S|x y z 0 10 100] at \\x01", "10"); }
    [Test]
    [Ignore]
    public void TestAtUS() { DoTest ("[S|x y z 0 10 100] at \"y\"", "10"); }
    [Test]
    [Ignore]
    public void TestAtUY() { DoTest ("[S|x y z 0 10 100] at #y", "10"); }

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
    public void TestFromTL() { DoTest ("1 from 01:00 02:00 03:00", "02:00"); }
    [Test]
    public void TestFromTD() { DoTest ("1.0 from 01:00 02:00 03:00", "02:00"); }
    [Test]
    public void TestFromTM() { DoTest ("1m from 01:00 02:00 03:00", "02:00"); }
    [Test]
    public void TestFromTX() { DoTest ("\\x01 from 01:00 02:00 03:00", "02:00"); }
    [Test]
    public void TestFromLK() { DoTest ("1 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromDK() { DoTest ("1.0 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromMK() { DoTest ("1m from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromLKNeg() { DoTest ("-2 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromDKNeg() { DoTest ("-2.0 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromMKNeg() { DoTest ("-2m from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromXK() { DoTest ("\\x01 from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromKS() { DoTest ("\"y\" from {x:0 y:1 z:2}", "{y:1}"); }
    [Test]
    public void TestFromKY() { DoTest ("#y from {x:0 y:1 z:2}", "{y:1}"); }

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
    public void TestWhereMB () { DoTest ("1.0 2.0 3.0m where true false true", "1.0 3.0m"); }
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
    [Test][Ignore]
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
    [Test][Ignore]
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
    [Test] [Ignore]
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
    [Test][Ignore]
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
    [Test][Ignore]
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
    [Test][Ignore]
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
    [Test][Ignore]
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
    [Test][Ignore]
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
    [Test][Ignore]
    public void TestUniqueK () { /*???*/ }

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
    [Test][Ignore]
    public void TestMapK () { /*???*/ }

    //Flow Control Operators
    [Test]
    public void TestSleepL() { DoTest("{t0:now{} :sleep 100 t1:now{} <-1000000<$t1-$t0}", "true"); }
    [Test]
    public void TestSleepD() { DoTest("{t0:now{} :sleep 100.0 t1:now{} <-1000000<$t1-$t0}", "true"); }
    [Test]
    public void TestSleepM() { DoTest("{t0:now{} :sleep 100m t1:now{} <-1000000<$t1-$t0}", "true"); }
    [Test]
    public void TestSleepX() { DoTest("{t0:now{} :sleep \\x64 t1:now{} <-1000000<$t1-$t0}", "true"); }

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
    public void TestSwitchLK2() { DoTest("-1 switch {:1+2}", "{}"); }
    [Test]
    [Ignore]
    public void TestSwitchLK3() { DoTest("2 switch {:0 :1 :2 :3}", "2"); }
    [Test]
    public void TestSwitchXK() { DoTest("\\x00 switch {:1+2}", "3"); }
    [Test]
    public void TestSwitchYK0() { DoTest("#b switch {c:#x b:#y a:#z}", "#y"); }
    [Test]
    public void TestSwitchYK1() { DoTest("#c switch {b:#y a:#z}", "{}"); }
    //[Test]
    //public void TestSweachInTake() { DoTest("{<-#lock take {<-true sweach {:0l :1l}}}", "{:0l}"); }

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
    [Test] [Ignore]
    public void TestEachKeY() { DoTest ("{<-[?<a>[!$R!]</a>?] $R} each #a #b #c #d", "\"<a>#a</a>\" \"<a>#b</a>\" \"<a>#c</a>\" \"<a>#d</a>\""); }
    [Test] [Ignore]
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
    public void TestDifferenceL () { DoTest ("1 2 3 except 2 3 4", "1 4"); }
    [Test]
    public void TestDifferenceD () { DoTest ("1.0 2.0 3.0 except 2.0 3.0 4.0", "1.0 4.0"); }
    [Test]
    public void TestDifferenceM () { DoTest ("1 2 3m except 2 3 4m", "1 4m"); }
    [Test]
    public void TestDifferenceX () { DoTest ("\\x01 \\x02 \\x03 except \\x02 \\x03 \\x04", "\\x01 \\x04"); }
    [Test]
    public void TestDifferenceB () { DoTest ("true except false", "true false"); }
    [Test]
    public void TestDifferenceS () { DoTest ("\"a\" \"b\" \"c\" except \"b\" \"c\" \"d\"", "\"a\" \"d\""); }
    [Test]
    public void TestDifferenceY () { DoTest ("#a #b #c except #b #c #d", "#a #d"); }
    [Test]
    public void TestDifferenceT () { DoTest ("2015.05.27 2015.05.28 2015.05.29 except 2015.05.28 2015.05.29 2015.05.30", "2015.05.27 2015.05.30"); }

    //This looks more like 'in' than 'contains' to me. Is this naming appropriate?
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

    //TODO: Each for operators

    [Test]
    public void TestPrint() { DoTest ("print \"this\" \"is\" \"some\" \"output\"", "0"); }

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
    public void TestSubT  () { DoTest ("08:00 08:10 08:10 08:20 08:20 08:30 sub 08:00 08:10 08:20", "08:10 08:20 08:30"); }

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
    public void TestFillT () { DoTest ("00:00 fill 08:00 00:00 00:00 09:00 00:00 00:00 10:00 00:00 00:00", "08:00 08:00 08:00 09:00 09:00 09:00 10:00 10:00 10:00"); }
    //Eval

    //Thru

    //String/Text Operators
    [Test]
    public void TestDelimit() { DoTest ("\",\" delimit \"x\" \"y\" \"z\"", "\"x,y,z\""); }
    [Test]
    public void TestSplit () { DoTest ("\",:\" split \"x,y,z\" \"a:b:c\"", "\"x\" \"y\" \"z\" \"a\" \"b\" \"c\""); }
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
    public void TestIndexOf () { DoTest ("\"foo\" indexof \"abcfoodef\" \"abcdef\" \"foo\" \"fooabcdef\" \"abcdeffoo\"", "3 15 18 33"); }
    [Test]
    public void TestPad () { DoTest (" \"-\" pad 1 2 3", "\"-\" \"--\" \"---\""); }
    [Test]
    public void TestStartsWith () { DoTest ("\"aaa\" \"foobar\" \"fozbaz\" \"foo\" \"fo\" startswith \"foo\"", "false true false true false"); }
    [Test]
    public void TestCut () { DoTest ("0 1 2 cut \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"abcdef\" \"hijkl\" \"opqr\""); }
    [Test]
    public void TestCut1 () { DoTest ("0 -1 -2 cut \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"abcdef\" \"l\" \"qr\""); }
    [Test]
    public void TestCutleft () { DoTest ("0 1 2 cutleft \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"\" \"g\" \"mn\""); }
    [Test]
    public void TestCutleft1 () { DoTest ("0 -1 -2 cutleft \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"\" \"ghijk\" \"mnop\""); }

    //Block
    [Test]
    public void TestNamesK() { DoTest ("names {a:1 b:2 c:3}", "\"a\" \"b\" \"c\""); }
    [Test]
    public void TestRename() { DoTest ("\"x\" \"y\" \"z\" rename {a:1 b:2 c:3}", "{x:1 y:2 z:3}"); }
    [Test]
    public void TestSetK0 () { DoTest ("{x:1 y:2} set {y:3 z:4}", "{x:1 y:3 z:4}"); }
    //Still need to think about how set should work in the case of a timeline.
    [Test]
    public void TestHasY () { DoTest ("{x:1 y:2 z:3} has #a #y #z", "false true true"); }
    [Test]
    public void TestHasS () { DoTest ("{x:1 y:2 z:3} has \"a\" \"y\" \"z\"", "false true true"); }
    [Test]
    [Ignore]
    public void TestHasL () { DoTest ("{x:1 y:2 z:3} has 0 1 4", "true true false"); }

    [Test]
    public void TestUnflipK () { DoTest ("unflip {x:1 2 3 4 y:5 6 7 8 z:9 10 11 12}", "{:{x:1 y:5 z:9} :{x:2 y:6 z:10} :{x:3 y:7 z:11} :{x:4 y:8 z:12}}"); }
    [Test]
    public void TestFlipK0 () { DoTest ("flip {:{x:1 y:4 z:7} :{x:2 y:5 z:8} :{x:3 y:6 z:9}}", "{x:1 2 3 y:4 5 6 z:7 8 9}"); }
    [Test]
    public void TestFlipK1 () { DoTest ("flip {:{x:1 y:5 z:9} :{x:2 y:6 z:10} :{x:3 y:7 z:11} :{x:4 y:8 z:12}}", "{x:1 2 3 4 y:5 6 7 8 z:9 10 11 12}"); }

  }

  [TestFixture]
  public class CoreTest3 : CoreTest
  {
    //Messaging/Stream Operators
    [Test]
    [Ignore]
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
    public void TestTryFail ()
    {
      DoTest ("try {<-900 fail \"fail with status 900\"}", "{status:900 data:\"fail with status 900\"}");
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
      DoTest ("{b:bot {<-try {<-#x assert #y}} <-$b}", "1");
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
      DoTest ("try {<-assert false}", "{status:1 data:\"An exception was thrown by the operator (assert false)\"}");
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
    public void TestModule ()
    {
      DoTest ("{lib:module {f1:{<-$R * $R} f2:{<-f1 $R}} <-lib.f2 3}", "9");
    }

    [Test]
    public void TestModule1 ()
    {
      DoTest ("{lib:module {f1:{<-$R * $R} f2:{<-f1 $R} f3:{<-f2 $R}} <-lib.f3 3}", "9");
    }

    [Test]
    public void TestModule2 ()
    {
      DoTest ("{lib:module {f1:{<-1 + 2}} <-lib.f1 {}}", "3");
    }

    //Parsing other data formats
    [Test]
    public void TestParseDefault ()
    {
      DoTest ("#o2 parse \"{a:1 b:2.0 c:\\\"3\\\"}\"", "{a:1 b:2.0 c:\"3\"}");
      DoTest ("parse \"{a:1 b:2.0 c:\\\"3\\\"}\"", "{a:1 b:2.0 c:\"3\"}");
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

      //Empty string at the end of a line.
      //\r\n for newlines.
      DoTest ("#csv parse \"a,b,c\\r\\nA,B,\\r\\n\"", "{a:\"A\" b:\"B\" c:\"\"}");

      //Empty string at the beginning of a line.
      DoTest ("#csv parse \"a,b,c\\r\\n,B,C\\r\\n\"", "{a:\"\" b:\"B\" c:\"C\"}");
    }

    [Test]
    public void TestParseJSON ()
    {
      //JSON parsing test cases are from:
      //http://code.google.com/p/json-ply/source/browse/trunk/jsonply_test.py

      //Empty array
      DoTest ("#json parse \"[]\"", "{}");
      //Array of strings
      DoTest ("#json parse \"[\\\"a\\\",\\\"b\\\",\\\"c\\\"]\"", "{:\"a\" :\"b\" :\"c\"}");
      //Array of numbers
      DoTest ("#json parse \"[1, 2, 3.4e5]\"", "{:1.0 :2.0 :340000.0}");
      //Array of arrays
      DoTest ("#json parse \"[[\\\"a\\\",\\\"b\\\",\\\"c\\\"]]\"", "{:{:\"a\" :\"b\" :\"c\"}}");
      //Array of dicts
      DoTest ("#json parse \"[{\\\"a\\\":\\\"b\\\"},{\\\"c\\\":\\\"d\\\"}]\"", "{:{a:\"b\"} :{c:\"d\"}}");
      //Array of mixed itmems
      DoTest ("#json parse \"[1, true, {\\\"a\\\": \\\"b\\\"}, [\\\"c\\\"]]\"", "{:1.0 :true :{a:\"b\"} :{:\"c\"}}");

      //Empty dict
      DoTest ("#json parse \"{}\"", "{}");
      //Dict of strings
      DoTest ("#json parse \"{\\\"a\\\":\\\"b\\\" \\\"c\\\":\\\"d\\\"}\"", "{a:\"b\" c:\"d\"}");
      //Dict of numbers
      DoTest ("#json parse \"{\\\"a\\\":1.0 \\\"b\\\":2.3}\"", "{a:1.0 b:2.3}");
      //Dict of arrays
      DoTest ("#json parse \"{\\\"a\\\": [\\\"b\\\", \\\"c\\\"], \\\"d\\\":[1.0, 2.3]}\"", "{a:{:\"b\" :\"c\"} d:{:1.0 :2.3}}");
      //Dict of dicts
      DoTest ("#json parse \"{\\\"a\\\": {\\\"b\\\":\\\"c\\\"} \\\"d\\\": {\\\"e\\\":\\\"f\\\"}}\"", "{a:{b:\"c\"} d:{e:\"f\"}}");
      //Dict of mixed items
      DoTest ("#json parse \"{\\\"a\\\": true, \\\"b\\\": [1.0, 2.3], \\\"c\\\": {\\\"d\\\":null}}\"", "{a:true b:{:1.0 :2.3} c:{d:{}}}");
    }

    [Test]
    public void TestParseXML ()
    {
      //Single empty element
      DoTest ("#xml parse \"<a></a>\"", "{a:{:\"\"}}");
      //Empty element single tag
      DoTest ("#xml parse \"<a/>\"", "{a:{:\"\"}}");
      //Single element with content
      DoTest ("#xml parse \"<a>x</a>\"", "{a:{:\"x\"}}");
      //Single element with attribute
      DoTest ("#xml parse \"<a b=\\\"x\\\"></a>\"", "{a:{b:\"x\" :\"\"}}");
      //Multiple attributes in an element
      DoTest ("#xml parse \"<a b=\\\"x\\\" c=\\\"y\\\" d=\\\"z\\\">w</a>\"", "{a:{b:\"x\" c:\"y\" d:\"z\" :\"w\"}}");
      //Multiple elements in a document
      DoTest ("#xml parse \"<a>x</a><b>y</b><c>z</c>\"", "{a:{:\"x\"} b:{:\"y\"} c:{:\"z\"}}");
      //Multiple elements with different content types.
      DoTest ("#xml parse \"<a>x</a><b></b><c/>\"", "{a:{:\"x\"} b:{:\"\"} c:{:\"\"}}");
      //Nested elements
      DoTest ("#xml parse \"<a><b>x</b><c>y</c><d>z</d></a>\"", "{a:{:{b:{:\"x\"} c:{:\"y\"} d:{:\"z\"}}}}");
    }

    [Test]
    public void TestParseXML1 ()
    {
      //Single empty element
      DoTest ("#xml parse \"<a></a>\"", "{a:{:\"\"}}");
      //Empty element single tag
      DoTest ("#xml parse \"<a/>\"", "{a:{:\"\"}}");
      //Single element with content
      DoTest ("#xml parse \"<a>x</a>\"", "{a:{:\"x\"}}");
      //Multiple elements in a document
      DoTest ("#xml parse \"<a>x</a><b>y</b><c>z</c>\"", "{a:{:\"x\"} b:{:\"y\"} c:{:\"z\"}}");
      //Multiple elements with different content types.
      DoTest ("#xml parse \"<a>x</a><b></b><c/>\"", "{a:{:\"x\"} b:{:\"\"} c:{:\"\"}}");
      //Single nested multiple elements
      DoTest ("#xml parse \"<a><b>x</b><c>y</c><d>z</d></a>\"", "{a:{:{b:{:\"x\"} c:{:\"y\"} d:{:\"z\"}}}}");
      //Multiple nested single elements
      DoTest ("#xml parse \"<a><b><c>x</c></b></a>\"", "{a:{:{b:{:{c:{:\"x\"}}}}}}");
      //Multiple nested multiple elements
      DoTest ("#xml parse \"<a><b><c>x</c><d>y</d></b><e/></a>\"", "{a:{:{b:{:{c:{:\"x\"} d:{:\"y\"}}} e:{:\"\"}}}}");
    }

    [Test]
    public void TestParseXML2 ()
    {
      //Single empty element
      DoTest ("#xml parse \"<a f=\\\"i\\\"></a>\"", "{a:{f:\"i\" :\"\"}}");
      //Empty element single tag
      DoTest ("#xml parse \"<a f=\\\"i\\\"/>\"", "{a:{f:\"i\" :\"\"}}");
      //Single element with content
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a>\"", "{a:{f:\"i\" :\"x\"}}");
      //Multiple elements in a document
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a><b g=\\\"j\\\">y</b><c h=\\\"k\\\">z</c>\"", "{a:{f:\"i\" :\"x\"} b:{g:\"j\" :\"y\"} c:{h:\"k\" :\"z\"}}");
      //Multiple elements with different content types.
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a><b g=\\\"j\\\"></b><c h=\\\"k\\\"/>\"", "{a:{f:\"i\" :\"x\"} b:{g:\"j\" :\"\"} c:{h:\"k\" :\"\"}}");
      //Single nested multiple elements
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b g=\\\"j\\\">x</b><c>y</c><d h=\\\"k\\\">z</d></a>\"", "{a:{f:\"i\" :{b:{g:\"j\" :\"x\"} c:{:\"y\"} d:{h:\"k\" :\"z\"}}}}");
      //Multiple nested single elements
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b g=\\\"j\\\"><c h=\\\"k\\\">x</c></b></a>\"", "{a:{f:\"i\" :{b:{g:\"j\" :{c:{h:\"k\" :\"x\"}}}}}}");
      //Multiple nested multiple elements
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b><c g=\\\"j\\\">x</c><d>y</d></b><e h=\\\"k\\\"/></a>\"", "{a:{f:\"i\" :{b:{:{c:{g:\"j\" :\"x\"} d:{:\"y\"}}} e:{h:\"k\" :\"\"}}}}");
    }

    [Test]
    public void TestFormatPretty ()
    {
      DoTest ("#pretty format {}", "\"{}\"");

      DoTest ("#pretty format {a:1.0 b:2.0 c:3.0}", "\"{\\n  a:1.0\\n  b:2.0\\n  c:3.0\\n}\"");

      DoTest ("#pretty format {:{a:1.0 b:2.0 c:3.0}}", "\"{\\n  :{\\n    a:1.0\\n    b:2.0\\n    c:3.0\\n  }\\n}\"");

      DoTest ("#pretty format {:{a:1.0 1.0 1.0 b:2.0 2.0 2.0 c:3.0 3.0 3.0}}", "\"{\\n  :{\\n    a:1.0 1.0 1.0\\n    b:2.0 2.0 2.0\\n    c:3.0 3.0 3.0\\n  }\\n}\"");
      //[
      //  T | S|   a b
      //  0l #x   1l "2"
      //  1l #x  10l "20"
      //  2l #x 100l "200"
      //]
      DoTest ("#pretty format [E|S|a b 0 #x 1 \"2\" 1 #x 10 \"20\" 2 #x 100 \"200\"]", "\"[\\n   E|S |  a b\\n   0 #x   1 \\\"2\\\"\\n   1 #x  10 \\\"20\\\"\\n   2 #x 100 \\\"200\\\"\\n]\"");
    }

    [Test]
    public void TestFormatPretty1 ()
    {
      DoTest ("#pretty format {u:[S|x #a 0]}", "\"{\\n  u:[\\n    S | x\\n    #a  0\\n  ]\\n}\"");
    }

    [Test]
    public void TestFormatText ()
    {
      DoTest ("#text format \"line one\" \"line two\" \"line three\"", "\"line one\\nline two\\nline three\\n\"");
    }

    [Test]
    public void TestFormatLog ()
    {
      //The log format option omits the header and uses spaces instead of commas for the delimiter.
      DoTest ("#log format [a b c 0 \"x y z\" #foo 1 \"u v w\" #bar 2 \"s t u\" #baz]", 
              "\"0 \\\"x y z\\\" #foo\\n1 \\\"u v w\\\" #bar\\n2 \\\"s t u\\\" #baz\\n\"");
    }

    //I was going to make binary a variant of format.
    //But binary will return a byte vector and format returns a string vector.
    //I do not want the output from an operator to vary based on the arguments.
    //(varying based on the TYPE of the arguments is ok).
    //So use a different operator name to format binary data.
    [Test]
    public void TestBinaryFixed ()
    {
      DoTest ("parse binary -1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000", "-1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000");
      DoTest ("parse binary -1000000000.0 -2000000.0 -3000.0 -4.0 0.0 5.0 6000.0 7000000.0 8000000000.0", "-1000000000.0 -2000000.0 -3000.0 -4.0 0.0 5.0 6000.0 7000000.0 8000000000.0");
      DoTest ("parse binary -1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000m", "-1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000m");
      DoTest ("parse binary -12345678.9 -234567.89 -3.456 -0.4 0 5 6.789 789.1234 8912.345678m", "-12345678.9 -234567.89 -3.456 -0.4 0 5 6.789 789.1234 8912.345678m");
      DoTest ("parse binary true false true", "true false true");
      DoTest ("parse binary \\x00 \\x01 \\x02", "\\x00 \\x01 \\x02");
      DoTest ("parse binary ++ ++ ++", "++ ++ ++");
      DoTest ("parse binary 2015.05.25 2015.05.26 2015.05.27", "2015.05.25 2015.05.26 2015.05.27");
    }

    //Vectors of variable length types like string and reference.
    [Test]
    public void TestBinaryString ()
    {
      DoTest ("parse binary \"a\" \"ba\" \"cba\" \"dcba\"", "\"a\" \"ba\" \"cba\" \"dcba\"");
      //test with repeated values
      DoTest ("parse binary \"x\" \"yy\" \"zzz\" \"yy\" \"x\" \"yy\" \"zzz\"", "\"x\" \"yy\" \"zzz\" \"yy\" \"x\" \"yy\" \"zzz\"");
    }

    [Test]
    public void TestBinaryReference ()
    {
      DoTest ("parse binary {<-$R * $R}", "{<-$R * $R}");
      DoTest ("parse binary {<-$R.x * $R.y}", "{<-$R.x * $R.y}");
      DoTest ("parse binary {<-$L lib.f $R}", "{<-$L lib.f $R}");
    }

    [Test]
    public void TestBinarySymbol ()
    {
      //With simple fixed types only.
      DoTest ("parse binary #1,2.0,true", "#1,2.0,true");
      //With fixed types including decimal
      DoTest ("parse binary #1,2.0,3m,true", "#1,2.0,3m,true");
      //With more than one value
      DoTest ("parse binary #1,2.0,3m #4,5.0,6m #7,8.0,9m", "#1,2.0,3m #4,5.0,6m #7,8.0,9m");
      //With repeating values
      DoTest ("parse binary #1,2.0,3m #4,5.0,6m #1,2.0,3m", "#1,2.0,3m #4,5.0,6m #1,2.0,3m");
      //With strings (variable length)
      DoTest ("parse binary #1,2.0,3m,true,abcdef", "#1,2.0,3m,true,abcdef");
      //Mixing strings and repeated values.
      //DoTest ("parse binary #abc #de,f #abc", "#abc #de,f #abc");
    }

    [Test]
    public void TestBinaryContainers ()
    {
      //block as expression holder.
      DoTest ("parse binary {<-1 + 2.0}", "{<-1 + 2.0}");
      //block with mixed variables.
      DoTest ("parse binary {a:1 2 3 b:4.0 5.0 6.0 c:7 8 9m d:true false e:\"this\" \"a\" \"string\" f:#x,y,z #u,v,w #r,s,t}", "{a:1 2 3 b:4.0 5.0 6.0 c:7 8 9m d:true false e:\"this\" \"a\" \"string\" f:#x,y,z #u,v,w #r,s,t}");
      //block with complex expression.
      DoTest ("parse binary {<-(1 - 2 * 4) - 3}", "{<-(1 - 2 * 4) - 3}");
      //nested blocks
      DoTest ("parse binary {l:{a:0 b:1 c:2} d:{d:3.0 e:4.0 f:5.0} m:{g:6m h:7m i:8m}}", "{l:{a:0 b:1 c:2} d:{d:3.0 e:4.0 f:5.0} m:{g:6m h:7m i:8m}}");
    }

    [Test]
    public void TestBinaryCube ()
    {
      //cubes
      DoTest ("parse binary [S|l d m #a 10 -- -- #a 11 21.0 -- #a 12 22.0 32m]", "[S|l d m #a 10 -- -- #a 11 21.0 -- #a 12 22.0 32m]");
      DoTest ("parse binary [E|S|l d m 0 #a 10 -- -- 1 #a 11 21.0 -- 2l #a 12 22.0 32m]", "[E|S|l d m 0 #a 10 -- -- 1 #a 11 21.0 -- 2 #a 12 22.0 32m]");
    }

    [Test]
    [Ignore]
    public void TestExec ()
    {
      DoTest ("{:try {<-exec \"rm foo/bar\"} :try {<-exec \"rmdir foo\"} <-0}", "0");
      DoTest ("try {<-exec \"ls foo\"}", "{status:1 data:\"Non-zero exit status\"}");
      DoTest ("{:exec \"mkdir foo\" <-count exec \"ls foo\"}", "0");
      DoTest ("{:exec \"touch foo/bar\" <-exec \"ls foo\"}", "\"bar\"");
      DoTest ("{:exec \"rm foo/bar\" :exec \"rmdir foo\" <-0}", "0");
    }
  }
}

using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  /// <summary>
  /// This test is similar in structure to RCEvalTest.
  /// But the purpose of this test is to check every single overload
  /// of every single operator, not to test the infrastructure of
  /// evaluation proper.
  /// </summary>
  [TestFixture]
  public class CoreTest1 : CoreTest
  {
    // Plus
    [Test]
    public void TestPlusXX ()
    {
      DoTest ("\\x01 + \\x02", "\\x03");
    }
    [Test]
    public void TestPlusXL ()
    {
      DoTest ("\\x01 + 2", "3");
    }
    [Test]
    public void TestPlusXD ()
    {
      DoTest ("\\x01 + 2.0", "3.0");
    }
    [Test]
    public void TestPlusXM ()
    {
      DoTest ("\\x01 + 2m", "3m");
    }
    [Test]
    public void TestPlusDD ()
    {
      DoTest ("1.0+2.0", "3.0");
    }
    [Test]
    public void TestPlusDL ()
    {
      DoTest ("1.0+2.0", "3.0");
    }
    [Test]
    // [ExpectedException(typeof(RCException))]
    public void TestPlusDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0+2m", "3.0");
      });
    }
    [Test]
    public void TestPlusDX ()
    {
      DoTest ("1.0 + \\x02", "3.0");
    }
    [Test]
    public void TestPlusLL ()
    {
      DoTest ("1+2", "3");
    }
    [Test]
    public void TestPlusLD ()
    {
      DoTest ("1+2.0", "3.0");
    }
    [Test]
    public void TestPlusLM ()
    {
      DoTest ("1+2m", "3m");
    }
    [Test]
    public void TestPlusLX ()
    {
      DoTest ("1 + \\x02", "3");
    }
    [Test]
    public void TestPlusMM ()
    {
      DoTest ("1m+2m", "3m");
    }
    [Test]
    public void TestPlusMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m+2.0", "3m");
      });
    }
    [Test]
    public void TestPlusML ()
    {
      DoTest ("1m+2", "3m");
    }
    [Test]
    public void TestPlusMX ()
    {
      DoTest ("1m + \\x02", "3m");
    }
    [Test]
    public void TestPlusSS ()
    {
      DoTest ("\"x\"+\"y\"", "\"xy\"");
    }
    [Test]
    public void TestPlusYY ()
    {
      DoTest ("#x+#y", "#x,y");
    }
    [Test]
    public void TestPlusTT ()
    {
      DoTest ("2015.05.31 + 1.00:00:00.000000", "2015.06.01 00:00:00.000000");
    }
    [Test]
    public void TestPlusTT1 ()
    {
      DoTest ("1.00:00:00.000000 + 2015.05.31", "2015.06.01 00:00:00.000000");
    }
    [Test]
    public void TestPlusTT2 ()
    {
      DoTest ("1.00:00:00.000000 + 2.00:00:00.000000", "3.00:00:00.000000");
    }
    [Test]
    public void TestPlusYY1 ()
    {
      DoTest ("#+#y", "#y");
    }
    [Test]
    public void TestPlusD0 ()
    {
      DoTest ("+ ~d", "~d");
    }
    [Test]
    public void TestPlusD1 ()
    {
      DoTest ("+ 1.0 2.0 3.0", "1.0 3.0 6.0");
    }
    [Test]
    public void TestPlusL0 ()
    {
      DoTest ("+ ~l", "~l");
    }
    [Test]
    public void TestPlusL1 ()
    {
      DoTest ("+ 1 2 3", "1 3 6");
    }
    [Test]
    public void TestPlusM0 ()
    {
      DoTest ("+ ~m", "~m");
    }
    [Test]
    public void TestPlusM1 ()
    {
      DoTest ("+ 1 2 3m", "1 3 6m");
    }
    [Test]
    public void TestPlusX0 ()
    {
      DoTest ("+ ~x", "~x");
    }
    [Test]
    public void TestPlusX1 ()
    {
      DoTest ("+ \\x01 \\x02 \\x03", "\\x01 \\x03 \\x06");
    }
    [Test]
    public void TestPlusY0 ()
    {
      DoTest ("+ #1 #2 #3", "#1,2,3");
    }

    // Minus
    [Test]
    public void TestMinusXX ()
    {
      DoTest ("\\x03 - \\x02", "\\x01");
    }
    [Test]
    public void TestMinusXXWrap ()
    {
      DoTest ("\\x01 - \\x02", "\\xFF");
    }
    [Test]
    public void TestMinusXL ()
    {
      DoTest ("\\x03 - 2", "1");
    }
    [Test]
    public void TestMinusXD ()
    {
      DoTest ("\\x03 - 2.0", "1.0");
    }
    [Test]
    public void TestMinusXM ()
    {
      DoTest ("\\x03 - 2m", "1m");
    }
    [Test]
    public void TestMinusDD ()
    {
      DoTest ("1.0-2.0", "-1.0");
    }
    [Test]
    public void TestMinusDL ()
    {
      DoTest ("1.0-2", "-1.0");
    }
    [Test]
    public void TestMinusDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0-2m", "-1.0");
      });
    }
    [Test]
    public void TestMinusDX ()
    {
      DoTest ("1.0-\\x02", "-1.0");
    }
    [Test]
    public void TestMinusLL ()
    {
      DoTest ("1-2", "-1");
    }
    [Test]
    public void TestMinusLD ()
    {
      DoTest ("1-2.0", "-1.0");
    }
    [Test]
    public void TestMinusLM ()
    {
      DoTest ("1-2m", "-1m");
    }
    [Test]
    public void TestMinusLX ()
    {
      DoTest ("1-\\x02", "-1");
    }
    [Test]
    public void TestMinusMM ()
    {
      DoTest ("1m-2m", "-1m");
    }
    [Test]
    public void TestMinusMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m-2d", "-1m");
      });
    }
    [Test]
    public void TestMinusML ()
    {
      DoTest ("1m-2", "-1m");
    }
    [Test]
    public void TestMinusMX ()
    {
      DoTest ("1m-\\x02", "-1m");
    }
    [Test]
    public void TestMinusTT0 ()
    {
      DoTest ("2015.05.31 - 1.00:00:00.000000", "2015.05.30 00:00:00.000000");
    }
    [Test]
    public void TestMinusTT1 ()
    {
      DoTest ("2015.05.31 - 2015.05.10", "21.00:00:00.000000");
    }
    [Test]
    public void TestMinusTT2 ()
    {
      DoTest ("1.00:00:00.000000 - 2.00:00:00.000000", "-1.00:00:00.000000");
    }

    [Test]
    public void TestMinusL ()
    {
      DoTest ("- 1 3 5 8 12", "1 2 2 3 4");
    }
    [Test]
    public void TestMinusD ()
    {
      DoTest ("- 1.0 3.0 5.0 8.0 12.0", "1.0 2.0 2.0 3.0 4.0");
    }
    [Test]
    public void TestMinusM ()
    {
      DoTest ("- 1 3 5 8 12m", "1 2 2 3 4m");
    }
    [Test]
    public void TestMinusX ()
    {
      DoTest ("- \\x01 \\x03 \\x05 \\x08 \\x0c", "\\x01 \\x02 \\x02 \\x03 \\x04");
    }

    // Multiply
    [Test]
    public void TestMultiplyXX ()
    {
      DoTest ("\\x01*\\x02", "\\x02");
    }
    [Test]
    public void TestMultiplyXL ()
    {
      DoTest ("\\x01*2", "2");
    }
    [Test]
    public void TestMultiplyXD ()
    {
      DoTest ("\\x01*2.0", "2.0");
    }
    [Test]
    public void TestMultiplyXM ()
    {
      DoTest ("\\x01*2m", "2m");
    }
    [Test]
    public void TestMultiplyDD ()
    {
      DoTest ("1.0*2.0", "2.0");
    }
    [Test]
    public void TestMultiplyDL ()
    {
      DoTest ("1.0*2", "2.0");
    }
    [Test]
    public void TestMultiplyDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0*2m", "2.0");
      });
    }
    [Test]
    public void TestMultiplyDX ()
    {
      DoTest ("1.0*\\x02", "2.0");
    }
    [Test]
    public void TestMultiplyLL ()
    {
      DoTest ("1*2", "2");
    }
    [Test]
    public void TestMultiplyLD ()
    {
      DoTest ("1*2.0", "2.0");
    }
    [Test]
    public void TestMultiplyLM ()
    {
      DoTest ("1*2m", "2m");
    }
    [Test]
    public void TestMultiplyLX ()
    {
      DoTest ("1*\\x02", "2");
    }
    [Test]
    public void TestMultiplyMM ()
    {
      DoTest ("1m*2m", "2m");
    }
    [Test]
    public void TestMultiplyMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m*2.0", "2.0");
      });
    }
    [Test]
    public void TestMultiplyML ()
    {
      DoTest ("1m*2", "2m");
    }
    [Test]
    public void TestMultiplyMX ()
    {
      DoTest ("1m*\\x02", "2m");
    }

    // Divide
    [Test]
    public void TestDivideXX ()
    {
      DoTest ("\\x04/\\x02", "\\x02");
    }
    [Test]
    public void TestDivideXL ()
    {
      DoTest ("\\x04/2", "2");
    }
    [Test]
    public void TestDivideXD ()
    {
      DoTest ("\\x04/2.0", "2.0");
    }
    [Test]
    public void TestDivideXM ()
    {
      DoTest ("\\x04/2m", "2m");
    }
    [Test]
    public void TestDivideDD ()
    {
      DoTest ("4.0/2.0", "2.0");
    }
    [Test]
    public void TestDivideDL ()
    {
      DoTest ("4.0/2", "2.0");
    }
    [Test]
    public void TestDivideDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("4.0/2m", "2.0");
      });
    }
    [Test]
    public void TestDivideDX ()
    {
      DoTest ("4.0/\\x02", "2.0");
    }
    [Test]
    public void TestDivideLL ()
    {
      DoTest ("4/2", "2");
    }
    [Test]
    public void TestDivideLD ()
    {
      DoTest ("4/2.0", "2.0");
    }
    [Test]
    public void TestDivideLM ()
    {
      DoTest ("4/2m", "2m");
    }
    [Test]
    public void TestDivideLX ()
    {
      DoTest ("4/\\x02", "2");
    }
    [Test]
    public void TestDivideMM ()
    {
      DoTest ("4m/2m", "2m");
    }
    [Test]
    public void TestDivideMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("4m/2.0", "2.0");
      });
    }
    [Test]
    public void TestDivideML ()
    {
      DoTest ("4m/2", "2m");
    }
    [Test]
    public void TestDivideMX ()
    {
      DoTest ("4m/\\x02", "2m");
    }

    // Modulo division
    [Test]
    public void TestModulo ()
    {
      DoTest ("10 % 3", "1");
    }

    // Logic Operators
    [Test]
    public void TestAndXX ()
    {
      DoTest ("\\xF0 and \\xAA", "\\xA0");
    }
    [Test]
    public void TestAndBB ()
    {
      DoTest ("true true false false and true false true false", "true false false false");
    }
    [Test]
    public void TestOrBB ()
    {
      DoTest ("true true false false or true false true false", "true true true false");
    }
    [Test]
    public void TestOrXX ()
    {
      DoTest ("\\xF0 or \\xAA", "\\xFA");
    }
    [Test]
    public void TestNotBB ()
    {
      DoTest ("not true false", "false true");
    }
    [Test]
    public void TestNotX ()
    {
      DoTest ("not \\xF0", "\\x0F");
    }

    // Greater Than
    [Test]
    public void TestGtXX ()
    {
      DoTest ("\\x01>\\x02", "false");
    }
    [Test]
    public void TestGtXL ()
    {
      DoTest ("\\x01>2", "false");
    }
    [Test]
    public void TestGtXD ()
    {
      DoTest ("\\x01>2.0", "false");
    }
    [Test]
    public void TestGtXM ()
    {
      DoTest ("\\x01>2m", "false");
    }
    [Test]
    public void TestGtDD ()
    {
      DoTest ("1.0>2.0", "false");
    }
    [Test]
    public void TestGtDL ()
    {
      DoTest ("1.0>2", "false");
    }
    // .Net doesn't allow comparing decimal and double.
    // I think this is too conservative, but I will tow the line for now.
    [Test]
    public void TestGtDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0>2m", "false");
      });
    }
    [Test]
    public void TestGtDX ()
    {
      DoTest ("1.0>\\x02", "false");
    }
    [Test]
    public void TestGtLL ()
    {
      DoTest ("1>2", "false");
    }
    [Test]
    public void TestGtLD ()
    {
      DoTest ("1>2.0", "false");
    }
    [Test]
    public void TestGtLM ()
    {
      DoTest ("1>2m", "false");
    }
    [Test]
    public void TestGtLX ()
    {
      DoTest ("1>\\x02", "false");
    }
    [Test]
    public void TestGtMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m>2.0", "false");
      });
    }
    [Test]
    public void TestGtML ()
    {
      DoTest ("1m>2", "false");
    }
    [Test]
    public void TestGtMM ()
    {
      DoTest ("1m>2m", "false");
    }
    [Test]
    public void TestGtMX ()
    {
      DoTest ("1m>\\x02", "false");
    }
    [Test]
    public void TestGtTT ()
    {
      DoTest ("2015.05.31 > 2015.06.01", "false");
    }
    [Test]
    public void TestGtTT1 ()
    {
      DoTest ("2015.05.31 08:00 > 2015.05.31 08:01", "false");
    }
    [Test]
    public void TestGtTT2 ()
    {
      DoTest ("1.00:00:00.000000 > 2.00:00:00.000000", "false");
    }
    // Greater Than or Equal To
    [Test]
    public void TestGteXX ()
    {
      DoTest ("\\x01>=\\x02", "false");
    }
    [Test]
    public void TestGteXL ()
    {
      DoTest ("\\x01>=2", "false");
    }
    [Test]
    public void TestGteXD ()
    {
      DoTest ("\\x01>=2.0", "false");
    }
    [Test]
    public void TestGteXM ()
    {
      DoTest ("\\x01>=2m", "false");
    }
    [Test]
    public void TestGteDD ()
    {
      DoTest ("1.0>=2.0", "false");
    }
    [Test]
    public void TestGteDL ()
    {
      DoTest ("1.0>=2", "false");
    }
    // .Net doesn't allow comparing decimal and double.
    // I think this is too conservative, but I will tow the line for now.
    [Test]
    public void TestGteDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0>=2m", "false");
      });
    }
    [Test]
    public void TestGteDX ()
    {
      DoTest ("1.0>=\\x02", "false");
    }
    [Test]
    public void TestGteLL ()
    {
      DoTest ("1>=2", "false");
    }
    [Test]
    public void TestGteLD ()
    {
      DoTest ("1>=2.0", "false");
    }
    [Test]
    public void TestGteLM ()
    {
      DoTest ("1>=2m", "false");
    }
    [Test]
    public void TestGteLX ()
    {
      DoTest ("1>=\\x02", "false");
    }
    [Test]
    public void TestGteMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m>=2.0", "false");
      });
    }
    [Test]
    public void TestGteML ()
    {
      DoTest ("1m>=2", "false");
    }
    [Test]
    public void TestGteMM ()
    {
      DoTest ("1m>=2m", "false");
    }
    [Test]
    public void TestGteMX ()
    {
      DoTest ("1m>=\\x02", "false");
    }
    [Test]
    public void TestGteTT ()
    {
      DoTest ("2015.05.31 >= 2015.06.01", "false");
    }
    [Test]
    public void TestGteTT1 ()
    {
      DoTest ("2015.05.31 08:00 >= 2015.05.31 08:00", "true");
    }
    [Test]
    public void TestGteTT2 ()
    {
      DoTest ("1.00:00:00.000000 >= 2.00:00:00.000000", "false");
    }

    // Less Than
    [Test]
    public void TestLtXX ()
    {
      DoTest ("\\x01<\\x02", "true");
    }
    [Test]
    public void TestLTXL ()
    {
      DoTest ("\\x01<2", "true");
    }
    [Test]
    public void TestLtXD ()
    {
      DoTest ("\\x01<2.0", "true");
    }
    [Test]
    public void TestLtXM ()
    {
      DoTest ("\\x01<2m", "true");
    }
    [Test]
    public void TestLtDD ()
    {
      DoTest ("1.0<2.0", "true");
    }
    [Test]
    public void TestLtDL ()
    {
      DoTest ("1.0<2", "true");
    }
    [Test]
    public void TestLtDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0<2m", "true");
      });
    }
    [Test]
    public void TestLtDX ()
    {
      DoTest ("1.0<\\x02", "true");
    }
    [Test]
    public void TestLtLL ()
    {
      DoTest ("1<2", "true");
    }
    [Test]
    public void TestLtLD ()
    {
      DoTest ("1<2.0", "true");
    }
    [Test]
    public void TestLtLM ()
    {
      DoTest ("1<2m", "true");
    }
    [Test]
    public void TestLtLX ()
    {
      DoTest ("1<\\x02", "true");
    }
    [Test]
    public void TestLtMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m<2.0", "true");
      });
    }
    [Test]
    public void TestLtML ()
    {
      DoTest ("1m<2", "true");
    }
    [Test]
    public void TestLtMM ()
    {
      DoTest ("1m<2m", "true");
    }
    [Test]
    public void TestLtMX ()
    {
      DoTest ("1m<\\x02", "true");
    }
    [Test]
    public void TestLtTT ()
    {
      DoTest ("2015.05.31 < 2015.06.01", "true");
    }
    [Test]
    public void TestLtTT1 ()
    {
      DoTest ("2015.05.31 08:00 < 2015.05.31 08:01", "true");
    }
    [Test]
    public void TestLtTT2 ()
    {
      DoTest ("1.00:00:00.000000 < 2.00:00:00.000000", "true");
    }

    // Less Than or Equal To
    [Test]
    public void TestLteXX ()
    {
      DoTest ("\\x01<=\\x02", "true");
    }
    [Test]
    public void TestLteXL ()
    {
      DoTest ("\\x01<=2", "true");
    }
    [Test]
    public void TestLteXD ()
    {
      DoTest ("\\x01<=2.0", "true");
    }
    [Test]
    public void TestLteXM ()
    {
      DoTest ("\\x01<=2m", "true");
    }
    [Test]
    public void TestLteDD ()
    {
      DoTest ("1.0<=2.0", "true");
    }
    [Test]
    public void TestLteDL ()
    {
      DoTest ("1.0<=1", "true");
    }
    // .Net doesn't allow comparing decimal and double.
    // I think this is too conservative, but I will tow the line for now.
    [Test]
    public void TestLteDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0<=2m", "true");
      });
    }
    [Test]
    public void TestLteDX ()
    {
      DoTest ("1.0<=\\x02", "true");
    }
    [Test]
    public void TestLteLL ()
    {
      DoTest ("1<=1", "true");
    }
    [Test]
    public void TestLteLD ()
    {
      DoTest ("1<=2.0", "true");
    }
    [Test]
    public void TestLteLM ()
    {
      DoTest ("1<=1m", "true");
    }
    [Test]
    public void TestLteLX ()
    {
      DoTest ("1<=\\x02", "true");
    }
    [Test]
    public void TestLteMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m<=2.0", "true");
      });
    }
    [Test]
    public void TestLteML ()
    {
      DoTest ("1m<=1", "true");
    }
    [Test]
    public void TestLteMM ()
    {
      DoTest ("1m<=2m", "true");
    }
    [Test]
    public void TestLteMX ()
    {
      DoTest ("1m<=\\x02", "true");
    }
    [Test]
    public void TestLteTT ()
    {
      DoTest ("2015.05.31 <= 2015.06.01", "true");
    }
    [Test]
    public void TestLteTT1 ()
    {
      DoTest ("2015.05.31 08:00 <= 2015.05.31 08:00", "true");
    }
    [Test]
    public void TestLteTT2 ()
    {
      DoTest ("1.00:00:00.000000 <= 2.00:00:00.000000", "true");
    }

    // Vector Equals
    [Test]
    public void TestEqXX ()
    {
      DoTest ("\\x01==\\x02", "false");
    }
    [Test]
    public void TestEqXL ()
    {
      DoTest ("\\x01==2", "false");
    }
    [Test]
    public void TestEqXD ()
    {
      DoTest ("\\x01==2.0", "false");
    }
    [Test]
    public void TestEqXM ()
    {
      DoTest ("\\x01==2m", "false");
    }
    [Test]
    public void TestEqDD ()
    {
      DoTest ("1.0==2.0", "false");
    }
    [Test]
    public void TestEqDD1 ()
    {
      DoTest ("1.1==1.1", "true");
    }
    [Test]
    public void TestEqDD2 ()
    {
      DoTest ("NaN==NaN", "true");
    }
    [Test]
    public void TestEqDL ()
    {
      DoTest ("1.0==2", "false");
    }
    [Test]
    public void TestEqDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0==2m", "false");
      });
    }
    [Test]
    public void TestEqDX ()
    {
      DoTest ("1.0==\\x02", "false");
    }
    [Test]
    public void TestEqLL ()
    {
      DoTest ("1==2", "false");
    }
    [Test]
    public void TestEqLD ()
    {
      DoTest ("1==2.0", "false");
    }
    [Test]
    public void TestEqLM ()
    {
      DoTest ("1==2m", "false");
    }
    [Test]
    public void TestEqLX ()
    {
      DoTest ("1==\\x02", "false");
    }
    [Test]
    public void TestEqMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m==2.0", "false");
      });
    }
    [Test]
    public void TestEqML ()
    {
      DoTest ("1m==2", "false");
    }
    [Test]
    public void TestEqMM ()
    {
      DoTest ("1m==2m", "false");
    }
    [Test]
    public void TestEqMX ()
    {
      DoTest ("1m==\\x02", "false");
    }
    [Test]
    public void TestEqBB ()
    {
      DoTest ("true==false", "false");
    }
    [Test]
    public void TestEqYY ()
    {
      DoTest ("#x==#y", "false");
    }
    [Test]
    public void TestEqYY1 ()
    {
      DoTest ("#x==#x", "true");
    }
    [Test]
    public void TestEqYY2 ()
    {
      DoTest ("#==symbol \"\"", "true");
    }
    [Test]
    public void TestEqSS ()
    {
      DoTest ("\"x\"==\"x\"", "true");
    }
    [Test]
    public void TestEqTT ()
    {
      DoTest ("2018.05.04 == 2018.05.04", "true");
    }
    [Test]
    public void TestEqTTWithTime ()
    {
      DoTest ("2018.05.04 11:21 == 2018.05.04", "false");
    }
    [Test]
    public void TestEqWithBlock ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("0 == {}", "");
      });
    }

    // Vector Not Equals
    [Test]
    public void TestNeqXX ()
    {
      DoTest ("\\x01!=\\x02", "true");
    }
    [Test]
    public void TestNeqXL ()
    {
      DoTest ("\\x01!=2", "true");
    }
    [Test]
    public void TestNeqXD ()
    {
      DoTest ("\\x01!=2.0", "true");
    }
    [Test]
    public void TestNeqXM ()
    {
      DoTest ("\\x01!=2m", "true");
    }
    [Test]
    public void TestNeqDD ()
    {
      DoTest ("1.0!=2.0", "true");
    }
    [Test]
    public void TestNeqDL ()
    {
      DoTest ("1.0!=2", "true");
    }
    [Test]
    public void TestNeqDM ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1.0!=2m", "true");
      });
    }
    [Test]
    public void TestNeqDX ()
    {
      DoTest ("1.0!=\\x02", "true");
    }
    [Test]
    public void TestNeqLL ()
    {
      DoTest ("1!=2", "true");
    }
    [Test]
    public void TestNeqLD ()
    {
      DoTest ("1!=2.0", "true");
    }
    [Test]
    public void TestNeqLM ()
    {
      DoTest ("1!=2m", "true");
    }
    [Test]
    public void TestNeqLX ()
    {
      DoTest ("1!=\\x02", "true");
    }
    [Test]
    public void TestNeqMD ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("1m!=2.0", "true");
      });
    }
    [Test]
    public void TestNeqML ()
    {
      DoTest ("1m!=2", "true");
    }
    [Test]
    public void TestNeqMM ()
    {
      DoTest ("1m!=2m", "true");
    }
    [Test]
    public void TestNeqMX ()
    {
      DoTest ("1m!=\\x02", "true");
    }
    [Test]
    public void TestNeqBB ()
    {
      DoTest ("true!=false", "true");
    }
    [Test]
    public void TestNeqYY ()
    {
      DoTest ("#x!=#y", "true");
    }
    [Test]
    public void TestNeqYY1 ()
    {
      DoTest ("#x!=#x", "false");
    }
    [Test]
    public void TestNeqSS ()
    {
      DoTest ("\"x\"!=\"x\"", "false");
    }

    [Test]
    public void TestEqualsYY ()
    {
      DoTest ("#x = #x", "true");
    }
    [Test]
    public void TestEqualsYY1 ()
    {
      DoTest ("#x #x = #x", "false");
    }
    [Test]
    public void TestEqualsDD ()
    {
      DoTest ("NaN 1.0 2.34 = NaN 1.0 2.34", "true");
    }

    // Type coercion for vector types - we are not trying to round trip here.
    // Also not to be used for parsing and serializing object either.
    // What about coercion for blocks? {}
    [Test]
    public void TestStringX ()
    {
      DoTest ("string \\xF0 \\xE1 \\xD2 \\xC3", "\"F0\" \"E1\" \"D2\" \"C3\"");
    }
    [Test]
    public void TestStringD ()
    {
      DoTest ("string 1.0", "\"1\"");
    }
    [Test]
    public void TestStringL ()
    {
      DoTest ("string 1", "\"1\"");
    }
    [Test]
    public void TestStringM ()
    {
      DoTest ("string 1m", "\"1\"");
    }
    [Test]
    public void TestStringB ()
    {
      DoTest ("string true", "\"true\"");
    }
    [Test]
    public void TestStringS0 ()
    {
      DoTest ("string \"string\"", "\"string\"");
    }
    [Test]
    public void TestStringS1 ()
    {
      DoTest ("{u:[E|S|x 0 #a \"string\"] <-string $u.x}", "[E|S|x 0 #a \"string\"]");
    }
    [Test]
    public void TestStringY ()
    {
      DoTest ("string #this,is,a,sym", "\"#this,is,a,sym\"");
    }
    [Test]
    public void TestStringK ()
    {
      DoTest ("string {x:\"a\" y:\"b\" z:\"c\"}", "\"a\" \"b\" \"c\"");
    }
    [Test]
    public void TestStringT0 ()
    {
      DoTest ("string 2015.05.26", "\"2015.05.26\"");
    }
    [Test]
    public void TestStringT1 ()
    {
      DoTest ("string 08:26", "\"08:26:00\"");
    }
    [Test]
    public void TestStringT2 ()
    {
      DoTest ("string 2015.05.26 08:26", "\"2015.05.26 08:26:00\"");
    }
    [Test]
    public void TestStringT3 ()
    {
      DoTest ("string 2015.05.26 08:26:00.123456", "\"2015.05.26 08:26:00.123456\"");
    }

    [Test]
    public void TestSymbolX ()
    {
      DoTest ("symbol \\xFF", "#\\xFF");
    }
    [Test]
    public void TestSymbolD ()
    {
      DoTest ("symbol 1.0", "#1.0");
    }
    [Test]
    public void TestSymbolL ()
    {
      DoTest ("symbol 1", "#1");
    }
    [Test]
    public void TestSymbolM ()
    {
      DoTest ("symbol 1m", "#1m");
    }
    [Test]
    public void TestSymbolB ()
    {
      DoTest ("symbol true", "#true");
    }
    [Test]
    public void TestSymbolS ()
    {
      DoTest ("symbol \"string\"", "#string");
    }
    [Test]
    public void TestSymbolS0 ()
    {
      DoTest ("symbol \"#string\"", "#string");
    }
    [Test]
    public void TestSymbolS01 ()
    {
      DoTest ("(symbol \"#string\") switch {string:11}", "11");
    }
    [Test]
    public void TestSymbolS1 ()
    {
      DoTest ("symbol \"a\"", "#a");
    }
    [Test]
    public void TestSymbolS2 ()
    {
      DoTest ("symbol \" a \"", "#' a '");
    }
    [Test]
    public void TestSymbolS3 ()
    {
      DoTest ("symbol \"2a\"", "#'2a'");
    }
    [Test]
    public void TestSymbolS4 ()
    {
      DoTest ("symbol \"\"", "#");
    }
    [Test]
    public void TestSymbolS5 ()
    {
      DoTest ("symbol \"#a,0\"", "#a,0");
    }
    [Test]
    public void TestSymbolS6 ()
    {
      DoTest ("#a symbol \"\"", "#a");
    }
    [Test]
    public void TestSymbolY0 ()
    {
      DoTest ("symbol #this,is,a,sym", "#this,is,a,sym");
    }
    [Test]
    public void TestSymbolY1 ()
    {
      DoTest ("{u:[E|S|x 0 #a #symbol] <-symbol $u.x}", "[E|S|x 0 #a #symbol]");
    }
    [Test]
    public void TestSymbolK ()
    {
      DoTest ("symbol {x:#a y:#b z:#c}", "#a #b #c");
    }
    [Test]
    public void TestSymbolT ()
    {
      DoTest ("symbol 2019.12.20", "#2019.12.20");
    }

    [Test]
    public void TestLongX ()
    {
      DoTest ("long \\xFF", "255");
    }
    [Test]
    public void TestLongD ()
    {
      DoTest ("long 1.0", "1");
    }
    [Test]
    public void TestLongL0 ()
    {
      DoTest ("long 1", "1");
    }
    [Test]
    public void TestLongL1 ()
    {
      DoTest ("{u:[E|S|x 0 #a 0] <-long $u.x}", "[E|S|x 0 #a 0]");
    }
    [Test]
    public void TestLongM ()
    {
      DoTest ("long 1m", "1");
    }
    [Test]
    public void TestLongB ()
    {
      DoTest ("long true", "1");
    }
    [Test]
    public void TestLongS ()
    {
      DoTest ("long \"1\"", "1");
    }
    [Test]
    public void TestLongS1 ()
    {
      DoTest ("100 long \"\"", "100");
    }
    [Test]
    public void TestLongK ()
    {
      DoTest ("long {x:0 y:1 z:2}", "0 1 2");
    }
    [Test]
    public void TestLongY ()
    {
      DoTest ("long #0 #1 #2", "0 1 2");
    }
    [Test]
    public void TestLongT0 ()
    {
      DoTest ("long 0001.01.01", "0");
    }
    [Test]
    public void TestLongT1 ()
    {
      DoTest ("long 00:00", "0");
    }
    [Test]
    public void TestLongT2 ()
    {
      DoTest ("long 0001.01.01 00:00", "0");
    }
    [Test]
    public void TestLongT3 ()
    {
      DoTest ("long 0001.01.01 00:00:00.000000", "0");
    }

    [Test]
    public void TestByteX ()
    {
      DoTest ("byte \\xFF", "\\xFF");
    }
    [Test]
    public void TestByteX1 ()
    {
      DoTest ("{u:[E|S|x 0 #a \\x00] <-byte $u.x}", "[E|S|x 0 #a \\x00]");
    }
    [Test]
    public void TestByteD ()
    {
      DoTest ("byte 1.0", "\\x01");
    }
    [Test]
    public void TestByteL0 ()
    {
      DoTest ("byte 1", "\\x01");
    }
    [Test]
    public void TestByteM ()
    {
      DoTest ("byte 1m", "\\x01");
    }
    [Test]
    public void TestByteB ()
    {
      DoTest ("byte true", "\\x01");
    }
    [Test]
    public void TestByteS ()
    {
      DoTest ("byte \"255\"", "\\xFF");
    }
    [Test]
    public void TestByteK ()
    {
      DoTest ("byte {x:\\x00 y:\\x01 z:\\x02}", "\\x00 \\x01 \\x02");
    }

    [Test]
    public void TestDoubleX ()
    {
      DoTest ("double \\xFF", "255.0");
    }
    [Test]
    public void TestDoubleD0 ()
    {
      DoTest ("double 1.0", "1.0");
    }
    [Test]
    public void TestDoubleD1 ()
    {
      DoTest ("{u:[E|S|x 0 #a 0.0] <-double $u.x}", "[E|S|x 0 #a 0.0]");
    }
    [Test]
    public void TestDoubleL ()
    {
      DoTest ("double 1", "1.0");
    }
    [Test]
    public void TestDoubleM ()
    {
      DoTest ("double 1m", "1.0");
    }
    [Test]
    public void TestDoubleB ()
    {
      DoTest ("double true", "1.0");
    }
    [Test]
    public void TestDoubleS ()
    {
      DoTest ("double \"1\"", "1.0");
    }
    [Test]
    public void TestDoubleS1 ()
    {
      DoTest ("double \"foo\" \"1\" \"2.34\"", "NaN 1.0 2.34");
    }
    [Test]
    public void TestDoubleS2 ()
    {
      DoTest ("0.0 double \"\" \"1\" \"2.34\"", "0.0 1.0 2.34");
    }
    [Test]
    public void TestDoubleK ()
    {
      DoTest ("double {x:0.0 y:1.0 z:2.0}", "0.0 1.0 2.0");
    }
    [Test]
    public void TestIsNaN ()
    {
      DoTest ("isnan 1.0 NaN 0.1", "false true false");
    }

    [Test]
    public void TestBooleanX ()
    {
      DoTest ("boolean \\x00 \\x01 \\x02", "false true true");
    }
    [Test]
    public void TestBooleanD ()
    {
      DoTest ("boolean 0.0", "false");
    }
    [Test]
    public void TestBooleanL ()
    {
      DoTest ("boolean 0", "false");
    }
    [Test]
    public void TestBooleanM ()
    {
      DoTest ("boolean 0m", "false");
    }
    [Test]
    public void TestBooleanB0 ()
    {
      DoTest ("boolean false", "false");
    }
    [Test]
    public void TestBooleanB1 ()
    {
      DoTest ("{u:[E|S|x 0 #a true] <-boolean $u.x}", "[E|S|x 0 #a true]");
    }
    [Test]
    public void TestBooleanS ()
    {
      DoTest ("boolean \"true\"", "true");
    }
    [Test]
    public void TestBooleanS1 ()
    {
      DoTest ("false boolean \"\"", "false");
    }
    [Test]
    public void TestBooleanK ()
    {
      DoTest ("boolean {x:true y:false z:true}", "true false true");
    }

    [Test]
    public void TestDecimalX ()
    {
      DoTest ("decimal \\x00", "0m");
    }
    [Test]
    public void TestDecimalD ()
    {
      DoTest ("decimal 0.0", "0m");
    }
    [Test]
    public void TestDecimalL ()
    {
      DoTest ("decimal 0", "0m");
    }
    [Test]
    public void TestDecimalM0 ()
    {
      DoTest ("decimal 0m", "0m");
    }
    [Test]
    public void TestDecimalM1 ()
    {
      DoTest ("{u:[E|S|x 0 #a 0m] <-decimal $u.x}", "[E|S|x 0 #a 0m]");
    }
    [Test]
    public void TestDecimalB ()
    {
      DoTest ("decimal true", "1m");
    }
    [Test]
    public void TestDecimalS ()
    {
      DoTest ("decimal \"1\"", "1m");
    }
    [Test]
    public void TestDecimalS1 ()
    {
      DoTest ("1m decimal \"\"", "1m");
    }
    [Test]
    public void TestDecimalK ()
    {
      DoTest ("decimal {x:0m y:1m z:2m}", "0 1 2m");
    }

    [Test]
    public void TestTimeL ()
    {
      DoTest ("time 0", "0001.01.01 00:00:00.000000");
    }
    [Test]
    public void TestTimeT0 ()
    {
      DoTest ("time 0001.01.01 00:00:00.000000", "0001.01.01 00:00:00.000000");
    }
    [Test]
    public void TestTimeS ()
    {
      DoTest ("time \"0001.01.01 00:00:00.000000\"", "0001.01.01 00:00:00.000000");
    }
    [Test]
    public void TestTimeTS ()
    {
      DoTest ("0001.01.01 00:00:00.000000 time \"\"", "0001.01.01 00:00:00.000000");
    }
    [Test]
    public void TestTimeYT1 ()
    {
      DoTest ("#date time 0001.01.01 00:00:00.000000", "0001.01.01");
    }
    [Test]
    public void TestTimeYT2 ()
    {
      DoTest ("#daytime time 0001.01.01 00:00:00.000000", "00:00:00");
    }
    [Test]
    public void TestTimeYT3 ()
    {
      DoTest ("#datetime time 0001.01.01 00:00:00.000000", "0001.01.01 00:00:00");
    }
    [Test]
    public void TestTimeYT4 ()
    {
      DoTest ("#timestamp time 0001.01.01 00:00:00.000000", "0001.01.01 00:00:00.000000");
    }
    [Test]
    public void TestTimeYL0 ()
    {
      DoTest ("#date time 0", "0001.01.01");
    }
    [Test]
    public void TestTimeYL1 ()
    {
      DoTest ("#daytime time 0", "00:00:00");
    }
    [Test]
    public void TestTimeYL2 ()
    {
      DoTest ("#datetime time 0", "0001.01.01 00:00:00");
    }
    [Test]
    public void TestTimeYL3 ()
    {
      DoTest ("#timestamp time 0", "0001.01.01 00:00:00.000000");
    }

    [Test]
    public void TestDayT ()
    {
      DoTest ("day 0.12:34:56.789123", "0");
    }
    [Test]
    public void TestDayTU ()
    {
      DoTest ("day [x 0.12:34:56.789123]", "[x 0]");
    }
    [Test]
    public void TestHourT ()
    {
      DoTest ("hour 0.12:34:56.789123", "12");
    }
    [Test]
    public void TestHourTU ()
    {
      DoTest ("hour [x 0.12:34:56.789123]", "[x 12]");
    }
    [Test]
    public void TestMinuteT ()
    {
      DoTest ("minute 0.12:34:56.789123", "34");
    }
    [Test]
    public void TestMinuteTU ()
    {
      DoTest ("minute [x 0.12:34:56.789123]", "[x 34]");
    }
    [Test]
    public void TestSecondT ()
    {
      DoTest ("second 0.12:34:56.789123", "56");
    }
    [Test]
    public void TestSecondTU ()
    {
      DoTest ("second [x 0.12:34:56.789123]", "[x 56]");
    }
    [Test]
    public void TestNanoT ()
    {
      DoTest ("nano 0.12:34:56.789123", "789123000");
    }
    [Test]
    public void TestNanoTU ()
    {
      DoTest ("nano [x 0.12:34:56.789123]", "[x 789123000]");
    }

    [Test]
    public void TestYearDT ()
    {
      DoTest ("year 2019.10.24 21:13:40.439622", "2019");
    }
    [Test]
    public void TestYearDTU ()
    {
      DoTest ("year [x 2019.10.24 21:13:40.439622]", "[x 2019]");
    }
    [Test]
    public void TestMonthDT ()
    {
      DoTest ("month 2019.10.24 21:13:40.439622", "10");
    }
    [Test]
    public void TestMonthDTU ()
    {
      DoTest ("month [x 2019.10.24 21:13:40.439622]", "[x 10]");
    }
    [Test]
    public void TestDayDT ()
    {
      DoTest ("day 2019.10.24 21:13:40.439622", "24");
    }
    [Test]
    public void TestDayDTU ()
    {
      DoTest ("day [x 2019.10.24 21:13:40.439622]", "[x 24]");
    }
    [Test]
    public void TestHourDT ()
    {
      DoTest ("hour 2019.10.24 21:13:40.439622", "21");
    }
    [Test]
    public void TestHourDTU ()
    {
      DoTest ("hour [x 2019.10.24 21:13:40.439622]", "[x 21]");
    }
    [Test]
    public void TestMinuteDT ()
    {
      DoTest ("minute 2019.10.24 21:13:40.439622", "13");
    }
    [Test]
    public void TestMinuteDTU ()
    {
      DoTest ("minute [x 2019.10.24 21:13:40.439622]", "[x 13]");
    }
    [Test]
    public void TestSecondDT ()
    {
      DoTest ("second 2019.10.24 21:13:40.439622", "40");
    }
    [Test]
    public void TestSecondDTU ()
    {
      DoTest ("second [x 2019.10.24 21:13:40.439622]", "[x 40]");
    }
    [Test]
    public void TestNanoDT ()
    {
      DoTest ("nano 2019.10.24 21:13:40.439622", "439622000");
    }
    [Test]
    public void TestNanoDTU ()
    {
      DoTest ("nano [x 2019.10.24 21:13:40.439622]", "[x 439622000]");
    }

    [Test]
    public void TestDayL ()
    {
      DoTest ("day 0", "0.00:00:00.000000");
    }
    [Test]
    public void TestDayLU ()
    {
      DoTest ("day [x 0]", "[x 0.00:00:00.000000]");
    }
    [Test]
    public void TestHourL ()
    {
      DoTest ("hour 12", "0.12:00:00.000000");
    }
    [Test]
    public void TestHourLU ()
    {
      DoTest ("hour [x 12]", "[x 0.12:00:00.000000]");
    }
    [Test]
    public void TestMinuteL ()
    {
      DoTest ("minute 34", "0.00:34:00.000000");
    }
    [Test]
    public void TestMinuteLU ()
    {
      DoTest ("minute [x 34]", "[x 0.00:34:00.000000]");
    }
    [Test]
    public void TestSecondL ()
    {
      DoTest ("second 56", "0.00:00:56.000000");
    }
    [Test]
    public void TestSecondLU ()
    {
      DoTest ("second [x 56]", "[x 0.00:00:56.000000]");
    }
    [Test]
    public void TestNanoL ()
    {
      DoTest ("nano 0.12:34:56.789123", "789123000");
    }
    [Test]
    public void TestNanoLU ()
    {
      DoTest ("nano [x 0.12:34:56.789123]", "[x 789123000]");
    }

    [Test]
    public void TestDateT ()
    {
      DoTest ("timestamp date 1979.09.04 12:34:56.789101", "1979.09.04 00:00:00.000000");
    }
    [Test]
    public void TestDateTU ()
    {
      DoTest ("timestamp date [x 1979.09.04 12:34:56.789101]", "[x 1979.09.04 00:00:00.000000]");
    }
    [Test]
    public void TestDaytimeT ()
    {
      DoTest ("timestamp daytime 1979.09.04 12:34:56.789101", "0001.01.01 12:34:56.000000");
    }
    [Test]
    public void TestDaytimeTU ()
    {
      DoTest ("timestamp daytime [x 1979.09.04 12:34:56.789101]", "[x 0001.01.01 12:34:56.000000]");
    }
    [Test]
    public void TestDatetimeT ()
    {
      DoTest ("timestamp datetime 1979.09.04 12:34:56.789101", "1979.09.04 12:34:56.000000");
    }
    [Test]
    public void TestDatetimeTU ()
    {
      DoTest ("timestamp datetime [x 1979.09.04 12:34:56.789101]",
              "[x 1979.09.04 12:34:56.000000]");
    }
    [Test]
    public void TestTimestampT ()
    {
      DoTest ("timestamp timestamp 1979.09.04 12:34:56.789101", "1979.09.04 12:34:56.789101");
    }
    [Test]
    public void TestTimestampTU ()
    {
      DoTest ("timestamp timestamp [x 1979.09.04 12:34:56.789101]",
              "[x 1979.09.04 12:34:56.789101]");
    }
    [Test]
    public void TestTimespanT ()
    {
      DoTest ("timespan 1979.09.04 12:34:56.789101", "722695.12:34:56.789101");
    }
    [Test]
    public void TestTimespanTU ()
    {
      DoTest ("timespan [x 1979.09.04 12:34:56.789101]", "[x 722695.12:34:56.789101]");
    }

    [Test]
    public void TestNextDayOfWeek1 ()
    {
      DoTest ("2018.10.07 nextDayOfWeek \"Friday\"", "2018.10.12");
    }
    [Test]
    public void TestNextDayOfWeek2 ()
    {
      DoTest ("2018.10.05 nextDayOfWeek \"Friday\"", "2018.10.05");
    }
    [Test]
    public void TestNextDayOfWeek3 ()
    {
      DoTest ("2018.10.04 nextDayOfWeek \"Friday\"", "2018.10.05");
    }
    [Test]
    public void TestToDisplayTime ()
    {
      DoTest (
        "{tzid:(\"Unix\" = info \"platform\") switch {:\"America/Chicago\" :\"Central Standard Time\"} :displayTimezone $tzid result:date toDisplayTime 2018.11.22 02:04:11.871303 :displayTimezone \"UTC\" <-$result}",
        "2018.11.21");
    }
    [Test]
    public void TestReferenceS ()
    {
      DoTest ("reference \"x\" \"y\" \"z\"", "$x.y.z");
    }
    [Test]
    public void TestReferenceY ()
    {
      DoTest ("reference #x,y,z", "$x.y.z");
    }
    [Test]
    public void TestDaysInMonth ()
    {
      DoTest ("daysInMonth 2019 2 2020 2", "28 29");
    }
    [Test]
    public void TestAddDays ()
    {
      DoTest ("2019.10.29 addDays 7", "2019.11.05");
    }
    [Test]
    public void TestDayOfWeek ()
    {
      DoTest ("dayOfWeek 2019.10.29", "\"Tuesday\"");
    }

    // Count for blocks and vectors
    [Test]
    public void TestCountK0 ()
    {
      DoTest ("count {}", "0");
    }
    [Test]
    public void TestCountK1 ()
    {
      DoTest ("count {a:1 b:2}", "2");
    }
    [Test]
    public void TestCountX ()
    {
      DoTest ("count \\xFF", "1");
    }
    [Test]
    public void TestCountD ()
    {
      DoTest ("count 1.0", "1");
    }
    [Test]
    public void TestCountL ()
    {
      DoTest ("count 1", "1");
    }
    [Test]
    public void TestCountM ()
    {
      DoTest ("count 1m", "1");
    }
    [Test]
    public void TestCountB ()
    {
      DoTest ("count false", "1");
    }
    [Test]
    public void TestCountS ()
    {
      DoTest ("count \"x\"", "1");
    }
    [Test]
    public void TestCountY ()
    {
      DoTest ("count #x", "1");
    }
    [Test]
    public void TestCountT ()
    {
      DoTest ("count 2015.05.26", "1");
    }
    [Test]
    public void TestCountN ()
    {
      DoTest ("count ++", "1");
    }
    [Test]
    public void TestCountU ()
    {
      DoTest ("{u:[S|x #a 0] <-count #b cube $u}", "1");
    }
    [Test]
    public void TestCountR ()
    {
      DoTest ("count reference \"x.y.z\"", "1");
    }
    [Test]
    public void TestCountP ()
    {
      DoTest ("count [? foo bar baz ?]", "2");
    }
    [Test]
    public void TestCountOMonad ()
    {
      DoTest ("{op::not true <-count $op}", "1");
    }
    [Test]
    public void TestCountODyad ()
    {
      DoTest ("{op::1 + 2 <-count $op}", "2");
    }
    [Test]
    public void TestCountOChain ()
    {
      DoTest ("{op::1 + 2 + 3 <-count $op}", "3");
    }

    // Length for strings
    [Test]
    public void TestLengthS ()
    {
      DoTest ("length \"aaa\" \"bb\" \"a\" \"\"", "3 2 1 0");
    }
    [Test]
    public void TestLengthY ()
    {
      DoTest ("length # #a #aaa #a,b #a,b,c", "0 1 1 2 3");
    }

    // Repeat
    [Test]
    public void TestRepeatLX ()
    {
      DoTest ("3 repeat \\x01", "\\x01 \\x01 \\x01");
    }
    [Test]
    public void TestRepeatLD ()
    {
      DoTest ("3 repeat 1.0", "1.0 1.0 1.0");
    }
    [Test]
    public void TestRepeatLL ()
    {
      DoTest ("3 repeat 1", "1 1 1");
    }
    [Test]
    public void TestRepeatLM ()
    {
      DoTest ("3 repeat 1m", "1 1 1m");
    }
    [Test]
    public void TestRepeatLB ()
    {
      DoTest ("3 repeat false", "false false false");
    }
    [Test]
    public void TestRepeatLS ()
    {
      DoTest ("3 repeat \"x\"", "\"x\" \"x\" \"x\"");
    }
    [Test]
    public void TestRepeatLY ()
    {
      DoTest ("3 repeat #x", "#x #x #x");
    }
    [Test]
    public void TestRepeatLT ()
    {
      DoTest ("3 repeat 2015.05.26", "2015.05.26 2015.05.26 2015.05.26");
    }
    [Test]
    public void TestRepeatLN ()
    {
      DoTest ("3 repeat ++", "++ ++ ++");
    }
    [Test]
    public void TestRepeatMulti ()
    {
      DoTest ("3 repeat #x #y #z", "#x #y #z #x #y #z #x #y #z");
    }

    // [Test]
    // public void TestRepeatLY() { DoTest("3l repeat #x", "#x #x #x"); }

    // Sequential Aggregations - avg, sum, max, min, med, dev, first, last, count
    [Test]
    public void TestSumD ()
    {
      DoTest ("sum 1.0 2.0 3.0", "6.0");
    }
    [Test]
    public void TestSumL ()
    {
      DoTest ("sum 1 2 3", "6");
    }
    [Test]
    public void TestSumM ()
    {
      DoTest ("sum 1 2 3m", "6m");
    }
    [Test]
    public void TestSumX ()
    {
      DoTest ("sum \\x01 \\x02 \\x03", "\\x06");
    }
    [Test]
    public void TestAvgD ()
    {
      DoTest ("avg 1.0 2.0 3.0", "2.0");
    }
    [Test]
    public void TestAvgL ()
    {
      DoTest ("avg 1 2 3", "2.0");
    }
    [Test]
    public void TestAvgM ()
    {
      DoTest ("avg 1 2 3m", "2m");
    }
    [Test]
    public void TestAvgX ()
    {
      DoTest ("avg \\x01 \\x02 \\x03", "2.0");
    }
    [Test]
    public void TestLowD ()
    {
      DoTest ("low 1.0 2.0 3.0", "1.0");
    }
    [Test]
    public void TestLowL ()
    {
      DoTest ("low 1.0 2.0 3.0", "1.0");
    }
    [Test]
    public void TestLowM ()
    {
      DoTest ("low 1 2 3m", "1m");
    }
    [Test]
    public void TestLowX ()
    {
      DoTest ("low \\x01 \\x02 \\x03", "\\x01");
    }
    [Test]
    public void TestHighD ()
    {
      DoTest ("high -1.0 -2.0 -3.0", "-1.0");
    }
    [Test]
    public void TestHighL ()
    {
      DoTest ("high -1 -2 -3", "-1");
    }
    [Test]
    public void TestHighM ()
    {
      DoTest ("high -1 -2 -3m", "-1m");
    }
    [Test]
    public void TestHighX ()
    {
      DoTest ("high \\x01 \\x02 \\x03", "\\x03");
    }
    [Test]
    public void TestAny1 ()
    {
      DoTest ("any false true false", "true");
    }
    [Test]
    public void TestAny2 ()
    {
      DoTest ("any false false false", "false");
    }
    [Test]
    public void TestAll1 ()
    {
      DoTest ("all true true true", "true");
    }
    [Test]
    public void TestAll2 ()
    {
      DoTest ("all false false false", "false");
    }
    [Test]
    public void TestNone1 ()
    {
      DoTest ("none false false false", "true");
    }
    [Test]
    public void TestNone2 ()
    {
      DoTest ("none false false true", "false");
    }

    [Test]
    public void TestSumKD ()
    {
      DoTest ("sum {x:1.0 2.0 3.0 y:4.0 5.0 6.0}", "5.0 7.0 9.0");
    }
    [Test]
    public void TestSumKL ()
    {
      DoTest ("sum {x:1 2 3 y:4 5 6}", "5 7 9");
    }
    [Test]
    public void TestSumKM ()
    {
      DoTest ("sum {x:1 2 3m y:4 5 6m}", "5 7 9m");
    }
    [Test]
    public void TestSumKX ()
    {
      DoTest ("sum {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "\\x05 \\x07 \\x09");
    }
    [Test]
    public void TestAvgKD ()
    {
      DoTest ("avg {x:1.0 2.0 3.0 y:4.0 5.0 6.0}", "2.5 3.5 4.5");
    }
    [Test]
    public void TestAvgKL ()
    {
      DoTest ("avg {x:1 2 3 y:4 5 6}", "2.5 3.5 4.5");
    }
    [Test]
    public void TestAvgKM ()
    {
      DoTest ("avg {x:1 2 3m y:4 5 6m}", "2.5 3.5 4.5m");
    }
    [Test]
    public void TestAvgKX ()
    {
      DoTest ("avg {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "2.5 3.5 4.5");
    }
    [Test]
    public void TestLowKD ()
    {
      DoTest ("low {x:1.0 2.0 3.0 y:4.0 5.0 6.0}", "1.0 2.0 3.0");
    }
    [Test]
    public void TestLowKL ()
    {
      DoTest ("low {x:1 2 3 y:4 5 6}", "1 2 3");
    }
    [Test]
    public void TestLowKM ()
    {
      DoTest ("low {x:1 2 3m y:4 5 6m}", "1 2 3m");
    }
    [Test]
    public void TestLowKX ()
    {
      DoTest ("low {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "\\x01 \\x02 \\x03");
    }
    [Test]
    public void TestHighKD ()
    {
      DoTest ("high {x:-1.0 -2.0 -3.0 y:-4.0 -5.0 -6.0}", "-1.0 -2.0 -3.0");
    }
    [Test]
    public void TestHighKL ()
    {
      DoTest ("high {x:-1 -2 -3 y:-4 -5 -6}", "-1 -2 -3");
    }
    [Test]
    public void TestHighKM ()
    {
      DoTest ("high {x:-1 -2 -3m y:-4 -5 -6m}", "-1 -2 -3m");
    }
    [Test]
    public void TestHighKX ()
    {
      DoTest ("high {x:\\x01 \\x02 \\x03 y:\\x04 \\x05 \\x06}", "\\x04 \\x05 \\x06");
    }
    [Test]
    public void TestAnyKB1 ()
    {
      DoTest ("any {x:false true false y:true true false}", "true true false");
    }
    [Test]
    public void TestAnyKB2 ()
    {
      DoTest ("any {x:false false false y:true true true}", "true true true");
    }
    [Test]
    public void TestAllKB1 ()
    {
      DoTest ("all {x:true false true y:false true true}", "false false true");
    }
    [Test]
    public void TestAllKB2 ()
    {
      DoTest ("all {x:false false false y:true true true}", "false false false");
    }
    [Test]
    public void TestNoneKB1 ()
    {
      DoTest ("none {x:false false true y:false true false}", "true false false");
    }
    [Test]
    public void TestNoneKB2 ()
    {
      DoTest ("none {x:false false true y:false true false}", "true false false");
    }

    [Test]
    public void TestMinDD ()
    {
      DoTest ("3.0 2.0 1.0 min 1.0 2.0 3.0", "1.0 2.0 1.0");
    }
    [Test]
    public void TestMinLL ()
    {
      DoTest ("3 2 1 min 1 2 3", "1 2 1");
    }
    [Test]
    public void TestMinMM ()
    {
      DoTest ("3 2 1m min 1 2 3m", "1 2 1m");
    }
    [Test]
    public void TestMinXX ()
    {
      DoTest ("\\x03 \\x02 \\x01 min \\x01 \\x02 \\x03", "\\x01 \\x02 \\x01");
    }
    [Test]
    public void TestMaxDD ()
    {
      DoTest ("-3.0 -2.0 -1.0 max -1.0 -2.0 -3.0", "-1.0 -2.0 -1.0");
    }
    [Test]
    public void TestMaxLL ()
    {
      DoTest ("-3 -2 -1 max -1 -2 -3", "-1 -2 -1");
    }
    [Test]
    public void TestMaxMM ()
    {
      DoTest ("-3 -2 -1m max -1 -2 -3m", "-1 -2 -1m");
    }
    [Test]
    public void TestMaxXX ()
    {
      DoTest ("\\x03 \\x02 \\x01 max \\x01 \\x02 \\x03", "\\x03 \\x02 \\x03");
    }
  }
}

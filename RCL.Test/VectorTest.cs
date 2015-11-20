
using System;
using NUnit.Framework;
using O2.Kernel;

//use an extra "Vector" namespace so that the nunit gui is easier to read.
namespace O2.Test.O2VectorTest
{
  public abstract class O2AbstractVectorTest<T>
  {
    [Test]
    public void TestCtor()
    {
      O2Vector<T> x = TestX();
      Assert.AreEqual(3, x.Count);
      Assert.AreEqual(TestX0(), x[0]);
      Assert.AreEqual(TestX1(), x[1]);
      Assert.AreEqual(TestX2(), x[2]);
    }

    [Test]
    public void TestEquals()
    {
      O2Vector<T> x = TestX();
      O2Vector<T> y = TestY();
      O2Vector<T> z = TestZ();
      Assert.AreNotEqual(x, y);
      Assert.AreNotEqual(y, x);
      Assert.AreEqual(x, z);
      Assert.AreEqual(y, y);
    }

    [Test]
    public void TestToString()
    {
      Assert.AreEqual(StringFormX(), TestX().ToString());
      Assert.AreEqual(StringFormY(), TestY().ToString());
      Assert.AreEqual(StringFormZ(), TestZ().ToString());
    }

    public abstract O2Vector<T> TestX();
    public abstract T TestX0();
    public abstract T TestX1();
    public abstract T TestX2();
    public abstract long TestXCount();

    public abstract O2Vector<T> TestY();
    public abstract T TestY0();
    public abstract T TestY1();
    public abstract T TestY2();

    public abstract O2Vector<T> TestZ();
    public abstract T TestZ0();
    public abstract T TestZ1();
    public abstract T TestZ2();

    public abstract string StringFormX();
    public abstract string StringFormY();
    public abstract string StringFormZ();
  }

  [TestFixture]
  public class O2DoubleTest : O2AbstractVectorTest<double>
  {
    public override O2Vector<double> TestX()
    {
      return new O2Double(10000000000.001, 20000000000.002, 30000000000.003);
    }

    public override double TestX0()
    {
      return 10000000000.001;
    }

    public override double TestX1()
    {
      return 20000000000.002;
    }

    public override double TestX2()
    {
      return 30000000000.003;
    }

    public override long TestXCount()
    {
      return 3;
    }

    public override string StringFormX()
    {
      return "10000000000.001 20000000000.002 30000000000.003d";
    }

    public override O2Vector<double> TestY()
    {
      return new O2Double(10000000000.001, 20000000000.002, 40000000000.004);
    }

    public override double TestY0()
    {
      return 10000000000.001;
    }

    public override double TestY1()
    {
      return 20000000000.002;
    }

    public override double TestY2()
    {
      return 40000000000.004;
    }

    public override string StringFormY()
    {
      return "10000000000.001 20000000000.002 40000000000.004d";
    }

    public override O2Vector<double> TestZ()
    {
      return new O2Double(10000000000.001, 20000000000.002, 30000000000.003);
    }

    public override double TestZ0()
    {
      return 10000000000.001;
    }

    public override double TestZ1()
    {
      return 20000000000.002;
    }

    public override double TestZ2()
    {
      return 30000000000.003;
    }

    public override string StringFormZ()
    {
      return "10000000000.001 20000000000.002 30000000000.003d";
    }
  }


  [TestFixture]
  public class O2LongTest : O2AbstractVectorTest<long>
  {
    public override O2Vector<long> TestX()
    {
      return new O2Long(100000000000, 200000000000, 300000000000);
    }

    public override long TestX0()
    {
      return 100000000000;
    }

    public override long TestX1()
    {
      return 200000000000;
    }

    public override long TestX2()
    {
      return 300000000000;
    }

    public override long TestXCount()
    {
      return 3;
    }

    public override string StringFormX()
    {
      return "100000000000 200000000000 300000000000l";
    }

    public override O2Vector<long> TestY()
    {
      return new O2Long(100000000000, 200000000000, 400000000000);
    }

    public override long TestY0()
    {
      return 100000000000;
    }

    public override long TestY1()
    {
      return 200000000000;
    }

    public override long TestY2()
    {
      return 400000000000;
    }

    public override string StringFormY()
    {
      return "100000000000 200000000000 400000000000l";
    }

    public override O2Vector<long> TestZ()
    {
      return new O2Long(100000000000, 200000000000, 300000000000);
    }

    public override long TestZ0()
    {
      return 100000000000;
    }

    public override long TestZ1()
    {
      return 200000000000;
    }

    public override long TestZ2()
    {
      return 300000000000;
    }

    public override string StringFormZ()
    {
      return "100000000000 200000000000 300000000000l";
    }
  }

  [TestFixture]
  public class O2DecimalTest : O2AbstractVectorTest<decimal>
  {
    public override O2Vector<decimal> TestX()
    {
      return new O2Decimal(1.25m, 2.50m, 3.75m);
    }

    public override decimal TestX0()
    {
      return 1.25m;
    }

    public override decimal TestX1()
    {
      return 2.50m;
    }

    public override decimal TestX2()
    {
      return 3.75m;
    }

    public override long TestXCount()
    {
      return 3;
    }

    public override string StringFormX()
    {
      return "1.25 2.50 3.75m";
    }

    public override O2Vector<decimal> TestY()
    {
      return new O2Decimal(1.25m, 2.50m, 4.75m);
    }

    public override decimal TestY0()
    {
      return 1.25m;
    }

    public override decimal TestY1()
    {
      return 2.50m;
    }

    public override decimal TestY2()
    {
      return 4.75m;
    }

    public override string StringFormY()
    {
      return "1.25 2.50 4.75m";
    }

    public override O2Vector<decimal> TestZ()
    {
      return new O2Decimal(1.25m, 2.50m, 3.75m);
    }

    public override decimal TestZ0()
    {
      return 1.25m;
    }

    public override decimal TestZ1()
    {
      return 2.50m;
    }

    public override decimal TestZ2()
    {
      return 3.75m;
    }

    public override string StringFormZ()
    {
      return "1.25 2.50 3.75m";
    }
  }

  [TestFixture]
  public class O2StringTest : O2AbstractVectorTest<string>
  {
    public override O2Vector<string> TestX()
    {
      return new O2String("x", "y", "z");
    }

    public override string TestX0()
    {
      return "x";
    }

    public override string TestX1()
    {
      return "y";
    }

    public override string TestX2()
    {
      return "z";
    }

    public override long TestXCount()
    {
      return 3;
    }

    public override string StringFormX()
    {
      return "\"x\" \"y\" \"z\"";
    }

    public override O2Vector<string> TestY()
    {
      return new O2String("x", "y", "n");
    }

    public override string TestY0()
    {
      return "x";
    }

    public override string TestY1()
    {
      return "y";
    }

    public override string TestY2()
    {
      return "n";
    }

    public override string StringFormY()
    {
      return "\"x\" \"y\" \"n\"";
    }

    public override O2Vector<string> TestZ()
    {
      return new O2String("x", "y", "z");
    }

    public override string TestZ0()
    {
      return "x";
    }

    public override string TestZ1()
    {
      return "y";
    }

    public override string TestZ2()
    {
      return "z";
    }

    public override string StringFormZ()
    {
      return "\"x\" \"y\" \"z\"";
    }
  }

  [TestFixture]
  public class O2BoolTest : O2AbstractVectorTest<bool>
  {
    public override O2Vector<bool> TestX()
    {
      return new O2Boolean(true, false, true);
    }

    public override bool TestX0()
    {
      return true;
    }

    public override bool TestX1()
    {
      return false;
    }

    public override bool TestX2()
    {
      return true;
    }

    public override long TestXCount()
    {
      return 3;
    }

    public override O2Vector<bool> TestY()
    {
      return new O2Boolean(true, false, false);
    }

    public override bool TestY0()
    {
      return true;
    }

    public override bool TestY1()
    {
      return false;
    }

    public override bool TestY2()
    {
      return false;
    }

    public override O2Vector<bool> TestZ()
    {
      return new O2Boolean(true, false, true);
    }

    public override bool TestZ0()
    {
      return true;
    }

    public override bool TestZ1()
    {
      return false;
    }

    public override bool TestZ2()
    {
      return true;
    }

    public override string StringFormX()
    {
      return "true false true";
    }

    public override string StringFormY()
    {
      return "true false false";
    }

    public override string StringFormZ()
    {
      return "true false true";
    }
  }
}

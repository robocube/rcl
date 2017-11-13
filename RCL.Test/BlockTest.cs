
using System;
using NUnit.Framework;
using RCL.Kernel;

namespace RCL.Test
{
  /// <summary>
  /// Test the behavior of the core objects of the RC language
  /// </summary>
  [TestFixture]
  public class BlockTest
  {
    protected RCRunner r = new RCRunner ();

    /// <summary>
    /// Interface with a simple RCObject
    /// </summary>
    [Test]
    public void TestBlock ()
    {
      RCVector<string> sv = new RCString ("x", "y", "z");
      RCVector<double> dv = new RCDouble (1, 2, 3);
      RCVector<bool> bv = new RCBoolean (true, false, true);
      RCBlock a = new RCBlock (null, "a", ":", sv);
      RCBlock b = new RCBlock (a, "b", ":", dv);
      RCBlock c = new RCBlock (b, "c", ":", bv);

      Assert.AreEqual (3, c.Count);
      Assert.AreSame (sv, c.Get ("a"));
      Assert.AreSame (dv, c.Get ("b"));
      Assert.AreSame (bv, c.Get ("c"));
      Assert.AreSame (sv, c.Get (0));
      Assert.AreSame (dv, c.Get (1));
      Assert.AreSame (bv, c.Get (2));

      Assert.AreEqual ("{a:\"x\" \"y\" \"z\" b:1.0 2.0 3.0 c:true false true}", c.ToString ());
    }

    /// <summary>
    /// Minimal operator evaluation
    /// </summary>
    [Test]
    public void TestMinimalOperator ()
    {
      r.Reset ();
      RCVector<double> x = new RCDouble (100, 1000, 10000);
      RCVector<double> y = new RCDouble (1, 2, 3);
      RCOperator v = r.New ("+", x, y);
      RCValue val = r.Run (v);
      Assert.AreEqual (new RCDouble (101, 1002, 10003), val);
      Assert.AreEqual ("101.0 1002.0 10003.0", val.ToString ());
    }

    /// <summary>
    /// Macro evaluation with one level of nesting
    /// </summary>
    [Test]
    public void TestNestedOperator()
    {
      r.Reset ();
      RCVector<double> x = new RCDouble (100, 1000, 10000);
      RCVector<double> y = new RCDouble (110, 1200, 13000);
      RCOperator pch = r.New ("/", r.New ("-", y, x), x);
      RCValue val = r.Run (pch);
      Assert.AreEqual (new RCDouble (0.1, 0.2, 0.3), val);
      Assert.AreEqual ("0.1 0.2 0.3", val.ToString ());
    }

    /// <summary>
    /// Macro evaluation with nesting on both sides, monadic macro execution
    /// </summary>
    [Test]
    public void TestTwoSidedNestedOperator()
    {
      r.Reset ();
      RCVector<double> tp = new RCDouble (10.00, 10.01, 10.02);
      RCVector<double> ts = new RCDouble (100, 500, 100);
      RCOperator vwp = r.New ("/",
                              r.New ("sum", r.New ("*", tp, ts)),
                              r.New ("sum", ts));
      RCValue val = r.Run (vwp);
      Assert.AreEqual (new RCDouble ((1000.0 + 5005.0 + 1002.0) / 700.0), val);
      Assert.AreEqual ("10.01", val.ToString ());
    }

    /// <summary>
    /// Core operations (+,-,/,*,&&,||,!) will have a special case to work
    /// over vectors with a scalar on either side of the operation.  Otherwise
    /// it would be hard to do something like (bp+ap)/2.  You would have to
    /// create a whole vector of 2's with the same length as the other vectors.
    /// Or I have to introduce scalars into the language which I will not do,
    /// because that means constantly worrying about whether you have a scalar
    /// or a vector and which operators work on which.
    /// </summary>
    [Test]
    public void TestScalarsRight()
    {
      r.Reset ();
      RCVector<double> x = new RCDouble (100, 200, 300);
      RCVector<double> two = new RCDouble (2);

      RCOperator add = r.New ("+", x, two);
      RCOperator sub = r.New ("-", x, two);
      RCOperator mult = r.New ("*", x, two);
      RCOperator div = r.New ("/", x, two);

      RCVector<double> addResult = (RCVector<double>) r.Run (add);
      RCVector<double> subResult = (RCVector<double>) r.Run (sub);
      RCVector<double> multResult = (RCVector<double>) r.Run (mult);
      RCVector<double> divResult = (RCVector<double>) r.Run (div);

      Assert.AreEqual (new RCDouble (102, 202, 302), addResult);
      Assert.AreEqual ("102.0 202.0 302.0", addResult.ToString());
      Assert.AreEqual (new RCDouble (98, 198, 298), subResult);
      Assert.AreEqual ("98.0 198.0 298.0", subResult.ToString());
      Assert.AreEqual (new RCDouble (200.0, 400.0, 600.0), multResult);
      Assert.AreEqual ("200.0 400.0 600.0", multResult.ToString ());
      Assert.AreEqual (new RCDouble (50, 100, 150), divResult);
      Assert.AreEqual ("50.0 100.0 150.0", divResult.ToString ());
    }

    /// <summary>
    /// See comment for TestScalarsRight
    /// </summary>
    [Test]
    public void TestScalarsLeft ()
    {
      r.Reset ();
      RCVector<double> two = new RCDouble (2);
      RCVector<double> x = new RCDouble (100, 200, 300);

      RCOperator add = r.New ("+", two, x);
      RCOperator sub = r.New ("-", two, x);
      RCOperator mult = r.New ("*", two, x);
      RCOperator div = r.New ("/", two, x);

      RCVector<double> addResult = (RCVector<double>) r.Run (add);
      RCVector<double> subResult = (RCVector<double>) r.Run (sub);
      RCVector<double> multResult = (RCVector<double>) r.Run (mult);
      RCVector<double> divResult = (RCVector<double>) r.Run (div);

      Assert.AreEqual (new RCDouble (102, 202, 302), addResult);
      Assert.AreEqual ("102.0 202.0 302.0", addResult.ToString());
      Assert.AreEqual (new RCDouble (-98, -198, -298), subResult);
      Assert.AreEqual ("-98.0 -198.0 -298.0", subResult.ToString());
      Assert.AreEqual (new RCDouble (200, 400, 600), multResult);
      Assert.AreEqual ("200.0 400.0 600.0", multResult.ToString());
      Assert.AreEqual (.02, divResult[0]);
      Assert.AreEqual (.01, divResult[1]);
      Assert.AreEqual (.00666, divResult[2], .0001);
      Assert.AreEqual ("0.02 0.01 0.00666666666666667", divResult.ToString());
    }

    /// <summary>
    /// Variable references, mixing object and operator evaluation
    /// </summary>
    [Test]
    public void TestReferencesInBlock()
    {
      r.Reset();
      RCBlock pos = new RCBlock (null, "pos", ":",
                                 new RCDouble (100, 200, 300));
      RCBlock px = new RCBlock (pos, "px", ":",
                                new RCDouble (10.01, 10.02, 10.01));
      RCBlock ccy = new RCBlock (px, "ccy", ":",
                                 r.New ("*", new RCReference ("pos"), new RCReference ("px")));

      RCValue val = r.Run (ccy);
      RCBlock obj = val as RCBlock;
      Assert.IsNotNull (obj, "result is not an RCObject");
      //Currency vector
      //RCVector<double> ccyv = obj.Get ("ccy") as RCVector<double>;
      Assert.AreEqual (new RCDouble (1001, 2004, 3003), obj.Get ("ccy"));
      Assert.AreEqual ("1001.0 2004.0 3003.0", obj.Get ("ccy").ToString ());
    }

    /// <summary>
    /// Nested objects, multiple level references, 
    /// yielding a single result from an object 
    /// </summary>
    [Test]
    public void TestYieldFromBlock ()
    {
      //{bbo:{bp=10.00 10.01 10.02 10.01 10.00 ap=10.02 10.03 10.03 10.02 10.01} sprd=$bbo.ap - $bbo.bp <=$sprd / $bbo.bp}
      //0.00199999999999996 0.00199800199800196 0.000998003992015947 0.000999000999000978 0.000999999999999979d

      r.Reset ();
      RCBlock bp = new RCBlock (null, "bp", ":",
                                new RCDouble (10.00, 10.01, 10.02, 10.01, 10.00));
      RCBlock ap = new RCBlock (bp, "ap", ":",
                                new RCDouble(10.02, 10.03, 10.03, 10.02, 10.01));
      
      RCBlock bbo = new RCBlock (null, "bbo", ":", ap);
      RCBlock sprd = new RCBlock (bbo, "sprd", ":",
                                  r.New ("-", new RCReference ("bbo.ap"), new RCReference ("bbo.bp")));
      RCBlock sprdpch = new RCBlock (sprd, "sprdbps", "<-",
                                     r.New ("/", new RCReference ("sprd"), new RCReference ("bbo.bp")));

      RCValue val = r.Run (sprdpch);
      RCVector<double> v = val as RCVector<double>;
      Assert.IsNotNull (v, "result was not an RCVector<double>");
      Assert.AreEqual (.02 / 10.00, v[0], .0001);
      Assert.AreEqual (.02 / 10.01, v[1], .0001);
      Assert.AreEqual (.01 / 10.02, v[2], .0001);
      Assert.AreEqual (.01 / 10.01, v[3], .0001);
      Assert.AreEqual (.01 / 10.00, v[4], .0001);
      Assert.AreEqual ("0.00199999999999996 0.00199800199800196 0.000998003992015947 0.000999000999000978 0.000999999999999979", v.ToString());
    }

    [Test]
    public void RawNames ()
    {
      r.Reset ();
      RCBlock k = (RCBlock) r.Peek ("{'Set-Cookie':\"abcdefg\"}");
      Assert.AreEqual ("'Set-Cookie'", k.GetName (0).Name);
      Assert.AreEqual ("Set-Cookie", k.GetName (0).RawName);
    }

    [Test]
    public void TestEscapedNames ()
    {
      r.Reset ();
      RCBlock k = new RCBlock (RCBlock.Empty, "foo-bar", ":", new RCString ("baz"));
      Assert.AreEqual ("'foo-bar'", k.GetName (0).Name);
      Assert.AreEqual ("foo-bar", k.GetName (0).RawName);
    }

    [Test]
    public void TestMacroCtor ()
    {
      r.Reset ();
      Assert.Throws<ArgumentNullException> (delegate () { r.New ("+", null, null); });
    }
  }
}

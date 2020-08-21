using System;
using NUnit.Framework;
using RCL.Kernel;

namespace RCL.Test
{
  /// <summary>
  /// Test the behavior of core classes in RCL
  /// </summary>
  [TestFixture]
  public class BlockTest
  {
    protected RCRunner runner = new RCRunner ();
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

      RCL.Kernel.Assert.AreEqual (3, c.Count);
      RCL.Kernel.Assert.AreSame (sv, c.Get ("a"));
      RCL.Kernel.Assert.AreSame (dv, c.Get ("b"));
      RCL.Kernel.Assert.AreSame (bv, c.Get ("c"));
      RCL.Kernel.Assert.AreSame (sv, c.Get (0));
      RCL.Kernel.Assert.AreSame (dv, c.Get (1));
      RCL.Kernel.Assert.AreSame (bv, c.Get (2));

      RCL.Kernel.Assert.AreEqual ("{a:\"x\" \"y\" \"z\" b:1.0 2.0 3.0 c:true false true}",
                                  c.ToString ());
    }

    /// <summary>
    /// Minimal operator evaluation
    /// </summary>
    [Test]
    public void TestMinimalOperator ()
    {
      runner.Reset ();
      RCVector<double> x = new RCDouble (100, 1000, 10000);
      RCVector<double> y = new RCDouble (1, 2, 3);
      RCOperator v = runner.New ("+", x, y);
      RCValue val = runner.Run (v);
      RCL.Kernel.Assert.AreEqual (new RCDouble (101, 1002, 10003), val);
      RCL.Kernel.Assert.AreEqual ("101.0 1002.0 10003.0", val.ToString ());
    }

    /// <summary>
    /// Macro evaluation with one level of nesting
    /// </summary>
    [Test]
    public void TestNestedOperator ()
    {
      runner.Reset ();
      RCVector<double> x = new RCDouble (100, 1000, 10000);
      RCVector<double> y = new RCDouble (110, 1200, 13000);
      RCOperator pch = runner.New ("/", runner.New ("-", y, x), x);
      RCValue val = runner.Run (pch);
      RCL.Kernel.Assert.AreEqual (new RCDouble (0.1, 0.2, 0.3), val);
      RCL.Kernel.Assert.AreEqual ("0.1 0.2 0.3", val.ToString ());
    }

    /// <summary>
    /// Macro evaluation with nesting on both sides, monadic macro execution
    /// </summary>
    [Test]
    public void TestTwoSidedNestedOperator ()
    {
      runner.Reset ();
      RCVector<double> tp = new RCDouble (10.00, 10.01, 10.02);
      RCVector<double> ts = new RCDouble (100, 500, 100);
      RCOperator vwp = runner.New ("/",
                                   runner.New ("sum", runner.New ("*", tp, ts)),
                                   runner.New ("sum", ts));
      RCValue val = runner.Run (vwp);
      RCL.Kernel.Assert.AreEqual (new RCDouble ((1000.0 + 5005.0 + 1002.0) / 700.0), val);
      RCL.Kernel.Assert.AreEqual ("10.01", val.ToString ());
    }

    /// <summary>
    /// Core operations like (+,-,/,*,and,or,not) will have a special case to work
    /// over vectors with a scalar on either side of the operation.  Otherwise
    /// it would be hard to do something like ($bp+$ap)/2.  You would have to
    /// create a whole vector of 2's with the same length as the other vectors.
    /// Or I have to introduce scalars into the language which I will not do,
    /// because that means constantly worrying about whether you have a scalar
    /// or a vector and which operators work on which.
    /// </summary>
    [Test]
    public void TestScalarsRight ()
    {
      runner.Reset ();
      RCVector<double> x = new RCDouble (100, 200, 300);
      RCVector<double> two = new RCDouble (2);

      RCOperator add = runner.New ("+", x, two);
      RCOperator sub = runner.New ("-", x, two);
      RCOperator mult = runner.New ("*", x, two);
      RCOperator div = runner.New ("/", x, two);

      RCVector<double> addResult = (RCVector<double>)runner.Run (add);
      RCVector<double> subResult = (RCVector<double>)runner.Run (sub);
      RCVector<double> multResult = (RCVector<double>)runner.Run (mult);
      RCVector<double> divResult = (RCVector<double>)runner.Run (div);

      RCL.Kernel.Assert.AreEqual (new RCDouble (102, 202, 302), addResult);
      RCL.Kernel.Assert.AreEqual ("102.0 202.0 302.0", addResult.ToString ());
      RCL.Kernel.Assert.AreEqual (new RCDouble (98, 198, 298), subResult);
      RCL.Kernel.Assert.AreEqual ("98.0 198.0 298.0", subResult.ToString ());
      RCL.Kernel.Assert.AreEqual (new RCDouble (200.0, 400.0, 600.0), multResult);
      RCL.Kernel.Assert.AreEqual ("200.0 400.0 600.0", multResult.ToString ());
      RCL.Kernel.Assert.AreEqual (new RCDouble (50, 100, 150), divResult);
      RCL.Kernel.Assert.AreEqual ("50.0 100.0 150.0", divResult.ToString ());
    }

    /// <summary>
    /// See comment for TestScalarsRight
    /// </summary>
    [Test]
    public void TestScalarsLeft ()
    {
      runner.Reset ();
      RCVector<double> two = new RCDouble (2);
      RCVector<double> x = new RCDouble (100, 200, 300);

      RCOperator add = runner.New ("+", two, x);
      RCOperator sub = runner.New ("-", two, x);
      RCOperator mult = runner.New ("*", two, x);
      RCOperator div = runner.New ("/", two, x);

      RCVector<double> addResult = (RCVector<double>)runner.Run (add);
      RCVector<double> subResult = (RCVector<double>)runner.Run (sub);
      RCVector<double> multResult = (RCVector<double>)runner.Run (mult);
      RCVector<double> divResult = (RCVector<double>)runner.Run (div);

      RCL.Kernel.Assert.AreEqual (new RCDouble (102, 202, 302), addResult);
      RCL.Kernel.Assert.AreEqual ("102.0 202.0 302.0", addResult.ToString ());
      RCL.Kernel.Assert.AreEqual (new RCDouble (-98, -198, -298), subResult);
      RCL.Kernel.Assert.AreEqual ("-98.0 -198.0 -298.0", subResult.ToString ());
      RCL.Kernel.Assert.AreEqual (new RCDouble (200, 400, 600), multResult);
      RCL.Kernel.Assert.AreEqual ("200.0 400.0 600.0", multResult.ToString ());
      RCL.Kernel.Assert.AreEqual (.02, divResult[0]);
      RCL.Kernel.Assert.AreEqual (.01, divResult[1]);
      RCL.Kernel.Assert.AreEqual (.00666, divResult[2]);
      RCL.Kernel.Assert.AreEqual ("0.02 0.01 0.00666666666666667", divResult.ToString ());
    }

    /// <summary>
    /// Variable references, mixing object and operator evaluation
    /// </summary>
    [Test]
    public void TestReferencesInBlock ()
    {
      runner.Reset ();
      RCBlock pos = new RCBlock (null, "pos", ":", new RCDouble (100, 200, 300));
      RCBlock px = new RCBlock (pos, "px", ":", new RCDouble (10.01, 10.02, 10.01));
      RCBlock ccy = new RCBlock (px,
                                 "ccy",
                                 ":",
                                 runner.New ("*",
                                             new RCReference ("pos"),
                                             new RCReference ("px")));

      RCValue val = runner.Run (ccy);
      RCBlock obj = val as RCBlock;
      RCL.Kernel.Assert.IsNotNull (obj);
      // Currency vector
      // RCVector<double> ccyv = obj.Get ("ccy") as RCVector<double>;
      RCL.Kernel.Assert.AreEqual (new RCDouble (1001, 2004, 3003), obj.Get ("ccy"));
      RCL.Kernel.Assert.AreEqual ("1001.0 2004.0 3003.0", obj.Get ("ccy").ToString ());
    }

    /// <summary>
    /// Nested objects, multiple level references,
    /// yielding a single result from an object
    /// </summary>
    [Test]
    public void TestYieldFromBlock ()
    {
      // {bbo:{bp=10.00 10.01 10.02 10.01 10.00 ap=10.02 10.03 10.03 10.02 10.01}
      // sprd=$bbo.ap -
      // $bbo.bp <=$sprd / $bbo.bp}
      // 0.00199999999999996 0.00199800199800196 0.000998003992015947 0.000999000999000978
      // 0.000999999999999979d

      runner.Reset ();
      RCBlock bp = new RCBlock (null, "bp", ":", new RCDouble (10.00, 10.01, 10.02, 10.01, 10.00));
      RCBlock ap = new RCBlock (bp, "ap", ":", new RCDouble (10.02, 10.03, 10.03, 10.02, 10.01));

      RCBlock bbo = new RCBlock (null, "bbo", ":", ap);
      RCBlock sprd = new RCBlock (bbo,
                                  "sprd",
                                  ":",
                                  runner.New ("-",
                                              new RCReference ("bbo.ap"),
                                              new RCReference ("bbo.bp")));
      RCBlock sprdpch = new RCBlock (sprd,
                                     "sprdbps",
                                     "<-",
                                     runner.New ("/",
                                                 new RCReference ("sprd"),
                                                 new RCReference ("bbo.bp")));

      RCValue val = runner.Run (sprdpch);
      RCVector<double> v = val as RCVector<double>;
      RCL.Kernel.Assert.IsNotNull (v);
      RCL.Kernel.Assert.AreEqual (.02 / 10.00, v[0]);
      RCL.Kernel.Assert.AreEqual (.02 / 10.01, v[1]);
      RCL.Kernel.Assert.AreEqual (.01 / 10.02, v[2]);
      RCL.Kernel.Assert.AreEqual (.01 / 10.01, v[3]);
      RCL.Kernel.Assert.AreEqual (.01 / 10.00, v[4]);
      RCL.Kernel.Assert.AreEqual (
        "0.00199999999999996 0.00199800199800196 0.000998003992015947 0.000999000999000978 0.000999999999999979",
        v.ToString ());
    }

    [Test]
    public void TestRawNames ()
    {
      runner.Reset ();
      RCBlock k = (RCBlock) RCSystem.Parse ("{'Set-Cookie':\"abcdefg\"}");
      RCL.Kernel.Assert.AreEqual ("'Set-Cookie'", k.GetName (0).Name);
      RCL.Kernel.Assert.AreEqual ("Set-Cookie", k.GetName (0).RawName);
    }

    [Test]
    public void TestEscapedNames ()
    {
      runner.Reset ();
      RCBlock k = new RCBlock (RCBlock.Empty, "foo-bar", ":", new RCString ("baz"));
      RCL.Kernel.Assert.AreEqual ("'foo-bar'", k.GetName (0).Name);
      RCL.Kernel.Assert.AreEqual ("foo-bar", k.GetName (0).RawName);
    }

    [Test]
    public void TestEscapedNumberNames ()
    {
      runner.Reset ();
      RCBlock k = new RCBlock (RCBlock.Empty, "0", ":", new RCString ("foo"));
      RCL.Kernel.Assert.AreEqual ("'0'", k.GetName (0).Name);
      RCL.Kernel.Assert.AreEqual ("0", k.GetName (0).RawName);
      RCL.Kernel.Assert.AreEqual ("{'0':\"foo\"}", k.Format (RCFormat.Default));
    }

    [Test]
    public void TestProgrammaticPartExtraction ()
    {
      bool fragment;
      RCSymbol symbol = (RCSymbol) RCSystem.Parse ("#0,'this-is-a-test'", out fragment);
      RCL.Kernel.Assert.AreEqual ("this-is-a-test", (string) symbol[0].Part (1));
    }

    [Test]
    public void TestIncrementalVectorParsing ()
    {
      runner.Reset ();
      RCValue vector0 = runner.Read ("1 2 3");
      RCValue vector1 = runner.Read ("4 5 6");
      RCL.Kernel.Assert.AreEqual ("1 2 3", vector0.ToString ());
      RCL.Kernel.Assert.AreEqual ("1 2 3 4 5 6", vector1.ToString ());
    }

    [Test]
    public void TestProgrammaticSymbolBuilding ()
    {
      RCL.Kernel.Assert.AreEqual ("#1,2,3",
                                  RCSymbolScalar.From ((long) 1,
                                                       (long) 2,
                                                       (long) 3).ToString ());
      RCL.Kernel.Assert.AreEqual ("#1,2,3",
                                  RCSymbolScalar.From ((long) 1,
                                                       (int) 2,
                                                       (long) 3).ToString ());
      RCL.Kernel.Assert.AreEqual ("#a,b,c", RCSymbolScalar.From ("a", "b", "c").ToString ());
      RCL.Kernel.Assert.AreEqual ("#'a-b',c,d", RCSymbolScalar.From ("a-b", "c", "d").ToString ());
      RCL.Kernel.Assert.AreEqual ("#a_b,c,d", RCSymbolScalar.From ("a_b", "c", "d").ToString ());
      RCL.Kernel.Assert.AreEqual ("#'1','2','3'", RCSymbolScalar.From ("1", "2", "3").ToString ());
    }

    [Test]
    public void TestProgrammaticSymbolBuildingEmpty ()
    {
      RCL.Kernel.Assert.AreEqual ("#", RCSymbolScalar.From ().ToString ());
    }

    [Test]
    public void TestProgrammaticSymbolBuildingWithIntFirst ()
    {
      RCL.Kernel.Assert.AreEqual ("#1,2,3",
                                  RCSymbolScalar.From ((int) 1,
                                                       (long) 2,
                                                       (long) 3).ToString ());
    }

    [Test]
    public void TestMacroCtor ()
    {
      runner.Reset ();
      NUnit.Framework.Assert.Throws<ArgumentNullException> (delegate ()
      {
        runner.New ("+", null, null);
      });
    }
  }
}

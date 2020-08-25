
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  [TestFixture]
  public class CoreTest2 : CoreTest
  {
    // Symbular Aggregations - xavg, xcount, xsum, xmax, xmed,

    // RCL read and write Operators

    // Generic Operators
    // Append
    [Test]
    public void TestAppendXX ()
    {
      DoTest ("\\x01 & \\x02", "\\x01 \\x02");
    }
    [Test]
    public void TestAppendLL ()
    {
      DoTest ("1 & 2", "1 2");
    }
    [Test]
    public void TestAppendDD ()
    {
      DoTest ("1.0 & 2.0", "1.0 2.0");
    }
    [Test]
    public void TestAppendMM ()
    {
      DoTest ("1m & 2m", "1 2m");
    }
    [Test]
    public void TestAppendBB ()
    {
      DoTest ("true & false", "true false");
    }
    [Test]
    public void TestAppendSS ()
    {
      DoTest ("\"x\" & \"y\"", "\"x\" \"y\"");
    }
    [Test]
    public void TestAppendYY ()
    {
      DoTest ("#x & #y", "#x #y");
    }
    [Test]
    public void TestAppendKK ()
    {
      DoTest ("{x:1.0} & {y:2.0}", "{x:1.0 y:2.0}");
    }
    [Test]
    public void TestAppendK0 ()
    {
      DoTest ("& {x:1.0 y:2.0 z:3.0}", "1.0 2.0 3.0");
    }
    [Test]
    public void TestAppendK1 ()
    {
      DoTest ("& {x:{a:1 b:2 c:3} y:{d:4 e:5 g:6}}", "{a:1 b:2 c:3 d:4 e:5 g:6}");
    }
    [Test]
    public void TestAppendK2 ()
    {
      DoTest ("& {:[x 1] :[x 2] :[x 3]}", "[x 1 2 3]");
    }
    [Test]
    public void TestAppendK3 ()
    {
      DoTest ("& {x:2015.05.22 y:2015.05.23 z:2015.05.24}", "2015.05.22 2015.05.23 2015.05.24");
    }
    [Test]
    public void TestAppendS ()
    {
      DoTest ("& \"a\" \"b\" \"c\"", "\"abc\"");
    }
    [Test]
    public void TestAppendT ()
    {
      DoTest ("2015.05.22 & 2015.05.23", "2015.05.22 2015.05.23");
    }
    [Test]
    public void TestAppendKMixed ()
    {
      DoTest ("& {:{:#this_is_a_symbol} :\"this is a string\" :{a:1 b:2} z:4}",
              "{:#this_is_a_symbol :\"this is a string\" a:1 b:2 z:4}");
    }

    [Test]
    public void TestPartY0 ()
    {
      DoTest ("0 part #a,b,c #d,e,f #g,h,i", "#a #d #g");
    }
    [Test]
    public void TestPartY1 ()
    {
      DoTest ("2 1 part #a,b,c #d,e,f #g,h,i", "#c,b #f,e #i,h");
    }
    [Test]
    public void TestPartY2 ()
    {
      DoTest ("0 -1 part #a,b,c #d,e,f #g,h,i", "#a,c #d,f #g,i");
    }
    [Test]
    public void TestPartY3 ()
    {
      DoTest ("-2 part #a,b,c #d,e,f #g,h,i", "#b #e #h");
    }

    [Test]
    public void TestPartsUntil1 ()
    {
      DoTest ("#a,b,c #d,e,f #g,h,i partsUntil 1", "#a,b #d,e #g,h");
    }
    [Test]
    public void TestPartsUntil2 ()
    {
      DoTest ("#a,b,c #d,e,f #g,h,i partsUntil 0 1 2", "#a #d,e #g,h,i");
    }
    [Test]
    public void TestPartsUntil3 ()
    {
      DoTest ("#a,b,c #d,e,f #g,h,i partsUntil -1", "# # #");
    }
    [Test]
    public void TestPartsAfter1 ()
    {
      DoTest ("#a,b,c #d,e,f #g,h,i partsAfter 1", "#c #f #i");
    }
    [Test]
    public void TestPartsAfter2 ()
    {
      DoTest ("#a,b,c #d,e,f #g,h,i partsAfter 0 1 2", "#b,c #f #");
    }

    // Thru
    [Test]
    public void TestToX ()
    {
      DoTest ("\\x00 to \\x02", "\\x00 \\x01 \\x02");
    }
    [Test]
    public void TestToL ()
    {
      DoTest ("0 to 2", "0 1 2");
    }
    [Test]
    public void TestToD ()
    {
      DoTest ("0.0 to 2.0", "0.0 1.0 2.0");
    }
    [Test]
    public void TestToM ()
    {
      DoTest ("0m to 2m", "0 1 2m");
    }

    // [Test]
    // public void TestAtCube() { DoTest ("{t:[S|i a #x 0l 10l #x 1l 20l #x 2l 30l] <-$t.a
    // at
    // $t.i}", "10 20 30l"); }
    [Test]
    public void TestAtXX ()
    {
      DoTest ("\\x00 \\x01 \\x02 at \\x01", "\\x01");
    }
    [Test]
    public void TestAtXL ()
    {
      DoTest ("\\x00 \\x01 \\x02 at 1", "\\x01");
    }
    [Test]
    public void TestAtXD ()
    {
      DoTest ("\\x00 \\x01 \\x02 at 1.0", "\\x01");
    }
    [Test]
    public void TestAtXM ()
    {
      DoTest ("\\x00 \\x01 \\x02 at 1m", "\\x01");
    }
    [Test]
    public void TestAtLL ()
    {
      DoTest ("0 1 2 at 1", "1");
    }
    [Test]
    public void TestAtLD ()
    {
      DoTest ("0 1 2 at 1.5", "1");
    }
    [Test]
    public void TestAtLM ()
    {
      DoTest ("0 1 2 at 1.5m", "1");
    }
    [Test]
    public void TestAtLX ()
    {
      DoTest ("0 1 2 at \\x01", "1");
    }
    [Test]
    public void TestAtLNeg ()
    {
      DoTest ("0 1 2 at 0 -1", "0 2");
    }
    [Test]
    public void TestAtDNeg ()
    {
      DoTest ("0.0 1.0 2.0 at 0.0 -1.0", "0.0 2.0");
    }
    [Test]
    public void TestAtMNeg ()
    {
      DoTest ("0 1 2m at 0 -1m", "0 2m");
    }
    [Test]
    public void TestAtDL ()
    {
      DoTest ("0.0 1.0 2.0 at 1", "1.0");
    }
    [Test]
    public void TestAtDD ()
    {
      DoTest ("0.0 1.0 2.0 at 1.5", "1.0");
    }
    [Test]
    public void TestAtDM ()
    {
      DoTest ("0.0 1.0 2.0 at 1.5m", "1.0");
    }
    [Test]
    public void TestAtDX ()
    {
      DoTest ("0.0 1.0 2.0 at \\x01", "1.0");
    }
    [Test]
    public void TestAtML ()
    {
      DoTest ("0 1 2m at 1", "1m");
    }
    [Test]
    public void TestAtMD ()
    {
      DoTest ("0 1 2m at 1.5", "1m");
    }
    [Test]
    public void TestAtMM ()
    {
      DoTest ("0 1 2m at 1.5m", "1m");
    }
    [Test]
    public void TestAtMX ()
    {
      DoTest ("0 1 2m at \\x01", "1m");
    }
    [Test]
    public void TestAtBL ()
    {
      DoTest ("true false true at 1", "false");
    }
    [Test]
    public void TestAtBD ()
    {
      DoTest ("true false true at 1.0", "false");
    }
    [Test]
    public void TestAtBM ()
    {
      DoTest ("true false true at 1m", "false");
    }
    [Test]
    public void TestAtBX ()
    {
      DoTest ("true false true at \\x01", "false");
    }
    [Test]
    public void TestAtSL ()
    {
      DoTest ("\"x\" \"y\" \"z\" at 1", "\"y\"");
    }
    [Test]
    public void TestAtSD ()
    {
      DoTest ("\"x\" \"y\" \"z\" at 1.0", "\"y\"");
    }
    [Test]
    public void TestAtSM ()
    {
      DoTest ("\"x\" \"y\" \"z\" at 1m", "\"y\"");
    }
    [Test]
    public void TestAtSX ()
    {
      DoTest ("\"x\" \"y\" \"z\" at \\x01", "\"y\"");
    }
    [Test]
    public void TestAtYL ()
    {
      DoTest ("#x #y #z at 1", "#y");
    }
    [Test]
    public void TestAtYD ()
    {
      DoTest ("#x #y #z at 1.0", "#y");
    }
    [Test]
    public void TestAtYM ()
    {
      DoTest ("#x #y #z at 1m", "#y");
    }
    [Test]
    public void TestAtYX ()
    {
      DoTest ("#x #y #z at \\x01", "#y");
    }
    [Test]
    public void TestAtTL ()
    {
      DoTest ("01:00 02:00 03:00 at 1", "02:00:00");
    }
    [Test]
    public void TestAtTD ()
    {
      DoTest ("01:00 02:00 03:00 at 1.0", "02:00:00");
    }
    [Test]
    public void TestAtTM ()
    {
      DoTest ("01:00 02:00 03:00 at 1m", "02:00:00");
    }
    [Test]
    public void TestAtTX ()
    {
      DoTest ("01:00 02:00 03:00 at \\x01", "02:00:00");
    }
    [Test]
    public void TestAtKL ()
    {
      DoTest ("{x:0 y:1 z:2} at 1", "{y:1}");
    }
    [Test]
    public void TestAtKD ()
    {
      DoTest ("{x:0 y:1 z:2} at 1.0", "{y:1}");
    }
    [Test]
    public void TestAtKM ()
    {
      DoTest ("{x:0 y:1 z:2} at 1m", "{y:1}");
    }
    [Test]
    public void TestAtKLNeg ()
    {
      DoTest ("{x:0 y:1 z:2} at -2", "{y:1}");
    }
    [Test]
    public void TestAtKDNeg ()
    {
      DoTest ("{x:0 y:1 z:2} at -2.0", "{y:1}");
    }
    [Test]
    public void TestAtKMNeg ()
    {
      DoTest ("{x:0 y:1 z:2} at -2m", "{y:1}");
    }
    [Test]
    public void TestAtKX ()
    {
      DoTest ("{x:0 y:1 z:2} at \\x01", "{y:1}");
    }
    [Test]
    public void TestAtKS ()
    {
      DoTest ("{x:0 y:1 z:2} at \"y\"", "{y:1}");
    }
    [Test]
    public void TestAtKY ()
    {
      DoTest ("{x:0 y:1 z:2} at #y", "{y:1}");
    }
    [Test]
    [Ignore ("because")]
    public void TestAtUL ()
    {
      DoTest ("[S|x y z #a 0 10 100] at 1", "10");
    }
    [Test]
    [Ignore ("because")]
    public void TestAtUD ()
    {
      DoTest ("[S|x y z #a 0 10 100] at 1.0", "10");
    }
    [Test]
    [Ignore ("because")]
    public void TestAtUM ()
    {
      DoTest ("[S|x y z #a 0 10 100] at 1m", "10");
    }
    [Test]
    [Ignore ("because")]
    public void TestAtUX ()
    {
      DoTest ("[S|x y z #a 0 10 100] at \\x01", "10");
    }
    [Test]
    [Ignore ("because")]
    public void TestAtUS ()
    {
      DoTest ("[S|x y z #a 0 10 100] at \"y\"", "10");
    }
    [Test]
    [Ignore ("because")]
    public void TestAtUY ()
    {
      DoTest ("[S|x y z #a 0 10 100] at #y", "10");
    }

    // From is just at with the indices on the left side.
    // [Test]
    // public void TestFromCube() { DoTest ("{t:[S|i a #x 0 10 #x 1 20 #x 2 30] <-$t.i
    // from $t.a}",
    // "10 20 30"); }
    [Test]
    public void TestFromXX ()
    {
      DoTest ("\\x01 from \\x00 \\x01 \\x02", "\\x01");
    }
    [Test]
    public void TestFromXL ()
    {
      DoTest ("\\x01 from 0 1 2", "1");
    }
    [Test]
    public void TestFromXD ()
    {
      DoTest ("\\x01 from 0.0 1.0 2.0", "1.0");
    }
    [Test]
    public void TestFromXM ()
    {
      DoTest ("\\x01 from 0 1 2m", "1m");
    }
    [Test]
    public void TestFromLL ()
    {
      DoTest ("1 from 0 1 2", "1");
    }
    [Test]
    public void TestFromLD ()
    {
      DoTest ("1 from 0.0 1.0 2.0", "1.0");
    }
    [Test]
    public void TestFromLM ()
    {
      DoTest ("1 from 0 1 2m", "1m");
    }

    [Test]
    public void TestFromLX ()
    {
      DoTest ("1 from \\x00 \\x01 \\x02", "\\x01");
    }
    [Test]
    public void TestFromLNeg ()
    {
      DoTest ("0 -1 from 0 1 2", "0 2");
    }
    [Test]
    public void TestFromDNeg ()
    {
      DoTest ("0.0 -1.0 from 0.0 1.0 2.0", "0.0 2.0");
    }
    [Test]
    public void TestFromMNeg ()
    {
      DoTest ("0 -1m from 0.0 1.0 2.0", "0.0 2.0");
    }
    [Test]
    public void TestFromDL ()
    {
      DoTest ("1.5 from 0 1 2", "1");
    }
    [Test]
    public void TestFromDD ()
    {
      DoTest ("1.5 from 0.0 1.0 2.0", "1.0");
    }
    [Test]
    public void TestFromDM ()
    {
      DoTest ("1.5 from 0 1 2m", "1m");
    }
    [Test]
    public void TestFromDX ()
    {
      DoTest ("1.5 from \\x00 \\x01 \\x02", "\\x01");
    }
    [Test]
    public void TestFromML ()
    {
      DoTest ("1.5m from 0 1 2", "1");
    }
    [Test]
    public void TestFromMD ()
    {
      DoTest ("1.5m from 0.0 1.0 2.0", "1.0");
    }
    [Test]
    public void TestFromMM ()
    {
      DoTest ("1.5m from 0 1 2m", "1m");
    }
    [Test]
    public void TestFromMX ()
    {
      DoTest ("1.5m from \\x00 \\x01 \\x02", "\\x01");
    }
    [Test]
    public void TestFromLB ()
    {
      DoTest ("1 from true false true", "false");
    }
    [Test]
    public void TestFromDB ()
    {
      DoTest ("1.0 from true false true", "false");
    }
    [Test]
    public void TestFromMB ()
    {
      DoTest ("1m from true false true", "false");
    }
    [Test]
    public void TestFromXB ()
    {
      DoTest ("\\x01 from true false true", "false");
    }
    [Test]
    public void TestFromLS ()
    {
      DoTest ("1 from \"x\" \"y\" \"z\"", "\"y\"");
    }
    [Test]
    public void TestFromDS ()
    {
      DoTest ("1.5 from \"x\" \"y\" \"z\"", "\"y\"");
    }
    [Test]
    public void TestFromMS ()
    {
      DoTest ("1.5m from \"x\" \"y\" \"z\"", "\"y\"");
    }
    [Test]
    public void TestFromXS ()
    {
      DoTest ("\\x01 from \"x\" \"y\" \"z\"", "\"y\"");
    }
    [Test]
    public void TestFromLY ()
    {
      DoTest ("1 from #x #y #z", "#y");
    }
    [Test]
    public void TestFromDY ()
    {
      DoTest ("1.0 from #x #y #z", "#y");
    }
    [Test]
    public void TestFromMY ()
    {
      DoTest ("1m from #x #y #z", "#y");
    }
    [Test]
    public void TestFromXY ()
    {
      DoTest ("\\x01 from #x #y #z", "#y");
    }
    [Test]
    public void TestFromTL ()
    {
      DoTest ("1 from 01:00 02:00 03:00", "02:00:00");
    }
    [Test]
    public void TestFromTD ()
    {
      DoTest ("1.0 from 01:00 02:00 03:00", "02:00:00");
    }
    [Test]
    public void TestFromTM ()
    {
      DoTest ("1m from 01:00 02:00 03:00", "02:00:00");
    }
    [Test]
    public void TestFromTX ()
    {
      DoTest ("\\x01 from 01:00 02:00 03:00", "02:00:00");
    }
    [Test]
    public void TestFromLK ()
    {
      DoTest ("1 from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromDK ()
    {
      DoTest ("1.0 from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromMK ()
    {
      DoTest ("1m from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromLKNeg ()
    {
      DoTest ("-2 from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromDKNeg ()
    {
      DoTest ("-2.0 from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromMKNeg ()
    {
      DoTest ("-2m from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromXK ()
    {
      DoTest ("\\x01 from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromYK ()
    {
      DoTest ("#y from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromYK1 ()
    {
      DoTest ("#'a b c' from {'a b c':1}", "{'a b c':1}");
    }
    [Test]
    public void TestFromYK2 ()
    {
      DoTest ("#a,c #e,f from {a:{b:1 c:2 d:3} e:{f:4 g:5}}", "{c:2 f:4}");
    }
    [Test]
    public void TestFromSK ()
    {
      DoTest ("\"y\" from {x:0 y:1 z:2}", "{y:1}");
    }
    [Test]
    public void TestFromSK1 ()
    {
      DoTest ("\"a b c\" from {'a b c':1}", "{'a b c':1}");
    }
    [Test]
    public void TestFromSK2 ()
    {
      DoTest ("\"'a b c'\" from {'a b c':1}", "{'a b c':1}");
    }
    [Test]
    public void TestFromSKNameError ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("\"a\" \"b\" \"c\" from {a:1 b:2}", "{}");
      });
    }
    [Test]
    public void TestFromYKNameError ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("#a #b #c from {a:1 b:2}", "{}");
      });
    }

    [Test]
    public void TestFirstK ()
    {
      DoTest ("first {:0 :1 :2}", "0");
    }
    [Test]
    public void TestFirstX ()
    {
      DoTest ("first \\x00 \\x01 \\x02", "\\x00");
    }
    [Test]
    public void TestFirstL ()
    {
      DoTest ("first 0 1 2", "0");
    }
    [Test]
    public void TestFirstD ()
    {
      DoTest ("first 0.0 1.0 2.0", "0.0");
    }
    [Test]
    public void TestFirstM ()
    {
      DoTest ("first 0 1 2m", "0m");
    }
    [Test]
    public void TestFirstB ()
    {
      DoTest ("first true false true", "true");
    }
    [Test]
    public void TestFirstY ()
    {
      DoTest ("first #a #b #c", "#a");
    }
    [Test]
    public void TestFirstS ()
    {
      DoTest ("first \"a\" \"b\" \"c\"", "\"a\"");
    }
    [Test]
    public void TestFirstI ()
    {
      DoTest ("first ++ ++ ++", "++");
    }
    [Test]
    public void TestFirstT ()
    {
      DoTest ("first 2018.09.04 2018.09.05 2018.09.06", "2018.09.04");
    }

    [Test]
    public void TestRestK ()
    {
      DoTest ("rest {:0 :1 :2}", "{:1 :2}");
    }
    [Test]
    public void TestRestX ()
    {
      DoTest ("rest \\x00 \\x01 \\x02", "\\x01 \\x02");
    }
    [Test]
    public void TestRestL ()
    {
      DoTest ("rest 0 1 2", "1 2");
    }
    [Test]
    public void TestRestD ()
    {
      DoTest ("rest 0.0 1.0 2.0", "1.0 2.0");
    }
    [Test]
    public void TestRestM ()
    {
      DoTest ("rest 0 1 2m", "1 2m");
    }
    [Test]
    public void TestRestB ()
    {
      DoTest ("rest true false true", "false true");
    }
    [Test]
    public void TestRestY ()
    {
      DoTest ("rest #a #b #c", "#b #c");
    }
    [Test]
    public void TestRestS ()
    {
      DoTest ("rest \"a\" \"b\" \"c\"", "\"b\" \"c\"");
    }
    [Test]
    public void TestRestI ()
    {
      DoTest ("rest ++ ++ ++", "++ ++");
    }
    [Test]
    public void TestRestT ()
    {
      DoTest ("rest 2018.09.04 2018.09.05 2018.09.06", "2018.09.05 2018.09.06");
    }

    [Test]
    public void TestLast ()
    {
      DoTest ("last {:0 :1 :2}", "{:2}");
    }
    [Test]
    public void TestLastX ()
    {
      DoTest ("last \\x00 \\x01 \\x02", "\\x02");
    }
    [Test]
    public void TestLastL ()
    {
      DoTest ("last 0 1 2", "2");
    }
    [Test]
    public void TestLastD ()
    {
      DoTest ("last 0.0 1.0 2.0", "2.0");
    }
    [Test]
    public void TestLastM ()
    {
      DoTest ("last 0m 1m 2m", "2m");
    }
    [Test]
    public void TestLastB ()
    {
      DoTest ("last true false false", "false");
    }
    [Test]
    public void TestLastY ()
    {
      DoTest ("last #a #b #c", "#c");
    }
    [Test]
    public void TestLastS ()
    {
      DoTest ("last \"a\" \"b\" \"c\"", "\"c\"");
    }
    [Test]
    public void TestLastI ()
    {
      DoTest ("last ++ ++ +-", "+-");
    }
    [Test]
    public void TestLastT ()
    {
      DoTest ("last 2018.09.04 2018.09.05 2018.09.06", "2018.09.06");
    }

    [Test]
    public void TestPop ()
    {
      DoTest ("pop {:0 :1 :2}", "{:0 :1}");
    }
    [Test]
    public void TestPopX ()
    {
      DoTest ("pop \\x00 \\x01 \\x02", "\\x00 \\x01");
    }
    [Test]
    public void TestPopL ()
    {
      DoTest ("pop 0 1 2", "0 1");
    }
    [Test]
    public void TestPopD ()
    {
      DoTest ("pop 0.0 1.0 2.0", "0.0 1.0");
    }
    [Test]
    public void TestPopM ()
    {
      DoTest ("pop 0m 1m 2m", "0 1m");
    }
    [Test]
    public void TestPopB ()
    {
      DoTest ("pop true false false", "true false");
    }
    [Test]
    public void TestPopY ()
    {
      DoTest ("pop #a #b #c", "#a #b");
    }
    [Test]
    public void TestPopS ()
    {
      DoTest ("pop \"a\" \"b\" \"c\"", "\"a\" \"b\"");
    }
    [Test]
    public void TestPopI ()
    {
      DoTest ("pop ++ ++ +-", "++ ++");
    }
    [Test]
    public void TestPopT ()
    {
      DoTest ("pop 2018.09.04 2018.09.05 2018.09.06", "2018.09.04 2018.09.05");
    }

    [Test]
    public void TestUnwrap ()
    {
      DoTest ("unwrap {:0}", "0");
    }
    [Test]
    public void TestUnwrapEx ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoRawTest (runner, RCFormat.DefaultNoT, "unwrap {:0 :1 :2}", "{}");
      });
    }

    // Oh no! What if find doesn't find anything.  How do I represent the result?
    // I have been avoiding this problem for a while... Let's think...
    // How about ~l ~d ~s, ok. That's not bad.
    // But the same problem exists with cubes, what can I do there?
    // [S|x~l y~s z~m]
    // Really not that bad, I kind of like it.  Shouldn't be too hard to implement.
    // Update: an empty cube with no observations is just []
    // so the new syntax is only required for the vector case.
    [Test]
    public void TestFindB ()
    {
      DoTest ("find true false true", "0 2");
    }
    [Test]
    public void TestFindBB ()
    {
      DoTest ("false find true false true", "1");
    }
    [Test]
    public void TestFindLL ()
    {
      DoTest ("0 find 0 1 2", "0");
    }
    [Test]
    public void TestFindDD ()
    {
      DoTest ("0.0 find 0.0 1.0 2.0", "0");
    }
    [Test]
    public void TestFindMM ()
    {
      DoTest ("0m find 0 1 2m", "0");
    }
    [Test]
    public void TestFindXX ()
    {
      DoTest ("\\x00 find \\x00 \\x01 \\x02", "0");
    }
    [Test]
    public void TestFindSS ()
    {
      DoTest ("\"y\" find \"x\" \"y\" \"z\"", "1");
    }
    [Test]
    public void TestFindYY ()
    {
      DoTest ("#y find #x #y #z", "1");
    }
    [Test]
    public void TestFindTT ()
    {
      DoTest ("2015.05.27 find 2015.05.26 2015.05.27 2015.05.28", "1");
    }

    [Test]
    public void TestFindBEmpty ()
    {
      DoTest ("find false false false", "~l");
    }
    [Test]
    public void TestFindBBEmpty ()
    {
      DoTest ("false find true true true", "~l");
    }
    [Test]
    public void TestFindLLEmpty ()
    {
      DoTest ("3 find 0 1 2", "~l");
    }
    [Test]
    public void TestFindDDEmpty ()
    {
      DoTest ("3.0 find 0.0 1.0 2.0", "~l");
    }
    [Test]
    public void TestFindMMEmpty ()
    {
      DoTest ("3m find 0 1 2m", "~l");
    }
    [Test]
    public void TestFindXXEmpty ()
    {
      DoTest ("\\x03 find \\x00 \\x01 \\x02", "~l");
    }
    [Test]
    public void TestFindSSEmpty ()
    {
      DoTest ("\"a\" find \"x\" \"y\" \"z\"", "~l");
    }
    [Test]
    public void TestFindYYEmpty ()
    {
      DoTest ("#a find #x #y #z", "~l");
    }
    [Test]
    public void TestFindTTEmpty ()
    {
      DoTest ("2015.05.27 find 2015.05.26 2015.05.28 2015.05.29", "~l");
    }

    [Test]
    public void TestWhereLB ()
    {
      DoTest ("1 2 3 where true false true", "1 3");
    }
    [Test]
    public void TestWhereDB ()
    {
      DoTest ("1.0 2.0 3.0 where true false true", "1.0 3.0");
    }
    [Test]
    public void TestWhereMB ()
    {
      DoTest ("1.0 2.0 3.0m where true false true", "1 3m");
    }
    [Test]
    public void TestWhereXB ()
    {
      DoTest ("\\x01 \\x02 \\x03 where true false true", "\\x01 \\x03");
    }
    [Test]
    public void TestWhereYB ()
    {
      DoTest ("#x #y #z where true false true", "#x #z");
    }
    [Test]
    public void TestWhereNB ()
    {
      DoTest ("++ ++ ++ where true false true", "++ ++");
    }
    [Test]
    public void TestWhereSB ()
    {
      DoTest ("\"x\" \"y\" \"z\" where true false true", "\"x\" \"z\"");
    }
    [Test]
    public void TestWhereTB ()
    {
      DoTest ("2015.05.27 2015.05.28 2015.05.29 where true false true", "2015.05.27 2015.05.29");
    }
    [Test]
    public void TestWhereKB ()
    {
      DoTest ("{a:{x:1} b:{y:1} c:{z:1}} where true false true", "{a:{x:1} c:{z:1}}");
    }
    [Test]
    public void TestWhereKK ()
    {
      DoTest (
        "{a:{x:1} b:{x:2} c:{x:3} d:{x:4}} where {a:{x:true} b:{x:true} c:{x:false} d:{x:false}}",
        "{a:{x:1} b:{x:2}}");
    }
    [Test]
    public void TestWhereKK1 ()
    {
      DoTest ("{a:{x:1 y:2 z:3}} where {a:false true false}", "{a:{y:2}}");
    }
    [Test]
    public void TestWhereKK2 ()
    {
      DoTest ("{k:{a:{x:1 y:2 z:3}} <-$k where {<-\"y\" == names $R} each $k}", "{a:{y:2}}");
    }
    [Test]
    public void TestWhereKK3 ()
    {
      DoTest ("{a:{x:1 y:2 z:3}} where {a:{x:false y:true z:false}}", "{a:{y:2}}");
    }
    [Test]
    public void TestWhereKK4 ()
    {
      DoTest ("{k:{a:{x:1 y:2 z:3}} <-$k where {<-{<-($L == \"y\")} each $R} each $k}",
              "{a:{y:2}}");
    }
    [Test]
    public void TestWhereB ()
    {
      DoTest ("where true true false true false true", "0 1 3 5");
    }

    [Test]
    public void TestSortAscL ()
    {
      DoTest ("#asc sort 2 0 1", "0 1 2");
    }
    [Test]
    public void TestSortAscD ()
    {
      DoTest ("#asc sort 2.0 0.0 1.0", "0.0 1.0 2.0");
    }
    [Test]
    public void TestSortAscM ()
    {
      DoTest ("#asc sort 2 0 1m", "0 1 2m");
    }
    [Test]
    public void TestSortAscX ()
    {
      DoTest ("#asc sort \\x02 \\x00 \\x01", "\\x00 \\x01 \\x02");
    }
    [Test]
    public void TestSortAscB ()
    {
      DoTest ("#asc sort true false true", "false true true");
    }
    [Test]
    public void TestSortAscS ()
    {
      DoTest ("#asc sort \"c\" \"b\" \"a\"", "\"a\" \"b\" \"c\"");
    }
    [Test]
    public void TestSortAscY ()
    {
      DoTest ("#asc sort #b #c #a", "#a #b #c");
    }
    [Test]
    public void TestSortAscY1 ()
    {
      DoTest ("#asc sort #1,a #0,y #1,c #0,x #1,b", "#0,x #0,y #1,a #1,b #1,c");
    }
    [Test]
    public void TestSortAscT ()
    {
      DoTest ("#asc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.24 2015.05.25 2015.05.26");
    }
    [Test]
    public void TestSortAscKX ()
    {
      DoTest ("#asc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}", "{x:\\x00 \\x01 \\x02 y:#c #a #b}");
    }
    [Test]
    public void TestSortAscKL ()
    {
      DoTest ("#asc,x sort {x:1 2 0 y:#a #b #c}", "{x:0 1 2 y:#c #a #b}");
    }

    [Test]
    public void TestSortAbsAscL ()
    {
      DoTest ("#absasc sort 1 -2 0", "0 1 -2");
    }
    [Test]
    public void TestSortAbsAscD ()
    {
      DoTest ("#absasc sort 1.0 -2.0 0.0", "0.0 1.0 -2.0");
    }
    [Test]
    public void TestSortAbsAscM ()
    {
      DoTest ("#absasc sort 1 -2 0m", "0 1 -2m");
    }
    [Test]
    public void TestSortAbsAscX ()
    {
      DoTest ("#absasc sort \\x01 \\x02 \\x00", "\\x00 \\x01 \\x02");
    }
    [Test]
    public void TestSortAbsAscB ()
    {
      DoTest ("#absasc sort true false true", "false true true");
    }
    [Test]
    public void TestSortAbsAscS ()
    {
      DoTest ("#absasc sort \"c\" \"b\" \"a\"", "\"a\" \"b\" \"c\"");
    }
    [Test]
    public void TestSortAbsAscY ()
    {
      DoTest ("#absasc sort #b #c #a", "#a #b #c");
    }
    [Test]
    public void TestSortAbsAscT ()
    {
      DoTest ("#absasc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.24 2015.05.25 2015.05.26");
    }
    [Test]
    public void TestSortAbsAscKX ()
    {
      DoTest ("#absasc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}",
              "{x:\\x00 \\x01 \\x02 y:#c #a #b}");
    }
    [Test]
    public void TestSortAbsAscKL ()
    {
      DoTest ("#absasc,x sort {x:1 -2 0 y:#a #b #c}", "{x:0 1 -2 y:#c #a #b}");
    }

    [Test]
    public void TestSortDescL ()
    {
      DoTest ("#desc sort 2 0 1", "2 1 0");
    }
    [Test]
    public void TestSortDescD ()
    {
      DoTest ("#desc sort 2.0 0.0 1.0", "2.0 1.0 0.0");
    }
    [Test]
    public void TestSortDescM ()
    {
      DoTest ("#desc sort 2 0 1m", "2 1 0m");
    }
    [Test]
    public void TestSortDescX ()
    {
      DoTest ("#desc sort \\x02 \\x00 \\x01", "\\x02 \\x01 \\x00");
    }
    [Test]
    public void TestSortDescB ()
    {
      DoTest ("#desc sort true false true", "true true false");
    }
    [Test]
    public void TestSortDescS ()
    {
      DoTest ("#desc sort \"c\" \"b\" \"a\"", "\"c\" \"b\" \"a\"");
    }
    [Test]
    public void TestSortDescY ()
    {
      DoTest ("#desc sort #b #c #a", "#c #b #a");
    }
    [Test]
    public void TestSortDescT ()
    {
      DoTest ("#desc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.26 2015.05.25 2015.05.24");
    }
    [Test]
    public void TestSortDescKX ()
    {
      DoTest ("#desc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}", "{x:\\x02 \\x01 \\x00 y:#b #a #c}");
    }
    [Test]
    public void TestSortDescKL ()
    {
      DoTest ("#desc,x sort {x:2 0 1 y:#a #b #c}", "{x:2 1 0 y:#a #c #b}");
    }
    [Test]
    public void TestSortDescKT ()
    {
      DoTest ("#desc,x sort {x:2 0 1 y:2015.05.25 2015.05.26 2015.05.27}",
              "{x:2 1 0 y:2015.05.25 2015.05.27 2015.05.26}");
    }

    [Test]
    public void TestSortAbsDescL ()
    {
      DoTest ("#absdesc sort 1 -2 0", "-2 1 0");
    }
    [Test]
    public void TestSortAbsDescD ()
    {
      DoTest ("#absdesc sort 1.0 -2.0 0.0", "-2.0 1.0 0.0");
    }
    [Test]
    public void TestSortAbsDescM ()
    {
      DoTest ("#absdesc sort 1 -2 0m", "-2 1 0m");
    }
    [Test]
    public void TestSortAbsDescX ()
    {
      DoTest ("#absdesc sort \\x01 \\x02 \\x00", "\\x02 \\x01 \\x00");
    }
    [Test]
    public void TestSortAbsDescB ()
    {
      DoTest ("#absdesc sort true false true", "true true false");
    }
    [Test]
    public void TestSortAbsDescS ()
    {
      DoTest ("#absdesc sort \"c\" \"b\" \"a\"", "\"c\" \"b\" \"a\"");
    }
    [Test]
    public void TestSortAbsDescY ()
    {
      DoTest ("#absdesc sort #b #c #a", "#c #b #a");
    }
    [Test]
    public void TestSortAbsDescT ()
    {
      DoTest ("#absdesc sort 2015.05.25 2015.05.24 2015.05.26", "2015.05.26 2015.05.25 2015.05.24");
    }
    [Test]
    public void TestSortAbsDescKX ()
    {
      DoTest ("#absdesc,x sort {x:\\x01 \\x02 \\x00 y:#a #b #c}",
              "{x:\\x02 \\x01 \\x00 y:#b #a #c}");
    }
    [Test]
    public void TestSortAbsDescKL ()
    {
      DoTest ("#absdesc,x sort {x:1 -2 0 y:#a #b #c}", "{x:-2 1 0 y:#b #a #c}");
    }

    [Test]
    public void TestRankAscL ()
    {
      DoTest ("#asc rank 2 0 1", "1 2 0");
    }
    [Test]
    public void TestRankAscD ()
    {
      DoTest ("#asc rank 2.0 0.0 1.0", "1 2 0");
    }
    [Test]
    public void TestRankAscM ()
    {
      DoTest ("#asc rank 2 0 1m", "1 2 0");
    }
    [Test]
    public void TestRankAscX ()
    {
      DoTest ("#asc rank \\x02 \\x00 \\x01", "1 2 0");
    }
    [Test]
    public void TestRankAscB ()
    {
      DoTest ("#asc rank true false true", "1 0 2");
    }
    [Test]
    public void TestRankAscS ()
    {
      DoTest ("#asc rank \"c\" \"b\" \"a\"", "2 1 0");
    }
    [Test]
    [Ignore ("because")]
    public void TestRankAscY ()
    {
      DoTest ("#asc rank #b #c #a", "2 0 1");
    }
    [Test]
    public void TestRankAscT ()
    {
      DoTest ("#asc rank 2015.05.25 2015.05.26 2015.05.24", "2 0 1");
    }

    [Test]
    public void TestRankAbsAscL ()
    {
      DoTest ("#absasc rank 1 -2 0", "2 0 1");
    }
    [Test]
    public void TestRankAbsAscD ()
    {
      DoTest ("#absasc rank 1.0 -2.0 0.0", "2 0 1");
    }
    [Test]
    public void TestRankAbsAscM ()
    {
      DoTest ("#absasc rank 1 -2 0m", "2 0 1");
    }
    [Test]
    public void TestRankAbsAscX ()
    {
      DoTest ("#absasc rank \\x01 \\x02 \\x00", "2 0 1");
    }
    [Test]
    public void TestRankAbsAscB ()
    {
      DoTest ("#absasc rank true false true", "1 0 2");
    }
    [Test]
    public void TestRankAbsAscS ()
    {
      DoTest ("#absasc rank \"c\" \"b\" \"a\"", "2 1 0");
    }
    [Test]
    [Ignore ("because")]
    public void TestRankAbsAscY ()
    {
      DoTest ("#absasc rank #b #c #a", "2 0 1");
    }
    [Test]
    public void TestRankAbsAscT ()
    {
      DoTest ("#absasc rank 2015.05.25 2015.05.26 2015.05.24", "2 0 1");
    }

    [Test]
    public void TestRankDescL ()
    {
      DoTest ("#desc rank 2 0 1", "0 2 1");
    }
    [Test]
    public void TestRankDescD ()
    {
      DoTest ("#desc rank 2.0 0.0 1.0", "0 2 1");
    }
    [Test]
    public void TestRankDescM ()
    {
      DoTest ("#desc rank 2 0 1m", "0 2 1");
    }
    [Test]
    public void TestRankDescX ()
    {
      DoTest ("#desc rank \\x02 \\x00 \\x01", "0 2 1");
    }
    [Test]
    public void TestRankDescB ()
    {
      DoTest ("#desc rank true false true", "0 2 1");
    }
    [Test]
    public void TestRankDescS ()
    {
      DoTest ("#desc rank \"c\" \"b\" \"a\"", "0 1 2");
    }
    [Test]
    [Ignore ("because")]
    public void TestRankDescY ()
    {
      DoTest ("#desc rank #b #c #a", "1 0 2");
    }
    [Test]
    public void TestRankDescT ()
    {
      DoTest ("#desc rank 2015.05.25 2015.05.26 2015.05.24", "1 0 2");
    }

    [Test]
    public void TestRankAbsDescL ()
    {
      DoTest ("#absdesc rank 1 -2 0", "1 0 2");
    }
    [Test]
    public void TestRankAbsDescD ()
    {
      DoTest ("#absdesc rank 1.0 -2.0 0.0", "1 0 2");
    }
    [Test]
    public void TestRankAbsDescM ()
    {
      DoTest ("#absdesc rank 1 -2 0m", "1 0 2");
    }
    [Test]
    public void TestRankAbsDescX ()
    {
      DoTest ("#absdesc rank \\x01 \\x02 \\x00", "1 0 2");
    }
    [Test]
    public void TestRankAbsDescB ()
    {
      DoTest ("#absdesc rank true false true", "0 2 1");
    }
    [Test]
    public void TestRankAbsDescS ()
    {
      DoTest ("#absdesc rank \"c\" \"b\" \"a\"", "0 1 2");
    }
    [Test]
    [Ignore ("because")]
    public void TestRankAbsDescY ()
    {
      DoTest ("#absdesc rank #b #c #a", "1 0 2");
    }
    [Test]
    public void TestRankAbsDescT ()
    {
      DoTest ("#absdesc rank 2015.05.25 2015.05.26 2015.05.24", "1 0 2");
    }
    // Identicalness - this is not like the others and needs good tests.

    // Math Operators - Should do a statistical test.
#if (!__MonoCS__)
    [Test]
    public void TestRandomdSeed ()
    {
      DoTest ("0 randomd 3", "0.72624326996796 0.817325359590969 0.768022689394663");
    }
#endif
    [Test]
    public void TestRandomD ()
    {
      DoTest ("3==count randomd 3", "true");
    }
#if (!__MonoCS__)
    [Test]
    public void TestRandomlSeed ()
    {
      DoTest ("0 randoml 3 0 10", "7 8 7");
    }
#endif
    [Test]
    public void TestRandomL ()
    {
      DoTest ("{v:randoml 3 0 10 <-(3 == count $v) and (0 <= low $v) and 10 >= high $v}", "true");
    }

    // These tests only check that each overload works and that the count is correct.
    // We should have tests for the statistical distribution of the results as well.
    [Test]
    public void TestShuffleL ()
    {
      DoTest ("count shuffle 0 1 2", "3");
    }
    [Test]
    public void TestShuffleLL ()
    {
      DoTest ("count 0 shuffle 0 1 2", "3");
    }
    [Test]
    public void TestShuffleD ()
    {
      DoTest ("count shuffle 0.0 1.0 2.0", "3");
    }
    [Test]
    public void TestShuffleLD ()
    {
      DoTest ("count 0 shuffle 0.0 1.0 2.0", "3");
    }
    [Test]
    public void TestShuffleM ()
    {
      DoTest ("count shuffle 0 1 2m", "3");
    }
    [Test]
    public void TestShuffleLM ()
    {
      DoTest ("count 0 shuffle 0 1 2m", "3");
    }
    [Test]
    public void TestShuffleX ()
    {
      DoTest ("count shuffle \\x00 \\x01 \\x02", "3");
    }
    [Test]
    public void TestShuffleLX ()
    {
      DoTest ("count 0 shuffle \\x00 \\x01 \\x02", "3");
    }
    [Test]
    public void TestShuffleS ()
    {
      DoTest ("count shuffle \"x\" \"y\" \"z\"", "3");
    }
    [Test]
    public void TestShuffleLS ()
    {
      DoTest ("count 0 shuffle \"x\" \"y\" \"z\"", "3");
    }
    [Test]
    public void TestShuffleB ()
    {
      DoTest ("count shuffle true false true", "3");
    }
    [Test]
    public void TestShuffleLB ()
    {
      DoTest ("count 0 shuffle true false true", "3");
    }
    [Test]
    public void TestShuffleY ()
    {
      DoTest ("count shuffle #x #y #z", "3");
    }
    [Test]
    public void TestShuffleT ()
    {
      DoTest ("count shuffle 2015.05.25 2015.05.26 2015.05.27", "3");
    }
    [Test]
    public void TestShuffleLT ()
    {
      DoTest ("count 0 shuffle 2015.05.25 2015.05.26 2015.05.27", "3");
    }
    [Test]
    public void TestShuffleK ()
    {
      DoTest ("count shuffle {x:0 y:1 z:2}", "3");
    }
    [Test]
    public void TestShuffleLK ()
    {
      DoTest ("count 0 shuffle {x:0 y:1 z:2}", "3");
    }

    [Test]
    public void TestAbsL ()
    {
      DoTest ("abs -1 0 1", "1 0 1");
    }
    [Test]
    public void TestAbsD ()
    {
      DoTest ("abs -1.0 0.0 1.0", "1.0 0.0 1.0");
    }
    [Test]
    public void TestAbsM ()
    {
      DoTest ("abs -1 0 1m", "1 0 1m");
    }
    // No byte overload cause there are no signed bytes.

    [Test]
    public void TestUniqueL ()
    {
      DoTest ("unique 3 2 1 3 2 4", "3 2 1 4");
    }
    [Test]
    public void TestUniqueD ()
    {
      DoTest ("unique 3.0 2.0 1.0 3.0 2.0 4.0", "3.0 2.0 1.0 4.0");
    }
    [Test]
    public void TestUniqueM ()
    {
      DoTest ("unique 3 2 1 3 2 4m", "3 2 1 4m");
    }
    [Test]
    public void TestUniqueX ()
    {
      DoTest ("unique \\x03 \\x02 \\x01 \\x03 \\x02 \\x04", "\\x03 \\x02 \\x01 \\x04");
    }
    [Test]
    public void TestUniqueS ()
    {
      DoTest ("unique \"3\" \"2\" \"1\" \"4\"", "\"3\" \"2\" \"1\" \"4\"");
    }
    [Test]
    public void TestUniqueB ()
    {
      DoTest ("unique true false true true", "true false");
    }
    [Test]
    public void TestUniqueY ()
    {
      DoTest ("unique #x #y #z #y", "#x #y #z");
    }
    [Test]
    public void TestUniqueT ()
    {
      DoTest ("unique 2015.05.25 2015.05.26 2015.05.27 2015.05.26",
              "2015.05.25 2015.05.26 2015.05.27");
    }
    [Test]
    public void TestUniqueK ()
    {
      DoTest ("unique {a:{x:1 y:2} b:{x:10 y:20} a:{x:100 y:200}}",
              "{a:{x:100 y:200} b:{x:10 y:20}}");
    }

    [Test]
    public void TestMapL ()
    {
      DoTest ("1 10 2 20 map 1 1 2 2 1 3", "10 10 20 20 10 3");
    }
    [Test]
    public void TestMapD ()
    {
      DoTest ("1.0 10.0 2.0 20.0 map 1.0 1.0 2.0 2.0 1.0 3.0", "10.0 10.0 20.0 20.0 10.0 3.0");
    }
    [Test]
    public void TestMapM ()
    {
      DoTest ("1 10 2 20m map 1 1 2 2 1 3m", "10 10 20 20 10 3m");
    }
    [Test]
    public void TestMapX ()
    {
      DoTest ("\\x01 \\x10 \\x02 \\x20 map \\x01 \\x01 \\x02 \\x02 \\x01 \\x03",
              "\\x10 \\x10 \\x20 \\x20 \\x10 \\x03");
    }
    [Test]
    public void TestMapS ()
    {
      DoTest ("\"1\" \"10\" \"2\" \"20\" map \"1\" \"1\" \"2\" \"2\" \"1\" \"3\"",
              "\"10\" \"10\" \"20\" \"20\" \"10\" \"3\"");
    }
    [Test]
    public void TestMapB ()
    {
      DoTest ("true false false true map true true false false true",
              "false false true true false");
    }
    [Test]
    public void TestMapY ()
    {
      DoTest ("#x #y #1 #10 map #x #x #y #10 #1 #1", "#y #y #y #10 #10 #10");
    }
    [Test]
    public void TestMapT ()
    {
      DoTest (
        "2015.05.25 2015.06.25 2015.05.26 2015.06.26 map 2015.05.25 2015.05.25 2015.06.25 2015.06.26 2015.05.26 2015.05.26",
        "2015.06.25 2015.06.25 2015.06.25 2015.06.26 2015.06.26 2015.06.26");
    }

    // Flow Control Operators
    [Test]
    public void TestSleepL ()
    {
      DoTest ("{t0:now{} :sleep 100 t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true");
    }
    [Test]
    public void TestSleepD ()
    {
      DoTest ("{t0:now{} :sleep 100.0 t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true");
    }
    [Test]
    public void TestSleepM ()
    {
      DoTest ("{t0:now{} :sleep 100m t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true");
    }
    [Test]
    public void TestSleepX ()
    {
      DoTest ("{t0:now{} :sleep \\x64 t1:now{} <-0.00:00:00.100000<$t1-$t0}", "true");
    }

    // The defining characteristic of sweach is that it takes a bunch of code blocks on
    // the right
    // and evaluates the correct ones based on the argument on the left.  Other than that
    // it is much
    // like from.
    // Switch is like sweach except that you promise to only have one element on the left
    // and gives you a single value back rather than a block of results.
    [Test]
    public void TestSwitchBK0 ()
    {
      DoTest ("false switch {:1+2 :1+3}", "4");
    }
    [Test]
    public void TestSwitchBK1 ()
    {
      DoTest ("false switch {:1+2}", "{}");
    }
    [Test]
    public void TestSwitchLK0 ()
    {
      DoTest ("-1 switch {:1+2 :1+3}", "4");
    }
    [Test]
    public void TestSwitchLK1 ()
    {
      DoTest ("0 switch {:1+2}", "3");
    }

    [Test]
    public void TestSwitchLK2 ()
    {
      DoTest ("-1 switch {:1+2}", "3");
    }
    [Test]
    public void TestSwitchLK3 ()
    {
      DoTest ("2 switch {:0 :1 :2 :3}", "2");
    }
    [Test]
    public void TestSwitchXK ()
    {
      DoTest ("\\x00 switch {:1+2}", "3");
    }
    [Test]
    public void TestSwitchYK0 ()
    {
      DoTest ("#b switch {c:#x b:#y a:#z}", "#y");
    }
    [Test]
    public void TestSwitchYK1 ()
    {
      DoTest ("#c switch {b:#y a:#z}", "{}");
    }
    // [Test]
    // public void TestSweachInTake() { DoTest("{<-#lock take {<-true sweach {:0l :1l}}}",
    // "{:0l}");
    // }
    [Test]
    public void TestSwitchYKWithQuote ()
    {
      DoTest ("#a switch {a::1 + 1 b::2 + 2}", "1 + 1");
    }
    [Test]
    public void TestSwitchSKWithQuote ()
    {
      DoTest ("\"b\" switch {a::1 + 1 b::2 + 2}", "2 + 2");
    }

    // Each for blocks
    [Test]
    public void TestEachL ()
    {
      DoTest ("{<-$R} each 0 to 4", "{:0 :1 :2 :3 :4}");
    }
    [Test]
    public void TestEachD ()
    {
      DoTest ("{<-$R} each 0.0 to 4.0", "{:0.0 :1.0 :2.0 :3.0 :4.0}");
    }
    [Test]
    public void TestEachM ()
    {
      DoTest ("{<-$R} each 0m to 4m", "{:0m :1m :2m :3m :4m}");
    }
    [Test]
    public void TestEachX ()
    {
      DoTest ("{<-$R} each \\x00 to \\x04", "{:\\x00 :\\x01 :\\x02 :\\x03 :\\x04}");
    }
    [Test]
    public void TestEachS ()
    {
      DoTest ("{<-$R} each string 0 to 4", "{:\"0\" :\"1\" :\"2\" :\"3\" :\"4\"}");
    }
    [Test]
    public void TestEachB ()
    {
      DoTest ("{<-$R} each boolean 0 to 4", "{:false :true :true :true :true}");
    }
    [Test]
    public void TestEachY ()
    {
      DoTest ("{<-$R} each #x #y #z", "{:#x :#y :#z}");
    }
    [Test]
    public void TestEachT ()
    {
      DoTest ("{<-$R} each 2015.05.27 2015.05.28 2015.05.29",
              "{:2015.05.27 :2015.05.28 :2015.05.29}");
    }
    [Test]
    public void TestEachK ()
    {
      DoTest ("{<-eval {x:$R.x-1.0}} each {:{x:1.0} :{x:2.0} :{x:3.0}}",
              "{:{x:0.0} :{x:1.0} :{x:2.0}}");
    }
    [Test]
    [Ignore ("because")]
    public void TestEachKeY ()
    {
      DoTest ("{<-[?<a>[!$R!]</a>?] $R} each #a #b #c #d",
              "\"<a>#a</a>\" \"<a>#b</a>\" \"<a>#c</a>\" \"<a>#d</a>\"");
    }
    [Test]
    [Ignore ("because")]
    public void TestEachEY ()
    {
      DoTest ("[?<a>[!$R!]</a>?] each #a #b #c #d",
              "\"<a>#a</a>\" \"<a>#b</a>\" \"<a>#c</a>\" \"<a>#d</a>\"");
    }

    // [Test]
    // public void TestEachE() { DoTest ("[?<a
    // href=\"query?symbol=%22[!$R!]%22\">#[!$R!]</a>?] each
    // #a #b #c #d",

    [Test]
    public void TestUnionL ()
    {
      DoTest ("1 2 3 union 2 3 4", "1 2 3 4");
    }
    [Test]
    public void TestUnionD ()
    {
      DoTest ("1.0 2.0 3.0 union 2.0 3.0 4.0", "1.0 2.0 3.0 4.0");
    }
    [Test]
    public void TestUnionM ()
    {
      DoTest ("1 2 3m union 2 3 4m", "1 2 3 4m");
    }
    [Test]
    public void TestUnionX ()
    {
      DoTest ("\\x01 \\x02 \\x03 union \\x02 \\x03 \\x04", "\\x01 \\x02 \\x03 \\x04");
    }
    [Test]
    public void TestUnionB ()
    {
      DoTest ("true union false", "true false");
    }
    [Test]
    public void TestUnionS ()
    {
      DoTest ("\"a\" \"b\" \"c\" union \"b\" \"c\" \"d\"", "\"a\" \"b\" \"c\" \"d\"");
    }
    [Test]
    public void TestUnionY ()
    {
      DoTest ("#a #b #c union #b #c #d", "#a #b #c #d");
    }
    [Test]
    public void TestUnionT ()
    {
      DoTest ("2015.05.27 2015.05.28 2015.05.29 union 2015.05.28 2015.05.29 2015.05.30",
              "2015.05.27 2015.05.28 2015.05.29 2015.05.30");
    }

    [Test]
    public void TestExceptL ()
    {
      DoTest ("1 2 3 except 2 3 4", "1");
    }
    [Test]
    public void TestExceptD ()
    {
      DoTest ("1.0 2.0 3.0 except 2.0 3.0 4.0", "1.0");
    }
    [Test]
    public void TestExceptM ()
    {
      DoTest ("1 2 3m except 2 3 4m", "1m");
    }
    [Test]
    public void TestExceptX ()
    {
      DoTest ("\\x01 \\x02 \\x03 except \\x02 \\x03 \\x04", "\\x01");
    }
    [Test]
    public void TestExceptB ()
    {
      DoTest ("true except false", "true");
    }
    [Test]
    public void TestExceptS ()
    {
      DoTest ("\"a\" \"b\" \"c\" except \"b\" \"c\" \"d\"", "\"a\"");
    }
    [Test]
    public void TestExceptY ()
    {
      DoTest ("#a #b #c except #b #c #d", "#a");
    }
    [Test]
    public void TestExceptT ()
    {
      DoTest ("2015.05.27 2015.05.28 2015.05.29 except 2015.05.28 2015.05.29", "2015.05.27");
    }
    [Test]
    public void TestExceptK ()
    {
      DoTest ("{a:1 b:2 c:3 d:4} except \"b\" \"c\"", "{a:1 d:4}");
    }
    [Test]
    [Ignore ("Should work but I want it to do nesting")]
    public void TestExceptKY ()
    {
      DoTest ("{a:1 b:2 c:3 d:4} except #b #c", "{a:1 d:4}");
    }

    [Test]
    public void TestInterL ()
    {
      DoTest ("1 2 3 inter 2 3 4", "2 3");
    }
    [Test]
    public void TestInterD ()
    {
      DoTest ("1.0 2.0 3.0 inter 2.0 3.0 4.0", "2.0 3.0");
    }
    [Test]
    public void TestInterM ()
    {
      DoTest ("1 2 3m inter 2 3 4m", "2 3m");
    }
    [Test]
    public void TestInterX ()
    {
      DoTest ("\\x01 \\x02 \\x03 inter \\x02 \\x03 \\x04", "\\x02 \\x03");
    }
    [Test]
    public void TestInterB ()
    {
      DoTest ("true inter false", "~b");
    }
    [Test]
    public void TestInterS ()
    {
      DoTest ("\"a\" \"b\" \"c\" inter \"b\" \"c\" \"d\"", "\"b\" \"c\"");
    }
    [Test]
    public void TestInterY ()
    {
      DoTest ("#a #b #c inter #b #c #d", "#b #c");
    }
    [Test]
    public void TestInterT ()
    {
      DoTest ("2015.05.27 2015.05.28 2015.05.29 inter 2015.05.28 2015.05.29 2015.05.30",
              "2015.05.28 2015.05.29");
    }
    [Test]
    public void TestInterK ()
    {
      DoTest ("{a:1 b:2 c:3 d:4} inter \"b\" \"c\"", "{b:2 c:3}");
    }
    [Test]
    [Ignore ("Should work but I want it to do nesting")]
    public void TestInterKY ()
    {
      DoTest ("{a:1 b:2 c:3 d:4} inter #b #c", "{b:2 c:3}");
    }

    [Test]
    public void TestInL ()
    {
      DoTest ("0 1 2 3 4 in 1 3", "false true false true false");
    }
    [Test]
    public void TestInD ()
    {
      DoTest ("0.0 1.0 2.0 3.0 4.0 in 1.0 3.0", "false true false true false");
    }
    [Test]
    public void TestInM ()
    {
      DoTest ("0 1 2 3 4m in 1 3m", "false true false true false");
    }
    [Test]
    public void TestInX ()
    {
      DoTest ("\\x00 \\x01 \\x02 \\x03 \\x04 in \\x01 \\x03", "false true false true false");
    }
    [Test]
    public void TestInB ()
    {
      DoTest ("false true false true false in true", "false true false true false");
    }
    [Test]
    public void TestInY ()
    {
      DoTest ("#0 #1 #2 #3 #4 in #1 #3", "false true false true false");
    }
    [Test]
    public void TestInT ()
    {
      DoTest ("2015.05.26 2015.05.27 2015.05.28 2015.05.29 2015.05.30 in 2015.05.28 2015.05.30",
              "false false true false true");
    }

    [Test]
    public void TestWithinL ()
    {
      DoTest ("1 2 3 4 5 6 7 8 9 10 within 2 4 6 8",
              "false true true true false true true true false false");
    }
    [Test]
    public void TestWithinD ()
    {
      DoTest ("1.0 2.0 3.0 4.0 5.0 6.0 7.0 8.0 9.0 10.0 within 2.0 4.0 6.0 8.0",
              "false true true true false true true true false false");
    }
    [Test]
    public void TestWithinM ()
    {
      DoTest ("1 2 3 4 5 6 7 8 9 10m within 2 4 6 8m",
              "false true true true false true true true false false");
    }
    [Test]
    public void TestWithinS ()
    {
      DoTest (
        "\"a\" \"b\" \"c\" \"d\" \"e\" \"f\" \"g\" \"h\" \"i\" \"j\" within \"b\" \"d\" \"f\" \"h\"",
        "false true true true false true true true false false");
    }
    [Test]
    public void TestWithinY ()
    {
      DoTest ("#a #b #c #d #e #f #g #h #i #j within #b #d #f #h",
              "false true true true false true true true false false");
    }
    [Test]
    public void TestWithinX ()
    {
      DoTest (
        "\\x01 \\x02 \\x03 \\x04 \\x05 \\x06 \\x07 \\x08 \\x09 \\x0a within \\x02 \\x04 \\x06 \\x08",
        "false true true true false true true true false false");
    }
    [Test]
    public void TestReverseL ()
    {
      DoTest ("reverse 1 2 3", "3 2 1");
    }
    [Test]
    public void TestReverseD ()
    {
      DoTest ("reverse 1.0 2.0 3.0", "3.0 2.0 1.0");
    }
    [Test]
    public void TestReverseM ()
    {
      DoTest ("reverse 1.0 2.0 3.0m", "3 2 1m");
    }
    [Test]
    public void TestReverseX ()
    {
      DoTest ("reverse \\x00 \\x01 \\x02", "\\x02 \\x01 \\x00");
    }
    [Test]
    public void TestReverseB ()
    {
      DoTest ("reverse true false true false", "false true false true");
    }
    [Test]
    public void TestReverseY ()
    {
      DoTest ("reverse #a #b #c", "#c #b #a");
    }
    [Test]
    public void TestReverseS ()
    {
      DoTest ("reverse \"a\" \"b\" \"c\"", "\"c\" \"b\" \"a\"");
    }
    [Test]
    public void TestReverseT ()
    {
      DoTest ("reverse 2017.08.24 2017.08.25 2017.08.26", "2017.08.26 2017.08.25 2017.08.24");
    }
    [Test]
    public void TestReverseK ()
    {
      DoTest ("reverse {a:1 b:2 c:3}", "{c:3 b:2 a:1}");
    }

    [Test]
    public void TestPrint ()
    {
      DoTest ("print \"this\" \"is\" \"some\" \"output\"", "\"this\" \"is\" \"some\" \"output\"");
    }

    [Test]
    public void TestSubX ()
    {
      DoTest ("\\x00 \\x01 \\x01 \\x02 \\x02 \\x03 sub \\x00 \\x01 \\x02", "\\x01 \\x02 \\x03");
    }
    [Test]
    public void TestSubD ()
    {
      DoTest ("0.0 1.0 1.0 2.0 2.0 3.0 sub 0.0 1.0 2.0", "1.0 2.0 3.0");
    }
    [Test]
    public void TestSubD0 ()
    {
      DoTest ("0.0 -1.0 sub 0.0 NaN 1.0", "-1.0 NaN 1.0");
    }
    [Test]
    public void TestSubD1 ()
    {
      DoTest ("NaN 0 sub NaN 1.0 2.0", "0.0 1.0 2.0");
    }
    [Test]
    public void TestSubL ()
    {
      DoTest ("0 1 1 2 2 3 sub 0 1 2", "1 2 3");
    }
    [Test]
    public void TestSubM ()
    {
      DoTest ("0 1 1 2 2 3m sub 0 1 2m", "1 2 3m");
    }
    [Test]
    public void TestSubS ()
    {
      DoTest ("\"0\" \"1\" \"1\" \"2\" \"2\" \"3\" sub \"0\" \"1\" \"2\"", "\"1\" \"2\" \"3\"");
    }
    [Test]
    public void TestSubY ()
    {
      DoTest ("#0 #1 #1 #2 #2 #3 sub #0 #1 #2", "#1 #2 #3");
    }
    [Test]
    public void TestSubB ()
    {
      DoTest ("true false false true sub true false", "false true");
    }
    [Test]
    public void TestSubT ()
    {
      DoTest ("08:00 08:10 08:10 08:20 08:20 08:30 sub 08:00 08:10 08:20",
              "08:10:00 08:20:00 08:30:00");
    }

    [Test]
    public void TestFillL ()
    {
      DoTest ("0 fill 1 0 2 0 0 3 0 0 0", "1 1 2 2 2 3 3 3 3");
    }
    [Test]
    public void TestFillD ()
    {
      DoTest ("0.0 fill 1.0 0.0 2.0 0.0 0.0 3.0 0.0 0.0 0.0",
              "1.0 1.0 2.0 2.0 2.0 3.0 3.0 3.0 3.0");
    }
    [Test]
    public void TestFillM ()
    {
      DoTest ("0m fill 1 0 2 0 0 3 0 0 0m", "1 1 2 2 2 3 3 3 3m");
    }
    [Test]
    public void TestFillY ()
    {
      DoTest ("#0 fill #1 #0 #2 #0 #0 #3 #0 #0 #0", "#1 #1 #2 #2 #2 #3 #3 #3 #3");
    }
    [Test]
    public void TestFillS ()
    {
      DoTest ("\"0\" fill \"1\" \"0\" \"2\" \"0\" \"0\" \"3\" \"0\" \"0\" \"0\"",
              "\"1\" \"1\" \"2\" \"2\" \"2\" \"3\" \"3\" \"3\" \"3\"");
    }
    [Test]
    public void TestFillX ()
    {
      DoTest ("\\x00 fill \\x01 \\x00 \\x02 \\x00 \\x00 \\x03 \\x00 \\x00 \\x00",
              "\\x01 \\x01 \\x02 \\x02 \\x02 \\x03 \\x03 \\x03 \\x03");
    }
    [Test]
    public void TestFillB ()
    {
      DoTest ("false fill false false true false false", "false false true true true");
    }
    [Test]
    public void TestFillT ()
    {
      DoTest ("00:00 fill 08:00 00:00 00:00 09:00 00:00 00:00 10:00 00:00 00:00",
              "08:00:00 08:00:00 08:00:00 09:00:00 09:00:00 09:00:00 10:00:00 10:00:00 10:00:00");
    }
    // Eval

    // Range
    [Test]
    public void TestRangeL ()
    {
      DoTest ("3 range 1 2 3 4 5 6", "4 5 6");
    }
    [Test]
    public void TestRangeL1 ()
    {
      DoTest ("1 4 range 1 2 3 4 5 6", "2 3 4 5");
    }
    [Test]
    public void TestRangeL2 ()
    {
      DoTest ("0 1 3 5 range 1 2 3 4 5 6", "1 2 4 5 6");
    }
    [Test]
    public void TestRangeL3 ()
    {
      DoTest ("0 1 2 5 range 1 2 3 4 5 6", "1 2 3 4 5 6");
    }

    [Test]
    public void TestRangeD ()
    {
      DoTest ("3 range 1.0 2.0 3.0 4.0 5.0 6.0", "4.0 5.0 6.0");
    }
    [Test]
    public void TestRangeD1 ()
    {
      DoTest ("1 4 range 1.0 2.0 3.0 4.0 5.0 6.0", "2.0 3.0 4.0 5.0");
    }
    [Test]
    public void TestRangeD2 ()
    {
      DoTest ("0 1 3 5 range 1.0 2.0 3.0 4.0 5.0 6.0", "1.0 2.0 4.0 5.0 6.0");
    }
    [Test]
    public void TestRangeD3 ()
    {
      DoTest ("0 1 2 5 range 1.0 2.0 3.0 4.0 5.0 6.0", "1.0 2.0 3.0 4.0 5.0 6.0");
    }

    [Test]
    public void TestRangeM ()
    {
      DoTest ("3 range 1 2 3 4 5 6m", "4 5 6m");
    }
    [Test]
    public void TestRangeM1 ()
    {
      DoTest ("1 4 range 1 2 3 4 5 6m", "2 3 4 5m");
    }
    [Test]
    public void TestRangeM2 ()
    {
      DoTest ("0 1 3 5 range 1 2 3 4 5 6m", "1 2 4 5 6m");
    }
    [Test]
    public void TestRangeM3 ()
    {
      DoTest ("0 1 2 5 range 1 2 3 4 5 6m", "1 2 3 4 5 6m");
    }

    [Test]
    public void TestRangeY ()
    {
      DoTest ("3 range #1 #2 #3 #4 #5 #6", "#4 #5 #6");
    }
    [Test]
    public void TestRangeY1 ()
    {
      DoTest ("1 4 range #1 #2 #3 #4 #5 #6", "#2 #3 #4 #5");
    }
    [Test]
    public void TestRangeY2 ()
    {
      DoTest ("0 1 3 5 range #1 #2 #3 #4 #5 #6", "#1 #2 #4 #5 #6");
    }
    [Test]
    public void TestRangeY3 ()
    {
      DoTest ("0 1 2 5 range #1 #2 #3 #4 #5 #6", "#1 #2 #3 #4 #5 #6");
    }

    [Test]
    public void TestRangeS ()
    {
      DoTest ("3 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"4\" \"5\" \"6\"");
    }
    [Test]
    public void TestRangeS1 ()
    {
      DoTest ("1 4 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"2\" \"3\" \"4\" \"5\"");
    }
    [Test]
    public void TestRangeS2 ()
    {
      DoTest ("0 1 3 5 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"", "\"1\" \"2\" \"4\" \"5\" \"6\"");
    }
    [Test]
    public void TestRangeS3 ()
    {
      DoTest ("0 1 2 5 range \"1\" \"2\" \"3\" \"4\" \"5\" \"6\"",
              "\"1\" \"2\" \"3\" \"4\" \"5\" \"6\"");
    }

    [Test]
    public void TestRangeX ()
    {
      DoTest ("3 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x04 \\x05 \\x06");
    }
    [Test]
    public void TestRangeX1 ()
    {
      DoTest ("1 4 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x02 \\x03 \\x04 \\x05");
    }
    [Test]
    public void TestRangeX2 ()
    {
      DoTest ("0 1 3 5 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06", "\\x01 \\x02 \\x04 \\x05 \\x06");
    }
    [Test]
    public void TestRangeX3 ()
    {
      DoTest ("0 1 2 5 range \\x01 \\x02 \\x03 \\x04 \\x05 \\x06",
              "\\x01 \\x02 \\x03 \\x04 \\x05 \\x06");
    }

    [Test]
    public void TestRangeT ()
    {
      DoTest ("3 range 00:01 00:02 00:03 00:04 00:05 00:06", "00:04:00 00:05:00 00:06:00");
    }
    [Test]
    public void TestRangeT1 ()
    {
      DoTest ("1 4 range 00:01 00:02 00:03 00:04 00:05 00:06",
              "00:02:00 00:03:00 00:04:00 00:05:00");
    }
    [Test]
    public void TestRangeT2 ()
    {
      DoTest ("0 1 3 5 range 00:01 00:02 00:03 00:04 00:05 00:06",
              "00:01:00 00:02:00 00:04:00 00:05:00 00:06:00");
    }
    [Test]
    public void TestRangeT3 ()
    {
      DoTest ("0 1 2 5 range 00:01 00:02 00:03 00:04 00:05 00:06",
              "00:01:00 00:02:00 00:03:00 00:04:00 00:05:00 00:06:00");
    }

    // String/Text Operators
    [Test]
    public void TestDelimit ()
    {
      DoTest ("\",\" delimit \"x\" \"y\" \"z\"", "\"x,y,z\"");
    }
    [Test]
    public void TestSplit ()
    {
      DoTest ("\",:\" split \"x,y,z\" \"a:b:c\"", "\"x\" \"y\" \"z\" \"a\" \"b\" \"c\"");
    }
    [Test]
    public void TestSplitw0 ()
    {
      DoTest ("\"  \" splitw \"x  y z\"", "\"x\" \"y z\"");
    }
    [Test]
    public void TestSplitw1 ()
    {
      DoTest ("\"  \" splitw \"  x  y z\"", "\"\" \"x\" \"y z\"");
    }
    [Test]
    public void TestTuple ()
    {
      DoTest ("\":\" tuple \"a\" \"b:c\" \"d:e:f\"", "#a #b,c #d,e,f");
    }
    [Test]
    public void TestSlice0 ()
    {
      DoTest ("\":\" slice \"a:b\" \"c:d\" \"e:f\"", "{:\"a\" \"c\" \"e\" :\"b\" \"d\" \"f\"}");
    }
    [Test]
    public void TestSlice1 ()
    {
      DoTest ("\",\" slice \"a\" \"b,c\" \"d,e,f\"",
              "{:\"a\" \"b\" \"d\" :\"\" \"c\" \"e\" :\"\" \"\" \"f\"}");
    }
    [Test]
    public void TestReplace ()
    {
      DoTest ("\"\\\"\" \"\" replace \"\\\"a\\\"\" \"b\"", "\"a\" \"b\"");
    }
    [Test]
    public void TestUpperS ()
    {
      DoTest ("upper \"aAa\" \"AAA\" \"aaa\"", "\"AAA\" \"AAA\" \"AAA\"");
    }
    [Test]
    public void TestLowerS ()
    {
      DoTest ("lower \"aAa\" \"AAA\" \"aaa\"", "\"aaa\" \"aaa\" \"aaa\"");
    }
    [Test]
    public void TestSubstring0 ()
    {
      DoTest ("1 substring \"/foo\" \"/bar\"", "\"foo\" \"bar\"");
    }
    [Test]
    public void TestSubstring1 ()
    {
      DoTest ("1 2 substring \"/foo\" \"/bar\"", "\"fo\" \"ba\"");
    }
    [Test]
    public void TestTrim ()
    {
      DoTest ("trim \"a\" \" b\" \"c \" \" d \"", "\"a\" \"b\" \"c\" \"d\"");
    }
    [Test]
    public void TestTrim1 ()
    {
      DoTest ("trim \"a\" \"\nb\" \"c\n\" \"\nd\n\"", "\"a\" \"b\" \"c\" \"d\"");
    }
    [Test]
    public void TestTrim2 ()
    {
      DoTest ("trim \"a\" \"\r\nb\" \"c\r\n\" \"\r\nd\r\n\"", "\"a\" \"b\" \"c\" \"d\"");
    }
    [Test]
    public void TestTrim3 ()
    {
      DoTest ("\"/\" trim \"a\" \"/b\" \"c/\" \"/d/\"", "\"a\" \"b\" \"c\" \"d\"");
    }
    [Test]
    public void TestTrimStart ()
    {
      DoTest ("trimStart \"a\" \" b\" \"c \" \" d \"", "\"a\" \"b\" \"c \" \"d \"");
    }
    [Test]
    public void TestTrimEnd ()
    {
      DoTest ("trimEnd \"a\" \" b\" \"c \" \" d \"", "\"a\" \" b\" \"c\" \" d\"");
    }
    [Test]
    public void TestTrimStartSS ()
    {
      DoTest ("\"/\" trimStart \"a\" \"/b\" \"c/\" \"d/\"", "\"a\" \"b\" \"c/\" \"d/\"");
    }
    [Test]
    public void TestTrimEndSS ()
    {
      DoTest ("\"/\" trimEnd \"a\" \"/b\" \"c/\" \"/d/\"", "\"a\" \"/b\" \"c\" \"/d\"");
    }
    [Test]
    public void TestIndexOf ()
    {
      DoTest ("\"foo\" indexof \"abcfoodef\" \"abcdef\" \"foo\" \"fooabcdef\" \"abcdeffoo\"",
              "3 15 18 33");
    }
    [Test]
    public void TestPad ()
    {
      DoTest (" \"-\" pad 1 2 3", "\"-\" \"--\" \"---\"");
    }
    [Test]
    public void TestStartsWith ()
    {
      DoTest ("\"aaa\" \"foobar\" \"fozbaz\" \"foo\" \"fo\" startsWith \"foo\"",
              "false true false true false");
    }
    [Test]
    public void TestStartsWith1 ()
    {
      DoTest ("\"x 1\" \"y 2\" \"z 3\" \"x 4\" \"y 5\" startsWith \"x\" \"y\"",
              "true true false true true");
    }
    [Test]
    public void TestStartsWithUS ()
    {
      DoTest ("[x \"x 1\" \"y 2\" \"z 3\" \"x 4\" \"y 5\"] startsWith \"x\" \"y\"",
              "[x true true false true true]");
    }
    [Test]
    public void TestEndsWith ()
    {
      DoTest ("\"x...\" \"y\" \"z..\" \"...\" endsWith \"...\"", "true false false true");
    }
    [Test]
    public void TestEndsWithUS ()
    {
      DoTest ("[x \"x...\" \"y\" \"z..\" \"...\"] endsWith \"...\"", "[x true false false true]");
    }
    [Test]
    public void TestCut ()
    {
      DoTest ("0 1 2 cut \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"abcdef\" \"hijkl\" \"opqr\"");
    }
    [Test]
    public void TestCut1 ()
    {
      DoTest ("0 -1 -2 cut \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"abcdef\" \"l\" \"qr\"");
    }
    [Test]
    public void TestCut2 ()
    {
      DoTest ("[x 0 -1 -2] cut [x \"abcdef\" \"ghijkl\" \"mnopqr\"]",
              "[x \"abcdef\" \"l\" \"qr\"]");
    }
    [Test]
    public void TestCut3 ()
    {
      DoTest ("[S|x #a 0 #b -1 #c -2] cut [S|x #a \"abcdef\" #b \"ghijkl\" #c \"mnopqr\"]",
              "[S|x #a \"abcdef\" #b \"l\" #c \"qr\"]");
    }
    [Test]
    public void TestCutleft ()
    {
      DoTest ("0 1 2 cutleft \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"\" \"g\" \"mn\"");
    }
    [Test]
    public void TestCutleft1 ()
    {
      DoTest ("0 -1 -2 cutleft \"abcdef\" \"ghijkl\" \"mnopqr\"", "\"\" \"ghijk\" \"mnop\"");
    }
    [Test]
    public void TestCutLeft2 ()
    {
      DoTest ("[x 0 -1 -2] cutleft [x \"abcdef\" \"ghijkl\" \"mnopqr\"]",
              "[x \"\" \"ghijk\" \"mnop\"]");
    }
    [Test]
    public void TestCutLeft3 ()
    {
      DoTest ("[S|x #a 0 #b -1 #c -2] cutleft [S|x #a \"abcdef\" #b \"ghijkl\" #c \"mnopqr\"]",
              "[S|x #a \"\" #b \"ghijk\" #c \"mnop\"]");
    }
    [Test]
    public void TestLike1 ()
    {
      DoTest ("\"foobar\" \"foobaz\" \"foo\" \"fazbar\" like \"foo*\"", "true true true false");
    }
    [Test]
    public void TestLike2 ()
    {
      DoTest ("\"foobar\" \"fazbar\" \"bar\" \"foobaz\" like \"*bar\"", "true true true false");
    }
    [Test]
    public void TestLike3 ()
    {
      DoTest ("\"foozbar\" \"foolsbar\" \"foobar\" \"foobaz\" like \"foo*bar\"",
              "true true true false");
    }
    [Test]
    public void TestLike4 ()
    {
      DoTest ("\"\" like \"xyz*\"", "false");
    }
    [Test]
    public void TestLike5 ()
    {
      DoTest ("\"\" like \"*\"", "true");
    }
    [Test]
    public void TestLike6 ()
    {
      DoTest ("\"foo bar baz\" like \"bar*\"", "false");
    }
    [Test]
    public void TestLike7 ()
    {
      DoTest ("\"foo bar baz\" like \"bar\"", "false");
    }
    [Test]
    public void TestLike8 ()
    {
      DoTest ("\"foo bar baz\" like \"foo\"", "false");
    }
    [Test]
    public void TestLike1U ()
    {
      DoTest ("[x \"foobar\" \"foobaz\" \"foo\" \"fazbar\"] like \"foo*\"",
              "[x true true true false]");
    }
    [Test]
    public void TestLike2U ()
    {
      DoTest ("[S|x #a \"foobar\" #b \"fazbar\" #c \"bar\" #d \"foobaz\"] like \"*bar\"",
              "[S|x #a true #b true #c true #d false]");
    }
    [Test]
    public void TestLike3U ()
    {
      DoTest ("[x \"foozbar\" \"foolsbar\" \"foobar\" \"foobaz\"] like \"foo*bar\"",
              "[x true true true false]");
    }
    [Test]
    public void TestIsName ()
    {
      DoTest ("isname \"aaa\" \"a a\" \"'foo'\" \"1foo\" \"foo1\" \"foo_bar\"",
              "true false false false true true");
    }

    [Test]
    public void TestNetformatSS ()
    {
      DoTest ("\"{0} {1} {2}\" \"{2} {1} {0}\" netformat \"a\" \"b\" \"c\"", "\"a b c\" \"c b a\"");
    }
    [Test]
    public void TestNetformatSL ()
    {
      DoTest ("\"{0} {1} {2}\" \"{2} {1} {0}\" netformat 1 2 3", "\"1 2 3\" \"3 2 1\"");
    }
    [Test]
    public void TestNetformatSD ()
    {
      DoTest ("\"{0:N1} {1:N1} {2:N1}\" \"{2:N2} {1:N2} {0:N2}\" netformat 1.0 2.0 3.0",
              "\"1.0 2.0 3.0\" \"3.00 2.00 1.00\"");
    }
    [Test]
    public void TestNetformatST ()
    {
      DoTest ("\"{0:s}\" \"{0:t}\" \"{0:r}\" netformat 2019.10.28",
              "\"2019-10-28T00:00:00\" \"12:00 AM\" \"Mon, 28 Oct 2019 00:00:00 GMT\"");
    }
    [Test]
    public void TestNetformatSM ()
    {
      DoTest ("\"{0:N1} {1:N1} {2:N1}\" \"{2:N2} {1:N2} {0:N2}\" netformat 1.0 2.0 3.0m",
              "\"1.0 2.0 3.0\" \"3.00 2.00 1.00\"");
    }
    [Test]
    public void TestNetformatSY ()
    {
      DoTest ("\"{0} {1} {2}\" \"{2} {1} {0}\" netformat #a #b #c", "\"#a #b #c\" \"#c #b #a\"");
    }
    [Test]
    public void TestNetFormatSK ()
    {
      DoTest (
        "\"{0} {1} {2} {3}\" netformat eval {a:1 2 3 b:\"4\" \"5\" \"6\" c:string 2019.10.28 2019.10.29 2019.10.30 d:#a #b #c}",
        "\"1 4 2019.10.28 #a\" \"2 5 2019.10.29 #b\" \"3 6 2019.10.30 #c\"");
    }
    [Test]
    public void TestNetFormatSKTime ()
    {
      DoTest (
        "\"{0} {1} {2:s} {3}\" netformat {a:1 2 3 b:\"4\" \"5\" \"6\" c:2019.10.28 2019.10.29 2019.10.30 d:#a #b #c}",
        "\"1 4 2019-10-28T00:00:00 #a\" \"2 5 2019-10-29T00:00:00 #b\" \"3 6 2019-10-30T00:00:00 #c\"");
    }

    [Test]
    public void TestUtf8SX ()
    {
      DoTest ("utf8 utf8 \"foobarbaz\"", "\"foobarbaz\"");
    }
    [Test]
    public void TestAsciiSX ()
    {
      DoTest ("ascii ascii \"foobarbaz\"", "\"foobarbaz\"");
    }
    [Test]
    public void TestMatch1 ()
    {
      DoTest ("\"^[a-zA-Z\\\\'\\\\-\\\\s]{1,55}$\" ismatch \"Brian M. Andersen\"", "false");
    }
    [Test]
    public void TestMatch2 ()
    {
      DoTest ("\"^[a-zA-Z\\\\'\\\\-\\\\s]{1,55}$\" ismatch \"Brian M Andersen\"", "true");
    }
    [Test]
    public void TestMatch3 ()
    {
      DoTest ("\"^\\\\d{4}$\" ismatch \"1\" \"12\" \"123\" \"1234\" \"12345\"",
              "false false false true false");
    }
    [Test]
    public void TestMatch4 ()
    {
      DoTest ("\"^\\\\d{4}$\" ismatch [x \"1\" \"12\" \"123\" -- \"1234\" \"12345\"]",
              "[x false false false -- true false]");
    }
    [Test]
    public void TestResplit ()
    {
      DoTest (
        "{number_regex:\"[+-]?(?:(?:\\\\d+(?:\\\\.\\\\d*)?)|(?:\\\\.\\\\d+))\" <-unwrap $number_regex resplit \"translate(202,33.625)\"}",
        "\"translate(\" \",\" \")\"");
    }
    [Test]
    public void TestMatches ()
    {
      DoTest (
        "{number_regex:\"[+-]?(?:(?:\\\\d+(?:\\\\.\\\\d*)?)|(?:\\\\.\\\\d+))\" <-unwrap $number_regex matches \"translate(202,33.625)\"}",
        "\"202\" \"33.625\"");
    }

    // Block
    [Test]
    public void TestNamesK1 ()
    {
      DoTest ("names {a:1 b:2 c:3}", "\"a\" \"b\" \"c\"");
    }
    [Test]
    public void TestNamesK2 ()
    {
      DoTest ("names {abc:1 'name-with-hyphens':2 '2':3 'true':4 '1-2-3':5}",
              "\"abc\" \"name-with-hyphens\" \"2\" \"true\" \"1-2-3\"");
    }
    [Test]
    public void TestRenameS ()
    {
      DoTest ("\"x\" \"y\" \"z\" rename {a:1 b:2 c:3}", "{x:1 y:2 z:3}");
    }
    [Test]
    public void TestRenameY ()
    {
      DoTest ("#x #y #z rename {a:1 b:2 c:3}", "{x:1 y:2 z:3}");
    }
    [Test]
    public void TestRenameS1 ()
    {
      DoTest ("\"x\" rename {a:1 b:2 c:3}", "{x:1 x:2 x:3}");
    }
    [Test]
    public void TestRenameY1 ()
    {
      DoTest ("#x rename {a:1 b:2 c:3}", "{x:1 x:2 x:3}");
    }
    [Test]
    public void TestNameKSS ()
    {
      DoTest ("\"name\" name {:{name:\"a\" value:1} :{name:\"b\" value:2} :{name:\"c\" value:3}}",
              "{a:{name:\"a\" value:1} b:{name:\"b\" value:2} c:{name:\"c\" value:3}}");
    }
    [Test]
    public void TestNameKSY ()
    {
      DoTest ("\"name\" name {:{name:#a value:1} :{name:#b value:2} :{name:#c value:3}}",
              "{a:{name:#a value:1} b:{name:#b value:2} c:{name:#c value:3}}");
    }
    [Test]
    public void TestNameKYS ()
    {
      DoTest ("#name name {:{name:\"a\" value:1} :{name:\"b\" value:2} :{name:\"c\" value:3}}",
              "{a:{name:\"a\" value:1} b:{name:\"b\" value:2} c:{name:\"c\" value:3}}");
    }
    [Test]
    public void TestNameKYY ()
    {
      DoTest ("#name name {:{name:#a value:1} :{name:#b value:2} :{name:#c value:3}}",
              "{a:{name:#a value:1} b:{name:#b value:2} c:{name:#c value:3}}");
    }
    [Test]
    public void TestNameSU ()
    {
      DoTest ("\"a\" \"b\" \"c\" name [a b c 0 -- -- 0 \"x\" -- 0 \"x\" 2020.02.13]",
              "[S|a b c #0 0 -- -- #0,x 0 \"x\" -- #0,x,2020.02.13 0 \"x\" 2020.02.13]");
    }
    [Test]
    public void TestNameSU1 ()
    {
      // Missing column names are skipped over.
      // It is possibly not the right thing to do.
      // Probably better to create a symbol with a blank part like #0,,2020.02.13
      DoTest ("\"a\" \"b\" \"c\" name [a c 0 -- 0 -- 0 2020.02.13]",
              "[S|a c #0 0 -- #0 0 -- #0,2020.02.13 0 2020.02.13]");
    }
    [Test]
    public void TestSetK0 ()
    {
      DoTest ("{x:1 y:2} set {y:3 z:4}", "{x:1 y:3 z:4}");
    }
    [Test]
    public void TestSetK1 ()
    {
      DoTest ("{:~s :~s :~s} set {:\"a0\" \"a1\" :\"b0\" \"b1\" :\"c0\" \"c1\"}",
              "{:\"a0\" \"a1\" :\"b0\" \"b1\" :\"c0\" \"c1\"}");
    }
    [Test]
    public void TestSetK2 ()
    {
      DoTest ("{:~s :~s :~s} set {}", "{:~s :~s :~s}");
    }
    [Test]
    public void TestSetK3 ()
    {
      DoTest ("{x:1 y:2 :3} set {z:4}", "{x:1 y:2 :3 z:4}");
    }
    [Test]
    public void TestSetK4 ()
    {
      DoTest ("{x:1 y:2 :3} set {:4}", "{x:1 y:2 :4}");
    }
    [Test]
    public void TestGetLK ()
    {
      DoTest ("1 get {:#x :#y :#z}", "#y");
    }
    [Test]
    public void TestGetKL ()
    {
      DoTest ("{:#x :#y :#z} get 1", "#y");
    }
    [Test]
    public void TestGetSK ()
    {
      DoTest ("\"b\" get {a:#x b:#y c:#z}", "#y");
    }
    [Test]
    public void TestGetKS ()
    {
      DoTest ("{a:#x b:#y c:#z} get \"b\"", "#y");
    }
    [Test]
    public void TestGetYK ()
    {
      DoTest ("#b get {a:#x b:#y c:#z}", "#y");
    }
    [Test]
    public void TestGetKY ()
    {
      DoTest ("{a:#x b:#y c:#z} get #b", "#y");
    }
    [Test]
    public void TestGetYK1 ()
    {
      DoTest ("#1 get {a:#x b:#y c:#z}", "#y");
    }
    [Test]
    public void TestGetKY1 ()
    {
      DoTest ("{a:#x b:#y c:#z} get #1", "#y");
    }

    [Test]
    public void TestMergeK0 ()
    {
      DoTest ("{a:3 b:4} merge {a:2}", "{a:2 b:4}");
    }
    [Test]
    public void TestMergeK1 ()
    {
      DoTest ("{x:1 y:2 z:{a:3 b:4}} merge {z:{a:2}}", "{x:1 y:2 z:{a:2 b:4}}");
    }
    [Test]
    public void TestMergeK2 ()
    {
      DoTest ("{x:1 y:2 z:{a:3 b:4}} merge {z:{}}", "{x:1 y:2 z:{a:3 b:4}}");
    }
    [Test]
    public void TestMergeK3 ()
    {
      DoTest ("{x:1 y:{a:3 b:4} z:2} merge {y:{b:5}}", "{x:1 y:{a:3 b:5} z:2}");
    }
    [Test]
    public void TestMergeK4 ()
    {
      DoTest ("{x:1 y:{a:3 b:5 + 1} z:2} merge {y:{b:{<-5 + 1}}}", "{x:1 y:{a:3 b:{<-5 + 1}} z:2}");
    }
    [Test]
    public void TestMergeK5 ()
    {
      DoTest ("{x:1 y:{a:3 b:{<-5 + 1}} z:2} merge {y:{b:5 + 1}}", "{x:1 y:{a:3 b:5 + 1} z:2}");
    }

    // Still need to think about how set should work in the case of a timeline.
    [Test]
    public void TestHasY ()
    {
      DoTest ("{x:1 y:2 z:3} has #a #y #z", "false true true");
    }
    [Test]
    public void TestHasS ()
    {
      DoTest ("{x:1 y:2 z:3} has \"a\" \"y\" \"z\"", "false true true");
    }
    [Test]
    [Ignore ("reason")]
    public void TestHasL ()
    {
      DoTest ("{x:1 y:2 z:3} has 0 1 4", "true true false");
    }
    [Test]
    public void TestHasUS ()
    {
      DoTest ("[S|x y #a 1 10 #b 2 20] has \"S\" \"x\" \"z\"", "true true false");
    }
    [Test]
    public void TestHasUY ()
    {
      DoTest ("[S|x y #a 1 10 #b 2 20] has #S #x #z", "true true false");
    }

    [Test]
    public void TestUnflipK ()
    {
      DoTest ("unflip {x:1 2 3 4 y:5 6 7 8 z:9 10 11 12}",
              "{:{x:1 y:5 z:9} :{x:2 y:6 z:10} :{x:3 y:7 z:11} :{x:4 y:8 z:12}}");
    }
    [Test]
    public void TestFlipK0 ()
    {
      DoTest ("flip {:{x:1 y:4 z:7} :{x:2 y:5 z:8} :{x:3 y:6 z:9}}", "{x:1 2 3 y:4 5 6 z:7 8 9}");
    }
    [Test]
    public void TestFlipK1 ()
    {
      DoTest ("flip {:{x:1 y:5 z:9} :{x:2 y:6 z:10} :{x:3 y:7 z:11} :{x:4 y:8 z:12}}",
              "{x:1 2 3 4 y:5 6 7 8 z:9 10 11 12}");
    }

    [Test]
    public void TestTypecodeL ()
    {
      DoTest ("typecode 1 2 3", "\"l\"");
    }
    [Test]
    public void TestTypecodeD ()
    {
      DoTest ("typecode 1.0 2.0 3.0", "\"d\"");
    }
    [Test]
    public void TestTypecodeM ()
    {
      DoTest ("typecode 1.0 2.0 3.0m", "\"m\"");
    }
    [Test]
    public void TestTypecodeY ()
    {
      DoTest ("typecode #a #b #c", "\"y\"");
    }
    [Test]
    public void TestTypecodeB ()
    {
      DoTest ("typecode true false true", "\"b\"");
    }
    [Test]
    public void TestTypecodeX ()
    {
      DoTest ("typecode \\x00 \\x01 \\x02", "\"x\"");
    }
    [Test]
    public void TestTypecodeS ()
    {
      DoTest ("typecode \"a\" \"b\" \"c\"", "\"s\"");
    }
    [Test]
    public void TestTypecodeT ()
    {
      DoTest ("typecode 2016.05.16 2016.05.17 2016.05.18", "\"t\"");
    }
    [Test]
    public void TestTypecodeU ()
    {
      DoTest ("typecode [x 0 1 2]", "\"u\"");
    }
    [Test]
    public void TestTypecodeK ()
    {
      DoTest ("typecode {a:1 b:2 c:3}", "\"k\"");
    }

    [Test]
    public void TestTypenameL ()
    {
      DoTest ("typename 1 2 3", "\"long\"");
    }
    [Test]
    public void TestTypenameD ()
    {
      DoTest ("typename 1.0 2.0 3.0", "\"double\"");
    }
    [Test]
    public void TestTypenameM ()
    {
      DoTest ("typename 1.0 2.0 3.0m", "\"decimal\"");
    }
    [Test]
    public void TestTypenameY ()
    {
      DoTest ("typename #a #b #c", "\"symbol\"");
    }
    [Test]
    public void TestTypenameB ()
    {
      DoTest ("typename true false true", "\"boolean\"");
    }
    [Test]
    public void TestTypenameX ()
    {
      DoTest ("typename \\x00 \\x01 \\x02", "\"byte\"");
    }
    [Test]
    public void TestTypenameS ()
    {
      DoTest ("typename \"a\" \"b\" \"c\"", "\"string\"");
    }
    [Test]
    public void TestTypenameT ()
    {
      DoTest ("typename 2016.05.16 2016.05.17 2016.05.18", "\"time\"");
    }
    [Test]
    public void TestTypenameU ()
    {
      DoTest ("typename [x 0 1 2]", "\"cube\"");
    }
    [Test]
    public void TestTypenameK ()
    {
      DoTest ("typename {a:1 b:2 c:3}", "\"block\"");
    }
  }

}


using System;
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  [TestFixture]
  public class CoreTest3 : CoreTest
  {
#if !__MonoCS__
    [SetUp]
    public void Setup ()
    {
      // This is to make tests involving file access work on Windows (under Parallels)
      // It would be better to do this by controlling the working directory from the
      // .runsettings
      // file but I have yet to see any evidence that the runsettings file is honored at
      // all by the
      // Test Explorer.
      Environment.SetEnvironmentVariable ("RCL_HOME", "Y:\\dev");
      // This operator alters the runtime environment, not just the current runner state.
      runner.Run (RCSystem.Parse ("cd #home,src,rcl,RCL.Test,bin,Debug"));
    }
#endif

    // Messaging/Stream Operators
    [Test]
    [Ignore ("because")]
    public void TestOpenSendReceiveCloseCube ()
    {
      DoTest (
        "{handle:open #cube,'..','..',data,test :$handle receive $handle send {verb:#write symbol:#a data:{a:1 b:2.0 c:3m d:\"x\" e:#x f:true}} <-$handle receive $handle send {verb:#read symbol:#a rows:0}}",
        "[S|a b c d e f #a 1 2.0 3m \"x\" #x true]");
    }

    // Persistence Operators
    [Test]
    public void TestSaveLoadDelete ()
    {
      DoTest ("{x:parse load \"file\" save format {a:1.0 b:2.0 c:3.0} :delete \"file\" <-$x}",
              "{a:1.0 b:2.0 c:3.0}");
    }

    [Test]
    public void TestSaveLoadExtraLine ()
    {
      DoTest (
        "{:\"file\" save \"line0\" \"line1\" \"line2\" text:load \"file\" :delete \"file\" <-$text}",
        "\"line0\\nline1\\nline2\"");
    }

    [Test]
    public void TestSaveLoadUnderline ()
    {
      DoTest (
        "{:\"file\" save \"line0\" \"line1\" \"line2\" :\"file\" save \"line0\" \"line1\" text:load \"file\" :delete \"file\" <-$text}",
        "\"line0\\nline1\"");
    }

    [Test]
    public void TestSavebinLoadbinDelete ()
    {
      DoTest ("{x:parse loadbin \"file\" savebin binary {a:1.0 b:2.0 c:3.0} :delete \"file\" <-$x}",
              "{a:1.0 b:2.0 c:3.0}");
    }

    [Test]
    public void TestPath ()
    {
      DoTest (
        "{p:path #home,env,env.rclb :assert $p like \"*env.rclb\" :assert (length $p) > 7 <-0}",
        "0");
    }

    [Test]
    public void TestFlagDefault ()
    {
      DoTest ("false flag \"not-a-flag\"", "false");
    }

    [Test]
    public void TestFile ()
    {
      DoTest (
        "{before:file \"file\" :\"file\" save #pretty format {a:1 b:2 c:3} after:file \"file\" :delete \"file\" <-$before & $after}",
        "false true");
    }

    [Test]
    public void TestFileAndPath ()
    {
      DoTest (
        "{before:file #work,file :#work,file save #pretty format {a:1 b:2 c:3} after:file #work,file :delete #work,file <-$before & $after}",
        "false true");
    }

    [Test]
    public void TestMkdirS ()
    {
      DoTest ("{:mkdir \"mydir\" :rmdir \"mydir\" <-0}", "0");
    }

    [Test]
    public void TestMkdirY ()
    {
      DoTest ("{:mkdir #work,mysymdir :rmdir #work,mysymdir <-0}", "0");
    }

    [Test]
    public void TestWait ()
    {
      // repro
      // {:print "before wait" :wait fiber {sh:startx "bash" :$sh writex "set -e\ncat
      // foo\nexit\n"
      // :waitx $sh} :print "after wait"}
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("{:0 :wait fiber {:assert false} :1}", "");
      });
    }

    [Test]
    public void TestWait1 ()
    {
      DoTest ("#status get try {<-eval {:0 :wait fiber {:assert false} :1}}", "1");
    }

    [Test]
    public void TestWaitExceptionsWithFibers ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest (
          "{inz:fiber {:sleep 250 :assert false} ind:fiber {:sleep 1000 :assert true} ing:fiber {:sleep 500 :assert true} :wait $ing :wait $ind :wait $inz}",
          "");
      });
    }

    [Test]
    public void TestWaitExceptionsWithBots ()
    {
      // Same as TestWait2 but wait for bots instead.
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest (
          "{inz:bot {:sleep 250 :assert false} ind:bot {:sleep 1000 :assert true} ing:bot {:sleep 500 :assert true} :wait $ing :wait $ind :wait $inz}",
          "");
      });
    }

    [Test]
    public void TestWaitExceptionsWithFibersAndTimeouts ()
    {
      // Wait with timeouts is a totally different implementation so this same thing needs
      // to be
      // tested again with timeouts.
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest (
          "{inz:fiber {:sleep 250 :assert false} ind:fiber {:sleep 1000 :assert true} ing:fiber {:sleep 500 :assert true} :1500 wait $ing :1500 wait $ind :1500 wait $inz}",
          "");
      });
    }

    [Test]
    public void TestWaitExceptionsWithBotsAndTimeouts ()
    {
      // Wait with timeouts is a totally different implementation so this same thing needs
      // to be
      // tested again with timeouts.
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest (
          "{inz:bot {:sleep 250 :assert false} ind:bot {:sleep 1000 :assert true} ing:bot {:sleep 500 :assert true} :1500 wait $ing :1500 wait $ind :1500 wait $inz}",
          "");
      });
    }

    [Test]
    public void TestTryFail ()
    {
      DoTest ("#status #data from try {<-900 fail \"fail with status 900\"}",
              "{status:900 data:[?\n    <<Custom,fail with status 900>>\n  ?]}");
    }

    [Test]
    public void TestTryFail1 ()
    {
      DoTest ("#status #data from try {<-fail \"rando failure message\"}",
              "{status:8 data:[?\n    <<Custom,rando failure message>>\n  ?]}");
    }

    [Test]
    [Ignore (
       "This test fails to expose the bugs in watchf all the time. Need a better test then implement buffer fixes.")
    ]
    public void TestWatchf ()
    {
      DoTest (
        "{h:exec \"mkdir test\" :cd \"test\" fsw:watchf pwd {} :exec \"touch foo\" create:waitf $fsw :\"foo\" save \"bar\" update:waitf $fsw :exec \"rm foo\" delete:waitf $fsw :cd \"..\" :exec \"rmdir test\" <-eval {create:#event #name from last $create update:#event #name from last $update delete:#event #name from last $delete}}",
        "{create:{event:\"created\" name:\"foo\"} update:{event:\"changed\" name:\"foo\"} delete:{event:\"deleted\" name:\"foo\"}}");
    }

    [Test]
    public void TestTryFailAssert ()
    {
      DoTest ("#status from try {<-assert #x = #y}", "{status:1}");
    }

    [Test]
    public void TestTryOk ()
    {
      DoTest ("#status #data from try {<-assert #x = #x}", "{status:0 data:true}");
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
      DoTest ("try try try {<-eval {a:1}}",
              "{status:0 data:{status:0 data:{status:0 data:{a:1}}}}");
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
      DoTest ("{b:bot {<-try {<-assert #x = #y}} <-0 < $b}", "true");
    }

    [Test]
    public void TestTryAsync1 ()
    {
      // The bot operator would continue from the exception and print twice.
      // This is not a good test because we can't check to see whether it printed once or
      // twice.
      DoTest ("{b:bot {<-try {<-assert #x = #y}} :print \"async try\" <-#status from wait $b}",
              "{status:1}");
    }

    [Test]
    public void TestTryAsync2 ()
    {
      DoTest ("{b:bot {<-try {<-assert #x = #x}} <-#status from wait $b}", "{status:0}");
    }

    [Test]
    public void TestTryAsync3 ()
    {
      DoTest ("{b:bot {<-try {<-#foo read 0}} :kill $b <-#status from wait $b}", "{status:2}");
    }

    [Test]
    public void TestTryAsync4 ()
    {
      DoTest ("{b:bot {<-try {<-#foo read 0}} :kill $b f:fiber {<-#status from wait $b} <-wait $f}",
              "{status:2}");
    }

    [Test]
    public void TestTryAssert ()
    {
      DoTest ("#status #data from try {<-assert false}",
              "{status:1 data:[?\n    <<Assert,Failed: assert false>>\n  ?]}");
    }

    [Test]
    public void TestBotWithChildFibers ()
    {
      // Would not be able to find f in the child fiber.
      // I should be able to wait on $b, and not have a sleep here.
      // Then the result of the test should be 11.
      // DoTest ("{b:bot {add_one:{<-$R + 1} f:fiber {<-add_one 10} :wait $f <-0} <-wait
      // $b}",
      // "11");
      DoTest ("{b:bot {add_one:{<-$R + 1} f:fiber {<-add_one 10} <-wait $f} <-wait $b}", "11");
    }

    [Test]
    public void TestFiberWithChildFibers ()
    {
      // Would not be able to find f in the child fiber.
      // I should be able to wait on $b, and not have a sleep here.
      // Then the result of the test should be 11.
      DoTest ("{p:fiber {<-eval {add_one:{<-$R + 1} c:fiber {<-add_one 10} <-wait $c}} <-wait $p}",
              "11");
      // DoTest ("{p:fiber {<-eval {add_one:{<-$R + 1} :fiber {<-add_one 10} <-wait 0}}
      // :sleep 1000
      // <-$p}", "1");
      // DoTest ("{add_ten:{<-$R + 10} :fiber {<-eval {add_one:{<-$R + 1} :fiber
      // {<-add_one 10}
      // <-wait 0}} :fiber {<-add_ten 10} :sleep 1000l <-0}", "0");
      // DoTest ("{serve_page:{request:httprecv $R header:httpheader $request args:eval
      // {file:(1l
      // substring $header.RawUrl) + \".o2\"} body:httpbody $request :bot {<-eval parse
      // load
      // $args.file} :$request httpsend \"<pre>\" + (#pretty format eval {header:$header
      // body:$body
      // args:$args}) + \"</pre>\" <-serve_page $R} :fiber {<-eval {add_one:{<-$R + 1}
      // :fiber
      // {<-add_one 10} <-wait 0}} :fiber {<-serve_page httpstart \"http://*:8080/\"}
      // <-wait 0}",
      // "0");
    }

    [Test]
    public void TestTryWaitKill ()
    {
      // Note use of sleep 0l to expose the _queue bug.
      DoTest (
        "{b:bot {<-try {w:{:#x write {i:++} <-w $R} :fiber {<-w 0} <-wait 0}} :sleep 0 :fiber {:#y dispatch 1 <-kill $b} :#y write {i:++} r:wait $b <-#status from $r}",
        "{status:2}");
    }

    [Test]
    public void TestTryWaitKill1 ()
    {
      // In this case the 0 fiber for the bot will return immediately resulting in a zero
      // status and
      // an actual result for the zero fiber of the child bot.
      DoTest (
        "{b:bot {<-try {w:{:#x write {i:++} <-w $R} :fiber {<-w 0} <-123}} :sleep 0 :fiber {:#y dispatch 1 <-kill $b} :#y write {i:++} <-wait $b}",
        "{status:0 data:123}");
    }

    [Test]
    public void TestTryWaitKill2 ()
    {
      // This is like the test above but with more fibers on the child bot.
      // Some of the kill bugs do not reproduce unless there are more than two child
      // fibers.
      DoTest (
        "{b:bot {<-try {w:{:#x write {i:++} <-w $R} :fiber {<-w 0} :fiber {<-w 0} :fiber {<-w 0} <-123}} :wait $b & 0 :kill $b <-wait $b}",
        "{status:0 data:123}");
    }

    [Test]
    public void TestTryWaitKill3 ()
    {
      DoTest (
        "{p:{serve:{b:bot {<-try {<-eval {<-2 + 3}}} f1:fiber {br:wait $b <-$br} <-wait $f1} r:wait fiber {<-serve #}} <-first #r from eval $p}",
        "{status:0 data:5}");
    }

    [Test]
    public void TestTryWaitKill4 ()
    {
      // This one exposed a bug in the bot number on the trailing closure.
      DoTest (
        "{p:{b:bot {<-try {<-eval {:12 :13}}} f:fiber {br:wait $b <-$br} r:wait $f} <-first #r from eval $p}",
        "{status:0 data:{:12 :13}}");
    }

    [Test]
    public void TestTryWaitKill5 ()
    {
      // Kill was yanking queued items that it should not.
      // We kill fiber f and then check to see that the others are still writing.
      DoTest (
        "{w:{:#x write {i:++} <-w $R} f0:fiber {<-w 0} f1:fiber {<-w 0} f2:fiber {<-w 0} :kill $f0 :#x dispatch 0 :#x dispatch 10 :kill $f1 :kill $f2 <-0}",
        "0");
    }

    [Test]
    public void TestTryWaitKill6 ()
    {
      // Much like test 5 except that f2 throws an exception rather than f0 getting
      // killed.
      DoTest (
        "{w:{:#x write {i:++} <-w $R} f0:fiber {<-w 0} f1:fiber {<-w 0} f2:fiber {<-assert false} :try {<-wait $f2} :#x dispatch 0 :#x dispatch 10 :kill $f0 :kill $f1 <-0}",
        "0");
    }

    [Test]
    public void TestKillAfterThrow ()
    {
      DoTest ("{b:bot {:assert false} :try {:wait $b} :kill $b <-0}", "0");
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
      DoTest (
        "{f:fiber {<-try {<-500 wait fiber {<-read #a}}} :#a write {x:1} <-#status get wait $f}",
        "0");
    }

    [Test]
    public void TestWaitBotNoTimeout ()
    {
      DoTest ("#status get try {<-500 wait bot {f:fiber {<-read #a} :#a write {x:1} <-wait $f}}",
              "0");
    }

    [Test]
    public void TestWaitWithConflictingResult1 ()
    {
      DoTest (
        "{f1:fiber {:read #a} f2:fiber {:try {<-200 wait $f1} :try {<-kill $f1}} :wait $f2 <-0}",
        "0");
      // The single exception is for the killed fiber
      NUnit.Framework.Assert.AreEqual (2, runner.ExceptionCount);
    }

    [Test]
    public void TestWaitWithConflictingResult2 ()
    {
      DoTest (
        "{p:{f1:fiber {:read #a} f2:fiber {:try {<-200 wait $f1} :try {<-kill $f1}} :wait $f2} :p {} :p {} <-0}",
        "0");
      NUnit.Framework.Assert.AreEqual (4, runner.ExceptionCount);
    }

    [Test]
    public void TestWaitWithConflictingResult3 ()
    {
      DoTest (
        "{p:{f1:fiber {out:#a read 0 <-$out} f2:fiber {:kill $f1 :try {:wait $f1} :#a write {x:0}} :wait $f2 <-0} :p {} :clear #a :p {} <-0}",
        "0");
      NUnit.Framework.Assert.AreEqual (4, runner.ExceptionCount);
    }

    [Test]
    public void TestWaitWithConflictingResult4 ()
    {
      DoTest (
        "{f:fiber {:#a read 0 :#m putm 1} :try {<-200 wait $f} :kill $f :#a write {x:1} :try {<-wait $f} :assert not hasm #m <-0}",
        "0");
    }

    // These three tests are still important but the module operator itself
    // can be removed since it was an abomination. Should work "out of the box" now.
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
    public void TestParseBadSyntax1 ()
    {
      DoTest ("eval \"data\" get try {:parse \"foo\"}",
              "\"<<Syntax,Invalid syntax around line 0 near the text 'foo'. Unfinished operator expression.>>\\n\"");
    }

    [Test]
    public void TestParseBadSyntax2 ()
    {
      DoTest ("eval \"data\" get try {:parse \"foo\" \"bad\"}",
              "\"<<Syntax,Invalid syntax around line 0 near the text 'bad'.>>\\n\"");
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
      // Example from http://en.wikipedia.org/wiki/Comma-separated_values
      // I think I am missing a test for an single double quote escaped between two other
      // double
      // quotes.
      // DoTest ("#csv parse \"Year,Make,Model,Description,Price\\n1997,Ford,E350,\\\"ac,
      // abs,
      // moon\\\",3000.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended
      // Edition\\\"\\\"\\\",\\\"\\\",4900.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended
      // Edition,
      // Very Large\\\"\\\"\\\",\\\"\\\",5000.00\\n1996,Jeep,Grand Cherokee,\\\"MUST
      // SELL!\\nair,
      // moon roof, loaded\\\",4799.00\"",
      //        "{Year:\"1997\" \"1999\" \"1999\" \"1996\" Make:\"Ford\" \"Chevy\"
      // \"Chevy\"
      // \"Jeep\" Model:\"E350\" \"\\\"Venture \\\"\\\"Extended Edition\\\"\\\"\\\"\"
      // \"\\\"Venture
      // \\\"\\\"Extended Edition, Very Large\\\"\\\"\\\"\" \"Grand Cherokee\"
      // Description:\"\\\"ac,
      // abs, moon\\\"\" \"\\\"\\\"\" \"\\\"\\\"\" \"\\\"MUST SELL!\\nair, moon roof,
      // loaded\\\"\"
      // Price:\"3000.00\" \"4900.00\" \"5000.00\" \"4799.00\"}");
      DoTest (
        "#csv parse \"Year,Make,Model,Description,Price\\n1997,Ford,E350,\\\"ac, abs, moon\\\",3000.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended Edition\\\"\\\"\\\",\\\"\\\",4900.00\\n1999,Chevy,\\\"Venture \\\"\\\"Extended Edition, Very Large\\\"\\\"\\\",\\\"\\\",5000.00\\n1996,Jeep,Grand Cherokee,\\\"MUST SELL!\\nair, moon roof, loaded\\\",4799.00\"",
        "{Year:\"1997\" \"1999\" \"1999\" \"1996\" Make:\"Ford\" \"Chevy\" \"Chevy\" \"Jeep\" Model:\"E350\" \"Venture \\\"\\\"Extended Edition\\\"\\\"\" \"Venture \\\"\\\"Extended Edition, Very Large\\\"\\\"\" \"Grand Cherokee\" Description:\"ac, abs, moon\" \"\" \"\" \"MUST SELL!\\nair, moon roof, loaded\" Price:\"3000.00\" \"4900.00\" \"5000.00\" \"4799.00\"}");

      // No terminal newline - this is not really valid csv, let's not worry about it.
      // DoTest ("#csv parse \"x,y,z\\nX,Y,\"", "{x:\"X\" y:\"Y\" z:\"\"}");
    }

    [Test]
    public void TestParseCSV1 ()
    {
      // Empty string at the end of a line.
      // \r\n for newlines.
      DoTest ("#csv parse \"a,b,c\\r\\nA,B,\\r\\n\"", "{a:\"A\" b:\"B\" c:\"\"}");
    }

    [Test]
    public void TestParseCSV2 ()
    {
      // Empty string at the beginning of a line.
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

    // JSON parsing test cases are from:
    // http://code.google.com/p/json-ply/source/browse/trunk/jsonply_test.py

    [Test]
    public void TestParseJSONEmptyArray ()
    {
      // Empty array
      DoTest ("#json parse \"[]\"", "{}");
    }

    [Test]
    public void TestParseJSONSingleToken ()
    {
      DoTest ("eval \"data\" get try {<-#json parse \"foo\\n\"}",
              "\"<<Syntax,Invalid syntax around line 0 near the text 'foo\\\\n'.>>\\n\"");
    }

    [Test]
    public void TestParseJSONStringArray ()
    {
      // Array of strings
      DoTest ("#json parse \"[\\\"a\\\",\\\"b\\\",\\\"c\\\"]\"", "{:\"a\" :\"b\" :\"c\"}");
    }

    [Test]
    public void TestParseJSONNumberArray ()
    {
      // Array of numbers
      DoTest ("#json parse \"[1, 2, 3.4e5]\"", "{:1.0 :2.0 :340000.0}");
    }

    [Test]
    public void TestParseJSONArrayOfArrays ()
    {
      // Array of arrays
      DoTest ("#json parse \"[[\\\"a\\\",\\\"b\\\",\\\"c\\\"]]\"", "{:{:\"a\" :\"b\" :\"c\"}}");
    }

    [Test]
    public void TestParseJSONArrayOfDicts ()
    {
      // Array of dicts
      DoTest ("#json parse \"[{\\\"a\\\":\\\"b\\\"},{\\\"c\\\":\\\"d\\\"}]\"",
              "{:{a:\"b\"} :{c:\"d\"}}");
    }

    [Test]
    public void TestParseJSONMixedArray ()
    {
      // Array of mixed itmems
      DoTest ("#json parse \"[1, true, {\\\"a\\\": \\\"b\\\"}, [\\\"c\\\"]]\"",
              "{:1.0 :true :{a:\"b\"} :{:\"c\"}}");
    }

    [Test]
    public void TestParseJSONEmptyString ()
    {
      // Empty dict
      DoTest ("#json parse \"\"", "{}");
    }

    [Test]
    public void TestParseJSONEmptyDict ()
    {
      // Empty dict
      DoTest ("#json parse \"{}\"", "{}");
    }

    [Test]
    public void TestParseJSONStringDict ()
    {
      // Dict of strings
      DoTest ("#json parse \"{\\\"a\\\":\\\"b\\\" \\\"c\\\":\\\"d\\\"}\"", "{a:\"b\" c:\"d\"}");
    }

    [Test]
    public void TestParseJSONNumberDict ()
    {
      // Dict of numbers
      DoTest ("#json parse \"{\\\"a\\\":1.0 \\\"b\\\":2.3}\"", "{a:1.0 b:2.3}");
    }

    [Test]
    public void TestParseJSONArrayDict ()
    {
      // Dict of arrays
      DoTest ("#json parse \"{\\\"a\\\": [\\\"b\\\", \\\"c\\\"], \\\"d\\\":[1.0, 2.3]}\"",
              "{a:{:\"b\" :\"c\"} d:{:1.0 :2.3}}");
    }

    [Test]
    public void TestParseJSONDictDict ()
    {
      // Dict of dicts
      DoTest ("#json parse \"{\\\"a\\\": {\\\"b\\\":\\\"c\\\"} \\\"d\\\": {\\\"e\\\":\\\"f\\\"}}\"",
              "{a:{b:\"c\"} d:{e:\"f\"}}");
    }

    [Test]
    public void TestParseJSONMixedDict ()
    {
      // Dict of mixed items
      DoTest (
        "#json parse \"{\\\"a\\\": true, \\\"b\\\": [1.0, 2.3], \\\"c\\\": {\\\"d\\\":null}}\"",
        "{a:true b:{:1.0 :2.3} c:{d:{}}}");
    }

    [Test]
    public void TestParseXML1 ()
    {
      // Single empty element
      DoTest ("#xml parse \"<a></a>\"", "{a:{:\"\"}}");
    }

    [Test]
    public void TestParseXML2 ()
    {
      // Empty element single tag
      DoTest ("#xml parse \"<a/>\"", "{a:{:\"\"}}");
    }

    [Test]
    public void TestParseXML3 ()
    {
      // Single element with content
      DoTest ("#xml parse \"<a>x</a>\"", "{a:{:\"x\"}}");
    }

    [Test]
    public void TestParseXML4 ()
    {
      // Single element with attribute
      DoTest ("#xml parse \"<a b=\\\"x\\\"></a>\"", "{a:{b:\"x\" :\"\"}}");
    }

    [Test]
    public void TestParseXML5 ()
    {
      // Multiple attributes in an element
      DoTest ("#xml parse \"<a b=\\\"x\\\" c=\\\"y\\\" d=\\\"z\\\">w</a>\"",
              "{a:{b:\"x\" c:\"y\" d:\"z\" :\"w\"}}");
    }

    [Test]
    public void TestParseXML6 ()
    {
      // Multiple elements in a document
      DoTest ("#xml parse \"<a>x</a><b>y</b><c>z</c>\"", "{a:{:\"x\"} b:{:\"y\"} c:{:\"z\"}}");
    }

    [Test]
    public void TestParseXML7 ()
    {
      // Multiple elements with different content types.
      DoTest ("#xml parse \"<a>x</a><b></b><c/>\"", "{a:{:\"x\"} b:{:\"\"} c:{:\"\"}}");
    }

    [Test]
    public void TestParseXML8 ()
    {
      // Nested elements
      DoTest ("#xml parse \"<a><b>x</b><c>y</c><d>z</d></a>\"",
              "{a:{:{b:{:\"x\"} c:{:\"y\"} d:{:\"z\"}}}}");
    }

    // Single empty element
    [Test]
    public void TestParseXml9 ()
    {
      DoTest ("#xml parse \"<a></a>\"", "{a:{:\"\"}}");
    }

    // Empty element single tag
    [Test]
    public void TestParseXml10 ()
    {
      DoTest ("#xml parse \"<a/>\"", "{a:{:\"\"}}");
    }

    // Single element with content
    [Test]
    public void TestParseXml11 ()
    {
      DoTest ("#xml parse \"<a>x</a>\"", "{a:{:\"x\"}}");
    }

    // Multiple elements in a document
    [Test]
    public void TestParseXml12 ()
    {
      DoTest ("#xml parse \"<a>x</a><b>y</b><c>z</c>\"", "{a:{:\"x\"} b:{:\"y\"} c:{:\"z\"}}");
    }

    // Multiple elements with different content types.
    [Test]
    public void TestParseXml13 ()
    {
      DoTest ("#xml parse \"<a>x</a><b></b><c/>\"", "{a:{:\"x\"} b:{:\"\"} c:{:\"\"}}");
    }

    // Single nested multiple elements
    [Test]
    public void TestParseXml14 ()
    {
      DoTest ("#xml parse \"<a><b>x</b><c>y</c><d>z</d></a>\"",
              "{a:{:{b:{:\"x\"} c:{:\"y\"} d:{:\"z\"}}}}");
    }

    // Multiple nested single elements
    [Test]
    public void TestParseXml15 ()
    {
      DoTest ("#xml parse \"<a><b><c>x</c></b></a>\"", "{a:{:{b:{:{c:{:\"x\"}}}}}}");
    }

    // Multiple nested multiple elements
    [Test]
    public void TestParseXml16 ()
    {
      DoTest ("#xml parse \"<a><b><c>x</c><d>y</d></b><e/></a>\"",
              "{a:{:{b:{:{c:{:\"x\"} d:{:\"y\"}}} e:{:\"\"}}}}");
    }

    // Single empty element
    [Test]
    public void TestParseXml17 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"></a>\"", "{a:{f:\"i\" :\"\"}}");
    }

    // Empty element single tag
    [Test]
    public void TestParseXml18 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"/>\"", "{a:{f:\"i\" :\"\"}}");
    }

    // Single element with content
    [Test]
    public void TestParseXml19 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a>\"", "{a:{f:\"i\" :\"x\"}}");
    }

    // Multiple elements in a document
    [Test]
    public void TestParseXml20 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a><b g=\\\"j\\\">y</b><c h=\\\"k\\\">z</c>\"",
              "{a:{f:\"i\" :\"x\"} b:{g:\"j\" :\"y\"} c:{h:\"k\" :\"z\"}}");
    }

    // Multiple elements with different content types.
    [Test]
    public void TestParseXml21 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\">x</a><b g=\\\"j\\\"></b><c h=\\\"k\\\"/>\"",
              "{a:{f:\"i\" :\"x\"} b:{g:\"j\" :\"\"} c:{h:\"k\" :\"\"}}");
    }

    // Single nested multiple elements
    [Test]
    public void TestParseXml22 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b g=\\\"j\\\">x</b><c>y</c><d h=\\\"k\\\">z</d></a>\"",
              "{a:{f:\"i\" :{b:{g:\"j\" :\"x\"} c:{:\"y\"} d:{h:\"k\" :\"z\"}}}}");
    }

    // Multiple nested single elements
    [Test]
    public void TestParseXml23 ()
    {
      DoTest ("#xml parse \"<a f=\\\"i\\\"><b g=\\\"j\\\"><c h=\\\"k\\\">x</c></b></a>\"",
              "{a:{f:\"i\" :{b:{g:\"j\" :{c:{h:\"k\" :\"x\"}}}}}}");
    }

    // Multiple nested multiple elements
    [Test]
    public void TestParseXml24 ()
    {
      DoTest (
        "#xml parse \"<a f=\\\"i\\\"><b><c g=\\\"j\\\">x</c><d>y</d></b><e h=\\\"k\\\"/></a>\"",
        "{a:{f:\"i\" :{b:{:{c:{g:\"j\" :\"x\"} d:{:\"y\"}}} e:{h:\"k\" :\"\"}}}}");
    }

    // Skip over xml declarations (shouldn't we really incorporate the header info into
    // the output
    // though?)
    [Test]
    public void TestParseXml25 ()
    {
      DoTest (
        "#xml parse \"<?xml version=\\\"1.0\\\" encoding=\\\"UTF-8\\\" standalone=\\\"no\\\" ?><a f=\\\"i\\\"><b><c g=\\\"j\\\">x</c><d>y</d></b><e h=\\\"k\\\"/></a>\"",
        "{a:{f:\"i\" :{b:{:{c:{g:\"j\" :\"x\"} d:{:\"y\"}}} e:{h:\"k\" :\"\"}}}}");
    }

    // Colons allowed in xml attribute names
    [Test]
    public void TestParseXml26 ()
    {
      DoTest ("#xml parse \"<a f:x=\\\"i\\\"></a>\"", "{a:{'f:x':\"i\" :\"\"}}");
    }

    [Test]
    public void TestParseXml27 ()
    {
      DoTest ("#xml parse \"<q url=\\\"N:\\\\foo\\\\bar\\\\\\\" r:s=\\\"baz\\\"/>\\n\"",
              "{q:{url:\"N:\\\\foo\\\\bar\\\\\" 'r:s':\"baz\" :\"\"}}");
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
      DoTest ("#pretty format {:{a:1.0 b:2.0 c:3.0}}",
              "\"{\\n  :{\\n    a:1.0\\n    b:2.0\\n    c:3.0\\n  }\\n}\"");
    }

    [Test]
    public void TestFormatPretty4 ()
    {
      DoTest ("#pretty format {:{a:1.0 1.0 1.0 b:2.0 2.0 2.0 c:3.0 3.0 3.0}}",
              "\"{\\n  :{\\n    a:1.0 1.0 1.0\\n    b:2.0 2.0 2.0\\n    c:3.0 3.0 3.0\\n  }\\n}\"");
    }

    [Test]
    public void TestFormatPretty5 ()
    {
      // [
      //  T | S|   a b
      //  0l #x   1l "2"
      //  1l #x  10l "20"
      //  2l #x 100l "200"
      // ]
      DoTest ("#pretty format [E|S|a b 0 #x 1 \"2\" 1 #x 10 \"20\" 2 #x 100 \"200\"]",
              "\"[\\n   E|S |  a b\\n   0 #x   1 \\\"2\\\"\\n   1 #x  10 \\\"20\\\"\\n   2 #x 100 \\\"200\\\"\\n]\"");
    }

    [Test]
    public void TestFormatPretty6 ()
    {
      DoTest ("#pretty format {u:[S|x #a 0]}", "\"{\\n  u:[\\n    S | x\\n    #a  0\\n  ]\\n}\"");
    }

    [Test]
    public void TestFormatText ()
    {
      DoTest ("\"\\r\" \"\" replace #text format \"line one\" \"line two\" \"line three\"",
              "\"line one\\nline two\\nline three\\n\"");
    }

    [Test]
    public void TestFormatFragment ()
    {
      DoTest ("#fragment format {:[S|x #a 1 #b 2 #c 3]}",
              "\":[\\n  S | x\\n  #a  1\\n  #b  2\\n  #c  3\\n]\\n\"");
    }

    [Test]
    public void TestFormatTextCrlf ()
    {
      DoTest ("#textcrlf format \"line one\" \"line two\" \"line three\"",
              "\"line one\\r\\nline two\\r\\nline three\\r\\n\"");
    }

    [Test]
    public void TestFormatTextLf ()
    {
      DoTest ("#textlf format \"line one\" \"line two\" \"line three\"",
              "\"line one\\nline two\\nline three\\n\"");
    }

    [Test]
    public void TestFormatLog ()
    {
      // The log format option omits the header and uses spaces instead of commas for the
      // delimiter.
      DoTest ("#log format [a b c 0 \"x y z\" #foo 1 \"u v w\" #bar 2 \"s t u\" #baz]",
              "\"0 \\\"x y z\\\" foo\\n1 \\\"u v w\\\" bar\\n2 \\\"s t u\\\" baz\\n\"");
    }

    // I was going to make binary a variant of format.
    // But binary will return a byte vector and format returns a string vector.
    // I do not want the output from an operator to vary based on the arguments.
    // (varying based on the TYPE of the arguments is ok).
    // So use a different operator name to format binary data.
    [Test]
    public void TestBinaryFixed1 ()
    {
      DoTest ("parse binary -1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000",
              "-1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000");
    }

    [Test]
    public void TestBinaryFixed2 ()
    {
      DoTest (
        "parse binary -1000000000.0 -2000000.0 -3000.0 -4.0 0.0 5.0 6000.0 7000000.0 8000000000.0",
        "-1000000000.0 -2000000.0 -3000.0 -4.0 0.0 5.0 6000.0 7000000.0 8000000000.0");
    }

    [Test]
    public void TestBinaryFixed3 ()
    {
      DoTest ("parse binary -1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000m",
              "-1000000000 -2000000 -3000 -4 0 5 6000 7000000 8000000000m");
    }

    [Test]
    public void TestBinaryFixed4 ()
    {
      DoTest ("parse binary -12345678.9 -234567.89 -3.456 -0.4 0 5 6.789 789.1234 8912.345678m",
              "-12345678.9 -234567.89 -3.456 -0.4 0 5 6.789 789.1234 8912.345678m");
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

    // Vectors of variable length types like string and reference.
    [Test]
    public void TestBinaryString1 ()
    {
      DoTest ("parse binary \"a\" \"ba\" \"cba\" \"dcba\"", "\"a\" \"ba\" \"cba\" \"dcba\"");
    }

    // test with repeated values
    [Test]
    public void TestBinaryString2 ()
    {
      DoTest ("parse binary \"x\" \"yy\" \"zzz\" \"yy\" \"x\" \"yy\" \"zzz\"",
              "\"x\" \"yy\" \"zzz\" \"yy\" \"x\" \"yy\" \"zzz\"");
    }

    // With simple fixed types only.
    [Test]
    public void TestBinarySymbol1 ()
    {
      DoTest ("parse binary #1,2.0,true", "#1,2.0,true");
    }

    // With fixed types including decimal
    [Test]
    public void TestBinarySymbol2 ()
    {
      DoTest ("parse binary #1,2.0,3m,true", "#1,2.0,3m,true");
    }
    // With more than one value
    [Test]
    public void TestBinarySymbol3 ()
    {
      DoTest ("parse binary #1,2.0,3m #4,5.0,6m #7,8.0,9m", "#1,2.0,3m #4,5.0,6m #7,8.0,9m");
    }
    // With repeating values
    [Test]
    public void TestBinarySymbol4 ()
    {
      DoTest ("parse binary #1,2.0,3m #4,5.0,6m #1,2.0,3m", "#1,2.0,3m #4,5.0,6m #1,2.0,3m");
    }
    // With strings (variable length)
    [Test]
    public void TestBinarySymbol5 ()
    {
      DoTest ("parse binary #1,2.0,3m,true,abcdef", "#1,2.0,3m,true,abcdef");
    }

    // Mixing strings and repeated values.
    // DoTest ("parse binary #abc #de,f #abc", "#abc #de,f #abc");

    // block as expression holder.
    [Test]
    public void TestBinaryContainers1 ()
    {
      DoTest ("parse binary {<-1 + 2.0}", "{<-1 + 2.0}");
    }

    // block with mixed variables.
    [Test]
    public void TestBinaryContainer2 ()
    {
      DoTest (
        "parse binary {a:1 2 3 b:4.0 5.0 6.0 c:7 8 9m d:true false e:\"this\" \"a\" \"string\" f:#x,y,z #u,v,w #r,s,t}",
        "{a:1 2 3 b:4.0 5.0 6.0 c:7 8 9m d:true false e:\"this\" \"a\" \"string\" f:#x,y,z #u,v,w #r,s,t}");
    }

    // block with complex expression.
    [Test]
    public void TestBinaryContainers3 ()
    {
      DoTest ("parse binary {<-(1 - 2 * 4) - 3}", "{<-(1 - 2 * 4) - 3}");
    }

    // nested blocks
    [Test]
    public void TestBinaryContainers4 ()
    {
      DoTest ("parse binary {l:{a:0 b:1 c:2} d:{d:3.0 e:4.0 f:5.0} m:{g:6m h:7m i:8m}}",
              "{l:{a:0 b:1 c:2} d:{d:3.0 e:4.0 f:5.0} m:{g:6m h:7m i:8m}}");
    }

    // cubes
    [Test]
    public void TestBinaryCube1 ()
    {
      DoTest ("parse binary [S|l d m #a 10 -- -- #a 11 21.0 -- #a 12 22.0 32m]",
              "[S|l d m #a 10 -- -- #a 11 21.0 -- #a 12 22.0 32m]");
    }

    [Test]
    public void TestBinaryCube2 ()
    {
      DoTest ("parse binary [E|S|l d m 0 #a 10 -- -- 1 #a 11 21.0 -- 2l #a 12 22.0 32m]",
              "[E|S|l d m 0 #a 10 -- -- 1 #a 11 21.0 -- 2 #a 12 22.0 32m]");
    }

    [Test]
    public void TestCompare0 ()
    {
      DoTest ("\"aaa\\n\" compare \"bbb\\n\"",
              "[op old new \"DELETE\" \"aaa\" -- \"INSERT\" -- \"bbb\" \"EQUAL\" \"\\n\" \"\\n\"]");
    }

    [Test]
    public void TestCompare1 ()
    {
      DoTest ("\"aba\" compare \"abc\"",
              "[op old new \"EQUAL\" \"ab\" \"ab\" \"DELETE\" \"a\" -- \"INSERT\" -- \"c\"]");
    }

    [Test]
    public void TestCompare2 ()
    {
      DoTest ("\"a\nx\nb\n\" compare \"a\nb\nc\n\"",
              "[op old new \"EQUAL\" \"a\\n\" \"a\\n\" \"DELETE\" \"x\\n\" -- \"EQUAL\" \"b\\n\" \"b\\n\" \"INSERT\" -- \"c\\n\"]");
    }

    [Test]
    public void TestCompare3 ()
    {
      DoTest ("\"aaa\nddd\n\" compare \"aaa\nbbb\nccc\nddd\n\"",
              "[op old new \"EQUAL\" \"aaa\\n\" \"aaa\\n\" \"INSERT\" -- \"bbb\\n\" \"INSERT\" -- \"ccc\\n\" \"EQUAL\" \"ddd\\n\" \"ddd\\n\"]");
    }

    [Test]
    public void TestCompare4 ()
    {
      DoTest ("\"aaa\\nbbb\\nccc\\nddd\\n\" compare \"aaa\\nddd\\n\"",
              "[op old new \"EQUAL\" \"aaa\\n\" \"aaa\\n\" \"DELETE\" \"bbb\\n\" -- \"DELETE\" \"ccc\\n\" -- \"EQUAL\" \"ddd\\n\" \"ddd\\n\"]");
    }

    [Test]
    public void TestCompare5 ()
    {
      DoTest ("\"aaaaaaaa0\" compare \"1aaaaaaaa2\"",
              "[op new old \"INSERT\" \"1\" -- \"EQUAL\" \"aaaaaaaa\" \"aaaaaaaa\" \"DELETE\" -- \"0\" \"INSERT\" \"2\" --]");
    }

    [Test]
    public void TestCompare6 ()
    {
      DoTest ("\"aaaaaaaa0\" compare \"aaaaaaaa2\"",
              "[op old new \"EQUAL\" \"aaaaaaaa\" \"aaaaaaaa\" \"DELETE\" \"0\" -- \"INSERT\" -- \"2\"]");
    }

    [Test]
    public void TestCompare7 ()
    {
      DoTest (
        "\"[?\n  first line\n  second line\n\" compare \"[?\n  first line \n  second line\n\"",
        "[op old new \"EQUAL\" \"[?\\n\" \"[?\\n\" \"EQUAL\" \"  first line\" \"  first line\" \"INSERT\" -- \" \" \"EQUAL\" \"\\n\" \"\\n\" \"EQUAL\" \"  second line\\n\" \"  second line\\n\"]");
    }

    [Test]
    public void TestGetm ()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate ()
      {
        DoTest ("getm #foo", "{}");
      });
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
      // This was to expose a race condition in exec.
      // It would return zero if the process exited before waitx registered.
      for (int i = 0; i < 10; ++i)
      {
        DoTest (
          "{sh:startx \"bash\" :$sh writex \"exit 1\n\" <-#status #data from try {<-waitx $sh}}",
          "{status:1 data:[?\n    <<Exec,exit status 1>>\n  ?]}");
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
    [Ignore (
       "There some issue spawning a bash shell under the debugger env. Would like to know why.")]
    public void TestExecBash ()
    {
      DoTest ("{sh:startx \"sh\" :$sh writex \"set\\\\nmkdir foo\" <-try {<-waitx $sh}}",
              "{status:1 data:~s}");
    }
#endif
  }
}

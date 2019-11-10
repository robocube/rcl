
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  [TestFixture]
  public class EvalTest
  {
    [Test]
    public void TestSimpleMonadicExpressionWithLiteral ()
    {
      DoEvalTest("sum 1 2 3", "6");
    }

    [Test]
    public void TestSimpleDyadicExpressionWithLiteral ()
    {
      DoEvalTest("1 2 3 + 4 5 6", "5 7 9");
    }

    [Test]
    public void TestNestedMonadicExpressions ()
    {
      DoEvalTest("not not true", "true");
    }
    
    [Test]
    public void TestMinusProblem ()
    {
      //The key thing is whether whitespace precedes the minus sign.
      //I think it's worth giving the lexer a single token lookback for
      //this purpose since this will confuse some people right off the bat.
      DoEvalTest("1-2", "-1");
    }
    
    [Test]
    public void TestMeaninglessParensWithMultipleLiterals ()
    {
      DoEvalTest("(1 - 2 - 3)", "2");
    }

    [Test]
    public void TestMeaninglessParensAroundVector ()
    {
      DoEvalTest("(1 2 3)", "1 2 3");
    }

    [Test]
    public void TestMixedNestingExpressions ()
    {
      DoEvalTest("(1 - 2 - 3) - 4", "-2");
    }
    
    [Test]
    public void TestMultipleNestedExpressions ()
    {
      DoEvalTest("((1 - 2 - 3) - 4) - 5", "-7");
    }

    [Test]
    public void TestMultipleMixedNestedExpressions ()
    {
      DoEvalTest("((1 - 2) - 3 - 4) - 5", "-5");
    }

    [Test]
    public void TestMeaninglessParensInComplexExpression ()
    {
      DoEvalTest("((1 - 2) - (3 - 4)) - 5", "-5");
    }

    [Test]
    public void TestEmptyBlock ()
    {
      DoEvalTest("{}", "{}");
    }

    [Test]
    public void TestBlockWithOneVariable ()
    {
      DoEvalTest("{x:1}", "{x:1}");
    }

    [Test]
    public void TestBlockWithMultipleVariables ()
    {
      DoEvalTest("{x:1 y:2 z:3}", "{x:1 y:2 z:3}");
    }

    [Test]
    public void TestNestedEmptyBlock ()
    {
      DoEvalTest("{x:{}}", "{x:{}}");
    }

    [Test]
    public void TestNestedBlock ()
    {
      DoEvalTest("{x:{a:1}}", "{x:{a:1}}");
    }

    [Test]
    public void TestNestedBlockWithExpression ()
    {
      DoEvalTest("{x:eval {a:1 - 2}}", "{x:{a:-1}}");
    }

    [Test]
    public void TestNestedBlockWithMultipleVariables ()
    {
      DoEvalTest("{x:{a:1 b:2}}", "{x:{a:1 b:2}}");
    }

    [Test]
    public void TestNestedBlockWithMultipleVariablesBothLevels ()
    {
      DoEvalTest("{x:{a:1 b:2} y:3}", "{x:{a:1 b:2} y:3}");
    }

    [Test]
    public void TestImpliedBlock ()
    {
      DoEvalTest("x:1", "{x:1}");
    }

    [Test]
    public void TestUnnamedBlock ()
    {
      DoEvalTest("{:1}", "{:1}");
    }

    [Test]
    public void TestUnnamedBlockWithTwoVariables ()
    {
      DoEvalTest("{:1 :2}", "{:1 :2}");
    }

    [Test]
    public void TestUnnamedBlockWithTwoExpressions ()
    {
      DoEvalTest("{:1 + 2 :3 + 4}", "{:3 :7}");
    }

    [Test]
    public void TestUnnamedImpliedBlock ()
    {
      DoEvalTest(":1", "{:1}");
    }

    [Test]
    public void TestAssignAtEndOfBlock ()
    {
      DoEvalTest("{:1 :2}", "{:1 :2}");
    }

    [Test]
    public void TestExpressionInBlocks ()
    {
      DoEvalTest("{x:1 + 2}", "{x:3}");
    }

    [Test]
    public void TestTwiceNestedEmptyBlock ()
    {
      DoEvalTest("{x:1 y:{a:{}}}", "{x:1 y:{a:{}}}");
    }

    [Test]
    public void TestBlocksAsArguments ()
    {
      DoEvalTest("{a:1 b:2 c:3} = {a:1 b:2 c:4}", "false");
    }

    [Test]
    public void TestBlocksAsArgumentsNotEvaluated ()
    {
      DoEvalTest("{x:1.0 + 2.0} & {y:3.0}", "{x:1.0 + 2.0 y:3.0}");
    }

    [Test]
    public void TestBlocksAsArgumentsNested()
    {
      DoEvalTest("{x:{} = {}}", "{x:true}");
    }

    [Test]
    public void TestReferenceToReference()
    {
      DoEvalTest("{a:1 b:$a c:$b}", "{a:1 b:1 c:1}");
    }

    [Test]
    public void TestThisContext ()
    {
      DoEvalTest ("{go:{<-\"The \" + $animal + \" goes \" + $sound} duck:eval {animal:\"duck\" sound:\"quack\" go:$go} pig:eval {animal:\"pig\" sound:\"oink\" go:$go} chicken:eval {animal:\"chicken\" sound:\"cluck\" go:$go} <-eval {:duck.go # :pig.go # :chicken.go #}}",
                  "{:\"The duck goes quack\" :\"The pig goes oink\" :\"The chicken goes cluck\"}");
    }

    [Test]
    public void TestThisContextDynamic ()
    {
      DoEvalTest ("{go:{<-\"The \" + $animal + \" goes \" + $sound} duck:{animal:\"duck\" sound:\"quack\"} pig:{animal:\"pig\" sound:\"oink\"} chicken:{animal:\"chicken\" sound:\"cluck\"} <-eval {:$duck eval $go :$pig eval $go :$chicken eval $go}}",
                  "{:\"The duck goes quack\" :\"The pig goes oink\" :\"The chicken goes cluck\"}");
    }

    [Test]
    public void TestQuote ()
    {
      DoEvalTest ("{x::1 + 1}", "{x:1 + 1}");
    }

    [Test]
    public void TestQuote1 ()
    {
      DoEvalTest ("{x::1 + 1 y:1 + 1}", "{x:1 + 1 y:2}");
    }

    [Test]
    public void TestYiote ()
    {
      DoEvalTest ("{x::1 + 1 <-:$x}", "{x:1 + 1 <-$x}");
    }

    [Test]
    public void TestYiyi ()
    {
      DoEvalTest ("{x:1 + 1 <--$x}", "{x:2 <-2}");
    }

    [Test]
    public void TestYiyi1 ()
    {
      DoEvalTest ("{<--1 + 1}", "{<-2}");
    }

    [Test]
    public void TestMonad ()
    {
      DoEvalTest ("{x:\"not\" monad true false true", "{x:not true false true}");
    }

    [Test]
    public void TestDyad ()
    {
      DoEvalTest ("{x:\"+\" dyad {l:1 r:2}", "{x:1 + 2}");
    }

    [Test]
    public void TestEvalX ()
    {
      DoEvalTest ("eval \\x00 \\x01 \\x02", "\\x00 \\x01 \\x02");
    }

    [Test]
    public void TestEvalKX ()
    {
      DoEvalTest ("{} eval \\x00 \\x01 \\x02", "\\x00 \\x01 \\x02");
    }

    [Test]
    public void TestEvalD ()
    {
      DoEvalTest ("eval 1.0 2.0 3.0", "1.0 2.0 3.0");
    }

    [Test]
    public void TestEvalKD ()
    {
      DoEvalTest ("{} eval 1.0 2.0 3.0", "1.0 2.0 3.0");
    }

    [Test]
    public void TestEvalL ()
    {
      DoEvalTest ("eval 1 2 3", "1 2 3");
    }

    [Test]
    public void TestEvalKL ()
    {
      DoEvalTest ("{} eval 1 2 3", "1 2 3");
    }

    [Test]
    public void TestEvalM ()
    {
      DoEvalTest ("eval 1 2 3m", "1 2 3m");
    }

    [Test]
    public void TestEvalKM ()
    {
      DoEvalTest ("{} eval 1 2 3m", "1 2 3m");
    }

    [Test]
    public void TestEvalB ()
    {
      DoEvalTest ("eval true false true", "true false true");
    }

    [Test]
    public void TestEvalKB ()
    {
      DoEvalTest ("{} eval true false true", "true false true");
    }

    [Test]
    public void TestEvalS ()
    {
      DoEvalTest ("eval \"a\" \"b\" \"c\"", "\"a\" \"b\" \"c\"");
    }

    [Test]
    public void TestEvalKS ()
    {
      DoEvalTest ("{} eval \"a\" \"b\" \"c\"", "\"a\" \"b\" \"c\"");
    }

    [Test]
    public void TestEvalY ()
    {
      DoEvalTest ("eval #a #b #c", "#a #b #c");
    }

    [Test]
    public void TestEvalKY ()
    {
      DoEvalTest ("{} eval #a #b #c", "#a #b #c");
    }

    [Test]
    public void TestEvalT ()
    {
      DoEvalTest (
        "eval 2016.09.23 09:48 2016.09.23 09:48 2016.09.23 09:48", 
        "2016.09.23 09:48:00 2016.09.23 09:48:00 2016.09.23 09:48:00");
    }

    [Test]
    public void TestEvalKT ()
    {
      DoEvalTest (
        "{} eval 2016.09.23 09:48 2016.09.23 09:48 2016.09.23 09:48", 
        "2016.09.23 09:48:00 2016.09.23 09:48:00 2016.09.23 09:48:00");
    }

    [Test]
    public void TextApplyKL ()
    {
      DoEvalTest ("{<-$R where $R > 2} apply 0 1 2 3 4 5", "3 4 5");
    }

    [Test]
    public void TextApplyKD ()
    {
      DoEvalTest ("{<-$R where $R > 2.0} apply 0.0 1.0 2.0 3.0 4.0 5.0", "3.0 4.0 5.0");
    }

    [Test]
    public void TextApplyKM ()
    {
      DoEvalTest ("{<-$R where $R > 2.0m} apply 0.0 1.0 2.0 3.0 4.0 5.0m", "3 4 5m");
    }

    [Test]
    public void TextApplyKS ()
    {
      DoEvalTest ("{<-$R where $R like \"foo*\"} apply \"foobar\" \"foobaz\" \"fazban\" \"farbat\"", "\"foobar\" \"foobaz\"");
    }

    [Test]
    public void TextApplyKY ()
    {
      DoEvalTest ("{<-$R where 1 == long 1 part $R} apply #x,1,foo #y,1,bar #z,2,baz", "#x,1,foo #y,1,bar");
    }

    [Test]
    public void TextApplyKT ()
    {
      DoEvalTest ("{<-$R where $R < 2017.08.26} apply 2017.08.24 2017.08.25 2017.08.26", "2017.08.24 2017.08.25");
    }

    [Test]
    public void TextApplyKX ()
    {
      DoEvalTest ("{<-$R where $R < \\x02} apply \\x00 \\x01 \\x02", "\\x00 \\x01");
    }

    [Test]
    public void TextApplyKB ()
    {
      DoEvalTest ("{<-not $R} apply true false true false true false", "false true false true false true");
    }

    [Test]
    public void TestApplyKK ()
    {
      DoEvalTest ("{<-$R where (names $R) like \"foo*\"} apply {foobar:1 fazban:2 foobad:3}", "{foobar:1 foobad:3}");
    }

    [Test]
    public void TestApplyTV ()
    {
      DoEvalTest ("{<-[? aaa [! format $R !] aaa ?] apply 111", "\" aaa 111 aaa \"");
    }

    [Test]
    public void TestApplyOV ()
    {
      DoEvalTest ("(\"+\" dyad {l:10 r:$R}) apply 1 2 3", "11 12 13");
    }

    [Test]
    public void TestApplyRV ()
    {
      DoEvalTest ("(reference \"R\" \"b\") apply {a:1 b:2 c:3}", "2");
    }

//[Test]
//#if (!__MonoCS__)
    //[ExpectedException(typeof(RCRuntimeException), "Unable to resolve name s.x")]
//#else
    //[ExpectedException(ExceptionType=typeof(System.Exception), ExpectedMessage="Unable to resolve name s.x")]
//#endif
    [Test]
    public void TestReferenceToMissingColumn()
    {
      DoEvalTest ("{u:[S|a #x 0] <-$u.x}", "[]");
    }

    [Test]
    public void TestReferenceToEmptyCube ()
    {
      DoEvalTest ("{u:[] <-$u.x}", "[]");
    }

    [Test]
    public void TestReferenceToTimelineColumn()
    {
      DoEvalTest ("{s:[S|a #x 0] <-$s.S}", "#x");
    }

    [Test]
    public void TestEvalBlock()
    {
      DoEvalTest("{a:2 b:$a + $a}", "{a:2 b:4}");
    }

    [Test]
    public void TestEvalBlockNested()
    {
      DoEvalTest("{x:1 y:2 z:eval {a:$x + $y}}", "{x:1 y:2 z:{a:3}}");
    }

    [Test]
    public void TestEvalBlockNested1 ()
    {
      DoEvalTest ("{a:eval {<-1 + 1}}", "{a:2}");
    }

    [Test]
    public void TestEvalBlockNested2 ()
    {
      DoEvalTest ("{a:eval {<-eval {<-1 + 1}}}", "{a:2}");
    }

    [Test]
    public void TestEvalBlockYield ()
    {
      //No eval before the <-, should not be evaluated.
      DoEvalTest ("{x:1 y:2 <-{a:$x + $y}}", "{a:$x + $y}");
    }

    [Test]
    public void TestVariableOverriding ()
    {
      //The expression should see the 2d $x, not the 1l $x.
      DoEvalTest ("{x:1 y:eval {x:2.0 z:eval {a:$x + $x}}}", "{x:1 y:{x:2.0 z:{a:4.0}}}");
    }

    [Test]
    public void TestBlockTypeInference()
    {
      DoEvalTest("{x:eval {<-1.0} <-$x - $x}", "0.0");
    }

    [Test]
    public void TestDyadicOpInTakeBlock()
    {
      DoEvalTest ("#lock take {<-true switch {:1+2 :3+4}}", "3");
    }

    [Test]
    public void TestYieldFromTakeNoOp()
    {
      DoEvalTest ("#lock take {<-1}", "1");
    }

    [Test]
    public void TestUserOp()
    {
      //This calculates an average as a user defined operator.
      DoEvalTest("{f:{<-(sum $R) / count $R} <-f 1.0 2.0 3.0}", "2.0");
    }

    [Test]
    public void TestUserOpVariableCapture()
    {
      //Test for unwanted variable capture in expression evaluation.
      DoEvalTest("{x:1.0 2.0 3.0 y:4.0 5.0 6.0 f:{<-$R - $L} <-$x f $y}", "3.0 3.0 3.0");
    }

    [Test]
    public void TestUserOpWithTake()
    {
      DoEvalTest ("{f:{x:#a take {<-1 + 2} <-0} <-f {}}", "0");
    }

    [Test]
    public void TestUserOpWithFiber()
    {
      DoEvalTest ("{f:{x:fiber {<-1 + 2} :wait $x <-0} <-f {}}", "0");
    }

    [Test]
    public void TestUserOpWithEval()
    {
      DoEvalTest ("{f:{x:eval {<-1 + 2} <-0} <-f {}}", "0");
    }

    [Test]
    public void TestUserOpWithEval1 ()
    {
      DoEvalTest ("{o:{x:1 f:{<-eval {y:$x + 2}}} <-o.f {}}", "{y:3}");
    }

    [Test]
    public void TestUserOpWithEvalModule1 ()
    {
      //DoEvalTest ("{build:eval {actions:module {varA:\"a\" A:{:print $R.inputA + \" - result\"} B:{:A eval {inputA:$varA}} define:$actions} <-#define switch $actions} <-build.B {}}", "");
      //DoEvalTest ("{n:eval {m:module {x:1 f0:{:$R.y} f1:{:f0 eval {y:$x}} define:$m} <-#define switch $m} <-n.f1 {}}", "{:{y:1}}");
      DoEvalTest ("{n:eval {m:module {x:1 f0:{y:$R.y + 2} f1:{:f0 eval {y:$x}}} <-$m} <-n.f1 {}}", "{:{y:3}}");
    }

    [Test]
    public void TestUserOpWithEvalModule1WithoutModule ()
    {
      //Lol - Obviously this idiom is better without module.
      //Just look how much cruftiness goes away.
      DoEvalTest ("{n:{x:1 f0:{y:$R.y + 2} f1:{:f0 eval {y:$x}}} <-n.f1 {}}", "{:{y:3}}");
    }

    [Test]
    public void TestUserOpWithSwitch0 ()
    {
      DoEvalTest ("{f:{x:true switch {:1 + 2} <-0} <-f {}}", "0");
    }

    [Test]
    public void TestUserOpWithSwitch1 ()
    {
      //DoEvalTest ("{f:{g:{<-$R} <-g $R} <-f 1 2 3}", "1 2 3");
      DoEvalTest ("{f:{g:{<-$R} <-true switch {:g $R}} <-f 1 2 3}", "1 2 3");
    }

    [Test]
    public void TestUserOpWithSwitch2 ()
    {
      DoEvalTest ("{f:{o:{x:1 g:{<-$R + $x}} <-true switch {:o.g $R}} <-f 1 2 3}", "2 3 4");
    }

    [Test]
    public void TestUserOpNested ()
    {
      DoEvalTest ("{lib:{f:{<-$L * $R}} <-3 lib.f 2}", "6");
    }

    [Test]
    public void TestTakeWithSwitch()
    {
      DoEvalTest ("#lock take {x:true switch {:1 + 2} <-0}", "0");
    }

    [Test]
    public void TestSwitchWithSwitch()
    {
      DoEvalTest ("true switch {:{x:true switch {:1 + 2} <-0}}", "0");
    }

    [Test]
    public void TestSwitchAndEval ()
    {
      DoEvalTest ("true switch {:{x:1 <-eval {<-$x + 1}}}", "2");
    }

    [Test]
    public void TestSwitchAndEval1 ()
    {
      DoEvalTest ("{f:{x:1 k:eval {x:$x + 1} <-$k} <-true switch {:f #}}", "{x:2}");
    }

    [Test]
    public void TestSwitchAndEval2 ()
    {
      DoEvalTest ("{f:{x:1 <-eval {x:$x + 1}} <-true switch {:f #}}", "{x:2}");
    }

    [Test]
    public void TestNestedSwitch ()
    {
      DoEvalTest ("{f:{<-true switch {:$L - $R}} <-13 f 9}", "4");
    }

    [Test]
    public void TestNestedSwitch1 ()
    {
      DoEvalTest ("{f:{<-true switch {:true switch {:$L - $R}}} <-13 f 9}", "4");
    }

    [Test]
    public void TestNestedSwitch2 ()
    {
      DoEvalTest ("{f:{<-true switch {:true switch {:true switch {:true switch {:$L - $R}}}}} <-13 f 9}", "4");
    }

    [Test]
    public void TestNestedSwitch3 ()
    {
      DoEvalTest ("eval \"switch\" dyad {l:true r:{:true :false}}", "true");
    }

    [Test]
    public void TestNestedSwitch4 ()
    {
      DoEvalTest ("{x:13} eval \"switch\" dyad {l:true r:{:$x + 1 :$x}}", "14");
    }

    [Test]
    public void TestSwitchInTake ()
    {
      DoEvalTest ("{<-#lock take {<-true switch {:0 :1}}}", "0");
    }

    [Test]
    public void TestSwitchWithString1 ()
    {
      DoEvalTest ("\"a\" switch {a:1 b:2 c:3}", "1");
    }

    [Test]
    public void TestSwitchWithString2 ()
    {
      DoEvalTest ("\"b\" switch {a:1 b:2 c:3}", "2");
    }

    [Test]
    public void TestSwitchWithString3 ()
    {
      DoEvalTest ("\"c\" switch {a:1 b:2 c:3}", "3");
    }

    [Test]
    public void TestSwitchWithString4 ()
    {
      DoEvalTest ("\"d\" switch {a:1 b:2 c:3}", "{}");
    }

    [Test]
    public void TestEach ()
    {
      //This test requires that operators will send closure Index values greater
      //than two back to the EvalOperator method.
      DoEvalTest("{<-$R+.5} each 0 to 4", "{:0.5 :1.5 :2.5 :3.5 :4.5}");
    }

    [Test]
    public void TestEachEmptyK ()
    {
      DoEvalTest ("{<-$R} each {}", "{}");
    }

    [Test]
    public void TestEachEmptyL ()
    {
      DoEvalTest ("{<-$R} each ~l", "{}");
    }

    [Test]
    public void TestEachWithParent ()
    {
      DoEvalTest ("{x:{<-1 + $R} each 0 to 4 <-long $x}", "1 2 3 4 5");
    }

    [Test]
    public void TestEachWithParentOfLast ()
    {
      DoEvalTest ("{loop:{:#x write {i:++} <-($L < $R) switch {:($L + 1) loop $R :$L}} f:long {<-fiber {<-0 loop 9}} each 0 to 4 :#x dispatch 25 :#x dispatch 25 <-0}", "0");
    }

    [Test]
    public void TestEachWithMultipleStatementsVector ()
    {
      DoEvalTest ("long {a:$R + 1 <-$a + 2} each 0 to 2", "3 4 5");
    }

    [Test]
    public void TestEachWithMultipleStatementsBlock ()
    {
      DoEvalTest ("long {a:$R + 1 <-$a + 2} each {:0 :1 :2}", "3 4 5");
    }

    [Test]
    public void TestEachWithBlock ()
    {
      DoEvalTest ("{<-$R + 1} each {a:1 b:2 c:3}", "{a:2 b:3 c:4}");
    }

    [Test]
    public void TestEachWithBlock1 ()
    {
      DoEvalTest ("{x:1} each {:{} :{}}", "{:{x:1} :{x:1}}");
    }

    [Test]
    public void TestEachEval ()
    {
      DoEvalTest ("eval \"each\" dyad {l:{<-$R+1} r:1 2 3}", "{:2 :3 :4}");
    }

    [Test]
    public void TestEachEval1 ()
    {
      DoEvalTest ("{x:7} eval \"each\" dyad {l:{<-$R+$x} r:1 2 3}", "{:8 :9 :10}");
    }

    [Test]
    public void TestEachWithTemplate ()
    {
      DoEvalTest ("[?foo [! $R !] baz?] each {:\"bar\" :\"bat\"}",
                  "{:\"foo bar baz\" :\"foo bat baz\"}");
    }

    [Test]
    public void TestTemplateInTailPositionSwitch ()
    {
      DoEvalTest ("{f:{x:\"x\" t:[?[!$x!]?] <-true switch {:t $R :\"\"}} <-f {}}", "\"x\"");
    }

    [Test]
    public void TestBlockInTailPositionSwitch ()
    {
      DoEvalTest ("{f:{x:\"x\" t:{<-$x} <-true switch {:t $R :\"\"}} <-f {}}", "\"x\"");
    }

    [Test]
    public void TestNameReuseInTailPositionTemplate ()
    {
      DoEvalTest ("{f:{t:[?[!$x!]?] <-true switch {:t {} :\"y\"}} x:\"x\" t:[?[!f {}!]?] <-t {}}", "\"x\"");
    }

    [Test]
    public void TestNameReuseInTailPositionBlock ()
    {
      DoEvalTest ("{f:{t:{<-$x} <-true switch {:t {} :\"y\"}} x:\"x\" t:{<-f {}} <-t {}}", "\"x\"");
    }

    [Test]
    public void TestNameReuseInTailPositionBlock1 ()
    {
      DoEvalTest ("{f:{x:\"x0\" t:{<-$x} <-true switch {:t {} :\"y\"}} t:{<-f {}} <-t {}}", "\"x0\"");
    }

    [Test]
    public void TestNameReuseInTailPositionBlock2 ()
    {
      // This one matters; the variable defined in the lexical scope of t:{<-$x} is the one used.
      DoEvalTest ("{f:{x:\"x0\" t:{<-$x} <-true switch {:t {} :\"y\"}} x:\"x1\" t:{<-f {}} <-t {}}", "\"x0\"");
    }

    [Test]
    public void TestNameNotReusedInTailPositionBlock ()
    {
      DoEvalTest ("{f:{t1:{<-$x} <-true switch {:t1 {} :2}} x:1 t:{<-f {}} <-t {}}", "1");
    }

    [Test]
    public void TestInnerOperatorInTailPosition ()
    {
      DoEvalTest ("{f:{g:{<-$R} <-g 1} <-f {}}", "1");
    }

    [Test]
    public void TestRInEval ()
    {
      DoEvalTest ("{<-eval {x:$R}} 0", "{x:0}");
    }

    [Test]
    public void TestEachNames ()
    {
      DoEvalTest ("{<-$L} each {a:1 b:2 c:3}", "{a:\"a\" b:\"b\" c:\"c\"}");
    }

    [Test]
    public void TestEachNamesEmpty ()
    {
      DoEvalTest ("{<-$L} each {a:1 :2 c:3}", "{a:\"a\" :\"\" c:\"c\"}");
    }

    [Test]
    public void TestEachNamesNotVisibleInLoop ()
    {
      DoEvalTest ("{<-R {}} each {a:{<-1 + 2} count:{<-3 + 4} b:{<-count {}}}", "{a:3 count:7 b:0}");
    }

    [Test]
    public void TestEachIndices ()
    {
      DoEvalTest ("{<-$I} each {:1 :2 :3}", "{:0 :1 :2}");
    }

    [Test]
    public void TestEachArrays ()
    {
      DoEvalTest ("{<-$I} each 3 4 5", "{:0 :1 :2}");
    }

    [Test]
    public void TestEmptyEval ()
    {
      DoEvalTest ("count eval {}", "0");
    }

    [Test]
    public void TestEvalReference ()
    {
      DoEvalTest ("{k:{a:1 b:2 c:3} <-eval reference \"k\" \"b\"}", "2");
    }

    [Test]
    public void TestEvalReference1 ()
    {
      DoEvalTest ("{k:{a:1 b:2 c:3} r:reference \"k\" \"b\" <-eval $r}", "2");
    }

    [Test]
    public void TestEvalOperatorReference ()
    {
      DoEvalTest ("{k:{f:1 + 2} <-eval $k.f}", "3");
    }

    [Test]
    public void TestEvalOperatorReference1 ()
    {
      DoEvalTest ("{k:{a:1 b:2 f:$a + $b} <-$k eval $k.f}", "3");
    }

    [Test]
    public void TestEvalOperatorReference2 ()
    {
      DoEvalTest ("{k:{a:1 b:2 f:$a + $b} <-k.f {}}", "3");
    }

    [Test]
    public void TestEvalWithStateAndReference ()
    {
      DoEvalTest ("{k:{a:{x:1} b:{x:2} c:{x:3}} <-$k eval reference \"b\" \"x\"}", "2");
    }
    
    [Test]
    public void TestEvalWithUserOperator ()
    {
      DoEvalTest ("{k:{v:{a:1} m1:{f:$v.a + $R} m2:{f:m1.f 10}} <-$k eval $k.m2.f}", "11");
    }

    [Test]
    public void TestUserOpConflictsWithBuiltin ()
    {
      //Why didn't I realize this until now and what on earth am I to do about it?
      DoEvalTest ("{file:{<-$R + 7} <-file 12}", "19");
    }

    [Test]
    public void TestRecursionTailCall()
    {
      DoEvalTest ("{f:{s:stack {} :assert 20 > $s.depth :assert 0 == $s.fiber <-($L < $R) switch {:($L + 1) f $R :$L}} <-0 f 100}", "100");
    }

    [Test]
    public void TestRecursionTailCallFinite()
    {
      DoEvalTest ("{loop:{s:stack {} :assert $s.depth < 20 :assert $s.fiber > 0 <-($L < $R) switch {:($L + 1l) loop $R :$L}} s0:stack{} :assert $s0.fiber == 0 f:eval {<-fiber {<-0 loop 100}} s1:stack {} :assert $s1.fiber == 0 :assert ($f at 1) = 1 <-wait $f}", "100");
    }

    [Test]
    public void TestRecursionTailCallInfinite()
    {
      //This is not really what the return value of a fiber that throws and exception should be.
      DoEvalTest ("{loop:{s:stack {} :1 assert $s.fiber :#x write {i:++} <-loop $R + 1} f:fiber {<-loop 0} :#x dispatch 100 :kill $f :try {:wait $f} <-0}", "0");
    }

    [Test]
    public void TestRecursionFibonacci()
    {
      DoEvalTest("{f:{<-($R in 0.0 1.0) switch {:$R :(f $R - 1.0) + f $R - 2.0}} <-f 10.0}", "55.0");
    }

    [Test]
    public void TestRecursionFactorial()
    {
      DoEvalTest("{factorial:{<-($R > 1) switch {:$R * factorial $R - 1 :1}} <-factorial 5}", "120");
    }

    [Test]
    public void TestRecursionWithSideEffect()
    {
      DoEvalTest ("{r:{line:#x write {i:++} <-($L < $R - 1) switch {:($L + 1) r $R :$line}} f:fiber {<-0 r 10} <-wait $f}", "10");
    }

    [Test]
    public void TestRecursionMutual()
    {
      //Hofstadter Female and Male sequences.
      //http://en.wikipedia.org/wiki/Hofstadter_sequence#Hofstadter_Female_and_Male_sequences
      DoEvalTest ("{f:{<-($R == 0) switch {:1 :$R - m f $R - 1}} m:{<-($R == 0) switch {:0 :$R - f m $R - 1}} :(long $f each 0 to 10) assert 1 1 2 2 3 3 4 5 5 6 6 :(long $m each 0 to 10) assert 0 0 1 2 2 3 4 4 5 6 6 <-0}", "0");
    }

    [Test]
    public void TestFiberUnparented0()
    {
      DoEvalTest ("1 from fiber {<-1 + 2}", "1");
    }

    [Test]
    public void TestFiberUnparented1()
    {
      DoEvalTest ("{<-1 from fiber {<-1 + 2}}", "1");
    }

    [Test]
    public void TestDyadicInlineOperator()
    {
      DoEvalTest ("1.0 2.0 3.0 {<-$R-$L} 4.0 5.0 6.0", "3.0 3.0 3.0");
    }

    [Test]
    public void TestMonadicInlineOperator()
    {
      DoEvalTest ("{<-$R + 1.0} 1.0 2.0 3.0", "2.0 3.0 4.0");
    }

    [Test]
    [Ignore ("syntax didn't work out")]
    public void TestMonadicInlineWithBlockRight()
    {
      DoEvalTest ("{z:$R.x y:$R.y x:$R.z} {x:1.0 y:2.0 z:3.0}", "{z:1.0 y:2.0 x:3.0}");
    }

    [Test]
    public void TestRDotWithoutBlock ()
    {
      //Assert.Throws<RCException> (delegate () { DoEvalTest ("{f:{<-$R.x} <-f 11}", "0"); });
      DoEvalTest ("{f:{<-$R.x} <-unwrap #status from try {<-f 11}}", "1");
    }

    [Test]
    public void TestRGetMissingName ()
    {
      //Barely recognize the syntax of c# anymore
      //Assert.That(() => DoEvalTest ("{f:{<-$R get \"x\"} <-f {}}", "0"),
      //  Throws.TypeOf<RCException> ()
      //    .With.Message.EqualTo ("Unable to resolve name x"));
      //DoTestException ("{f:{<-$R get \"x\"} <-f {}}", RCErrors.Name, "Unable to resolve name x");
      DoEvalTest ("{f:{<-$R get \"x\"} <-unwrap #status from try {<-f {}}}", "1");
    }

    [Test]
    public void TestLGetMissingName ()
    {
      //DoTestException ("{f:{<-\"x\" get $R} <-f {}}", RCErrors.Name, "Unable to resolve name x");
      DoEvalTest ("{f:{<-\"x\" get $R} <-unwrap #status from try {<-f {}}}", "1");
    }

    [Test]
    public void TestRGetMissingIndex ()
    {
      //DoTestException ("{f:{<-1 get $R} <-f {}}", RCErrors.Range, "Index 1 is out of range");
      DoEvalTest ("{f:{<-1 get $R} <-unwrap #status from try {<-f {}}}", "1");
    }

    [Test]
    public void TestLGetMissingIndex ()
    {
      DoEvalTest ("{f:{<-$R get 1} <-unwrap #status from try {<-f {}}}", "1");
    }

    [Test]
    public void TestMonadicInlineOpWithLiteralNoYield()
    {
      DoEvalTest ("{x:$R + 1.0 y:$R + 2.0 z:$R + 3.0} 0.0", "{x:1.0 y:2.0 z:3.0}");
    }

    [Test]
    public void TestMonadicUserOpWithLiteralNoYield()
    {
      DoEvalTest ("{f:{x:$R + 1.0 y:$R + 2.0 z:$R + 3.0} <-f 0.0}", "{x:1.0 y:2.0 z:3.0}");
    }

    [Test]
    public void TestWriteChecksCount()
    {
      NUnit.Framework.Assert.Throws<RCException> (delegate () { DoRawTest ("#x write {a:1 10 b:2 20 c:3 30}", "0"); });
    }

    [Test]
    public void TestWriteRead ()
    {
      DoEvalTest("{:#x write {a:1 b:2 c:3} :(#x read 0) assert [S|a b c #x 1 2 3] <-0}", "0");
    }

    [Test]
    public void TestWriteReadX1 ()
    {
      //I added these tests because I found after adding bytes vectors they didn't work correctly
      //With read and write.
      DoEvalTest ("{:#x write {a:\\x00 b:\\x01 c:\\x02} :(#x read 0) assert [S|a b c #x \\x00 \\x01 \\x02] <-0}", "0");
    }

    [Test]
    public void TestWriteReadX2 ()
    {
      DoEvalTest ("{:write [S|a b c #x \\x00 \\x01 \\x02] :(#x read 0) assert [S|a b c #x \\x00 \\x01 \\x02] <-0}", "0");
    }

    [Test]
    public void TestWriteEmptyCube ()
    {
      DoEvalTest ("{z:write [] <-$z}", "0");
    }

    [Test]
    public void TestWriteReadDupSymbol()
    {
      DoEvalTest ("{:#x write {i:++} :(#x #x read 0) assert [S|i #x 0] <-0}", "0");
    }

    [Test]
    public void TestWriteNoDupsInTimeline()
    {
      DoEvalTest ("{:#x write {a:1} :#x write {a:1} <-lines #x read 0}", "1");
    }

    [Test]
    public void TestWriteReadSparse ()
    {
      DoEvalTest ("{:write [S|x y #a 0 10 #a 1 10] <-#a read 1}", "[S|x y #a 1 10]");
    }

    [Test]
    public void TestWriteReadSuspends ()
    {
      DoEvalTest ("{:write [S|x #a 0] f:fiber {<-#a read 1} :sleep 100 :assert not done $f <-0}", "0");
    }

    [Test]
    public void TestWriteReadSparseMultipleSymbol ()
    {
      DoEvalTest ("{:write [S|x y #a,0 0 10 #a,1 1 11 #a,0 1 -- #a,1 2 --] <-#a,* read 2}", "[S|x y #a,0 1 10 #a,1 2 11]");
    }

    [Test]
    public void TestWriteLastSparse ()
    {
      DoEvalTest ("{:write [S|x y #a 0 10 #a 1 10] <-#a last 0}", "[S|x y #a 1 10]");
    }

    [Test]
    public void TestWriteLastSparser ()
    {
  //[
  //G|                  T|S        |id      x    y  mx  my    w    h selected dockto
  //0l 635104349707739700l #items,i0 "i0" 100d  75d  0d  0d 150d 150d     true ""
  //1l 635104349707964650l #items,i1 "i1" 200d 300d  0d  0d 150d 150d    false ""
  //2l 635104349829843180l #items,i0 --     --   --  --  --   --   --    false --
  //3l 635104349829845000l #items,i2 "i2" 589d 136d 75d 75d 150d 150d     true --
  //]
      //DoEvalTest ("{:write [S|x y selected #items,i0 100d 75d true #items,i1 200d 300d false #items,i0 -- -- false #items,i2 589d 136d true] <-#items snap 0 -1l}", "");

      //DoEvalTest ("{:write [S|dockto #items,i0 \"\" #items,i1 \"\" #items,i0 -- #items,i2 --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|x #items,i0 100d #items,i1 200d #items,i0 -- #items,i2 589d] <-#items snap 0 -1l}", "");

      DoEvalTest ("{:write [S|x y #s,1 100 1 #s,2 200 2 #s,1 -- -- #s,3 300 --] <-#s,* snap 0 -1}", "[S|x y #s,1 100 1 #s,2 200 2 #s,3 300 --]");
      //DoEvalTest ("{:write [S|x y #s,0l 100d 1d #s,1l 200d 2d #s,0l -- -- #s,2l 589d --] <-#s snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|x dockto #items,i0 100d \"\" #items,i1 200d \"\" #items,i0 -- -- #items,i2 589d --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|x selected dockto #items,i0 100d true \"\" #items,i1 200d false \"\" #items,i0 -- false -- #items,i2 589d true --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|x y selected dockto #items,i0 100d  75d true \"\" #items,i1 200d 300d false \"\" #items,i0 -- -- false -- #items,i2 589d 136d true --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|id x y selected dockto #items,i0 \"i0\" 100d  75d true \"\" #items,i1 \"i1\" 200d 300d false \"\" #items,i0 -- -- -- false -- #items,i2 \"i2\" 589d 136d true --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|id x y mx my selected dockto #items,i0 \"i0\" 100d  75d  0d  0d true \"\" #items,i1 \"i1\" 200d 300d 0d 0d false \"\" #items,i0 -- -- -- -- -- false -- #items,i2 \"i2\" 589d 136d 75d 75d true --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [S|id x y mx my w h selected dockto #items,i0 \"i0\" 100d  75d  0d  0d 150d 150d true \"\" #items,i1 \"i1\" 200d 300d 0d 0d 150d 150d false \"\" #items,i0 -- -- -- -- -- -- -- false -- #items,i2 \"i2\" 589d 136d 75d 75d 150d 150d true --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [T|S|id x y mx my w h selected dockto 635104349707739700l #items,i0 \"i0\" 100d  75d  0d  0d 150d 150d true \"\" 635104349707964650l #items,i1 \"i1\" 200d 300d 0d 0d 150d 150d false \"\" 635104349829843180l #items,i0 -- -- -- -- -- -- -- false -- 635104349829845000l #items,i2 \"i2\" 589d 136d 75d 75d 150d 150d true --] <-#items snap 0 -1l}", "");
      //DoEvalTest ("{:write [G|T|S|id x y mx my w h selected dockto 0l 635104349707739700l #items,i0 \"i0\" 100d  75d  0d  0d 150d 150d true \"\" 1l 635104349707964650l #items,i1 \"i1\" 200d 300d 0d 0d 150d 150d false \"\" 2l 635104349829843180l #items,i0 -- -- -- -- -- -- -- false -- 3l 635104349829845000l #items,i2 \"i2\" 589d 136d 75d 75d 150d 150d true --] <-#items snap 0 -1l}", "");

      //DoEvalTest ("{:write [S|x y #s,a 1l 2l #s,b 10l 20l #s,a -- 3l #s,c 100l 200l] <-#s snap 0 -1l}", "[S|x y #s,a 0l -- #s,b -- 2l]");
      //DoEvalTest ("{:write [S|x y #s,a 1d 2d] :write [S|x y #s,b 10d 20d] :write [S|x y #s,a -- 3d] :write [S|x y #s,c 100d 200d] <-#s snap 0 -1l}", "[S|x y #s,a 0l -- #s,b -- 2l]");
    }

    [Test]
    public void TestWriteTReadTAndG()
    {
      DoEvalTest ("{:write [E|S|x 25 #s,a 456.7] <-#s,* read 0 0}", "[G|E|S|x 0 25 #s,a 456.7]", RCFormat.Default);
    }

    [Test]
    [Ignore ("because")]
    public void TestWriteGTSWithBlock()
    {
      DoEvalTest ("{:write {E:25 S:#s,a x:456.7} <-#s read 0 0}", "[G|E|S|x 0 25 #s,a 456.7]", RCFormat.Default);
    }

    [Test]
    public void TestWriteReadMultipleTimes()
    {
      DoEvalTest("{:#x write {a:1 b:2 c:3} :(#x read 0) assert [S|a b c #x 1 2 3] :#x write {a:10 b:20 c:30} :(#x read 1) assert [S|a b c #x 10 20 30] <-0}", "0");
    }

    [Test]
    public void TestWriteReadMultipleSymbols()
    {
      DoEvalTest("{:#s,0 #s,1 write {x:1 10} :#s,0 #s,1 write {x:2 20} :(#s,0 read 0) assert [S|x #s,0 1 #s,0 2] :(#s,1 read 0) assert [S|x #s,1 10 #s,1 20] <-0}", "0");
    }

    [Test]
    public void TestWriteReadDuplicates()
    {
      DoEvalTest ("{:#s,1 #s,2 #s,3 write {x:10 20 30} :#s,2 #s,3 #s,1 write {x:20 35 10} :(#s,1 #s,2 #s,3 read 0) assert [S|x #s,1 10 #s,2 20 #s,3 30 #s,3 35] <-0}", "0");
    }

    [Test]
    public void TestWriteReadCube()
    {
      DoEvalTest ("{:write [S|x #s,1 10 #s,2 20 #s,3 30] :write [S|x #s,2 20 #s,3 35 #s,1 10] :(#s,1 #s,2 #s,3 read 0) assert [S|x #s,1 10l #s,2 20 #s,3 30 #s,3 35] <-0}", "0");
    }

    [Test]
    public void TestWriteReadLimit()
    {
      DoEvalTest ("{:write [S|x #1 10 #1 20] :(#1 read 0 1) assert [S|x #1 10] <-0}", "0");
    }

    [Test]
    public void TestWriteReadNegLimit()
    {
      DoEvalTest ("{:write [S|x #1 10 #1 20] :(#1 read 0 -1) assert [S|x #1 20] <-0}", "0");
    }

    [Test]
    public void TestWriteReadSeesDispatchedRows()
    {
      DoEvalTest ("{:write [S|x #1 10] :#1 dispatch 1 :(#1 read 0) assert [S|x #1 10] <-0}", "0");
    }

    [Test]
    public void TestWriteWithMonadicTrace()
    {
      DoEvalTest("{:#a write {x:1 y:2} :#a write {y:2 z:3} :(trace #a) assert [S|x y z #a 1 2 -- #a -- -- 3] <-0}", "0");
    }

    [Test]
    public void TestForceWithMonadicTrace ()
    {
      DoEvalTest ("{:#a force {x:1 y:2} :#a force {y:2 z:3} :(trace #a) assert [S|x y z #a 1 2 -- #a -- 2 3] <-0}", "0");
    }

    [Test]
    public void TestForceWithDyadicTrace ()
    {
      DoEvalTest ("{:#a force {x:1 y:2} :#a force {y:2 z:3} :(1 0 trace #a) assert [S|y z #a 2 3] <-0}", "0");
    }

    [Test]
    public void TestForceCubeDyadicTrace ()
    {
      DoEvalTest ("{:force [S|x y #a 1 2] :force [S|y z #a 2 3] :(1 0 trace #a) assert [S|y z #a 2 3] <-0}", "0");
    }

    [Test]
    public void TestForceCubeMonadicTrace ()
    {
      DoEvalTest ("{:force [S|x y #a 1 2] :force [S|y z #a 2 3] :(trace #a) assert [S|x y z #a 1 2 -- #a -- 2 3] <-0}", "0");
    }

    [Test]
    public void TestWriteReadSuspended()
    {
      DoEvalTest ("{reader:fiber {<-#x read 0} :#x #x write {a:1 10 b:2 20 c:3 30} :(wait $reader) assert [S|a b c #x 1 2 3 #x 10 20 30] <-0}", "0");
    }

    [Test]
    public void TestWriteReadSuspendedWildcard()
    {
      DoEvalTest ("{reader:fiber {<-#x,* read 0} :#x,0 #x,1 write {a:1 10 b:2 20 c:3 30} :(wait $reader) assert [S|a b c #x,0 1 2 3 #x,1 10 20 30] <-0}", "0");
    }

    [Test]
    public void TestWriteReadSuspendedLimit()
    {
      DoEvalTest ("{reader:fiber {<-#x read 0 3} :#x #x write {a:1 10 b:2 20 c:3 30} :#x write {a:100 b:200 c:300} :(wait $reader) assert [S|a b c #x 1 2 3 #x 10 20 30 #x 100 200 300] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchSingleSymbol()
    {
      DoEvalTest ("{:write [E|S|x 0 #a 1 1 #a 2 2 #a 3] :(#a dispatch 1) assert [G|E|S|x 0 0 #a 1] :(#a dispatch 2) assert [G|E|S|x 1 1 #a 2 2 2 #a 3] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchSingleSymbolSuspended()
    {
      DoEvalTest ("{reader:fiber {<-#x dispatch 1} :#x #x write {a:1 10 b:2 20 c:3 30} :(wait $reader) assert [S|a b c #x 1 2 3] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchWindowUnder()
    {
      //This test could pass by accident if the second dispatch doesn't happen until after the second write.
      //This could be fixed with a gate operator that would let you wait on a signal from another fiber.
      DoEvalTest ("{:#x write {a:1 b:2 c:3} r0:#x dispatch 1 r1:fiber {<-#x dispatch 1} :#x write {a:10 b:20 c:30} :(wait $r1) assert [S|a b c #x 10 20 30] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchMultiSymbolSuspended()
    {
      //dispatch should not yield until the second write.
      //It has to run twice to expose the flaw in the program.
      DoEvalTest ("{:#s,x write {a:0 b:1 c:2} reader:fiber {<-#s,x #s,y dispatch 1} :sleep 50 :#s,y write {a:10 b:20 c:30} :(wait $reader) assert [S|a b c #s,x 0 1 2 #s,y 10 20 30] :#s,x write {a:1 b:2 c:3} reader:fiber {<-#s,x #s,y dispatch 1} :sleep 50 :#s,y write {a:11 b:21 c:31} :(wait $reader) assert [S|a b c #s,x 1 2 3 #s,y 11 21 31] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchWildcard()
    {
      DoEvalTest ("{reader:fiber {<-#x,* dispatch 1} :#x,0 #x,1 write {a:1 10 b:2 20 c:3 30} :(wait $reader) assert [S|a b c #x,0 1 2 3] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchWildcard1 ()
    {
      DoEvalTest ("{reader:fiber {<-#x dispatch 1} :#x,0 #x,1 #x write {a:1 10 100 b:2 20 200 c:3 30 300} :(wait $reader) assert [S|a b c #x 100 200 300] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchWildcard2()
    {
      DoEvalTest ("{reader:fiber {<-#x dispatch 1} :#x,0 #x,1 write {a:1 10 b:2 20 c:3 30} :#x write {d:4} :(wait $reader) assert [S|d #x 4] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchAggregateSymbols ()
    {
      DoEvalTest ("{reader:fiber {<-#p,* dispatch 3} :#p,s,a #p,s,b #p,s write {x:2 3 5 y:4 5 9} :(wait $reader) assert [S|x y #p,s,a 2 4 #p,s,b 3 5 #p,s 5 9] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchWildcardMultipleTimes()
    {
      DoEvalTest ("{:#x,0 #x,1 write {a:1 10} :(#x,* dispatch 1) assert [S|a #x,0 1] :(#x,* dispatch 1) assert [S|a #x,1 10] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchWildcardMultipleMultipleTimes()
    {
      DoEvalTest ("{:write [S|a #x,0 1] r0:fiber {<-eval {c0:#x,* dispatch 2 c1:#x,* dispatch 2}} :sleep 20 :write [S|a #x,1 2 #x,0 10 #x,1 20] out:wait $r0 :assert $out.c0 = [S|a #x,0 1 #x,1 2] :assert $out.c1 = [S|a #x,0 10 #x,1 20] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchFutureNullsNotAccepted ()
    {
      DoEvalTest ("{:write [S|a b #x,0 0 #const #x,1 0 #const #x,0 1 #const1 #x,1 1 -- #x,0 2 --] c:#x,* dispatch 2 <-count $c.b}", "2");
    }

    [Test]
    public void TestWriteDispatchNoCandidates ()
    {
      DoEvalTest ("{reader:fiber {<-#x dispatch 1} :#y write {a:1} <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchZero()
    {
      DoEvalTest ("{f:fiber {<-#x dispatch 0} :sleep 20 :assert not done $f :#x #x #x write {i:++ ++ ++} :(wait $f) assert [S|i #x 0 #x 1 #x 2] f1:fiber {<-#x #y dispatch 0} :assert not done $f1 :#x write {i:++} :(wait $f1) assert [S|i #x 3] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchZeroWithDups0()
    {
      DoEvalTest ("{:write [S|a #x 10 #x 10] :(#x dispatch 0) assert [S|a #x 10] <-0}", "0");
    }

    [Test]
    public void TestWriteDispatchZeroWithDups1()
    {
      //When writing cubes to the timeline, axis lines were being created even for complete dups, and this was
      //interfering with the counts for dispatch.
      DoEvalTest ("{:write cube {S:#x #x a:10 10} :(#x dispatch 0) assert [S|a #x 10] :(#x peek 0) assert false <-0}", "0");
    }

    [Test]
    public void TestWritePeekZero()
    {
      DoEvalTest ("{:assert not #x peek 0 :#x #x #x write {i:++ ++ ++} :assert #x #y peek 0 <-0}", "0");
    }

    [Test]
    public void TestWriteThrottleZero()
    {
      DoEvalTest ("{f0:fiber {<-#x throttle 0} :wait $f0 :#x write {i:++} f1:fiber {<-#x #y throttle 0} :sleep 20 :assert not done $f1 :#x dispatch 1 :wait $f1 <-0}", "0");
    }

    [Test]
    public void TestWriteGawkZero()
    {
      DoEvalTest ("{f:fiber {<-#x #y gawk 0} :assert not done $f :#x write {i:++} :(wait $f) assert [S|i #x 0] <-0}", "0");
    }

    [Test]
    public void TestWriteGawkConcrete()
    {
      DoEvalTest ("{f:fiber {<-#x gawk 0} :#x,y write {i:++} :#x write {i:++} :(wait $f) assert [S|i #x 0] <-0}", "0");
    }

    [Test]
    public void TestWriteReadFirstNull()
    {
      DoEvalTest ("{:write [S|a b #x 1 -- #y 10 2] :(#x dispatch 1) assert [S|a #x 1] <-0}", "0");
    }

    [Test]
    public void TestWriteReadIncr()
    {
      DoEvalTest ("{:#x write {i:++} :(#x dispatch 1) assert [S|i #x 0] :#x write {i:++} :(#x dispatch 1) assert [S|i #x 1] <-0}", "0");
    }

    [Test]
    public void TestWriteReadAll()
    {
      DoEvalTest ("{:write [S|x #s,a 0 #s,b 1 #s,c 2] :(#s,* read 0) assert [S|x #s,a 0 #s,b 1 #s,c 2] :(#s,* read 1) assert [S|x #s,b 1 #s,c 2] :(#s,* read 2) assert [S|x #s,c 2] <-0}", "0");
    }

    [Test]
    public void TestWriteReadAllMissingSymbol()
    {
      DoEvalTest ("{:#x,a write {i:++} :(#x,a #x,b read 0 0) assert [S|i #x,a 0] <-0}", "0");
    }

    [Test]
    public void TestWriteLast()
    {
      DoEvalTest ("{:write [S|x #a 10 #c 30 #b 20 #a 11 #a 12 #b 21] <-#a #b last 0}", "[S|x #a 12 #b 21]");
    }

    [Test]
    public void TestWriteLastReadFirstWithLast()
    {
      DoEvalTest ("{:write [S|x #a 10] <-#a last 0}", "[S|x #a 10]");
    }

    [Test]
    public void TestWriteReadFirstWithLastWithMore()
    {
      DoEvalTest ("{:write [S|x y #a 0 -- #b -- 1] <-#a last 0}", "[S|x #a 0]");
    }

    [Test]
    public void TestWriteLastSuspended()
    {
      //would be cool if we operators to get the state of a fiber, like suspended, done, active, etc...
      DoEvalTest ("{f:fiber {<-#x last 0} :sleep 20 :assert not done $f <-0}", "0");
    }

    [Test]
    public void TestWriteReadLinesIsThreadSafe()
    {
      DoEvalTest ("{:write [S|x #a 0] data:#a read 0 :write [S|x #a 1] <-lines $data}", "1");
    }

    [Test]
    public void TestWriteThrottle()
    {
      DoEvalTest ("{:write [S|x #a 0 #a 1 #a 2] :fiber {:#a throttle 3 :#check write {order:#second}} :#check write {order:#first} :#a dispatch 1 <-#check read 0 2}", "[S|order #check #first #check #second]");
    }

    [Test]
    public void TestWriteThrottleWithDispatchWild()
    {
      DoEvalTest ("{:write [S|x #a,0 0] f:fiber {<-#a,0 throttle 1} :#a,* dispatch 1 :(wait $f) assert 1 <-0}", "0");
    }

    [Test]
    public void TestWriteThrottleWithDispatchWild1()
    {
      DoEvalTest ("{:write [S|x #a,b,0 0] f:fiber {<-#a,* throttle 1} :#a,b,* dispatch 1 :(wait $f) assert 1 <-0}", "0");
    }

    [Test]
    public void TestWritePeek()
    {
      DoEvalTest ("{:write [S|x #a 0] :(#a peek 2) assert false :write [S|x #a 1] :(#a peek 2) assert true <-0}", "0");
    }

    [Test]
    public void TestWritePeekDispatchCounter()
    {
      DoEvalTest ("{:#a,0 write {i:++} :#a,1 write {i:++} :#a,1 dispatch 1 <-#a,* peek 1}", "true");
    }

    [Test]
    public void TestWriteReadAndDispatchSameRows()
    {
      DoEvalTest ("{:#x write {i:++} :#x dispatch 1 <-count #x read 0 1}", "1");
    }

    [Test]
    public void TestWriteLastStartPoint()
    {
      DoEvalTest ("{:#x write {i:++} f:fiber {<-#x last 1} :sleep 20 :assert not done $f :#x write {i:++} :sleep 20 :assert done $f <-0}", "0");
    }

    [Test]
    public void TestWriteGawkDoesNotDispatch()
    {
      DoEvalTest ("{:#x write {i:++} :(#x gawk 1) assert [S|i #x 0] :(#x dispatch 1) assert [S|i #x 0] <-0}", "0");
    }

    [Test]
    public void TestWritePoll()
    {
      DoEvalTest ("{:(#x poll 0) assert [] :#x #x #x write {i:++ ++ ++} :(#x poll 0) assert [S|i #x 0 #x 1 #x 2] :(#x poll 1) assert [S|i #x 1 #x 2] :(#x poll 2) assert [S|i #x 2] :(#x poll 3) assert [] <-0}", "0");
    }

    [Test]
    public void TestWriteSnapColumnRemoval ()
    {
      DoEvalTest ("{:#x,y write {a:1 b:2} :#x,z write {c:3} :(#x,* snap 1) assert [S|c #x,z 3] <-0}", "0");
    }

    [Test]
    public void TestWriteSnapConcrete ()
    {
      DoEvalTest ("{:#x,y write {i:++} :(#x,y snap -1) assert [S|i #x,y 0] <-0}", "0");
    }

    [Test]
    public void TestWriteGContinuesAfterClear ()
    {
      DoEvalTest ("{:#a write {i:++} :clear #a :#a write {i:++} u:#a read 1 <-$u.G}", "1");
    }

    [Test]
    public void TestCannotWriteGOutOfOrder ()
    {
      DoEvalTest ("first #status from try {<-write [G|E|S|x 1 1 #a 101 0 0 #a 100]}", "1");
    }

    [Test]
    public void TestWriteCubeIncrementsG ()
    {
      DoEvalTest ("{:write [S|x #a 1] :write [S|x #a 2] :write [S|x #a 3] u:#a read 0 <-$u.G}", "0 1 2");
    }
      
    [Test]
    public void TestWriteReadFromCubeAfterClear ()
    {
      DoEvalTest ("{:write [S|x #a 0] :clear #a :write [S|x #a 0] u:#a read 0 1 <-$u.G}", "1");
    }

    [Test]
    public void TestWriteReadFromCubeAfterClear1 ()
    {
      DoEvalTest ("{:write [S|x #a 0] :clear #a :write [S|x #a 0] u:#a read 0 1 <-lines $u}", "2");
    }

    [Test]
    public void TestWriteReadFromCubeAfterClear2 ()
    {
      DoEvalTest ("{:write [S|x #a 0 #a 1] :clear #a :write [S|x #a 2] u:#a read 0 1 <-lines $u}", "3");
    }

    [Test]
    public void TestPage ()
    {
      DoEvalTest ("{:(#x page 0 2) assert [] :#x #x #x #x #x write {i:++ ++ ++ ++ ++} :(#x page 0 2) assert [S|i #x 0 #x 1] :(#x page 1 2) assert [S|i #x 2 #x 3] :(#x page 2 2) assert [S|i #x 4] <-0}", "0");
    }

    [Test]
    public void TestPageBackwards ()
    {
      DoEvalTest ("{:(#x page 0 -2) assert [] :#x #x #x #x #x write {i:++ ++ ++ ++ ++} :(#x page 0 -2) assert [S|i #x 3 #x 4] :(#x page 1 -2) assert [S|i #x 1 #x 2] :(#x page 2 -2) assert [S|i #x 0] <-0}", "0");
    }

    [Test]
    public void TestSnap ()
    {
      DoEvalTest ("{:(#s,* snap 0 -1) assert [] :#s,a write {i:++} :(#s,* snap 0 -1) assert [S|i #s,a 0] :#s,a #s,b write {i:++ ++} :(#s,* snap 0 -1) assert [S|i #s,a 1 #s,b 0] <-0}", "0");
    }

    [Test]
    public void TestSnapDups ()
    {
      DoEvalTest ("{:#s,a write {x:1} :#s,b write {x:10} :#s,c write {x:100} :#s,c write {x:100} :#s,d write {x:1000} :(#s,* snap 0 -1) assert [S|x #s,a 1 #s,b 10 #s,c 100 #s,d 1000] <-0}", "0");
      //DoEvalTest ("{:#s,a write {x:1l y:10l z:100l} :#s,b write {x:2l y:20l z:200l} :#s,c write {x:3l y:30l z:300l} :#s,d write {x:4l y:40l z:400l} :#s,c write {x:3l y:30l z:300l} :(#s snap 0 -1l) assert [S|x y z #s,a 1l 10l 100l #s,b 2l 20l 200l #s,c 3l 30l 300l #s,d 4l 40l 400l] <-0l}", "0l");
    }

    [Test]
    public void TestSnapChanges1Level ()
    {
      DoEvalTest ("{:#s,a write {x:1} :#s,b write {x:10} :#s,a write {x:2} :(#s,* snap 0 -1) assert [S|x #s,b 10 #s,a 2] <-0}", "0");
    }

    [Test]
    public void TestSnapChanges2Level ()
    {
      DoEvalTest ("{:#s,a,0 write {x:1} :#s,b,0 write {x:10} :#s,a,0 write {x:2} :(#s,* snap 0 -1) assert [S|x #s,b,0 10 #s,a,0 2] <-0}", "0");
    }

    [Test]
    public void TestSnapSparseFirstRow ()
    {
      DoEvalTest ("{:write [S|x y #s,a 1 -- #s,b -- 2] <-#s,* snap 0}", "[S|x y #s,a 1 -- #s,b -- 2]");
    }

    [Test]
    public void TestSnapSparseFirstRow1 ()
    {
      DoEvalTest ("{:write [S|x y #s,a 1 -- #s,b 2 -- #s,c -- 3] <-#s,* snap 0}", "[S|x y #s,a 1 -- #s,b 2 -- #s,c -- 3]");
    }

    [Test]
    public void TestSnapSparseFirstRow2 ()
    {
      DoEvalTest ("{:write [S|x #s,a 0 #s,b 1] :write [S|y #s,c 2 #s,d 3] <-#s,* snap 0}", "[S|x y #s,a 0 -- #s,b 1 -- #s,c -- 2 #s,d -- 3]");
    }

    [Test]
    public void TestWriteClearWriteK ()
    {
      DoEvalTest ("{f:fiber {<-#a read 1} :#a write {x:0} :clear #a :#a write {x:1} :(wait $f) assert [S|x #a 1] <-0}", "0");
    }

    [Test]
    public void TestWriteClearWriteU ()
    {
      DoEvalTest ("{f:fiber {<-#a read 1} :write [S|x #a 0] :clear #a :write [S|x #a 1] :(wait $f) assert [S|x #a 1] <-0}", "0");
    }

    [Test]
    public void TestWriteClearWriteSyncK ()
    {
      DoEvalTest ("{:#a write {x:0} :clear #a :#a write {x:1} :(#a read 1) assert [S|x #a 1] <-0}", "0");
    }

    [Test]
    public void TestWriteClearWriteSyncU ()
    {
      DoEvalTest ("{:write [S|x #a 0] :clear #a :write [S|x #a 1] :(#a read 1) assert [S|x #a 1] <-0}", "0");
    }

    [Test]
    public void TestWriteIncrementE ()
    {
      DoEvalTest ("{:write [S|x #a 0 #a 1] <-#a read 0}", "[G|E|S|x 0 0 #a 0 1 1 #a 1]", RCFormat.Default);
    }

    [Test]
    public void TestStarReadNotPresent ()
    {
      DoEvalTest ("{:write [S|x #page,a 1 #page,b 2 #page,c 3] f:fiber {<-#page,d,* read 0 1} :sleep 20 :write [S|x #page,d,x 4] :wait $f <-0}", "0");
    }

    [Test]
    public void TestSelect1 ()
    {
      string ustring = "[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]";
      DoEvalTest (string.Format ("#a select {0}", ustring), "[G|S|x y z 0 #a 1 10 100]", RCFormat.Default);
      DoEvalTest (string.Format ("#b select {0}", ustring), "[G|S|x y z 1 #b 2 20 200]", RCFormat.Default);
      DoEvalTest (string.Format ("#c select {0}", ustring), "[G|S|x y z 2 #c 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#* select {0}", ustring), "[G|S|x y z 0 #a 1 10 100 1 #b 2 20 200 2 #c 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#a #b select {0}", ustring), "[G|S|x y z 0 #a 1 10 100 1 #b 2 20 200]", RCFormat.Default);
      DoEvalTest (string.Format ("#c #b select {0}", ustring), "[G|S|x y z 1 #b 2 20 200 2 #c 3 30 300]", RCFormat.Default);

      string vstring = "[S|x y z #a,x 1 10 100 #a,y 2 20 200 #b,z 3 30 300]";
      DoEvalTest (string.Format ("#a select {0}", vstring), "[]", RCFormat.Default);
      DoEvalTest (string.Format ("#a,* select {0}", vstring), "[G|S|x y z 0 #a,x 1 10 100 1 #a,y 2 20 200]", RCFormat.Default);
      DoEvalTest (string.Format ("#b,* select {0}", vstring), "[G|S|x y z 2 #b,z 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#b,z select {0}", vstring), "[G|S|x y z 2 #b,z 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#a,x select {0}", vstring), "[G|S|x y z 0 #a,x 1 10 100]", RCFormat.Default);
      DoEvalTest (string.Format ("#a,x #b,z select {0}", vstring), "[G|S|x y z 0 #a,x 1 10 100 2 #b,z 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#a,* #b,z select {0}", vstring), "[G|S|x y z 0 #a,x 1 10 100 1 #a,y 2 20 200 2 #b,z 3 30 300]", RCFormat.Default);
  
      DoEvalTest (string.Format ("#b,z select {0}", vstring), "[G|S|x y z 2 #b,z 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#b,* select {0}", vstring), "[G|S|x y z 2 #b,z 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#b select {0}", vstring), "[]", RCFormat.Default);
      DoEvalTest (string.Format ("#b,* select {0}", vstring), "[G|S|x y z 2 #b,z 3 30 300]", RCFormat.Default);

      string wstring = "[S|x y z #1,a 1 10 100 #1,b 2 20 200 #2,c 3 30 300]";
      DoEvalTest (string.Format ("#1,a select {0}", wstring), "[G|S|x y z 0 #1,a 1 10 100]", RCFormat.Default);
      DoEvalTest (string.Format ("#1,b select {0}", wstring), "[G|S|x y z 1 #1,b 2 20 200]", RCFormat.Default);
      DoEvalTest (string.Format ("#2,c select {0}", wstring), "[G|S|x y z 2 #2,c 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#1,c select {0}", wstring), "[]", RCFormat.Default);
      DoEvalTest (string.Format ("#2,* select {0}", wstring), "[G|S|x y z 2 #2,c 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#3,* select {0}", wstring), "[]", RCFormat.Default);

      string xstring = "[S|x y z #1,a,0 1 10 100 #1,b,0 2 20 200 #2,c,0 3 30 300]";
      DoEvalTest (string.Format ("#3,* select {0}", xstring), "[]", RCFormat.Default);
      DoEvalTest (string.Format ("#2,* select {0}", xstring), "[G|S|x y z 2 #2,c,0 3 30 300]", RCFormat.Default);
      DoEvalTest (string.Format ("#1,* select {0}", xstring), "[G|S|x y z 0 #1,a,0 1 10 100 1 #1,b,0 2 20 200]", RCFormat.Default);
      DoEvalTest (string.Format ("#1,a,* select {0}", xstring), "[G|S|x y z 0 #1,a,0 1 10 100]", RCFormat.Default);
      DoEvalTest (string.Format ("#2,b,* select {0}", xstring), "[]", RCFormat.Default);
      DoEvalTest (string.Format ("#1,b,* select {0}", xstring), "[G|S|x y z 1 #1,b,0 2 20 200]", RCFormat.Default);
    }

    [Test]
    public void TestLeadingStars1 ()
    {
      DoEvalTest ("#1,*,0 select [S|x y z #1,a,0 1 10 100 #1,b,0 2 20 200 #2,b,0 3 30 300]", "[G|S|x y z 0 #1,a,0 1 10 100 1 #1,b,0 2 20 200]", RCFormat.Default);
    }

    [Test]
    public void TestLeadingStars2 ()
    {
      DoEvalTest ("#*,*,0 select [S|x y z #1,a,0 1 10 100 #1,b,0 2 20 200 #2,b,0 3 30 300]", "[G|S|x y z 0 #1,a,0 1 10 100 1 #1,b,0 2 20 200 2 #2,b,0 3 30 300]", RCFormat.Default);
    }

    [Test]
    public void TestLeadingStars3 ()
    {
      DoEvalTest ("#*,b,* select [S|x y z #1,a,0 1 10 100 #1,b,0 2 20 200 #2,b,0 3 30 300]", "[G|S|x y z 1 #1,b,0 2 20 200 2 #2,b,0 3 30 300]", RCFormat.Default);
    }

    [Test]
    public void TestBot ()
    {
      DoEvalTest ("{b0:bot {:#x write {i:++} <-wait 0} b1:bot {:#y write {i:++} <-wait 0} :(first #x from dump $b0) assert [S|i #x 0] :(first #y from dump $b1) assert [S|i #y 0] <-0}", "0");
    }

    [Test]
    public void TestKillBot ()
    {
      //This sees that the two child fibers finish, but what about the zero waiter?
      //Waiters do not get killed, they just stay around until they get released,
      //  which is likely never if the fiber that was signalling them was killed.
      //Let's just keep it that way until we have good tools for visualizing the state of the system.
      //Update Dec 2, 2014. This was one of the earliest tests for bot lifecycle stuff.
      //It used the blackboard log to tell when fibers were done. I'm getting rid of the blackboard log
      //because of concurrency issues when trying to reset from within the shell.
      //plus we never used it for anything other than this test.
      //Now we have a better set of tests for this in CoreTest.
      //DoEvalTest ("{f1:{<-f1 $R + 1l} f2:{<-f2 $R & 1l} b:bot {:fiber {<-f1 0l} :fiber {<-f2 0l} <-wait 0l} :(#log,fiber + (symbol $b) + #1l #2l) dispatch 1l :kill $b <-(#log,fiber + (symbol $b) + #1l #2l) dispatch 1l}", "[S|state #log,fiber,1l,1l \"killed\" #log,fiber,1l,2l \"killed\"]");
      //I think we are going to need to convert RCNative into RCException and give it some proper syntax.
      DoEvalTest ("{f1:{<-f1 $R + 1} f2:{<-f2 $R & 1} b:bot {:fiber {<-f1 0} :fiber {<-f2 0} <-wait 0} :kill $b <-unwrap #status from try {<-wait $b}}", "1");
    }

    [Test]
    public void TestReferenceToACube ()
    {
      //When you reference a column within a cube you get a cube out.
      DoEvalTest ("{u:[S|a b #x 0 10 #x 1 11 #x 2 12] <-$u.b}", "[S|b #x 10 #x 11 #x 12]");
    }

    [Test]
    public void TestCubeArgumentsUL1 ()
    {
      DoEvalTest ("[S|a #x 10 #x 11 #x 12] - 1", "[S|x #x 9 #x 10 #x 11]");
    }

    [Test]
    public void TestCubeArgumentsUL2 ()
    {
      DoEvalTest ("[S|a #x 10 #x 11 #x 12] + 1", "[S|x #x 11 #x 12 #x 13]");
    }

    [Test]
    public void TestCubeArgumentsUL3 ()
    {
      DoEvalTest ("[S|a #x 10 #x 20 #x 30] / 2", "[S|x #x 5 #x 10 #x 15]");
    }

    [Test]
    public void TestCubeArgumentsUL4 ()
    {
      DoEvalTest ("[S|a #x 5 #x 10 #x 15] * 2", "[S|x #x 10 #x 20 #x 30]");
    }

    [Test]
    public void TestCubeArgumentsLU1 ()
    {
      DoEvalTest ("1 - [S|a #x 10 #x 11 #x 12]", "[S|x #x -9 #x -10 #x -11]");
    }

    [Test]
    public void TestCubeArgumentsLU2 ()
    {
      DoEvalTest ("1 + [S|a #x 10 #x 11 #x 12]", "[S|x #x 11 #x 12 #x 13]");
    }

    [Test]
    public void TestCubeArgumentsLU3 ()
    {
      DoEvalTest ("12 / [S|a #x 1 #x 2 #x 3]", "[S|x #x 12 #x 6 #x 4]");
    }

    [Test]
    public void TestCubeArgumentsLU4 ()
    {
      DoEvalTest ("2 * [S|a #x 5 #x 10 #x 15]", "[S|x #x 10 #x 20 #x 30]");
    }

    [Test]
    public void TestCubeOpWithNoResult1 ()
    {
      DoEvalTest ("{u:[E|S|a b 0 #x 1 -- 1 #y -- 2] <-$u.a + $u.b}", "[]");
    }

    [Test]
    public void TestCubeOpWithNoResult2 ()
    {
      DoEvalTest ("{u:[S|a b #x 1 -- #y -- 2] <-$u.a + $u.b}", "[]");
    }

    [Test]
    public void TestCubeEmptyArguments1 ()
    {
      DoEvalTest ("[S|a #x 1] * []", "[S|a #x 1]");
    }

    [Test]
    public void TestCubeEmptyArguments2 ()
    {
      DoEvalTest ("[] * [S|a #x 1]", "[S|a #x 1]");
    }

    [Test]
    public void TestCubeEmptyArguments3 ()
    {
      DoEvalTest ("[E|S|a 0 #x 1] * []", "[]");
    }

    [Test]
    public void TestCubeEmptyArguments4 ()
    {
      DoEvalTest ("[] * [E|S|a 0 #x 1]", "[]");
    }

    [Test]
    public void TestCubeEmptyArguments5 ()
    {
      DoEvalTest ("[] * []", "[]");
    }

    [Test]
    public void TestEvalTemplateMinimal ()
    {
      DoEvalTest ("[? between [! $R !] words ?] \"the\"", "\" between the words \"");
    }

    [Test]
    public void TestEvalTemplateMultiSection ()
    {
      DoEvalTest ("{html:[?<html><head>[!$R.head!]</head><body>[!$R.body!]</body></html>?] <-html {head:\"a head\" body:\"a body\"}}",
                  "\"<html><head>a head</head><body>a body</body></html>\"");
    }

    [Test]
    public void TestEvalTemplateMultiSection1 ()
    {
      DoEvalTest ("{html:[?\n<html><head>[!$R.head!]</head><body>[!$R.body!]</body></html>\n?] <-html {head:\"head 0\\nhead 1\\nhead 2\\n\" body:\"body 0\\nbody 1\\nbody 2\\n\"}}",
                  "\"<html><head>head 0\\nhead 1\\nhead 2\\n</head><body>body 0\\nbody 1\\nbody 2\\n</body></html>\\n\"");
    }

    [Test]
    public void TestEvalTemplateMultiSection2 ()
    {
      DoEvalTest ("{html:[?\n<html>\n  <head>[!$R.head!]\n  </head>\n  <body>[!$R.body!]\n  </body>\n</html>\n?] <-html {head:\"head 0\\nhead 1\\nhead 2\\n\" body:\"body 0\\nbody 1\\nbody 2\\n\"}}",
                  "\"<html>\\n  <head>head 0\\n  head 1\\n  head 2\\n  </head>\\n  <body>body 0\\n  body 1\\n  body 2\\n  </body>\\n</html>\\n\"");
    }

    [Test]
    public void TestEvalTemplateMultiSection3 ()
    {
      DoEvalTest ("{html:[?\n<html>\n  <head>[!$R.head!]</head>\n  <body>[!$R.body!]</body>\n</html>\n?] <-html {head:\"head 0\\nhead 1\\nhead 2\\n\" body:\"body 0\\nbody 1\\nbody 2\\n\"}}",
                  "\"<html>\\n  <head>head 0\\n  head 1\\n  head 2\\n  </head>\\n  <body>body 0\\n  body 1\\n  body 2\\n  </body>\\n</html>\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent ()
    {
      DoEvalTest ("{t:[?\n  x\n    y[! \"b\\n\" !]z\n?] <-t {}}", "\"x\\n  yb\\n  z\\n\"");
    }

    [Test]
    public void TestEvalTemplateTailCall ()
    {
      DoEvalTest ("{t:[?\n  x[!\"a\\n\"!]y\n?] f:{<-eval {i:0}} <-t f {}}", "\"xa\\ny\\n\"");
    }

    [Test]
    public void TestEvalTemplateNested ()
    {
      DoEvalTest ("{head:[?<head>a head</head>?] body:[?<body>a body</body>?] html:[?<html>[!head {}!][!body {}!]</html>?] <-html {}}",
                  "\"<html><head>a head</head><body>a body</body></html>\"");
    }

    [Test]
    public void TestEvalTemplateNestedFormatted ()
    {
      //Bear witness to the awesome power of my templates.
      /*
      {
        head:[?
          <head>
            a head
          </head>
        ?]
        body:[?
          <body>
            a body
          </body>
        ?]
        html:[?
          <html>
            [!head {}!]
            [!body {}!]
          </html>
        ?]
      }
      <html>
        <head>
          a head
        </head>
        <body>
          a body
        </body>
      </html>
      */

      DoEvalTest ("{\n  head:[?\n    <head>\n      a head\n    </head>\n  ?]\n  body:[?\n    <body>\n      a body\n    </body>\n  ?]\n  html:[?\n    <html>\n      [!head {}!]\n      [!body {}!]\n    </html>\n  ?]\n <-html {}\n}\n",
                  "\"<html>\\n  <head>\\n    a head\\n  </head>\\n  <body>\\n    a body\\n  </body>\\n</html>\\n\"");
    }

    [Test]
    public void TestEvalTemplateNestedFormattedCRLF ()
    {
      DoEvalTest ("{\r\n  head:[?\r\n    <head>\r\n      a head\r\n    </head>\r\n  ?]\r\n  body:[?\r\n    <body>\r\n      a body\r\n    </body>\r\n  ?]\r\n  html:[?\r\n    <html>\r\n      [!head {}!]\r\n      [!body {}!]\r\n    </html>\r\n  ?]\r\n <-html {}\r\n}\r\n",
                  "\"<html>\\n  <head>\\n    a head\\n  </head>\\n  <body>\\n    a body\\n  </body>\\n</html>\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent0 ()
    {
      DoEvalTest ("{t:[?\na\n          [!$R.array!]\n?] <-t {array:\"w\" \"x\" \"y\" \"z\"}}",
                  "\"a\\n          wxyz\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent1 ()
    {
      DoEvalTest ("{t:[?\na\n          [!\"w\" \"x\" \"y\" \"z\"!]\n?] <-t {}}", 
                  "\"a\\n          wxyz\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent2 ()
    {
      DoEvalTest ("{t:[?\na\n          [!\"w\\n\" \"x\\n\" \"y\\n\" \"z\\n\"!]\n?] <-t {}", 
                  "\"a\\n          w\\n          x\\n          y\\n          z\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent3 ()
    {
      DoEvalTest ("{t:[?\na\n          [!$R.array + \"\\n\"!]\n?] <-t {array:\"w\" \"x\" \"y\" \"z\"}}", 
                  "\"a\\n          w\\n          x\\n          y\\n          z\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent4 ()
    {
      DoEvalTest ("{t:[?\n    a\n      [!\"w\\n\" \"x\\n\" \"y\\n\" \"z\\n\"!]\n    b\n  ?] <-t {}}", 
                  "\"a\\n  w\\n  x\\n  y\\n  z\\nb\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent5 ()
    {
      DoEvalTest ("{t:[?\r\n    a\r\n      [!\"w\\r\\n\" \"x\\r\\n\" \"y\\r\\n\" \"z\\r\\n\"!]\r\n    b\r\n  ?] <-t {}}", 
                  "\"a\\n  w\\n  x\\n  y\\n  z\\nb\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent6 ()
    {
      DoEvalTest ("{t:[?\n  a\n    x:[!\"T\"!]\n    y\n?] <-t {}}", 
                  "\"a\\n  x:T\\n  y\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent7 ()
    {
      DoEvalTest ("{t:[?\n  a\n    x:[!\"T\"!];\n    y\n?] <-t {}}",
                  "\"a\\n  x:T;\\n  y\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent8 ()
    {
      DoEvalTest ("{t:[?\n  [!\"a\" \"b\" \"c\"!]\n  [!\"x\" \"y\" \"z\"!]\n?] <-t {}}", "\"abc\\nxyz\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent9 ()
    {
      /*
      {
        t:[?
          x
          [!
            eval {
              u:[?
                [!$R.a!]
                [!$R.b!]
              ?]
              <-{<-u $R} each $R.k
            }
          !]
          y
        ?]
        <-t {
          k:{
            :{
              a:"0_a"
              b:"0_b"
            }
            :{
              a:"1_a"
              b:"1_b"
            }
          }
        }
      }
      */
      DoEvalTest ("{t:[?\n  x\n    [! \n      eval {\n        u:[?\n          [!$R.a!]\n          [!$R.b!]\n        ?]\n        <-{<-u $R} each $R.k\n      }\n    !]\n  y\n?] <-t {k:{:{a:\"0_a\" b:\"0_b\"} :{a:\"1_a\" b:\"1_b\"}}}}",
                  "\"x\\n  0_a\\n  0_b\\n  1_a\\n  1_b\\ny\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent10 ()
    {
      DoEvalTest ("{VAR:\"var\" <-eval [?\n  FIRST LINE [! $VAR !] [! $VAR !]\n  SHOULD BE ALIGNED\n  [! $VAR !]\n?]}",
                  "\"FIRST LINE var var\\nSHOULD BE ALIGNED\\nvar\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent11 ()
    {
      DoEvalTest ("{VAR:\"$VAR\" <-eval [?\n  [! $VAR !] BETWEEN[! $VAR !]\n  SHOULD BE ALIGNED\n  [! $VAR !]\n?]}",
                  "\"$VAR BETWEEN$VAR\\nSHOULD BE ALIGNED\\n$VAR\\n\"");
    }

    [Test]
    public void TestEvalTemplateIndent12 ()
    {
      DoEvalTest ("{rcldir:\"VAR1\" tmpdir:\"VAR2\" <-eval [?\n  {\n    {\n      {\n        FIRST LINE BEFORE [! $rcldir !] FIRST LINE BETWEEN [! $tmpdir !] FIRST LINE AFTER\n      }\n      ALIGNED BETWEEN BRACKETS\n      {\n      SECOND LINE BEFORE [! $rcldir !] SECOND LINE BETWEEN [! $tmpdir !] SECOND LINE AFTER\n      }\n    }\n  }\n?]",
                  "\"{\\n  {\\n    {\\n      FIRST LINE BEFORE VAR1 FIRST LINE BETWEEN VAR2 FIRST LINE AFTER\\n    }\\n    ALIGNED BETWEEN BRACKETS\\n    {\\n    SECOND LINE BEFORE VAR1 SECOND LINE BETWEEN VAR2 SECOND LINE AFTER\\n    }\\n  }\\n}\\n\"");
    }

    [Test]
    public void TestEvalTemplateLeadingNewline ()
    {
      //content:{
      //  x:"\nx"
      //}
      //template:[?
      //  a
      //  b[! $R.x !]c
      //  d
      //?]
      DoEvalTest ("{t:[?\n  a\n  b[! $R.x !]c\n  d\n?] c:{x:\"\\nx\"} <-t $c}", "\"a\\nb\\nxc\\nd\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines0 ()
    {
      DoEvalTest ("{t:[?\n  \n?] <-t #}", "\"\\n\"");
    }
    
    [Test]
    public void TestEvalTemplateEmptyLines1 ()
    {
      DoEvalTest ("{t:[?\n  \n  \n?] <-t #}", "\"\\n\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines2 ()
    {
      DoEvalTest ("{t:[?\n  \n  [! \"foo\" + ~s !]\n  \n?] <-t #}", "\"\\n\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines3 ()
    {
      DoEvalTest ("{t:[?\n  \n  [! ~s !]\n  \n?] <-t #}", "\"\\n\\n\"");
    }
    
    [Test]
    public void TestEvalTemplateEmptyLines4 ()
    {
      DoEvalTest ("{t:[?\n  \n  [! ~s !]\na \n?] <-t #}", "\"  \\n  \\na \\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines5 ()
    {
      DoEvalTest ("{u:~s t:[?\n  line0\n  [! \"foo\" + $u !]\n  line1\n?] <-t #}", "\"line0\\nline1\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines6 ()
    {
      DoEvalTest ("{t:[?\n\n[! ~s !]\n\n?] <-t #}", "\"\\n\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines7 ()
    {
      DoEvalTest ("{t:[?\n\n[! ~s !]\n\n\n?] <-t #}", "\"\\n\\n\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines8 ()
    {
      DoEvalTest ("{t:[?\n  \n[! ~s !]\n  \n  \n?] <-t #}", "\"\\n\\n\\n\"");
    }

    
    [Test]
    public void TestEvalTemplateEmptyLines9 ()
    {
      DoEvalTest ("{u:~s t:[?\n  \n  \n?] <-t #}", "\"\\n\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines10 ()
    {
      DoEvalTest ("{u:~s t:[?\n  \n \n?] <-t #}", "\" \\n\\n\"");
    }

    [Test]
    public void TestEvalTemplateEmptyLines11 ()
    {
      //[?
      //  
      //  [! "foo" + ~s !]
      //  a
      //?]
      DoEvalTest ("{t:[?\n  \n  [! \"foo\" + ~s !]\n  a\n?] <-t #}", "\"\\na\\n\"");
    }

    /*
    [Test]
    public void TestStringTemplateCoercion ()
    {
      DoEvalTest ("{t:[?\n  a\n  b\n  c\n?] <-string $t}", "a\\nb\\nc\\n");
    }
    */

    [Test]
    public void TestTemplateToBlockCoercion ()
    {
      DoEvalTest ("{t:[?\n  a\n  b\n  c\n?] <-block $t}", "{:\"a\\nb\\nc\\n\"}");
    }

    [Test]
    public void TestStringTemplateCoercion ()
    {
      DoEvalTest ("template \"foo bar baz\"", "[?foo bar baz?]");
    }

    [Test]
    public void TestLex ()
    {
      DoEvalTest ("lex \"{x:1 y:2}\"", "\"{\" \"x\" \":\" \"1\" \" \" \"y\" \":\" \"2\" \"}\"");
    }

    [Test]
    public void TestStringTemplateCoercionMulti ()
    {
      DoEvalTest ("template \"foo\nbar\nbaz\n\"", "[?\n  foo\n  bar\n  baz\n?]");
    }

    [Test]
    public void TestStringTemplateEscaped ()
    {
      DoEvalTest ("2 template \"foo\nbar\nbaz\n\"", "[??\n  foo\n  bar\n  baz\n??]");
    }

    [Test]
    public void TestFormatTemplate ()
    {
      DoEvalTest ("format [?\n  foo\n  bar\n  baz\n?]", "\"[?\\n  foo\\n  bar\\n  baz\\n?]\"");
    }
    
    [Test]
    public void TestFormatTemplate1 ()
    {
      DoEvalTest ("#pretty format parse \"[?\n  before\n    [! eval {host:#foo port:#bar} !]\n  after\n?]\"", 
                  "\"[?\\n  before\\n    [! eval {host:#foo port:#bar} !]\\n  after\\n?]\"");
    }
    
    [Test]
    public void TestFormatTemplate2 ()
    {
      DoEvalTest ("#pretty format parse \"[?\n  before\n    [! eval {host:$R <-{<-user_entry $R} each $R.users} !]\n  after\n?]\"", 
                  "\"[?\\n  before\\n    [! eval {host:$R <-{<-user_entry $R} each $R.users} !]\\n  after\\n?]\"");
    }

    [Test]
    public void TestCanonicalCubeFormat ()
    {
      DoEvalTest ("#testcanonical format parse \"[S|x #a 10.0 #a -- #a 30.0]\"", "\"[S|x #a 10.0 #a -- #a 30.0]\"");
    }

    [Test]
    public void TestCanonicalCubeFormat1 ()
    {
      DoEvalTest ("#testcanonical format parse \"[S|bp ap #x 1001 1003 #y 2002 -- #x 1002 -- #x 1001 1002]\"", "\"[S|bp ap #x 1001 1003 #y 2002 -- #x 1002 -- #x 1001 1002]\"");
    }

    [Test]
    public void TestTemplateEval ()
    {
      DoEvalTest ("eval [?\n  aaa bbb ccc ddd eee\n?]", "\"aaa bbb ccc ddd eee\\n\"");
    }

    [Test]
    public void TestTemplateEval1 ()
    {
      DoEvalTest ("{a:\"aaa\" b:\"bbb\" c:\"ccc\"} eval [?\n  [! $a !] [! $b !] [! $c !]\n?]", "\"aaa bbb ccc\\n\"");
    }

    [Test]
    [Ignore ("because")]
    public void TestServer ()
    {
      //Once we get some traction with demo/messaging, should be able to deprecate this test and the operators it depends on.
      DoEvalTest ("{http:httpstart \"http://*:8000/\" server:fiber {context:httprecv $http qs:httpqs $context <-$context httpsend eval {i:$qs.i+1}} request:{<-($R.i < 100) switch {:request \"http://localhost:8000/\" httpget $R :$R}} output:request {i:0} :httpstop $http <-$output}", "{i:100}");
    }

    [Test]
    [Ignore ("because")]
    public void TestHttpSendMulti ()
    {
      //Test sending two replies containing the same payload.
      //This test sometimes get stuck when run from inside the runner, but never under the normal shell.
      //I have convinced myself that it's something to do with monodevelop ide and not my code.
      //So I'm ignoring it for now until we create some superior test harness for working with distributed apps.
      //eval {serve:{r1:httprecv $h r2:httprecv $h :($r1 & $r2) httpsend "{}" <-0l} h:httpstart "http://*:8001/test/" s:fiber {<-serve $h} c1:fiber {<-"http://localhost:8001/test" httpget {}} c2:fiber {<-"http://localhost:8001/test" httpget {}} :wait $c1 :wait $c2 :wait $s :httpstop $h <-0l}
      DoEvalTest ("{serve:{r1:httprecv $h r2:httprecv $h :($r1 & $r2) httpsend \"{}\" <-0} h:httpstart \"http://*:8001/test/\" s:fiber {<-serve $h} c1:fiber {<-\"http://localhost:8001/test\" httpget {}} c2:fiber {<-\"http://localhost:8001/test\" httpget {}} :wait $c1 :wait $c2 :wait $s :httpstop $h <-0}", "0");
    }

    [Test]
    public void TestCalcSpread ()
    {
      DoEvalTest ("{bbo:{bp:10.00 10.01 10.02 10.01 10.00 ap:10.02 10.03 10.03 10.02 10.01} sprd:$bbo.ap - $bbo.bp <-$sprd / $bbo.bp}",
                 "0.00199999999999996 0.00199800199800196 0.000998003992015947 0.000999000999000978 0.000999999999999979");
    }

    [Test]
    public void TestLastValueInRootPosition ()
    {
      DoEvalTest ("{a:1 b:2 c:$a + $b f:{<-$c}}", "{a:1 b:2 c:3 f:{<-$c}}");
    }

#if __MonoCS__
    [Test]
    public void TestMonoSelfExecWithExit ()
    {
      DoEvalTest ("{status:unwrap #status from try {:\"exit.rcl\" save #pretty format {go:exit 21} exe:(codebase {}) + \"/rcl.exe\" <-exec \"mono --debug \" + $exe + \" --program=exit.rcl --action=go\"} :$status assert 21 <-0}", "0");
    }
#endif

    RCRunner runner = RCRunner.TestRunner ();

    /// <summary>
    /// Run the test using RCFormat.DefaultNoT (No timestamps on cubes)
    /// </summary>
    public void DoEvalTest (string code, string expected)
    {
      CoreTest.DoTest (runner, RCFormat.DefaultNoT, code, expected);
    }

    public void DoRawTest (string code, string expected)
    {
      CoreTest.DoRawTest (runner, RCFormat.DefaultNoT, code, expected);
    }

    public void DoEvalTest (string code, string expected, RCFormat format)
    {
      CoreTest.DoTest (runner, format, code, expected);
    }
  }
}


using System;
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  [TestFixture]
  public class ParserTest
  {
    [Test]
    public void TestByte ()
    {
      DoParserTest ("\\x0A \\x1B \\x2C");
    }

    [Test]
    public void TestString ()
    {
      DoParserTest ("\"test\" \"some\" \"strings\"");
    }

    [Test]
    public void TestStringEscapeChars ()
    {
      DoParserTest ("\"\\\\n\" \"\\\\r\" \"\\\\t\" \"\\\\a\" \"\\\\\"");
    }

    [Test]
    public void TestStringUnescapedChars ()
    {
      DoParserTest ("\"\\n\" \"\\r\" \"\\t\" \"\\a\"");
    }

    [Test]
    public void TestEscapedForwardSlashInString ()
    {
      //Allow forward slashes to be escaped in strings literals.
      //This is for compatibility with json.
      //Many json library's escape forward slashes to be more compatible with html.
      //I don't want to do this with RC but I do want to be able to work with json strings.
      //So our string escaping routine will allow forward slashes to be escaped.
      DoParserTest ("\"a\\/b\\/c\"", "\"a/b/c\"");
    }

    [Test]
    public void TestEscapedUnicodeString ()
    {
      DoParserTest ("\"\\u8ba1\\u7b97\\u673a\\u2022\\u7f51\\u7edc\\u2022\\u6280\\u672f\\u7c7b\"", "\"计算机•网络•技术类\"");
    }

    [Test]
    public void TestDouble ()
    {
      DoParserTest ("1 2 3d", "1.0 2.0 3.0");
    }

    [Test]
    public void TestDoubleWithThousands ()
    {
      DoParserTest ("1000.0");
    }

    [Test]
    public void TestNaN ()
    {
      DoParserTest ("NaN");
    }

    [Test]
    public void TestNoDotsDefaultToLong ()
    {
      DoParserTest ("1 2 3", "1 2 3");
    }

    [Test]
    public void TestNoDotOrSuffixDefaultsToLong ()
    {
      DoParserTest ("100 200 300");
    }

    [Test]
    public void TestDotsDefaultToDouble ()
    {
      DoParserTest ("1.0 2.0 3.0");
    }

    [Test]
    public void TestDecimal ()
    {
      DoParserTest ("10.01 10.0125 10.015m");
    }

    [Test]
    public void TestBoolean ()
    {
      DoParserTest ("true false true false false");
    }

    [Test]
    public void TestIncr ()
    {
      DoParserTest ("++ +- +~");
    }

    [Test]
    public void TestTimestamp ()
    {
      DoParserTest ("2015.05.24 08:12:00.1234567 2015.05.25 09:13:00.1234567 2015.05.26 10:14:00.1234567");
    }

    [Test]
    public void TestDate ()
    {
      DoParserTest ("2015.05.24 2015.05.25 2015.05.26");
    }

    [Test]
    public void TestTime ()
    {
      DoParserTest ("08:12 09:13 10:14");
    }

    [Test]
    public void TestDatetime ()
    {
      DoParserTest ("2015.05.24 08:12 2015.05.25 09:13 2015.05.26 10:14");
    }

    [Test]
    public void TestTimespan ()
    {
      DoParserTest ("100.07:29:00.0000001 -10.07:30:00.0000001 1.07:31:00.0000001"); 
    }

    [Test]
    public void TestSymbolSimple ()
    {
      DoParserTest ("#x");
    }

    [Test]
    public void TestSymbolMulti ()
    {
      DoParserTest ("#x,y");
    }

    [Test]
    public void TestSymbolEmpty ()
    {
      DoParserTest ("#");
    }

    [Test]
    public void TestSymbolTyped ()
    {
      DoParserTest ("#10,20,30");
    }

    [Test]
    public void TestSymbolNegative ()
    {
      DoParserTest ("#-1");
    }

    [Test]
    public void TestSymbolNegative1 ()
    {
      DoParserTest ("#foo,-1");
    }

    [Test]
    public void TestSymbolWithDouble ()
    {
      DoParserTest ("#1.2");
    }

    [Test]
    public void TestSymbolMixed ()
    {
      DoParserTest ("#x #x,y #10,20,30");
    }

    [Test]
    public void TestSymbolInBlock ()
    {
      DoParserTest ("{x:#a,b #c,d}");
    }

    [Test]
    public void TestSymbolInExpression ()
    {
      DoParserTest ("#a,b #c,d + #e,f #g,h");
    }

    [Test]
    public void TestSimpleMonadicExpressionWithLiteral ()
    {
      DoParserTest ("sum 1 2 3");
    }

    [Test]
    public void TestSimpleDyadicExpressionWithLiteral ()
    {
      DoParserTest ("1 2 3 + 4 5 6");
    }

    [Test]
    public void TestSimpleDyadicExpressionWithReference ()
    {
      DoParserTest ("$x + $y");
    }

    [Test]
    public void TestSimpleMonadicExpressionWithReference ()
    {
      DoParserTest ("sum $x");
    }

    [Test]
    public void TestNestedMonadicExpressions ()
    {
      DoParserTest ("not not true");
    }
    
    [Test]
    public void TestMultipleOperatorsInExpression ()
    {
      DoParserTest ("$w + $x / $y - $z");
    }

    [Test]
    public void TestMinusProblem ()
    {
      //The key thing is whether whitespace precedes the minus sign.
      //I think it's worth giving the lexer a single token lookback for
      //this purpose since this will confuse some people right off the bat.
      DoParserTest ("1-2", "1 - 2");
    }

    [Test]
    public void TestMinusProblemAlone ()
    {
      DoParserTest ("-1");
    }

    [Test]
    public void TestMinusProblemWithSymbol ()
    {
      DoParserTest ("$x-1.0", "$x - 1.0");
    }

    [Test]
    public void TestMinusProblemWithParen ()
    {
      DoParserTest ("(-1.0)", "-1.0");
    }

    [Test]
    public void TestMinusProblemWithParen1 ()
    {
      DoParserTest ("(-1.0)-1.0", "-1.0 - 1.0");
    }

    [Test]
    public void TestMinusProblemWithBlock ()
    {
      DoParserTest ("{:-1.0}");
    }

    [Test]
    public void TestVectorWithNegativeLong ()
    {
      DoParserTest ("1 -2");
    }
    
    [Test]
    public void TestMeaninglessParens ()
    {
      DoParserTest ("($x + $y)", "$x + $y");
    }

    [Test]
    public void TestMeaninglessParensWithMultipleLiterals ()
    {
      DoParserTest ("(1 - 2 - 3)", "1 - 2 - 3");
    }

    [Test]
    public void TestMeaninglessParensWithMultipleOperators ()
    {
      DoParserTest ("$x + ($y / $z)", "$x + $y / $z");
    }

    [Test]
    public void TestMeaninglessParensAroundVector ()
    {
      DoParserTest ("(1 2 3)", "1 2 3");
    }

    [Test]
    public void TestLeftParenInExpression ()
    {
      DoParserTest ("($x + $y) / $z");
    }

    [Test]
    public void TestLeftParenInExpressionWithLiteral ()
    {
      DoParserTest ("($x + 1) / $y");
    }

    [Test]
    public void TestMixedNestingExpressions ()
    {
      DoParserTest ("(1 - 2 - 3) - 4");
    }

    [Test]
    public void TestMixedNestingExpressionsWithReference ()
    {
      DoParserTest ("($w - $x - $y) - $z");
    }

    [Test]
    public void TestMultipleNestedExpressions ()
    {
      DoParserTest ("((1 - 2 - 3) - 4) - 5");
    }

    [Test]
    public void TestMultipleMixedNestedExpressions ()
    {
      DoParserTest ("((1 - 2) - 3 - 4) - 5");
    }

    [Test]
    public void TestMeaninglessParensInComplexExpression ()
    {
      DoParserTest ("((1 - 2) - (3 - 4)) - 5", "((1 - 2) - 3 - 4) - 5");
    }

    [Test]
    public void TestMixedNestingExpressions1 ()
    {
      DoParserTest ("1.0 - (2.0 - 3.0) / 4.0");
    }

    [Test]
    public void TestEmptyBlock ()
    {
      DoParserTest ("{}");
    }

    [Test]
    public void TestBlockWithOneVariable ()
    {
      DoParserTest ("{x:1}");
    }

    [Test]
    public void TestBlockWithMultipleVariables ()
    {
      DoParserTest ("{x:1 y:2 z:3}");
    }

    [Test]
    public void TestNestedEmptyBlock ()
    {
      DoParserTest ("{x:{}}");
    }

    [Test]
    public void TestNestedBlock ()
    {
      DoParserTest ("{x:{a:1}}");
    }

    [Test]
    public void TestNestedBlockWithExpression ()
    {
      DoParserTest ("{x:{a:1 - 2}}");
    }

    [Test]
    public void TestImpliedBlock ()
    {
      DoParserTest ("x:1", "{x:1}");
    }

    [Test]
    public void TestImpliedBlockNested ()
    {
      DoParserTest ("x:{a:1}", "{x:{a:1}}");
    }

    [Test]
    public void TestUnnamedBlock ()
    {
      DoParserTest ("{:1}");
    }

    [Test]
    public void TestUnnamedBlockWithTwoVariables ()
    {
      DoParserTest ("{:1 :2}");
    }

    [Test]
    public void TestUnnamedBlockWithTwoExpressions ()
    {
      DoParserTest ("{:1 + 2 :3 + 4}");
    }
    
    [Test]
    public void TestUnnamedImpliedBlock ()
    {
      DoParserTest (":1", "{:1}");
    }

    [Test]
    public void TestExpressionInBlocks ()
    {
      DoParserTest ("{x:1 + 2}");
    }

    [Test]
    public void TestTwiceNestedEmptyBlock ()
    {
      DoParserTest ("{x:1 y:{a:{}}}");
    }

    [Test]
    public void TestBlocksAsArguments ()
    {
      DoParserTest ("{a:1 b:2 c:3} == {a:1 b:2 c:4}");
    }

    [Test]
    public void TestOperatorInlineDyadic ()
    {
      //Aka lambda
      DoParserTest ("1.0 2.0 3.0 {<-$right - $left} 4.0 5.0 6.0");
    }

    [Test]
    public void TestOperatorInlineMonadic ()
    {
      DoParserTest ("{<-$R + 1.0} 1.0 2.0 3.0");
    }

    [Test]
    public void TestOperatorWithLeftBlock ()
    {
      DoParserTest ("{} + 1.0");
    }

    [Test]
    public void TestEmptyBlocksAsArgumentsNested ()
    {
      DoParserTest ("{x:{} == {}}");
    }

    [Test]
    public void TestOperatorConsecutiveNames ()
    {
      DoParserTest ("1 - sum 2");
    }

    [Test]
    public void TestUserOperator ()
    {
      //This is not a user operator the test is misnamed.
      DoParserTest ("{x:1.0 y:sum 1.0}");
    }

    [Test]
    public void TestQuotedNamesInReferences ()
    {
      DoParserTest ("{'a b c':1 2 3 '!@#$%^&*':4 5 6}");
      DoParserTest ("{x:\"a\" \"b\" c:1 2 3}");
      DoParserTest ("{x:\"a\" \"b\" 'c-d':1 2 3}");
      DoParserTest ("$'a b'.c.'d e'");
    }

    [Test]
    public void TestQuotedNamesInSymbol ()
    {
      DoParserTest ("#'..','..'");
    }

    [Test]
    public void TestQuotedNamesInSymbol1 ()
    {
      DoParserTest ("#'a b c','d e f'");
    }

    [Test]
    public void TestQuotedNamesInSymbol2 ()
    {
      //In this case the unnecessary quote should be removed.
      DoParserTest ("#x,'y'", "#x,y");
    }

    [Test]
    public void TestCubeMultiRow ()
    {
      //the bar separates the "key" of the table from values.
      DoParserTest ("[S|x #a 10.0 #a 20.0 #a 30.0]");
    }
    
    [Test]
    public void TestCubeMultiCol ()
    {
      DoParserTest ("[S|a b c #x 1 2 3]");
    }

    [Test]
    public void TestCubeEmpty ()
    {
      DoParserTest ("[]");
    }

    [Test]
    public void TestCubesWithNoTimeline ()
    {
      DoParserTest ("[]");
      DoParserTest ("[x 1]");
      DoParserTest ("[x y 1 10]");
      DoParserTest ("[x y z 1 10 100]");
      DoParserTest ("[x 1 2]");
      DoParserTest ("[x y 1 10 2 20]");
      DoParserTest ("[x y z 1 10 100 2 20 200 3 30 300]");
      DoParserTest ("[x y z -- 10 100 2 20 200 3 30 300]");
      DoParserTest ("[x y z 1 10 100 2 -- 200 3 30 300]");
      DoParserTest ("[x y z 1 10 100 2 20 200 3 30 --]");
    }

    [Test]
    public void TestCubesWithNoTimelineHavingGETS ()
    {
      DoParserTest ("[S a #x 1]");
      DoParserTest ("[T S a 00:00 #x 1]");
      DoParserTest ("[E T S a 0 00:00 #x 1]");
      DoParserTest ("[G E T S a 0 0 00:00 #x 1]");
    }

    [Test]
    public void TestCubeDupWithTwoColumns ()
    {
      DoParserTest ("[S|bp ap #x 1000 1002 #x -- 1004 #x 1001 1003]");
    }

    [Test]
    public void TestCubeWithNull ()
    {
      //We don't represent nulls as data internally.
      //The implementation will depend on a timeline.
      DoParserTest ("[S|x #a 10.0 #a -- #a 30.0]", "[S|x #a 10.0 #a 30.0]");
    }

    [Test]
    public void TestCubeWithNullInFirstRow ()
    {
      DoParserTest ("[S|x #a -- #a -- #a 10.0]", "[S|x #a 10.0]");
    }

    [Test]
    public void TestCubeWithNullInLastRow ()
    {
      //Parser assumed that the type would always be on the last value in the column.
      DoParserTest ("[S|x #a 10 #a --]", "[S|x #a 10]");
    }

    [Test]
    public void TestCubeDupsAreNotIgnored ()
    {
      DoParserTest ("[S|x #a 10 #a 10]", "[S|x #a 10 #a 10]");
    }

    [Test]
    public void TestCubeDupsAreNotIgnoredBoolean ()
    {
      DoParserTest ("[S|x #a false #a false]", "[S|x #a false #a false]");
    }

    [Test]
    public void TestCubeDupsAreNotIgnoredEvent ()
    {
      DoParserTest ("[E|S|x 0 #a 0 1 #a 0]", "[E|S|x 0 #a 0 1 #a 0]", RCFormat.Default);
    }

    [Test]
    public void TestCubeSymbolColumn ()
    {
      DoParserTest ("[S|x #a #i #b #j]");
    }

    [Test]
    public void TestCubeIncrColumn ()
    {
      DoParserTest ("[S|x #a ++ #b ++]");
    }

    [Test]
    public void TestCubeWithExplicitTime ()
    {
      DoParserTest ("[E|S|a 1 #x 200]", "[E|S|a 1 #x 200]", RCFormat.Default);
      DoParserTest ("[E|S|a 1 #x 100 3 #y 300]", "[E|S|a 1 #x 100 3 #y 300]", RCFormat.Default);
    }

    [Test]
    public void TestCubeWithExplicitTimeAndNulls()
    {
      DoParserTest ("[E|S|a b 1 #x 100 -- 3 #y -- 3000]", "[E|S|a b 1 #x 100 -- 3 #y -- 3000]", RCFormat.Default);
    }

    [Test]
    public void TestCubeWithG ()
    {
      DoParserTest ("[G|E|S|a 1000 1 #x 200]", "[G|E|S|a 1000 1 #x 200]", RCFormat.Default);
    }

    [Test]
    public void TestCubeInExpression ()
    {
      DoParserTest ("[S|y #0 1 #0 2] = [S|y #0 1 #0 2]");
    }

    [Test]
    public void TestColumnOrderStableWithNullsInTheFirstRow ()
    {
      DoParserTest ("[x y z -- 10 100 2 20 200 3 30 300]");
    }

    [Test]
    [Ignore ("because")]
    public void TestCubeWithColumnOfBlock ()
    {
      DoParserTest ("[x {a:1} {b:2} {c:3}]");
      DoParserTest ("{u:[x {a:1} {b:2} {c:3}]}");
      DoParserTest ("[S|x #a {:1} #b {:2} #c {:3}]");
      DoParserTest ("[S|x #a {<-$a0 + $a1} #b {<-$b0 + $b1} #c {<-$c0 + $c1}]");
      DoParserTest ("[x {a:[q 1]} {b:[r 2]} {c:[s 3]}]");
      DoParserTest ("[x {t:[?This is a [!$R.parameter!]?] <-t {parameter:\"template\"}}]");
    }

    [Test]
    public void TestRandom ()
    {
      DoParserTest ("{:#0 #1 write {x:1 10} :#0 #1 write {x:2 20} :(0 read #0) assert [S|x #0 1 #0 10] :(0 read #1) assert [S|x #1 2 #1 20] <-true}");
    }

    [Test]
    public void TestTemplateWithoutCode ()
    {
      DoParserTest ("[? ?]"); //, "{:\" \"}");
      DoParserTest ("[? some free text ?]"); //, "{:\" some free text \"}");
      DoParserTest ("[?<html></html>?]"); //, "{:\"<html></html>\"}");
      DoParserTest ("[? ?]"); //, "{:\" \"}");
      DoParserTest ("[??]"); //, "{:\"\"}");
      DoParserTest ("[?? [? ?] ??]"); //, "{:\" [? ?] \"}");
      DoParserTest ("[??? [??] ???]"); //, "{:\" [??] \"}");
      DoParserTest ("[???[??]???]"); //, "{:\"[??]\"}");
    }

    [Test]
    public void TestTemplateWithCode ()
    {
      DoParserTest ("[? [! $x !] ?]", "[? [! $x !] ?]"); // "{:\" \" :$x :\" \"}");
      DoParserTest ("[? [! $x !] ?]", "[? [! $x !] ?]"); //"{:\" \" :$x :\" \"}");
      DoParserTest ("[?[! $x !]?]");  //"{:\"\" :$x :\"\"}");
      //Now let's see if we can trick the parser.
      DoParserTest ("[?!][! $x !]?]"); //, "{:\"!]\" :$x :\"\"}");

      DoParserTest ("[?[! $x !]!]?]"); //, "{:\"\" :$x :\"!]\"}");
      DoParserTest ("[??[![!! $x !!]!]??]"); //, "{:\"[!\" :$x :\"!]\"}");
      //I'm not quite sure if this is ok like this.  Since the value in the
      //code block is just a string, a code block is not created on the way out.
      //Maybe we should prevent this by encapsulating literals inside a block.
      DoParserTest ("[?[! \"[? ?]\" !]?]");//, "[?[? ?]?]"); //, "{:\"\" :\"[? ?]\" :\"\"}");
      DoParserTest ("[?this is a question??]"); //, "{:\"this is a question?\"}");
      //Now let's do some more stuff in the code section.
      DoParserTest ("[?before[! $x + $y !]after?]"); //, "{:\"before\" :$x + $y :\"after\"}");
      DoParserTest ("[?before[! {<-$x - $y} !]after?]"); //, "{:\"before\" :{<-$x - $y} :\"after\"}");
      DoParserTest ("[?before[! {x:1 y:2 z:3} !]after?]"); //, "{:\"before\" :{x:1l y:2l z:3l} :\"after\"}");
      DoParserTest ("[?before[! [] !]after?]"); //, "{:\"before\" :[] :\"after\"}");
      //Multiple code sections?
      DoParserTest ("[?before[! $x !]between[! $y !]after?]"); //, "{:\"before\" :$x :\"between\" :$y :\"after\"}");
      //Nested code sections?
      DoParserTest ("[?before[! [?inside?] !]after?]"); //, "{:\"before\" :{:\"inside\"} :\"after\"}");
      DoParserTest ("[?before[! {x:[?one?] y:[?two?] z:[?three?]} !]after?]"); //, "{:\"before\" :{x:{:\"one\"} y:{:\"two\"} z:{:\"three\"}} :\"after\"}");
    }

    [Test]
    public void TestTemplateWithCodeEx ()
    {
      //Bugs found after TestTemplateWithCode
      DoParserTest ("[?foo[! operator {} !]?]");
      DoParserTest ("{t:[?text?]}");
      DoParserTest ("{head:[?a head?] body:[?a body?] html:[?<html>[! head {} !][! body {} !]</html>?] <-html {}}");
    }

    [Test]
    public void TestTemplateMultiline ()
    {
      DoParserTest ("[?\n  line number one\n  line number two\n  line number three\n?]",
                    "[?\n  line number one\n  line number two\n  line number three\n?]");
      DoParserTest ("    [?\n    line number one\n    line number two\n    line number three\n  ?]",
                    "[?\n  line number one\n  line number two\n  line number three\n?]");
    }

    [Test]
    public void TestTemplateMultiline1 ()
    {
      /*
      [?
        <html>
          [!head {}!]
          [!body {}!]
        </html>
      ?]
      */
      DoParserTest ("[?\n  <html>\n    [! head {} !]\n    [! body {} !]\n  </html>\n?]", 
                    "[?\n  <html>\n    [! head {} !]\n    [! body {} !]\n  </html>\n?]");
    }

    [Test]
    public void TestTemplateMultilineCRLF ()
    {
      //When CRLFs are involved strip them out in the parser.
      DoParserTest ("[?\r\n  line number one\r\n  line number two\r\n  line number three\r\n?]",
                    "[?\n  line number one\n  line number two\n  line number three\n?]");
      DoParserTest ("    [?\r\n    line number one\r\n    line number two\r\n    line number three\r\n  ?]",
                    "[?\n  line number one\n  line number two\n  line number three\n?]");
/*
[?
  <html>
    [!head {}!]
    [!body {}!]
  </html>
?]
*/
      DoParserTest ("[?\r\n  <html>\r\n    [! head {} !]\r\n    [! body {} !]\r\n  </html>\r\n?]",
                    "[?\n  <html>\n    [! head {} !]\n    [! body {} !]\n  </html>\n?]");
    }

    [Test]
    public void TestTemplateMultilineEx ()
    {
/*
[?
  <html>
    <h1>[!string $R!]</h1>
  </html>
?]
*/
      DoParserTest ("[?\n  <html>\n    <h1>[! string $R !]</h1>\n  </html>\n?]", 
                    "[?\n  <html>\n    <h1>[! string $R !]</h1>\n  </html>\n?]");
    }

    [Test]
    public void TestTemplateMultilineWithMultipleSectionsOnOne ()
    {
/*
[?
  first line
  this [!$R.is!] the [!$R.middle!] line
  last line
?]
*/
      DoParserTest ("[?\n  first line\n  this [! $R.is !] the [! $R.middle !] line\n  last line\n?]",
                    "[?\n  first line\n  this [! $R.is !] the [! $R.middle !] line\n  last line\n?]");
    }

    [Test]
    public void TestTemplateWithStringArray ()
    {
      DoParserTest ("[?\na\n          [! \"w\" \"x\" \"y\" \"z\" !]\n?]",
                    "[?\n  a\n            [! \"w\" \"x\" \"y\" \"z\" !]\n?]");
    }

    [Test]
    public void TestTemplateWithLongArray ()
    {
      DoParserTest ("[?\na\n          [! 1 2 3 4 5 !]\n?]", "[?\n  a\n            [! 1 2 3 4 5 !]\n?]");
    }

    [Test]
    public void TestTemplateMultilineMultipleCodeSections0 ()
    {
      DoParserTest ("[?\n[! \"a\" \"b\" \"c\" !][! \"x\" \"y\" \"z\" !]\n?]",
                    "[?\n  [! \"a\" \"b\" \"c\" !][! \"x\" \"y\" \"z\" !]\n?]");
    }

    [Test]
    public void TestTemplateMultilineMultipleCodeSections1 ()
    {
      DoParserTest ("[?\n  [! \"a\" \"b\" \"c\" !][! \"x\" \"y\" \"z\" !]\n?]", 
                    "[?\n  [! \"a\" \"b\" \"c\" !][! \"x\" \"y\" \"z\" !]\n?]");
    }

    [Test]
    public void TestTemplateMultilineMultipleCodeSections2 ()
    {
      DoParserTest ("[?\n  [! \"a\" \"b\" \"c\" !]\n    [! \"d\" \"e\" \"f\" !]\n      [! \"g\" \"h\" \"i\" !]?]", 
                    "[?\n  [! \"a\" \"b\" \"c\" !]\n    [! \"d\" \"e\" \"f\" !]\n      [! \"g\" \"h\" \"i\" !]?]");
    }

    [Test]
    public void TestTemplateMultilineMultipleCodeSections3 ()
    {
      //Now make sure that when there are multiple code sections per line, that everything still works.
      DoParserTest ("[?\n  [! \"a\" !] [! \"b\" \"c\" !]\n    [! \"d\" \"e\" !] [! \"f\" !]\n      [! \"g\" !] [! \"h\" !] [! \"i\" !]?]", 
                    "[?\n  [! \"a\" !] [! \"b\" \"c\" !]\n    [! \"d\" \"e\" !] [! \"f\" !]\n      [! \"g\" !] [! \"h\" !] [! \"i\" !]?]");
    }

    [Test]
    public void TestTemplateMultilineMultipleCodeSections4 ()
    {
      //One code section per line, multiple lines.
      DoParserTest ("[?\n  [! \"a\" \"b\" \"c\" !]\n  [! \"x\" \"y\" \"z\" !]\n?]", 
                    "[?\n  [! \"a\" \"b\" \"c\" !]\n  [! \"x\" \"y\" \"z\" !]\n?]");
    }

    [Test]
    public void TestIncrementalVectorParsing ()
    {
      runner.Reset ();
      RCValue vector0 = runner.Read ("1 2 3");
      RCValue vector1 = runner.Read ("4 5 6");
      Assert.AreEqual ("1 2 3", vector0.ToString ());
      Assert.AreEqual ("1 2 3 4 5 6", vector1.ToString ());
    }

    [Test]
    public void TestProgrammaticSymbolBuilding ()
    {
      Assert.AreEqual ("#1,2,3", RCSymbolScalar.From ((long) 1, (long) 2, (long) 3).ToString ());
      Assert.AreEqual ("#a,b,c", RCSymbolScalar.From ("a", "b", "c").ToString ());
      Assert.AreEqual ("#'a-b',c,d", RCSymbolScalar.From ("a-b", "c", "d").ToString ());
      Assert.AreEqual ("#a_b,c,d", RCSymbolScalar.From ("a_b", "c", "d").ToString ());
      Assert.AreEqual ("#'1','2','3'", RCSymbolScalar.From ("1", "2", "3").ToString ());
    }

    [Test]
    public void TestProgrammaticPartExtraction ()
    {
      //RCSymbolScalar symbol = RCSymbolScalar.From ((long) 0, "CDwindow-2b19bc6d-b257-4c8d-8104-0d2bae022016");
      bool fragment;
      RCSymbol symbol = (RCSymbol) runner.Peek ("#0,'this-is-a-test'", out fragment);
      Assert.AreEqual ("this-is-a-test", symbol[0].Part (1));
    }

    [Test]
    public void TestEmptyVector ()
    {
      DoParserTest ("~x", "~x");
      DoParserTest ("~b", "~b");
      DoParserTest ("~l", "~l");
      DoParserTest ("~d", "~d");
      DoParserTest ("~m", "~m");
      DoParserTest ("~y", "~y");
      DoParserTest ("~t", "~t");
      DoParserTest ("~n", "~n");
    }

    protected RCRunner runner = new RCRunner ();
    public void DoParserTest (string code)
    {
      DoParserTest (code, code);
    }

    public void DoParserTest (string code, string expected)
    {
      DoParserTest (code, expected, RCFormat.DefaultNoT);
    }

    public void DoParserTest (string code, string expected, RCFormat format)
    {
      runner.Reset ();
      RCValue result = runner.Read (code);
      //Console.Out.WriteLine ("code:{0}", code);
      //Console.Out.WriteLine ("expected:{0}", expected);
      string actual = result.Format (format);
      //Console.Out.WriteLine ("actual:{0}", actual);
      Assert.IsNotNull (actual, "RCParser.Parse result was null");
      Assert.AreEqual (expected, actual);
    }
  }
}

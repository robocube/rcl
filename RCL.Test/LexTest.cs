
using RCL.Kernel;
using System.Collections.Generic;
using NUnit.Framework;

namespace RCL.Test
{
  [TestFixture]
  public class LexerTest
  {
    [Test]
    public void TestByteLiterals ()
    {
      RCArray<RCToken> tokens = Lex ("\\xA0 \\xB1 \\xC2");
      CheckTokens (tokens, "\\xA0", " ", "\\xB1", " ", "\\xC2");
      CheckTypes (tokens,
                  RCTokenType.Literal,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Literal,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Literal);
    }

    [Test]
    public void TestNumbers ()
    {
      RCArray<RCToken> tokens = Lex ("1.0 2.0 3.0");
      CheckTokens (tokens, "1.0", " ", "2.0", " ", "3.0");
      CheckTypes (tokens,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Number);
    }

    [Test]
    public void TestDouble ()
    {
      RCArray<RCToken> tokens = Lex ("1 2 3d");
      CheckTokens (tokens, "1", " ", "2", " ", "3d");
      CheckTypes (tokens,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Number);
    }

    [Test]
    public void TestBoolean ()
    {
      RCArray<RCToken> tokens = Lex ("true");
      CheckTokens (tokens, "true");
      CheckTypes (tokens, RCTokenType.Boolean);
    }

    [Test]
    public void TestNumber ()
    {
      RCArray<RCToken> tokens = Lex (" 37 ");
      CheckTokens (tokens, " ", "37", " ");
      CheckTypes (tokens, RCTokenType.WhiteSpace, RCTokenType.Number, RCTokenType.WhiteSpace);
    }

    [Test]
    public void TestNumberWithE ()
    {
      RCArray<RCToken> tokens = Lex ("3.4e5d");
      CheckTokens (tokens, "3.4e5d");
      CheckTypes (tokens, RCTokenType.Number);
    }

    [Test]
    public void TestNumberWithEPlus ()
    {
      RCArray<RCToken> tokens = Lex ("1.026451811E+15d");
      CheckTokens (tokens, "1.026451811E+15d");
      CheckTypes (tokens, RCTokenType.Number);
    }

    [Test]
    public void TestString ()
    {
      RCArray<RCToken> tokens = Lex ("\"HelloWorld\"");
      CheckTokens (tokens, "\"HelloWorld\"");
      CheckTypes (tokens, RCTokenType.String);
    }

    [Test]
    public void TestStringWithWhitespace ()
    {
      RCArray<RCToken> tokens = Lex ("\" Hello World! \"");
      CheckTokens (tokens, "\" Hello World! \"");
      CheckTypes (tokens, RCTokenType.String);
    }

    [Test]
    public void TestIncr ()
    {
      RCArray<RCToken> tokens = Lex ("++ +- +~");
      CheckTokens (tokens, "++", " ", "+-", " ", "+~");
      CheckTypes (tokens,
                  RCTokenType.Incr,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Incr,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Incr);
    }

    [Test]
    public void TestReference ()
    {
      RCArray<RCToken> tokens = Lex ("$var");
      CheckTokens (tokens, "$var");
      CheckTypes (tokens, RCTokenType.Reference);
    }

    [Test]
    public void TestTypedReference ()
    {
      RCArray<RCToken> tokens = Lex ("i$var");
      CheckTokens (tokens, "i$var");
      CheckTypes (tokens, RCTokenType.Reference);
    }

    [Test]
    public void TestOperatorNextToName ()
    {
      RCArray<RCToken> tokens = Lex ("$R+$L");
      CheckTokens (tokens, "$R", "+", "$L");
      CheckTypes (tokens, RCTokenType.Reference, RCTokenType.Name, RCTokenType.Reference);
    }

    [Test]
    public void TestOperatorNextToNameAndFollowedByDecimalPoint ()
    {
      RCArray<RCToken> tokens = Lex ("$R+.5d");
      CheckTokens (tokens, "$R", "+", ".5d");
      CheckTypes (tokens, RCTokenType.Reference, RCTokenType.Name, RCTokenType.Number);
    }

    [Test]
    public void TestNestedReference ()
    {
      RCArray<RCToken> tokens = Lex ("$var.block.target");
      CheckTokens (tokens, "$var.block.target");
      CheckTypes (tokens, RCTokenType.Reference);
    }

    [Test]
    public void TestSymbol ()
    {
      RCArray<RCToken> tokens = Lex ("#var");
      CheckTokens (tokens, "#var");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestTimestamp ()
    {
      RCArray<RCToken> tokens = Lex ("2015.05.24 08:12:00.123456");
      CheckTokens (tokens, "2015.05.24 08:12:00.123456");
      CheckTypes (tokens, RCTokenType.Time);
    }

    [Test]
    public void TestTimestamp1 ()
    {
      RCArray<RCToken> tokens = Lex ("2015.05.24 08:12:00.123456 2015.05.25 09:13:00.123456");
      CheckTokens (tokens, "2015.05.24 08:12:00.123456", " ", "2015.05.25 09:13:00.123456");
      CheckTypes (tokens, RCTokenType.Time, RCTokenType.WhiteSpace, RCTokenType.Time);
    }

    [Test]
    public void TestTimespan ()
    {
      RCArray<RCToken> tokens = Lex ("0.08:00:00.000000 10.08:00:00.000000 100.08:00:00.000000");
      CheckTokens (tokens,
                   "0.08:00:00.000000",
                   " ",
                   "10.08:00:00.000000",
                   " ",
                   "100.08:00:00.000000");
      CheckTypes (tokens,
                  RCTokenType.Time,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Time,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Time);
    }

    [Test]
    public void TestDate ()
    {
      RCArray<RCToken> tokens = Lex ("2015.05.24");
      CheckTokens (tokens, "2015.05.24");
      CheckTypes (tokens, RCTokenType.Time);
    }

    [Test]
    public void TestTime ()
    {
      RCArray<RCToken> tokens = Lex ("08:12");
      CheckTokens (tokens, "08:12");
      CheckTypes (tokens, RCTokenType.Time);
    }

    [Test]
    public void TestDatetime ()
    {
      RCArray<RCToken> tokens = Lex ("2015.05.24 08:12");
      CheckTokens (tokens, "2015.05.24 08:12");
      CheckTypes (tokens, RCTokenType.Time);
    }

    [Test]
    public void TestSymbolTwoPart ()
    {
      RCArray<RCToken> tokens = Lex ("#part0,part1");
      CheckTokens (tokens, "#part0,part1");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestSymbolThreePart ()
    {
      RCArray<RCToken> tokens = Lex ("#part0,part1,part2");
      CheckTokens (tokens, "#part0,part1,part2");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestSymbolTyped ()
    {
      RCArray<RCToken> tokens = Lex ("#id,43,23");
      CheckTokens (tokens, "#id,43,23");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestSymbolWithDouble ()
    {
      RCArray<RCToken> tokens = Lex ("#id,1.2,3.4");
      CheckTokens (tokens, "#id,1.2,3.4");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestEmptySymbol ()
    {
      RCArray<RCToken> tokens = Lex ("#");
      CheckTokens (tokens, "#");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestWildcardSymbol ()
    {
      RCArray<RCToken> tokens = Lex ("#page,*");
      CheckTokens (tokens, "#page,*");
      CheckTypes (tokens, RCTokenType.Symbol);
    }

    [Test]
    public void TestQuotedSymbols ()
    {
      RCArray<RCToken> tokens = Lex ("#'a sym' #'b sym' #'c sym'");
      CheckTokens (tokens, "#'a sym'", " ", "#'b sym'", " ", "#'c sym'");
      CheckTypes (tokens,
                  RCTokenType.Symbol,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Symbol,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Symbol);
    }

    [Test]
    public void TestEmptyBlock ()
    {
      RCArray<RCToken> tokens = Lex ("{}");
      CheckTokens (tokens, "{", "}" );
      CheckTypes (tokens, RCTokenType.Block, RCTokenType.Block);
    }

    [Test]
    public void TestAnonymousBlocks ()
    {
      RCArray<RCToken> tokens = Lex ("{:23 :true :\"My String\"}");
      CheckTokens (tokens, "{", ":", "23", " ", ":", "true", " ", ":", "\"My String\"", "}" );
      CheckTypes (tokens,
                  RCTokenType.Block,
                  RCTokenType.Evaluator,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Evaluator,
                  RCTokenType.String,
                  RCTokenType.Block);
    }

    [Test]
    public void TestNamedBlocks ()
    {
      RCArray<RCToken> tokens = Lex ("{number:23 boolean:true string:\"this is a string\"}");
      CheckTokens (tokens,
                   "{",
                   "number",
                   ":",
                   "23",
                   " ",
                   "boolean",
                   ":",
                   "true",
                   " ",
                   "string",
                   ":",
                   "\"this is a string\"",
                   "}" );
      CheckTypes (tokens,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.String,
                  RCTokenType.Block);
    }

    [Test]
    public void TestMixingNestedNamedAndAnonymousBlocks ()
    {
      RCArray<RCToken> tokens = Lex (
        "{number:23 :true nestedBlock:{nestedString:\"aNestedString\"}string:\"a string\"}");
      CheckTokens (tokens,
                   "{",
                   "number",
                   ":",
                   "23",
                   " ",
                   ":",
                   "true",
                   " ",
                   "nestedBlock",
                   ":",
                   "{",
                   "nestedString",
                   ":",
                   "\"aNestedString\"",
                   "}",
                   "string",
                   ":",
                   "\"a string\"",
                   "}" );
      CheckTypes (tokens,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.String,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.String,
                  RCTokenType.Block);
    }

    [Test]
    public void TestOperator ()
    {
      RCArray<RCToken> tokens = Lex ("or{left:true right:false}");
      CheckTokens (tokens, "or", "{", "left", ":", "true", " ", "right", ":", "false", "}" );
      CheckTypes (tokens,
                  RCTokenType.Name,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.Block);
    }

    [Test]
    public void TestNestedOperator ()
    {
      RCArray<RCToken> tokens = Lex ("or{left:and{left:true right:true}right:false}");
      CheckTokens (tokens,
                   "or",
                   "{",
                   "left",
                   ":",
                   "and",
                   "{",
                   "left",
                   ":",
                   "true",
                   " ",
                   "right",
                   ":",
                   "true",
                   "}",
                   "right",
                   ":",
                   "false",
                   "}" );
      CheckTypes (tokens,
                  RCTokenType.Name,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Name,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Boolean,
                  RCTokenType.Block);
    }

    [Test]
    public void TestGteExpression ()
    {
      RCArray<RCToken> tokens = Lex ("1d>=2d");
      CheckTokens (tokens, "1d", ">=", "2d");
      CheckTypes (tokens, RCTokenType.Number, RCTokenType.Name, RCTokenType.Number);
    }

    [Test]
    public void TestWhiteSpace ()
    {
      RCArray<RCToken> tokens = Lex (" \t \a \n \r ");
      CheckTokens (tokens, " \t \a \n \r ");
      CheckTypes (tokens, RCTokenType.WhiteSpace);
    }

    [Test]
    public void TestBooleanWithWhitespace ()
    {
      RCArray<RCToken> tokens = Lex (" \t true ");
      CheckTokens (tokens, " \t ", "true", " " );
      CheckTypes (tokens, RCTokenType.WhiteSpace, RCTokenType.Boolean, RCTokenType.WhiteSpace);
    }

    [Test]
    public void TestBackslashInStringToken ()
    {
      RCArray<RCToken> tokens = Lex ("{a:\"\\\\A\" b:\"B\"}");
      CheckTokens (tokens, "{", "a", ":", "\"\\\\A\"", " ", "b", ":", "\"B\"", "}" );
      CheckTypes (tokens,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.String,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.String,
                  RCTokenType.Block);
    }

    [Test]
    public void TestStringWithSyntaxEmbedded ()
    {
      RCArray<RCToken> tokens = Lex ("\"assert failed:=={l:1l r:2l}\"");
      CheckTokens (tokens, "\"assert failed:=={l:1l r:2l}\"" );
      CheckTypes (tokens, RCTokenType.String);
    }

    [Test]
    public void TestEscapedBackslashJunk ()
    {
      RCArray<RCToken> tokens = Lex ("\\");
      CheckTokens (tokens, "\\" );
      CheckTypes (tokens, RCTokenType.Junk);
    }

    [Test]
    public void TestMultipleEscapedBackslashInString ()
    {
      RCArray<RCToken> tokens = Lex ("\"\\\\\\\\\"");
      CheckTokens (tokens, "\"\\\\\\\\\"" );
      CheckTypes (tokens, RCTokenType.String);
    }

    [Test]
    public void Test3Spacers ()
    {
      RCArray<RCToken> tokens = Lex ("--- --- ---");
      CheckTokens (tokens, "---", " ", "---", " ", "---" );
      CheckTypes (tokens,
                  RCTokenType.Spacer,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Spacer,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Spacer);
    }

    [Test]
    public void TestConsecutiveSpecialOperators ()
    {
      RCArray<RCToken> tokens = Lex ("!*%-+/");
      CheckTokens (tokens, "!", "*", "%", "-", "+", "/");
      CheckTypes (tokens,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name);
    }

    [Test]
    public void TestSymbolWithDollarSign ()
    {
      RCArray<RCToken> tokens = Lex ("a:$ref");
      CheckTokens (tokens, "a", ":", "$ref" );
      CheckTypes (tokens, RCTokenType.Name, RCTokenType.Evaluator, RCTokenType.Reference);
    }

    [Test]
    public void TestDashNotAllowedInNames ()
    {
      RCArray<RCToken> tokens = Lex ("a-b-c");
      CheckTokens (tokens, "a", "-", "b", "-", "c" );
      CheckTypes (tokens,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name,
                  RCTokenType.Name);
    }

    [Test]
    public void TestUnderscoreAllowedInNames ()
    {
      RCArray<RCToken> tokens = Lex ("a_b_c");
      CheckTokens (tokens, "a_b_c" );
      CheckTypes (tokens, RCTokenType.Name);
    }

    [Test]
    public void TestQuotedName ()
    {
      RCArray<RCToken> tokens = Lex ("{'t h i s h a s s p a c e s':1 2 3l}");
      CheckTokens (tokens, "{", "'t h i s h a s s p a c e s'", ":", "1", " ", "2", " ", "3l", "}");
      CheckTypes (tokens,
                  RCTokenType.Block,
                  RCTokenType.Name,
                  RCTokenType.Evaluator,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Number,
                  RCTokenType.WhiteSpace,
                  RCTokenType.Number,
                  RCTokenType.Block);
    }

    [Test]
    public void TestLeadingUnderscore ()
    {
      RCArray<RCToken> tokens = Lex ("_private");
      CheckTokens (tokens, "_private");
      CheckTypes (tokens, RCTokenType.Name);
    }

    [Test]
    public void TestMultiLeadingUnderscore ()
    {
      RCArray<RCToken> tokens = Lex ("___private");
      CheckTokens (tokens, "___private");
      CheckTypes (tokens, RCTokenType.Name);
    }

    [Test]
    public void TestPipe ()
    {
      RCArray<RCToken> tokens = Lex ("x|y");
      CheckTokens (tokens, "x", "|", "y");
      CheckTypes (tokens, RCTokenType.Name, RCTokenType.Spacer, RCTokenType.Name);
    }

    /*
       [Test]
       public void TestTemplate ()
       {
       RCArray<RCToken> tokens = Lex ("[?\n  <html>\n    [!head {}!]\n    [!body {}!]\n
           </html>\n?]");
       //CheckTokens (tokens, "[?", "\n  <html>\n    ", "[!", "head", " ", "{", "}"
       }
     */

    protected RCRunner runner = new RCRunner ();
    public RCArray<RCToken> Lex (string code)
    {
      runner.Reset ();
      return runner.Lex (code);
    }


    public void LexTest (string code, string tokens, string types)
    {
      NUnit.Framework.Assert.AreEqual (tokens,
                                       runner.Rep (string.Format ("lex format {0}",
                                                                  code)).ToString ());
      NUnit.Framework.Assert.AreEqual (types,
                                       runner.Rep (string.Format ("lextype format {0}",
                                                                  code)).ToString ());
    }

    /// <summary>
    /// The usual xunit convention is that the expected parameter comes first,
    /// but given that the expected parameter is an array that needs to be
    /// hardcoded into the unit test, its nice to be able to use a param array.
    /// </summary>
    public void CheckTokens (RCArray<RCToken> actual, params string[] expected)
    {
      NUnit.Framework.Assert.AreEqual (expected.Length, actual.Count);
      for (int i = 0; i < actual.Count; ++i)
      {
        NUnit.Framework.Assert.AreEqual (expected[i], actual[i].Text);
      }
    }

    public void CheckTypes (RCArray<RCToken> actual, params RCTokenType[] expected)
    {
      NUnit.Framework.Assert.AreEqual (expected.Length, actual.Count);
      for (int i = 0; i < actual.Count; ++i)
      {
        NUnit.Framework.Assert.AreEqual (expected[i], actual[i].Type);
      }
    }
  }
}

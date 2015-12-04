﻿
using System;
using System.Threading;
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  [TestFixture]
  public class RunnerTest
  {
    [Test]
    public void TestEmptyStringIsNoop ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, ""));
    }

    [Test]
    public void TestLiteral ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("1.0 2.0 3.0", RepString (runner, "1 2 3d"));
    }

    [Test]
    public void TestEmptyStringAfterStatement ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "a:1 2 3"));
      Assert.AreEqual ("", RepString (runner, ""));
    }

    [Test]
    public void TestInteractive ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "x:1 2 3"));
      Assert.AreEqual ("", RepString (runner, "y:4 5 6"));
      Assert.AreEqual ("5 7 9", RepString (runner, "$x+$y"));
    }

    [Test]
    public void TestNestedBlock ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "x:{a:1 2 3d}"));
      Assert.AreEqual ("{a:1.0 2.0 3.0}", RepString (runner, "$x"));
    }

    [Test]
    public void TestNestedBlockWithPriorVariables ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "x:1 2 3"));
      Assert.AreEqual ("", RepString (runner, "y:4 5 6"));
      Assert.AreEqual ("", RepString (runner, "a:{x:10 y:20}"));
      Assert.AreEqual ("{x:10 y:20}", RepString (runner, "$a"));
    }

    [Test]
    public void TestBlockAfterReadingVariable()
    {
      RCRunner runner = new RCRunner();
      Assert.AreEqual("", RepString (runner, "a:1.0"));
      Assert.AreEqual("1.0", RepString (runner, "$a"));
      Assert.AreEqual("", RepString (runner, "b:{x:10.0}"));
      Assert.AreEqual("{x:10.0}", RepString (runner, "$b"));
    }

    [Test]
    public void TestInvokeNestedOperatorDefinition ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "lib:{f:{<-$R * $R}}"));
      Assert.AreEqual ("4", RepString (runner, "lib.f 2"));
    }

    [Test]
    public void TestInvokeNestedOperatorAndSave ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "lib:{f:{<-$R * $R}}"));
      Assert.AreEqual ("", RepString (runner, "square:lib.f 2"));
      Assert.AreEqual ("4", RepString (runner, "$square"));
    }

    [Test]
    public void TestYieldingABlock ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("", RepString (runner, "f:{<-eval {x:$R * $R}}"));
      Assert.AreEqual ("", RepString (runner, "r:f 10"));
      Assert.AreEqual ("{x:100}", RepString (runner, "$r"));
    }

    [Test]
    public void TestReplBotWorks ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("1", RepString (runner, "write [S|a #x 1]"));
      Assert.AreEqual ("[S|a #x 1]",  RepString (runner, "#x read 0"));
    }

    [Test]
    public void TestEvalBlock ()
    {
      RCRunner runner = new RCRunner ();
      Assert.AreEqual ("{a:1 b:2 c:3}", RepString (runner, "{a:1 b:2 c:3}"));
    }

    [Test]
    public void TestForConflictingResults ()
    {
      RCRunner runner = new RCRunner (RCActivator.Default,
                                      new RCLog (new RCL.Core.Output ()), 1, RCRunner.CreateArgs ());
      Assert.AreEqual ("{status:0 data:5}", RepString (runner, "first #r from eval {serve:{b:bot {<-try {<-eval {<-2 + 3}}} f1:fiber {r:wait $b <-$r} <-wait $f1} r:wait fiber {<-serve #}}"));
    }

    [Test]
    public void TestExceptionsDoNotGetStuck ()
    {
      RCRunner runner = new RCRunner ();
      try
      { 
        RepString (runner, "$a+$b");
      }
      catch (Exception) {}
      Assert.AreEqual ("3.0", RepString (runner, "1.0+2.0"));
    }

    [Test]
    public void TestExitFromShell ()
    {
      RCRunner runner = new RCRunner ();
      for (int i = 0; i < 5; ++i)
      {
        bool caught = false;
        try
        {
          RepString (runner, "exit 1");
        }
        catch (ThreadAbortException)
        {
          Thread.ResetAbort ();
          caught = true;
        }
        Assert.IsTrue (caught);
        runner.Reset ();
      }
    }

    [Test]
    public void TestExitFromShellRemote ()
    {
      RCRunner runner = new RCRunner ();
      for (int i = 0; i < 5; ++i)
      {
        runner.Rep ("p:startx \"mono rcl.exe --output=clean\"");
        runner.Rep ("$p writex \"exit 0\"");
        runner.Rep ("waitx $p");
      }
    }

    [Test]
    public void TestExitFromScript ()
    {
      RCRunner runner = new RCRunner ();
      for (int i = 0; i < 5; ++i)
      {
        runner.Rep ("\"exit.o2\" save #pretty format {:exit 0}");
        runner.Rep ("p:startx \"mono rcl.exe --output=clean --program=exit.o2\"");
        runner.Rep ("waitx $p");
        runner.Rep ("exec \"rm exit.o2\"");
      }
    }

    [Test]
    public void TestCdWithQuotes ()
    {
      RCRunner runner = new RCRunner ();
      runner.Rep ("p:startx \"mono rcl.exe --output=clean\"");
      runner.Rep ("$p writex \"pwd\"");
      RCString pwdBefore = (RCString) runner.Rep ("\"\\n\" readx $p");
      runner.Rep ("$p writex \"cd \\\"..\\\"\"");
      runner.Rep ("$p writex \"pwd\"");
      RCString pwdAfter = (RCString) runner.Rep ("\"\\n\" readx $p");
      runner.Rep ("$p writex \"exit\"");
      runner.Rep ("waitx $p");
      string before = pwdBefore [0].TrimEnd ('"');
      string after = pwdAfter [0].TrimEnd ('"');
      Assert.IsTrue (before.StartsWith (after));
    }

    protected string RepString (RCRunner runner, string code)
    {
      int status;
      RCValue result = runner.Rep (code);
      if (result == null)
      {
        return "";
      }
      return result.Format (RCFormat.DefaultNoT);
    }
  }
}
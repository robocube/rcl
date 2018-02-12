
using System;
using RCL.Kernel;
using NUnit.Framework;

namespace RCL.Test
{
  /// <summary>
  /// This test is similar in structure to RCOperatorTest.
  /// But the purpose of this test is to check the overloads of standard operators on cubes.
  /// We don't check for every type of vector on every operator, that would be over the top.
  /// Also I am not testing all the different variations of columnar operations on cubes.
  /// For that we will have one set of operator agnostic tests.
  /// </summary>
  [TestFixture]
  public class CubeTest
  {
    [Test]
    public void TestCubeMathWithSymbol ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x 1000 1002 1 #y 2000 2002 2 #y 2001 2003 3 #x 1001 1003 4 #y 2002 2003 5 #x 1002 1003 6 #x 1001 1002 7 #y 2001 2002 8 #y 2000 2001 9 #x 1000 1001] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 0 #x 2 1 #y 2 4 #y 1 5 #x 1]");
    }

    [Test]
    public void TestCubeMathWithSymbol1 ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x 1000 1002 1 #y 2000 2002 2 #y 2001 2003 3 #x 1001 1003 4 #y 2002 -- 5 #x 1002 -- 6 #x 1001 1002 7 #y 2001 2002 8 #y 2000 2001 9 #x 1000 1001] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 0 #x 2 1 #y 2 4 #y 1 5 #x 1]");
    }

    [Test]
    public void TestCubeMathOneSymbolNoNulls ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x 1000 1002 1 #x 1000 1004 2 #x 1001 1003] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 0 #x 2 1 #x 4 2 #x 2]");
    }

    [Test]
    public void TestCubeMathOneSymbolWithNulls ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x 1000 -- 1 #x -- 1004 2 #x -- 1003] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 1 #x 4 2 #x 3]");
    }

    [Test]
    public void TestCubeMathOneSymbolWithNullsReverse ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x -- 1004 1 #x 1000 -- 2 #x 1001 --] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 1 #x 4 2 #x 3]");
    }

    [Test]
    public void TestCubeMathWithTwoSymbolsWithNull ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x 1000 -- 1 #y -- 2008 2 #x -- 1004 3 #y 2000 -- 4 #x -- 1003 5 #y -- 2006] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 2 #x 4 3 #y 8 4 #x 3 5 #y 6]");
    }

    [Test]
    public void TestCubeMathWithMultipleTimelines ()
    {
      DoTest (
        "{bp:[E|S|bp 0 #x 1000 3l #y 2000] ap:[E|S|ap 1 #y 2008 2 #x 1004 4 #x 1003 5 #y 2006] <-$ap.ap-$bp.bp}",
        "[E|S|x 2 #x 4 3 #y 8 4 #x 3 5 #y 6]");
    }

    [Test]
    public void TestCubeMathWithTwoSymbolsWithNullReverse ()
    {
      DoTest (
        "{bbo:[E|S|bp ap 0 #x -- 1000 1 #y 2008 -- 2 #x 1004 -- 3 #y -- 2000 4 #x 1003 -- 5 #y 2006 --] <-$bbo.ap-$bbo.bp}",
        "[E|S|x 2 #x -4 3 #y -8 4 #x -3 5 #y -6]");
    }

    [Test]
    public void TestCubeMathWithMultipleKeys ()
    {
      DoTest (
        "{book:[E|S|bs as 0 #x,1002 -- 1 1 #x,1003 -- 2 2 #x,1001 1 -- 3 #x,1002 1 -- 4 #x,1001 2 -- 5 #x,1003 1 -- 6 #x,1002 2 -- 7 #x,1001 3 --] <-$book.as min $book.bs}",
        "[E|S|x 3 #x,1002 1 5 #x,1003 1]");
    }

    [Test]
    public void TestCubeMathWithMultipleKeys1 ()
    {
      DoTest (
        "{book:[E|S|bs as 0 #x,1002 -- 1 1 #x,1003 -- 2 2 #x,1001 1 -- 3 #x,1002 1 -- 4 #x,1001 2 -- 5 #x,1003 1 -- 6 #x,1002 2 -- 7 #x,1001 3 --] <-$book.as min $book.bs}",
        "[E|S|x 3 #x,1002 1 5 #x,1003 1]");
    }

    [Test]
    public void TestCubeMathSequential ()
    {
      DoTest (
        "{sprd:[E|S|sprd 0 #x 4.0 0 #y 8.0 1 #x 3.0 1 #y 7.0 2 #x 5.0 2 #y 6.0] <-avg $sprd.sprd}",
        "[E|S|x 0 #x 4.0 0 #y 8.0 1 #x 3.5 1 #y 7.5 2 #x 4.0 2 #y 7.0]");
    }

    [Test]
    public void TestCubeMathMonadic ()
    {
      DoTest (
        "{u:[E|S|signal 0 #y true 1 #x true 2 #x true 3 #y false] <-not $u.signal}",
        "[E|S|x 0 #y false 1 #x false 3 #y true]");
    }

    [Test]
    public void TestCubeMathMonadicWithDups ()
    {
      DoTest (
        "{t:[E|S|signal 0 #y 1.1 1 #x 2.1 2 #x 2.2 3 #y 2.1] <-long $t.signal}",
        "[E|S|x 0 #y 1 1 #x 2 3 #y 2]");
    }

    [Test]
    public void TestCubeMathObject ()
    {
      DoTest ("[S|x #a 1] + [S|x #b 1]", "[S|x #a 1 #b 1]");
    }

    [Test]
    public void TestCubeMathObject1 ()
    {
      DoTest ("[S|x #a 1] + [S|x #b 1 #a 1]", "[S|x #a 2 #b 1]");
    }

    [Test]
    public void TestCubeMathObject2 ()
    {
      DoTest ("[S|x #a 1 #b 1] + [S|x #b 1]", "[S|x #a 1 #b 2]");
    }

    [Test]
    public void TestCubeMathTime ()
    {
      //I may want to change the output in this case.
      DoTest ("[x 08:00 09:00 10:00] + [x 00:30 00:30 00:30]", "[x 0001.01.01 08:30:00.0000000 0001.01.01 09:30:00.0000000 0001.01.01 10:30:00.0000000]");
    }

    [Test]
    public void TestCube ()
    {
      DoTest ("{u:[S|x #a 0 #b 1] <-$u cube eval {y:$u.x + 10}}", "[S|x y #a 0 10 #b 1 11]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u.S cube eval {y:$u.x + 10}}", "[S|y #a 10 #b 11]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u cube eval {y:$u.x + 10}}", "[S|x y #a 0 10 #b 1 11]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u.S cube eval {y:$u.x + 10}}", "[S|y #a 10 #b 11]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u cube eval {y:10 + $u.x}}", "[S|x y #a 0 10 #b 1 11]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u.S cube eval {y:10 + $u.x}}", "[S|y #a 10 #b 11]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u.S cube eval {y:10}}", "[S|y #a 10 #b 10]");
      DoTest ("{u:[S|x #a 0 #b 1] <-$u cube eval {y:10}}", "[S|x y #a 0 10 #b 1 10]");

      //Test incr
      DoTest ("{u:[S|x #a 0 #b 1] <-$u.S cube eval {i:++}}", "[S|i #a ++ #b ++]");
    }

    [Test]
    public void TestCubeReplaceSymbol ()
    {
      //Replace the symbol column
      //DoTest ("{u:[S|x #a 0l #b 10l] <-$u join eval {S:#c #d x:$u.x}}", "[S|x #c 0l #d 10l]");
      //Replace the symbol column with one element only.
      //DoTest ("{u:[S|x #a 0l] <-$u join eval {S:#c x:$u.x}}", "[S|x #c 0l]");
      //This test exposes a bug where the column indexes returned from join contained the wrong symbols.
      //DoTest ("{u:[S|x #a 10l] <-[] merge $u join eval {S:#vars + $u.S x:$u.x}}", "[S|x #vars,a 10l]");
      //Now we replace symbols like this.
      DoTest ("{u:[S|x #a 10] <-(#vars + $u.S) key $u}", "[S|x #vars,a 10]");
      DoTest ("{u:[S|x #a 10] <-(#vars + $u.S) key cube eval {x:$u.x + 1}}", "[S|x #vars,a 11]");
    }

    [Test]
    public void TestCubeNoSymbol ()
    {
      //Tests for simple cubes with no timeline.
      DoTest ("[x y z 1 2 3 10 20 30] cube [z 4 40]", "[x y z 1 2 4 10 20 40]");
      //This test is to ensure that interior columns can also be replaced.
      DoTest ("[x y z 1 2 4 10 20 40] cube [y 3 30]", "[x y z 1 3 4 10 30 40]");
    }

    [Test]
    public void TestCubeAddSymbol ()
    {
      DoTest ("#a #b #c cube [x y z 1 2 3 4 5 6 7 8 9]", "[S|x y z #a 1 2 3 #b 4 5 6 #c 7 8 9]");
    }

    [Test]
    public void TestCubeNoSymbolWithBlock ()
    {
      DoTest ("{u:[x y 100 1 200 2 300 3 400 4 500 5] <-$u cube eval {z:$u.x + $u.y}}", "[x y z 100 1 101 200 2 202 300 3 303 400 4 404 500 5 505]");
      DoTest ("{u:[x y 100 1 200 2 300 3 400 4 500 5] <-cube eval {z:$u.x + $u.y}}", "[z 101 202 303 404 505]");
      DoTest ("{u:[x 100 200 300 400 500] <-cube eval {z:5 repeat 13}}", "[z 13 13 13 13 13]");
      DoTest ("{u1:[x 100 200 300 400 500] u2:[x 1 2 3 4 5] <-$u1 cube eval {z:$u1.x + $u2.x}}", "[x z 100 101 200 202 300 303 400 404 500 505]");
      DoTest ("{u1:[x 100 200 300 400 500] u2:[x 1 2 3 4 5] <-cube eval {z:$u1.x + $u2.x}}", "[z 101 202 303 404 505]");
    }

    [Test]
    public void TestCubeNoSymbolWithBlock1 ()
    {
      DoTest ("{u:[x 100 200 300 400 500] <-$u cube {z:13}}", "[x z 100 13 200 13 300 13 400 13 500 13]");
    }

    [Test]
    public void TestCubeColumns ()
    {
      //This cube operator could be called flip (or table/untable).
      //And the name cube applied to an operator that creates a cube that fully represents a block.
      DoTest ("cube {E:0 1 2 S:#x #x #x a:10 20 30}", "[E|S|a 0 #x 10 1 #x 20 2 #x 30]");
    }

    [Test]
    public void TestCubeColumns1 ()
    {
      //This cube operator could be called flip (or table/untable).
      //And the name cube applied to an operator that creates a cube that fully represents a block.
      DoTest ("cube {E:0 1 2 T:08:00 09:00 10:00 S:#x #x #x a:10 20 30}", "[E|T|S|a 0 08:00 #x 10 1 09:00 #x 20 2 10:00 #x 30]");
    }

    [Test]
    public void TestCubeNoTimeline ()
    {
      DoTest ("cube {a:1 10 100 b:2 20 200 c:3 30 300}", "[a b c 1 2 3 10 20 30 100 200 300]");
    }

    [Test]
    public void TestCubeNoTimeline1 ()
    {
      DoTest ("cube {t:08:00 09:00 10:00}", "[t 08:00 09:00 10:00]");
    }

    [Test]
    public void TestCubeLeftSymbol ()
    {
      DoTest ("#a #b #c cube {x:1 y:\"2\" z:++}", "[S|x y z #a 1 \"2\" ++ #b 1 \"2\" ++ #c 1 \"2\" ++]");
    }

    [Test]
    public void TestCubeLeftSymbol1 ()
    {
      DoTest ("#a #c cube {:[S|x #a 0 #b 1 #c 2 #d 4]}", "[S|x #a 0 #c 2]");
    }

    [Test]
    public void TestCubeLeftSymbol2 ()
    {
      //Duplicates
      DoTest ("#a #a cube {x:1 1 y:10 10 z:100 100}", "[S|x y z #a 1 10 100 #a 1 10 100]");
    }

    [Test]
    public void TestCubeLeftSymbol3 ()
    {
      //Duplicates
      DoTest ("count #a #a cube {x:1 1 y:10 10 z:100 100}", "2");
    }

    [Test]
    public void TestCubeLeftCube ()
    {
      DoTest ("[S|w #a -1 #b -2 #c -3] cube {x:1 y:\"2\" z:++}", "[S|w x y z #a -1 1 \"2\" ++ #b -2 1 \"2\" ++ #c -3 1 \"2\" ++]");
    }

    [Test]
    public void TestCubeLeftEmpty ()
    {
      //This is important when constructing cubes from datasets that may be empty.
      DoTest ("[] cube {x:1}", "[]");
    }

    [Test]
    public void TestCubeWithSInBlock ()
    {
      //The cube should contain dups in this case.
      DoTest ("cube {S:#a #a x:1 1 y:10 10 z:100 100}", "[S|x y z #a 1 10 100 #a 1 10 100]");
    }

    [Test]
    public void TestKey0 ()
    {
      DoTest ("[] key []", "[]");
    }

    [Test]
    public void TestKey1 ()
    {
      DoTest ("{u:[S|x y #a 0 #c #b 1 #d] <-(colofy $u.y) key $u}", "[S|x y #c 0 #c #d 1 #d]");
    }

    [Test]
    public void TestKey2 ()
    {
      DoTest ("{u:[S|x y #a 0 #c #b 1 #d] <-$u.y key $u}", "[S|x y #c 0 #c #d 1 #d]");
    }

    [Test]
    public void TestKey3 ()
    {
      DoTest ("{u:[x y z 0 1 2 3 4 5 6 7 8] <-$u.x key $u}", "[S|x y z #0 0 1 2 #3 3 4 5 #6 6 7 8]");
    }

    [Test]
    public void TestBlockCubeConversions ()
    {
      DoTest ("cube block []", "[]");
      DoTest ("block cube {}", "{}");
      DoTest ("cube block [x 0]", "[x 0]");
      DoTest ("block cube {:{x:0}}", "{:{x:0}}");
      DoTest ("cube block [S|x #a 0]", "[S|x #a 0]");
      DoTest ("block cube {a:{x:0}}", "{a:{x:0}}");
    }

    [Test]
    public void TestBlockCubeConversions1 ()
    {
      DoTest ("cubify {s:+ 1}", "[S|o l #s,R,0 -- 1 #s \"+\" --]");
      //DoTest ("block cube {s:+ 1}", "{s:+ 1}"); 
    }

    [Test]
    public void TestBlockCubeConversions2 ()
    {
      DoTest ("cubify {s:1 + 1}", "[S|o l #s,L,0 -- 1 #s,R,0 -- 1 #s \"+\" --]");
      //DoTest ("block cube {s:1 + 1}", "{s:1 + 1}");
    }

    [Test]
    public void TestBlockCubeConversions3 ()
    {
      DoTest ("cubify {s:1 + 1 2}", "[S|o l #s,L,0 -- 1 #s,R,0 -- 1 #s,R,1 -- 2 #s \"+\" --]");
      //DoTest ("block cube {s:1 + 1 2}", "{s:1 + 1 2}");
    }

    [Test]
    public void TestBlockCubeConversions4 ()
    {
      DoTest ("cubify {s:1 2 + 1}", "[S|o l #s,L,0 -- 1 #s,L,1 -- 2 #s,R,0 -- 1 #s \"+\" --]");
      //DoTest ("block cube {s:1 2 + 1}", "{s:1 2 + 1}");
    }

    [Test]
    public void TestBlockCubeConversions5 ()
    {
      DoTest ("cubify {t:{s:1 + 1}}", "[S|o l #t,s,L,0 -- 1 #t,s,R,0 -- 1 #t,s \"+\" --]");
      //DoTest ("block cube {t:{s:1 + 1}}", "{t:{s:1 + 1}}");
    }

    [Test]
    public void TestBlockCubeConversions6 ()
    {
      DoTest ("cubify {t:{s:1 + 1 b:2}}", "[S|o l #t,s,L,0 -- 1 #t,s,R,0 -- 1 #t,s \"+\" -- #t,b,0 -- 2]");
      //DoTest ("block cube {t:{s:1 + 1 b:2}}", "{t:{s:1 + 1 b:2}}");
    }

    [Test]
    [Ignore ("because")]
    public void TestBlockCubeCorrespondence ()
    {
      //DoTest ("cube block []", "[]");
      //DoTest ("block cube {}", "{}");
      //DoTest ("cube block [S|x #a 0l]", "[S|x #a 0l]");
      //DoTest ("block cube {a:{x:0l}}", "{a:{x:0l}}");
      DoTest ("cube block [S|x #a,b 0]", "[S|x #a,b 0]");
      DoTest ("block cube {a:{b:{x:0}}}", "{a:{b:{x:0}}}");
      DoTest ("cube block [S|x #a 0 #b 1]", "[S|x #a 0 #b 1]");
      DoTest ("block cube {a:{x:0} b:{x:1}}", "{a:{x:0} b:{x:1}}");
      //Is it really ok to use E here as a kind of index?
      //G was invented for paging purposes but I started thinking about it as other things.
      //E was invented to allow multiple values to change at the same moment. A logical Event identifier.
      //How should it really be?
      //G - For any page the first G should be 0 and the last should be count E-1.
      //It does nothing but allow you to identify the source of the data you are looking at.
      //E will be what links multiple pages together, linking the event info with information derived from that event.
      //E also ties together multiple rows with different symbols to indicate that they are part of the state. The state at the same time.
      //T will measure quantity of time, where applicable. Why need both E and T? Because T is normally a timestamp.
      //And timestamps might not increase monotonically. Also multiple events in definite order might appear to have the same T.
      //S is a point in space being described.
      //Those definitions seem pretty solid to me. But here what I am considering is allowing the same symbol to have multiple values
      //For a given E, and using this idea to allow vectors to be held inside of cubes.
      //So, if there was a complex nested block where the leaf values were arrays of varying length, it would be taken to describe
      //the state of a system at a specific time.
      //Update 2015.06.03 : We are not going to do what I said above, using the E row to identify array indices. 
      //symbols should always be used for that purpose. If too many symbols are a concern then make them more efficient.
      DoTest ("cube block [E S|x 0 #a 10 0 #a 11]", "[E S|x 0 #a 10 0 #a 11]");
      DoTest ("block cube {a:{x:10 11}}", "{a:{x:10 11}}");
      DoTest ("cube block [E S|x 0 #a 10 1 #a 11]", "[E S|x 0 #a 10 1 #a 11]");
      DoTest ("block cube {a:{x:10} a:{x:11}}", "{a:{x:10} a:{x:11}}");
      DoTest ("cube block [E S|x 0 #a 10 0 #b 11]", "[E S|x 0 #a 10 0 #b 11]");
      DoTest ("block cube {a:{x:10} b:{x:11}}", "{a:{x:10} b:{x:11}}");
      DoTest ("cube block [E S|x 0 #a 10 0 #b 11 1 #a 12 1 #b 13]", "[E S|x 0 #a 10 0 #b 11 1 #a 12 1 #b 13]");
      DoTest ("block cube {a:{x:10} b:{x:11} a:{x:12} b:{x:13}}", "{a:{x:10} b:{x:11} a:{x:12} b:{x:13}}");
      DoTest ("cube block [E S|x 0 #a 10 0 #b 11 0 #a 12 0 #b 13]", "[E S|x 0 #a 10 0 #b 11 0 #a 12 0 #b 13]");
      DoTest ("block cube {a:{x:10 12} b:{x:11 13}}", "{a:{x:10 12} b:{x:11 13}}");
      //This is actually not bad so far.
      //Maybe we can start at an even more basic level.
      //So start with vectors as in:
      DoTest ("block [x 1 2 3]", "{x:1 2 3}");
      DoTest ("block [x y 1 10 2 20 3 30]", "{x:1 2 3 y:10 20 30}");
      DoTest ("block [S|x #a 1 #a 2 #a 3]", "{a:{x:1 2 3}}");
      DoTest ("block [S|x #a 1 #b 2 #c 3]", "{a:{x:1} b:{x:2} c:{x:3}}");
      DoTest ("block [S|x y #a 1 10 #a 2 20 #a 3 30]", "{a:{x:1 2 3 y:10 20 30}}");
      DoTest ("block [S|x y #a 1 10 #b 2 20]", "{a:{x:1 y:10} b:{x:2 y:20}}");
      DoTest ("block [E S|x 0 #a 1 0 #a 2 0 #a 3]", "{a:{x:1 2 3}}");
      DoTest ("block [E S|x 0 #a 1 1 #a 2 2 #a 3]", "{a:{x:1} a:{x:2} a:{x:3}}");
      DoTest ("block [E S|x 0 #a 1 0 #b 10]", "{a:{x:1} b:{x:10}}");
      //Nothing is different between the result below and above. Is that a problem?
      DoTest ("block [E S|x 0 #a 1 1 #b 10]", "{a:{x:1} b:{x:10}}");
      //But here there is a difference, there are two a blocks rather than one.
      DoTest ("block [E S|x 0 #a 1 1 #a 2]", "{a:{x:1} a:{x:2}}");
      //If the same symbol appears multiple times with the same E that means the value held multiple scalars at that time.
      //Vectors are the only thing that can hold multiple scalars at one time.
    }

    [Test]
    public void TestMeta ()
    {
      //What should meta give me?
      //type, count, min, max
      DoTest ("meta {x:1 2 3}", "[S|type count #x #long 3]");
      DoTest ("meta {x:{:1 2 3}}", "[S|type count #x,0 #long 3]");
      DoTest ("meta {x:1l y:2d z:3m}", "[S|type count #x #long 1 #y #double 1 #z #decimal 1]");
      DoTest ("meta {:1l :2d :3m}", "[S|type count #0 #long 1 #1 #double 1 #2 #decimal 1]");
      DoTest ("meta {x:{y:{z:1 2 3}}}", "[S|type count #x,y,z #long 3]");
      DoTest ("meta {x:1 2 3 y:$x + 4}", "[S|type count #x #long 3 #y #operator 2]");
      DoTest ("meta {x:$a}", "[S|type count #x #reference 1]");
    }

    [Test]
    public void TestTreeL ()
    {
      DoTest ("#k #v from tree 1 2 3", "[S|k v # # \"\" #0 #0 \"1\" #1 #1 \"2\" #2 #2 \"3\"]");
      DoTest ("#x #y from tree -1 -2 -3", "[S|x y # 0.0 0.0 #0 2.38328747137721E-16 -1.9461661814331 #1 -1.88781514014697 1.0899305793441 #2 2.04311121652593 1.17959081084559]");
    }

    [Test]
    public void TestTreeD ()
    {
      DoTest ("#k #v from tree 1.0 2.0 3.0", "[S|k v # # \"\" #0 #0 \"1\" #1 #1 \"2\" #2 #2 \"3\"]");
      DoTest ("#x #y from tree -1.0 -2.0 -3.0", "[S|x y # 0.0 0.0 #0 2.38328747137721E-16 -1.9461661814331 #1 -1.88781514014697 1.0899305793441 #2 2.04311121652593 1.17959081084559]");
    }

    [Test]
    public void TestTreeM ()
    {
      DoTest ("#k #v from tree 1 2 3m", "[S|k v # # \"\" #0 #0 \"1\" #1 #1 \"2\" #2 #2 \"3\"]");
      DoTest ("#x #y from tree -1 -2 -3m", "[S|x y # 0.0 0.0 #0 2.38328747137721E-16 -1.9461661814331 #1 -1.88781514014697 1.0899305793441 #2 2.04311121652593 1.17959081084559]");
    }

    [Test]
    public void TestTreeB ()
    {
      DoTest ("#k #v from tree true false true", "[S|k v # # \"\" #0 #0 \"True\" #1 #1 \"False\" #2 #2 \"True\"]");
    }

    [Test]
    public void TestTreeY ()
    {
      DoTest ("#k #v from tree #x,0 #y,1 #z,2", "[S|k v # # \"\" #0 #0 \"#x,0\" #1 #1 \"#y,1\" #2 #2 \"#z,2\"]");
    }

    [Test]
    public void TestTreeS ()
    {
      DoTest ("#k #v from tree \"x\" \"y\" \"z\"", "[S|k v # # \"\" #0 #0 \"x\" #1 #1 \"y\" #2 #2 \"z\"]");
    }

    [Test]
    public void TestTree1 ()
    {
      //x 1
      //y 2
      //z 3
      DoTest ("#k #v from tree {x:1 y:2 z:3}", "[S|k v # # \"\" #0 #x \"x\" #0,0 #x,0 \"1\" #1 #y \"y\" #1,0 #y,0 \"2\" #2 #z \"z\" #2,0 #z,0 \"3\"]");
    }

    [Test]
    public void TestTree2 ()
    {
      //k
      //  x 1
      //  y 2
      //  z 3
      DoTest ("#k #v from tree {k:{x:1 y:2 z:3}}", "[S|k v # # \"\" #0 #k \"k\" #0,0 #k,x \"x\" #0,0,0 #k,x,0 \"1\" #0,1 #k,y \"y\" #0,1,0 #k,y,0 \"2\" #0,2 #k,z \"z\" #0,2,0 #k,z,0 \"3\"]");
    }

    [Test]
    public void TestTree3 ()
    {
      //k
      //  x 1
      //l
      //  y 2
      //m
      //  z 3
      DoTest ("#k #v from tree {k:{x:1} l:{y:2} m:{z:3}}", "[S|k v # # \"\" #0 #k \"k\" #0,0 #k,x \"x\" #0,0,0 #k,x,0 \"1\" #1 #l \"l\" #1,0 #l,y \"y\" #1,0,0 #l,y,0 \"2\" #2 #m \"m\" #2,0 #m,z \"z\" #2,0,0 #m,z,0 \"3\"]");
    }

    [Test]
    public void TestTree4 ()
    {
      //x
      //1
      //2
      //3
      DoTest ("#k #v from tree [x 1 2 3]", "[S|k v # # \"\" #0 #x \"x\" #0,0 #x,0 \"1\" #0,1 #x,1 \"2\" #0,2 #x,2 \"3\"]");
    }

    [Test]
    public void TestTree5 ()
    {
      //S  x
      //#a 1
      //#b 2
      //#c 3
      DoTest ("#k #v from tree [S|x #a 1 #b 2 #c 3]", "[S|k v # # \"\" #0 #x \"x\" #0,0 #x,a \"1\" #0,1 #x,b \"2\" #0,2 #x,c \"3\"]");
    }

    [Test]
    public void TestTree6 ()
    {
      //S  x  y
      //#a 1
      //#b    2
      DoTest ("#k #v from tree [S|x y #a 1 -- #b -- 2]", "[S|k v # # \"\" #0 #x \"x\" #0,0 #x,a \"1\" #1 #y \"y\" #1,1 #y,b \"2\"]");
    }

    [Test]
    public void TestTree7 ()
    {
      //Time columns are currently discarded when doing trees from cubes.
      //It made sense to display them in a table context when doing the chart operator.
      //But there is really nowhere for them to go in the tree case.
      //Eventually, we will want to support to visualize these trees in a temporal context.
      //That will also lead to a change in the interpretation of charts. That's why the l - layer column is on charts.
      //Trees should have an analogous z column.
      DoTest ("#k #v from tree [G E T S|x 0 0 00:00 #a 1]", "[S|k v # # \"\" #0 #x \"x\" #0,0 #x,a \"1\"]");
      //[S|n v #0l #x "x" #0l,0l #x,a "1"]
    }

    [Test]
    public void TestTree8 ()
    {
      DoTest ("#k #v from tree {u:[S|x #a 1]}", "[S|k v # # \"\" #0 #u \"u\" #0,0 #u,x \"x\" #0,0,0 #u,x,a \"1\"]");
              //"[S|n v #0l,0l,0l #u \"u\"" +
              //      " #1l,1l,0l #u,S \"S\" #2l,1l,0l #u,x \"x\"" +
              //      " #1l,2l,0l #u,S,0l \"#a\" #2l,2l,0l #u,x,0l \"1\"]");
    }

    [Test]
    public void TestTree9 ()
    {
      DoTest ("#k #v from tree {u0:[S|x #a 1] u1:[S|y #b 2]}",
              "[S|k v # # \"\" #0 #u0 \"u0\" #0,0 #u0,x \"x\" #0,0,0 #u0,x,a \"1\" #1 #u1 \"u1\" #1,0 #u1,y \"y\" #1,0,0 #u1,y,b \"2\"]");
    }

    [Test]
    public void TestTree10 ()
    {
      DoTest ("#k #v from tree {r:$x}", "[S|k v # # \"\" #0 #r \"r\" #0,0 #r,0 \"$x\"]");
    }

    [Test]
    public void TestTree11 ()
    {
      DoTest ("#k #v from tree {r:$x s:$y}", "[S|k v # # \"\" #0 #r \"r\" #0,0 #r,0 \"$x\" "+
                                                    "#1 #s \"s\" #1,0 #s,0 \"$y\"]");
    }

    [Test]
    public void TestTree12 ()
    {
      DoTest ("#k #v from tree {r:$x + $y s:not $y}", "[S|k v # # \"\" #0 #r \"r\" #0,0 #r,0 \"$x + $y\" "+
                                                             "#1 #s \"s\" #1,0 #s,0 \"not $y\"]");
    }

    [Test]
    public void TestTree13 ()
    {
      DoTest ("(#n from tree 1 1) * 2.0", "[S|x # 4.0 #0 2.0 #1 2.0]");
    }

    [Test]
    public void TestTree14 ()
    {
      DoTest ("{t:tree [S|x y z #a 1 2 3] <-#n #w from $t cube eval {w:17}}", "[S|n w # 6.0 17 #0 1.0 17 #0,0 1.0 17 #1 2.0 17 #1,0 2.0 17 #2 3.0 17 #2,0 3.0 17]");
    }

    [Test]
    public void TestTree15 ()
    {
      DoTest ("#n #g from tree 1 -1", "[S|n g # 0.0 2.0 #0 1.0 1.0 #1 -1.0 1.0]");
    }

    [Test]
    public void TestTree16 ()
    {
      DoTest ("#n #g from tree {r:$x}", "[S|n g # 1.0 1.0 #0 1.0 1.0 #0,0 1.0 1.0]");
    }

    [Test]
    public void TestTree17 ()
    {
      DoTest ("#n #g from tree {r:not $x}", "[S|n g # 1.0 1.0 #0 1.0 1.0 #0,0 1.0 1.0]");
    }

    [Test]
    public void TestTree18 ()
    {
      DoTest ("#n #g from tree [a 1 -1]", "[S|n g # 0.0 2.0 #0 0.0 2.0 #0,0 1.0 1.0 #0,1 -1.0 1.0]");
    }

    [Test]
    public void TestChartL ()
    {
      DoTest ("#k #v from chart 1 2 3", "[S|k v #0,0,0 #0 \"1\" #0,1,0 #1 \"2\" #0,2,0 #2 \"3\"]");
    }

    [Test]
    public void TestChartD ()
    {
      DoTest ("#k #v from chart 1.0 2.0 3.0", "[S|k v #0,0,0 #0 \"1\" #0,1,0 #1 \"2\" #0,2,0 #2 \"3\"]");
    }

    [Test]
    public void TestChartM ()
    {
      DoTest ("#k #v from chart 1 2 3m", "[S|k v #0,0,0 #0 \"1\" #0,1,0 #1 \"2\" #0,2,0 #2 \"3\"]");
    }

    [Test]
    public void TestChartB ()
    {
      DoTest ("#k #v from chart true false true", "[S|k v #0,0,0 #0 \"True\" #0,1,0 #1 \"False\" #0,2,0 #2 \"True\"]");
    }

    [Test]
    public void TestChartY ()
    {
      DoTest ("#k #v from chart #x,0 #y,1 #z,2", "[S|k v #0,0,0 #0 \"#x,0\" #0,1,0 #1 \"#y,1\" #0,2,0 #2 \"#z,2\"]");
    }

    [Test]
    public void TestChartS ()
    {
      DoTest ("#k #v from chart \"x\" \"y\" \"z\"", "[S|k v #0,0,0 #0 \"x\" #0,1,0 #1 \"y\" #0,2,0 #2 \"z\"]");
    }

    [Test]
    public void TestChart1 ()
    {
      //x 1
      //y 2
      //z 3
      DoTest ("chart {x:1 y:2 z:3}", "[S|r c l k v #0,0,0 0 0 0 #x \"x\" #0,1,0 0 1 0 #x,0 \"1\" #1,0,0 1 0 0 #y \"y\" #1,1,0 1 1 0 #y,0 \"2\" #2,0,0 2 0 0 #z \"z\" #2,1,0 2 1 0 #z,0 \"3\"]");
    }

    [Test]
    public void TestChart2 ()
    {
      //k
      //  x 1
      //  y 2
      //  z 3
      DoTest ("#k #v from chart {k:{x:1 y:2 z:3}}",
              "[S|k v #0,0,0 #k \"k\" #1,1,0 #k,x \"x\" #1,2,0 #k,x,0 \"1\" #2,1,0 #k,y \"y\" #2,2,0 #k,y,0 \"2\" #3,1,0 #k,z \"z\" #3,2,0 #k,z,0 \"3\"]");
    }

    [Test]
    public void TestChart3 ()
    {
      //k
      //  x 1
      //l
      //  y 2
      //m
      //  z 3
      DoTest ("#k #v from chart {k:{x:1} l:{y:2} m:{z:3}}", 
              "[S|k v #0,0,0 #k \"k\" #1,1,0 #k,x \"x\" #1,2,0 #k,x,0 \"1\" #2,0,0 #l \"l\" #3,1,0 #l,y \"y\" #3,2,0 #l,y,0 \"2\" #4,0,0 #m \"m\" #5,1,0 #m,z \"z\" #5,2,0 #m,z,0 \"3\"]");
    }

    [Test]
    public void TestChart4 ()
    {
      //x
      //1
      //2
      //3
      DoTest ("#k #v from chart [x 1 2 3]", "[S|k v #0,0,0 #x \"x\" #0,1,0 #x,0 \"1\" #0,2,0 #x,1 \"2\" #0,3,0 #x,2 \"3\"]");
    }

    [Test]
    public void TestChart5 ()
    {
      //x  1  2  3
      //S #a #b #c
      DoTest ("chart [S|x #a 1 #b 2 #c 3]",
              "[S|r c l k v #0,0,0 0 0 0 #x \"x\" #0,1,0 0 1 0 #x,0 \"1\" #0,2,0 0 2 0 #x,1 \"2\" #0,3,0 0 3 0 #x,2 \"3\" #1,0,0 1 0 0 #S \"S\" #1,1,0 1 1 0 #S,0 \"#a\" #1,2,0 1 2 0 #S,1 \"#b\" #1,3,0 1 3 0 #S,2 \"#c\"]");
    }

    [Test]
    public void TestChart6 ()
    {
      //S  x y
      //#a 1  
      //#b   2
      DoTest ("#k #v from chart [S|x y #a 1 -- #b -- 2]",
              "[S|k v #0,0,0 #x \"x\" #0,1,0 #x,0 \"1\" #1,0,0 #y \"y\" #1,2,0 #y,0 \"2\" #2,0,0 #S \"S\" #2,1,0 #S,0 \"#a\" #2,2,0 #S,1 \"#b\"]");
            //"[S|k v #0,0,0 #S \"S\" #1,0,0 #x \"x\" #2,0,0 #y \"y\" #0,1,0 #S,0 \"#a\" #1,1,0 #x,0 \"1\" #0,2,0 #S,1 \"#b\" #2,2,0 #y,1 \"2\"]");
    }

    [Test]
    public void TestChart7 ()
    {
      //DoTest ("#n #v from layout [G E T S|x 0l 0l 0l #a 1l]",
      //        "[S|n v #0l,0l,0l #G    \"G\" #0l,1l,0l #E    \"E\" #0l,2l,0l #T    \"T\" #0l,3l,0l #S    \"S\" #0l,4l,0l #x    \"x\"" +
      //        "       #1l,0l,0l #G,0l \"0\" #1l,1l,0l #E,0l \"0\" #1l,2l,0l #T,0l \"0\" #1l,3l,0l #S,0l \"#\" #1l,4l,0l #x,0l \"1\"]");
      DoTest ("#k #v from chart [G E T S|x 0 0 00:00 #a 1]",
              "[S|k v #0,0,0 #x \"x\" #0,1,0 #x,0 \"1\" #1,0,0 #G \"G\" #1,1,0 #G,0 \"0\" #2,0,0 #E \"E\" #2,1,0 #E,0 \"0\" #3,0,0 #T \"T\" #3,1,0 #T,0 \"00:00\" #4,0,0 #S \"S\" #4,1,0 #S,0 \"#a\"]");
              //"[S|k v #0,0,0 #G \"G\" #1,0,0 #E \"E\" #2,0,0 #T \"T\" #3,0,0 #S \"S\" #4,0,0 #x \"x\" #0,1,0 #G,0 \"0\" #1,1,0 #E,0 \"0\" #2,1,0 #T,0 \"0\" #3,1,0 #S,0 \"#a\" #4,1,0 #x,0 \"1\"]");
    }

    [Test]
    public void TestChart8 ()
    {
      DoTest ("#k #v from chart {u:[S|x #a 1]}",
              "[S|k v #0,0,0 #u \"u\" #1,1,0 #u,x \"x\" #1,2,0 #u,x,0 \"1\" #2,1,0 #u,S \"S\" #2,2,0 #u,S,0 \"#a\"]");
              //"[S|k v #0,0,0 #u \"u\" #1,1,0 #u,x \"x\" #1,2,0 #u,x,0 \"1\"]");
              //"[S|k v #0,0,0 #u \"u\"" +
              //      " #1,1,0 #u,S \"S\" #2,1,0 #u,x \"x\"" +
              //      " #1,2,0 #u,S,0 \"#a\" #2,2,0 #u,x,0 \"1\"]");
    }

    [Test]
    public void TestChart9 ()
    {
      DoTest ("#k #v from chart {u0:[S|x #a 1] u1:[S|y #b 2]}",
              "[S|k v #0,0,0 #u0 \"u0\" #1,1,0 #u0,x \"x\" #1,2,0 #u0,x,0 \"1\" #2,1,0 #u0,S \"S\" #2,2,0 #u0,S,0 \"#a\" #4,0,0 #u1 \"u1\" #5,1,0 #u1,y \"y\" #5,2,0 #u1,y,0 \"2\" #6,1,0 #u1,S \"S\" #6,2,0 #u1,S,0 \"#b\"]");
              //"[S|k v #0,0,0 #u0 \"u0\"" +
              //      " #1,1,0 #u0,S \"S\" #2,1,0 #u0,x \"x\"" +
              //      " #1,2,0 #u0,S,0 \"#a\" #2,2,0 #u0,x,0 \"1\"" +
              //      " #0,3,0 #u1 \"u1\"" +
              //      " #1,4,0 #u1,S \"S\" #2,4,0 #u1,y \"y\"" +
              //      " #1,5,0 #u1,S,0 \"#b\" #2,5,0 #u1,y,0 \"2\"]");
    }

    [Test]
    public void TestChart10 ()
    {
      DoTest ("#k #v from chart {r:$x}", "[S|k v #0,0,0 #r \"r\" #0,1,0 #r,0 \"$x\"]");
    }

    [Test]
    public void TestChart11 ()
    {
      DoTest ("#k #v from chart {r:$x s:$y}\"",
              "[S|k v #0,0,0 #r \"r\" #0,1,0 #r,0 \"$x\" #1,0,0 #s \"s\" #1,1,0 #s,0 \"$y\"]");
    }

    [Test]
    public void TestChart12 ()
    {
      DoTest ("#k #v from chart {r:$x + $y s:not $y}",
              "[S|k v #0,0,0 #r \"r\" #0,1,0 #r,0 \"$x + $y\" #1,0,0 #s \"s\" #1,1,0 #s,0 \"not $y\"]");
    }

    [Test]
    public void TestChart13 ()
    {
      DoTest ("chart 1 2 3", "[S|r c l k v #0,0,0 0 0 0 #0 \"1\" #0,1,0 0 1 0 #1 \"2\" #0,2,0 0 2 0 #2 \"3\"]");
    }

    [Test]
    public void TestChart14 ()
    {
      DoTest ("chart [x 1 2 3]",
              "[S|r c l k v #0,0,0 0 0 0 #x \"x\" #0,1,0 0 1 0 #x,0 \"1\" #0,2,0 0 2 0 #x,1 \"2\" #0,3,0 0 3 0 #x,2 \"3\"]");
    }

    [Test]
    public void TestChart15 ()
    {
      DoTest ("chart {x:1 2 3}",
              "[S|r c l k v #0,0,0 0 0 0 #x \"x\" #0,1,0 0 1 0 #x,0 \"1\" #0,2,0 0 2 0 #x,1 \"2\" #0,3,0 0 3 0 #x,2 \"3\"]");
    }

    [Test]
    public void TestChart16 ()
    {
      //Output looks like this.
      //x  1
      //S #a
      //It would extend to more columns and symbols like this.
      //x  1  2  3
      //y  4  5  6
      //S #a #b #c
      DoTest ("chart [S|x #a 1]",
              "[S|r c l k v #0,0,0 0 0 0 #x \"x\" #0,1,0 0 1 0 #x,0 \"1\" #1,0,0 1 0 0 #S \"S\" #1,1,0 1 1 0 #S,0 \"#a\"]");
    }

    [Test]
    public void TestPlusWithOneColNoTimeline ()
    {
      //The stuff commented out is for doing addition on multiple columns at once.
      //We should be able to do that.
      //But currently addition is based on the order (it always adds the first column of either arg).
      //Should it be based on the name?
      string col1row1 = "[x 1]";
      //string col2row1 = "[x y 1 10]";
      //string col3row1 = "[x y z 1 10 100]";
      string col1row2 = "[x 1 2]";
      //string col2row2 = "[x y 1 10 2 20]";
      //string col3row3 = "[x y z 1 10 100 2 20 200 3 30 300]";
      string col1row3 = "[x 1l 2l 3l]";
      //string null00 = "[x y z -- 10 100 2 20 200 3 30 300]";
      //string null11 = "[x y z 1 10 100 2 -- 200 3 30 300]";
      //string null22 = "[x y z 1 10 100 2 20 200 3 30 --]";

      DoTest (string.Format ("{0} + {1}", col1row1, col1row1), "[x 2]");
      DoTest (string.Format ("{0} + {1}", col1row2, col1row2), "[x 2 4]");
      DoTest (string.Format ("{0} + {1}", col1row3, col1row3), "[x 2 4 6]");
      //DoTest (string.Format ("{0} + {1}", col2row1, col2row1), "[x y 2 20]");
      //DoTest (string.Format ("{0} + {1}", col3row1, col3row1), "[x y z 2 20 200]");

      //DoTest (string.Format ("{0} + {1}", col2row2, col2row2), "[x y 2 20 4 40]");
      //DoTest (string.Format ("{0} + {1}", col3row3, col3row3), "[x y z 2 20 200 4 40 400 6 60 600]");
      //DoTest (string.Format ("{0} + {1}", null00, null11), "[x y z -- 20 200 4 -- 400 6 60 600]");
      //DoTest (string.Format ("{0} + {1}", null00, null22), "[x y z -- 10 100 4 40 400 6 60 --]");
      //DoTest (string.Format ("{0} + {1}", null11, null22), "[x y z 2 20 200 4 -- 400 6 60 --]");
    }

    [Test]
    public void TestSortWithNoTimeline ()
    {
      string col1row1 = "[x 1]";
      string col2row1 = "[x y 1 10]";
      string col3row1 = "[x y z 1 10 100]";
      string col1row2 = "[x 1 2]";
      string col2row2 = "[x y 1 10 2 20]";
      string col3row2 = "[x y z 1 10 100 2 20 200]";
      string col3row3 = "[x y z 1 10 100 2 20 200 3 30 300]";
      //string col1row3 = "[x 1l 2l 3l]";
      string null00 = "[x y z -- 10 100 2 20 200 3 30 300]";
      string null11 = "[x y z 1 10 100 2 -- 200 3 30 300]";
      string null22 = "[x y z 1 10 100 2 20 200 3 30 --]";
      
      DoTest (string.Format ("#desc,x sort {0}", col1row1), "[x 1]");
      DoTest (string.Format ("#desc,y sort {0}", col2row1), "[x y 1 10]");
      DoTest (string.Format ("#desc,z sort {0}", col3row1), "[x y z 1 10 100]");
      DoTest (string.Format ("#desc,x sort {0}", col1row2), "[x 2 1]");
      DoTest (string.Format ("#desc,y sort {0}", col2row2), "[x y 2 20 1 10]");
      DoTest (string.Format ("#desc,z sort {0}", col3row2), "[x y z 2 20 200 1 10 100]");
      DoTest (string.Format ("#desc,y sort {0}", col3row3), "[x y z 3 30 300 2 20 200 1 10 100]");
      //DoTest (string.Format ("#desc,x sort {0}", null00), "[x y z 3l 30l 300l 2l 20l 200l -- 10l 100l]");
      //It should really be the way commented above this line.
      //Gotta fix that column ordering bug.
      DoTest (string.Format ("#desc,x sort {0}", null00), "[x y z 3 30 300 2 20 200 -- 10 100]");
      DoTest (string.Format ("#desc,y sort {0}", null11), "[x y z 3 30 300 1 10 100 2 -- 200]");
      DoTest (string.Format ("#desc,z sort {0}", null22), "[x y z 2 20 200 1 10 100 3 30 --]");
    }

    /* Please come back to this soon
    [Test]
    public void TestSortWithTimeline ()
    {
      string col1row1 = "[S|x #a 1]";
      string col2row1 = "[S|x y #a 1 10]";
      string col3row1 = "[S|x y z #a 1 10 100]";
      string col1row2 = "[S|x #a 1 #b 2]";
      string col2row2 = "[S|x y #a 1 10 #b 2 20]";
      string col3row2 = "[S|x y z #a 1 10 100 #b 2 20 200]";
      string col3row3 = "[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]";
      //string col1row3 = "[x 1l 2l 3l]";
      string null00 = "[S|x y z -- #a 10 100 #b 2 20 200 #c 3 30 300]";
      string null11 = "[S|x y z #a 1 10 100 #b 2 -- 200 #c 3 30 300]";
      string null22 = "[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 --]";
      
      DoTest (string.Format ("#desc,x sort {0}", col1row1), "[S|x #a 1]");
      DoTest (string.Format ("#desc,y sort {0}", col2row1), "[S|x y #a 1 10]");
      DoTest (string.Format ("#desc,z sort {0}", col3row1), "[S|x y z #a 1 10 100]");
      DoTest (string.Format ("#desc,x sort {0}", col1row2), "[S|x #b 2 #a 1]");
      DoTest (string.Format ("#desc,y sort {0}", col2row2), "[S|x y #b 2 20 #a 1 10]");
      DoTest (string.Format ("#desc,z sort {0}", col3row2), "[S|x y z #b 2 20 200 #a 1 10 100]");
      DoTest (string.Format ("#desc,y sort {0}", col3row3), "[S|x y z #c 3 30 300 #b 2 20 200 #a 1 10 100]");
      //DoTest (string.Format ("#desc,x sort {0}", null00), "[x y z 3l 30l 300l 2l 20l 200l -- 10l 100l]");
      //It should really be the way commented above this line.
      //Gotta fix that column ordering bug.
      DoTest (string.Format ("#desc,x sort {0}", null00), "[S|x y z #c 3 30 300 #b 2 20 200 #a -- 10 100]");
      DoTest (string.Format ("#desc,y sort {0}", null11), "[S|x y z #c 3 30 300 #a 1 10 100 #b 2 -- 200]");
      DoTest (string.Format ("#desc,z sort {0}", null22), "[S|x y z #b 2 20 200 #a 1 10 100 #c 3 30 --]");
    }
    */

    [Test]
    public void TestSortColumnsStayInOrder ()
    {
      string null00 = "[x y z -- 10 100 2 20 200 3 30 300]";
      DoTest (string.Format ("#desc,x sort {0}", null00), "[x y z 3 30 300 2 20 200 -- 10 100]");
    }

    [Test]
    public void TestSortWithNoTimelineButBigger ()
    {
      //[
      //   a   b    c     d      e
      //   1  10  100  1000     --
      //   2  20   --  2000     --
      //   3  30  300  3000     --
      //   4  --   --    --     --
      //   5  50  500    --  50000
      //]
      string t = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 -- 3 30 300 3000 -- 4 -- -- -- -- 5 50 500 -- 50000]";

      //[
      //   a   b    c     d      e
      //   5  50  500    --  50000
      //   4  --   --    --     --
      //   3  30  300  3000     --
      //   2  20   --  2000     --
      //   1  10  100  1000     --
      //]
      string tbya = "[a b c d e 5 50 500 -- 50000 4 -- -- -- -- 3 30 300 3000 -- 2 20 -- 2000 -- 1 10 100 1000 --]";

      //[
      //   a   b    c     d      e
      //   5  50  500    --  50000
      //   3  30  300  3000     --
      //   2  20   --  2000     --
      //   1  10  100  1000     --
      //   4  --   --    --     --
      //]
      string tbyb = "[a b c d e 5 50 500 -- 50000 3 30 300 3000 -- 2 20 -- 2000 -- 1 10 100 1000 -- 4 -- -- -- --]";

      //[
      //   a   b    c     d      e
      //   5  50  500    --  50000
      //   3  30  300  3000     --
      //   1  10  100  1000     --
      //   2  20   --  2000     --
      //   4  --   --    --     --
      //]
      string tbyc = "[a b c d e 5 50 500 -- 50000 3 30 300 3000 -- 1 10 100 1000 -- 2 20 -- 2000 -- 4 -- -- -- --]";

      //[
      //   a   b    c     d      e
      //   3  30  300  3000     --
      //   2  20   --  2000     --
      //   1  10  100  1000     --
      //   4  --   --    --     --
      //   5  50  500    --  50000
      //]
      string tbyd = "[a b c d e 3 30 300 3000 -- 2 20 -- 2000 -- 1 10 100 1000 -- 4 -- -- -- -- 5 50 500 -- 50000]";

      //[
      //   a   b    c     d      e
      //   5  50  500    --  50000
      //   3  30  300  3000     --
      //   2  20   --  2000     --
      //   1  10  100  1000     --
      //   4  --   --    --     --
      //]
      string tbye = "[a b c d e 5 50 500 -- 50000 1 10 100 1000 -- 2 20 -- 2000 -- 3 30 300 3000 -- 4 -- -- -- --]";

      DoTest (string.Format ("#desc,a sort {0}", t), tbya);
      DoTest (string.Format ("#desc,b sort {0}", t), tbyb);
      DoTest (string.Format ("#desc,c sort {0}", t), tbyc);
      DoTest (string.Format ("#desc,d sort {0}", t), tbyd);
      DoTest (string.Format ("#desc,e sort {0}", t), tbye);
    }

    [Test]
    public void SortVariousDataTypes ()
    {
      //[
      //   l   m     d      s     b     x  y
      //   1 10m 100.0 "1000" false \\x01 #a
      //   2 20m 200.0 "2000" false \\x02 #b
      //   3 30m 300.0 "3000" false \\x03 #c
      //   4 40m 400.0 "4000"  true \\x04 #d
      //   5 50m 500.0 "5000"  true \\x05 #e
      //]
      string t = "[l m d s b x y 1 10m 100.0 \"1000\" false \\x01 #a 2 20m 200.0 \"2000\" false \\x02 #b 3 30m 300.0 \"3000\" false \\x03 #c 4 40m 400.0 \"4000\" true \\x04 #d 5 50m 500.0 \"5000\" true \\x05 #e]";

      //[
      //   l   m     d      s     b     x  y
      //   5 50m 500.0 "5000"  true \\x05 #e
      //   4 40m 400.0 "4000"  true \\x04 #d
      //   3 30m 300.0 "3000" false \\x03 #c
      //   2 20m 200.0 "2000" false \\x02 #b
      //   1 10m 100.0 "1000" false \\x01 #a
      //]
      string tReversed = "[l m d s b x y 5 50m 500.0 \"5000\" true \\x05 #e 4 40m 400.0 \"4000\" true \\x04 #d 3 30m 300.0 \"3000\" false \\x03 #c 2 20m 200.0 \"2000\" false \\x02 #b 1 10m 100.0 \"1000\" false \\x01 #a]";
      //Sorting is stable so when sorting by b we expect row index 4 to be on top still.
      string tbyb = "[l m d s b x y 4 40m 400.0 \"4000\" true \\x04 #d 5 50m 500.0 \"5000\" true \\x05 #e 1 10m 100.0 \"1000\" false \\x01 #a 2 20m 200.0 \"2000\" false \\x02 #b 3 30m 300.0 \"3000\" false \\x03 #c]";

      DoTest (string.Format ("#desc,l sort {0}", t), tReversed);
      DoTest (string.Format ("#desc,m sort {0}", t), tReversed);
      DoTest (string.Format ("#desc,d sort {0}", t), tReversed);
      DoTest (string.Format ("#desc,s sort {0}", t), tReversed);
      DoTest (string.Format ("#desc,b sort {0}", t), tbyb);
      DoTest (string.Format ("#desc,x sort {0}", t), tReversed);
      DoTest (string.Format ("#desc,y sort {0}", t), tReversed);
    }

    [Test]
    public void TestSortByTime ()
    {
      DoTest ("#asc,t sort [t 0.03:00:00.0000000 0.02:00:00.0000000 0.01:00:00.0000000]", "[t 0.01:00:00.0000000 0.02:00:00.0000000 0.03:00:00.0000000]"); 
    }

    [Test]
    public void TestSortOutOfOrder ()
    {
      DoTest ("#desc,a sort [a 2 -1 4 0 3 1]", "[a 4 3 2 1 0 -1]");
    }

    [Test]
    public void TestRows ()
    {
      string t = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 20000 3 30 300 3000 -- 4 -- -- -- -- 5 50 500 -- 50000]";
      string t24 = "[a b c d e 3 30 300 3000 -- 4 -- -- -- -- 5 50 500 -- 50000]";
      DoTest (string.Format ("2 4 rows {0}", t), t24);
    }

    [Test]
    public void TestRowsWithInitialNulls ()
    {
      string t = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 20000 3 30 300 3000 -- 4 -- -- -- -- 5 50 500 -- 50000]";
      string t01 = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 20000]";
      DoTest (string.Format ("0 1 rows {0}", t), t01);
    }

    [Test]
    public void TestRowsColumnsWithOnlyNulls ()
    {
      string t = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 -- 3 30 300 3000 -- 4 -- -- -- -- 5 50 500 -- 50000]";
      string t01 = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 --]";
      DoTest (string.Format ("0 1 rows {0}", t), t01);
    }

    [Test]
    public void TestInitialValue ()
    {
      //The first value in a time series must be included in the output even if its value is equal to the inital value
      //for the corresponding data type.  Booleans are particularly problematic because they are equal to false by default.

      //Simultaneous initial values.
      DoTest (RCFormat.Default, "[E|S|x 0 #a -1 1 #a 0 2 #a 1] > [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 2 #a true]");

      //Inital value on left.
      DoTest (RCFormat.Default, "[E|S|x 0 #a -1 2 #a 0 3 #a 1] > [E|S|x 1 #a 1 2 #a 0 3 #a -1]", "[E|S|x 1 #a false 3 #a true]");

      //Initial value on right.
      DoTest (RCFormat.Default, "[E|S|x 1 #a -1 2 #a 0 3 #a 1] > [E|S|x 0 #a 1 2 #a 0 3 #a -1]", "[E|S|x 1 #a false 3 #a true]");
    }

    [Test]
    public void TestLatestTimestamp ()
    {
      //Make sure that the timestamp in the result is the one with the greatest value from the right and left.

      //In this example the value x changes at T==3l on the RIGHT side.
      DoTest (RCFormat.Default, "[E|S|x 0 #a true 2 #a false] or [E|S|x 0 #a true 1 #a false 2 #a true 3 #a false]", "[E|S|x 0 #a true 3 #a false]");

      //In this example the value x changes at T==3l on the LEFT side.
      DoTest (RCFormat.Default, "[E|S|x 0 #a true 1 #a false 2 #a true 3 #a false] or [E|S|x 0 #a true 2 #a false]", "[E|S|x 0 #a true 3 #a false]");
    }

    //We should do tests for duplicates as well...

    //Plus
    [Test]
    public void TestPlus ()
    {
      DoTest ("[S|x #a 10 #a 11 #a 12] + 1", "[S|x #a 11 #a 12 #a 13]");
      DoTest ("1 + [S|x #a 10 #a 11 #a 12]", "[S|x #a 11 #a 12 #a 13]");
      DoTest ("[E|S|x 0 #a 1 1 #a 2 2 #a 3] + [E|S|x 0 #a 10 1 #a 11 2 #a 12]", "[E|S|x 0 #a 11 1 #a 13 2 #a 15]");
      DoTest ("[S|x #a 1 #a 2 #a 3] + [S|x #a 10 #a 11 #a 12]", "[S|x #a 15]");
      DoTest ("[] + [S|x #a 1]", "[S|x #a 1]");
      DoTest ("[S|x #a 1] + []", "[S|x #a 1]");
      //add this test for the other arithmetic.
      DoTest ("{u:[a b 10 1 20 2 30 3] <-$u.a + $u.b}", "[x 11 22 33]");
    }

    [Test]
    public void TestMinus ()
    {
      DoTest ("[S|x #a 10 #a 11 #a 12] - 1", "[S|x #a 9 #a 10 #a 11]");
      DoTest ("1 - [S|x #a 10 #a 11 #a 12]", "[S|x #a -9 #a -10 #a -11]");
      DoTest ("[S|x #a 1 #a 2 #a 3] - [S|x #a 10 #a 11 #a 12]", "[S|x #a -9]");
      DoTest ("{u:[a b 10 1 20 2 30 3] <-$u.a - $u.b}", "[x 9 18 27]");
    }

    [Test]
    public void TestMultiply ()
    {
      DoTest ("[S|x #a 10 #a 11 #a 12] * 2", "[S|x #a 20 #a 22 #a 24]");
      DoTest ("2 * [S|x #a 10 #a 11 #a 12]", "[S|x #a 20 #a 22 #a 24]");
      DoTest ("[E|S|x 0 #a 1 1 #a 2 2 #a 3] * [E|S|x 0 #a 10 1 #a 11 2 #a 12]", "[E|S|x 0 #a 10 1 #a 22 2 #a 36]");
      DoTest ("[S|x #a 1 #a 2 #a 3] * [S|x #a 10 #a 11 #a 12]", "[S|x #a 36]");
      DoTest ("{u:[a b 10 1 20 2 30 3] <-$u.a * $u.b}", "[x 10 40 90]");
    }

    [Test]
    public void TestDivide ()
    {
      //Let's mix up the data types a little bit now.
      DoTest ("[S|x #a 10.0 #a 11.0 #a 12.0] / 2", "[S|x #a 5.0 #a 5.5 #a 6.0]");
      DoTest ("10 / [S|x #a 2.0 #a 4.0 #a 5.0]", "[S|x #a 5.0 #a 2.5 #a 2.0]");
      DoTest ("[S|x #a 10 #a 20 #a 30] / [S|x #a 10 #a 20 #a 30]", "[S|x #a 1]");
      DoTest ("{u:[a b 10 1 20 2 30 3] <-$u.a / $u.b}", "[x 10 10 10]");
    }

    [Test]
    public void TestSum ()
    {
      DoTest ("sum [S|x #a 10 #a 11 #a 12]", "[S|x # 33 #a 33]");
    }

    [Test]
    public void TestSum0 ()
    {
      DoTest ("sum [S|x #a 1 #b 2 #c 3]", "[S|x # 6 #a 1 #b 2 #c 3]");
    }
      
    [Test]
    public void TestSum1 ()
    {
      //This idea of levels applies to all the aggregators like sum, avg, high, low etc...
      DoTest ("1 sum [S|x #a,b,x 0 #a,b,y 1 #a,b,z 2]", "[S|x # 3]");
    } 

    [Test]
    public void TestSum2 ()
    {
      DoTest ("2 sum [S|x #a,b,x 0 #a,b,y 1 #a,b,z 2]", "[S|x # 3 #a 3]");
    }

    [Test]
    public void TestSum3 ()
    {
      DoTest ("3 sum [S|x #a,b,x 0 #a,b,y 1 #a,b,z 2]", "[S|x # 3 #a 3 #a,b 3]");
    }

    [Test]
    public void TestSum4 ()
    {
      DoTest ("-1 sum [S|x #a,b,x 0 #a,b,y 1 #a,b,z 2]", "[S|x #a,b,x 0 #a,b,y 1 #a,b,z 2]");
    }

    [Test]
    public void TestSum5 ()
    {
      DoTest ("-1 sum [S|x #x 0 #y 1 #x 2 #y 3 #x 4 #y 5]", "[S|x #x 6 #y 9]");
    }

    [Test]
    public void TestSum6 ()
    {
      DoTest ("-2 sum [S|x #a,x 0 #a,y 1 #a,x 2 #a,y 3 #a,x 4 #a,y 5]", "[S|x #a 15 #a,x 6 #a,y 9]");
    }

    [Test]
    public void TestSum7 ()
    {
      DoTest ("-3 sum [S|x #a,b,x 0 #a,b,y 1 #a,b,x 2 #a,b,y 3 #a,b,x 4 #a,b,y 5]",
              "[S|x #a 15 #a,b 15 #a,b,x 6 #a,b,y 9]");
    }

    [Test]
    public void TestSum8 ()
    {
      DoTest ("-4 sum [S|x #a,b,x 0 #a,b,y 1 #a,b,x 2 #a,b,y 3 #a,b,x 4 #a,b,y 5]", "[S|x # 15 #a 15 #a,b 15 #a,b,x 6 #a,b,y 9]");
    }

    [Test]
    public void TestAny ()
    {
      DoTest ("any [S|x #a false #a false #a true #a false #a false]", "[S|x # true #a true]");
      DoTest ("any [S|x #a false #a false #b true #b false #a false]", "[S|x # true #a false #b true]");
      DoTest ("any [S|x #a,x false #a,x false #b,x true #b,x false #a,x false]",
              "[S|x # true #a false #a,x false #b true #b,x true]");
    }

    [Test]
    public void TestAll ()
    {
      DoTest ("all [S|x #a false #a false #a true #a false #a false]", "[S|x # false #a false]");
      DoTest ("all [S|x #a false #a false #b true #b true #a false]", "[S|x # false #a false #b true]");
      DoTest ("all [S|x #a,x false #a,x false #b,x true #b,x true #a,x false]",
              "[S|x # false #a false #a,x false #b true #b,x true]");
    }

    [Test]
    public void TestNone ()
    {
      DoTest ("none [S|x #a false #a false #a true #a false #a false]", "[S|x # false #a false]");
      DoTest ("none [S|x #a false #a false #b true #b true #a false]", "[S|x # false #a true #b false]");
      DoTest ("none [S|x #a,x false #a,x false #b,x true #b,x true #a,x false]",
              "[S|x # false #a true #a,x true #b false #b,x false]");
    }

    [Test]
    public void TestAvg ()
    {
      DoTest ("avg [S|x #a 10 #a 20 #a 30]", "[S|x # 20.0 #a 20.0]");
    }

    [Test]
    public void TestAvg0 ()
    {
      DoTest ("avg [S|x #a 10 #b 20 #c 30]", "[S|x # 20.0 #a 10.0 #b 20.0 #c 30.0]");
    }

    [Test]
    public void TestHigh ()
    {
      DoTest ("high [S|x #a -5 #a -10 #a -15]", "[S|x # -5 #a -5]");
    }

    [Test]
    public void TestHigh1 ()
    {
      DoTest ("high [S|x #a -5 #b -10 #c -15]", "[S|x # -5 #a -5 #b -10 #c -15]");
    }

    [Test]
    public void TestHigh2 ()
    {
      DoTest ("high [E|S|x 0 #a -10 1 #a -5 2 #a -15 3 #a -10 4 #a -20]", "[E|S|x 0 #a -10 1 #a -5]");
    }

    [Test]
    public void TestHigh3 ()
    {
      DoTest ("high [S|x #a 1 #b 2 #c 3]", "[S|x # 3 #a 1 #b 2 #c 3]");
    }

    [Test]
    public void TestLow ()
    {
      DoTest ("low [S|x #a 20 #a 10 #a 15 #a 5 #a 5]", "[S|x # 5 #a 5]");
    }

    [Test]
    public void TestLow1 ()
    {
      DoTest ("low [S|x #a 20 #b 10 #c 15]", "[S|x # 10 #a 20 #b 10 #c 15]");
    }

    [Test]
    public void TestLow2 ()
    {
      DoTest ("low [E|S|x 0 #a 20 1 #a 10 2 #a 15 3 #a 5 4 #a 5]", "[E|S|x 0 #a 20 1 #a 10 3 #a 5]");
    }

    [Test]
    public void TestLow4 ()
    {
      DoTest ("low [S|x #a 1 #b 2 #c 3]", "[S|x # 1 #a 1 #b 2 #c 3]");
    }

    [Test]
    public void TestString ()
    {
      DoTest ("string [S|x #a 0 #a 1 #a 2]", "[S|x #a \"0\" #a \"1\" #a \"2\"]");
    }

    [Test]
    public void TestSymbol ()
    {
      DoTest ("symbol [S|x #a \"a\" #a \"b\" #a \"c\"]", "[S|x #a #a #a #b #a #c]");
    }

    [Test]
    public void TestLong ()
    {
      DoTest ("long [S|x #a 0.0 #a 1.0 #a 2.0]", "[S|x #a 0 #a 1 #a 2]");
    }

    [Test]
    public void TestDouble ()
    {
      DoTest ("double [S|x #a 0 #a 1 #a 2]", "[S|x #a 0.0 #a 1.0 #a 2.0]");
    }

    [Test]
    public void TestBoolean ()
    {
      DoTest ("boolean [S|x #a 0 #a 1 #a 2]", "[S|x #a false #a true]");
    }

    [Test]
    public void TestDecimal ()
    {
      DoTest ("decimal [S|x #a \\x00 #a \\x01 #a \\x02]", "[S|x #a 0m #a 1m #a 2m]");
    }

    [Test]
    public void TestByte ()
    {
      DoTest ("byte [S|x #a 0 #a 1 #a 2]", "[S|x #a \\x00 #a \\x01 #a \\x02]");
    }

    [Test]
    public void TestAnd ()
    {
      DoTest ("[E|S|x 0 #a true 1 #a true 2 #a false 3 #a false] and [E|S|x 0 #a true 1 #a false 2 #a true 3 #a false]", "[E|S|x 0 #a true 1 #a false]");
      DoTest ("[E|S|x 0 #a true 1 #a true 2 #a false 3 #a false] and true", "[E|S|x 0 #a true 2 #a false]");
      DoTest ("true and [E|S|x 0 #a true 1 #a true 2 #a false 3 #a false]", "[E|S|x 0 #a true 2 #a false]");
    }

    [Test]
    public void TestOr ()
    {
      DoTest ("[E|S|x 0 #a true 2 #a false] or [E|S|x 0 #a true 1 #a false 2 #a true 3 #a false]", "[E|S|x 0 #a true 3 #a false]");
      DoTest ("[E|S|x 0 #a true 1 #a true 2 #a false 3 #a false] or false", "[E|S|x 0 #a true 2 #a false]");
      DoTest ("false or [E|S|x 0 #a true 1 #a true 2 #a false 3 #a false]", "[E|S|x 0 #a true 2 #a false]");
    }

    [Test]
    public void TestNot ()
    {
      DoTest ("not [S|x #a true #a false]", "[S|x #a false #a true]");
    }

    [Test]
    public void TestGreaterThan ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] > [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 2 #a true]");
      DoTest ("0 > [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 2 #a true]");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] > 0", "[E|S|x 0 #a true 1 #a false]");
    }

    [Test]
    public void TestGreaterThanOrEqual ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] >= [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 1 #a true]");
      DoTest ("0 >= [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 1 #a true]");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] >= 0", "[E|S|x 0 #a true 2 #a false]");
    }

    [Test]
    public void TestLessThan ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] < [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a true 1 #a false]");
      DoTest ("0 < [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a true 1 #a false]");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] < 0", "[E|S|x 0 #a false 2 #a true]");
    }

    [Test]
    public void TestLessThanOrEqual ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] <= [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a true 2 #a false]");
      DoTest ("0 <= [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a true 2 #a false]");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] <= 0", "[E|S|x 0 #a false 1 #a true]");
    }

    [Test]
    public void TestVectorEquals ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] == [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 1 #a true 2 #a false]");
      DoTest ("0 == [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a false 1 #a true 2 #a false]");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] == 0", "[E|S|x 0 #a false 1 #a true 2 #a false]");
    }

    [Test]
    public void TestVectorNotEquals ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] != [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a true 1 #a false 2 #a true]");
      DoTest ("0 != [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "[E|S|x 0 #a true 1 #a false 2 #a true]");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] != 0", "[E|S|x 0 #a true 1 #a false 2 #a true]");
    }

    [Test]
    public void TestEquals ()
    {
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] = [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "false");
      DoTest ("[E|S|x 0 #a -1 1 #a 0 2 #a 1] = [E|S|x 0 #a -1 1 #a 0 2 #a 1]", "true");
      DoTest ("0 = [E|S|x 0 #a 1 1 #a 0 2 #a -1]", "false");
      DoTest ("[E|S|x 0 #a 1 1 #a 0 2 #a -1] = 0", "false");
    }

    [Test]
    public void TestAbs ()
    {
      DoTest ("abs [S|x #a -1 #b 2 #c -3]", "[S|x #a 1 #b 2 #c 3]");
      DoTest ("abs [S|x #a -1.0 #b 2.0 #c -3.0]", "[S|x #a 1.0 #b 2.0 #c 3.0]");
      DoTest ("abs [S|x #a -1m #b 2m #c -3m]", "[S|x #a 1m #b 2m #c 3m]");
    }

    [Test]
    public void TestCount ()
    {
      DoTest ("count [S|x #a 0 #b 10 #c 100]", "3");
      DoTest ("count [S|x #a 0 #a 1]", "2");
      DoTest ("count []", "0");
    }

    [Test]
    public void TestLength0 ()
    {
      DoTest ("length [S|y #a \"xyz\"]", "[S|x #a 3]");
    }

    [Test]
    [Ignore ("because")]
    public void TestLength1 ()
    {
      DoTest ("length [y \"xyz\" \"xy\"]", "[x 3 2]");
    }

    [Test]
    public void TestGCol ()
    {
      DoTest ("gcol [G S|x 0 #a 0 1 #a 1 2 #a 2]", "0 1 2");
    }

    [Test]
    public void TestECol ()
    {
      DoTest ("ecol [T E S|x 2017.08.24 0 #a 0 2017.08.25 1 #a 1 2017.08.26 2 #a 2]", "0 1 2");
    }

    [Test]
    public void TestTCol ()
    {
      DoTest ("tcol [T S|x 2017.08.24 #a 1 2017.08.25 #a 2 2017.08.26 #a 3]", "2017.08.24 2017.08.25 2017.08.26");
    }

    [Test]
    public void TestSCol ()
    {
      DoTest ("scol [S|x #a 0 #a 1 #a 2]", "#a #a #a");
    }

    [Test]
    public void TestColofl0 ()
    {
      DoTest ("colofl [S|x #a 0 #a 1 #a 2]", "0 1 2");
    }

    [Test]
    public void TestColofl1 ()
    {
      DoTest ("sum colofl [S|x #a 0 #a 1 #a 2]", "3");
    }

    [Test]
    public void TestColofd0 ()
    {
      DoTest ("colofd []", "~d");
    }

    [Test]
    public void TestColofd1 ()
    {
      DoTest ("colofd [x 0.0 1.0 2.0]", "0.0 1.0 2.0");
    }

    [Test]
    public void TestColofd2 ()
    {
      DoTest ("colofd [S|x #a 0.0 #b 1.0 #c 2.0]", "0.0 1.0 2.0");
    }

    [Test]
    public void TestColofd3 ()
    {
      DoTest ("NaN colofd [S|x #a 0.0 #b -- #c 2.0]", "0.0 NaN 2.0");
    }

    [Test]
    public void TestColofd4 ()
    {
      DoTest ("0.0 colofd [S|x #a -- #b 1.0 #c 2.0]", "0.0 1.0 2.0");
    }

    [Test]
    public void TestColofl ()
    {
      DoTest ("colofl []", "~l");
      DoTest ("colofl [x 0 1 2]", "0 1 2");
      DoTest ("colofl [S|x #a 0 #b 1 #c 2]", "0 1 2");
      DoTest ("0 colofl [S|x #a -- #b 1 #c 2]", "0 1 2");
    }

    [Test]
    public void TestColofm ()
    {
      DoTest ("colofm []", "~m");
      DoTest ("colofm [x 0m 1m 2m]", "0 1 2m");
      DoTest ("colofm [S|x #a 0m #b 1m #c 2m]", "0 1 2m");
      DoTest ("0m colofm [S|x #a -- #b 1m #c 2m]", "0 1 2m");
    }

    [Test]
    public void TestColofx ()
    {
      DoTest ("colofx []", "~x");
      DoTest ("colofx [x \\x00 \\x01 \\x02]", "\\x00 \\x01 \\x02");
      DoTest ("colofx [S|x #a \\x00 #b \\x01 #c \\x02]", "\\x00 \\x01 \\x02");
      DoTest ("\\x00 colofx [S|x #a -- #b \\x01 #c \\x02]", "\\x00 \\x01 \\x02");
    }

    [Test]
    public void TestColofb ()
    {
      DoTest ("colofb []", "~b");
      DoTest ("colofb [x false true]", "false true");
      DoTest ("colofb [S|x #a true #b false #c true]", "true false true");
      DoTest ("false colofb [S|x #a -- #b false #c true]", "false false true");
    }

    [Test]
    public void TestColofs ()
    {
      DoTest ("colofs []", "~s");
      DoTest ("colofs [x \"0\" \"1\" \"2\"]", "\"0\" \"1\" \"2\"");
      DoTest ("colofs [S|x #a \"0\" #b \"1\" #c \"2\"]", "\"0\" \"1\" \"2\"");
      DoTest ("\"0\" colofs [S|x #a -- #b \"1\" #c \"2\"]", "\"0\" \"1\" \"2\"");
    }

    [Test]
    public void TestColofsAfterJoin ()
    {
      DoTest ("{left:[k \"a\" \"b\" \"c\"] right:[k v \"c\" #s \"b\" #r \"a\" #q] result:$left join $right <-colofs $result.k}", "\"a\" \"b\" \"c\"");
    }

    /*
    parse_machine_test:{
      left:[
        key
        "a"
        "b"
        "c"
        "d"
        "e"
      ]
      right:[
        key value
        "e" #q
        "d" #r
        "c" #s
        "b" #t
        "a" #u
      ]
      result:$left join $right
      :(colofs $result.key) assert "a" "b" "c" "d" "e"
    }
    */

    [Test]
    public void TestColofy ()
    {
      DoTest ("colofy []", "~y");
      DoTest ("colofy [x #0 #1 #2]", "#0 #1 #2");
      DoTest ("colofy [S|x #a #0 #b #1 #c #2]", "#0 #1 #2");
      DoTest ("#0 colofy [S|x #a -- #b #1 #c #2]", "#0 #1 #2");
    }

    [Test]
    public void TestColoft ()
    {
      DoTest ("coloft []", "~t");
      DoTest ("coloft [x 08:00 09:00 10:00]", "08:00 09:00 10:00");
      DoTest ("coloft [S|x #a 08:00 #b 09:00 #c 10:00]", "08:00 09:00 10:00");
      DoTest ("08:00 coloft [S|x #a -- #b 09:00 #c 10:00]", "08:00 09:00 10:00");
    }
      
    [Test]
    public void TestColofError ()
    {
      DoTest ("#status from try {<-colofl [S|x #a -- #b 1 #c 2]}", "{status:1}");
      DoTest ("#status from try {<-colofm [S|x #a -- #b 1m #c 2m]}", "{status:1}");
      DoTest ("#status from try {<-colofd [S|x #a -- #b 1.0 #c 2.0]}", "{status:1}");
      DoTest ("#status from try {<-colofx [S|x #a -- #b \\x01 #c \\x02]}", "{status:1}");
      DoTest ("#status from try {<-colofb [S|x #a -- #b false #c true]}", "{status:1}");
      DoTest ("#status from try {<-colofs [S|x #a -- #b \"1\" #c \"2\"]}", "{status:1}");
      DoTest ("#status from try {<-colofy [S|x #a -- #b #1 #c #2]}", "{status:1}");
    }

    [Test]
    public void TestSwitch ()
    {
      DoTest ("[S|x #a false] switch {:#it,was,true :#it,was,false}", "#it,was,false");
      DoTest ("[S|x #a true] switch {:#it,was,true :#it,was,false}", "#it,was,true");
    }

    [Test]
    public void TestMin ()
    {
      DoTest ("0.0 min [S|x #0 1.0]", "[S|x #0 0.0]");
      DoTest ("[S|x #0 1.0] min 0.0", "[S|x #0 0.0]");
    }

    [Test]
    public void TestMax ()
    {
      DoTest ("0.0 max [S|x #0 1.0]", "[S|x #0 1.0]");
      DoTest ("[S|x #0 1.0] max 0.0", "[S|x #0 1.0]");
    }

    [Test]
    public void TestWriteRead ()
    {
      //write should be able to accept a cube on the left.
      //read should be able to do the same.
      //This test is to ensure the overload works, not the semantics of the write operator.
      DoTest (RCFormat.DefaultNoT, "{:[S|x #ignore #a] write {i:++} <-[S|x #ignore #a] read 0}", "[S|i #a 0]");
      DoTest (RCFormat.DefaultNoT, "{:[S|x #ignore #a] write {i:++} <-[S|x #ignore #a] read [S|x #ignore 0]}", "[S|i #a 0]");
    }

    [Test]
    public void TestWriteDispatch ()
    {
      DoTest (RCFormat.DefaultNoT, "{:[S|x #ignore #a] write {i:++} <-[S|x #ignore #a] dispatch 1}", "[S|i #a 0]");
      DoTest (RCFormat.DefaultNoT, "{:[S|x #ignore #a] write {i:++} <-[S|x #ignore #a] dispatch [S|x #ignore 1]}", "[S|i #a 0]");
    }

    [Test]
    public void TestWriteGawk ()
    {
      DoTest (RCFormat.DefaultNoT, "{:[S|x #ignore #a] write {i:++} <-[S|x #ignore #a] gawk 1}", "[S|i #a 0]");
      DoTest (RCFormat.DefaultNoT, "{:[S|x #ignore #a] write {i:++} <-[S|x #ignore #a] gawk [S|x #ignore 1]}", "[S|i #a 0]");
    }

    [Test]
    public void TestPeek ()
    {
      //read should be able to do the same.
      //This test is to ensure the overload works, not the semantics of the operators.
      DoTest ("[S|x #ignore #a] peek 1", "false");
      DoTest ("[S|x #ignore #a] peek [S|x #ignore 1]", "false");
    }

    [Test]
    public void TestThrottle ()
    {
      DoTest ("[S|x #ignore #a] throttle 1", "0");
      DoTest ("[S|x #ignore #a] throttle [S|x #ignore 1]", "0");
    }

    [Test]
    public void TestPoll ()
    {
      DoTest ("[S|x #ignore #a] poll 1", "[]");
      DoTest ("[S|x #ignore #a] poll [S|x #ignore 1]", "[]");
    }

    [Test]
    public void TestSwitchOnEmptyCube ()
    {
      DoTest ("#status from try {<-[] switch {:#foo :#bar}}", "{status:1}");
    }

    [Test]
    [Ignore ("because")]
    public void TestRepeat ()
    {
      //I don't know what repeat should do for cubes...
      //What should happen to the T column?
      //Should it be incremented so that it goes at the end?
      //As a musician, not having the ability to repeat a sequence of events in time is an issue.
      //But as a musician, I would need to control how frequently the sequence repeats.
      //DoTest (RCFormat.Default, "repeat
    }

    [Test]
    public void TestPart ()
    {
      DoTest (RCFormat.Default,
              "1 part [E|S|x 0 #a #h,i,j 0 #b #0,1,2 0 #c #0.0,1.0,2.0]",
              "[E|S|x 0 #a #i 0 #b #1 0 #c #1.0]");
    }

    [Test]
    public void TestFormatPretty ()
    {
      DoTest (RCFormat.Default,
              "#pretty format [E|S|a_long_name 0 #x 100.0]",
              "\"[\\n   E|S |a_long_name\\n   0 #x       100.0\\n]\"");
    }

    [Test]
    public void TestFormatHtml ()
    {
      DoTest (RCFormat.Default, "#html format [E|S|col0 col1 col2 0 #x 100 101.0 102m 1 #y 200 201.0 202m 2 #z 300 301.0 302m]",
        "\"<table>\\n  <thead><tr><th id='c0' class='num ch'>E</th><th id='c1' class='txt ch'>S</th><th id='c2' class='num ch'>col0</th><th id='c3' class='num ch'>col1</th><th id='c4' class='num ch'>col2</th></tr></thead>\\n  <tr><th id='r0_c0' class='num rh'>0</th><th id='r0_c1' class='txt rh'>#x</th><td id='r0_c2' class='num'>100</td><td id='r0_c3' class='num'>101.0</td><td id='r0_c4' class='num'>102m</td></tr>\\n  <tr><th id='r1_c0' class='num rh'>1</th><th id='r1_c1' class='txt rh'>#y</th><td id='r1_c2' class='num'>200</td><td id='r1_c3' class='num'>201.0</td><td id='r1_c4' class='num'>202m</td></tr>\\n  <tr><th id='r2_c0' class='num rh'>2</th><th id='r2_c1' class='txt rh'>#z</th><td id='r2_c2' class='num'>300</td><td id='r2_c3' class='num'>301.0</td><td id='r2_c4' class='num'>302m</td></tr>\\n</table>\\n\"");
    }

    [Test]
    public void TestFormatHtmlWithG ()
    {
      DoTest (RCFormat.Default, "{:write [E|S|col0 col1 col2 0 #x 100 101.0 102m] <-#html format #x read 0}",
        "\"<table>\\n  <thead><tr><th id='c0' class='num ch'>G</th><th id='c1' class='num ch'>E</th><th id='c2' class='txt ch'>S</th><th id='c3' class='num ch'>col0</th><th id='c4' class='num ch'>col1</th><th id='c5' class='num ch'>col2</th></tr></thead>\\n  <tr><th id='r0_c0' class='num rh'>0</th><th id='r0_c1' class='num rh'>0</th><th id='r0_c2' class='txt rh'>#x</th><td id='r0_c3' class='num'>100</td><td id='r0_c4' class='num'>101.0</td><td id='r0_c5' class='num'>102m</td></tr>\\n</table>\\n\"");
    }

    [Test]
    public void TestFormatCsv ()
    {
      DoTest (RCFormat.Default, "#csv format [x y z 1 10 100 2 20 200 3 30 300]", "\"x,y,z\\n1,10,100\\n2,20,200\\n3,30,300\\n\"");
    }

    [Test]
    public void TestFormatCsvEscapeStringAndSymbols ()
    {
      DoTest (RCFormat.Default, "#csv format [x y #s,1 \",\" #s,2 \"\\\"\" #s,3 \"\n\"]", "\"x,y\\n\\\"s,1\\\",\\\",\\\"\\n\\\"s,2\\\",\\\"\\\"\\\"\\\"\\n\\\"s,3\\\",\\\"\\n\\\"\\n\"");
    }

    [Test]
    public void TestFormatCsvEscapeDoubleQuote ()
    {
      //[
      //                          x                                  y
      //  "This is a normal string" "This is a string with \"quotes\""
      //]
      //x,y
      //This is a normal string,This is a string with ""quotes""
      DoTest (RCFormat.Default, "#csv format [x y \"This is a normal string\" \"This is a \\\"string\\\" with quotes\"]",
              "\"x,y\\nThis is a normal string,\\\"This is a \\\"\\\"string\\\"\\\" with quotes\\\"\\n\"");
    }

    [Test]
    public void TestFormatCsvWithBoolean ()
    {
      DoTest (RCFormat.Default, "#csv format [x true]", "\"x\\ntrue\\n\"");
    }

    //To - does not apply to cubes at all

    //Should at and from use indexes or timestamps?
    //For vectors it uses indexes so I think cubes should work the same way.
    //It will use indexes in the timeline though, not in the vector.
    //That's a problem because the timeline could contain values that are not visible at all in the cube...
    //I think I will have to use a visitor for it.
    //Let's hold off on these two for a while.
    //At and From
    [Test]
    public void TestAt ()
    {
      DoTest ("[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300] at #z #x", "[S|z x #a 100 1 #b 200 2 #c 300 3]");
    }

    [Test]
    public void TestFromYU ()
    {
      DoTest ("#z #x from [S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]", "[S|z x #a 100 1 #b 200 2 #c 300 3]");
    }

    [Test]
    public void TestFromSU ()
    {
      DoTest ("\"z\" \"x\" from [S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]", "[S|z x #a 100 1 #b 200 2 #c 300 3]");
    }

    [Test]
    public void TestFromYUEmpty ()
    {
      DoTest ("#z #x from []", "[]");
    }

    [Test]
    public void TestFromSUEmpty ()
    {
      DoTest ("\"z\" \"x\" from []", "[]");
    }

    [Test]
    public void TestAtYUEmpty ()
    {
      DoTest ("[] at #z #x", "[]");
    }

    [Test]
    public void TestAtSUEmpty ()
    {
      DoTest ("[] at \"z\" \"x\"", "[]");
    }

    [Test]
    [Ignore ("because")]
    public void TestFrom1 ()
    {
      //This cannot be because right because from is supposed to return a cube.
      //You will have to use the dot notation to access the timeline arrays.
      DoTest ("#G from [G|E|T|S|x 1 2 08:00 #a 3]", "1");
      DoTest ("#E from [G|E|T|S|x 1 2 08:00 #a 3]", "2");
      DoTest ("#T from [G|E|T|S|x 1 2 08:00 #a 3]", "08:00");
      DoTest ("#S from [G|E|T|S|x 1 2 08:00 #a 3]", "#a");
    }

    //Find - what I want to do with find is return the indices that you would use with at or from.
    //Again you probably need a visitor.
    [Test]
    [Ignore ("because")]
    public void TestFind () {}

    //Sort - cubes are always and forever sorted by time so there is no need of a sort routine.

    //Rank - rank however will be very important.
    //also xrank, which will be the version that works across symbols rather than within them.
    [Test]
    [Ignore ("because")]
    public void TestRank ()
    {
      DoTest (RCFormat.Default,
              "#asc rank [T|S|x 0 #a 30 1 #a 10 2 #a 20]",
              "[T|S|x 0 #a 1 1 #a 2 2 #a 0]");
      //We should also do a test with multiple symbols.  I've got a feeling...
      //Yea, I just don't know how to do this for cubes yet.
      //But making it work will be awesome.
    }

    //Random - not relevant for cubes, at least right now.
    //Shuffle - not relevant for now.
    //Unique - not relevant for now.

    //Sleep - not relevant ever.
    //Switch - not relevant ever.
    //Each - not relevant ever.
    //Print - not relevant ever.

    [Test]
    public void TestMap ()
    {
      DoTest (RCFormat.Default,
              "1 10 2 20 map [E|S|x 0 #a 1 1 #b 1 2 #a 2 3 #b 2 4 #a 1 5 #b 3]",
              "[E|S|x 0 #a 10 1 #b 10 2 #a 20 3 #b 20 4 #a 10 5 #b 3]");
    }

    //Delimit - not relevant.

    //Split - this doesn't map that easily because it produces two or more columns of output rather than one.
    //That isn't necessarily a showstopper and will probably be needed at some point.
    //But it can wait.
    [Test]
    [Ignore ("because")]
    public void TestSplit () {}

    [Test]
    public void TestReplace ()
    {
      DoTest ("\"hi\" \"bye\" replace [S|x #a \"hi\" #b \"hello good hi\"]", "[S|x #a \"bye\" #b \"hello good bye\"]");
    }

    [Test]
    public void TestUpper ()
    {
      DoTest ("upper [S|x #a \"abcdefg\" #b \"hijklmnop\" #c \"qrstuv\"]", "[S|x #a \"ABCDEFG\" #b \"HIJKLMNOP\" #c \"QRSTUV\"]");
    }

    [Test]
    public void TestLower ()
    {
      DoTest ("lower [S|x #a \"ABC\" #b \"DEF\" #c \"GHI\"]", "[S|x #a \"abc\" #b \"def\" #c \"ghi\"]");
    }

    [Test]
    [Ignore ("We need to make these operators work on cubes without time.")]
    public void TestLower1 ()
    {
      DoTest ("lower [x \"ABC\" \"DEF\" \"GHI\"]", "[x \"abc\" \"def\" \"ghi\"]");
    }

    //Names
    [Test]
    public void TestNames ()
    {
      DoTest ("names [S|a b c #x 1 2 3]", "\"a\" \"b\" \"c\"");
    }

    [Test]
    public void TestTime ()
    {
      //[
      //   G E T S |a
      //   0 0 0 #x 1
      //]
      DoTest ("untimeline [G E T S|a 0 0 00:00 #x 1]", "[G E T S a 0 0 00:00 #x 1]");
      //DoTest ("timeline [G E T S a 0 0 0 #x 1]", "[G E T S|a 0 0 0 #x 1]");
    }

    [Test]
    public void TestBlock ()
    {
      string cube = "[a b c d e 1 10 100 1000 -- 2 20 -- 2000 -- 3 30 300 3000 -- 4 -- -- -- -- 5 50 500 -- 50000]";
      string block = "{:{a:1 b:10 c:100 d:1000} :{a:2 b:20 d:2000} :{a:3 b:30 c:300 d:3000} :{a:4} :{a:5 b:50 c:500 e:50000}}";
      DoTest ("block " + cube, block);
    }

    //Rename
    [Test]
    [Ignore ("because")]
    public void TestRename ()
    {
      //ideally name/rename should use symbols because names in strings
      //are not necessarily legal identifiers.  But symbols are.
      //It is possible to create a symbol with an illegal name by coercing a string however.
      //So it's not exactly bulletproof yet.
      DoTest ("\"d\" \"e\" \"f\" rename [S|a b c #x 1 2 3]", "[S|d e f #x 1 2 3]");
    }

    [Test]
    public void TestBang0 ()
    {
      //Empty cube on the right.
      DoTest ("[S|x #a 0 #b 1] ! []", "[S|x #a 0 #b 1]");
      //Empty cube on the left.
      DoTest ("[] ! [S|x #a 0 #b 1]", "[S|x #a 0 #b 1]");
      //Simple update scenario.
      DoTest ("[S|x #a 0 #b 10] ! [S|x #b 11]", "[S|x #a 0 #b 11]");
      //Simple insert scenario.
      DoTest ("[S|x #a 0] ! [S|x #b 11]", "[S|x #a 0 #b 11]");
      //New columns added with overlap
      DoTest ("[S|x #a 1 #b 2] ! [S|x y #a 2 10 #b 3 20]", "[S|x y #a 2 10 #b 3 20]");
      //New columns have been added, no overlap
      DoTest ("[S|x #a 1 #b 2] ! [S|y #a 10 #b 20]", "[S|x y #a 1 10 #b 2 20]");
      //New columns added, no overlap, null in right
      DoTest ("[S|x #a 1 #b 2] ! [S|y #a 10]", "[S|x y #a 1 10 #b 2 --]");
      //Test incr
      DoTest ("[S|x #a 1 #b 2] ! [S|i #a ++]", "[S|x i #a 1 0 #b 2 --]");
      DoTest ("[S|x #a 1 #b 2] ! [S|x #b ++]", "[S|x #a 1 #b 3]");
    }

    [Test]
    public void TestBang1 ()
    {
      //empty cube bang
      DoTest ("!{}", "[]");
      DoTest ("!{u0:[S|x #a 1]}", "[S|u0 #a 1]");
      //vertical extension
      DoTest ("!{:[S|x #a 1] :[S|x #b 2] :[S|x #c 3]}", "[S|x #a 1 #b 2 #c 3]");
      //horizontal extension
      DoTest ("!{x:[S|x #a 1 #b 2 #c 3] y:[S|x #a 10 #b 20 #c 30] z:[S|x #a 100 #b 200 #c 300]}", "[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]");
      //horizontal extension, scalar
      DoTest ("!{x:[S|x #a 1 #b 2 #c 3] y:13}", "[S|x y #a 1 13 #b 2 13 #c 3 13]");
      //horizontal extension, vector
      DoTest ("!{x:[S|x #a 1 #b 2 #c 3] y:11 12 13}", "[S|x y #a 1 11 #b 2 12 #c 3 13]");
      //horizontal extension, unordered
      DoTest ("!{x:[S|x #a 1 #b 2 #c 3] y:[S|x #b 20 #a 10 #c 30] z:[S|x #c 300 #b 200 #a 100]}", "[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]");
      //horizontal extension, shared source.
      DoTest ("{u:[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300] <-!eval {x:$u.x y:$u.y z:$u.z}}", "[S|x y z #a 1 10 100 #b 2 20 200 #c 3 30 300]");
      //update, unordered
      DoTest ("!{:[S|x #a 1 #b 2 #c 3] :[S|x #c 30 #a 10]}", "[S|x #a 10 #b 2 #c 30]");
      //update filling nulls - notice the center square is updated
      DoTest ("!{u0:[S|x y z #a -- 1 -- #b 3 4 5 #c -- 7 --] u1:[S|x y z #a 0 -- 2 #b -- 9 -- #c 6 -- 8]}", "[S|x y z #a 0 1 2 #b 3 9 5 #c 6 7 8]");
      //apply bang multiple times
      DoTest ("{u:[S|x #a 1 #b 2 #c 3] u:$u ! eval {y:$u.x + 1} u:$u ! eval {z:$u.y + 1} <-$u}", "[S|x y z #a 1 2 3 #b 2 3 4 #c 3 4 5]");
      //test incrementing a variable
      DoTest ("[S|x #a 11] ! {:[S|x #a ++] :[S|x #a ++]}", "[S|x #a 13]");
      //DoTest ("!{:[S|x #a 11 #b 101 #c 1001] :[S|x #b ++] :[S|x #a ++]}", "[S|x #a 12 #b 102 #c 1001]");
      //DoTest ("!{:[S|x #a 11] :[S|x #a ++] :[S|x #a ++]}", "[S|x #a 13]");
    }

    [Test]
    public void TestBang2 ()
    {
      DoTest ("{u0:[S|x y #a 0 0 #b 1 1 #c 2 2 #d 3 3 #e 4 4 #f 5 -- #g 6 5 #h 7 --] u1:$u0 ! eval {y:0 to (count $u0) - 1} <-$u1}",
              "[S|x y #a 0 0 #b 1 1 #c 2 2 #d 3 3 #e 4 4 #f 5 5 #g 6 6 #h 7 7]");
    }

    [Test]
    public void TestBang3 ()
    {
      DoTest ("{u0:[S|x y #b 1 1 #c 2 2 #d 3 3 #e 4 4 #f 5 -- #g 6 5 #h 7 --] u1:$u0 ! eval {y:0 to (count $u0) - 1l} <-$u1}",
              "[S|x y #b 1 0 #c 2 1 #d 3 2 #e 4 3 #f 5 4 #g 6 5 #h 7 6]");
    }

    [Test]
    public void TestBang4 ()
    {
      DoTest ("{u0:[S|x y #a 0 0 #b 1 1 #c 2 2] u1:$u0 ! eval {y:13} <-$u1}",
              "[S|x y #a 0 13 #b 1 13 #c 2 13]");
    }

    [Test]
    public void TestBang5 ()
    {
      //Duplicates on the right side.
      DoTest ("count [S|x y z #a 1 10 100] ! #b #b cube eval {x:2 2 y:20 20 z:200 200}", "2");
    }

    [Test]
    public void TestBang6 ()
    {
      //"[S|type varname varcount varrev #canvas,meta,3,0,0,0,diner \"M\" #3,0,0,0,diner 1 0] ! {varrow:0 removed:false}"
      DoTest ("[S|x #a 1] ! {y:10 z:100}", "[S|x y z #a 1 10 100]");
    }

    [Test]
    public void TestBang7 ()
    {
      DoTest ("[S|y #a -1 #b 1 #c 1 #d -1] ! {y:[S|x #a -2 #b 2 #c 2 #d -2]}", "[S|y #a -2 #b 2 #c 2 #d -2]");
    }

    [Test]
    public void TestExcept ()
    {
      DoTest ("[S|x #a 1] except [S|x #b 2 #c 3]", "[S|x #a 1]");
    }

    [Test]
    public void TestExcept1 ()
    {
      DoTest ("[S|x #a 1] except [S|x #a 10 #b 2 #c 3]", "[]");
    }

    [Test]
    public void TestExcept2 ()
    {
      DoTest ("[S|x #a 1 #b 2 #c 3] except [S|x #a 1]", "[S|x #b 2 #c 3]");
    }

    [Test]
    public void TestInter ()
    {
      DoTest ("[S|x #a 1] inter [S|x #b 2 #c 3]", "[]");
    }

    [Test]
    public void TestInter1 ()
    {
      DoTest ("[S|x #a 1] inter [S|x #a 10 #b 2 #c 3]", "[S|x #a 10]");
    }

    [Test]
    public void TestInter2 ()
    {
      DoTest ("[S|x #a 1 #b 2 #c 3] inter [S|x #a 10]", "[S|x #a 10]");
    }

    [Test]
    public void TestWhere ()
    {
      DoTest ("{u:[S|x #a -1 #b 0 #c 1] <-$u.x where $u.x > 0}", "[S|x #c 1]");
    }

    [Test]
    public void TestWhere1 ()
    {
      //It doesn't work yet for cubes without timelines.
      DoTest ("{u:[s x #a -1 #b 0 #c 1] <-$u where $u.x > 0}", "[s x #c 1]");
    }

    [Test]
    public void TestWhere2 ()
    {
      //It doesn't work yet for cubes without timelines.
      DoTest ("{u:[s x #a -1 #b 0 #c 1] <-$u where colofb $u.x > 0}", "[s x #c 1]");
    }

    [Test]
    public void TestWhere3 ()
    {
      DoTest ("{u:[S|x #a 0 #b 1 #c 2] <-$u where $u.S == #b}", "[S|x #b 1]");
    }

    [Test]
    public void TestWhere4 ()
    {
      DoTest ("{u:[E|S|x 0 #a 0 0 #b 10 0 #c 100 1 #a 1 1 #b 11 1 #c 101 2 #a 2 2 #b 12 2 #c 202] <-$u where $u.S == #b}", "[E|S|x 0 #b 10 1 #b 11 2 #b 12]");
    }

    [Test]
    public void TestWhere5 ()
    {
      //Similar to the above with no timeline.
      //The right argument to where has to be a hardcoded array or else it will end up as a cube.
      DoTest ("{u:[s x #a 0 #b 10 #c 100 #a 1 #b 11 #c 101 #a 2 #b 12 #c 202] <-$u where false true false false true false false true false", "[s x #b 10 #b 11 #b 12]");
    }

    [Test]
    public void TestWhere6 ()
    {
      DoTest ("{u:[E|S|x 0 #a 0 0 #b 10 0 #c 100 1 #a 1 1 #b 11 1 #c 101 2 #a 2 2 #b 12 2 #c 202] <-$u where $u.x > 50}", "[E|S|x 0 #c 100 1 #c 101 2 #c 202]");
    }

    [Test]
    public void TestWhere7 ()
    {
      //Do not remove dups while wring to the result.
      DoTest ("{u:[E|S|x 0 #a 0 1 #a 0] <-$u where $u.x == 0}", "[E|S|x 0 #a 0 1 #a 0]");
    }

    [Test]
    public void TestWhere8 ()
    {
      //Do not remove dups while writing to the result.
      DoTest ("{u:[E|S|x 0 #a 0 1 #a 0] <-$u where $u.S == #a}", "[E|S|x 0 #a 0 1 #a 0]");
    }

    [Test]
    public void TestWhere9 ()
    {
      //18007130744
      DoTest ("[] where []", "[]");
    }

    [Test]
    public void TestIn ()
    {
      DoTest ("[S|x #a 0 #b 0 #c 1 #d 1 #e 2 #f 2] in 0 2", "[S|x #a true #b true #c false #d false #e true #f true]");
    }

    [Test]
    public void TestFill ()
    {
      DoTest ("fill [S|x y z #a 1 10 100 #a 2 -- -- #a 3 -- --]", "[S|x y z #a 1 10 100 #a 2 10 100 #a 3 10 100]");
    }

    [Test]
    public void TestFill1 ()
    {
      DoTest ("fill [S|x y #a 1 -- #a,0 3 4]", "[S|x y #a 1 -- #a,0 3 4]");
    }

    [Test]
    public void TestObject ()
    {
      DoTest ("object [S|x y z #a 1 10 100 #a 2 -- -- #a 3 -- --]", "[S|x y z #a 3 10 100]");
    }

    [Test]
    public void TestObject1 ()
    {
      DoTest ("count object [S|x y z #a 1 10 100 #a 2 -- -- #a 3 -- --]", "1");
    }

    [Test]
    public void TestObject2 ()
    {
      DoTest ("{u0:[S|x y z #a 1 10 100] u1:[S|x y z #b 2 20 200] <-object $u0 ! object $u1}", "[S|x y z #a 1 10 100 #b 2 20 200]");
    }

    [Test]
    public void TestObject3 ()
    {
      DoTest ("{u:[S|x y z #a 1 10 #new,a #a 2 20 -- #a 3 -- --] <-#z object $u}", "[S|x y z #new,a 3 20 #new,a]");
    }

    [Test]
    public void TestObject4 ()
    {
      DoTest ("#y object #a #a cube {x:0 0 y:#new,a #new,a}", "[S|x y #new,a 0 #new,a]");
    }

    [Test]
    [Ignore ("because")]
    public void TestObject5 ()
    {
      DoTest ("{u:[S|x #a 0 #a 1 #b 1 #b 2 #c 2 #c 3] <-#x object $u}", "[S|x #0 0 #1 1 #2 2 #3 3]");
    }

    [Test]
    public void TestJoin ()
    {
      DoTest ("[S|x #a 0 #b 1 #c 1 #d 2] join [x y 2 20 1 10 0 0]", "[S|x y #a 0 0 #b 1 10 #c 1 10 #d 2 20]");
    }

    [Test]
    public void TestJoin1 ()
    {
      //Unmatched row in the left argument.
      DoTest ("[S|x #a 0 #b 1 #c 1 #d 2 #e 3] join [x y 2 20 1 10 0 0]", "[S|x y #a 0 0 #b 1 10 #c 1 10 #d 2 20 #e 3 --]");
    }

    [Test]
    public void TestJoin2 ()
    {
      //Unmatched row in right argument.
      DoTest ("[S|x #a 0 #b 1 #c 1 #d 2] join [x y 2 20 3 30 1 10 0 0]", "[S|x y #a 0 0 #b 1 10 #c 1 10 #d 2 20]");
    }

    [Test]
    public void TestJoin3 ()
    {
      DoTest ("[S|k # # #0 #g #0,0 #g,0 #1 #i #1,0 #i,0] join [k src #g,0 #r,0 #i,0 #g,r,0]", "[S|k src # # -- #0 #g -- #0,0 #g,0 #r,0 #1 #i -- #1,0 #i,0 #g,r,0]");
    }

    [Test]
    public void TestAppendRect0 ()
    {
      DoTest ("[] & []", "[]");
    }

    [Test]
    public void TestAppendRect1 ()
    {
      DoTest ("[x 0] & [x 1]", "[x 0 1]");
    }

    [Test]
    public void TestAppendRect2 ()
    {
      DoTest ("[x 0] & [y 1]", "[x y 0 -- -- 1]");
    }

    [Test]
    public void TestAppendRect3 ()
    {
      DoTest ("[x y 0 1] & [x y 2 3]", "[x y 0 1 2 3]");
    }

    [Test]
    public void TestAppendRect4 ()
    {
      DoTest ("[x y 0 1] & [y z 2 3]", "[x y z 0 1 -- -- 2 3]");
    }

    [Test]
    public void TestAppendRect5 ()
    {
      DoTest ("[x 0] & [x 1 2]", "[x 0 1 2]");
    }

    [Test]
    public void TestAppendSymbol0 ()
    {
      DoTest ("[S|x #a 0] & [S|x #a 1]", "[S|x #a 0 #a 1]");
    }

    [Test]
    public void TestAppendSymbol1 ()
    {
      DoTest ("[S|x #a 0] & [S|y #a 1]", "[S|x y #a 0 -- #a -- 1]");
    }

    [Test]
    public void TestAppendSymbol2 ()
    {
      DoTest ("[S|x y #a 0 1] & [S|x y #a 2 3]", "[S|x y #a 0 1 #a 2 3]");
    }

    [Test]
    public void TestAppendSymbol3 ()
    {
      DoTest ("[S|x y #a 0 1] & [S|y z #a 2 3]", "[S|x y z #a 0 1 -- #a -- 2 3]");
    }

    [Test]
    [Ignore ("because")]
    public void TestAppendTime0 ()
    {
      //For cubes, append should merge the timelines of both arguments into one.
      //This assumes that both are already sorted properly.
      //But this has yet to be implemented.
      //We should be able to use RCCube.VisitCellsForward.
      DoTest (RCFormat.Default,
              "[E|S|x 0 #a 10 2 #a 12 4 #a 14] & [E|S|x 1 #a 11 3 #a 13 5 #a 15]",
              "[E|S|x 0 #a 10 1 #a 11 2 #a 12 3 #a 13 4 #a 14 5 #a 15]");
    }

    [Test]
    public void TestAppendTime1 ()
    {
      DoTest ("[E|S|x 0 #a 0] & [E|S|x 1 #a 1]", "[E|S|x 0 #a 0 1 #a 1]");
    }

    [Test]
    public void TestAppendTime2 ()
    {
      DoTest ("& {:[E|S|x 0 #a 0] :[E|S|x 1 #a 1] :[E|S|x 2 #a 2]}", "[E|S|x 0 #a 0 1 #a 1 2 #a 2]");
    }

    [Test]
    public void TestAppendTime3 ()
    {
      DoTest ("[T|S|x 08:00 #a 0] & [T|S|x 08:01 #a 1]", "[T|S|x 08:00 #a 0 08:01 #a 1]");
    }
      
    [Test]
    public void TestAppendTime4 ()
    {
      //Do not remove dups in the case of &.
      DoTest ("[E|S|x 0 #x 0] & [E|S|x 1 #x 0]", "[E|S|x 0 #x 0 1 #x 0]");
    }
      
    //[Test]
    //public void TestBang4 ()
    //{
    //  DoTest ("! {:[x y #a 0l #b 1l #c 2l]}", "[x y #a 0l #b 1l #c 2l]");
    //}

    [Test]
    public void TestRetimeline0 ()
    {
      //you still have to include T to get S to show up.
      //This is a bug.
      DoTest ("\"T\" \"S\" retimeline [S x #a 0 #b 1]", "[S|x #a 0 #b 1]");
    }

    [Test]
    public void TestRetimeline1 ()
    {
      DoTest ("\"S\" retimeline []", "[]");
    }

    [Test]
    [Ignore ("because")]
    public void TestRetimeline2 ()
    {
      //These don't work quite right because of the weird behavoir with S and T.
      //DoTest ("\"G\" retimeline [G|T|S|x 0 0 #a 0]", "[G|x 0 0]");
      //DoTest ("\"S\" retimeline [G|T|S|x 0 0 #a 0]", "[S|x #a 0]");
      //DoTest ("\"T\" retimeline [G|T|S|x 0 0 #a 0]", "[T|x 0 0]");
      //DoTest ("\"G\" \"S\" retimeline [G|T|S|x 0 0 #a 0]", "[G|S|x 0 #a 0]");
    }

    protected RCRunner runner = new RCRunner (RCActivator.Default,
                                              new RCLog (), 1,
                                              new RCLArgv ("--output=test", "--show=print"));

    public void DoTest (string code, string expected)
    {
      CoreTest.DoTest (runner, RCFormat.Default, code, expected);
    }

    public void DoTest (RCFormat args, string code, string expected)
    {
      CoreTest.DoTest (runner, args, code, expected);
    }
  }
}

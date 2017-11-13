
using System;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core {

  public class Willow {

    public class Point {
      public static readonly int BACK = 3;
      public static readonly int LEFT = -1;
      public static readonly int RIGHT = 1;
      public readonly long G, R, B;
      public readonly int D;
      public readonly long T;
      public char S;

      public Point (long t, long g, long r, long b, int d, char s) {
        if (g + r + b != 0) {
          throw new Exception ("Invalid point, (g + r + b) != 0");
        }
        G = g;
        R = r;
        B = b;
        D = d;
        T = t;
        S = s;
      }

      public Point PointIn (long t, int d, char s) {
        int d1 = (D + d) % 6;
        if (d1 < 0) d1 = 6 + d1;
        return new Point (t,
                          G + m_direction[d1, 0],
                          R + m_direction[d1, 1],
                          B + m_direction[d1, 2], d1, s);

      }

      public Point PointIn (int d, char s) {
        return PointIn (T, d, s);
      }

      public Point PointBack (Dictionary<Point, Point> space, char s) {
        Point p1 = PointIn (BACK, s);
        //This p1 doesn't know which direction it was supposed to come from.
        //Need to get the real one from space to have D be correct.
        Point p2;
        if (!space.TryGetValue (p1, out p2)) {
          return null;
        }
        return p2;
      }

      public double X () {
        return Math.Sqrt (3) * (G + B / 2d);
      }

      public double Y () {
        return (-3 / 2d) * B;
      }

      public override int GetHashCode ()
      {
        //This could lead to messed up distributions as we get outside of int space.
        int h = 33 * 0 ^ (int) G;
        h = 33 * h ^ (int) R;
        h = 33 * h ^ (int) B;
        return h;
      }

      public override bool Equals (object obj)
      {
        if (obj == this) return true;
        if (obj == null) return false;
        Point cobj = obj as Point;
        if (cobj == null) return false;
        if (G != cobj.G) return false;
        if (R != cobj.R) return false;
        if (B != cobj.B) return false;
        //D is not in equality.
        //if (D != cobj.D) return false;
        return true;
      }

      public override string ToString ()
      {
        return string.Format ("x:{0} y:{1}", X (), Y ());
      }

      public char State () {
        char s = S;
        S = 'o';
        return s;
      }

      protected static int[,] m_direction = new int[,] {
        {  1,  -1,  0 },  //right
        {  0,  -1,  1 },  //down right
        { -1,   0,  1 },  //down left
        { -1,   1,  0 },  //left
        {  0,   1, -1 },  //up left
        {  1,   0, -1 }   //up right
      };
    }

    [RCVerb ("willow")]
    public virtual void EvalWillow (
      RCRunner runner, RCClosure closure, RCLong right) {

      int time, bar;
      if (right.Count == 1) {
        time = (int) right[0];
        bar = time;
      }
      else {
        time = (int) right[0];
        bar = (int) right[1];
      }

      //Okay now how to index and retreive the points in space?
      RCCube result = new RCCube ();
      Random random = new Random ();
      Dictionary<Point, Point> space = new Dictionary<Point, Point> ();
      Point origin = new Point (0, 0, 0, 0, 0, 'n');
      space.Add (origin, origin);
      int t = 0;
      int b = 0;
      try {
        while (t < time) {
          int begin = b * bar;
          int end = Math.Min (time, (b + 1) * bar);
          for (t = begin; t < end; ++t) {
            //Follow a single quantum until it finds a new point on the frontier.
            //This universe will flow from left to right
            Force (t, random, origin, space);
          }
          State (space, result, ref b, end);
        }
      }
      catch (Exception ex) {
        Console.Out.WriteLine (ex);
      }
      finally {
        runner.Yield (closure, result);
      }
    }

    protected void Force (long t, Random random, Point origin, Dictionary<Point, Point> space) {
      Point p0;
      Point p1 = origin;
      Point pre = null;
      while (true) {
        int choice = ((random.Next () % 2) == 0) ? -1 : 1;
        p0 = p1;
        p1 = p0.PointIn (t, choice, 'n');
        space.TryGetValue (p1, out pre);
        if (p1.Equals (origin)) {
          p0 = p1;
        }
        else if (pre == null) {
          Console.Out.WriteLine ("(t={0}) Adding {1}", t, p1);
          space.Add (p1, p1);
          break;
        }
        //it is a branch from the current object.
        else if (pre.PointBack (space, 't').Equals (p0)) {
          p0 = p1;
        }
        //it is a collision
        else {
          Queue<Point> conflicts = new Queue<Point> ();
          conflicts.Enqueue (p1);
          while (conflicts.Count > 0) {
            Point c = conflicts.Dequeue ();
            Console.Out.WriteLine ("(t={0}) Initial conflict at {1}", t, c);
            //p0 is a point on the mover.
            //c is the point occupied by the movee.
            //p1 is also the point of contact.
            //The fault line is a set of points from p0 back to the origin.
            //f is a point on the fault line.
            HashSet<Point> fault = new HashSet<Point> ();
            Point f = p0;
            while (!f.Equals (origin)) {
              fault.Add (f);
              f = f.PointBack (space, 't');
              if (f == null) {
                Console.Out.WriteLine ("Disconnected from origin");
                throw new Exception ();
              }
            }
            fault.Add (origin);

            //s is the point that is shared between the movee and the fault line.
            //m0 is the root of the movee.
            Point m0 = null;
            Point s;
            if (!space.TryGetValue (c, out s)) {
              Console.Out.WriteLine ("Collision point {0} does not exist. Conflict already resolved.", c);
              continue;
            }
            int limit = 0;
            while (limit < 100 && !fault.Contains (s)) {
              m0 = s;
              s = s.PointBack (space, 't');
              if (s == null) {
                Console.Out.WriteLine ("Disconnected from fault line");
                throw new Exception ();
              }
              ++limit;
            }
            Console.Out.WriteLine ("The shared point is {0}.", s);

            //insert a new link separating the mover and the movee.
            if (m0 == null) {
              Console.Out.WriteLine ("Could not find the point to move.");
              break;
            }

            //delete all points from m0.
            //replace with the same structure relative to m1.
            //this provides basic motion and rotation.
            Point m1 = m0.PointIn (t, choice, 'm');
            Console.Out.WriteLine ("(t={0}) Initial Move: {1} to {2}", t, m0, m1);
            Dictionary<Point, Point> removed = new Dictionary<Point, Point> ();
            Remove (t, null, m0, space, removed);
            space.Add (m0, m0);
            Readd (t, m0, m1, space, removed, conflicts);
          }
          break;
        }
      }
    }

    protected void Remove (
      long t, Point @from, Point m0, Dictionary<Point, Point> space, Dictionary<Point, Point> removed) {
      Console.Out.WriteLine ("(t={0}) Removing {1} from {2}", t, m0, @from);
      if (m0.G == 0 && m0.R == 0 && m0.B == 0) {
        Console.Out.WriteLine ("Not removing the origin.");
        return;
      }

      Point l = m0.PointIn (Point.LEFT, 'r');
      Point lb = l.PointIn (Point.BACK, 'r');
      Point la = null;
      space.TryGetValue (l, out la);
      Point r = m0.PointIn (Point.RIGHT, 'r');
      Point rb = r.PointIn (Point.BACK, 'r');
      Point ra = null;
      space.TryGetValue (r, out ra);
      space.Remove (m0);
      removed.Add (m0, m0);

      if (lb == null) {
        Console.Out.WriteLine ("No point back from l " + l);
      }
      else if (la != null && lb.Equals (m0)) {
        if (la.D == l.D)
          Remove (t, m0, la, space, removed);
      }
      if (rb == null) {
        Console.Out.WriteLine ("No point back from r " + r);
      }
      else if (ra != null && rb.Equals (m0)) {
        if (ra.D == r.D)
          Remove (t, m0, ra, space, removed);
      }
    }

    protected void Readd (
      long t, Point m0, Point m1, Dictionary<Point, Point> space, Dictionary<Point, Point> removed, Queue<Point> conflicts) {
      Console.Out.WriteLine ("(t={0}) Readding {1}", t, m1);
      if (space.ContainsKey (m1) && !removed.ContainsKey (m1)) {
        Console.Out.WriteLine ("(t={0}) Secondary conflict at {1}", t, m1);
        conflicts.Enqueue (m1);
        return;
      }
      if (space.ContainsKey (m1)) {
        Console.Out.WriteLine ("(t={0}) Tried to add {0} twice.", m1);
        return;
      }
      space.Add (m1, m1);
      Point l0 = m0.PointIn (Point.LEFT, 'm');
      if (removed.ContainsKey (l0)) {
        Point l1 = m1.PointIn (removed[l0].T, Point.LEFT, 'm');
        Readd (t, l0, l1, space, removed, conflicts);
      }
      Point r0 = m0.PointIn (Point.RIGHT, 'm');
      if (removed.ContainsKey (r0)) {
        Point r1 = m1.PointIn (removed[r0].T, Point.RIGHT, 'm');
        Readd (t, r0, r1, space, removed, conflicts);
      }
    }

    public void State (Dictionary<Point, Point> space, RCCube result, ref int b, int end) {
      foreach (Point p1 in space.Values) {
        Point p0 = p1.PointIn (Point.BACK, 'o');
        //RCSymbolScalar sym = RCSymbolScalar.From (
        //  "gen", (long) p1.G, (long) p1.R, (long) p1.B);
        long t = (long) p1.T;
        RCSymbolScalar sym = RCSymbolScalar.From ("gen", t);
        result.WriteCell ("g0", sym, p0.G);
        result.WriteCell ("r0", sym, p0.R);
        result.WriteCell ("b0", sym, p0.B);
        result.WriteCell ("g1", sym, p1.G);
        result.WriteCell ("r1", sym, p1.R);
        result.WriteCell ("b1", sym, p1.B);
        result.WriteCell ("x0", sym, p0.X ());
        result.WriteCell ("y0", sym, p0.Y ());
        result.WriteCell ("x1", sym, p1.X ());
        result.WriteCell ("y1", sym, p1.Y ());
        result.WriteCell ("state", sym, p1.State ().ToString ());
        result.Write (end - 1, sym);
      }
      ++b;
    }
  }

  public class TopGun {

    public class Point {
      public readonly long G, R, B;

      public Point (long g, long r, long b) {
        if (g + r + b != 0) {
          throw new Exception ("Invalid point, (g + r + b) != 0");
        }
        G = g;
        R = r;
        B = b;
      }

      public double X () {
        return Math.Sqrt (3) * (G + B/2d);
      }

      public double Y () {
        return (-3/2d) * B;
      }

      public override int GetHashCode ()
      {
        //This could lead to messed up distributions as we get outside of int space.
        int h = 33 * 0 ^ (int) G;
        h = 33 * h ^ (int) R;
        h = 33 * h ^ (int) B;
        return h;
      }

      public override bool Equals (object obj)
      {
        if (obj == this) return true;
        if (obj == null) return false;
        Point cobj = obj as Point;
        if (cobj == null) return false;
        if (G != cobj.G) return false;
        if (R != cobj.R) return false;
        if (B != cobj.B) return false;
        return true;
      }

      public override string ToString ()
      {
        return string.Format ("g:{0} r:{1} b:{2} x:{3} y:{4}", G, R, B, X (), Y ());
      }
    }

    public class Line {
      public readonly long T;
      public int D;
      public Line U, C0, C1;
      public Point P;
      public System.Drawing.Color C;

      public Line (long t, Line u, Line c0, Line c1, int d, Point p) {
        T = t;
        U = u;
        C0 = c0;
        C1 = c1;
        D = d;
        P = p;
      }

      public Point PointUp () {
        return PointIn (D + 3);
      }

      public Point PointIn (int d) {
        switch (d % 6) {
        case 0:return Right;
        case 1:return DownRight;
        case 2:return DownLeft;
        case 3:return Left;
        case 4:return UpLeft;
        case 5:return UpRight;
          default: throw new Exception ();
        }
      }

      public Point Right {
        get { return new Point(P.G+1, P.R-1, P.B); }
      }
      public Point DownRight {
        get { return new Point(P.G, P.R-1, P.B+1); }
      }
      public Point DownLeft {
        get { return new Point(P.G-1, P.R, P.B+1); }
      }
      public Point Left {
        get { return new Point(P.G-1, P.R+1, P.B); }
      }
      public Point UpLeft {
        get { return new Point(P.G, P.R+1, P.B-1); }
      }
      public Point UpRight {
        get { return new Point(P.G+1, P.R, P.B-1); }
      }
    }

    protected int[,] m_direction = new int[,] {
      {  1,  -1,  0 },  //right
      {  0,  -1,  1 },  //down right
      { -1,   0,  1 },  //down left
      { -1,   1,  0 },  //left
      {  0,   1, -1 },  //up left
      {  1,   0, -1 }   //up right
    };

    protected int[,] m_choice = new int[,] {
      {  5,  1 },       //from right go left to upright or right to downright
      {  0,  2 },       //from down right go left to right or right to down left
      {  1,  3 },       //from down left go left to down right or right to up left
      {  2,  4 },       //from left go left to down left or right to up left
      {  3,  5 },       //from up left go left to left or right to up right
      {  4,  0 }        //from up right go left to up left or right to right
    };

    [RCVerb ("topgun")]
    public virtual void EvalTopGun (
      RCRunner runner, RCClosure closure, RCLong right) {

      int time, bar;
      if (right.Count == 1) {
        time = (int) right[0];
        bar = time;
      }
      else {
        time = (int) right[0];
        bar = (int) right[1];
      }

      //Okay now how to index and retreive the points in space?
      RCCube result = new RCCube ();
      Random random = new Random ();
      Dictionary<Point, Line> space = new Dictionary<Point, Line> ();
      List<Line> events = new List<Line> ();
      int t = 0;
      int b = 0;
      try {
        while (t < time) {
          int begin = b * bar;
          int end = Math.Min (time, (b + 1) * bar);
          for (t = begin; t < end; ++t) {
            //Follow a single quantum until it finds a new point on the frontier.
            //This universe will flow from left to right
            Force (t, random, space, events);
            Check (space);
          }
          State (space, events, result, ref b, end);
        }
      }
      catch (Exception ex) {
        Console.Out.WriteLine (ex);
      }
      finally {
        runner.Yield (closure, result);
      }
    }

    protected void Force (long t, Random random, Dictionary<Point, Line> space, List<Line> events) {
      //growth phase
      Line u = new Line (t, null, null, null, 0, new Point (0, 0, 0));
      long d0 = 0;
      long g0 = 0;
      long r0 = 0;
      long b0 = 0;
      Queue<Line> checklist = new Queue<Line> ();

      while (true) {
        //calculate the next point using the choice and direction maps.
        int choice = random.Next () % 2;
        int d1 = m_choice[d0 % 6, choice];
        long g1 = g0 + m_direction[d1, 0];
        long r1 = r0 + m_direction[d1, 1];
        long b1 = b0 + m_direction[d1, 2];
        Line c = null;
        Point p1 = new Point (g1, r1, b1);
        space.TryGetValue (p1, out c);

        //ensure we do not create a cycle at the origin.
        if (g1 == 0 && r1 == 0 && b1 == 0) {
          d0 = 0;
          g0 = 0;
          r0 = 0;
          b0 = 0;
          choice = random.Next () % 2;
        }
        else {
          //nothing exists at this point.
          if (c == null) {
            Line newLine = new Line (t, u, null, null, d1, p1);
            space.Add (p1, newLine);
            Console.Out.WriteLine ("Adding:" + p1);
            if (choice == 0)
              u.C0 = newLine;
            else
              u.C1 = newLine;
            break;
          }
          //p is a branch from this point.  Keep going.
          if (c.U == null || (c.U.P.G == g0 && c.U.P.R == r0 && c.U.P.B == b0)) {
            u = c;
            d0 = d1;
            g0 = g1;
            r0 = r1;
            b0 = b1;
          }
          //it is a collision point
          else {
            checklist.Enqueue (c);
            break;
          }
        }
      }

      //configuration phase
      while (checklist.Count > 0) {
        //c is a point on the object responsible for the collision.
        Line c = checklist.Dequeue ();
        Line mover = null;
        //m is a point on the object that will move.
        Line m = null;
        space.TryGetValue (c.P, out mover);
        m = mover;
        if (mover != null) {
          //Resolve collisions.
          //But my way means updating absolute coordinates on all points that move.
          Dictionary<Point, Line> parents = new Dictionary<Point, Line> ();
          c = c.U;
          m = m.U;
          while (true) {
            if (c.U == null) break;

            Line p;
            parents.TryGetValue (c.U.P, out p);
            //c.P not identified as the common point.
            if (p == null) {
              parents.Add (c.U.P, c.U);
            }
            else if (m.U.C0 == null || m.U.C1 == null) {
              throw new Exception ();
            }
            //c is C0 (left) from the common point.
            else if (c.P.Equals (c.U.C0.P)) {
              //insert on the opposite (C1) side.
              p.C1 = new Line (t, p, null, p.C1, p.C1.D+1, p.C1.P);
              Fix (space, events, checklist, p.C1.C1, p, 1);
              break;
            }
            //c is C1 (right) from the common point.
            else if (c.P.Equals (c.U.C1.P)) {
              //insert on the opposite (C0) side.
              p.C0 = new Line (t, p, p.C0, null, p.C0.D-1, p.C0.P);
              Fix (space, events, checklist, p.C0.C0, p, 0);
              break;
            }
            else throw new Exception ();

            //m.P not identified as the common point.
            parents.TryGetValue (m.U.P, out p);
            if (p == null) {
              parents.Add (m.U.P, m.U);
            }
            else if (p.C0 != null && m.P.Equals (p.C0.P)) {
              //insert on the same (C0) side
              p.C0 = new Line (t, p, p.C0, null, p.C0.D-1, p.C0.P);
              Fix (space, events, checklist, p.C0.C0, p, 0);
              break;
            }
            else if (p.C1 != null && m.P.Equals (p.C1.P)) {
              //insert on the same (C1) side
              p.C1 = new Line (t, p, null, p.C1, p.C1.D+1, p.C1.P);
              Fix (space, events, checklist, p.C1.C1, p, 1);
              break;
            }
            else throw new Exception ();

            c = c.U;
            m = m.U;
          }

          //I think this is where you finally update the index for m.
          //now walk up from the both current and mover until you find the common point.
          //insert the current link in between the common point and the object that is moving.
          //then recalculate all the new positions for the moved object.
          //All of the moved points must be added to the checklist.
          //We need to look at two sets, points previously occupied by this object, points now occupied by this object.
          //The points in the new set that are not in the old set are the collision candidates.
        }
      }
      //check collisions
      //break if no collisions
      //resolve collisions
    }

    public void Fix (Dictionary<Point, Line> space, List<Line> events, Queue<Line> checklist, Line mover, Line parent, int choice) {

      if (mover == null) return;
      mover.U = parent;

      //It is the points and directions, and space that need fixing, not anything else.
      //First get a list of all the points by visiting each line.
      List<Point> oldPoints = new List<Point> ();
      Stack<Line> stack = new Stack<Line> ();
      stack.Push (mover);
      while (stack.Count > 0) {
        Line m = stack.Pop ();
        oldPoints.Add (m.P);
        space.Remove (m.P);
        Console.Out.WriteLine ("Removed:" + m.P);
        //events.Add (new Line (m.U,
        if (m.C0 != null) {
          stack.Push (m.C0);
        }
        if (m.C1 != null) {
          stack.Push (m.C1);
        }
      }

      //Now we build a list of all of the new points.
      //This is done by walking the tree again from m.
      stack.Clear ();
      stack.Push (mover);
      while (stack.Count > 0) {
        //I have to know what the initial direction is without going back to the origin!
        Line m = stack.Pop ();
        int d1 = m_choice[m.D % 6, choice];
        long g1 = m.P.G + m_direction[d1, 0];
        long r1 = m.P.R + m_direction[d1, 1];
        long b1 = m.P.B + m_direction[d1, 2];
        Point p1 = new Point (g1, r1, b1);

        //Totally unclear if this is right or even close.
        Line c = null;
        space.TryGetValue (p1, out c);
        if (c == null) {
          //Update direction and point.  The relationships stay the same.
          m.D = d1;
          m.P = p1;
          Console.Out.WriteLine ("Replacing:" + p1);
          space.Add (p1, m);
          if (m.C0 != null)
            stack.Push (m.C0);
          if (m.C1 != null)
            stack.Push (m.C1);
        }
        else {
          Console.Out.WriteLine ("Resolving:" + c.P);
          checklist.Enqueue (c);
        }
      }
      //space.Add (mover.P, mover);
    }

    public void Check (Dictionary<Point, Line> space) {

      //Ensure each point in the index matches the point in the value.
      foreach (KeyValuePair<Point, Line> kv in space) {
        if (!kv.Key.Equals (kv.Value.P)) {
          Console.Out.WriteLine ("space/point mismatch at key:{0} value:{1}", kv.Key, kv.Value.P);
        }
      }

      //Ensure each line in the tree is represented in the index.
      //But this means searching three ways from the origin.
      //Yuck.
    }

    public void State (Dictionary<Point, Line> space, List<Line> events, RCCube result, ref int b, int end) {
      RCSymbolScalar gen = new RCSymbolScalar (null, "gen");
      foreach (KeyValuePair<Point, Line> kv in space) {
        Line v = kv.Value;
        RCSymbolScalar gent = new RCSymbolScalar (gen, v.T);
        Point pup = v.PointUp ();
        result.WriteCell ("g0", gent, pup.G);
        result.WriteCell ("r0", gent, pup.R);
        result.WriteCell ("b0", gent, pup.B);
        result.WriteCell ("g1", gent, v.P.G);
        result.WriteCell ("r1", gent, v.P.R);
        result.WriteCell ("b1", gent, v.P.B);
        result.WriteCell ("x0", gent, pup.X ());
        result.WriteCell ("y0", gent, pup.Y ());
        result.WriteCell ("x1", gent, v.P.X ());
        result.WriteCell ("y1", gent, v.P.Y ());
        result.Write (end - 1, gent);
      }
      ++b;
    }
  }

  public class TriMap<K, V> {
    protected Dictionary<K, Dictionary<K, Dictionary<K, V>>> m_x =
      new Dictionary<K, Dictionary<K, Dictionary<K, V>>> ();

    public V Get (K x, K y, K z) {
      V v = default (V);
      Dictionary<K, Dictionary<K, V>> Y;
      if (m_x.TryGetValue (x, out Y)) {
        Dictionary<K, V> Z;
        if (Y.TryGetValue (y, out Z)) {
          Z.TryGetValue (z, out v);
        }
      }
      return v;
    }

    public void Put (K x, K y, K z, V v) {
      Dictionary<K, Dictionary<K, V>> Y;
      Dictionary<K, V> Z;
      if (!m_x.TryGetValue (x, out Y)) {
        Y = new Dictionary<K, Dictionary<K, V>> ();
        m_x.Add (x, Y);
      }
      if (!Y.TryGetValue (y, out Z)) {
        Z = new Dictionary<K, V> ();
        Y.Add (y, Z);
      }
      if (Z.ContainsKey (z)) {
        //This is a known issue, it should never happen
        //but happens all the time.
        //probably why the algo doesn't work right.
        //Console.Out.WriteLine ("Overwriting at " + v);
      }
      Z[z] = v;
    }

    public V Delete (K x, K y, K z) {
      Dictionary<K, Dictionary<K, V>> Y;
      Dictionary<K, V> Z;
      if (m_x.TryGetValue (x, out Y)) {
        V v;
        if (Y.TryGetValue (y, out Z)) {
          if (Z.TryGetValue (z, out v)) {
            Z.Remove (z);
          }
          return v;
        }
      }
      Console.Out.WriteLine ("Cannot delete: " + x + "," + y + "," + z);
      return default (V);
    }

    public List<V> ToArray () {
      List<V> result = new List<V> ();
      foreach (KeyValuePair<K, Dictionary<K, Dictionary<K, V>>> Y in m_x) {
        foreach (KeyValuePair<K, Dictionary<K, V>> Z in Y.Value) {
          foreach (KeyValuePair<K, V> v in Z.Value) {
            result.Add (v.Value);
          }
        }
      }
      return result;
    }
  }

  //Generator
  public class Generator {

    //Should this be a relative or an absolute data structure?
    //I don't care about moving large numbers of these things.
    //Do I care about identity?
    //I should, that is an interesting line of inquiry, which geometry ends up where.
    //So all these fields are going to be mutable except T, right?
    //So you can say at any time, where is that q from t=12345 right?
    //And you would likely find that it moved.

    //Quantum
    public class Quantum {

      //The timestamp marked by the creation of this quantum.
      public long T;
      //Direction vector into this point.
      public long d0;
      //Hexagonal coordinates.
      public long g0, g1, r0, r1, b0, b1;
      //Rectangular coordinate.
      public double x0, x1, y0, y1;

      public override string ToString ()
      {
        return string.Format ("{0},{1},{2} to {3},{4},{5}", g0, r0, b0, g1, r1, b1);
      }
    }

    public enum CollisionRule {
      Annhilate,
      Reverberate
    }

    //Space
    public class Space {

      protected TriMap<long, Quantum> m_end = new TriMap<long, Quantum> ();
      protected TriMap<long, Quantum> m_start0 = new TriMap<long, Quantum> ();
      protected TriMap<long, Quantum> m_start1 = new TriMap<long, Quantum> ();

      public Quantum StartsAt (long x, long y, long z) {
        Quantum q = m_start0.Get (x, y, z);
        if (q == null)
          q = m_start1.Get (x, y, z);
        return q;
      }

      public Quantum EndsAt (long x, long y, long z) {
        return m_end.Get (x, y, z);
      }

      public void Put (long t, long d0, long x0, long y0, long z0, long x1, long y1, long z1, long choice) {
        if (x0 + y0 + z0 != 0) {
          throw new Exception ("Invalid start point");
        }
        if (x1 + y1 + z1 != 0) {
          throw new Exception ("Invalid end point");
        }
        Quantum q = new Quantum ();
        q.T = t;
        q.d0 = d0;
        q.g0 = x0;
        q.r0 = y0;
        q.b0 = z0;
        q.g1 = x1;
        q.r1 = y1;
        q.b1 = z1;

        long g = q.g0;
        //long r = q.r0;
        long b = q.b0;
        q.x0 = Math.Sqrt (3) * (g + b/2d);
        q.y0 = (-3/2d) * b;
        g = q.g1;
        //r = q.r1;
        b = q.b1;
        q.x1 = Math.Sqrt (3) * (g + b/2d);
        q.y1 = (-3/2d) * b;

        m_end.Put (x1, y1, z1, q);

        Quantum s0 = m_start0.Get (x0, y0, z0);
        if (s0 == null) {
          m_start0.Put (x0, y0, z0, q);
        }
        else {
          //Quantum s1 = m_start1.Get (x0, y0, z0);
          m_start1.Put (x0, y0, z0, q);
        }
      }

      public long Delete (long x1, long y1, long z1) {
        Quantum q = m_end.Delete (x1, y1, z1);
        //Console.Out.WriteLine ("Deleting " + q);
        Quantum s0 = m_start0.Get (q.g0, q.r0, q.b0);
        if (s0 != null && s0.g1 == x1 && s0.r1 == y1 && s0.b1 == z1) {
          m_start0.Delete (q.g0, q.r0, q.b0);
        }
        Quantum s1 = m_start1.Get (q.g0, q.r0, q.b0);
        if (s1 != null && s1.g1 == x1 && s1.r1 == y1 && s1.b1 == z1) {
          m_start1.Delete (q.g0, q.r0, q.b0);
        }
        return q.T;
      }

      public void State (Space space, RCCube result, ref int b, int end) {
        RCSymbolScalar gen = new RCSymbolScalar (null, "gen");
        List<Quantum> list = m_end.ToArray ();
        for (int i = 0; i < list.Count; ++i) {
          Quantum q = list[i];
          RCSymbolScalar gent = new RCSymbolScalar (gen, q.T);
          result.WriteCell ("g0", gent, q.g0);
          result.WriteCell ("r0", gent, q.r0);
          result.WriteCell ("b0", gent, q.b0);
          result.WriteCell ("g1", gent, q.g1);
          result.WriteCell ("r1", gent, q.r1);
          result.WriteCell ("b1", gent, q.b1);
          result.WriteCell ("x0", gent, q.x0);
          result.WriteCell ("y0", gent, q.y0);
          result.WriteCell ("x1", gent, q.x1);
          result.WriteCell ("y1", gent, q.y1);
          result.Write (end - 1, gent);
        }
        ++b;
      }
    }

    protected long[,] m_direction = new long[,] {
      {  1,  -1,  0 },  //right
      {  0,  -1,  1 },  //down right
      { -1,   0,  1 },  //down left
      { -1,   1,  0 },  //left
      {  0,   1, -1 },  //up left
      {  1,   0, -1 }   //up right
    };

    protected long[,] m_choice = new long[,] {
      {  5,  1 },       //from right go left to upright or right to downright
      {  0,  2 },       //from down right go left to right or right to down left
      {  1,  3 },       //from down left go left to down right or right to up left
      {  2,  4 },       //from left go left to down left or right to up left
      {  3,  5 },       //from up left go left to left or right to up right
      {  4,  0 }        //from up right go left to up left or right to right
    };

    protected void Force (
      long t, Random random, Space space, CollisionRule rule, long d0, long x0, long y0, long z0, int choice) {
      if (x0 + y0 + z0 != 0) {
        throw new Exception ("Invalid start point (" + x0 + "," + y0 + "," + z0 + ")");
      }

      while (true) {
        long d1 = m_choice[d0 % 6, choice];
        long x1 = x0 + m_direction[d1, 0];
        long y1 = y0 + m_direction[d1, 1];
        long z1 = z0 + m_direction[d1, 2];

        if (x1 == 0 && y1 == 0 && z1 == 0) {
          d0 = d1;
          x0 = x1;
          y0 = y1;
          z0 = z1;
          choice = random.Next () % 2;
        }

        Quantum e = space.EndsAt (x1, y1, z1);
        if (e == null) {
          //It is an end point only
          space.Put (t, d0, x0, y0, z0, x1, y1, z1, choice);
          break;
        }
        else if (e.g0 != x0 || e.r0 != y0 || e.b0 != z0) {
          //There is a point ending here but it is not the point x0,y0,z0
          //This means there is a collision.
          if (rule == CollisionRule.Annhilate) {
            long oldt = space.Delete (x1, y1, z1);
            Force (oldt, random, space, rule, d0 + 3, e.g1, e.r1, e.b1, choice);
            Force (t, random, space, rule, d0, x0, y0, z0, (choice + 1) % 2);
            break;
          }
          else if (rule == CollisionRule.Reverberate) {
            d0 = d1;
            x0 = x1;
            y0 = y1;
            z0 = z1;
            choice = (choice + 1) % 2;
          }
          else throw new Exception ();
        }
        else {
          d0 = d1;
          x0 = x1;
          y0 = y1;
          z0 = z1;
          choice = random.Next () % 2;
        }
      }
    }

    [RCVerb ("annhilate")]
    public virtual void Annhilate (
      RCRunner runner, RCClosure closure, RCLong right) {
      EvalGenerator (runner, closure, right, CollisionRule.Annhilate);
    }

    [RCVerb ("reverberate")]
    public virtual void Reverberate (
      RCRunner runner, RCClosure closure, RCLong right) {
      EvalGenerator (runner, closure, right, CollisionRule.Reverberate);
    }

    public virtual void EvalGenerator (
      RCRunner runner, RCClosure closure, RCLong right, CollisionRule rule) {

      int time, bar;
      if (right.Count == 1) {
        time = (int) right[0];
        bar = time;
      }
      else {
        time = (int) right[0];
        bar = (int) right[1];
      }

      //Okay now how to index and retreive the points in space?
      RCCube result = new RCCube ();
      Random random = new Random ();
      Space space = new Space ();
      int t = 0;
      int b = 0;
      try {
        while (t < time) {
          int begin = b * bar;
          int end = Math.Min (time, (b + 1) * bar);
          for (t = begin; t < end; ++t) {
            //Follow a single quantum until it finds a new point on the frontier.
            //This universe will flow from left to right
            Force (t, random, space, rule, 0, 0, 0, 0, random.Next () % 2);
          }
          space.State (space, result, ref b, end);
        }
      }
      catch (Exception ex) {
        Console.Out.WriteLine (ex);
      }
      finally {
        runner.Yield (closure, result);
      }
    }
  }

  public class Quadra {

    public class Atom {
      //Current time.
      public long T;
      //Unique id.
      public long I;
      //Distance from the parent.
      public long D;
      //The first child.
      public Atom X;
      //The second child.
      public Atom Y;
      //The integer x coordinate
      public long x;
      //The integer y coordinate
      public long y;
      //The parent of this node.
      public Atom Parent;

      //Used by the quadroid model.
      public Atom (int t, int i)
      {
        T = t;
        I = i;
        D = 2;
        X = null;
        Y = null;
      }

      //Used by the nexus model.
      public Atom (int t, int i, int choice, Atom parent) {
        T = t;
        I = i;
        D = 1;
        X = null;
        Y = null;
        if (parent != null) {
          if (choice == 0) {
            x = parent.x + 1;
            y = parent.y;
          }
          else {
            x = parent.x;
            y = parent.y + 1;
          }
        }
        Parent = parent;
      }
    }

    [RCVerb ("quadra")]
    public virtual void EvalQuadra (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      int time, bar;
      if (right.Count == 1) {
        time = (int) right[0];
        bar = time;
      }
      else {
        time = (int) right[0];
        bar = (int) right[1];
      }

      RCCube result = new RCCube ();
      Random random = new Random ();
      Atom origin = new Atom (0, 0);
      int i = 1;
      int t = 0;
      int b = 0;
      while (t < time) {
        int begin = b * bar;
        int end = Math.Min (time, (b + 1) * bar);
        for (t = begin; t < end; ++t) {
          //Follow a single quantum until it finds a new place in space.
          Atom point = origin;
          while (true) {
            if (point.D < 3) {
              //Any new location must acquire two points of distance before it can branch.
              ++point.D;
              break;
            }
            else if (point.X == null || point.Y == null) {
              ++point.D;
              if (point.X == null) {
                ++i;
                point.X = new Atom (t, i);
              }
              if (point.Y == null) {
                ++i;
                point.Y = new Atom (t, i);
              }
              break;
            }
            else {
              long dx = point.X.D;
              long dy = point.Y.D;
              if (point.D <= dx + dy) {
                ++point.D;
                break;
              }
              //do not collide children until they are big enough to do something interesting.
              //any point with size less than 4 does not really have a shape, not something we can move.
              else if (dx < 4) {
                ++point.X.D;
                if (point.X.D == 4) {
                  if (point.X.X != null || point.X.Y != null) {
                    throw new Exception ("Should not happen");
                  }
                  ++i;
                  point.X.X = new Atom (t, i);
                  ++i;
                  point.X.Y = new Atom (t, i);
                }
                break;
              }
              else if (dy < 4) {
                ++point.Y.D;
                if (point.Y.D == 4) {
                  if (point.Y.X != null || point.Y.Y != null) {
                    throw new Exception ("Should not happen");
                  }
                  ++i;
                  point.Y.X = new Atom (t, i);
                  ++i;
                  point.Y.Y = new Atom (t, i);
                }
                break;
              }
              else if (Math.Max (dx, dy) - Math.Min (dx, dy) == 1) {
                //If the distance between an object and its parent is 1 and a quantum arrives,
                //Combine smaller one into the larger one.
                ++point.D;
                Atom parent = dx > dy ? point.X : point.Y;
                Atom mover;
                if (parent == point.X) {
                  mover = point.Y;
                  point.Y = null;
                }
                else {
                  mover = point.X;
                  point.X = null;
                }
                while (true) {
                  parent.D += Math.Max (1, mover.D - 1);
                  if (parent.X == null && parent.Y == null) {
                    parent.X = mover.X;
                    parent.Y = mover.Y;
                    break;
                  }
                  else if (parent.X == null) {
                    parent.X = mover;
                    break;
                  }
                  else if (parent.Y == null) {
                    parent.Y = mover;
                    break;
                  }
                  else {
                    if (random.Next () % 2 == 0) {
                      parent = parent.X;
                    }
                    else {
                      parent = parent.Y;
                    }
                  }
                }
                break;
              }
              else {
                //Chose a path for this quantum.
                if (random.Next () % 2 == 0) {
                  point = point.X;
                }
                else {
                  point = point.Y;
                }
              }
            }
          }
        }

        //Reorganize the data into a cube structure.
        //For each node we want to know 2 things, its distance, its parent, and the time it was created.
        //time will serve as the name of the node as well.
        RCSymbolScalar atom = new RCSymbolScalar (null, "atom");
        Stack<Atom> stack = new Stack<Atom> ();
        Atom current = origin;
        RCSymbolScalar s = new RCSymbolScalar (atom, origin.I);
        result.WriteCell ("p", s, (long)-1);
        result.WriteCell ("d", s, origin.D);
        result.WriteCell ("x", s, origin.X != null ? origin.X.I : -1);
        result.WriteCell ("y", s, origin.Y != null ? origin.Y.I : -1);
        result.Write (end - 1, s);
        stack.Push (origin);
        while (stack.Count > 0)
        {
          current = stack.Pop ();
          if (current.X != null) {
            s = new RCSymbolScalar (atom, current.X.I);
            result.WriteCell ("p", s, current.I);
            result.WriteCell ("d", s, current.X.D);
            result.WriteCell ("x", s, current.X.X != null ? current.X.X.I : -1);
            result.WriteCell ("y", s, current.X.Y != null ? current.X.Y.I : -1);
            result.Write (end - 1, s);
            stack.Push (current.X);
          }
          if (current.Y != null) {
            s = new RCSymbolScalar (atom, current.Y.I);
            result.WriteCell ("p", s, current.I);
            result.WriteCell ("d", s, current.Y.D);
            result.WriteCell ("x", s, current.Y.X != null ? current.Y.X.I : -1);
            result.WriteCell ("y", s, current.Y.Y != null ? current.Y.Y.I : -1);
            result.Write (end - 1, s);
            stack.Push (current.Y);
          }
        }
        ++b;
      }
      runner.Yield (closure, result);
    }
  }
}

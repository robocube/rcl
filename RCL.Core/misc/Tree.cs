
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Tree
  {
    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<byte> (root, right, ref root.n, ref root.g, Areax, Formatx);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Areax (byte r) { return (double) r; }
    protected string Formatx (byte r) { return r.ToString (); }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<double> (root, right, ref root.n, ref root.g, Aread, Formatd);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Aread (double r) { return r; }
    protected string Formatd (double r) { return string.Format ("{0:0.#}", r); }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<long> (root, right, ref root.n, ref root.g, Areal, Formatl);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Areal (long r) { return (double) r; }
    protected string Formatl (long r) { return r.ToString (); }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<string> (root, right, ref root.n, ref root.g, Areas, Formats);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Areas (string r) { return 1; }
    protected string Formats (string r) { return r; }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<decimal> (root, right, ref root.n, ref root.g, Aream, Formatm);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Aream (decimal r) { return (double) r; }
    protected string Formatm (decimal r) { return string.Format ("{0:0.#}", r); }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCBoolean right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<bool> (root, right, ref root.n, ref root.g, Areab, Formatb);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Areab (bool r) { return r ? 1 : 0; }
    protected string Formatb (bool r) { return r.ToString (); }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree<RCSymbolScalar> (root, right, ref root.n, ref root.g, Areay, Formaty);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }
    protected double Areay (RCSymbolScalar r) { return 1; }
    protected string Formaty (RCSymbolScalar r) { return r.ToString (); }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree (root, right, ref root.n, ref root.g);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }

    [RCVerb ("tree")]
    public void EvalTree (
      RCRunner runner, RCClosure closure, RCCube right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      TreeNode root = new TreeNode (RCSymbolScalar.Empty, RCSymbolScalar.Empty);
      root.children = new RCArray<TreeNode> ();
      DoTree (root, right, ref root.n, ref root.g);
      LayoutBottomUp (root, 0);
      LayoutTree (0, root);
      WriteTree (result, root);
      runner.Yield (closure, result);
    }

    protected class TreeNode
    {
      public TreeNode (RCSymbolScalar symbol, RCSymbolScalar name)
      {
        s = symbol;
        k = name;
      }

      public TreeNode (TreeNode parent, string name, int number)
      {
        s = new RCSymbolScalar (parent.s, (long) number);
        if (name == null || name.Equals (""))
        {
          k = new RCSymbolScalar (parent.k, (long) number);
        }
        else
        {
          k = new RCSymbolScalar (parent.k, name);
        }
        if (parent.children == null)
        {
          parent.children = new RCArray<TreeNode> (8);
        }
        parent.children.Write (this);
      }

      public string v = "";
      public double m = 0;
      public double n = 0;
      public double g = 0;
      public double r = 0;
      public double d = 0;
      public double e = 0;
      public double x = 0, y = 0;
      public RCSymbolScalar s, k;
      public RCArray<TreeNode> children;
    }

    protected void LayoutTree (double angle, TreeNode node)
    {
      if (node.children != null)
      {
        if (node.children.Count == 1)
        {
          angle += Math.PI;
          node.children[0].x = node.x + node.children[0].d * Math.Sin (angle);
          node.children[0].y = node.y + node.children[0].d * Math.Cos (angle);
          LayoutTree (angle, node.children[0]);
        }
        else if (node.children.Count == 2)
        {
          angle += Math.PI;
          node.children[0].x = node.x + node.children[0].d * Math.Sin (angle);
          node.children[0].y = node.y + node.children[0].d * Math.Cos (angle);
          LayoutTree (angle, node.children[0]);

          angle += Math.PI;
          node.children[1].x = node.x + node.children[1].d * Math.Sin (angle);
          node.children[1].y = node.y + node.children[1].d * Math.Cos (angle);
          LayoutTree (angle, node.children[1]);
        }
        else
        {
          double delta = (2 * Math.PI) / node.children.Count;
          angle += node.children.Count % 2 == 1 ? delta / 2 : 0;
          for (int i = 0; i < node.children.Count; ++i)
          {
            angle += delta;
            node.children[i].x = node.x + (node.children[i].d) * Math.Sin (angle);
            node.children[i].y = node.y + (node.children[i].d) * Math.Cos (angle);
            LayoutTree (angle, node.children[i]);
          }
        }
      }
    }

    protected static double C = (Math.PI * Math.PI) / 8;
    protected void LayoutBottomUp (TreeNode node, int level)
    {
      node.r = Math.Sqrt (Math.Abs (node.g) / Math.PI);
      node.e = 0;
      if (node.children != null)
      {
        double delta = (2 * Math.PI) / node.children.Count;
        if (node.children.Count == 1)
        {
          LayoutBottomUp (node.children[0], level + 1);
          node.e = 2 * node.children[0].r + .5 * Math.PI * node.children[0].e;
          node.children[0].d = node.r + node.children[0].r + node.children[0].e;
          node.children[0].d = Math.Max (node.children[0].d,
                                         (node.children[0].r + node.children[0].e) / Math.Tan (delta / 2d));
        }
        else
        {
          for (int i = 0; i < node.children.Count; ++i)
          {
            LayoutBottomUp (node.children[i], level + 1);
            int times = (node.children.Count - 1) / 8;
            for (int j = 0; j < times; ++j)
            {
              node.children[i].e += node.children[i].e;
            }
            node.e = Math.Max (node.e, 2 * node.children[i].r + C * node.children[i].e);
            node.children[i].d = node.r + node.children[i].r + C * node.children[i].e;
            node.children[i].d = Math.Max (node.children[i].d,
                                           (node.children[i].r + C * node.children[i].e) / Math.Tan (delta / 2d));
          }
        }
      }
    }

    protected void WriteTree (RCCube result, TreeNode node)
    {
      result.WriteCell ("x", node.s, node.x);
      result.WriteCell ("y", node.s, node.y);
      result.WriteCell ("k", node.s, node.k);
      result.WriteCell ("v", node.s, node.v);
      result.WriteCell ("m", node.s, node.m);
      result.WriteCell ("n", node.s, node.n);
      result.WriteCell ("g", node.s, node.g);
      result.Axis.Write (node.s);
      if (node.children != null)
      {
        for (int i = 0; i < node.children.Count; ++i)
        {
          TreeNode child = node.children[i];
          WriteTree (result, child);
        }
      }
    }

    protected delegate double Area<T> (T r);
    protected delegate string Format<T> (T r);
    protected void DoTree<T> (TreeNode parent, RCVector<T> right, ref double a, ref double g, Area<T> area, Format<T> format)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        TreeNode child = new TreeNode (parent, null, i);
        child.n = area (right[i]);
        child.m = child.n;
        child.g = Math.Abs (child.n);
        child.v = format (right[i]);
        a += child.n;
        g += child.g;
      }
    }

    protected void DoTree (TreeNode parent, RCReference right, ref double a, ref double g)
    {
      TreeNode child = new TreeNode (parent, null, 0);
      child.n = 1;
      child.m = 1;
      child.g = 1;
      child.v = right.ToString ();
      a += child.n;
      g += Math.Abs (child.n);
    }

    protected void DoTree (TreeNode parent, RCOperator right, ref double a, ref double g)
    {
      //This is not quite correct. Operators should have more space allocated like blocks.
      //And not always assigned an area of one. TestTree16 and others will have to change.
      TreeNode opNode = new TreeNode (parent, null, 0);
      opNode.n = 1;
      opNode.m = 1;
      opNode.g = 1;
      opNode.v = right.ToString ();
      a += opNode.n;
      g += Math.Abs (opNode.n);
    }

    protected void DoTree (TreeNode parent, RCCube right, ref double a, ref double g)
    {
      if (right.Axis.ColCount > 1)
      {
        //throw new NotImplementedException ("Cannot handle time cols on cubes yet.");
        //But we will still handle them as if the time col didn't exist.
      }
      if (right.Axis.Symbol != null)
      {
        Dictionary<RCSymbolScalar, TreeNode> map = new Dictionary<RCSymbolScalar, TreeNode> ();
        for (int i = 0; i < right.Cols; ++i)
        {
          string colName = right.ColumnAt (i);
          TreeNode colNode = new TreeNode (parent, colName, i);
          colNode.v = colName;
          colNode.n = 0;
          ColumnBase col = right.GetColumn (i);
          bool numeric = typeof (long).IsAssignableFrom (col.GetElementType());
          for (int j = 0; j < col.Count; ++j)
          {
            RCSymbolScalar symbol = right.Axis.SymbolAt (col.Index[j]);
            TreeNode rowNode = new TreeNode (colNode, symbol.Key.ToString (), col.Index[j]);
            object box = col.BoxCell (j);
            if (numeric)
            {
              rowNode.n = (long) box;
              rowNode.m = (long) box;
              rowNode.g = Math.Abs ((long) box);
            }
            else
            {
              rowNode.n = 1;
              rowNode.m = 1;
              rowNode.g = 1;
            }
            rowNode.v = box.ToString ();
            colNode.n += rowNode.n;
            colNode.g += Math.Abs (rowNode.n);
            map[rowNode.s] = rowNode;
          }
          a += colNode.n;
          g += Math.Abs (colNode.g);
        }
      }
      else
      {
        for (int i = 0; i < right.Cols; ++i)
        {
          string colName = right.ColumnAt (i);
          TreeNode colNode = new TreeNode (parent, colName, i);
          colNode.v = colName;
          colNode.n = 0;
          ColumnBase col = right.GetColumn (i);
          bool numeric = typeof (long).IsAssignableFrom (col.GetElementType());
          for (int j = 0; j < right.Count; ++j)
          {
            TreeNode rowNode = new TreeNode (colNode, null, col.Index[j]);
            object box = col.BoxCell (j);
            if (numeric)
            {
              rowNode.n = (long) box;
              rowNode.m = (long) box;
              rowNode.g = Math.Abs ((long) box);
            }
            else
            {
              rowNode.n = 1;
              rowNode.m = 1;
              rowNode.g = 1;
            }
            rowNode.v = box.ToString ();
            colNode.n += rowNode.n;
            colNode.g += Math.Abs (rowNode.n);
          }
          a += colNode.n;
          g += Math.Abs (colNode.g);
        }
      }
    }

    protected void DoTree (TreeNode parent, RCBlock right, ref double a, ref double g)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock current = right.GetName (i);
        object shortName = current.Name;
        if (shortName.Equals (""))
        {
          shortName = (long) i;
        }
        RCSymbolScalar s = new RCSymbolScalar (parent.s, (long) i);
        RCSymbolScalar n = new RCSymbolScalar (parent.k, shortName);
        TreeNode node = new TreeNode (s, n);
        node.v = current.Name;
        node.children = new RCArray<TreeNode> ();
        RCVectorBase vector = current.Value as RCVectorBase;
        if (vector != null)
        {
          switch (vector.TypeCode)
          {
            case 'l' : DoTree<long> (node, (RCVector<long>) vector, ref node.n, ref node.g, Areal, Formatl); break;
            case 'd' : DoTree<double> (node, (RCVector<double>) vector, ref node.n, ref node.g, Aread, Formatd); break;
            case 'm' : DoTree<decimal> (node, (RCVector<decimal>) vector, ref node.n, ref node.g, Aream, Formatm); break;
            case 's' : DoTree<string> (node, (RCVector<string>) vector, ref node.n, ref node.g, Areas, Formats); break;
            case 'x' : DoTree<byte> (node, (RCVector<byte>) vector, ref node.n, ref node.g, Areax, Formatx); break;
            case 'y' : DoTree<RCSymbolScalar> (node, (RCVector<RCSymbolScalar>) vector, ref node.n, ref node.g, Areay, Formaty); break;
            case 'b' : DoTree<bool> (node, (RCVector<bool>) vector, ref node.n, ref node.g, Areab, Formatb); break;
            default : throw new Exception ("Unknown typecode: " + vector.TypeCode);
          }
          a += node.n;
          g += Math.Abs (node.g);
          parent.children.Write (node);
          continue;
        }
        RCBlock block = current.Value as RCBlock;
        if (block != null)
        {
          DoTree (node, block, ref node.n, ref node.g);
          a += node.n;
          g += Math.Abs (node.g);
          parent.children.Write (node);
          continue;
        }
        RCCube cube = current.Value as RCCube;
        if (cube != null)
        {
          DoTree (node, cube, ref node.n, ref node.g);
          a += node.n;
          g += Math.Abs (node.g);
          parent.children.Write (node);
          continue;
        }
        RCOperator oper = current.Value as RCOperator;
        if (oper != null)
        {
          DoTree (node, oper, ref node.n, ref node.g);
          a += node.n;
          g += Math.Abs (node.g);
          parent.children.Write (node);
          continue;
        }
        RCReference reference = current.Value as RCReference;
        if (reference != null)
        {
          DoTree (node, reference, ref node.n, ref node.g);
          a += node.n;
          g += Math.Abs (node.g);
          parent.children.Write (node);
          continue;
        }
      }
    }
  }
}

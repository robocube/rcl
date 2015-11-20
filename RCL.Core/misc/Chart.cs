using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using RCL.Kernel;

namespace RCL.Core
{
  public class Chart
  {
    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCByte right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<byte> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCDouble right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<double> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCLong right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<long> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCString right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<string> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCDecimal right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<decimal> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCBoolean right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<bool> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCSymbol right)
    {
      RCCube result = new RCCube (new RCArray<string> ("S"));
      DoChart<RCSymbolScalar> (result, RCSymbolScalar.Empty, 0, 0, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCBlock right)
    {
      RCSymbolScalar parent = RCSymbolScalar.Empty;
      RCCube result = new RCCube (new RCArray<string> ("S"));
      long row = 0;
      long col = 0;
      DoChart (result, parent, ref row, col, right);
      runner.Yield (closure, result);
    }

    [RCVerb ("chart")]
    public void EvalChart (
      RCRunner runner, RCClosure closure, RCCube right)
    {
      RCSymbolScalar parent = RCSymbolScalar.Empty;
      RCCube result = new RCCube (new RCArray<string> ("S"));
      long row = 0;
      long col = 0;
      DoChart (result, parent, ref row, col, right);
      runner.Yield (closure, result);
    }

    protected void DoChart (RCCube result, RCSymbolScalar parent, ref long row, long col, RCBlock right)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        RCBlock current = right.GetName (i);
        object shortName = current.Name;
        if (shortName.Equals (""))
        {
          shortName = (long) i;
        }
        RCSymbolScalar name = new RCSymbolScalar (parent, shortName);
        RCSymbolScalar cell = RCSymbolScalar.From (row, col, 0L);
        result.WriteCell ("r", cell, row);
        result.WriteCell ("c", cell, col);
        result.WriteCell ("l", cell, 0L);
        result.WriteCell ("k", cell, name);
        result.WriteCell ("v", cell, shortName is long ? shortName.ToString () : shortName);
        result.Axis.Write (cell);
        RCVectorBase vector = current.Value as RCVectorBase;
        if (vector != null)
        {
          ++col;
          switch (vector.TypeCode)
          {
            case 'l' : DoChart<long> (result, name, row, col,  (RCVector<long>) vector); break;
            case 'd' : DoChart<double> (result, name, row, col, (RCVector<double>) vector); break;
            case 'm' : DoChart<decimal> (result, name, row, col, (RCVector<decimal>) vector); break;
            case 's' : DoChart<string> (result, name, row, col, (RCVector<string>) vector); break;
            case 'x' : DoChart<byte> (result, name, row, col, (RCVector<byte>) vector); break;
            case 'y' : DoChart<RCSymbolScalar> (result, name, row, col, (RCVector<RCSymbolScalar>) vector); break;
            case 'b' : DoChart<bool> (result, name, row, col, (RCVector<bool>) vector); break;
            default : throw new Exception ("Unknown typecode: " + vector.TypeCode);
          }
          --col;
          ++row;
          continue;
        }
        RCBlock block = current.Value as RCBlock;
        if (block != null)
        {
          ++col;
          ++row;
          DoChart (result, name, ref row, col, block);
          --col;
          continue;
        }
        RCOperator oper = current.Value as RCOperator;
        if (oper != null)
        {
          ++col;
          string val = oper.ToString ();
          cell = RCSymbolScalar.From (row, col, 0L);
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, new RCSymbolScalar (name, 0L));
          result.WriteCell ("v", cell, val);
          result.Write (cell);
          ++row;
          --col;
          continue;
        }
        RCLReference reference = current.Value as RCLReference;
        if (reference != null)
        {
          ++col;
          string val = reference.ToString ();
          cell = RCSymbolScalar.From (row, col, 0L);
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, new RCSymbolScalar (name, 0L));
          result.WriteCell ("v", cell, val);
          result.Write (cell);
          ++row;
          --col;
          continue;
        }
        RCCube cube = current.Value as RCCube;
        if (cube != null)
        {
          ++col;
          ++row;
          DoChart (result, name, ref row, col, cube);
          ++row;
          --col;
          continue;
        }
      }
    }

    protected void DoChart (RCCube result, RCSymbolScalar name, 
                            ref long row, long col, RCCube cube)
    {
      for (int i = 0; i < cube.Cols; ++i)
      {
        string colname = cube.NameAt (i);
        ColumnBase data = cube.GetColumn (i);
        RCSymbolScalar cell = RCSymbolScalar.From (row, (long) col, 0L);
        RCSymbolScalar parent = new RCSymbolScalar (name, colname);
        result.WriteCell ("r", cell, (long) row);
        result.WriteCell ("c", cell, (long) col);
        result.WriteCell ("l", cell, 0L);
        result.WriteCell ("k", cell, parent);
        result.WriteCell ("v", cell, colname);
        result.Write (cell);

        for (int j = 0; j < data.Count; ++j)
        {
          string val = data.BoxCell (j).ToString ();
          cell = RCSymbolScalar.From (row, (long) col + data.Index [j] + 1, 0L);
          RCSymbolScalar child = new RCSymbolScalar (parent, (long) j);
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col + data.Index [j] + 1);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, child);
          result.WriteCell ("v", cell, val);
          result.Write (cell);
        }
        ++row;
      }
      if (cube.Axis.Global != null)
      {
        RCSymbolScalar cell = RCSymbolScalar.From (row, col, 0L);
        RCSymbolScalar parent = new RCSymbolScalar (name, "G");
        result.WriteCell ("r", cell, (long) row);
        result.WriteCell ("c", cell, (long) col);
        result.WriteCell ("l", cell, 0L);
        result.WriteCell ("k", cell, parent);
        result.WriteCell ("v", cell, "G");
        result.Write (cell);
        for (int i = 0; i < cube.Axis.Global.Count; ++i)
        {
          string val = cube.Axis.Global [i].ToString ();
          cell = RCSymbolScalar.From (row, col + i + 1, 0L);
          RCSymbolScalar child = new RCSymbolScalar (parent, (long) i); 
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col + i + 1);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, child);
          result.WriteCell ("v", cell, val);
          result.Write (cell);
        }
        ++row;
      }
      if (cube.Axis.Event != null)
      {
        RCSymbolScalar cell = RCSymbolScalar.From (row, col, 0L);
        RCSymbolScalar parent = new RCSymbolScalar (name, "E");
        result.WriteCell ("r", cell, (long) row);
        result.WriteCell ("c", cell, (long) col);
        result.WriteCell ("l", cell, 0L);
        result.WriteCell ("k", cell, parent);
        result.WriteCell ("v", cell, "E");
        result.Write (cell);
        for (int i = 0; i < cube.Axis.Event.Count; ++i)
        {
          string val = cube.Axis.Event [i].ToString ();
          cell = RCSymbolScalar.From (row, col + i + 1, 0L);
          RCSymbolScalar child = new RCSymbolScalar (parent, (long) i); 
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col + i + 1);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, child);
          result.WriteCell ("v", cell, val);
          result.Write (cell);
        }
        ++row;
      }
      if (cube.Axis.Time != null)
      {
        RCSymbolScalar cell = RCSymbolScalar.From (row, col, 0L);
        RCSymbolScalar parent = new RCSymbolScalar (name, "T");
        result.WriteCell ("r", cell, (long) row);
        result.WriteCell ("c", cell, (long) col);
        result.WriteCell ("l", cell, 0L);
        result.WriteCell ("k", cell, parent);
        result.WriteCell ("v", cell, "T");
        result.Write (cell);
        for (int i = 0; i < cube.Axis.Time.Count; ++i)
        {
          string val = cube.Axis.Time [i].ToString ();
          cell = RCSymbolScalar.From (row, col + i + 1, 0L);
          RCSymbolScalar child = new RCSymbolScalar (parent, (long) i); 
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col + i + 1);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, child);
          result.WriteCell ("v", cell, val);
          result.Write (cell);
        }
        ++row;
      }
      if (cube.Axis.Symbol != null)
      {
        RCSymbolScalar cell = RCSymbolScalar.From (row, col, 0L);
        RCSymbolScalar parent = new RCSymbolScalar (name, "S");
        result.WriteCell ("r", cell, (long) row);
        result.WriteCell ("c", cell, (long) col);
        result.WriteCell ("l", cell, 0L);
        result.WriteCell ("k", cell, parent);
        result.WriteCell ("v", cell, "S");
        result.Write (cell);
        for (int i = 0; i < cube.Axis.Symbol.Count; ++i)
        {
          string val = cube.Axis.Symbol [i].ToString ();
          cell = RCSymbolScalar.From (row, col + i + 1, 0L);
          RCSymbolScalar child = new RCSymbolScalar (parent, (long) i); 
          result.WriteCell ("r", cell, (long) row);
          result.WriteCell ("c", cell, (long) col + i + 1);
          result.WriteCell ("l", cell, 0L);
          result.WriteCell ("k", cell, child);
          result.WriteCell ("v", cell, val);
          result.Write (cell);
        }
        ++row;
      }
    }

    protected void DoChart<T> (RCCube result, RCSymbolScalar name, 
                               long row, long col, RCVector<T> right)
    {
      for (int i = 0; i < right.Count; ++i)
      {
        string val = right[i].ToString ();
        RCSymbolScalar s = RCSymbolScalar.From (row, (long) col + i, 0L);
        RCSymbolScalar k = new RCSymbolScalar (name, (long) i);
        result.WriteCell ("r", s, row);
        result.WriteCell ("c", s, (long) col + i);
        result.WriteCell ("l", s, 0L);
        result.WriteCell ("k", s, k);
        result.WriteCell ("v", s, val);
        result.Write (s);
      }
    }
  }
}
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCCube : RCValue, IRefable
  {
    public static readonly RCCube Empty = new RCCube ();
    public readonly Timeline Axis;
    protected RCArray<string> m_names;
    protected RCArray<ColumnBase> m_columns;

    //These variables are not always initialized in the ctor.
    //It's a little messy, is there something we can do?
    protected Reader m_reader;
    protected long m_lines;

    public RCCube ()
    {
      m_names = new RCArray<string> ();
      Axis = new Timeline (null,
                           new RCArray<long> (),
                           null,
                           new RCArray<RCSymbolScalar> ());
      m_columns = new RCArray<ColumnBase> ();
    }

    public RCCube (params string[] tlcolnames)
    {
      m_names = new RCArray<string> ();
      Axis = new Timeline (tlcolnames);
      m_columns = new RCArray<ColumnBase> ();
    }

    public RCCube (RCArray<string> tlcolnames)
    {
      m_names = new RCArray<string> ();
      Axis = new Timeline (tlcolnames);
      m_columns = new RCArray<ColumnBase> ();
    }

    //Sometimes you want to create a cube starting with a prexisting timeline.
    public RCCube (Timeline timeline)
      :this (timeline, new RCArray<string> (), new RCArray<ColumnBase> ()) {}

    public RCCube (RCCube cube)
    {
      Axis = cube.Axis.Match ();
      Axis.Count = cube.Count;
      m_names = new RCArray<string> ();
      m_columns = new RCArray<ColumnBase> ();
      if (Axis.Global != null)
      {
        Axis.Global.Write (cube.Axis.Global);
      }
      if (Axis.Event != null)
      {
        Axis.Event.Write (cube.Axis.Event);
      }
      if (Axis.Time != null)
      {
        Axis.Time.Write (cube.Axis.Time);
      }
      if (Axis.Symbol != null)
      {
        Axis.Symbol.Write (cube.Axis.Symbol);
      }
      for (int i = 0; i < cube.Cols; ++i)
      {
        ColumnBase oldcol = cube.GetColumn (i);
        ColumnBase newcol = ColumnBase.FromArray (
          Axis, oldcol.Index, oldcol.Array);
        m_columns.Write (newcol);
        m_names.Write (cube.NameAt (i));
      }
    }

    public RCCube (Timeline timeline,
                   RCArray<string> names,
                   RCArray<ColumnBase> columns)
    {
      if (timeline == null)
      {
        throw new ArgumentNullException ("timeline");
      }
      if (names == null)
      {
        throw new ArgumentNullException ("names");
      }
      if (columns == null)
      {
        throw new ArgumentNullException ("columns");
      }
      Axis = timeline;
      m_names = names;
      m_columns = columns;
    }

    /// <summary>
    /// A block representation of the internals of the cube.
    /// Used for debugging.
    /// </summary>
    public RCBlock FlatPack ()
    {
      RCBlock result = RCBlock.Empty;
      if (Axis.ColCount > 0)
      {
        RCBlock axis = RCBlock.Empty;
        if (Axis.Global != null)
        {
          axis = new RCBlock ("G", ":", new RCLong (Axis.Global));
        }
        if (Axis.Event != null)
        {
          axis = new RCBlock ("E", ":", new RCLong (Axis.Event));
        }
        if (Axis.Time != null)
        {
          axis = new RCBlock ("T", ":", new RCTime (Axis.Time));
        }
        if (Axis.Symbol != null)
        {
          axis = new RCBlock ("S", ":", new RCSymbol (Axis.Symbol));
        }
        result = new RCBlock ("", ":", axis);
      }
      for (int i = 0; i < m_columns.Count; ++i)
      {
        RCBlock column = RCBlock.Empty;
        column = new RCBlock (column, "index", ":", RCVectorBase.FromArray (m_columns[i].Index));
        column = new RCBlock (column, "array", ":", RCVectorBase.FromArray (m_columns[i].Array));
        result = new RCBlock (result, m_names[i], ":", column);
      }
      return result;
    }

    public int FirstRow
    {
      get
      {
        int first = 0;
        for (int i = 0; i < m_columns.Count; ++i)
        {
          if (m_columns[i] != null && m_columns[i].Count > 0)
          {
            if (m_columns[i].Index[0] < first)
            {
              first = m_columns[i].Index[0];
            }
          }
        }
        return first;
      }
    }

    public int LastRow
    {
      get
      {
        if (m_columns.Count == 0)
        {
          return -1;
        }
        int last = 0;
        for (int i = 0; i < m_columns.Count; ++i)
        {
          if (m_columns[i] != null && m_columns[i].Count > 0)
          {
            int lastInCol = m_columns[i].Index[m_columns[i].Count - 1];
            if (lastInCol > last)
            {
              last = lastInCol;
            }
          }
        }
        return last;
      }
    }

    public void Write (RCSymbolScalar s)
    {
      Axis.Write (s);
    }

    public void Write (long t, RCSymbolScalar s)
    {
      Axis.Write (t, s);
    }

    public void Write (long g, long e, RCTimeScalar t, RCSymbolScalar s)
    {
      Axis.Write (g, e, t, s);
    }

    public class ColumnOfNothing : Column<object>
    {
      public ColumnOfNothing (Timeline timeline) : base (timeline) {}

      public ColumnOfNothing (Timeline timeline, RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override bool Write (RCSymbolScalar key, int index, object box, bool force)
      {
        m_data.Write (box);
        m_index.Write (index);
        return true;
      }

      public override Type GetElementType ()
      {
        return typeof (object);
      }

      public override void Lock ()
      {
        m_data.Lock ();
        m_index.Lock ();
      }

      public override void ReverseInPlace (int tlcount)
      {
        m_data.ReverseInPlace ();
        //m_index.ReverseInPlace ();
      }

      public override void Accept (string name, Visitor visitor, int i)
      {
        visitor.VisitNull<object> (name, this, i);
      }

      public override void AcceptNull (string name, Visitor visitor, int i)
      {
        visitor.VisitNull<object> (name, this, i);
      }

      public new bool Last (RCSymbolScalar key, out object val)
      {
        val = null;
        return false;
      }

      public override string ScalarToString (string format, int i)
      {
        throw new NotImplementedException ();
      }

      public override string ScalarToCsvString (string format, int vrow)
      {
        throw new NotImplementedException ();
      }

      public override char TypeCode { get { return '0'; } }
      public override object BoxCell (int i) { return null; }
      public override object Array { get { return m_data; } }
      public override RCArray<int> Index { get { return m_index; } }
      public new RCArray<object> Data { get { return m_data; } }
      public override int Count { get { return 0; } }
    }

    public class ColumnOfByte : Column<byte>
    {
      public ColumnOfByte (Timeline timeline)
        : base (timeline) {}

      public ColumnOfByte (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCByte.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 'x'; } }
    }

    public class ColumnOfLong : Column<long>
    {
      public ColumnOfLong (Timeline timeline)
        : base (timeline) {}

      public ColumnOfLong (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCLong.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 'l'; } }
    }

    public class ColumnOfDouble : Column<double>
    {
      public ColumnOfDouble (Timeline timeline) : base (timeline) {}

      public ColumnOfDouble (Timeline timeline,
                             RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCDouble.FormatScalar (format, m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 'd'; } }
    }

    public class ColumnOfDecimal : Column<decimal>
    {
      public ColumnOfDecimal (Timeline timeline) : base (timeline) {}

      public ColumnOfDecimal (Timeline timeline,
                              RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCDecimal.FormatScalar (format, m_data[i]) + "m";
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 'm'; } }
    }

    public class ColumnOfBool : Column<bool>
    {
      public ColumnOfBool (Timeline timeline) : base (timeline) {}

      public ColumnOfBool (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCBoolean.FormatScalar (format, m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 'b'; } }
    }

    public class ColumnOfString : Column<string>
    {
      public ColumnOfString (Timeline timeline) : base (timeline) {}

      public ColumnOfString (Timeline timeline,
                             RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCString.FormatScalar (format, m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return m_data[i].ToString ();
      }

      public override char TypeCode { get { return 's'; } }
    }

    public class ColumnOfSymbol : Column<RCSymbolScalar>
    {
      public ColumnOfSymbol (Timeline timeline) : base (timeline) {}

      public ColumnOfSymbol (Timeline timeline,
                             RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCSymbol.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return m_data[i].ToCsvString ();
      }

      public override char TypeCode { get { return 'y'; } }
    }

    public class ColumnOfTime : Column<RCTimeScalar>
    {
      public ColumnOfTime (Timeline timeline) : base (timeline) {}

      public ColumnOfTime (Timeline timeline,
                           RCArray<int> index,
                           object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCTime.FormatScalar (format, m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 't'; } }
    }

    public class ColumnOfIncr : Column<RCIncrScalar>
    {
      public ColumnOfIncr (Timeline timeline) : base (timeline) {}

      public ColumnOfIncr (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (string format, int i)
      {
        return RCIncr.FormatScalar (format, m_data[i]);
      }

      public override string ScalarToCsvString (string format, int i)
      {
        return ScalarToString (format, i);
      }

      public override char TypeCode { get { return 'n'; } }
    }

    public override bool Equals (object obj)
    {
      if (obj == null)
      {
        return false;
      }
      if (obj == this)
      {
        return true;
      }
      RCCube cobj = obj as RCCube;
      if (cobj == null)
      {
        return false;
      }
      //if (cobj.Count != Count) return false;
      //Come on let's write a real comparison routine.
      //Let's use RCVisitor.
      string thisString = Format (RCL.Kernel.RCFormat.DefaultNoT);
      string otherString = cobj.Format (RCL.Kernel.RCFormat.DefaultNoT);
      return thisString.Equals (otherString);
    }

    public override void Lock ()
    {
      Axis.Lock ();
      if (m_reader != null)
      {
        m_reader.Lock ();
      }
      RCArray<int> missingCols = new RCArray<int> ();
      for (int i = 0; i < m_columns.Count; ++i)
      {
        if (m_columns[i] == null)
        {
          m_columns.Write (i, new ColumnOfNothing (Axis));
        }
        else
        {
          m_columns[i].Lock ();
        }
      }
      //This happens because of the way cubes are parsed,
      //As they are read in we use WriteCell and something called ReserveColumn
      //Reserve columns holds the order for a column whose initial value is null,
      //But if nothing comes in later those empty columns need to be removed or bad things happen.
      for (int i = missingCols.Count - 1; i >= 0; --i)
      {
        m_columns.RemoveAt (missingCols[i]);
        m_names.RemoveAt (missingCols[i]);
      }
      m_columns.Lock ();
      m_names.Lock ();
      base.m_lock = true;
    }

    public override int GetHashCode ()
    {
      return base.GetHashCode ();
    }

    public string ColumnAt (int i)
    {
      return m_names[i];
    }

    public long Lines
    {
      get { return m_lines; }
    }

    //This returns the line numbers from the source data set.
    public RCArray<int> AcceptedLines
    {
      get { return m_reader.AcceptedLines; }
    }

    //This returns the actual symbols represented in the result set.
    public RCArray<RCSymbolScalar> AcceptedSymbols
    {
      get { return m_reader.AcceptedSymbols; }
    }

    public override void Format (StringBuilder builder, RCFormat args, int level)
    {
      new Formatter (builder, args, null, level).Format (this);
    }

    public override void Format (StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      new Formatter (builder, args, colmap, level).Format (this);
    }

    public override string TypeName
    {
      get { return CUBE_TYPENAME; }
    }

    public override char TypeCode
    {
      get { return 'u'; }
    }

    public override bool IsCube
    {
      get { return true; }
    }

    public override void ToByte (RCArray<byte> result)
    {
      //Timeline
      result.Write ((byte) 'u');
      result.Write ((byte) Axis.Count);
      result.Write ((byte) (Axis.Count << 8));
      result.Write ((byte) (Axis.Count << 16));
      result.Write ((byte) (Axis.Count << 24));

      RCArray<string> tlcols = new RCArray<string> (4);
      if (Axis.Global != null) tlcols.Write ("G");
      if (Axis.Event != null) tlcols.Write ("E");
      if (Axis.Time != null) tlcols.Write ("T");
      if (Axis.Symbol != null) tlcols.Write ("S");
      Binary.WriteVectorString (result, tlcols);

      if (Axis.Global != null)
      {
        Binary.WriteVector<long> (result, Axis.Global, sizeof (long));
      }
      if (Axis.Event != null)
      {
        Binary.WriteVector<long> (result, Axis.Event, sizeof (long));
      }
      if (Axis.Time != null)
      {
        Binary.WriteVectorTime (result, Axis.Time);
      }
      if (Axis.Symbol != null)
      {
        Binary.WriteVectorSymbol (result, Axis.Symbol);
      }
      Binary.WriteVectorString (result, m_names);
      for (int i = 0; i < m_columns.Count; ++i)
      {
        result.Write ((byte) m_columns[i].TypeCode);
        Binary.WriteVector<int> (result, m_columns[i].Index, sizeof (int));
        switch (m_columns[i].TypeCode)
        {
          case 'x': Binary.WriteVector<byte> (result, ((Column<byte>) m_columns[i]).Data, sizeof (byte)); break;
          case 'd': Binary.WriteVector<double> (result, ((Column<double>) m_columns[i]).Data, sizeof (double)); break;
          case 'l': Binary.WriteVector<long> (result, ((Column<long>) m_columns[i]).Data, sizeof (long)); break;
          case 'm': Binary.WriteVectorDecimal (result, ((Column<decimal>) m_columns[i]).Data); break;
          case 'b': Binary.WriteVector<bool> (result, ((Column<bool>) m_columns[i]).Data, sizeof (bool)); break;
          case 'n': Binary.WriteVectorIncr (result, ((Column<RCIncrScalar>) m_columns[i]).Data); break;
          case 's': Binary.WriteVectorString (result, ((Column<string>) m_columns[i]).Data); break;
          case 'y': Binary.WriteVectorSymbol (result, ((Column<RCSymbolScalar>) m_columns[i]).Data); break;
          default: throw new Exception ("Cannot WriteVector with type " + m_columns[i].TypeCode);
        }
      }
    }

    public RCValue Get (string[] name, RCArray<RCBlock> @this)
    {
      return Get (name[0]);
    }

    public RCValue Get (RCArray<string> name, RCArray<RCBlock> @this)
    {
      return Get (name, @this);
    }

    public RCValue Get (string name)
    {
      int column = m_names.IndexOf (RCName.Get (name));
      if (column < 0)
      {
        //Special columns in the timeline.
        //We need tests for G and E
        if (name == "G")
        {
          return new RCLong (Axis.Global);
        }
        if (name == "E")
        {
          return new RCLong (Axis.Event);
        }
        else if (name == "T")
        {
          return new RCTime (Axis.Time);
        }
        else if (name == "S")
        {
          return new RCSymbol (Axis.Symbol);
        }
        else
        {
          return new RCCube (Axis, new RCArray<string> (name), new RCArray<ColumnBase> (new ColumnOfNothing (Axis)));
        }
      }
      else
      {
        //This is not quite right yet.
        //You could end up with nulls in the output.
        //It should create a new timeline with times only
        //at the points referenced by the selected column.
        return new RCCube (Axis,
                           new RCArray<string> (name),
                           new RCArray<ColumnBase> (m_columns[column]));
      }
    }

    public ColumnBase GetColumn (string name)
    {
      int column = m_names.IndexOf (RCName.Get (name));
      if (column < 0)
      {
        return null;
      }
      return m_columns[column];
    }

    public RCValue Get (long index)
    {
      //Would it be better to return a cube here?
      return GetSimpleVector ((int) index);
    }

    public ColumnBase GetColumn (int index)
    {
      if (index < 0 || index >= m_columns.Count)
      {
        return null;
      }
      else
      {
        return m_columns[index];
      }
    }

    public Type GetType (int index)
    {
      return m_columns[index].GetElementType ();
    }

    public RCArray<T> DoColof<T> (string name, T def)
    {
      int col = FindColumn (name);
      if (col < 0)
      {
        throw new Exception (string.Format ("Unknown column: {0}", name));
      }
      return DoColof<T> (col, def, true);
    }

    public RCArray<T> DoColof<T> (int col, T def, bool allowSparse)
    {
      if (this.Count == 0)
      {
        return new RCArray<T> (1);
      }
      RCArray<T> data = this.GetData<T> (col);
      RCArray<T> result;
      if (data.Count < this.Axis.Count)
      {
        if (!allowSparse)
        {
          throw new Exception ("There were missing values. Please specify a default value.");
        }
        result = new RCArray<T> (this.Axis.Count);
        RCArray<int> index = this.GetIndex<T> (col);
        for (int i = 0; i < index.Count; ++i)
        {
          while (result.Count < index[i])
          {
            result.Write (def);
          }
          result.Write (data[i]);
        }
        while (result.Count < this.Axis.Count)
        {
          result.Write (def);
        }
      }
      else
      {
        result = data;
      }
      return result;
    }

    public RCVectorBase GetSimpleVector (int index)
    {
      //We need to create vectors to wrap these arrays because
      //you cannot pass an array to an operator (since it isn't an RCValue).
      return RCVectorBase.FromArray (m_columns[index].Array);
    }

    public RCArray<int> GetIndex<T> (int index)
    {
      return ((Column<T>) m_columns[index]).Index;
    }

    /// <summary>
    /// Warning: These are sparse arrays, you need to call GetIndex to know
    /// which value goes with which entry in the timeline. Use Colof
    /// to avoid handling sparse data.
    /// </summary>
    public RCArray<T> GetData<T> (int index)
    {
      //Twould be cool if I could get rid of this cast.
      //But don't know how.
      return ((Column<T>) m_columns[index]).Data;
    }

    public char GetTypeCode (string name)
    {
      int column = m_names.IndexOf (name);
      if (m_columns.Count < 0)
      {
        if (name == "G")
        {
          return 'l';
        }
        if (name == "E")
        {
          return 'l';
        }
        if (name == "T")
        {
          return 't';
        }
        else if (name == "S")
        {
          return 'y';
        }
        else throw new Exception ("Unknown column \"" + name + "\"");
      }
      if (column >= m_columns.Count)
      {
        throw new Exception (string.Format ("Column with name {0} not found in cube.", name));
      }
      else
      {
        if (m_columns[column] == null)
        {
          return '~';
        }
        else return m_columns[column].TypeCode;
      }
    }

    public int FindColumn (string name)
    {
      return m_names.IndexOf (name);
    }

    public bool Has (string name)
    {
      if (Axis.Has (name))
      {
        return true;
      }
      else return FindColumn (name) > -1;
    }

    public override int Count
    {
      get { return Axis.Count; }
    }

    public int Rows
    {
      get
      {
        int last = LastRow;
        if (last < 0)
        {
          return 0;
        }
        int first = FirstRow;
        return 1 + (last - first);
      }
    }

    public long Cols
    {
      get { return m_names.Count; }
    }

    public string NameAt (int i)
    {
      return m_names[i];
    }

    public RCSymbolScalar SymbolAt (int i)
    {
      return Axis.SymbolAt (i);
    }

    public RCArray<string> Names
    {
      get { return m_names; }
    }

    public RCArray<ColumnBase> Columns
    {
      get { return m_columns; }
    }

    public RCCube Untl ()
    {
      RCArray<string> names = new RCArray<string> ();
      RCArray<ColumnBase> columns = new RCArray<ColumnBase> ();
      Timeline timeline = new Timeline (Axis.Count);
      if (Axis.Has ("G"))
      {
        names.Write ("G");
        columns.Write (new ColumnOfLong (timeline,
                                         StuffIndex (Axis.Count), Axis.Global));
      }
      if (Axis.Has ("E"))
      {
        names.Write ("E");
        columns.Write (new ColumnOfLong (timeline,
                                         StuffIndex (Axis.Count), Axis.Event));
      }
      if (Axis.Has ("T"))
      {
        names.Write ("T");
        columns.Write (new ColumnOfTime (timeline,
                                         StuffIndex (Axis.Count), Axis.Time));
      }
      if (Axis.Has ("S"))
      {
        names.Write ("S");
        columns.Write (new ColumnOfSymbol (timeline,
                                           StuffIndex (Axis.Count), Axis.Symbol));
      }
      for (int i = 0; i < Cols; ++i)
      {
        names.Write (NameAt (i));
        columns.Write (GetColumn (i));
      }
      RCCube result = new RCCube (timeline, names, columns);
      return result;
    }

    public RCCube Retimeline (RCArray<string> tlcols)
    {
      RCArray<string> names = new RCArray<string> ();
      RCArray<ColumnBase> columns = new RCArray<ColumnBase> ();
      HashSet<string> cols = new HashSet<string> (tlcols);
      RCArray<long> G = cols.Contains ("G") ? Axis.Global : null;
      RCArray<long> E = cols.Contains ("E") ? Axis.Event : null;
      RCArray<RCTimeScalar> T = cols.Contains ("T") ? Axis.Time : null;
      RCArray<RCSymbolScalar> S = cols.Contains ("S") ? Axis.Symbol : null;
      for (int i = 0; i < m_columns.Count; ++i)
      {
        if (m_names[i] == "G" && cols.Contains ("G"))
        {
          G = (RCArray<long>) m_columns[i].Array;
        }
        else if (m_names[i] == "E" && cols.Contains ("E"))
        {
          E = (RCArray<long>) m_columns[i].Array;
        }
        else if (m_names[i] == "T" && cols.Contains ("T"))
        {
          T = (RCArray<RCTimeScalar>) m_columns[i].Array;
        }
        else if (m_names[i] == "S" && cols.Contains ("S"))
        {
          S = (RCArray<RCSymbolScalar>) m_columns[i].Array;
        }
      }
      Timeline axis = new Timeline (G, E, T, S);
      for (int i = 0; i < m_columns.Count; ++i)
      {
        if (!tlcols.Contains (m_names[i]))
        {
          ColumnBase oldcol = m_columns[i];
          ColumnBase newcol = ColumnBase.FromArray (axis, oldcol.Index, oldcol.Array);
          columns.Write (newcol);
          names.Write (m_names[i]);
        }
      }
      return new RCCube (axis, names, columns);
    }

    protected static RCArray<int> StuffIndex (int count)
    {
      RCArray<int> result = new RCArray<int> (count);
      for (int i = 0; i < count; ++i)
      {
        result.Write (i);
      }
      return result;
    }

    public RCCube Read (ReadSpec spec, ReadCounter counter, bool forceg, int end)
    {
      //Always include the G column when reading from the blackboard.
      Reader reader = new Reader (this, spec, counter, forceg, end);
      RCCube result = reader.Read ();
      result.m_reader = reader;
      if (result.Axis.Global != null && result.Axis.Global.Count > 0)
      {
        result.m_lines = result.Axis.Global[result.Axis.Global.Count - 1] + 1;
      }
      else
      {
        result.m_lines = Count;
      }
      return result;
    }

    public RCArray<RCSymbolScalar> Write (ReadCounter counter,
                                          RCSymbol symbol,
                                          RCBlock data,
                                          long initg,
                                          bool force)
    {
      HashSet<RCSymbolScalar> result = new HashSet<RCSymbolScalar> ();
      for (int i = 0; i < symbol.Count; ++i)
      {
        bool delete = false;
        long dups = 0;
        for (int j = 0; j < data.Count; ++j)
        {
          RCBlock column = data.GetName (j);
          //ideally the whole transaction would be rolled back in this case
          //but I don't want to open that can of worms right now.
          if (column.Value.Count != symbol.Count)
          {
            throw new Exception ("All columns must have the same count.");
          }
          object box = column.Value.Child (i);
          if (WriteCell (column.Name, symbol[i], box, -1, false, force, out delete) != null)
          {
            result.Add (symbol[i]);
          }
          else
          {
            ++dups;
          }
        }
        if (delete || dups < data.Count)
        {
          long g = initg + Axis.Count;
          if (delete)
          {
            ++g;
            g = -g;
          }
          counter.Write (symbol[i], (int) g);
          long now = DateTime.UtcNow.Ticks;
          long e = Math.Abs (g);
          RCTimeScalar t = new RCTimeScalar (new DateTime (now), RCTimeType.Timestamp);
          Write (g, e, t, symbol[i]);
        }
      }
      return new RCArray<RCSymbolScalar> (result);
    }

    public RCArray<RCSymbolScalar> Write (ReadCounter counter, 
                                          RCCube cube, 
                                          bool keepIncrs, bool force, long initg)
    {
      return new Writer (this, counter, keepIncrs, force, initg).Write (cube);
    }

    public void UnreserveColumn (string name)
    {
      int index = m_names.IndexOf (name);
      if (m_names.IndexOf (name) < 0)
      {
        throw new Exception (string.Format ("Unknown column name: {0}", name));
      }
      m_columns.Write (index, (ColumnBase) null);
    }

    //This method is used to Reserve a spot in the column order
    //for a column whose first row is null.
    public void ReserveColumn (string name, bool canonical)
    {
      if (m_names.IndexOf (name) < 0)
      {
        m_names.Write (name);
        if (canonical)
        {
          m_columns.Write (new ColumnOfNothing (Axis));
        }
        else
        {
          m_columns.Write ((ColumnBase) null);
        }
      }
    }

    public void ReserveColumn (string name)
    {
      ReserveColumn (name, canonical:false);
    }

    public RCSymbolScalar WriteCell (string name, object box)
    {
      return WriteCell (name, null, box);
    }

    public RCSymbolScalar WriteCell (string name, RCSymbolScalar symbol, object box)
    {
      if (box == null)
      {
        return null;
      }
      bool delete;
      return WriteCell (name, symbol, box, -1, false, false, out delete);
    }

    public RCSymbolScalar WriteCell (string name,
                                     RCSymbolScalar symbol,
                                     object box,
                                     int index,
                                     bool parsing,
                                     bool force)
    {
      if (box == null)
      {
        return null;
      }
      bool delete;
      return WriteCell (name, symbol, box, index, parsing, force, out delete);
    }

    public RCSymbolScalar WriteCell (string name,
                                     RCSymbolScalar symbol,
                                     object box,
                                     int index,
                                     bool parsing,
                                     bool force,
                                     out bool delete)
    {
      if (box == null)
      {
        throw new ArgumentNullException ("box");
      }
      else if (box is int)
      {
        box = (long) (int) box;
      }
      ColumnBase old = null;
      delete = false;
      int col = m_names.IndexOf (name);
      if (col > -1)
      {
        old = m_columns[col];
      }
      //Not pretty, and this is going to make it harder to get rid
      //of the boxing at some later stage.
      if (!parsing)
      {
        RCIncrScalar? incr = box as RCIncrScalar?;
        if (!parsing && incr != null)
        {
          if (incr == RCIncrScalar.Increment || incr == RCIncrScalar.Delete)
          {
            if (old == null)
            {
              box = 0L;
            }
            Column<double> d = old as Column<double>;
            if (d != null)
            {
              double val = 0;
              if (d.Last (symbol, out val)) ++val;
              box = val;
            }
            Column<long> l = old as Column<long>;
            if (l != null)
            {
              long val = 0;
              if (l.Last (symbol, out val)) ++val;
              box = val;
            }
            Column<decimal> m = old as Column<decimal>;
            if (m != null)
            {
              decimal val = 0;
              if (m.Last (symbol, out val)) ++val;
              box = val;
            }
          }
          else if (incr == RCIncrScalar.Decrement)
          {
            if (old == null)
            {
              box = 0L;
            }
            Column<double> d = old as Column<double>;
            if (d != null)
            {
              double val = 0;
              if (d.Last (symbol, out val)) --val;
              box = val;
            }
            Column<long> l = old as Column<long>;
            if (l != null)
            {
              long val = 0;
              if (l.Last (symbol, out val)) --val;
              box = val;
            }
            Column<decimal> m = old as Column<decimal>;
            if (m != null)
            {
              decimal val = 0;
              if (m.Last (symbol, out val)) --val;
              box = val;
            }
          }
          // Actually delete (note that +~ incrop also increments the column like ++)
          if (incr == RCIncrScalar.Delete)
          {
            for (int i = 0; i < m_columns.Count; ++i)
            {
              if (m_columns[i] != null)
              {
                m_columns[i].Delete (symbol);
              }
            }
            delete = true;
          }
        }
      }

      if (old == null)
      {
        //Yuck, now I have to maintain a last map for each column as well.
        //I was doing it before anyhow, I just need to move some code around.
        ColumnBase column;
        Type type = box.GetType ();
        if (type == typeof (byte)) column = new ColumnOfByte (Axis);
        else if (type == typeof (double)) column = new ColumnOfDouble (Axis);
        else if (type == typeof (long)) column = new ColumnOfLong (Axis);
        else if (type == typeof (decimal)) column = new ColumnOfDecimal (Axis);
        else if (type == typeof (bool)) column = new ColumnOfBool (Axis);
        else if (type == typeof (string)) column = new ColumnOfString (Axis);
        else if (type == typeof (RCSymbolScalar)) column = new ColumnOfSymbol (Axis);
        else if (type == typeof (RCTimeScalar)) column = new ColumnOfTime (Axis);
        else if (type == typeof (RCIncrScalar)) column = new ColumnOfIncr (Axis);
        else throw new Exception ("Unsupported type: " + type);

        if (col > -1)
        {
          m_columns.Write (col, column);
        }
        else
        {
          m_columns.Write (column);
          m_names.Write (name);
          //the Name would have been populated by ReserveColumn in this case.
          //m_names.Write (index, name);
        }
        return column.Write (symbol, index >= 0 ? index : Axis.Count, box, force) ? symbol : null;
      }
      else
      {
        return old.Write (symbol, index >= 0 ? index : Axis.Count, box, force) ? symbol : null;
      }
    }

    //When reading backwards the result will end up backwards as well.
    //The best way that I can think of to solve this is to reverse all the vectors
    //and the timeline in place.  I don't want to allocate memory again.
    //This will be done before returning the newly created cube.
    internal void ReverseInPlace ()
    {
      for (int i = 0; i < m_columns.Count; ++i)
      {
        if (m_columns[i] != null)
        {
          m_columns[i].ReverseInPlace (Axis.Count);
        }
      }
      Axis.ReverseInPlace ();
    }

    /*
    public static long VisitCellsSorted (RCCube cube, RCSymbolScalar spec)
    {
      string direction = (string) spec.Part (0);
      string name = (string) spec.Part (1);

      ColumnBase column = cube.GetColumn (cube.FindColumn (name));
      //1) First sort column, getting a list of ranks.
      //2) Then apply the ranks to the time and symbol columns.
      //So now I have a timeline sorted along with column.
      //And of course column is sorted.
      //Now I need to go through the other columns one by one, and see if they contain
      //values at the same point in time
      //Now, for each index in column I need to find the other columns that have values at point.
      //  That means O(n) searching each column for each cell O(n^2) OR
      //  maybe only allowing sorting iff the thing is already sorted by time.
      //What does this do when the database contains nulls that are supposed to represent repeated values?
      //  I'm not certain but I think it will be wrong.
    }
    */

    public static long VisitCellsForward (Timeline timeline,
                                          Visitor visitor,
                                          RCArray<string> names,
                                          RCArray<ColumnBase> columns,
                                          int start,
                                          int end,
                                          bool canonical)
    {
      if (timeline.Count == 0)
      {
        return 0;
      }
      //Row number in the source data grid.
      //NOT THE DESTINATION!
      int tlrow = start;
      //last timestamp on each column.
      long[] times = new long[columns.Count];
      //last index in each column.
      int[] vrow = new int[columns.Count];
      for (int i = 0; i < columns.Count; ++i)
      {
        if (columns[i] != null)
        {
          vrow[i] = columns[i].CountBefore (tlrow);
        }
        else
        {
          vrow[i] = 0;
        }
        visitor.BeforeCol (i, names[i]);
      }

      //number of columns that we are beyond the end of.
      int numDoneCols = 0;
      //status indicator for each column.
      bool[] doneCols = new bool[columns.Count];
      for (int i = 0; i < doneCols.Length; ++i)
      {
        if (columns[i] == null || vrow[i] >= columns[i].Count)
        {
          doneCols[i] = true;
          ++numDoneCols;
        }
      }
      //return early if everything is count zero.
      if (numDoneCols >= columns.Count)
      {
        if (!canonical || timeline.Count == 0)
        {
          return tlrow;
        }
      }
      // Always use the canonical method if there is no S column on the axis.
      canonical = canonical || timeline.Symbol == null;

    LOOP:
      long mintime = long.MaxValue;
      int mincol = int.MinValue;
      RCSymbolScalar symbol = null;
      Queue<int> mincols = new Queue<int> ();
      RCTimeScalar time = new RCTimeScalar (new DateTime (0), RCTimeType.Timestamp);
      int destTlRow = int.MinValue;
      int nextSourceRow = int.MaxValue;
      //Basic merge sort code.
      //Normally you use a heap with log(k) lookups.
      //But I don't want the overhead of a heap given that I expect
      //k to be small, so it's O(n*k) instead of O(n*log(k)).
      //Someday I would like to experiment with data structures whose
      //implementations change based on size.
      //So for k < 10 it would use an array and after that a heap for example.
      for (int col = 0; col < columns.Count; ++col)
      {
        if (doneCols[col])
        {
          continue;
        }
        if (columns[col].TypeCode == '0')
        {
          nextSourceRow = tlrow;
        }
        else if (columns[col].Index[vrow[col]] < nextSourceRow)
        {
          nextSourceRow = columns[col].Index[vrow[col]];
        }
        if (canonical)
        {
          destTlRow = tlrow;
        }
        else
        {
          destTlRow = nextSourceRow;
        }

        if (timeline.Event != null)
        {
          if (!(columns[col].TypeCode == '0'))
          {
            times[col] = timeline.Event[columns[col].Index[vrow[col]]];
          }
        }
        else
        {
          if (!(columns[col].TypeCode == '0'))
          {
            times[col] = columns[col].Index[vrow[col]];
          }
        }

        if (times[col] < mintime)
        {
          mincols.Clear ();
          mintime = times[col];
          mincol = col;
          if (timeline.Symbol != null)
          {
            int symbolIndex;
            if (destTlRow < nextSourceRow)
            {
              symbolIndex = destTlRow;
            }
            else
            {
              if (!(columns[col].TypeCode == '0'))
              {
                symbolIndex = columns[col].Index[vrow[col]];
              }
              else
              {
                symbolIndex = destTlRow;
              }
            }
            symbol = timeline.Symbol[symbolIndex];
            if (timeline.Time != null)
            {
              // Not sure whether to use destTlRow here - I think we should? Maybe
              time = timeline.Time[symbolIndex];
            }
          }
          else
          {
            symbol = null;
          }
          mincols.Enqueue (col);
        }
        else if (times[col] == mintime)
        {
          //If they have the same symbol then put them on the same row.
          if (symbol == null || timeline.Symbol[columns[col].Index[vrow[col]]].Equals (symbol))
          {
            mincols.Enqueue (col);
          }
          //TODO: Give preference to the symbol with the greatest number of
          //identical timestamps/symbols in the future, so that the
          //last row with that timestamp/symbol will have values
          //on all fields.
        }
      }

      if (symbol == null && timeline != null && timeline.Symbol != null)
      {
        symbol = timeline.Symbol[tlrow];
      }

      visitor.BeforeRow (mintime, time, symbol, tlrow);
      if (timeline.Has ("G"))
      {
        visitor.GlobalCol (timeline.Global[tlrow]);
      }
      if (timeline.Has ("E"))
      {
        if (canonical && timeline.Event != null)
        {
          visitor.EventCol (timeline.Event[tlrow]);
        }
        else
        {
          visitor.EventCol (mintime);
        }
      }
      if (timeline.Has ("T"))
      {
        visitor.TimeCol (time);
      }
      visitor.BetweenCols (-2);
      if (timeline.Has ("S"))
      {
        visitor.SymbolCol (symbol);
      }
      visitor.BetweenCols (-1);

      int j = 0;
      if (canonical && destTlRow < nextSourceRow)
      {
        while (mincols.Count > 0)
        {
          columns[j].AcceptNull (names[j], visitor, vrow[j]);
          if (j < columns.Count - 1)
          {
            visitor.BetweenCols (j);
          }
          mincols.Dequeue ();
          ++j;
        }
      }
      while (mincols.Count > 0)
      {
        mincol = mincols.Dequeue ();
        while (j < mincol)
        {
          //Do the nulls before the mincolth column
          columns[j].AcceptNull (names[j], visitor, vrow[j]);
          if (j < columns.Count - 1)
          {
            visitor.BetweenCols (j);
          }
          ++j;
        }
        //Do the value at the mincolth column
        columns[mincol].Accept (names[mincol], visitor, vrow[mincol]);
        ++j;
        if (mincol < columns.Count - 1)
        {
          visitor.BetweenCols (j);
        }
        ++vrow[mincol];
        if (vrow[mincol] >= columns[mincol].Count)
        {
          doneCols[mincol] = true;
          ++numDoneCols;
        }
      }
      //Do the remaining nulls after the mincolth column
      while (j < columns.Count)
      {
        if (columns[j] == null)
        {
          visitor.VisitNull<object> (names[j], null, vrow[j]);
        }
        else
        {
          columns[j].AcceptNull (names[j], visitor, vrow[j]);
        }
        if (j < columns.Count - 1)
        {
          visitor.BetweenCols (j);
        }
        ++j;
      }
      //Finish this row
      if (numDoneCols < columns.Count)
      {
        visitor.AfterRow (mintime, time, symbol, tlrow);
        visitor.BetweenRows (tlrow);
        ++tlrow;
        if (tlrow == end)
        {
          goto DONE;
        }
        goto LOOP;
      }
      else
      {
        if (canonical)
        {
          visitor.AfterRow (mintime, time, symbol, tlrow);
          visitor.BetweenRows (tlrow);
          ++tlrow;
          while (tlrow < end)
          {
            if (timeline.Symbol != null)
            {
              symbol = timeline.Symbol[tlrow];
            }
            VisitEmptyRow (visitor, timeline, columns, names, tlrow, end, mintime, time, symbol);
            ++tlrow;
          }
        }
        else
        {
          visitor.AfterRow (mintime, time, symbol, tlrow);
        }
        goto DONE;
      }

    DONE: return tlrow;
    }

    protected static void VisitEmptyRow (Visitor visitor,
                                         Timeline timeline,
                                         RCArray<ColumnBase> columns,
                                         RCArray<string> names,
                                         int tlrow,
                                         int end,
                                         long mintime,
                                         RCTimeScalar time,
                                         RCSymbolScalar symbol)
    {
      // Do timeline columns here:
      if (timeline.Has ("G"))
      {
        visitor.GlobalCol (timeline.Global[tlrow]);
      }
      if (timeline.Has ("E"))
      {
        visitor.EventCol (timeline.Event[tlrow]);
      }
      if (timeline.Has ("T"))
      {
        visitor.TimeCol (timeline.Time[tlrow]);
      }
      visitor.BetweenCols (-2);
      if (timeline.Has ("S"))
      {
        visitor.SymbolCol (timeline.Symbol[tlrow]);
      }
      visitor.BetweenCols (-1);
      if (timeline.Symbol != null)
      {
        symbol = timeline.Symbol[tlrow];
      }
      for (int i = 0; i < columns.Count; ++i)
      {
        columns[i].AcceptNull (names[i], visitor, tlrow);
        if (i < columns.Count - 1)
        {
          visitor.BetweenCols (i);
        }
      }
      visitor.AfterRow (mintime, time, symbol, tlrow);
      if (tlrow == end - 1)
      {
        visitor.BetweenRows (tlrow);
      }
    }

    /// <summary>
    /// Visit all of the cells in temporal order.
    /// This should really have a start and end row.
    /// I think we have the possibility of reading cells that are in the
    /// process of being written. Also I want to generalize this so that
    /// it can go backwards, and write a read visitor that can read from
    /// the end of the cube.
    /// </summary>
    public virtual long VisitCellsForward (Visitor visitor, int start, int end)
    {
      return VisitCellsForward (Axis, visitor, m_names, m_columns, start, end, false);
    }

    public virtual long VisitCellsCanonical (Visitor visitor, int start, int end)
    {
      return VisitCellsForward (Axis, visitor, m_names, m_columns, start, end, true);
    }

    public static long VisitCellsBackward (Timeline timeline,
                                           Visitor visitor,
                                           RCArray<string> names,
                                           RCArray<ColumnBase> columns,
                                           int start,
                                           //It may no longer be necessary to supply an end parameter, since we will mandate that
                                           //that the global, mutable cube is never shared between fibers.
                                           int end)
    {
      //Row number in the source data grid
      int row = end - 1;
      if (columns.Count == 0)
      {
        return row;
      }

      //last timestamp on each column.
      long[] times = new long[columns.Count];
      //last index in each column.
      int[] vrow = new int[columns.Count];

      for (int i = 0; i < columns.Count; ++i)
      {
        vrow[i] = columns[i].CountBefore (end) - 1;
        visitor.BeforeCol (i, names[i]);
      }

      //number of columns that we are beyond the end of.
      int numDoneCols = 0;
      //status indicator for each column.
      bool[] doneCols = new bool[columns.Count];

    LOOP:
      //long mintime = long.MaxValue;
      long maxtime = long.MinValue;
      int mincol = int.MinValue;
      //Uh, shouldn't we reuse the same instance of the Queue?
      Queue<int> mincols = new Queue<int> ();
      RCSymbolScalar symbol = null;
      RCTimeScalar time = new RCTimeScalar (new DateTime (0), RCTimeType.Timestamp);

      //Basic merge sort code.
      //Normally you use a heap with log(k) lookups.
      //But I don't want the overhead of a heap given that I expect
      //k to be small, so its O(n*k) instead of O(n*log(k)).
      //Someday I would like to experiment with data structures whose
      //implementations change based on size.
      //So for k < 10 it would use an array and after that a heap for example.
      for (int col = 0; col < columns.Count; ++col)
      {
        //Are we already done with this column?
        if (doneCols[col]) continue;

        times[col] = timeline.Event[columns[col].Index[vrow[col]]];
        if (times[col] > maxtime)
        {
          mincols.Clear ();
          maxtime = times[col];
          mincol = col;
          symbol = timeline.Symbol[columns[col].Index[vrow[col]]];
          if (timeline.Time != null)
          {
            time = timeline.Time[columns[col].Index[vrow[col]]];
          }
          mincols.Enqueue (col);
        }
        else if (times[col] == maxtime)
        {
          if (timeline.Symbol[columns[col].Index[vrow[col]]].Equals (symbol))
          {
            mincols.Enqueue (col);
          }
          //TODO: Give preference to the one with the greatest number of
          //identical timestamps/symbols in the future, so that the
          //last row with that timestamp/symbol will have values
          //on all fields.
        }
      }

      visitor.BeforeRow (maxtime, time, symbol, row);
      visitor.EventCol (maxtime);
      visitor.BetweenCols (-2);
      visitor.SymbolCol (symbol);
      visitor.BetweenCols (-1);

      //Why can't I reuse the name col here?  I don't get it.
      int j = 0;
      while (mincols.Count > 0)
      {
        mincol = mincols.Dequeue ();
        while (j < mincol)
        {
          columns[j].AcceptNull (names[j], visitor, row);
          if (j < columns.Count - 1)
          {
            visitor.BetweenCols (j);
          }
          ++j;
        }
        columns[mincol].Accept (names[mincol], visitor, vrow[mincol]);
        ++j;
        if (mincol < columns.Count - 1)
        {
          visitor.BetweenCols (j);
        }
        --vrow[mincol];
        if (vrow[mincol] < 0)
        {
          doneCols[mincol] = true;
          ++numDoneCols;
        }
      }
      while (j < columns.Count)
      {
        columns[j].AcceptNull (names[j], visitor, row);
        if (j < columns.Count - 1)
        {
          visitor.BetweenCols (j);
        }
        ++j;
      }

      if (numDoneCols < columns.Count)
      {
        visitor.AfterRow (maxtime, time, symbol, row);
        visitor.BetweenRows (row);
        --row;
        if (row < start)
        {
          goto DONE;
        }
        goto LOOP;
      }
      else
      {
        visitor.AfterRow (maxtime, time, symbol, row);
        goto DONE;
      }

    DONE: return row;
    }

    /// <summary>
    /// Visit all of the cells in temporal order.
    /// This should really have a start and end row.
    /// I think we have the possibility of reading cells that are in the process of being written.
    /// Also I want to generalize this so that it can go backwards, and write a read visitor that
    /// can read from the end of the cube.
    /// </summary>
    public virtual long VisitCellsBackward (Visitor visitor, int start, int end)
    {
      return VisitCellsBackward (Axis, visitor, m_names, m_columns, start, end);
    }
  }

  public enum SortDirection
  {
    asc = 0,
    desc = 1,
    absasc = 2,
    absdesc = 3
  }

  /// <summary>
  /// Encapsulate differences between different types of cubes.
  /// Presence or absence of the G column does not affect the CubeProto.
  /// This class is mostly a way to share comparer operations between SortAxis and IsAxisSorted.
  /// </summary>
  public abstract class CubeProto
  {
    public static CubeProto Create (Timeline axis)
    {
      if (axis.Symbol != null)
      {
        if (axis.Time != null)
        {
          if (axis.Event != null)
          {
            return new ETSCubeProto (axis);
          }
          else
          {
            return new TSCubeProto (axis);
          }
        }
        else
        {
          return new SCubeProto (axis);
        }
      }
      else
      {
        return new RectCubeProto (axis);
      }
    }

    protected Timeline m_axis;
    public CubeProto (Timeline axis)
    {
      if (axis == null)
      {
        throw new ArgumentNullException ("axis");
      }
      m_axis = axis;
    }

    public abstract int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2);
    public virtual int CompareAxisRows (int i1, int i2)
    {
      return CompareAxisRows (m_axis, i1, m_axis, i2);
    }

    public Timeline Sort ()
    {
      if (m_axis.Proto.IsAxisSorted ())
      {
        return m_axis;
      }
      else
      {
        return RankUtils.ApplyAxisRank (m_axis, RankUtils.DoAxisRank (m_axis));
      }
    }

    public bool IsAxisSorted ()
    {
      for (int i = 1; i < m_axis.Count; ++i)
      {
        if (CompareAxisRows (i - 1, i) > 0)
        {
          return false;
        }
      }
      return true;
    }
  }

  public class ETSCubeProto : CubeProto
  {
    public ETSCubeProto (Timeline axis) : base (axis) {}

    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      long eventX = axis1.Event[i1];
      long eventY = axis2.Event[i2];
      int compareResult = eventX.CompareTo (eventY);
      if (compareResult == 0)
      {
        RCTimeScalar timeX = axis1.Time[i1];
        RCTimeScalar timeY = axis2.Time[i2];
        compareResult = timeX.CompareTo (timeY);
        if (compareResult == 0)
        {
          RCSymbolScalar symbolX = axis1.SymbolAt (i1);
          RCSymbolScalar symbolY = axis2.SymbolAt (i2);
          return symbolX.CompareTo (symbolY);
        }
        else
        {
          return compareResult;
        }
      }
      else
      {
        return compareResult;
      }
    }
  }

  public class TSCubeProto : CubeProto
  {
    public TSCubeProto (Timeline axis) : base (axis) {}

    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      RCAssert.AxisHasT (axis1, "CompareAxisRows: axis1 must contain the T column");
      RCAssert.AxisHasT (axis2, "CompareAxisRows: axis2 must contain the T column");
      RCTimeScalar timeX = axis1.Time[i1];
      RCTimeScalar timeY = axis2.Time[i2];
      int compareResult = timeX.CompareTo (timeY);
      if (compareResult == 0)
      {
        RCSymbolScalar symbolX = axis1.SymbolAt (i1);
        RCSymbolScalar symbolY = axis2.SymbolAt (i2);
        return symbolX.CompareTo (symbolY);
      }
      else return compareResult;
    }
  }

  public class SCubeProto : CubeProto
  {
    public SCubeProto (Timeline axis) : base (axis) {}

    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      RCSymbolScalar symbolX = axis1.SymbolAt (i1);
      RCSymbolScalar symbolY = axis2.SymbolAt (i2);
      return symbolX.CompareTo (symbolY);
    }
  }

  public class RectCubeProto : CubeProto
  {
    public RectCubeProto (Timeline axis) : base (axis) {}
    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      return i1.CompareTo (i2);
    }
  }

  //I wanted rank to support the same set of operators as sort.
  //But in practice you could just take the abs of your column
  //before ranking it, which doesn't work for sorting.
  //So I'm not certain bending over backwards for absolute ranking makes sense.
  //On the other hand the work is already done and this should be a little more performant;
  //no additional vector created for the abs.
  public class AbsoluteValue<T>
  {
    public virtual T Abs (T val)
    {
      return val;
    }
  }

  public class LongAbs : AbsoluteValue<long>
  {
    public override long Abs (long val)
    {
      return Math.Abs (val);
    }
  }

  public class DoubleAbs : AbsoluteValue<double>
  {
    public override double Abs (double val)
    {
      return Math.Abs (val);
    }
  }

  public class DecimalAbs : AbsoluteValue<decimal>
  {
    public override decimal Abs (decimal val)
    {
      return Math.Abs (val);
    }
  }

  public class RankUtils
  {
    public static long[] DoRank<T> (SortDirection direction, RCVector<T> vector)
      where T : IComparable<T>
    {
      long[] indices = new long[vector.Count];
      for (long i = 0; i < indices.Length; ++i)
      {
        indices[i] = i;
      }
      RankState<T> state = new RankState<T> (vector);
      Comparison<long> comparison;
      switch (direction)
      {
        case SortDirection.asc : comparison = new Comparison<long> (state.Asc); break;
        case SortDirection.desc : comparison = new Comparison<long> (state.Desc); break;
        case SortDirection.absasc : comparison = new Comparison<long> (state.AbsAsc); break;
        case SortDirection.absdesc : comparison = new Comparison<long> (state.AbsDesc); break;
        default : throw new Exception ("Unknown SortDirection: " + direction.ToString ());
      }
      Array.Sort (indices, comparison);
      return indices;
    }

    public static long[] DoRank<T> (SortDirection direction, char typeCode, RCArray<T> array)
      where T : IComparable<T>
    {
      long[] indices = new long[array.Count];
      for (long i = 0; i < indices.Length; ++i)
      {
        indices[i] = i;
      }
      RankStateArray<T> state = new RankStateArray<T> (array, typeCode);
      Comparison<long> comparison;
      switch (direction)
      {
        case SortDirection.asc : comparison = new Comparison<long> (state.Asc); break;
        case SortDirection.desc : comparison = new Comparison<long> (state.Desc); break;
        case SortDirection.absasc : comparison = new Comparison<long> (state.AbsAsc); break;
        case SortDirection.absdesc : comparison = new Comparison<long> (state.AbsDesc); break;
        default : throw new Exception ("Unknown SortDirection: " + direction.ToString ());
      }
      Array.Sort (indices, comparison);
      return indices;
    }

    public static void ApplyArrayRank<T> (RCArray<T> inputData,
                                          RCArray<int> inputIndex,
                                          Dictionary<long, int> map,
                                          int axisCount,
                                          out RCArray<T> data,
                                          out RCArray<int> index)
    {
      RCArray<long> im = new RCArray<long> (inputData.Count);
      T[] fd = new T[inputData.Count];
      int[] fi = new int[inputData.Count];
      if (inputIndex != null)
      {
        // Any rows that are lacking values in the sort column will be missing from map.
        // Push these rows so that they come after all the rows with valid values in the sort column.
        // But importantly, they are kept in the original order.
        for (int j = 0; j < inputIndex.Count; ++j)
        {
          int newRow;
          if (!map.TryGetValue (inputIndex[j], out newRow))
          {
            throw new Exception (string.Format ("inputIndex[j]: {0} was not represented in map!", inputIndex[j]));
          }
          im.Write (newRow);
        }
      }
      else
      {
        for (int j = 0; j < inputData.Count; ++j)
        {
          int newRow;
          if (!map.TryGetValue (j, out newRow))
          {
            newRow = map.Count + j;
          }
          im.Write (newRow);
        }
      }
      //Now rank the values in im ascending.
      long[] rim = DoRank<long> (SortDirection.asc, 'l', im);
      for (int j = 0; j < rim.Length; ++j)
      {
        fd[j] = inputData[(int) rim[j]];
        fi[j] = (int) im[(int) rim[j]];
      }
      index = new RCArray<int> (fi);
      data = new RCArray<T> (fd);
    }

    public static Timeline ApplyAxisRank (Timeline axis, Dictionary<long, int> map)
    {
      Timeline result;
      RCArray<long> g = axis.Global;
      RCArray<long> gNew = null;
      RCArray<long> e = axis.Event;
      RCArray<long> eNew = null;
      RCArray<RCTimeScalar> t = axis.Time;
      RCArray<RCTimeScalar> tNew = null;
      RCArray<RCSymbolScalar> s = axis.Symbol;
      RCArray<RCSymbolScalar> sNew = null;
      if (axis.Global != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (g, null, map, axis.Count, out gNew, out ignore);
      }
      if (axis.Event != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (e, null, map, axis.Count, out eNew, out ignore);
      }
      if (axis.Time != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (t, null, map, axis.Count, out tNew, out ignore);
      }
      if (axis.Symbol != null)
      {
        RCArray<int> ignore;
        ApplyArrayRank (s, null, map, axis.Count, out sNew, out ignore);
      }
      if (gNew == null && eNew == null && tNew == null && sNew == null)
      {
        result = new Timeline (axis.Count);
      }
      else
      {
        result = new Timeline (gNew, eNew, tNew, sNew);
      }
      return result;
    }

    /// <summary>
    /// DoAxisRank implements the standard ranking for a sorted timeline axis, which is by T or E, followed by S.
    /// </summary>
    public static Dictionary<long, int> DoAxisRank (Timeline axis)
    {
      int[] indices = new int[axis.Count];
      for (int i = 0; i < axis.Count; ++i)
      {
        indices[i] = i;
      }
      Array.Sort (indices, axis.Proto.CompareAxisRows);
      Dictionary<long, int> result = new Dictionary<long, int> ();
      for (int i = 0; i < indices.Length; ++i)
      {
        result[indices[i]] = i;
      }
      return result;
    }
  }

  public class RankState<T> where T : IComparable<T>
  {
    protected static readonly Dictionary<char, object> m_absmap = new Dictionary<char, object>();
    protected RCVector<T> m_data;
    protected AbsoluteValue<T> m_abs;

    static RankState ()
    {
      m_absmap['l'] = new LongAbs();
      m_absmap['d'] = new DoubleAbs();
      m_absmap['m'] = new DecimalAbs();
    }

    public RankState (RCVector<T> data)
    {
      m_data = data;
      object abs = null;
      m_absmap.TryGetValue (data.TypeCode, out abs);
      if (abs == null)
      {
        m_abs = new AbsoluteValue<T>();
      }
      else
      {
        m_abs = (AbsoluteValue<T>) abs;
      }
    }

    public virtual int Asc (long x, long y)
    {
      return m_data[(int) x].CompareTo (m_data[(int) y]);
    }

    public virtual int Desc (long x, long y)
    {
      return m_data[(int) y].CompareTo (m_data[(int) x]);
    }

    public virtual int AbsAsc (long x, long y)
    {
      return m_abs.Abs (m_data[(int) x]).CompareTo (m_abs.Abs (m_data[(int) y]));
    }

    public virtual int AbsDesc (long x, long y)
    {
      return m_abs.Abs (m_data[(int) y]).CompareTo (m_abs.Abs (m_data[(int) x]));
    }
  }

  public class RankStateArray<T> where T : IComparable<T>
  {
    protected static readonly Dictionary<char, object> m_absmap = new Dictionary<char, object>();
    protected RCArray<T> m_data;
    protected AbsoluteValue<T> m_abs;

    static RankStateArray ()
    {
      m_absmap['l'] = new LongAbs ();
      m_absmap['d'] = new DoubleAbs ();
      m_absmap['m'] = new DecimalAbs ();
    }

    public RankStateArray (RCArray<T> data, char typeCode)
    {
      m_data = data;
      object abs = null;
      m_absmap.TryGetValue (typeCode, out abs);
      if (abs == null)
      {
        m_abs = new AbsoluteValue<T>();
      }
      else
      {
        m_abs = (AbsoluteValue<T>) abs;
      }
    }

    public virtual int Asc (long x, long y)
    {
      return m_data[(int) x].CompareTo (m_data[(int) y]);
    }

    public virtual int Desc (long x, long y)
    {
      return m_data[(int) y].CompareTo (m_data[(int) x]);
    }

    public virtual int AbsAsc (long x, long y)
    {
      return m_abs.Abs (m_data[(int) x]).CompareTo (m_abs.Abs (m_data[(int) y]));
    }

    public virtual int AbsDesc (long x, long y)
    {
      return m_abs.Abs (m_data[(int) y]).CompareTo (m_abs.Abs (m_data[(int) x]));
    }
  }
}

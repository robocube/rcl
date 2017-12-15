
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  //We should just call it RoboCube.Cube.
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
      public ColumnOfNothing (Timeline timeline)
        : base (timeline) {}

      public ColumnOfNothing (
        Timeline timeline, RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override bool Write (RCSymbolScalar key, int index, object box, bool force)
      {
        throw new Exception ("Cannot write to an EmptyColumn");
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
        //visitor.VisitScalar<object> (name, this, i);
        //throw new NotImplementedException ();
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

      public override string ScalarToString (int i)
      {
        throw new NotImplementedException ();
      }

      public override string ScalarToCsvString (int vrow)
      {
        throw new NotImplementedException ();
      }

      public override char TypeCode { get { return '0'; } }
      public override object BoxCell (int i) { return null; }
      public override object Array { get { return m_data; } }
      public override RCArray<int> Index { get { return m_index; } }
      public new RCArray<object> Data { get { return m_data; } }
      public override int Count { get { return m_data.Count; } }
    }
    
    public class ColumnOfByte : Column<byte>
    {
      public ColumnOfByte (Timeline timeline)
        : base (timeline) {}

      public ColumnOfByte (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (int i)
      {
        return RCByte.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
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

      public override string ScalarToString (int i)
      {
        return RCLong.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
      }

      public override char TypeCode { get { return 'l'; } }
    }

    public class ColumnOfDouble : Column<double>
    {
      public ColumnOfDouble (Timeline timeline) : base (timeline) {}

      public ColumnOfDouble (Timeline timeline,
                             RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (int i)
      {
        return RCDouble.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
      }

      public override char TypeCode { get { return 'd'; } }
    }

    public class ColumnOfDecimal : Column<decimal>
    {
      public ColumnOfDecimal (Timeline timeline) : base (timeline) {}

      public ColumnOfDecimal (Timeline timeline,
                              RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (int i)
      {
        return RCDecimal.FormatScalar (m_data[i]) + "m";
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
      }

      public override char TypeCode { get { return 'm'; } }
    }

    public class ColumnOfBool : Column<bool>
    {
      public ColumnOfBool (Timeline timeline) : base (timeline) {}

      public ColumnOfBool (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (int i)
      {
        return RCBoolean.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
      }

      public override char TypeCode { get { return 'b'; } }
    }

    public class ColumnOfString : Column<string>
    {
      public ColumnOfString (Timeline timeline) : base (timeline) {}

      public ColumnOfString (Timeline timeline,
                             RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (int i)
      {
        return RCString.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
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

      public override string ScalarToString (int i)
      {
        return RCSymbol.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
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

      public override string ScalarToString (int i)
      {
        return RCTime.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
      }

      public override char TypeCode { get { return 't'; } }
    }

    public class ColumnOfIncr : Column<RCIncrScalar>
    {
      public ColumnOfIncr (Timeline timeline) : base (timeline) {}

      public ColumnOfIncr (Timeline timeline,
                           RCArray<int> index, object data)
        : base (timeline, index, data) {}

      public override string ScalarToString (int i)
      {
        return RCIncr.FormatScalar (m_data[i]);
      }

      public override string ScalarToCsvString (int i)
      {
        return ScalarToString (i);
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
      m_names.Lock ();
      if (m_reader != null)
      {
        m_reader.Lock ();
      }
      m_columns.Lock ();
      RCArray<int> missingCols = new RCArray<int> ();
      for (int i = 0; i < m_columns.Count; ++i)
      {
        if (m_columns[i] == null)
        {
          missingCols.Write (i);
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
      //for (int i = 0; i < missingCols.Count; ++i)
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

    public override void Format (
      StringBuilder builder, RCFormat args, int level)
    {
      new Formatter (builder, args, level).Format (this);
    }

    public override string TypeName
    {
      get { return "cube"; }
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
      int column = m_names.IndexOf (name);
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
          return new RCCube ();
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
      int column = m_names.IndexOf (name);
      return m_columns[column];
    }

    public ColumnBase GetColumn (int index)
    {
      return m_columns[index];
    }

    public Type GetType (int index)
    {
      return m_columns[index].GetElementType ();
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
      else return m_columns[column].TypeCode;
    }

    public int FindColumn (string name)
    {
      return m_names.IndexOf (name);
    }

    public bool Has (string name)
    {
      return FindColumn (name) > -1;
    }

    public override int Count
    {
      get { return Rows; }
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

    public RCCube Untimeline ()
    {
      RCArray<string> names = new RCArray<string> ();
      RCArray<ColumnBase> columns = new RCArray<ColumnBase> ();
      Timeline timeline = new Timeline (Count);
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
        else
        {
          names.Write (m_names[i]);
          columns.Write (m_columns[i]);
        }
      }
      Timeline axis = new Timeline (G, E, T, S);
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

    public void AttachCols (string name, RCCube cols)
    {
      if (cols.Axis != this.Axis)
      {
        throw new Exception ("AttachCols requires both cols to have the same Axis already.");
      }
      for (int i = 0; i < cols.Cols; ++i)
      {
        if (name == "")
        {
          m_names.Write (cols.ColumnAt (i));
        }
        else
        {
          m_names.Write (name);
        }
        m_columns.Write (cols.GetColumn (i));
      }
    }

    public RCCube Read (ReadSpec spec, ReadCounter counter, bool forceg, int end)
    {
      //Always include the G column when reading from the blackboard.
      Reader reader = new Reader (this, spec, counter, forceg, end);
      RCCube result = reader.Read ();
      result.m_reader = reader;
      if (result.Axis.Global != null && result.Axis.Global.Count > 0)
      {
        //result.m_lines = Count + result.Axis.Global[0];
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
          if (WriteCell (column.Name, symbol[i], box, -1, false, force) != null)
          {
            result.Add (symbol[i]);
          }
          else
          {
            ++dups;
          }
        }
        if (dups < data.Count)
        {
          long g = initg + Axis.Count;
          counter.Write (symbol[i], (int) g);
          long now = DateTime.Now.Ticks;
          Write (g, now, new RCTimeScalar (new DateTime (now), RCTimeType.Timestamp), symbol[i]);
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

    //This method is used to Reserve a spot in the column order
    //for a column whose first row is null.
    public void ReserveColumn (string name)
    {
      if (m_names.IndexOf (name) < 0)
      {
        m_names.Write (name);
        m_columns.Write ((ColumnBase) null);
      }
    }

    public RCSymbolScalar WriteCell (
      string name, RCSymbolScalar symbol, object box)
    {
      return WriteCell (name, symbol, box, -1, false, false);
    }

    public RCSymbolScalar WriteCell (
      string name, RCSymbolScalar symbol, object box, int index, bool parsing, bool force)
    {
      ColumnBase old = null;
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
          if (incr == RCIncrScalar.Increment)
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
              if (m.Last(symbol, out val)) ++val;
              box = val;
            }
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
                                          int end)
    {
      //Row number in the source data grid.
      int tlrow = start;
      //if (timeline.Global != null && timeline.Count > 0)
      //{
      //  tlrow -= (int) timeline.Global [0];
      //}
      //last timestamp on each column.
      long[] times = new long[columns.Count];
      //last index in each column.
      int[] vrow = new int[columns.Count];
      for (int i = 0; i < columns.Count; ++i)
      {
        vrow[i] = columns[i].CountBefore (tlrow);
        visitor.BeforeCol (i, names[i]);
      }

      //number of columns that we are beyond the end of.
      int numDoneCols = 0;
      //status indicator for each column.
      bool[] doneCols = new bool[columns.Count];
      for (int i = 0; i < doneCols.Length; ++i)
      {
        if (vrow[i] >= columns[i].Count)
        {
          doneCols[i] = true;
          ++numDoneCols;
        }
      }
      //return early if everything is count zero.
      if (numDoneCols >= columns.Count)
      {
        return tlrow;
      }

    LOOP:
      long mintime = long.MaxValue;
      int mincol = int.MinValue;
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
        if (doneCols[col])
        {
          continue;
        }
        if (timeline.Event != null)
        {
          times[col] = timeline.Event[columns[col].Index[vrow[col]]];
        }
        else
        {
          times[col] = columns[col].Index[vrow[col]];
        }

        if (times[col] < mintime)
        {
          mincols.Clear ();
          mintime = times[col];
          mincol = col;
          if (timeline.Symbol != null)
          {
            symbol = timeline.Symbol[columns[col].Index[vrow[col]]];
            if (timeline.Time != null)
            {
              time = timeline.Time[tlrow];
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
          if (symbol == null ||
              timeline.Symbol[columns[col].Index[vrow[col]]].Equals (symbol))
          {
            mincols.Enqueue (col);
          }
          //TODO: Give preference to the symbol with the greatest number of
          //identical timestamps/symbols in the future, so that the
          //last row with that timestamp/symbol will have values
          //on all fields.
        }
      }

      visitor.BeforeRow (mintime, time, symbol, tlrow);
      if (timeline.Has ("G"))
      {
        visitor.GlobalCol (timeline.Global[tlrow]);
      }
      if (timeline.Has ("E"))
      {
        visitor.EventCol (mintime);
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
      while (mincols.Count > 0)
      {
        mincol = mincols.Dequeue ();
        while (j < mincol)
        {
          columns[j].AcceptNull (names[j], visitor, vrow[j]);
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
        ++vrow[mincol];
        if (vrow[mincol] >= columns[mincol].Count)
        {
          doneCols[mincol] = true;
          ++numDoneCols;
        }
      }
      while (j < columns.Count)
      {
        columns[j].AcceptNull (names[j], visitor, vrow[j]);
        if (j < columns.Count - 1)
        {
          visitor.BetweenCols (j);
        }
        ++j;
      }
      if (numDoneCols < columns.Count)
      {
        visitor.AfterRow (mintime, time, symbol, tlrow);
        visitor.BetweenRows (tlrow);
        ++tlrow;
        if (tlrow == end) goto DONE;
        goto LOOP;
      }
      else
      {
        visitor.AfterRow (mintime, time, symbol, tlrow);
        goto DONE;
      }

    DONE: return tlrow;
    }

    /// <summary>
    /// Visit all of the cells in temporal order.
    /// This should really have a start and end row.
    /// I think we have the possibility of reading cells that are in the 
    /// process of being written.  Also I want to generalize this so that 
    /// it can go backwards, and write a read visitor that can read from 
    /// the end of the cube.
    /// </summary>
    public virtual long VisitCellsForward (Visitor visitor, int start, int end)
    {
      return VisitCellsForward (Axis, visitor, m_names, m_columns, start, end);
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
      //Row number in the source data grid.C:\dev3\RC.Test\TestMain.cs
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
      return VisitCellsBackward (
        Axis, visitor, m_names, m_columns, start, end);
    }
  }
}

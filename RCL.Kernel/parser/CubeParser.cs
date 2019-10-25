
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  [RCExtension ('u', "[", "]")]
  public class CubeParser : RCActivator.ParserExtension
  {
    protected class State : RCActivator.ParserState
    {
      /// <summary>
      /// Names of the columns in a cube being read.
      /// </summary>
      public RCArray<string> m_tnames = new RCArray<string> ();

      /// <summary>
      /// The column of the current token.
      /// </summary>
      public int m_tcolumn = 0;

      // The actual data of the cube being read.
      public RCCube m_cube;

      // The current timestamp for parsing a cube.
      public RCTimeScalar m_time = new RCTimeScalar (new DateTime (0), RCTimeType.Timestamp);

      // The current symbol for parsing a cube.
      public RCSymbolScalar m_symbol;

      // The value in the G column.
      public long m_global = -1;

      // The value in the E column.
      public long m_event = -1;

      // True if the cube contains column T.
      public bool m_hasT = false;

      // True if the cube contains column S.
      public bool m_hasS = false;

      // True if the cube has a timeline.
      public bool m_hasTimeline = false;

      // True if cube should contain rows and columns consisting entirely of nulls
      public bool m_canonical = false;

      // In the canonical case, use this to unsure empty rows are not skipped over.
      //public bool m_forceAxisWrite = false;

      public RCArray<string> m_tlcolnames = new RCArray<string> ();
    }

    public override RCActivator.ParserState StartParsing (bool canonical)
    {
      State state = new State ();
      state.m_canonical = canonical;
      return state;
    }

    public override RCValue EndParsing (object state)
    {
      State s = (State) state;
      if (s.m_cube == null)
      {
        return new RCCube ();
      }
      if (!s.m_hasTimeline && s.m_cube.Axis.Exists)
      {
        return s.m_cube.Untl ();
      }
      else
      {
        return s.m_cube;
      }
    }

    public override void AcceptName (object state, RCToken token)
    {
      State s = (State) state;
      s.m_tnames.Write (token.Text);
      s.m_tcolumn = (s.m_tcolumn + 1) % s.m_tnames.Count;
      if (token.Text == "E")
      {
        s.m_tlcolnames.Write ("E");
        s.m_hasT = true;
      }
      else if (token.Text == "G" || token.Text == "T" || token.Text == "S")
      {
        s.m_hasS = false;
        s.m_tlcolnames.Write (token.Text);
      }
    }

    public override void AcceptScalar (object state, RCToken token, RCLexer lexer)
    {
      State s = (State) state;
      //Move the column number forward.
      s.m_tcolumn = (s.m_tcolumn + 1) % s.m_tnames.Count;
      //Create the cube if necessary.
      if (s.m_cube == null)
      {
        s.m_cube = new RCCube (s.m_tlcolnames);
      }
      if (s.m_tcolumn < s.m_tlcolnames.Count)
      {
        string colname = s.m_tnames[s.m_tcolumn];
        char letter = colname[0];
        switch (letter)
        {
          case 'G' :
            s.m_global = token.ParseLong (lexer);
            break;
          case 'E' :
            s.m_event = token.ParseLong (lexer);
            break;
          case 'T':
            s.m_time = token.ParseTime (lexer);
            break;
          case 'S' :
            s.m_symbol = token.ParseSymbol (lexer);
            break;
          default : throw new Exception (
            "Unknown timeline column with letter: " + letter);
        }
      }
      else
      {
        object val = token.Parse (lexer);
        if (val != null)
        {
          ColumnBase column = s.m_cube.GetColumn (s.m_tnames[s.m_tcolumn]);
          if (s.m_canonical && column != null && column is RCCube.ColumnOfNothing)
          {
            s.m_cube.UnreserveColumn (s.m_tnames[s.m_tcolumn]);
          }
          s.m_cube.WriteCell (s.m_tnames[s.m_tcolumn], s.m_symbol, val, -1, true, true);
        }
        else
        {
          s.m_cube.ReserveColumn (s.m_tnames[s.m_tcolumn], canonical:s.m_canonical);
        }
      }
      //if (s.m_forceAxisWrite || s.m_tcolumn == s.m_tnames.Count - 1)
      if (s.m_tcolumn == s.m_tnames.Count - 1)
      {
        //If there is no time column in the source text then create a
        //series of ascending integers.
        if (!s.m_hasT)
        {
          ++s.m_event;
        }
        s.m_cube.Axis.Write (s.m_global, s.m_event, s.m_time, s.m_symbol);
        //s.m_forceAxisWrite = false;
      }
    }

    public override void AcceptSpacer (object state, RCToken token)
    {
      State s = (State) state;
      if (token.Text == "|")
      {
        s.m_hasTimeline = true;
      }
    }

    public override RCValue BinaryParse (RCActivator activator, RCArray<byte> array, ref int start)
    {
      start += sizeof (int);
      RCArray<string> tlnames = Binary.ReadVectorString (array, ref start);
      RCArray<long> G = null;
      RCArray<long> E = null;
      RCArray<RCTimeScalar> T = null;
      RCArray<RCSymbolScalar> S = null;
      for (int i = 0; i < tlnames.Count; ++i)
      {
        char name = tlnames[i][0];
        switch (name)
        {
          case 'G' : G = Binary.ReadVector<long> (array, ref start, sizeof (long)); break;
          case 'E' : E = Binary.ReadVector<long> (array, ref start, sizeof (long)); break;
          case 'T' : T = Binary.ReadVectorTime (array, ref start); break;
          case 'S' : S = Binary.ReadVectorSymbol (array, ref start); break;
          default: throw new Exception ("Unknown timeline column: " + tlnames[i]);
        }
      }
      Timeline timeline = new Timeline (G, E, T, S);
      RCArray<string> names = Binary.ReadVectorString (array, ref start);
      RCArray<ColumnBase> columns = new RCArray<ColumnBase> (names.Count);
      for (int i = 0; i < names.Count; ++i)
      {
        char type = (char) array[start];
        start += sizeof (byte);
        RCArray<int> index = RCLong.ReadVector4 (array, ref start);
        switch (type)
        {
          case 'x': columns.Write (new RCCube.ColumnOfByte (timeline, index, Binary.ReadVector<byte> (array, ref start, sizeof (byte)))); break;
          case 'd': columns.Write (new RCCube.ColumnOfDouble (timeline, index, Binary.ReadVector<double> (array, ref start, sizeof (double)))); break;
          case 'l': columns.Write (new RCCube.ColumnOfLong (timeline, index, Binary.ReadVector<long> (array, ref start, sizeof (long)))); break;
          case 'm': columns.Write (new RCCube.ColumnOfDecimal (timeline, index, Binary.ReadVectorDecimal (array, ref start))); break;
          case 'n': columns.Write (new RCCube.ColumnOfIncr (timeline, index, Binary.ReadVectorIncr (array, ref start))); break;
          case 'b': columns.Write (new RCCube.ColumnOfBool (timeline, index, Binary.ReadVector<bool> (array, ref start, sizeof (bool)))); break;
          case 's': columns.Write (new RCCube.ColumnOfString (timeline, index, Binary.ReadVectorString (array, ref start))); break;
          case 'y': columns.Write (new RCCube.ColumnOfSymbol (timeline, index, Binary.ReadVectorSymbol (array, ref start))); break;
          default: throw new Exception ("Cannot ReadVector with type " + type);
        }
      }
      return new RCCube (timeline, names, columns);
    }
  }
}

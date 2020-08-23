
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
      public RCArray<string> _tnames = new RCArray<string> ();

      /// <summary>
      /// The column of the current token.
      /// </summary>
      public int _tcolumn = 0;

      // The actual data of the cube being read.
      public RCCube _cube;

      // The current timestamp for parsing a cube.
      public RCTimeScalar _time = new RCTimeScalar (new DateTime (0), RCTimeType.Timestamp);

      // The current symbol for parsing a cube.
      public RCSymbolScalar _symbol;

      // The value in the G column.
      public long _global = -1;

      // The value in the E column.
      public long _event = -1;

      // True if the cube contains column T.
      public bool _hasT = false;

      // True if the cube contains column S.
      public bool _hasS = false;

      // True if the cube has a timeline.
      public bool _hasTimeline = false;

      // True if cube should contain rows and columns consisting entirely of nulls
      public bool _canonical = false;

      // In the canonical case, use this to unsure empty rows are not skipped over.
      // public bool _forceAxisWrite = false;

      public RCArray<string> _tlcolnames = new RCArray<string> ();
    }

    public override RCActivator.ParserState StartParsing (bool canonical)
    {
      State state = new State ();
      state._canonical = canonical;
      return state;
    }

    public override RCValue EndParsing (object state)
    {
      State s = (State) state;
      if (s._cube == null) {
        return new RCCube ();
      }
      if (!s._hasTimeline && s._cube.Axis.Exists) {
        return s._cube.Untl ();
      }
      else {
        return s._cube;
      }
    }

    public override void AcceptName (object state, RCToken token)
    {
      State s = (State) state;
      s._tnames.Write (token.Text);
      s._tcolumn = (s._tcolumn + 1) % s._tnames.Count;
      if (token.Text == "E") {
        s._tlcolnames.Write ("E");
        s._hasT = true;
      }
      else if (token.Text == "G" || token.Text == "T" || token.Text == "S") {
        s._hasS = false;
        s._tlcolnames.Write (token.Text);
      }
    }

    public override void AcceptScalar (object state, RCToken token, RCLexer lexer)
    {
      State s = (State) state;
      // Move the column number forward.
      s._tcolumn = (s._tcolumn + 1) % s._tnames.Count;
      // Create the cube if necessary.
      if (s._cube == null) {
        s._cube = new RCCube (s._tlcolnames);
      }
      if (s._tcolumn < s._tlcolnames.Count) {
        string colname = s._tnames[s._tcolumn];
        char letter = colname[0];
        switch (letter)
        {
        case 'G':
          s._global = token.ParseLong (lexer);
          break;
        case 'E':
          s._event = token.ParseLong (lexer);
          break;
        case 'T':
          s._time = token.ParseTime (lexer);
          break;
        case 'S':
          s._symbol = token.ParseSymbol (lexer);
          break;
        default: throw new Exception (
                  "Unknown timeline column with letter: " + letter);
        }
      }
      else {
        object val = token.Parse (lexer);
        if (val != null) {
          ColumnBase column = s._cube.GetColumn (s._tnames[s._tcolumn]);
          if (s._canonical && column != null && column is RCCube.ColumnOfNothing) {
            s._cube.UnreserveColumn (s._tnames[s._tcolumn]);
          }
          s._cube.WriteCell (s._tnames[s._tcolumn], s._symbol, val, -1, true, true);
        }
        else {
          s._cube.ReserveColumn (s._tnames[s._tcolumn], canonical: s._canonical);
        }
      }
      // if (s._forceAxisWrite || s._tcolumn == s._tnames.Count - 1)
      if (s._tcolumn == s._tnames.Count - 1) {
        // If there is no time column in the source text then create a
        // series of ascending integers.
        if (!s._hasT) {
          ++s._event;
        }
        s._cube.Axis.Write (s._global, s._event, s._time, s._symbol);
        // s._forceAxisWrite = false;
      }
    }

    public override void AcceptSpacer (object state, RCToken token)
    {
      State s = (State) state;
      if (token.Text == "|") {
        s._hasTimeline = true;
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
        case 'G': G = Binary.ReadVector<long> (array, ref start, sizeof (long)); break;
        case 'E': E = Binary.ReadVector<long> (array, ref start, sizeof (long)); break;
        case 'T': T = Binary.ReadVectorTime (array, ref start); break;
        case 'S': S = Binary.ReadVectorSymbol (array, ref start); break;
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
        case 'x':
          RCArray<byte> datax = Binary.ReadVector<byte> (array, ref start, sizeof (byte));
          columns.Write (new RCCube.ColumnOfByte (timeline, index, datax));
          break;
        case 'd':
          RCArray<double> datad = Binary.ReadVector<double> (array, ref start, sizeof (double));
          columns.Write (new RCCube.ColumnOfDouble (timeline, index, datad));
          break;
        case 'l':
          RCArray<long> datal = Binary.ReadVector<long> (array, ref start, sizeof (long));
          columns.Write (new RCCube.ColumnOfLong (timeline, index, datal));
          break;
        case 'm':
          RCArray<decimal> datam = Binary.ReadVectorDecimal (array, ref start);
          columns.Write (new RCCube.ColumnOfDecimal (timeline, index, datam));
          break;
        case 'n':
          RCArray<RCIncrScalar> datan = Binary.ReadVectorIncr (array, ref start);
          columns.Write (new RCCube.ColumnOfIncr (timeline, index, datan));
          break;
        case 'b':
          RCArray<bool> datab = Binary.ReadVector <bool> (array, ref start, sizeof (bool));
          columns.Write (new RCCube.ColumnOfBool (timeline, index, datab));
          break;
        case 's':
          RCArray<string> datas = Binary.ReadVectorString (array, ref start);
          columns.Write (new RCCube.ColumnOfString (timeline, index, datas));
          break;
        case 'y':
          RCArray<RCSymbolScalar> datay = Binary.ReadVectorSymbol (array, ref start);
          columns.Write (new RCCube.ColumnOfSymbol (timeline, index, datay));
          break;
        default: throw new Exception ("Cannot ReadVector with type " + type);
        }
      }
      return new RCCube (timeline, names, columns);
    }
  }
}

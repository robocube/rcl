
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Formatter : Visitor
  {
    protected static readonly int MIN_WIDTH = 2;
    protected StringBuilder _builder;
    protected RCFormat _args;
    protected RCColmap _colmap;

    // Names of the columns.
    protected List<string> _names;
    // For each column, the list of "stringified" values.
    protected List<List<string>> _columns;
    // For each column, the length of the longest string.
    protected List<int> _max;
    // Align text to the left of the column, otherwise right.
    protected List<bool> _leftAlign;
    // The current column.
    protected int _col = 0;
    // The indent level.
    protected int _level = 0;

    public Formatter (StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      _builder = builder;
      _args = args;
      _level = level;
      _colmap = colmap;
      if (_colmap == null) {
        _colmap = new RCColmap ();
      }
    }

    public void Format (RCCube source)
    {
      if (source.Count == 0) {
        if (_args.Syntax == "RCL") {
          _builder.Append ("[]");
        }
        return;
      }
      _names = new List<string> ();
      _columns = new List<List<string>> ();
      _max = new List<int> ();
      _leftAlign = new List<bool> ();
      int tcols = 0;
      bool useGRows = false;
      if (source.Axis.Has ("G") && _args.Showt) {
        _names.Add ("G");
        _columns.Add (new List<string> ());
        _max.Add (MIN_WIDTH);
        _leftAlign.Add (false);
        ++tcols;
        useGRows = true;
      }
      if (source.Axis.Has ("E") && _args.Showt) {
        _names.Add ("E");
        _columns.Add (new List<string> ());
        _max.Add (MIN_WIDTH);
        _leftAlign.Add (false);
        ++tcols;
      }
      if (source.Axis.Has ("T") && _args.Showt) {
        _names.Add ("T");
        _columns.Add (new List<string> ());
        _max.Add (MIN_WIDTH);
        _leftAlign.Add (false);
        ++tcols;
      }
      if (source.Axis.Has ("S")) {
        _names.Add ("S");
        _columns.Add (new List<string> ());
        _max.Add (MIN_WIDTH);
        _leftAlign.Add (true);
        ++tcols;
      }
      for (int i = 0; i < source.Cols; ++i)
      {
        string name = source.ColumnAt (i);
        char type = source.GetTypeCode (name);
        _names.Add (source.NameAt (i));
        _columns.Add (new List<string> ());
        _max.Add (MIN_WIDTH);
        _leftAlign.Add (type == 'y' || type == 's');
      }
      // Populate _columns and _max.
      if (_args.CanonicalCubes) {
        source.VisitCellsCanonical (this, 0, source.Axis.Count);
      }
      else {
        source.VisitCellsForward (this, 0, source.Axis.Count);
      }
      if (_args.Syntax == "RCL") {
        FormatRC (tcols);
      }
      else if (_args.Syntax == "HTML") {
        FormatHtml (tcols, useGRows);
      }
      else if (_args.Syntax == "CSV") {
        FormatCsv (tcols, useGRows, CSV_ESCAPE_CHARS, true);
      }
      else if (_args.Syntax == "LOG") {
        FormatCsv (tcols, useGRows, LOG_ESCAPE_CHARS, false);
      }
      else {
        throw new Exception ("Unknown syntax for format:" + _args.Syntax);
      }
    }

    protected void FormatRC (int tlcols)
    {
      _builder.Append ("[");
      _builder.Append (_args.Newline);

      // Format the header line with the appropriate
      // column widths determined in VisitCellsForward.
      ++_level;
      for (int i = _args.Fragment ? 1 : 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }

      int[] width = new int[_names.Count];
      for (int i = 0; i < _names.Count; ++i)
      {
        width[i] = Math.Max (_max[i], _names[i].Length);
        if (_args.Align && !_leftAlign[i]) {
          _builder.Append (' ', (width[i] - _names[i].Length));
        }
        _builder.Append (_names[i]);
        // But what about when the name is longer than max?
        // Need to handle that here.  Also different types
        // should be justified differently; strings left, numbers right.
        // Test cases please.
        if (i < _names.Count - 1) {
          if (_args.Align && _leftAlign[i]) {
            _builder.Append (' ', (width[i] - _names[i].Length));
          }
          if (i < tlcols) {
            _builder.Append ("|");
          }
          else {
            _builder.Append (_args.Delimeter);
          }
        }
      }
      _builder.Append (_args.RowDelimeter);

      // Okay now format the individual rows using the gathered
      // strings and widths determined by the visitor.
      int rows;
      if (_columns.Count == 0 || _columns[0] == null) {
        rows = 0;
      }
      else {
        rows = _columns[0].Count;
      }
      for (int row = 0; row < rows; ++row)
      {
        for (int i = _args.Fragment ? 1 : 0; i < _level; ++i)
        {
          _builder.Append (_args.Indent);
        }
        for (int col = 0; col < _names.Count; ++col)
        {
          string scalar = _columns[col][row];
          if (_args.Align && !_leftAlign[col]) {
            _builder.Append (' ', (width[col] - scalar.Length));
          }
          _builder.Append (scalar);
          if (col < _columns.Count - 1) {
            if (_args.Align && _leftAlign[col]) {
              _builder.Append (' ', (width[col] - scalar.Length));
            }
            _builder.Append (_args.Delimeter);
          }
        }
        if (row < rows - 1) {
          _builder.Append (_args.RowDelimeter);
        }
      }
      _builder.Append (_args.Newline);

      --_level;
      for (int i = _args.Fragment ? 1 : 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }
      _builder.Append ("]");
      // _builder.Append (_args.Newline);
    }

    protected void FormatHtml (int tcols, bool useGRows)
    {
      int colOffset = _args.Showt ? 0 : tcols;
      int tlcols = tcols;// + 1;

      for (int i = 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }
      _builder.Append ("<table>");
      _builder.Append (_args.Newline);

      // Format the header line with the appropriate
      // column widths determined in VisitCellsForward.
      ++_level;
      for (int i = 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }

      int[] width = new int[_names.Count];
      _builder.Append ("<thead><tr>");
      for (int i = colOffset; i < _names.Count; ++i)
      {
        width[i] = Math.Max (_max[i], _names[i].Length);
        if (i < tlcols) {
          _builder.AppendFormat (
            "<th id='c{0}' class='{1}'>",
            i,
            _leftAlign[i] ? "txt ch" : "num ch");
        }
        else {
          _builder.AppendFormat (
            "<th id='c{0}' class='{1}'>",
            i,
            _leftAlign[i] ? "txt ch" : "num ch");
        }
        _builder.Append (_names[i]);
        _builder.Append ("</th>");
      }
      _builder.Append ("</tr></thead>");
      _builder.Append (_args.RowDelimeter);

      // Okay now format the individual rows using the gathered
      // strings and widths determined by the visitor.
      int rows = _columns[0].Count;
      for (int row = 0; row < rows; ++row)
      {
        string outRow;
        if (useGRows) {
          outRow = _columns[0][row];
        }
        else {
          outRow = row.ToString ();
        }
        for (int i = 0; i < _level; ++i)
        {
          _builder.Append (_args.Indent);
        }
        _builder.Append ("<tr>");
        for (int col = colOffset; col < _columns.Count; ++col)
        {
          string scalar = _columns[col][row];
          if (col < tlcols) {
            if (row % 2 == 0) {
              _builder.AppendFormat (
                "<th id='r{0}_c{1}' class='{2}'>",
                outRow,
                col,
                _leftAlign[col] ? "txt rh" : "num rh");
            }
            else {
              _builder.AppendFormat (
                "<th id='r{0}_c{1}' class='{2}'>",
                outRow,
                col,
                _leftAlign[col] ? "txt rh" : "num rh");
            }
          }
          else {
            if (row % 2 == 0) {
              _builder.AppendFormat (
                "<td id='r{0}_c{1}' class='{2}'>",
                outRow,
                col,
                _leftAlign[col] ? "txt" : "num");
            }
            else {
              _builder.AppendFormat (
                "<td id='r{0}_c{1}' class='{2}'>",
                outRow,
                col,
                _leftAlign[col] ? "txt" : "num");
            }
          }
          _builder.Append (scalar);
          if (col < tlcols) {
            _builder.Append ("</th>");
          }
          else {
            _builder.Append ("</td>");
          }
        }
        _builder.Append ("</tr>");
        if (row < rows - 1) {
          _builder.Append (_args.RowDelimeter);
        }
      }
      _builder.Append (_args.Newline);

      --_level;
      for (int i = 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }
      _builder.Append ("</table>");
      _builder.Append (_args.Newline);
    }

    protected void FormatCsv (int tcols, bool useGRows, char[] escapeChars, bool head)
    {
      int colOffset = _args.Showt ? 0 : tcols;

      for (int i = 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }

      if (head) {
        int[] width = new int[_names.Count];
        for (int i = colOffset; i < _names.Count; ++i)
        {
          width[i] = Math.Max (_max[i], _names[i].Length);
          _builder.Append (_names[i]);
          if (i < _names.Count - 1) {
            _builder.Append (_args.Delimeter);
          }
        }
        _builder.Append (_args.RowDelimeter);
      }

      // Okay now format the individual rows using the gathered
      // strings and widths determined by the visitor.
      int rows = _columns[0].Count;
      for (int row = 0; row < rows; ++row)
      {
        for (int i = 0; i < _level; ++i)
        {
          _builder.Append (_args.Indent);
        }
        for (int col = colOffset; col < _columns.Count; ++col)
        {
          string scalar = _columns[col][row];
          _builder.Append (CsvEscapeScalar (scalar, escapeChars));
          if (col < _columns.Count - 1) {
            _builder.Append (_args.Delimeter);
          }
        }
        if (row < rows - 1) {
          _builder.Append (_args.RowDelimeter);
        }
      }
      --_level;
      for (int i = 0; i < _level; ++i)
      {
        _builder.Append (_args.Indent);
      }
      _builder.Append (_args.Newline);
    }

    protected static readonly char[] CSV_ESCAPE_CHARS = new char[] {'\r', '\n', ',', '"'};
    protected static readonly char[] LOG_ESCAPE_CHARS = new char[] {'\r', '\n', ' ', '"'};
    protected string CsvEscapeScalar (string scalar, char[] escapeChars)
    {
      string result = scalar;
      int loc = scalar.IndexOf ('"');
      if (loc > -1) {
        result = result.Replace ("\"", "\"\"");
      }
      loc = result.IndexOfAny (escapeChars);
      if (loc > -1) {
        result = string.Format ("\"{0}\"", result);
      }
      return result;
    }

    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      string format = _colmap.GetDisplayFormat (name);
      string scalar = _args.ParsableScalars ? column.ScalarToString (format, row) :
                      column.ScalarToCsvString (format, row);
      int max = _max[_col];
      if (scalar.Length > max) {
        _max[_col] = scalar.Length;
      }
      _columns[_col].Add (scalar);
      _col = (_col + 1) % _names.Count;
    }

    public override void GlobalCol (long grow)
    {
      if (!_args.Showt) {
        return;
      }
      string scalar = grow.ToString ();
      int max = _max[_col];
      if (scalar.Length > max) {
        _max[_col] = scalar.Length;
      }
      _columns[_col].Add (scalar);
      _col = (_col + 1) % _names.Count;
    }

    public override void EventCol (long time)
    {
      if (!_args.Showt) {
        return;
      }
      string scalar = time.ToString ();
      int max = _max[_col];
      if (scalar.Length > max) {
        _max[_col] = scalar.Length;
      }
      _columns[_col].Add (scalar);
      _col = (_col + 1) % _names.Count;
    }

    public override void TimeCol (RCTimeScalar time)
    {
      if (!_args.Showt) {
        return;
      }
      string scalar = time.ToString ();
      int max = _max[_col];
      if (scalar.Length > max) {
        _max[_col] = scalar.Length;
      }
      _columns[_col].Add (scalar);
      _col = (_col + 1) % _names.Count;
    }

    public override void SymbolCol (RCSymbolScalar symbol)
    {
      string scalar = symbol.ToString ();
      int max = _max[_col];
      if (scalar.Length > max) {
        _max[_col] = scalar.Length;
      }
      _columns[_col].Add (scalar);
      _col = (_col + 1) % _names.Count;
    }

    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      _columns[_col].Add (_args.ParsableScalars ? "--" : "");
      _col = (_col + 1) % _names.Count;
    }
  }
}

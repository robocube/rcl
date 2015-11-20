
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class Formatter : Visitor
  {
    protected static readonly int MIN_WIDTH = 2;
  
    protected StringBuilder m_builder;
    protected RCFormat m_args;

    //Names of the columns.
    protected List<string> m_names;
    //For each column, the list of "stringified" values.
    protected List<List<string>> m_columns;
    //For each column, the length of the longest string.
    protected List<int> m_max;
    //Align text to the left of the column, otherwise right.
    protected List<bool> m_leftAlign;
    //The current column.
    protected int m_col = 0;
    //The indent level.
    protected int m_level = 0;
  
    public Formatter (StringBuilder builder, RCFormat args, int level)
                      
    {
      m_builder = builder;
      m_args = args;
      m_level = level;
    }
  
    public void Format (RCCube source)
    {
      if (source.Count == 0)
      {
        m_builder.Append ("[]");
        return;
      }
      m_names = new List<string> ();
      m_columns = new List<List<string>> ();
      m_max = new List<int> ();
      m_leftAlign = new List<bool> ();

      int tcols = 0;
      bool useGRows = false;
      if (source.Axis.Has ("G") && m_args.Showt)
      {
        m_names.Add ("G");
        m_columns.Add (new List<string> ());
        m_max.Add (MIN_WIDTH);
        m_leftAlign.Add (false);
        ++tcols;
        useGRows = true;
      }
      if (source.Axis.Has ("E") && m_args.Showt)
      {
        m_names.Add ("E");
        m_columns.Add (new List<string> ());
        m_max.Add (MIN_WIDTH);
        m_leftAlign.Add (false);
        ++tcols;
      }
      if (source.Axis.Has ("T") && m_args.Showt)
      {
        m_names.Add ("T");
        m_columns.Add (new List<string> ());
        m_max.Add (MIN_WIDTH);
        m_leftAlign.Add (false);
        ++tcols;
      }
      if (source.Axis.Has ("S"))
      {
        m_names.Add ("S");
        m_columns.Add (new List<string> ());
        m_max.Add (MIN_WIDTH);
        m_leftAlign.Add (true);
        ++tcols;
      }
      for (int i = 0; i < source.Cols; ++i)
      {
        string name = source.ColumnAt (i);
        char type = source.GetTypeCode (name);
        m_names.Add (source.NameAt (i));
        m_columns.Add (new List<string> ());
        m_max.Add (MIN_WIDTH);
        m_leftAlign.Add (type == 'y' || type == 's');
      }
      //Populate m_columns and m_max.
      source.VisitCellsForward (this, 0, source.Count);
      if (m_args.Syntax == "RC")
      {
        FormatRC (tcols);
      }
      else if (m_args.Syntax == "HTML")
      {
        FormatHtml (tcols, useGRows);
      }
      else if (m_args.Syntax == "CSV")
      {
        FormatCsv (tcols, useGRows, CSV_ESCAPE_CHARS, true);
      }
      else if (m_args.Syntax == "LOG")
      {
        FormatCsv (tcols, useGRows, LOG_ESCAPE_CHARS, false);
      }
      else throw new Exception ("Unknown syntax for format:" + m_args.Syntax);
    }

    protected void FormatRC (int tlcols)
    {
      //for (int i = 0; i < m_level; ++i)
      //{
      //  m_builder.Append (m_args.Indent);
      //}
      m_builder.Append ("[");
      m_builder.Append (m_args.Newline);

      //Format the header line with the appropriate
      //column widths determined in VisitCellsForward.
      ++m_level;
      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }

      int[] width = new int[m_names.Count];
      for (int i = 0; i < m_names.Count; ++i)
      {
        width[i] = Math.Max (m_max[i], m_names[i].Length);
        if (m_args.Align && !m_leftAlign[i])
        {
          m_builder.Append (' ', (width[i] - m_names[i].Length));
        }
        m_builder.Append (m_names[i]);
        //But what about when the name is longer than max?
        //Need to handle that here.  Also different types
        //should be justified differently; strings left, numbers right.
        //Test cases please.
        if (i < m_names.Count - 1)
        {
          if (m_args.Align && m_leftAlign[i])
          {
            m_builder.Append (' ', (width[i] - m_names[i].Length));
          }
          if (i < tlcols)
          {
            m_builder.Append ("|");
          }
          else
          {
            m_builder.Append (m_args.Delimeter);
          }
        }
      }
      m_builder.Append (m_args.RowDelimeter);
  
      //Okay now format the individual rows using the gathered
      //strings and widths determined by the visitor.
      int rows = m_columns[0].Count;
      for (int row = 0; row < rows; ++row)
      {
        for (int i = 0; i < m_level; ++i)
        {
          m_builder.Append (m_args.Indent);
        }
        for (int col = 0; col < m_names.Count; ++col)
        {
          string scalar = m_columns[col][row];
          if (m_args.Align && !m_leftAlign[col])
          {
            m_builder.Append (' ', (width[col] - scalar.Length));
          }
          m_builder.Append (scalar);
          if (col < m_columns.Count - 1)
          {
            if (m_args.Align && m_leftAlign[col])
            {
              m_builder.Append (' ', (width[col] - scalar.Length));
            }
            m_builder.Append (m_args.Delimeter);
          }
        }
        if (row < rows - 1)
        {
          m_builder.Append (m_args.RowDelimeter);
        }
      }
      m_builder.Append (m_args.Newline);
  
      --m_level;
      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }
      m_builder.Append ("]");
      //m_builder.Append (m_args.Newline);
    }

    protected void FormatHtml (int tcols, bool useGRows)
    {
      int colOffset = m_args.Showt ? 0 : tcols;
      int tlcols = tcols;// + 1;

      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }
      m_builder.Append ("<table>");
      m_builder.Append (m_args.Newline);

      //Format the header line with the appropriate
      //column widths determined in VisitCellsForward.
      ++m_level;
      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }

      int[] width = new int[m_names.Count];
      m_builder.Append ("<thead><tr>");
      for (int i = colOffset; i < m_names.Count; ++i)
      {
        width[i] = Math.Max (m_max[i], m_names[i].Length);
        if (i < tlcols)
        {
          m_builder.AppendFormat (
                                  "<th id='c{0}' class='{1}'>",
                                  i, m_leftAlign[i] ? "txt ch" : "num ch");
        }
        else
        {
          m_builder.AppendFormat (
                                  "<th id='c{0}' class='{1}'>",
                                  i, m_leftAlign[i] ? "txt ch" : "num ch");
        }
        m_builder.Append (m_names[i]);
        m_builder.Append ("</th>");
      }
      m_builder.Append ("</tr></thead>");
      m_builder.Append (m_args.RowDelimeter);

      //Okay now format the individual rows using the gathered
      //strings and widths determined by the visitor.
      int rows = m_columns[0].Count;
      for (int row = 0; row < rows; ++row)
      {
        string outRow;
        if (useGRows)
        {
          outRow = m_columns[0][row];
        }
        else
        {
          outRow = row.ToString ();
        }
        for (int i = 0; i < m_level; ++i)
        {
          m_builder.Append (m_args.Indent);
        }
        m_builder.Append ("<tr>");
        for (int col = colOffset; col < m_columns.Count; ++col)
        {
          string scalar = m_columns[col][row];
          if (col < tlcols)
          {
            if (row % 2 == 0)
            {
              m_builder.AppendFormat (
                                      "<th id='r{0}_c{1}' class='{2}'>",
                                      outRow, col, m_leftAlign[col] ? "txt rh" : "num rh");
            }
            else
            {
              m_builder.AppendFormat (
                                      "<th id='r{0}_c{1}' class='{2}'>",
                                      outRow, col, m_leftAlign[col] ? "txt rh" : "num rh");
            }
          }
          else
          {
            if (row % 2 == 0)
            {
              m_builder.AppendFormat (
                                      "<td id='r{0}_c{1}' class='{2}'>",
                                      outRow, col, m_leftAlign[col] ? "txt" : "num");
            }
            else
            {
              m_builder.AppendFormat (
                                      "<td id='r{0}_c{1}' class='{2}'>",
                                      outRow, col, m_leftAlign[col] ? "txt" : "num");
            }
          }
          m_builder.Append (scalar);
          if (col < tlcols)
          {
            m_builder.Append ("</th>");
          }
          else
          {
            m_builder.Append ("</td>");
          }
        }
        m_builder.Append ("</tr>");
        if (row < rows - 1)
        {
          m_builder.Append (m_args.RowDelimeter);
        }
      }
      m_builder.Append (m_args.Newline);

      --m_level;
      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }
      m_builder.Append ("</table>");
      m_builder.Append (m_args.Newline);
    }

    protected void FormatCsv (int tcols, bool useGRows, char[] escapeChars, bool head)
    {
      int colOffset = m_args.Showt ? 0 : tcols;
      int tlcols; // = tcols;// + 1;

      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }

      if (head)
      {
        int[] width = new int[m_names.Count];
        for (int i = colOffset; i < m_names.Count; ++i)
        {
          width[i] = Math.Max (m_max[i], m_names[i].Length);
          m_builder.Append (m_names[i]);
          if (i < m_names.Count - 1)
          {
            m_builder.Append (m_args.Delimeter);
          }
        }
        m_builder.Append (m_args.RowDelimeter);
      }

      //Okay now format the individual rows using the gathered
      //strings and widths determined by the visitor.
      int rows = m_columns[0].Count;
      for (int row = 0; row < rows; ++row)
      {
        string outRow;
        if (useGRows)
        {
          //A wee bit of ugliness.
          string full = m_columns[0][row];
          outRow = full.Substring (0, full.Length - 1);
        }
        else
        {
          outRow = row.ToString ();
        }
        for (int i = 0; i < m_level; ++i)
        {
          m_builder.Append (m_args.Indent);
        }
        for (int col = colOffset; col < m_columns.Count; ++col)
        {
          string scalar = m_columns[col][row];
          m_builder.Append (CsvEscapeScalar(scalar, escapeChars));
          if (col < m_columns.Count - 1)
          {
            m_builder.Append (m_args.Delimeter);
          }
        }
        if (row < rows - 1)
        {
          m_builder.Append (m_args.RowDelimeter);
        }
      }
      --m_level;
      for (int i = 0; i < m_level; ++i)
      {
        m_builder.Append (m_args.Indent);
      }
      m_builder.Append (m_args.Newline);
    }

    protected static readonly char[] CSV_ESCAPE_CHARS = new char[] {'\r', '\n', ',', '"'};
    protected static readonly char[] LOG_ESCAPE_CHARS = new char[] {'\r', '\n', ' ', '"'};
    protected string CsvEscapeScalar (string scalar, char[] escapeChars)
    {
      string result = scalar;
      int loc = scalar.IndexOf ('"');
      if (loc > -1)
      {
        result = result.Replace ("\"", "\"\"");
      }
      loc = result.IndexOfAny (escapeChars);
      if (loc > -1)
      {
        result = string.Format ("\"{0}\"", result);
      }
      return result;
    }
  
    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      string scalar = m_args.ParsableScalars ? column.ScalarToString (row) : column.Data[row].ToString ();
      int max = m_max[m_col];
      if (scalar.Length > max)
      {
        m_max[m_col] = scalar.Length;
      }
      m_columns[m_col].Add (scalar);
      m_col = (m_col + 1) % m_names.Count;
    }

    public override void GlobalCol (long grow)
    {
      if (!m_args.Showt)
      {
        return;
      }
      string scalar = grow.ToString ();
      int max = m_max[m_col];
      if (scalar.Length > max)
      {
        m_max[m_col] = scalar.Length;
      }
      m_columns[m_col].Add (scalar);
      m_col = (m_col + 1) % m_names.Count;
    }
  
    public override void EventCol (long time)
    {
      if (!m_args.Showt)
      {
        return;
      }
      string scalar = time.ToString ();
      int max = m_max[m_col];
      if (scalar.Length > max)
      {
        m_max[m_col] = scalar.Length;
      }
      m_columns[m_col].Add (scalar);
      m_col = (m_col + 1) % m_names.Count;
    }

    public override void TimeCol (RCTimeScalar time)
    {
      if (!m_args.Showt)
      {
        return;
      }
      string scalar = time.ToString ();
      int max = m_max[m_col];
      if (scalar.Length > max)
      {
        m_max[m_col] = scalar.Length;
      }
      m_columns[m_col].Add (scalar);
      m_col = (m_col + 1) % m_names.Count;
    }
  
    public override void SymbolCol (RCSymbolScalar symbol)
    {
      string scalar = symbol.ToString ();
      int max = m_max[m_col];
      if (scalar.Length > max)
      {
        m_max[m_col] = scalar.Length;
      }
      m_columns[m_col].Add (scalar);
      m_col = (m_col + 1) % m_names.Count;
    }
  
    public override void VisitNull<T> (string name, Column<T> column, int row)
    {
      m_columns[m_col].Add (m_args.ParsableScalars ? "--" : "");
      m_col = (m_col + 1) % m_names.Count;
    }
  }
}
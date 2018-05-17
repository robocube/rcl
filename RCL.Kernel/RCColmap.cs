using System;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCColmap
  {
    protected class DisplayCol
    {
      public readonly string Display;
      public readonly string Format;

      public DisplayCol (string display, string format)
      {
        Display = display;
        Format = format;
      }
    }

    protected Dictionary<string, DisplayCol> m_displayCols = new Dictionary<string, DisplayCol> ();

    public RCColmap Update (RCArray<string> column, RCArray<string> format)
    {
      RCColmap result = new RCColmap ();
      result.m_displayCols = this.m_displayCols;
      foreach (string key in m_displayCols.Keys)
      {
        result.m_displayCols[key] = m_displayCols[key];
      }
      m_displayCols = new Dictionary<string, DisplayCol> ();
      for (int i = 0; i < column.Count; i++)
      {
        result.m_displayCols[column[i]] = new DisplayCol (column[i], format[i]);
      }
      return result;
    }

    public string GetDisplayFormat (string column)
    {
      if (column == null)
      {
        throw new ArgumentNullException ("column");
      }
      DisplayCol col;
      if (m_displayCols.TryGetValue (column, out col))
      {
        return col.Format;
      }
      return null;
    }
  }
}
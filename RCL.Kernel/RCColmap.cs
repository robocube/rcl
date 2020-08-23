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

    protected Dictionary<string, DisplayCol> _displayCols = new Dictionary<string, DisplayCol> ();

    public RCColmap Update (RCArray<string> column, RCArray<string> format)
    {
      RCColmap result = new RCColmap ();
      result._displayCols = this._displayCols;
      string[] keys = new string[this._displayCols.Count];
      _displayCols.Keys.CopyTo (keys, 0);
      foreach (string key in keys)
      {
        result._displayCols[key] = _displayCols[key];
      }
      _displayCols = new Dictionary<string, DisplayCol> ();
      for (int i = 0; i < column.Count; i++)
      {
        result._displayCols[column[i]] = new DisplayCol (column[i], format[i]);
      }
      return result;
    }

    public string GetDisplayFormat (string column)
    {
      if (column == null) {
        throw new ArgumentNullException ("column");
      }
      DisplayCol col;
      if (_displayCols.TryGetValue (column, out col)) {
        return col.Format;
      }
      return null;
    }
  }
}

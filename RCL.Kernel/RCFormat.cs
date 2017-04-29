
using System;
using System.Text;

namespace RCL.Kernel
{
  public class RCFormat
  {
    public static readonly RCFormat Default, Pretty, DefaultNoT, Html, Csv, Log;

    public readonly string Syntax;
    public readonly string Indent;
    public readonly string Newline;
    public readonly string Delimeter;
    public readonly string RowDelimeter;
    public readonly bool Align;
    public readonly bool Showt;
    //public readonly int Level;
    public readonly bool ParsableScalars;

    static RCFormat ()
    {
      Default = new RCFormat ("RCL", "", "", " ", " ", false, true, true);
      Pretty = new RCFormat ("RCL", "  ", "\n", " ", "\n", true, true, true);
      DefaultNoT = new RCFormat ("RCL", "", "", " ", " ", false, false, true);
      Html = new RCFormat ("HTML", "  ", "\n", " ", "\n", true, true, true);
      Csv = new RCFormat ("CSV", "  ", "\n", ",", "\n", false, true, false);
      Log = new RCFormat ("LOG", "  ", "\n", " ", "\n", false, true, false);
    }

    public RCFormat (
      string syntax, string indent, string newline, string delimeter,
      string rowDelimeter, bool align, bool showt, bool parsableScalars)
    {
      Syntax = syntax;
      Indent = indent;
      Newline = newline;
      Delimeter = delimeter;
      RowDelimeter = rowDelimeter;
      Align = align;
      Showt = showt;
      ParsableScalars = parsableScalars;
    }
  }
}

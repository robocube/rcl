
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
    public readonly int Level;
    public readonly bool ParsableScalars;

    static RCFormat ()
    {
      Default = new RCFormat ("RC", "", "", " ", " ", false, true, 0, true);
      Pretty = new RCFormat ("RC", "  ", "\n", " ", "\n", true, true, 0, true);
      DefaultNoT = new RCFormat ("RC", "", "", " ", " ", false, false, 0, true);
      Html = new RCFormat ("HTML", "  ", "\n", " ", "\n", true, true, 0, true);
      Csv = new RCFormat ("CSV", "  ", "\n", ",", "\n", false, true, 0, false);
      Log = new RCFormat ("LOG", "  ", "\n", " ", "\n", false, true, 0, false);
    }

    public RCFormat (
      string syntax, string indent, string newline, string delimeter,
      string rowDelimeter, bool align, bool showt, int level, bool parsableScalars)
    {
      Syntax = syntax;
      Indent = indent;
      Newline = newline;
      Delimeter = delimeter;
      RowDelimeter = rowDelimeter;
      Align = align;
      Showt = showt;
      Level = level;
      ParsableScalars = parsableScalars;
    }
  }
}
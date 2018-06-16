
using System;
using System.Text;

namespace RCL.Kernel
{
  public class RCFormat
  {
    public static readonly RCFormat Default, Pretty, Canonical, DefaultNoT, TestCanonical, Html, Csv, Log;

    public readonly string Syntax;
    public readonly string Indent;
    public readonly string Newline;
    public readonly string Delimeter;
    public readonly string RowDelimeter;
    public readonly bool Align;
    public readonly bool Showt;
    public readonly bool ParsableScalars;
    public readonly bool CanonicalCubes;

    static RCFormat ()
    {
      Default = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:true, parsableScalars:true, canonicalCubes:false);
      Pretty = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:false);
      Canonical = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true);
      DefaultNoT = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:false, parsableScalars:true, canonicalCubes:false);
      TestCanonical = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:false, parsableScalars:true, canonicalCubes:true);
      Html = new RCFormat ("HTML", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:false);
      Csv = new RCFormat ("CSV", "  ", "\n", ",", "\n", align:false, showt:true, parsableScalars:false, canonicalCubes:false);
      Log = new RCFormat ("LOG", "  ", "\n", " ", "\n", align:false, showt:true, parsableScalars:false, canonicalCubes:false);
    }

    public RCFormat (string syntax, string indent, string newline, string delimeter,
                     string rowDelimeter, bool align, bool showt, bool parsableScalars, bool canonicalCubes)
    {
      Syntax = syntax;
      Indent = indent;
      Newline = newline;
      Delimeter = delimeter;
      RowDelimeter = rowDelimeter;
      Align = align;
      Showt = showt;
      ParsableScalars = parsableScalars;
      CanonicalCubes = canonicalCubes;
    }
  }
}

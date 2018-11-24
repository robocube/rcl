
using System;
using System.Text;

namespace RCL.Kernel
{
  public class RCFormat
  {
    public static readonly RCFormat Default, Pretty, Canonical, DefaultNoT, TestCanonical, EditorFragment, Html, Csv, Log;

    public readonly string Syntax;
    public readonly string Indent;
    public readonly string Newline;
    public readonly string Delimeter;
    public readonly string RowDelimeter;
    public readonly bool Align;
    public readonly bool Showt;
    public readonly bool ParsableScalars;
    public readonly bool CanonicalCubes;
    public readonly bool Fragment;

    static RCFormat ()
    {
      Default = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:true, parsableScalars:true, canonicalCubes:false, fragment:false);
      Pretty = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:false, fragment:false);
      // EditorFragment uses canonical cubes because it is intended for use in the editor where I often edit test cases involving cubes
      EditorFragment = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true, fragment:true);
      Canonical = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false);
      DefaultNoT = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:false, parsableScalars:true, canonicalCubes:false, fragment:false);
      TestCanonical = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false);
      Html = new RCFormat ("HTML", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:false, fragment:false);
      Csv = new RCFormat ("CSV", "  ", "\n", ",", "\n", align:false, showt:true, parsableScalars:false, canonicalCubes:false, fragment:false);
      Log = new RCFormat ("LOG", "  ", "\n", " ", "\n", align:false, showt:true, parsableScalars:false, canonicalCubes:false, fragment:false);
    }

    public RCFormat (string syntax, string indent, string newline, string delimeter, string rowDelimeter, bool align,
                     bool showt, bool parsableScalars, bool canonicalCubes, bool fragment)
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
      Fragment = fragment;
    }
  }
}

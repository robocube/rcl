
using System;
using System.Text;

namespace RCL.Kernel
{
  public class RCFormat
  {
    public static readonly RCFormat Default, Pretty, Canonical, DefaultNoT, TestCanonical, EditorFragment, Html, Csv, Log, Json;

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
    public readonly bool UseDisplayCols;

    static RCFormat ()
    {
      Default = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false, useDisplayCols:false);
      Pretty = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false, useDisplayCols:true);
      // EditorFragment uses canonical cubes because it is intended for use in the editor where I often edit test cases involving cubes
      EditorFragment = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true, fragment:true, useDisplayCols:false);
      Canonical = new RCFormat ("RCL", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false, useDisplayCols:false);
      DefaultNoT = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:false, parsableScalars:true, canonicalCubes:true, fragment:false, useDisplayCols:false);
      TestCanonical = new RCFormat ("RCL", "", "", " ", " ", align:false, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false, useDisplayCols:false);
      Json = new RCFormat ("JSON", "  ", "\n", " ", ",\n", align:true, showt:true, parsableScalars:false, canonicalCubes:true, fragment:false, useDisplayCols:false);
      Html = new RCFormat ("HTML", "  ", "\n", " ", "\n", align:true, showt:true, parsableScalars:true, canonicalCubes:true, fragment:false, useDisplayCols:false);
      Csv = new RCFormat ("CSV", "  ", "\n", ",", "\n", align:false, showt:true, parsableScalars:false, canonicalCubes:true, fragment:false, useDisplayCols:false);
      Log = new RCFormat ("LOG", "  ", "\n", " ", "\n", align:false, showt:true, parsableScalars:false, canonicalCubes:true, fragment:false, useDisplayCols:false);
    }

    public RCFormat (string syntax, string indent, string newline, string delimeter, string rowDelimeter, bool align,
                     bool showt, bool parsableScalars, bool canonicalCubes, bool fragment, bool useDisplayCols)
    {
      Syntax = syntax;
      Indent = indent;
      Newline = newline;
      Delimeter = delimeter;
      RowDelimeter = rowDelimeter;
      Align = align;
      Showt = showt;
      ParsableScalars = parsableScalars;
      //CanonicalCubes = canonicalCubes;
      //TODO Remove the argument which is no longer honored.
      CanonicalCubes = true;
      Fragment = fragment;
      UseDisplayCols = useDisplayCols;
    }
  }
}

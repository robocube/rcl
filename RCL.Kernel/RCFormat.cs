
using System;
using System.Text;

namespace RCL.Kernel
{
  public class RCFormat
  {
    public static readonly RCFormat Default,
                                    Pretty,
                                    Canonical,
                                    DefaultNoT,
                                    TestCanonical,
                                    EditorFragment,
                                    Html,
                                    Csv,
                                    Log,
                                    Json;

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
      // Single line format.
      Default = new RCFormat ("RCL",
                              "",
                              "",
                              " ",
                              " ",
                              align: false,
                              showt: true,
                              parsableScalars: true,
                              canonicalCubes: true,
                              fragment: false,
                              useDisplayCols: false);

      // Multiline format.
      Pretty = new RCFormat ("RCL",
                             "  ",
                             "\n",
                             " ",
                             "\n",
                             align: true,
                             showt: true,
                             parsableScalars: true,
                             canonicalCubes: true,
                             fragment: false,
                             useDisplayCols: true);

      // RCL fragment sourced from an editor.
      // Uses canonical cubes for accurate editing.
      EditorFragment = new RCFormat ("RCL",
                                     "  ",
                                     "\n",
                                     " ",
                                     "\n",
                                     align: true,
                                     showt: true,
                                     parsableScalars: true,
                                     canonicalCubes: true,
                                     fragment: true,
                                     useDisplayCols: false);

      // Purist multiline format. Similar to pretty format but doesn't incorporate display cols.
      Canonical = new RCFormat ("RCL",
                                "  ",
                                "\n",
                                " ",
                                "\n",
                                align: true,
                                showt: true,
                                parsableScalars: true,
                                canonicalCubes: true,
                                fragment: false,
                                useDisplayCols: false);

      // Use for tests that do not want to specify G, E and T cols.
      DefaultNoT = new RCFormat ("RCL",
                                 "",
                                 "",
                                 " ",
                                 " ",
                                 align: false,
                                 showt: false,
                                 parsableScalars: true,
                                 canonicalCubes: true,
                                 fragment: false,
                                 useDisplayCols: false);

      // Use for test cases involving canonical cubes.
      TestCanonical = new RCFormat ("RCL",
                                    "",
                                    "",
                                    " ",
                                    " ",
                                    align: false,
                                    showt: true,
                                    parsableScalars: true,
                                    canonicalCubes: true,
                                    fragment: false,
                                    useDisplayCols: false);

      // Translate rcl values to JSON.
      Json = new RCFormat ("JSON",
                           "  ",
                           "\n",
                           " ",
                           ",\n",
                           align: true,
                           showt: true,
                           parsableScalars: false,
                           canonicalCubes: true,
                           fragment: false,
                           useDisplayCols: false);

      // Translate rcl cubes to html tables.
      Html = new RCFormat ("HTML",
                           "  ",
                           "\n",
                           " ",
                           "\n",
                           align: true,
                           showt: true,
                           parsableScalars: true,
                           canonicalCubes: true,
                           fragment: false,
                           useDisplayCols: false);

      // Translate rcl cubes to csv format.
      Csv = new RCFormat ("CSV",
                          "  ",
                          "\n",
                          ",",
                          "\n",
                          align: false,
                          showt: true,
                          parsableScalars: false,
                          canonicalCubes: true,
                          fragment: false,
                          useDisplayCols: false);

      // Translate an appropriate cube to rcl log format.
      Log = new RCFormat ("LOG",
                          "  ",
                          "\n",
                          " ",
                          "\n",
                          align: false,
                          showt: true,
                          parsableScalars: false,
                          canonicalCubes: true,
                          fragment: false,
                          useDisplayCols: false);
    }

    public RCFormat (string syntax,
                     string indent,
                     string newline,
                     string delimeter,
                     string rowDelimeter,
                     bool align,
                     bool showt,
                     bool parsableScalars,
                     bool canonicalCubes,
                     bool fragment,
                     bool useDisplayCols)
    {
      Syntax = syntax;
      Indent = indent;
      Newline = newline;
      Delimeter = delimeter;
      RowDelimeter = rowDelimeter;
      Align = align;
      Showt = showt;
      ParsableScalars = parsableScalars;
      // CanonicalCubes = canonicalCubes;
      // TODO Remove the argument which is no longer honored.
      CanonicalCubes = true;
      Fragment = fragment;
      UseDisplayCols = useDisplayCols;
    }
  }
}

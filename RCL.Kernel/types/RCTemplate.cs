
using System;
using System.Text;

namespace RCL.Kernel
{
  public class RCTemplate : RCBlock
  {
    // I am going to need to store this number but I would like
    // to move it into the block somehow.
    protected readonly int _escapeCount;
    protected readonly bool _multiline;
    protected readonly bool _cr;

    public RCTemplate (RCBlock definition, int escapeCount, bool multiline)
      : base (definition, "", RCEvaluator.Expand, new RCLong (escapeCount))
    {
      _escapeCount = escapeCount;
      _multiline = multiline;
    }

    /// <summary>
    /// Determine the correct escape level for the exception report template by scanning
    /// for embedded template syntax.
    /// </summary>
    public static int CalculateReportTemplateEscapeLevel (string report)
    {
      int nextBracket = report.IndexOf ("?]");
      int questionMarks = 0;
      int maxQuestionMarks = 0;
      while (nextBracket > -1)
      {
        int possibleQuestionMark;
        ++questionMarks;
        maxQuestionMarks = Math.Max (maxQuestionMarks, questionMarks);
        possibleQuestionMark = nextBracket - 1;
        while (possibleQuestionMark >= 0 && report[possibleQuestionMark] == '?')
        {
          --possibleQuestionMark;
          ++questionMarks;
          maxQuestionMarks = Math.Max (maxQuestionMarks, questionMarks);
        }
        nextBracket = report.IndexOf ("?]", nextBracket + 2);
        questionMarks = 0;
      }
      int result = maxQuestionMarks + 1;
      return result;
    }

    public override void Format (StringBuilder builder, RCFormat args, int level)
    {
      RCFormat format = new RCFormat (args.Syntax,
                                      "  ",
                                      args.Newline,
                                      args.Delimeter,
                                      args.RowDelimeter,
                                      args.Align,
                                      args.Showt,
                                      args.ParsableScalars,
                                      args.CanonicalCubes,
                                      args.Fragment,
                                      args.UseDisplayCols);
      RCL.Kernel.Format.DoFormat (this, builder, format, null, level);
    }

    public override void Format (StringBuilder builder, RCFormat args, RCColmap colmap, int level)
    {
      RCFormat format = new RCFormat (args.Syntax,
                                      "  ",
                                      args.Newline,
                                      args.Delimeter,
                                      args.RowDelimeter,
                                      args.Align,
                                      args.Showt,
                                      args.ParsableScalars,
                                      args.CanonicalCubes,
                                      args.Fragment,
                                      args.UseDisplayCols);
      RCL.Kernel.Format.DoFormat (this, builder, format, colmap, level);
    }

    public override string TypeName
    {
      get { return RCValue.TEMPLATE_TYPENAME; }
    }

    public override char TypeCode
    {
      get { return 'p'; }
    }

    public int EscapeCount
    {
      get { return _escapeCount; }
    }

    public bool Multiline
    {
      get { return _multiline; }
    }

    public override bool IsTemplate
    {
      get { return true; }
    }
  }
}
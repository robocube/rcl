using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using RCL.Kernel;

namespace RCL.Core
{
  public class Now
  {
    [RCVerb ("now")]
    public void EvalNow (RCRunner runner, RCClosure closure, RCBlock right)
    {
      runner.Yield (closure, new RCTime (new RCTimeScalar (DateTime.UtcNow.Ticks, RCTimeType.Timestamp)));
    }

    [RCVerb ("now")]
    public void EvalNow (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, new RCTime (new RCTimeScalar (DateTime.UtcNow.Ticks, RCTimeType.Timestamp)));
    }

    [RCVerb ("toLocalTime")]
    public void EvalNow (RCRunner runner, RCClosure closure, RCTime right)
    {
      RCArray<RCTimeScalar> result = new RCArray<RCTimeScalar> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        DateTime date = new DateTime (right[i].Ticks);
        DateTime adjustedDate = date.ToLocalTime ();
        result.Write (new RCTimeScalar (adjustedDate, right[i].Type));
      }
      runner.Yield (closure, new RCTime (result));
    }

    /// <summary>
    /// Return the date corresponding to the following day of week eg "Friday."
    /// The date returned is inclusive of the current day.
    /// </summary>
    [RCVerb ("nextDayOfWeek")]
    public void EvalNextDayOfWeek (RCRunner runner, RCClosure closure, RCTime left, RCString right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("Only one day of week allowed");
      }
      RCArray<RCTimeScalar> result = new RCArray<RCTimeScalar> (left.Count);
      DayOfWeek dayOfWeek = (DayOfWeek) Enum.Parse (typeof (DayOfWeek), right[0], ignoreCase:true);
      for (int i = 0; i < left.Count; ++i)
      {
        DateTime date = new DateTime (left[i].Ticks);
        int daysUntil = dayOfWeek - date.DayOfWeek;
        DateTime targetDate = date;
        if (daysUntil < 0)
        {
          daysUntil += 7;
        }
        targetDate = date.AddDays (daysUntil);
        result.Write (new RCTimeScalar (targetDate, RCTimeType.Date));
      }
      runner.Yield (closure, new RCTime (result));
    }

    [RCVerb ("dayOfWeek")]
    public void EvalDayOfWeek (RCRunner runner, RCClosure closure, RCTime right)
    {
      RCArray<string> result = new RCArray<string> (right.Count);
      for (int i = 0; i < right.Count; ++i)
      {
        DateTime date = new DateTime (right[i].Ticks);
        result.Write (date.DayOfWeek.ToString ());
      }
      runner.Yield (closure, new RCString (result));
    }

    [RCVerb ("addDays")]
    public void EvalNextDay (RCRunner runner, RCClosure closure, RCTime left, RCLong right)
    {
      if (right.Count != 1)
      {
        throw new Exception ("Only one number of days to add allowed");
      }
      RCArray<RCTimeScalar> result = new RCArray<RCTimeScalar> (left.Count);
      for (int i = 0; i < left.Count; ++i)
      {
        DateTime date = new DateTime (left[i].Ticks).AddDays (1);
        result.Write (new RCTimeScalar (date, left[i].Type));
      }
      runner.Yield (closure, new RCTime (result));
    }
  }
}

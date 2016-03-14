
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class SpecRecord
  {
    public SpecRecord (RCSymbolScalar scalar)
    {
      original = scalar;
      RCSymbolScalar current = scalar;
      while (current != null)
      {
        if (current.Key.Equals ("*"))
        {
          Concrete = false;
          scalar = current.Previous;
          if (current.Length < scalar.Length)
          {
            LeadingStar = true;
          }
        }
        current = current.Previous;
      }
      symbol = scalar;
    }

    //If false, it means read symbols under this one.
    public readonly bool Concrete = true;
    //True if symbol contains a leading star.
    public readonly bool LeadingStar = false;
    //The symbol being tracked.
    public RCSymbolScalar symbol;
    public RCSymbolScalar original;
    //True for dispatch and false for regular reads.
    public bool ignoreDispatchedRows;
    //The number of records accumulated so far.
    public int count = 0;
    //Beginning of the range you need to search for rows.
    public int start = 0;
    //Number of rows to return per symbol.
    public int limit = 0;
  }
}
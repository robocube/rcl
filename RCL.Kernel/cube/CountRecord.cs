
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class CountRecord
  {
    public CountRecord (RCSymbolScalar scalar, bool concrete)
    {
      symbol = scalar;
      Concrete = concrete;
    }

    //If false, the count is for an abstract symbol.
    public readonly bool Concrete;
    //The referenced symbol.
    public RCSymbolScalar symbol;
    //Beginning of the range you need to search for rows.
    public int start = 0;
    //The position of the last row with this symbol.
    public int end = -1;
    //Number of undispatched rows between start and end inclusive.
    public int count = 0;
    //The total number of rows written whether dispatched or not.
    public int total = 0;
    //True if the symbol has been deleted
    public bool deleted = false;
  }
}


using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Constant
  {
    protected static Dictionary<RCSymbolScalar, RCValue> _values =
      new Dictionary<RCSymbolScalar, RCValue> ();

    static Constant ()
    {
      _values[new RCSymbolScalar (null, "pi")] = new RCDouble (Math.PI);
      _values[new RCSymbolScalar (null, "e")] = new RCDouble (Math.E);
      _values[new RCSymbolScalar (null, "maxl")] = new RCLong (long.MaxValue);
      _values[new RCSymbolScalar (null, "minl")] = new RCLong (long.MinValue);
      _values[new RCSymbolScalar (null, "maxd")] = new RCDouble (double.MaxValue);
      _values[new RCSymbolScalar (null, "mind")] = new RCDouble (double.MinValue);
      _values[new RCSymbolScalar (null, "maxm")] = new RCDecimal (decimal.MaxValue);
      _values[new RCSymbolScalar (null, "minm")] = new RCDecimal (decimal.MinValue);
      _values[new RCSymbolScalar (null, "nan")] = new RCDouble (double.NaN);
      _values[new RCSymbolScalar (null, "epsilon")] = new RCDouble (double.Epsilon);
      _values[new RCSymbolScalar (null, "infinity")] = new RCDouble (double.PositiveInfinity);
      _values[new RCSymbolScalar (null, "ninfinity")] = new RCDouble (double.NegativeInfinity);
    }

    [RCVerb ("constant")]
    public void EvalConstant (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, _values[right[0]]);
    }
  }
}

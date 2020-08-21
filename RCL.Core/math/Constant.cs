
using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class Constant
  {
    protected static Dictionary<RCSymbolScalar, RCValue> m_values =
      new Dictionary<RCSymbolScalar, RCValue> ();

    static Constant ()
    {
      m_values[new RCSymbolScalar (null, "pi")] = new RCDouble (Math.PI);
      m_values[new RCSymbolScalar (null, "e")] = new RCDouble (Math.E);
      m_values[new RCSymbolScalar (null, "maxl")] = new RCLong (long.MaxValue);
      m_values[new RCSymbolScalar (null, "minl")] = new RCLong (long.MinValue);
      m_values[new RCSymbolScalar (null, "maxd")] = new RCDouble (double.MaxValue);
      m_values[new RCSymbolScalar (null, "mind")] = new RCDouble (double.MinValue);
      m_values[new RCSymbolScalar (null, "maxm")] = new RCDecimal (decimal.MaxValue);
      m_values[new RCSymbolScalar (null, "minm")] = new RCDecimal (decimal.MinValue);
      m_values[new RCSymbolScalar (null, "nan")] = new RCDouble (double.NaN);
      m_values[new RCSymbolScalar (null, "epsilon")] = new RCDouble (double.Epsilon);
      m_values[new RCSymbolScalar (null, "infinity")] = new RCDouble (double.PositiveInfinity);
      m_values[new RCSymbolScalar (null, "ninfinity")] = new RCDouble (double.NegativeInfinity);
    }

    [RCVerb ("constant")]
    public void EvalConstant (RCRunner runner, RCClosure closure, RCSymbol right)
    {
      runner.Yield (closure, m_values[right[0]]);
    }
  }
}

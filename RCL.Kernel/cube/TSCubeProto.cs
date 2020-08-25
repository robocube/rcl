
namespace RCL.Kernel
{
  public class TSCubeProto : CubeProto
  {
    public TSCubeProto (Timeline axis) : base (axis) { }

    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      RCAssert.AxisHasT (axis1, "CompareAxisRows: axis1 must contain the T column");
      RCAssert.AxisHasT (axis2, "CompareAxisRows: axis2 must contain the T column");
      RCTimeScalar timeX = axis1.Time[i1];
      RCTimeScalar timeY = axis2.Time[i2];
      int compareResult = timeX.CompareTo (timeY);
      if (compareResult == 0)
      {
        RCSymbolScalar symbolX = axis1.SymbolAt (i1);
        RCSymbolScalar symbolY = axis2.SymbolAt (i2);
        return symbolX.CompareTo (symbolY);
      }
      else
      {
        return compareResult;
      }
    }
  }
}


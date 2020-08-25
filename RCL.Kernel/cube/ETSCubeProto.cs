
namespace RCL.Kernel
{
  public class ETSCubeProto : CubeProto
  {
    public ETSCubeProto (Timeline axis) : base (axis) { }

    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      long eventX = axis1.Event[i1];
      long eventY = axis2.Event[i2];
      int compareResult = eventX.CompareTo (eventY);
      if (compareResult == 0)
      {
        RCTimeScalar timeX = axis1.Time[i1];
        RCTimeScalar timeY = axis2.Time[i2];
        compareResult = timeX.CompareTo (timeY);
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
      else
      {
        return compareResult;
      }
    }
  }
}
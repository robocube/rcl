
namespace RCL.Kernel
{
  public class SCubeProto : CubeProto
  {
    public SCubeProto (Timeline axis) : base (axis) { }

    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      RCSymbolScalar symbolX = axis1.SymbolAt (i1);
      RCSymbolScalar symbolY = axis2.SymbolAt (i2);
      return symbolX.CompareTo (symbolY);
    }
  }
}
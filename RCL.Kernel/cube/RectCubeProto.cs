
namespace RCL.Kernel
{
  public class RectCubeProto : CubeProto
  {
    public RectCubeProto (Timeline axis) : base (axis) { }
    public override int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2)
    {
      return i1.CompareTo (i2);
    }
  }
}

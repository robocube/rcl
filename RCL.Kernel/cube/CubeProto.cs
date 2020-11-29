
using System;

namespace RCL.Kernel
{
  /// <summary>
  /// Encapsulate differences between different types of cubes.
  /// Presence or absence of the G column does not affect the CubeProto.
  /// This class is mostly a way to share comparer operations between SortAxis and
  /// IsAxisSorted.
  /// </summary>
  public abstract class CubeProto
  {
    public static CubeProto Create (Timeline axis)
    {
      if (axis.Symbol != null) {
        if (axis.Time != null) {
          if (axis.Event != null) {
            return new ETSCubeProto (axis);
          }
          else {
            return new TSCubeProto (axis);
          }
        }
        else {
          return new SCubeProto (axis);
        }
      }
      else if (axis.Time != null) {
        return new TCubeProto (axis);
      }
      else {
        return new RectCubeProto (axis);
      }
    }

    protected Timeline _axis;
    public CubeProto (Timeline axis)
    {
      if (axis == null)
      {
        throw new ArgumentNullException ("axis");
      }
      _axis = axis;
    }

    public abstract int CompareAxisRows (Timeline axis1, int i1, Timeline axis2, int i2);
    public virtual int CompareAxisRows (int i1, int i2)
    {
      return CompareAxisRows (_axis, i1, _axis, i2);
    }

    public Timeline Sort ()
    {
      if (_axis.Proto.IsAxisSorted ())
      {
        return _axis;
      }
      else
      {
        return RankUtils.ApplyAxisRank (_axis, RankUtils.DoAxisRank (_axis));
      }
    }

    public bool IsAxisSorted ()
    {
      for (int i = 1; i < _axis.Count; ++i)
      {
        if (CompareAxisRows (i - 1, i) > 0)
        {
          return false;
        }
      }
      return true;
    }
  }
}

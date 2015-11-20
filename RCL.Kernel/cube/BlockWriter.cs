
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class BlockWriter : Visitor
  {
    protected RCCube m_source;
    protected RCBlock m_target = RCBlock.Empty;
    protected RCBlock m_row = RCBlock.Empty;
    protected string m_rowName = "";

    public BlockWriter (RCCube source)
    {
      m_source = source;
    }

    public RCBlock Write ()
    {
      m_source.VisitCellsForward (this, 0, m_source.Count);
      return m_target;
    }
  
    public override void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar symbol, int row)
    {
      if (m_source.Axis.Has ("G"))
      {
        m_row = new RCBlock (RCBlock.Empty, "G", ":", new RCLong (m_source.Axis.Global[row]));
      }
      if (m_source.Axis.Has ("E"))
      {
        m_row = new RCBlock (m_row, "E", ":", new RCLong (m_source.Axis.Event[row]));
      }
      if (m_source.Axis.Has ("T"))
      {
        m_row = new RCBlock (m_row, "T", ":", new RCTime (m_source.Axis.Time[row]));
      }
      if (m_source.Axis.Has ("S"))
      {
        m_rowName = m_source.Axis.Symbol[row].Key.ToString ();
      }
    }
  
    public override void VisitScalar<T> (string name, Column<T> column, int row)
    {
      m_row = new RCBlock (m_row, name, ":",
                           RCVectorBase.FromArray (new RCArray<T>(column.Data[row])));
    }

    public override void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row)
    {
      m_target = new RCBlock (m_target, m_rowName, ":", m_row);
      m_row = RCBlock.Empty;
      m_rowName = "";
    }
  }
}
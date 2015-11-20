
namespace RCL.Kernel
{
  public class Visitor
  {
    public virtual void VisitNull<T> (string name, Column<T> vector, int row) { }
    public virtual void VisitScalar<T> (string name, Column<T> column, int row) {}
    public virtual void BeforeCol (long col, string name) { }
    public virtual void GlobalCol (long g) {}
    public virtual void EventCol (long e) { }
    public virtual void TimeCol (RCTimeScalar time) { }
    public virtual void SymbolCol (RCSymbolScalar symbol) { }
    public virtual void BetweenCols (long prior) { }
    public virtual void BeforeRow (long e, RCTimeScalar t, RCSymbolScalar s, int row) { }
    public virtual void AfterRow (long e, RCTimeScalar t, RCSymbolScalar s, int row) { }
    public virtual void BetweenRows (int row) { }
  }
}
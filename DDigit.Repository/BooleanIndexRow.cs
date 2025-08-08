namespace DDigit.Repository;

public class BooleanIndexRow : IndexRow
{
  public BooleanIndexRow(IndexData index, int id) : base(index, null, 1, id)
  {
  }

  public BooleanIndexRow(IndexData index, object value, int id) : base(index, null, 1, id)
  {
    Term = value;
    DisplayTerm = value;
  }

  public override string ToString() => $"{Term} ({DisplayTerm}), {Id} [{Count}]";
}

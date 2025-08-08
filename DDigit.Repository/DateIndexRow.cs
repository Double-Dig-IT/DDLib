namespace DDigit.Repository;

public class DateIndexRow : IndexRow
{
  public DateIndexRow(IndexData index, int id) : base(index, null, 1, id)
  {
  }

  public DateIndexRow(IndexData index, object value, int id) : base(index, null, 1, id)
  {
    Term = KeyConversions.DateTimeStringToInt((string?)value);
    DisplayTerm = (string?)value;
  }

  public override string ToString() => $"{Term} ({DisplayTerm}), {Id} [{Count}]";
}

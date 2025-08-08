namespace DDigit.Repository;

public class IsoDateIndexRow : DateIndexRow
{
  public IsoDateIndexRow(IndexData index, object value, int id) : base(index, id)
  {
    var isoDate = new IsoDate((string)value, index.DateCompletion);
    Term = isoDate.ToDecimal();
    DisplayTerm = isoDate.ToString();
  }
}

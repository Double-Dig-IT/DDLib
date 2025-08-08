namespace DDigit.Repository;

public class IntegerIndexRow : IndexRow
{
  public IntegerIndexRow(IndexData index, int term, int id) : base(index, null, 1, id)
  {
    Term = term;
  }

  public override string ToString() => $"{Term} {Id} [{Count}]";
}
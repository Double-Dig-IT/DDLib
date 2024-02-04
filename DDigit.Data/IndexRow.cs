namespace DDigit.Data;

public abstract class IndexRow(string table, int id)
{
  public string Table
  {
    get; private set;
  } = table;

  public int Id
  {
    get; private set;
  } = id;

  public int Count
  {
    get; set;
  }
}
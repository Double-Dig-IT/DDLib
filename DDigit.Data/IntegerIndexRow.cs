namespace DDigit.Data;

public class IntegerIndexRow(string table, int key, int id) : 
  IndexRow(table, id)
{
  public int Key
  {
    get;
  } = key;

  public override string ToString() => $"{Key} {Id} ({Count})";
}
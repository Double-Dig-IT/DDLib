namespace DDigit.Repository;

public abstract class IndexRow(IndexData index, string? tag, int occ, int id)
{
  public IndexData? Index { get; set; } = index;

  public string Table { get; private set; } = index.TableName;

  public string? Tag { get; private set; } = tag;

  public int Occ { get; internal set; } = occ;

  public int Id { get; private set; } = id;

  public int Count { get; set; }

  public object? Term
  {
    get; protected set;
  }

  public object? DisplayTerm
  {
    get; protected set;
  }

  public override string ToString() => $"{Tag} {Table} '{DisplayTerm}' {Id} ({Count})";
}
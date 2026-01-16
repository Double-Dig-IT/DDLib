namespace DDigit.Search;

public class RecordSet
{
  public required RecordSetMetaData MetaData { get; init; }
  public required ResultSet Set { get; init; }
  public required DatabaseData Database { get; init; }

  public bool Contains(int id) => Set.Ids.Contains(id);
}

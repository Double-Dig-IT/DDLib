namespace DDigit.MetaData;

public class UpdateLinksData : Dictionary<DatabaseData, List<FieldData>>
{
  internal UpdateLinksData(DatabaseData databaseData)
  {
    foreach (var database in GetDatabases(databaseData))
    {
      if (database != databaseData)
      {
        foreach (var field in database.Fields.Where(
                        f => f.IsLinked && f.LinkedDatabase == databaseData))
        {
          Add(database, field);
        }
      }
    }
  }

  private void Add(DatabaseData database, FieldData field)
  {
    if (!TryGetValue(database, out var list))
    {
      list = this[database] = [];
    }
    list.Add(field);
  }

  private static IEnumerable<DatabaseData> GetDatabases(DatabaseData databaseData)
  {
    if (databaseData.PhysicalPath == null)
    {
      throw new NullReferenceException(nameof(databaseData.PhysicalPath));
    }
    string folder;
    folder = Path.GetDirectoryName(databaseData.PhysicalPath) ??
      throw new NullReferenceException(nameof(folder));
    return MetaDataCache.FindDatabases(folder);
  }


}

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<DatabaseData> GetDatabase(string folder) =>
    MetaDataCache.FindDatabases(folder);

  public DatabaseData? GetDatabase(string? folder, string? database)
  {
    if (string.IsNullOrWhiteSpace(folder))
    {
      throw new ArgumentNullException(nameof(folder));
    }
    if (string.IsNullOrWhiteSpace(database))
    {
      throw new ArgumentNullException(nameof(database));
    }
    return MetaDataCache.ReadDatabase(folder, database, false);
  }
}

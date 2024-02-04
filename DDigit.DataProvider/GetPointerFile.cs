namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<List<RecordSetMetaData>> GetRecordSetMetaData(string folder, string? databaseName, HashSet<int>? sets)
  {
    var result = new List<RecordSetMetaData>();
    foreach (var databaseData in MetaDataCache.FindDatabases(folder, databaseName))
    {
      foreach (var set in await GetRecordSetPerDatabase(databaseData))
      {
        if (sets == null || sets.Contains(set.Number))
        {
          result.Add(set);
        }
      }
    }
    return result;
  }
}

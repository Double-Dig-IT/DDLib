namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<RecordSetList> GetRecordSetMetaDataAsync(string folder, string? databaseName, HashSet<int>? sets = null,
                                                                  string? searchTerm = null, int startFrom = 1, int limit = 0, RecordSetSortEnum? sort = null, SearchSortOrderEnum? sortOrder = null,
                                                                  CancellationToken cancellationToken = default)
  {
    var result = new RecordSetList();

    int current = 1;
    int hits = 0;
    foreach (var databaseData in MetaDataCache.FindDatabases(folder, databaseName))
    {
      foreach (var set in await GetRecordSetMetaDataPerDatabase(databaseData, searchTerm, 1, 0, sort, sortOrder, cancellationToken))
      {
        if (sets == null || sets.Contains(set.Number))
        {
          hits++;
          if (current++ < startFrom || (limit > 0 && result.Count >= limit))
          {
            continue;
          }
          result.Add(set);
        }
      }
    }
    result.Hits = hits;
    return result;
  }

  public async Task<RecordSetMetaData> GetRecordSetMetaDataAsync(DatabaseData database, int set,
                                                                 CancellationToken cancellationToken = default)
  {
    if (set < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(set), "Set number must be greater than zero.");
    }
    var result = await Repository.GetResultSetMetaDataAsync(database, set, cancellationToken);
    return result ?? throw new RecordSetNotFoundException(database.Name!, set);
  }
}

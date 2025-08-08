namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Task<RecordSetList> GetRecordSetMetaDataPerDatabase(DatabaseData database, string? searchTerm = null, 
                                                  int startFrom = 1, int limit = 1,
                                                  CancellationToken cancellationToken = default)
   => Repository.GetRecordSetMetaDataPerDatabaseAsync(database, searchTerm, startFrom, limit, cancellationToken);
}

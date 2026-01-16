namespace DDigit.DataProvider
{
  public partial class DDataProvider : IDataProvider
  {
    public async Task<RecordSet> ReadRecordSetAsync(string folder, string database, int set, 
      CancellationToken cancellationToken = default)
    {
      var databaseData = MetaDataCache.ReadDatabase(folder, database, false)
        ?? throw new DatabaseNotFoundException(folder, database);
      return new RecordSet
      {
        Database = databaseData,
        Set = await GetRecordSetAsync(databaseData, set, cancellationToken),
        MetaData = await GetRecordSetMetaDataAsync(databaseData, set, cancellationToken: cancellationToken)
      };
    }
  }
}

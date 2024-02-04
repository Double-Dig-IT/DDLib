namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<ResultSet> GetRecordSet(string folder, string database, int set)
    => await GetRecordSet(MetaDataCache.ReadDatabase(folder, database, false) ??
                        throw new DatabaseNotFoundException(folder, database), set);

  public async Task<ResultSet> GetRecordSet(DatabaseData databaseData, int set)
  {
    var result = await Repository.GetResultSet(databaseData, set, null, null);
    result.WorkingDirectory = Path.GetDirectoryName(databaseData.PhysicalPath);
    return result;
  }
}

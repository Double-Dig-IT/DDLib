namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<ResultSet> GetRecordSetAsync(string folder, string database, int set, CancellationToken cancellationToken)
    => await GetRecordSetAsync(MetaDataCache.ReadDatabase(folder, database, false) ??
                        throw new DatabaseNotFoundException(folder, database), set, cancellationToken);

  private async Task<ResultSet> GetRecordSetAsync(DatabaseData databaseData, int set, CancellationToken cancellationToken)
  {
    var searchTree = new SearchTree
    {
      Database = databaseData,
      SqlState = new SqlStateInfo
      {
        Connection = null,
        Transaction = null,
        CancellationToken = cancellationToken
      }
    };
    var result = await Repository.GetResultSetAsync(searchTree, set);
    result.WorkingDirectory = Path.GetDirectoryName(databaseData.FileName);
    return result;
  }
}

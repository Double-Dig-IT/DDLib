namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Task<int> WriteRecordSetAsync(string folder, RecordSetMetaData metaData, ResultSet set, CancellationToken cancellationToken = default)
    => Repository.WriteRecordSetAsync(folder, metaData, set, cancellationToken);

  public Task DeleteRecordSetAsync(string folder, string database, int setNo, CancellationToken cancellationToken = default)
    => Repository.DeleteRecordSetAsync(folder, database, setNo, cancellationToken);
}

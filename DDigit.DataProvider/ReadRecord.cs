namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Task<Record?> ReadRecordAsync(string path, string databaseName,
    int id, CancellationToken cancellationToken = default)

    => ReadRecordAsync(GetDatabase(path, databaseName) ??
      throw new DatabaseNotFoundException(path, databaseName), id, new SqlStateInfo { CancellationToken = cancellationToken });

  public Task<Record?> ReadRecordAsync(DatabaseData databaseData, int id, CancellationToken cancellationToken = default)
    => ReadRecordAsync(databaseData, id, new SqlStateInfo { CancellationToken = cancellationToken });
}

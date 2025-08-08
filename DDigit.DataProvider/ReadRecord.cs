namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Task<Record?> ReadRecordAsync(string path, string databaseName,
    int id, CancellationToken cancellationToken = default) =>
    ReadRecordAsync(GetDatabase(path, databaseName) ??
      throw new DatabaseNotFoundException(path, databaseName), id, null, null, cancellationToken);
}

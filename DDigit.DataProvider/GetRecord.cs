namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<Record?> ReadRecordAsync(DatabaseData database, int id,
    IDbConnection? connection = null,
    IDbTransaction? transaction = null,
    CancellationToken cancellationToken = default)
  {
    var data = await Repository.ReadDataAsync(database, id, connection, transaction, cancellationToken);
    return data is not DBNull ? new Record(this, id, data, database) : null;
  }
}

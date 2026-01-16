namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<Record?> ReadRecordAsync(DatabaseData database, int id, SqlStateInfo sqlState)
  {
    var data = await Repository.ReadDataAsync(database, id, sqlState);
    return data is not null && data is not DBNull ? new Record(this, id, data, database) : null;
  }
}

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<Record?> GetRecord(DatabaseData database, string table, int id)
  {
    var data = await Repository.ReadData(database, table, id);
    return data != null ? new Record(this, id, data, database) : null;
  }
}

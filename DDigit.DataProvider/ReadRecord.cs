namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<Record?> ReadRecord(string path, string table, int id) =>
    await GetRecord(GetDatabase(path, table) ??
      throw new DatabaseNotFoundException(path, table), table, id);
}

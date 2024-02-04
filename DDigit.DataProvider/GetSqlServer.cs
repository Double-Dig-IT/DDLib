namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<SqlSetting> GetSqlServer(string folder)
  {
    var result = new List<SqlSetting>();
    foreach (var database in MetaDataCache.FindDatabases(folder))
    {
      result.Add(
        new SqlSetting
        {
          Database = database.DSN,
          Table = database.Name,
          Server = database.SqlServer,
          User = database.SqlUserId,
          Password = database.SqlPassword,
        });
    }
    return result;
  }
}

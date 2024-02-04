namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  private async Task<List<RecordSetMetaData>> GetRecordSetPerDatabase(DatabaseData database)
   => await Repository.GetRecordSetPerDatabase(database);
}

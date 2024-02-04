namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<IEnumerable<RecordLock>> GetRecordLock(string folder)
  {
    var databaseData = MetaDataCache.FirstDatabase(folder, false);
    if (databaseData == null)
    {
      throw new NullReferenceException(nameof(databaseData));
    }
    return await Repository.GetRecordLock(databaseData);
  }
}

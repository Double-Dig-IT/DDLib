namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<string> GetCacheEntries()
    => MetaDataCache.GetCacheEntries();
}

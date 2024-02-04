namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public ApplicationData GetApplication(string folder)
  => MetaDataCache.ReadApplication(folder, false) ?? throw new ApplicationNotFoundException(folder);
}

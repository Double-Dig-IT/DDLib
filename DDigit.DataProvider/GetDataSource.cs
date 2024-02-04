namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<DataSourceData> GetDatasource(string folder)
    => GetApplication(folder).DataSources;
}

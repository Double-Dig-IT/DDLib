namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<FieldData> GetTaskField(string folder)
  {
    var result = new List<FieldData>();
    var application = MetaDataCache.ReadApplication(folder, false);
    application?.DataSources.ForEach(dataSource =>
        dataSource.Tasks.ForEach(task =>
          result.AddRange(task.Fields)));
    return result;
  }
}

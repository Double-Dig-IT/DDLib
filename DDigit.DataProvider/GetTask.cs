namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<TaskData> GetTask(string folder)
  {
    var result = new List<TaskData>();
    GetApplication(folder).DataSources.ForEach(ds =>
      ds.Tasks.ForEach(task => result.Add(task)));
    return result;
  }
}

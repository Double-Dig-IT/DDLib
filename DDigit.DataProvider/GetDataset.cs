namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<DatasetData> GetDataset(string folder, string? databaseName)
  {
    var result = new List<DatasetData>();
    foreach (var database in GetDatabase(folder))
    {
      if (databaseName == null || MatchList.Match(databaseName, database.Name!))
      {
        result.AddRange(database.Datasets);
      }
    }
    return result;
  }
}

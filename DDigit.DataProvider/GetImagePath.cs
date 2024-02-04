namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<ImageData> GetImagePath(string folder, string? databaseName)
  {
    var result = new List<ImageData>();
    foreach (var database in GetDatabase(folder))
    {
      if (databaseName == null || MatchList.Match(databaseName, database.Name))
      {
        result.AddRange(database.Fields.Where(field => field.Type == FieldTypeEnum.Image)
          .Select(field => new ImageData(database.Name, field)));
      }
    }
    return result;
  }
}

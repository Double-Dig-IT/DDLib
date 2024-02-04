namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<FieldData> GetField(string folder, string? databaseName,
        string? tag, FieldTypeEnum? type, bool? isLinked, string? fieldName, string? group)
  {
    var result = new List<FieldData>();
    foreach (var database in GetDatabase(folder))
    {
      if (databaseName == null || MatchList.Match(databaseName, database.Name!))
      {
        result.AddRange(database.Fields.Where(field =>
           MatchList.Match(tag, field.Tag) &&
           (type == null || field.Type == type) &&
           (isLinked == null || !string.IsNullOrEmpty(field.LinkIndexTag)) &&
           MatchList.Match(fieldName, field.Name) &&
           MatchList.Match(group, field.Group)));
      }
    }
    return result;
  }
}

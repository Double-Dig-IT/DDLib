namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<MergedFieldData> GetMergedField(string folder)
  {
    var result = new List<MergedFieldData>();
    foreach (var database in GetDatabase(folder))
    {
      foreach (var field in database.Fields)
      {
        if (!string.IsNullOrWhiteSpace(field.LinkIndexTag) &&
            !string.IsNullOrWhiteSpace(field.LinkedDatabasePath))
        {
          var linkedDatabase = field.LinkedDatabasePath == "=" ?
            database : MetaDataCache.ReadDatabase(Path.Combine(folder, field.LinkedDatabasePath), false);

          result.AddRange(field.MergeTags.Select(mergePair => new MergedFieldData
          {
            Database = database.Name,
            Tag = field.Tag,
            Name = field.Name,
            LinkedDatabase = linkedDatabase != null ? linkedDatabase.Name : $"[{field.LinkedDatabasePath}]",
            SourceTag = mergePair.Source,
            SourceName = linkedDatabase != null ? linkedDatabase.GetFieldNameByTag(mergePair.Source) : $"[{mergePair.Source}]",
            DestinationTag = mergePair.Destination,
            DestinationName = database.GetFieldNameByTag(mergePair.Destination)
          }));
        }
      }
    }
    return result;
  }

}

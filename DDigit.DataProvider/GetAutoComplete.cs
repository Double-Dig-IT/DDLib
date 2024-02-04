
namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<AutoCompleteResult?> GetAutoComplete(string folder, string database, string[]? datasets, string[] fields, string? value, int? startFrom, int? limit, string? language, bool count)
  {
    var databaseData = MetaDataCache.ReadDatabase(folder, database, false);

    if (databaseData == null)
    {
      return null;
    }

    var datasetFilter = datasets != null ? new DatasetFilter(databaseData, datasets) : null;
    var fieldData = fields.Select(field => databaseData.FindFieldByTagOrName(field) ?? throw new FieldNotFoundException(field, database));
      
    return await Repository.GetAutoComplete(fieldData, datasetFilter, value, startFrom, limit, language, count);
  }
}

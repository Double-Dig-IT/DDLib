namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<List<Record>> SearchLocationsAsync(DatabaseData locations, string nameField, string barcodeField, string value)
  {
    var limits = new SearchLimits(1, 25); // Default start from the first record, limit to 100 records
    var nodes = await Repository.SearchLocationsAsync(locations, nameField, barcodeField, value, limits);

    List<int> ids = [];
    foreach (var node in nodes)
    {
      ids.AddRange(node.GetAllIds());
    }

    if (ids.Count > limits.Limit)
    {
      ids = ids[..limits.Limit];
    }

    return await ReadRecords(locations, ids);
  }

  protected async Task<List<Record>> ReadRecords(DatabaseData database, IEnumerable<int> ids)
  {
    var result = new List<Record>();
    foreach (var id in ids)
    {
      var record = await ReadRecordAsync(database, id, null, null, default);
      if (record != null)
      {
        result.Add(record);
      }
    }
    return result;
  }
}

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Record NewRecord(string folder, string database, string? dataset) => new(this, folder, database, dataset);
}

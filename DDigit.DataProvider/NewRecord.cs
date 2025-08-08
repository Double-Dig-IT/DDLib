namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Record NewRecord(string folder, string database, string? dataset = null) => new(this, folder, database, dataset);
}

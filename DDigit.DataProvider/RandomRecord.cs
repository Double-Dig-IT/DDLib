using System.Text;

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<ResultSet?> RandomSample(string folder, string? database, string[]? datasets, ResultSet? results, int sample, int? seed, bool unique)
  {
    if (results != null)
    {
      return results.Randomize(sample, seed, unique);
    }
    if (database != null)
    {
      var statement = new StringBuilder($"all random {sample}");
      if (seed != null)
      {
        statement.Append($" seed {seed}");
      }
      if (unique)
      {
        statement.Append(" unique");
      }

      return await Search(folder, database, datasets, statement.ToString(), results);
    }

    throw new DDException("No input provided, must be database or a result set.");
  }
}

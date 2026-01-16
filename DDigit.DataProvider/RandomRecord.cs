using System.IO;
using System.Text;

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<ResultSet?> RandomSample(string folder, string databaseName, IEnumerable<string>? datasets,
                                             ResultSet? results, int sampleSize, int? seed, bool unique,
                                             CancellationToken cancellationToken)
  {
    var databaseData = GetDatabase(folder, databaseName) ?? throw new DatabaseNotFoundException(folder, databaseName);

    var searchTree = new SearchTree()
    {
      Database = databaseData,
      SampleSize = sampleSize,
      Unique = unique,
      PreviousResults = results,
      DatasetFilter = datasets != null ? new DatasetFilter(databaseData, datasets) : null,
      SqlState = new SqlStateInfo { Connection = null, Transaction = null, CancellationToken = cancellationToken },
    };
   
    if (results is not null)
    {
      return results.Randomize(searchTree);
    }

    if (databaseData is not null)
    {
      var statement = new StringBuilder($"all random {sampleSize}");
      if (seed is not null)
      {
        statement.Append($" seed {seed}");
      }
      if (unique)
      {
        statement.Append(" unique");
      }

      return await SearchAsync(folder, databaseName, datasets, statement.ToString(), results, 1000, cancellationToken);
    }

    throw new DDException("No input provided, must be database or a result set.");
  }
}

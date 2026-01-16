namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  readonly DDSearchParser parser = new();

  public SearchTree? ParseSearchStatement(string folder, string database, string statement)
  {
    var databaseData = MetaDataCache.ReadDatabase(folder, database, false) ??
      throw new DatabaseNotFoundException(folder, database);
    return parser.Parse(databaseData, statement, default);
  }


  /// <summary>
  /// Run a search statement
  /// </summary>
  /// <param name="folder">The folder in which the database metadata is stored</param>
  /// <param name="database">The database to query</param>
  /// <param name="statement">The statement to executer</param>
  /// <param name="previousResults">Previous results (from a PowerShell pipeline)</param>
  /// <returns>search result</returns>
  public async Task<ResultSet?> SearchAsync(string folder,
                                            string database,
                                            IEnumerable<string>? datasets, string statement,
                                            ResultSet? previousResults = null,
                                            int milestone = 0,
                                            CancellationToken cancellationToken = default)
  {
    var databaseData = MetaDataCache.ReadDatabase(folder, database, false);
    if (databaseData is null)
    {
      return null;
    }
    return await SearchAsync(databaseData, datasets, statement, previousResults, milestone, cancellationToken);
  }

  /// <summary>
  /// Run a search statement
  /// </summary>
  /// <param name="databaseData">The databaseData to search in</param>
  /// <param name="statement">The statement to executer</param>
  /// <param name="previousResults">Previous results (from a PowerShell pipeline)</param>
  /// <returns></returns>
  public async Task<ResultSet?> SearchAsync(DatabaseData databaseData,
                                            IEnumerable<string>? datasets, string statement,
                                            ResultSet? previousResults = null,
                                            int milestone = 0,
                                            CancellationToken cancellationToken = default)
  {
    try
    {
      var datasetFilter = datasets is not null ? new DatasetFilter(databaseData, datasets) : null;
      var searchTree = parser.Parse(databaseData, statement, cancellationToken);
      searchTree.DatasetFilter = datasetFilter;
      searchTree.PreviousResults = previousResults;
      searchTree.SqlState.CancellationToken = cancellationToken;
      searchTree.Milestone = milestone;

      return searchTree is not null ?
        (await SearchWithSortAsync(searchTree, searchTree.Root!)) : null;
    }
    catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
    {
      throw;
    }
    catch (SqlException ex) when (ex.Number == -2 || ex.Number == 0 && cancellationToken.IsCancellationRequested)
    {
      throw new TaskCanceledException();
    }
    catch (Exception ex)
    {
      throw new SearchException(databaseData.Folder!, databaseData.Name!, statement, ex);
    }
  }
}

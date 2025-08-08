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
  /// <returns></returns>
  public async Task<ResultSet?> SearchAsync(string folder,
                                            string database,
                                            IEnumerable<string>? datasets, string statement,
                                            ResultSet? previousResults = null, 
                                            int milestone = 0, 
                                            CancellationToken cancellationToken = default)
  {
    try
    {
      var databaseData = MetaDataCache.ReadDatabase(folder, database, false);
      if (databaseData == null)
      {
        return null;
      }
   
      var datasetFilter = datasets != null ? new DatasetFilter(databaseData, datasets) : null;
      var searchTree = parser.Parse(databaseData, statement, cancellationToken);
      searchTree.DatasetFilter = datasetFilter;
      searchTree.PreviousResults = previousResults;
      searchTree.Cancellation = cancellationToken;
      searchTree.Milestone = milestone;

      return searchTree != null ?
        (await SearchAsync(searchTree, searchTree.Root!)).Randomize(searchTree).Limit(searchTree) : null;
    }
    catch (TaskCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new SearchException(folder, database, statement, ex);
    }
  }


  private async Task<ResultSet> SearchAsync(SearchTree searchTree, SearchNode searchNode)
  {
    // This is a simple search.
    if (searchNode is SearchTreeLeaf leaf)
    {
      return await FindRecordSetAsync(searchTree, leaf);
    }

    // This is a Boolean search with 2 nodes that need to be combined
    if (searchNode is SearchTreeNode searchTreeNode)
    {
      var left = await SearchAsync(searchTree, searchTreeNode.Left);
      var right = await SearchAsync(searchTree, searchTreeNode.Right);
      return JoinRecordSet(left, searchTreeNode.Operator, right);
    }

    // We are dealing with a set here.
    if (searchNode is SearchSetLeaf searchSetLeaf)
    {
      return await Repository.GetResultSetAsync(searchTree, searchSetLeaf.SetId);
    }

    // This should never happen
    throw new DDException($"Unexpected search node: {searchNode}");
  }
}

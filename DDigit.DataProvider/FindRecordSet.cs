namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  /// <summary>
  /// Find record set is the main entry point for all searches.
  /// </summary>
  /// <param name="datasets">A list of datasets to filter</param>
  /// <param name="field">The field (tag or name) to search on</param>
  /// <param name="value">The value to search for</param>
  /// <param name="previousResults">A pipeline result from previous searches</param>
  /// <returns>A result set object</returns>

  public async Task<ResultSet?> FindRecordSet(string path, string table, IEnumerable<string>? datasets,
                                  string field, string? language, string value, ResultSet? previousResults,
                                  CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(field))
    {
      throw new ArgumentNullException(nameof(field));
    }

    var databaseData = MetaDataCache.ReadDatabase(path, table, false);
    if (databaseData == null)
    {
      return null;
    }

    var datasetFilter = datasets != null ? new DatasetFilter(databaseData, datasets) : null;
    var fieldData = databaseData.FindFieldByTagOrName(field);
    if (fieldData == null)
    {
      return null;
    }

    var searchTree = new SearchTree()
    {
      Database = databaseData,
      Root = new SearchTreeLeaf(fieldData, value, SearchOperatorEnum.Equals, language, null),
      PreviousResults = previousResults,
      DatasetFilter = datasetFilter,
      Milestone = Milestone,
      MilestoneReached = MilestoneReached,
      SqlState = new SqlStateInfo
      {
        Connection = null,
        Transaction = null,
        CancellationToken = cancellationToken
      }
    };

    return await SearchWithSortAsync(searchTree, searchTree.Root);
  }

  internal async Task<ResultSet> FindRecordSetAsync(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    async Task<ResultSet> FindFlatRecordSetAsync()
    {
      using var command = searchTree.SqlState.Connection!.CreateCommand();

      var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
      var result = field.PreferredIndex != null ?
        await Repository.FindFlatIndexedRecordSetAsync(command, searchTree, leaf) :
        await FindNonIndexedRecordSetAsync(command);

      return result;
    }

    async Task<ResultSet> FindNonIndexedRecordSetAsync(IDbCommand command)
    {
      var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
      var databaseData = field.Database ?? throw new NullReferenceException(nameof(field.Database));

      var count = 0;
      var result = new ResultSet(databaseData);
      var allRecords = await Repository.ReadAllRecordsAsync(command, searchTree);
      foreach (var id in allRecords.Ids)
      {
        var record = await ReadRecordAsync(databaseData, id, searchTree.SqlState);
        if (record != null)
          if (searchTree.Milestone > 0)
          {
        {
          if (record.Match(field, leaf.Values))
          {
            result.Ids.Add(id);
          }
            count++;
            if (count % searchTree.Milestone == 0)
            {
              searchTree.MilestoneReached?.Invoke(null, new MilestoneEventArgs(count, allRecords.Ids.Count));
            }
          }
        }
      }
      return result;
    }

    searchTree.SqlState.Connection = await Repository.GetDbConnectionAsync(searchTree.Database!);

    await Repository.PreparePreviousResultTable(searchTree);

    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var result = field.IsLinked ? await Repository.FindLinkedRecordSetAsync(searchTree, leaf) : await FindFlatRecordSetAsync();

    await Repository.DropPreviousResultTable(searchTree);
    searchTree.SqlState.Connection.Dispose();
    searchTree.SqlState.Connection = null;

    return result;
  }

  private Task<ResultSet> FindRecordSet(SearchTree searchTree, int setId) => Repository.GetResultSetAsync(searchTree, setId);
}

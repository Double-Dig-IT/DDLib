namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  /// <summary>
  /// Find record set is the main entry point for all searched.
  /// </summary>
  /// <param name="datasets">A list of datasets to filter</param>
  /// <param name="field">The field (tag or name) to search on</param>
  /// <param name="value">The value to search for</param>
  /// <param name="previousResults">A pipeline result from previous searches</param>
  /// <returns>A result set object</returns>

  public async Task<ResultSet?> FindRecordSet(string path, string table, string[]? datasets,
                                  string field, string? language, string? value, ResultSet? previousResults)
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
    return await FindRecordSet(databaseData, fieldData, language, SearchOperators.Equals, value, datasetFilter, previousResults, Milestone, MilestoneReached);
  }

  public async Task<ResultSet> FindRecordSet(DatabaseData databaseData, FieldData fieldData, string? language, SearchOperators searchOperator, string? value,
                                DatasetFilter? datasetFilter, ResultSet? previousResults, int milestone,
                                EventHandler<MilestoneEventArgs>? milestoneReached)
  {
    using var connection = Repository.GetDbConnection(databaseData);
    await Repository.PreparePreviousResultTable(connection, previousResults);

    var result = fieldData.IsLinked ?
      await Repository.FindLinkedRecordSet(connection, fieldData, value, datasetFilter, previousResults) :
      await FindFlatRecordSet(connection, fieldData, language, searchOperator, value, datasetFilter, previousResults, milestone, milestoneReached);

    await Repository.DropPreviousResultTable(connection, previousResults);
    return result;
  }

  private async Task<ResultSet> FindFlatRecordSet(IDbConnection connection, FieldData fieldData, string? language,
                                     SearchOperators searchOperator,
                                     string? value, DatasetFilter? filter,
                                     ResultSet? previousResults, int milestone,
                                     EventHandler<MilestoneEventArgs>? milestoneReached)
  {
    var databaseData = fieldData.Database ?? throw new NullReferenceException(nameof(fieldData.Database));

    var index = fieldData.PreferredIndex;
    using var command = connection.CreateCommand();
    var transaction = connection.BeginTransaction();
    command.Transaction = transaction;
    var result = index != null ?
      await Repository.FindIndexedRecordSet(command, databaseData, fieldData, index.Type, index.TableName, language, searchOperator, value,
      filter, previousResults) :
      await FindNonIndexedRecordSet(command, fieldData, value, filter, previousResults, milestone, milestoneReached);

    transaction.Commit();
    return result;
  }

  private async Task<ResultSet> FindNonIndexedRecordSet(IDbCommand command, FieldData fieldData, string? value,
                                   DatasetFilter? filter, ResultSet? previousResults,
                                   int milestone, EventHandler<MilestoneEventArgs>? milestoneReached)
  {
    var databaseData = fieldData.Database ?? throw new NullReferenceException(nameof(fieldData.Database));
    var databaseName = databaseData.Name ?? throw new NullReferenceException(nameof(fieldData.Database.Name));

    var count = 0;
    var result = new ResultSet(databaseData);
    var allRecords = await Repository.ReadAllRecords(command, databaseData, filter, previousResults);
    foreach (var id in allRecords.Ids)
    {
      var record = await GetRecord(databaseData, databaseName, id);
      if (record != null)
      {
        if (record.Match(fieldData, value))
        {
          result.Ids.Add(id);
        }
        count++;
        if (count % milestone == 0)
        {
          milestoneReached?.Invoke(null, new MilestoneEventArgs(count, allRecords.Ids.Count));
        }
      }
    }
    return result;
  }

  private async Task<ResultSet> FindRecordSet(DatabaseData databaseData, int setId, DatasetFilter? datasetFilter, ResultSet? previousResults)
    => await Repository.GetResultSet(databaseData, setId, datasetFilter, previousResults);

}

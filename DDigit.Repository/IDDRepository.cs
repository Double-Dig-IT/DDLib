namespace DDigit.Repository;

public interface IDDRepository
{
  Task<int> FindLink(string tableName, DatasetData? dataset, string? tag,
                     string? domain, string value, bool isMultiLingual, string language, SqlStateInfo sqlState);

  Task<RecordSetList> GetRecordSetMetaDataPerDatabaseAsync(DatabaseData database, string? searchTerm, int startFrom = 1, int limit = 0, RecordSetSortEnum? sort = null, SearchSortOrderEnum? sortOrder = null,
                                                        CancellationToken cancellationToken = default);

  Task<IEnumerable<RecordLock>> GetRecordLock(DatabaseData databaseData, CancellationToken cancellationToken);

  Task<int> AddWord(string text, string language, SqlStateInfo sqlState);

  /// <summary>
  /// Retrieve a result set, (aka pointer file)
  /// </summary>
  /// <param name="databaseData"The meta data for the database></param>
  /// <param name="set">Set number</param>
  /// <param name="filter">Filter on datasets</param>
  /// <param name="previousResults">Any previous results from the PowerShell pipeline</param>
  /// <returns>The new ResultSet</returns>
  Task<ResultSet> GetResultSetAsync(SearchTree searchTree, int set);

  Task<int> GetWordNumberAsync(string text, string language, SqlStateInfo sqlState);

  Task<object> ReadDataAsync(DatabaseData database, int id, SqlStateInfo sqlState);

  Task PreparePreviousResultTable(SearchTree searchTree);

  Task DropPreviousResultTable(SearchTree searchTree);

  Task<ResultSet> FindLinkedRecordSetAsync(SearchTree searchTree, SearchTreeLeaf leaf);

  Task<ResultSet> FindFlatIndexedRecordSetAsync(IDbCommand command, SearchTree searchTree, SearchTreeLeaf leaf);

  Task<ResultSet> ReadAllRecordsAsync(IDbCommand command, SearchTree searchTree);

  Task<AutoCompleteResult?> GetAutoCompleteAsync(IEnumerable<FieldData> fieldData, DatasetFilter? datasetFilter, string? value,
                                            int? starFrom, int? limit, string? language, bool count, CancellationToken cancellationToken);

  Task<int> GetNewRecordIdAsync(DatabaseData database, DatasetData? dataset, SqlStateInfo sqlState);

  Task WriteNewDataAsync(string table, int id,
                         DateTime creation, DateTime modification, string data, SqlStateInfo sqlState);

  Task UpdateDataAsync(string table, int id, DateTime modification, string data, SqlStateInfo sqlState);

  Task<IDbTransaction> StartTransactionAsync(IDbConnection connection);

  Task RollbackAsync(SqlStateInfo sqlState);

  Task CommitAsync(SqlStateInfo sqlState);

  Task<IDbConnection> GetDbConnectionAsync(DatabaseData database);

  Task<int> AddIndexKeyAsync(IntegerIndexRow row, SqlStateInfo sqlState);

  Task<int> AddIndexKeyAsync(string? fullTextTable, TermIndexRow row, SqlStateInfo sqlState);

  Task<int> AddIndexKeyAsync(DateIndexRow dateRow, SqlStateInfo sqlState);

  Task<int> AddIndexKeyAsync(IsoDateIndexRow dateRow, SqlStateInfo sqlState);

  Task<int> AddIndexKeyAsync(BooleanIndexRow booleanRow, SqlStateInfo sqlState);

  Task<int> AddIndexKeyAsync(string? fullTextTable, AlphaNumericIndexRow alphaNumericRow, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeyAsync(IntegerIndexRow row, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeyAsync(string? fullTextTable, TermIndexRow row, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeysAsync(string tableName, int id, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeyAsync(DateIndexRow dateRow, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeyAsync(IsoDateIndexRow dateRow, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeyAsync(BooleanIndexRow booleanRow, SqlStateInfo sqlState);

  Task<int> DeleteIndexKeyAsync(string? fullTextTable, AlphaNumericIndexRow alphaNumericRow, SqlStateInfo sqlState);

  Task<List<int>> ReadLinkedRecordIdsAsync(string table, int id, SqlStateInfo sqlState);

  Task<int> WriteRecordSetAsync(string folder, RecordSetMetaData metaData, ResultSet set, CancellationToken cancellationToken);

  Task DeleteRecordSetAsync(string folder, string database, int setNo, CancellationToken cancellationToken);
  Task<IEnumerable<int>> SearchLinksAsync(DatabaseData database, DatasetData? dataset, FieldData field, string searchValue, string domain, SearchLimits limits, SqlStateInfo sqlState);
  Task<string> GetAutoNumberValue(FieldData fieldData, SqlStateInfo sqlState);
  Task<RecordSetMetaData?> GetResultSetMetaDataAsync(DatabaseData database, int set, CancellationToken cancellationToken);
  Task AddToRecordSetAsync(RecordSet resultSet, int id, SqlStateInfo sqlState);
  Task RemoveFromRecordSetAsync(RecordSet recordSet, int id, SqlStateInfo sqlState);
  Task<ResultSet> RunSqlSearch(DatabaseData databaseData, string sql, Dictionary<string, object> parameters, SqlStateInfo sqlState);
  Task<int> WriteRecordSetMetaDataAsync(RecordSetMetaData metaData, SqlStateInfo sqlState);
  bool CheckIndexTable(DatabaseData databaseData, string tableName);
}

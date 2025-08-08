namespace DDigit.Repository;

public interface IDDRepository
{
  Task<int> FindLink(string tableName, DatasetData? dataset,
                     string? domain, string value, string language,
                     IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken);

  Task<RecordSetList> GetRecordSetMetaDataPerDatabaseAsync(DatabaseData database, string? searchTerm, int startFrom = 1, int limit = 0, 
                                                        CancellationToken cancellationToken = default);

  Task<IEnumerable<RecordLock>> GetRecordLock(DatabaseData databaseData, CancellationToken cancellationToken);

  Task<int> AddWord(IDbConnection connection, IDbTransaction transaction, string text, string language, CancellationToken cancellationToken);

  /// <summary>
  /// Retrieve a result set, (aka pointer file)
  /// </summary>
  /// <param name="databaseData"The meta data for the database></param>
  /// <param name="set">Set number</param>
  /// <param name="filter">Filter on datasets</param>
  /// <param name="previousResults">Any previous results from the PowerShell pipeline</param>
  /// <returns>The new ResultSet</returns>
  Task<ResultSet> GetResultSetAsync(SearchTree searchTree, int set);

  Task<int> GetWordNumber(IDbConnection connection, IDbTransaction transaction, string text, string language);

  Task<object> ReadDataAsync(DatabaseData database, int id, IDbConnection? connection, 
    IDbTransaction? transaction, CancellationToken cancellationToken);

  Task PreparePreviousResultTable(SearchTree searchTree);

  Task DropPreviousResultTable(SearchTree searchTree);

  Task<ResultSet> FindLinkedRecordSetAsync(SearchTree searchTree, SearchTreeLeaf leaf);

  Task<ResultSet> FindFlatIndexedRecordSetAsync(IDbCommand command, SearchTree searchTree, SearchTreeLeaf leaf);

  Task<ResultSet> ReadAllRecordsAsync(IDbCommand command, SearchTree searchTree);

  Task<AutoCompleteResult?> GetAutoCompleteAsync(IEnumerable<FieldData> fieldData, DatasetFilter? datasetFilter, string? value,
                                            int? starFrom, int? limit, string? language, bool count, CancellationToken cancellationToken);

  Task<int> GetNewRecordIdAsync(IDbConnection connection, IDbTransaction transaction, DatabaseData database, DatasetData? dataset);

  Task WriteNewDataAsync(string table, int id,
                         DateTime creation, DateTime modification, string data,
                         IDbConnection connection,
                         IDbTransaction transaction,
                         CancellationToken cancellationToken);
     
  Task UpdateDataAsync(IDbConnection connection, IDbTransaction transaction, string table, int id,
                  DateTime modification, string data, CancellationToken cancellationToken);

  Task<IDbTransaction> StartTransactionAsync(IDbConnection connection);

  Task RollbackAsync(IDbTransaction transaction, CancellationToken cancellationToken);

  Task CommitAsync(IDbTransaction transaction, CancellationToken cancellationToken);

  Task<IDbConnection> GetDbConnectionAsync(DatabaseData database);

  Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IntegerIndexRow row, CancellationToken cancellationToken);

  Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IntegerIndexRow row, CancellationToken cancellationToken);

  Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, string? fullTextTable, TermIndexRow row, CancellationToken cancellationToken);

  Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, string? fullTextTable, TermIndexRow row, CancellationToken cancellationToken);

  Task<int> DeleteIndexKeysAsync(IDbConnection connection, IDbTransaction transaction, string tableName, int id, CancellationToken cancellationToken);

  Task<List<int>> ReadLinkedRecordIdsAsync(IDbConnection connection, IDbTransaction transaction, string table, int id, CancellationToken cancellationToken);

  Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, DateIndexRow dateRow, CancellationToken cancellationToken);

  Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, DateIndexRow dateRow, CancellationToken cancellationToken);

  Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IsoDateIndexRow dateRow, CancellationToken cancellationToken);

  Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IsoDateIndexRow dateRow, CancellationToken cancellationToken);

  Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, BooleanIndexRow booleanRow, CancellationToken cancellationToken);

  Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, BooleanIndexRow booleanRow, CancellationToken cancellationToken);

  Task<int> AddIndexKey(IDbConnection connection, IDbTransaction transaction, string? fullTextTable, AlphaNumericIndexRow alphaNumericRow, CancellationToken cancellationToken);

  Task<int> DeleteIndexKey(IDbConnection connection, IDbTransaction transaction, string? fullTextTable, AlphaNumericIndexRow alphaNumericRow, CancellationToken cancellationToken);

  Task<int> WriteRecordSetAsync(string folder, RecordSetMetaData metaData, ResultSet set, CancellationToken cancellationToken);

  Task DeleteRecordSetAsync(string folder, string database, int setNo, CancellationToken cancellationToken);
  Task<List<HierarchyNode>> SearchLocationsAsync(DatabaseData locations, string nameField, string barcodeField, string value, SearchLimits limits);
  Task<IEnumerable<int>> SearchLinksAsync(DatabaseData database, DatasetData dataset, FieldData field, string searchValue, string domain, SearchLimits limits);
  Task<string> GetAutoNumberValue(IDbConnection connection, IDbTransaction transaction, FieldData fieldData, CancellationToken cancellationToken);
}

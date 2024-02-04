
namespace DDigit.Repository;

public interface IDDRepository
{
  Task<int> FindLink(string tableName, string? domain, string v, string language);

  Task<List<RecordSetMetaData>> GetRecordSetPerDatabase(DatabaseData database);

  Task<IEnumerable<RecordLock>> GetRecordLock(DatabaseData databaseData);

  Task<int> AddWord(string text, string language);

  /// <summary>
  /// Retrieve a result set, (aka pointer file)
  /// </summary>
  /// <param name="databaseData"The meta data for the database></param>
  /// <param name="set">Set number</param>
  /// <param name="filter">Filter on datasets</param>
  /// <param name="previousResults">Any previous results from the PowerShell pipeline</param>
  /// <returns>The new ResultSet</returns>
  Task<ResultSet> GetResultSet(DatabaseData databaseData, int set, DatasetFilter? filter, ResultSet? previousResults);

  Task<int> GetWordNumber(string text, string language);

  Task<object?> ReadData(DatabaseData database, string table, int id);

  Task PreparePreviousResultTable(IDbConnection connection, ResultSet? previousResults);

  Task DropPreviousResultTable(IDbConnection connection, ResultSet? previousResults);

  Task<ResultSet> FindLinkedRecordSet(IDbConnection connection, FieldData fieldData, string? value, DatasetFilter? filter, ResultSet? previousResults);

  Task<ResultSet> FindIndexedRecordSet(IDbCommand command, DatabaseData databaseData, FieldData fieldData, IndexTypeEnum type, string tableName,
                                       string? language, SearchOperators searchOperator, string? value, DatasetFilter? filter,
                                       ResultSet? previousResults);

  Task<ResultSet> ReadAllRecords(IDbCommand command, DatabaseData databaseData, DatasetFilter? filter, ResultSet? previousResults);

  Task<AutoCompleteResult?> GetAutoComplete(IEnumerable<FieldData> fieldData, DatasetFilter? datasetFilter, string? value, int? starFrom, int? limit, string? language, bool count);

  Task<int> GetNewRecordId(DatabaseData database, DatasetData? dataset);

  Task WriteNewData(string table, int id, DateTime creation, DateTime modification, string data);

  Task UpdateData(string table, int id, DateTime modification, string data);

  void StartTransaction(DatabaseData databaseData);

  void Rollback();

  void Commit();

  IDbConnection GetDbConnection(DatabaseData database);

  Task AddIndexKey(string table, int key, int id);

  Task RemoveIndexKey(string table, int key, int id);

  Task RemoveIndexKey(string table, string term, string displayTerm, string language, string? domain, int id);

  Task AddIndexKey(string table, string key, string displayTerm, string language, string? domain, int id);
}

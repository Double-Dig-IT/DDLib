using DDigit.Search;

namespace DDigit.Data;

public interface IDataProvider
{
  IDDRepository Repository { get; }

  event EventHandler<MilestoneEventArgs>? MilestoneReached;

  Task<Record?> ReadRecordAsync(DatabaseData database, int id,
                                IDbConnection? connection, 
                                IDbTransaction? transaction,
                                CancellationToken cancellationToken);

  Task WriteRecordAsync(Record record, IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken);

  ResultSet JoinRecordSet(ResultSet left, BooleanOperator @operator, ResultSet right);

  Task<Record?> ReadRecordAsync(string folder, string database, int id, CancellationToken cancellationToken);

  Task<ResultSet?> FindRecordSet(string folder, string database, IEnumerable<string>? dataset, string fieldOrTag, string? language,
                                 string value, ResultSet? results, CancellationToken cancellationToken);

  IEnumerable<FieldData> GetTaskField(string folder);

  IEnumerable<SqlSetting> GetSqlServer(string folder);

  Task<ResultSet> GetRecordSetAsync(string folder, string database, int set, CancellationToken cancellationToken);

  Task<IEnumerable<RecordLock>> GetRecordLock(string Folder);

  Task<RecordSetList> GetRecordSetMetaDataAsync(string folder, string? databaseName, HashSet<int>? sets, 
                                                     string? searchTerm, int startFrom, int limit, CancellationToken cancellationToken);

  void SetUser(string workingDirectory, string user, string? role, string? password);

  IEnumerable<UserData> GetUser(string folder, string? user, string? role);

  IEnumerable<TaskData> GetTask(string folder);

  IEnumerable<MergedFieldData> GetMergedField(string folder);

  IEnumerable<ImageData> GetImagePath(string folder, string? database);

  IEnumerable<FieldData> GetField(string folder, string? database, string? tag, FieldTypeEnum? type, bool? isLinked, string? field, string? group);

  IEnumerable<DatasetData> GetDataset(string folder, string? database);

  IEnumerable<DatabaseData> GetDatabase(string folder);

  DatabaseData? GetDatabase(string? folder, string? database);

  IEnumerable<DataSourceData> GetDatasource(string folder);

  ApplicationData GetApplication(string folder);

  IEnumerable<string> GetCacheEntries();

  Task<ResultSet?> SearchAsync(string folder, string database, IEnumerable<string>? datasets, string statement,
                          ResultSet? results, int milestone, CancellationToken cancellationToken);

  Task<AutoCompleteResult?> GetAutoComplete(string folder, string database, IEnumerable<string>? datasets,
                                            string[] fields, string? value, int? startFrom, int? limit, string? language, bool count,
                                            CancellationToken cancellationToken);

  Task<ResultSet?> RandomSample(string folder, string database, IEnumerable<string>? datasets, ResultSet? results, int sample,
                                int? seed, bool unique, CancellationToken cancellationToken);

  Record NewRecord(string folder, string database, string? dataset);

  IEnumerable<FormData> GetForm(string folder, string? fileName = "*");

  Task RemoveRecord(string folder, string database, int id, CancellationToken cancellationToken);

  Task DeleteRecordAsync(Record record, CancellationToken cancellationToken);
  Task <string> GetAutoNumberValue(IDbConnection connection, IDbTransaction transaction,
                            FieldData fieldData, CancellationToken cancellationToken);
}
namespace DDigit.Data;

public interface IDataProvider
{
  IDDRepository Repository { get; }

  event EventHandler<MilestoneEventArgs>? MilestoneReached;

  Task<Record?> GetRecord(DatabaseData linkDatabase, string table, int id);

  Task WriteRecordAsync(Record record);

  ResultSet JoinRecordSet(ResultSet left, BooleanOperator @operator, ResultSet right);

  Task<Record?> ReadRecord(string folder, string database, int id);

  Task<ResultSet?> FindRecordSet(string folder, string database, string[]? dataset, string fieldOrTag, string? language,
                                 string? value, ResultSet? results);

  IEnumerable<FieldData> GetTaskField(string folder);

  IEnumerable<SqlSetting> GetSqlServer(string folder);

  Task<ResultSet> GetRecordSet(string folder, string database, int set);

  Task<IEnumerable<RecordLock>> GetRecordLock(string Folder);

  Task<List<RecordSetMetaData>> GetRecordSetMetaData(string folder, string? databaseName, HashSet<int>? sets);

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

  Task<ResultSet?> Search(string folder, string database, string[]? datasets, string statement, ResultSet? results = null, int milestone = 1000);

  Task<AutoCompleteResult?> GetAutoComplete(string folder, string database, string[]? datasets, string[] fields, string? value, int? startFrom, int? limit, string? language, bool count);

  Task<ResultSet?> RandomSample(string folder, string? database, string[]? datasets, ResultSet? results, int sample, int? seed, bool unique);

  Record NewRecord(string folder, string database, string? dataset);

  IEnumerable<FormData> GetForm(string workingDirectory, string? fileName = "*");
}
namespace DDigit.Repository;

public class MSSqlRepository : IDDRepository
{
  private const int MaxWordLength = 32;

  public async Task<List<RecordSetMetaData>> GetRecordSetPerDatabase(DatabaseData database)
  {
    var result = new List<RecordSetMetaData>();
    var table = database.Name!;
    using var connection = GetDbConnection(database);
    {
      using var command = connection.CreateCommand() as SqlCommand;
      command!.CommandText = SqlBuilder.GetPointerFileData(table);
      using var reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        result.Add(new()
        {
          Number = (int)reader["pfNumber"],
          Database = table,
          Title = ReadString(reader, "title"),
          Owner = ReadString(reader, "owner"),
          Selection = ReadString(reader, "selectionStatement"),
          Hits = (int)reader["hitCount"],
          Created = (DateTime)reader["creation"],
          Modified = (DateTime)reader["modification"]
        });
      }
    }
    return result;
  }

  private static string? ReadString(IDataReader reader, string property)
  {
    var value = reader[property];
    return value != DBNull.Value ? value.ToString() : null;
  }

  public IDbConnection GetDbConnection(DatabaseData database)
  {
    var connection = new SqlConnection(GetConnectionString(database));
    connection.Open();
    return connection;
  }

  private static string GetConnectionString(DatabaseData database)
  {
    var connectionString = new SqlConnectionStringBuilder
    {
      DataSource = database.SqlServer,
      InitialCatalog = database.DSN,
      Encrypt = false
    };

    if (!string.IsNullOrWhiteSpace(database.SqlUserId) &&
        !string.IsNullOrWhiteSpace(database.SqlPassword))
    {
      connectionString.UserID = database.SqlUserId;
      connectionString.Password = database.SqlPassword;
      connectionString.IntegratedSecurity = false;
    }
    else
    {
      connectionString.IntegratedSecurity = true;
    }
    return connectionString.ToString();
  }

  private void CheckTransaction()
  {
    CheckConnection();
    if (transaction == null)
    {
      throw new NullReferenceException(nameof(transaction));
    }
  }

  private void CheckConnection()
  {
    if (connection == null)
    {
      throw new NullReferenceException(nameof(connection));
    }
  }

  public async Task<int> AddWord(string word, string language)
  {
    if (word.Length > MaxWordLength)
    {
      word = word[..MaxWordLength];
    }

    CheckTransaction();
    using var command = connection!.CreateCommand() as SqlCommand;
    if (command == null)
    {
      throw new NullReferenceException(nameof(command));
    }
    command.Transaction = transaction;
    command.CommandText = SqlBuilder.SelectHighestWordNumber;
    var wordNumber = (int?)await command.ExecuteScalarAsync()! + 1;
    command.CommandText = SqlBuilder.AddWord;
    command.Parameters.Add(new SqlParameter("@term", word));
    command.Parameters.Add(new SqlParameter("@displayTerm", word));
    command.Parameters.Add(new SqlParameter("@language", language));
    command.Parameters.Add(new SqlParameter("@wordNumber", wordNumber));
    command.ExecuteNonQuery();
    return wordNumber != null ? wordNumber.Value : 0;
  }

  public async Task<int> GetWordNumber(string word, string language)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = SqlBuilder.GetWordNumber;
    command.Parameters.Add(new SqlParameter("@term", word));
    command.Parameters.Add(new SqlParameter("@language", language));
    var result = await command.ExecuteScalarAsync();
    return result != null && result != DBNull.Value ? (int)result : 0;
  }

  public async Task<int> FindLink(string table, string? domain, string key, string language)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = SqlBuilder.FindLink(table);
    command.Parameters.Add(new SqlParameter("@term", key));
    command.Parameters.Add(new SqlParameter("@language", language));
    command.Parameters.Add(new SqlParameter("@domain", domain));
    var result = await command.ExecuteScalarAsync();
    return result != null && result != DBNull.Value ? (int)result : 0;
  }

  public async Task<object?> ReadData(DatabaseData database, string table, int id)
  {
    object? result = null;
    using var connection = GetDbConnection(database) as SqlConnection;
    if (connection != null)
    {
      using var command = connection.CreateCommand();
      command.CommandText = SqlBuilder.SelectData(table);
      command.Parameters.Add(new SqlParameter("@priref", id));
      result = await command.ExecuteScalarAsync();
    }
    return result;
  }

  public async Task<IEnumerable<RecordLock>> GetRecordLock(DatabaseData databaseData)
  {
    var result = new List<RecordLock>();
    using var connection = GetDbConnection(databaseData) as SqlConnection;
    if (connection != null)
    {
      using var command = connection.CreateCommand();
      command.CommandText = SqlBuilder.SelectAllRecordLocks;
      using var reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        result.Add(new RecordLock
        {
          Database = ReadString(reader, "database_name"),
          Id = (int)reader["priref"],
          User = ReadString(reader, "id"),
          LockTime = (DateTime)reader["lockTime"],
          Info = ReadString(reader, "info")
        });
      }
    }
    return result;
  }

  public async Task<ResultSet> GetResultSet(DatabaseData databaseData, int set,
                   DatasetFilter? filter, ResultSet? previousResults)
  {
    using var connection = GetDbConnection(databaseData) as SqlConnection;
    if (connection == null)
    {
      throw new NullReferenceException(nameof(connection));
    }

    using var command = connection.CreateCommand();
    command.CommandText = SqlBuilder.SelectFromHitList(databaseData.Name!);
    command.Parameters.Add(new SqlParameter("@set", set));
    return await ReadPrirefs(databaseData, command, filter, previousResults);
  }

  public async Task PreparePreviousResultTable(IDbConnection connection, ResultSet? previousResults)
  {
    if (previousResults != null)
    {
      if (connection is not SqlConnection sqlConnection)
      {
        throw new NullReferenceException(nameof(sqlConnection));
      }

      using var command = sqlConnection.CreateCommand();

      command.CommandText = SqlBuilder.CreatePreviousResultsTable;
      command.ExecuteNonQuery();

      command.CommandText = SqlBuilder.CreatePreviousResultsIndex;
      await command.ExecuteNonQueryAsync();

      command.CommandText = SqlBuilder.AddToPreviousResults;
      var parameter = new SqlParameter("@id", SqlDbType.Int);
      command.Parameters.Add(parameter);
      foreach (var id in previousResults.Ids)
      {
        parameter.Value = id;
        command.ExecuteNonQuery();
      }
    }
  }

  public async Task DropPreviousResultTable(IDbConnection connection, ResultSet? previousResults)
  {
    if (previousResults != null)
    {
      if (connection is not SqlConnection sqlConnection)
      {
        throw new NullReferenceException(nameof(sqlConnection));
      }
      using var command = sqlConnection.CreateCommand();
      command.CommandText = SqlBuilder.DropPreviousResultsTable;
      await command.ExecuteNonQueryAsync();
    }
  }

  public async Task<ResultSet> FindLinkedRecordSet(IDbConnection connection, FieldData fieldData,
                                       string? value, DatasetFilter? filter, ResultSet? previousResults)
  {
    var databaseData = fieldData.Database ?? throw new NullReferenceException(nameof(fieldData.Database));

    if (connection is not SqlConnection sqlConnection)
    {
      throw new NullReferenceException(nameof(sqlConnection));
    }

    using var command = sqlConnection.CreateCommand();
    command.CommandText = SqlBuilder.LinkedFieldSearch(fieldData, previousResults);
    if (!fieldData.LinkedDatabase!.FullText)
    {
      command.Parameters.Add(new SqlParameter("@term0", SqlBuilder.ReplaceStar(value)));
    }
    else
    {
      command.Parameters.Add(new SqlParameter("@term0", FullTextValue(value)));
    }
    return await ReadPrirefs(databaseData, command, filter, previousResults);
  }

  private async static Task<ResultSet> ReadPrirefs(DatabaseData databaseData, SqlCommand sqlCommand, DatasetFilter? filter,
                                  ResultSet? previousResults)
  {
    var result = new ResultSet(databaseData);
    using var reader = await sqlCommand.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
      result.Add(await reader.GetFieldValueAsync<int>(0), filter, previousResults);
    }
    return result;
  }

  private static readonly char[] separators = [' ', ',', '\'', '\"'];
  private string[] GetFreeTextSearchWords(string? value)
  {
    var result = value != null ?
     value.Split(separators, StringSplitOptions.RemoveEmptyEntries) : [];
    return result.ToList().Select((item) => item.Length > MaxWordLength ? item[..MaxWordLength] : item).ToArray();
  }


  public async Task<ResultSet> FindIndexedRecordSet(IDbCommand command, DatabaseData databaseData, FieldData fieldData, 
                                                    IndexTypeEnum indexType, 
                                                    string indexTable, string? language, SearchOperators searchOperator,
                                                    string? value, DatasetFilter? filter, ResultSet? previousResults)
  {
    if (command is not SqlCommand sqlCommand)
    {
      throw new NullReferenceException(nameof(sqlCommand));
    }

    string[] searchWords = [];
    if (indexTable.Contains('_'))
    {
      switch (indexType)
      {
        case IndexTypeEnum.FreeText:
          if (databaseData.FullText)
          {
            searchWords = [FullTextValue(value)];
            command.CommandText = SqlBuilder.FullTextSearch(databaseData, fieldData.Tag!, previousResults);
          }
          else
          {
            searchWords = GetFreeTextSearchWords(value);
            command.CommandText = SqlBuilder.FreeTextSearch(indexTable, searchWords, previousResults);
          }
          break;

        default:
          if (fieldData.IsEnumeration.HasValue && fieldData.IsEnumeration.Value)
          {
            searchWords = fieldData.EnumKeys(value, language).ToArray();
            if (searchWords.Length == 0)  // return empty result set when no search terms were matched
            {
              return new ResultSet(databaseData);
            }
            if (databaseData.FullText)
            {
              command.CommandText = SqlBuilder.EnumSearchFullText(databaseData.FullTextTable, fieldData.Tag!, searchWords, previousResults);
;            }
            else
            {
              command.CommandText = SqlBuilder.EnumSearch(indexTable, searchWords, previousResults);
            }
          }
          else
          {
            if (databaseData.FullText)
            {
              searchWords = [FullTextValue(value)];
              command.CommandText = SqlBuilder.FullTextSearch(databaseData, fieldData.Tag, previousResults);
            }
            else
            {
              if (value != null)
              {
                searchWords = [value];
              }
              command.CommandText = SqlBuilder.Search(indexTable, searchOperator, ref searchWords[0]!, previousResults);
            }
          }
          break;
      }
    }
    else
    {
      command.CommandText = SqlBuilder.IdSearch(indexTable, searchOperator);
      searchWords = [value!];
    }

    for (int i = 0; i < searchWords.Length; i++)
    {
      command.Parameters.Add(new SqlParameter($"@term{i}", searchWords[i]));
    }

    return await ReadPrirefs(databaseData, sqlCommand, filter, previousResults);
  }

  public async Task<ResultSet> ReadAllRecords(IDbCommand command, DatabaseData databaseData, DatasetFilter? filter, ResultSet? previousResults)
  {
    if (command is not SqlCommand sqlCommand)
    {
      throw new NullReferenceException(nameof(sqlCommand));
    }
    command.CommandText = SqlBuilder.ReadAllRecords(databaseData, previousResults);
    return await ReadPrirefs(databaseData, sqlCommand, filter, previousResults);
  }


  private static string FullTextValue(string? value)
  {
    var searchString = new StringBuilder();
    searchString.Append('\"');
    if (value != null)
    {
      foreach (var ch in value)
      {
        if (ch != '\"')
        {
          searchString.Append(ch);
        }
      }
    }
    searchString.Append('*');
    searchString.Append('\"');

    return searchString.ToString(); ;
  }

  public async Task<AutoCompleteResult?> GetAutoComplete(IEnumerable<FieldData> fields, DatasetFilter? datasetFilter, string? value, int? startFrom, int? limit, string? language, bool count)
  {
    var result = new AutoCompleteResult(startFrom ?? 1, limit ?? 10);

    var firstField = fields.FirstOrDefault() ?? throw new NullReferenceException(nameof(fields));
    var databaseData = firstField.Database!;
    using var connection = GetDbConnection(databaseData) as SqlConnection;

    if (connection == null)
    {
      throw new NullReferenceException(nameof(connection));
    }

    using var command = connection.CreateCommand();
    var sql = new StringBuilder();
    var returnRows = startFrom + limit + 1; // return one more than the requested number of keys
    sql.AppendLine($"select top {returnRows} term, [use] , sum(hits) as hits from (");

    var term = $"{value}%";
    _ = int.TryParse(value, out int number);

    int fieldNumber = 0;
    foreach (var fieldData in fields)
    {
      fieldNumber++;
      if (fieldNumber > 1)
      {
        sql.AppendLine(" union ");
      }

      var index = fieldData.PreferredIndex ?? throw new IndexNotFoundException(fieldData.Name);
      switch (index.Type)
      {
        case IndexTypeEnum.FreeText:
          if (databaseData.FullText)
          {
            if (string.IsNullOrWhiteSpace(value))
            {
              sql.AppendLine(SqlBuilder.FullTextEmptyAutoComplete(databaseData.FullTextTable, fieldData.Tag, datasetFilter, fieldNumber));
            }
            else
            {
              sql.AppendLine(SqlBuilder.FullTextAutoComplete(databaseData.FullTextTable, fieldData.Tag, datasetFilter, fieldNumber));
            }
            command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", FullTextValue(value)));
          }
          else
          {
            sql.AppendLine(SqlBuilder.FreeTextAutoComplete(index.TableName, datasetFilter, fieldNumber));
            command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", term));
          }
          break;

        case IndexTypeEnum.Integer:
          if (fieldData.IsLinked)
          {
            sql.AppendLine(SqlBuilder.LinkedAutocomplete(fieldData, datasetFilter, fieldNumber, value));
            if (fieldData.LinkedDatabase!.FullText)
            {
              command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", FullTextValue(value)));
            }
            else
            { 
              command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", term));
            }
          }
          else
          {
            sql.AppendLine(SqlBuilder.IntegerAutoComplete(index.TableName, datasetFilter, fieldNumber));
            command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", number));
          }
          break;

        default:
          if (fieldData.IsEnumeration.HasValue && fieldData.IsEnumeration.Value)
          {
            AutoCompleteEnum(command, sql, index.TableName, datasetFilter, fieldNumber, fieldData, value, language, count);
          }
          else
          {
            if (databaseData.FullText)
            {
              sql.AppendLine(SqlBuilder.FullTextAutoComplete(databaseData.FullTextTable, fieldData.Tag, datasetFilter, fieldNumber));
              command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", FullTextValue(value)));
            }
            else
            {
              sql.AppendLine(SqlBuilder.FlatAutoComplete(index.TableName, datasetFilter, fieldNumber));
              command.Parameters.Add(new SqlParameter($"@term{fieldNumber}", term));
            }
          }
          break;
      }

      if (!string.IsNullOrWhiteSpace(fieldData.LinkDomain))
      {
        command.Parameters.Add(new SqlParameter($"@domain{fieldNumber}", fieldData.LinkDomain));
      }
    }

    sql.AppendLine(") result");
    sql.AppendLine(" group by term, [use] order by term");

    command.CommandText = sql.ToString();
    using var reader = await command.ExecuteReaderAsync();

    int counter = 0;
    while (await reader.ReadAsync())
    {
      counter++;
      if (startFrom > 0 && counter >= startFrom)
      {
        if (result.Count < limit)
        {
          result.Add(new AutoCompleteObject
          {
            Term = (await reader.GetFieldValueAsync<object>(0)).ToString(),
            Use = await reader.GetFieldValueAsync<string>(1),
            Hits = await reader.GetFieldValueAsync<int>(2)
          });
        }
      }
    }

    // IF we are doing autocomplete on a single enumerated field translate he keys to re requested value.
    if (fields.Count() == 1 && firstField.IsEnumeration.HasValue && firstField.IsEnumeration.Value && language != null)
    {
      foreach (var r in result)
      {
        if (r.Term != null)
        {
          r.Term = firstField.GetLanguageEnumValue(r.Term, language);
        }
      }
      result.Sort();
    }

    result.Hits = counter;
    return result;
  }

  private static void AutoCompleteEnum(SqlCommand command, StringBuilder sql, string tableName,
                                DatasetFilter? datasetFilter, int fieldNumber, FieldData fieldData,
                                string? value, string? language, bool count)
  {
    var keys = fieldData.EnumKeys(value, language);

    sql.AppendLine(SqlBuilder.EnumAutoComplete(tableName, datasetFilter, fieldNumber, keys, count));

    int i = 0;
    foreach (var key in keys)
    {
      if (key != null)
      {
        command.Parameters.Add(new SqlParameter($"@term{fieldNumber}key{i}", key.ToLower()));
        i++;
      }
    }
  }

  public async Task<int> GetNewRecordId(DatabaseData databaseData, DatasetData? datasetData)
  {
    int min = datasetData != null ? datasetData.LowerLimit : 1;
    int max = datasetData != null ? datasetData.UpperLimit : int.MaxValue;
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = $"select max(priref) from {databaseData.Name} where priref between @min and @max";
    command.Parameters.AddRange([new SqlParameter("@min", min), new SqlParameter("@max", max)]);

    var lastPriref = (int)(await command.ExecuteScalarAsync())!;
    if (lastPriref == max)
    {
      var text = datasetData != null ? $"Dataset {datasetData.Name}" : $"Database {databaseData.Name}";
      throw new DDException($"{text} is full");
    }

    return lastPriref + 1;
  }

  public async Task WriteNewData(string table, int id, DateTime creation, DateTime modification, string data)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = $"insert into [{table}] (priref, creation, modification, data)" +
                           "values (@priref, @creation, @modification, @data)";
    command.Parameters.AddRange(
      [
        new SqlParameter("@priref", id),
        new SqlParameter("@creation", creation),
        new SqlParameter("@modification", modification),
        new SqlParameter("@data", data)
      ]);
    await command.ExecuteNonQueryAsync();
  }

  public async Task UpdateData(string table, int id, DateTime modification, string data)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = $"update [{table}] set modification = @modification, data = @data where priref = @priref";
    command.Parameters.AddRange(
      [
        new SqlParameter("@priref", id),
        new SqlParameter("@modification", modification),
        new SqlParameter("@data", data)
      ]);
    await command.ExecuteNonQueryAsync();
  }

  public void StartTransaction(DatabaseData databaseData)
  {
    connection ??= GetDbConnection(databaseData) as SqlConnection;
    if (connection == null)
    {
      throw new NullReferenceException(nameof(connection));
    }
    if (transactionLevel == 0)
    {
      transaction = connection.BeginTransaction();
    }
    transactionLevel++;
  }

  public void Rollback()
  {
    transaction?.Rollback();
    transactionLevel = 0;
    transaction = null;
  }

  public void Commit()
  {
    transactionLevel--;
    if (transactionLevel == 0)
    {
      transaction?.Commit();
      transaction = null;
    }
  }

  public async Task AddIndexKey(string table, int key, int id)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = SqlBuilder.InsertKey(table);
    command.Parameters.AddRange(
      [
        new SqlParameter("@key", key),
        new SqlParameter("@id", id)
      ]);
    await command.ExecuteNonQueryAsync();
  }

  public async Task RemoveIndexKey(string table, int key, int id)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = SqlBuilder.DeleteKey(table);
    command.Parameters.AddRange(
      [
        new SqlParameter("@key", key),
        new SqlParameter("@id", id)
      ]);
    await command.ExecuteNonQueryAsync();
  }

  public async Task RemoveIndexKey(string table, string key, string displayTerm, string language,
                             string? domain, int id) =>
    await UpdateIndexTable(SqlBuilder.DeleteTextKey(table, domain), key, displayTerm, language, domain, id);

  public async Task AddIndexKey(string table, string key, string displayTerm, string language, string? domain, int id) =>
    await UpdateIndexTable(SqlBuilder.InsertTextKey(table, domain), key, displayTerm, language, domain, id);

  private async Task UpdateIndexTable(string commandText, string key, string displayTerm, string language,
                                string? domain, int id)
  {
    CheckTransaction();
    using var command = connection!.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("@key", key),
        new SqlParameter("@displayTerm", displayTerm),
        new SqlParameter("@language", language),
        new SqlParameter("@id", id)
      ]);
    if (domain != null)
    {
      command.Parameters.Add(new SqlParameter("@domain", domain));
    }
    await command.ExecuteNonQueryAsync();
  }

  private int transactionLevel;

  private SqlTransaction? transaction = null;

  private SqlConnection? connection = null;
}


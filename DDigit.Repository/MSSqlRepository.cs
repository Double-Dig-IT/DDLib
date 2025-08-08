namespace DDigit.Repository;

public partial class MSSqlRepository : IDDRepository
{
  public async Task<RecordSetList> GetRecordSetMetaDataPerDatabaseAsync(DatabaseData database, string? searchTerm = null,
                                                                             int startFrom = 1, int limit = 0,
                                                                             CancellationToken cancellationToken = default)
  {
    var result = new RecordSetList();
    var table = database.Name!;
    var connection = await GetDbConnectionAsync(database);

    try
    {
      int current = 1;
      int hits = 0;
      using var command = connection.CreateCommand() as SqlCommand;
      command!.CommandText = SqlBuilder.GetSetData(table, searchTerm);
      if (!string.IsNullOrWhiteSpace(searchTerm))
      {
        command.Parameters.AddWithValue("term", $"%{searchTerm}%");
      }
      using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
      {
        hits++;
        if (current++ < startFrom || (limit > 0 && result.Count >= limit))
        {
          continue;
        }
        result.Add(new RecordSetMetaData(table, reader));
      }
      result.Hits = hits;
      return result;
    }
    catch (Exception)
    {
      throw;
    }
    finally
    {
      connection.Dispose();
    }
  }

  private static string? ReadString(SqlDataReader reader, string property)
  {
    var value = reader[property];
    return value != DBNull.Value ? value.ToString() : null;
  }

  public async Task<IDbConnection> GetDbConnectionAsync(DatabaseData database)
  {
    var connection = new SqlConnection(GetConnectionString(database));
    await connection.OpenAsync();
    return connection;
  }

  private static string GetConnectionString(DatabaseData database)
  {
    var connectionString = new SqlConnectionStringBuilder
    {
      DataSource = database.SqlServer,
      InitialCatalog = database.DSN,
      Encrypt = false,
      Pooling = true,
      MultipleActiveResultSets = true
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

  public async Task<int> AddWord(IDbConnection connection, IDbTransaction transaction, string word, string language,
                                 CancellationToken cancellationToken)
  {
    word = SqlBuilder.MaxLength(word);

    using var command = connection!.CreateCommand() as SqlCommand;
    if (command == null)
    {
      throw new NullReferenceException(nameof(command));
    }
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = SqlBuilder.SelectHighestWordNumber;
    var wordNumber = (int?)await command.ExecuteScalarAsync(cancellationToken)! + 1;
    command.CommandText = SqlBuilder.AddWord;
    command.Parameters.Add(new SqlParameter("term", word));
    command.Parameters.Add(new SqlParameter("displayTerm", word));
    command.Parameters.Add(new SqlParameter("language", language));
    command.Parameters.Add(new SqlParameter("wordNumber", wordNumber));
    await command.ExecuteNonQueryAsync(cancellationToken);
    return wordNumber != null ? wordNumber.Value : 0;
  }

  public async Task<int> GetWordNumber(IDbConnection connection, IDbTransaction transaction, string word, string language)
  {
    using var command = (SqlCommand)connection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = SqlBuilder.GetWordNumber;
    command.Parameters.Add(new SqlParameter("term", word));
    command.Parameters.Add(new SqlParameter("language", language));
    var result = await command.ExecuteScalarAsync();
    return result != null && result != DBNull.Value ? (int)result : 0;
  }

  public async Task<int> FindLink(string table, DatasetData? dataset, string? domain, string key, string language,
                                  IDbConnection connection,
                                  IDbTransaction transaction,
                                  CancellationToken cancellationToken)
  {
    using var command = (SqlCommand)connection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = SqlBuilder.FindLink(table, domain, language, dataset);
    command.Parameters.Add(new SqlParameter("term", key));
    if (language != null)
    {
      command.Parameters.Add(new SqlParameter("language", language));
    }
    if (domain != null)
    {
      command.Parameters.Add(new SqlParameter("domain", domain));
    }
    if (dataset != null)
    {
      command.Parameters.Add(new SqlParameter("lower", dataset.LowerLimit));
      command.Parameters.Add(new SqlParameter("upper", dataset.UpperLimit));
    }
    var result = await command.ExecuteScalarAsync(cancellationToken);
    return result != null && result != DBNull.Value ? (int)result : 0;
  }

  public async Task<object> ReadDataAsync(DatabaseData database, int id,
                             IDbConnection? connection,
                             IDbTransaction? transaction,
                             CancellationToken cancellationToken)
  {

    var currentConnection = (SqlConnection)(connection ?? await GetDbConnectionAsync(database));
    var currentTransaction = (SqlTransaction)(transaction ?? await StartTransactionAsync(currentConnection));
    var table = database.Name ?? throw new NullReferenceException(nameof(database.Name));

    try
    {
      using var command = currentConnection.CreateCommand();
      command.Transaction = currentTransaction;
      command.CommandText = SqlBuilder.SelectData(table);
      command.Parameters.AddWithValue("id", id);
      var result = await command.ExecuteScalarAsync(cancellationToken);
      if (transaction == null)
      {
        await currentTransaction.CommitAsync(cancellationToken);
      }
      if (connection == null)
      {
        currentConnection.Dispose();
      }
      return result;
    }
    catch (Exception)
    {
      if (transaction == null)
      {
        await currentTransaction.RollbackAsync(cancellationToken);
      }
      if (connection == null)
      {
        await currentConnection.DisposeAsync();
      }
      throw;
    }
  }

  public async Task<IEnumerable<RecordLock>> GetRecordLock(DatabaseData databaseData, CancellationToken cancellationToken)
  {
    var result = new List<RecordLock>();
    if (await GetDbConnectionAsync(databaseData) is SqlConnection connection)
    {
      using var command = connection.CreateCommand();
      command.CommandText = SqlBuilder.SelectAllRecordLocks;
      using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
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
      connection.Dispose();
    }
    return result;
  }

  public async Task<ResultSet> GetResultSetAsync(SearchTree searchTree, int set)
  {
    var database = searchTree.Database ?? throw new NullReferenceException(nameof(searchTree.Database));

    using var connection = new SqlConnection(GetConnectionString(database));
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = SqlBuilder.SelectFromHitList(database.Name!);
    command.Parameters.Add(new SqlParameter("@set", set));
    var result = await ReadIds(searchTree, database, command);
    connection.Dispose();
    return result;
  }

  public async Task PreparePreviousResultTable(SearchTree searchTree)
  {
    var previousResults = searchTree.PreviousResults;
    var cancellationToken = searchTree.Cancellation;
    if (previousResults != null)
    {
      if (searchTree.Connection is not SqlConnection sqlConnection)
      {
        throw new NullReferenceException(nameof(sqlConnection));
      }

      using var command = sqlConnection.CreateCommand();

      command.CommandText = SqlBuilder.CreatePreviousResultsTable;
      await command.ExecuteNonQueryAsync(cancellationToken);

      command.CommandText = SqlBuilder.CreatePreviousResultsIndex;
      await command.ExecuteNonQueryAsync(cancellationToken);

      command.CommandText = SqlBuilder.AddToPreviousResults;
      var parameter = new SqlParameter("@id", SqlDbType.Int);
      command.Parameters.Add(parameter);
      foreach (var id in previousResults.Ids)
      {
        parameter.Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
      }
    }
  }

  public async Task DropPreviousResultTable(SearchTree searchTree)
  {
    if (searchTree.PreviousResults != null)
    {
      if (searchTree.Connection is not SqlConnection sqlConnection)
      {
        throw new NullReferenceException(nameof(sqlConnection));
      }
      using var command = sqlConnection.CreateCommand();
      command.CommandText = SqlBuilder.DropPreviousResultsTable;
      await command.ExecuteNonQueryAsync(searchTree.Cancellation);
    }
  }

  public async Task<ResultSet> FindLinkedRecordSetAsync(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    if (searchTree.Connection is not SqlConnection sqlConnection)
    {
      throw new NullReferenceException(nameof(sqlConnection));
    }

    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var database = field.Database ?? throw new NullReferenceException(nameof(field.Database));

    using var command = sqlConnection.CreateCommand();
    if (leaf.IndirectionFields != null && leaf.IndirectionFields.Count > 0)
    {
      command.CommandText = SqlBuilder.LinkedFieldIndirectionSearch(searchTree, leaf);
    }
    else
    {
      command.CommandText = SqlBuilder.LinkedFieldSearch(searchTree, leaf);
    }
    leaf.SearchTerms.Clear();

    if (!leaf.SearchAll || (leaf.IndirectionFields != null && leaf.IndirectionFields.Count > 0))
    {
      //if (remoteIndex != null && remoteIndex.UseFullText)
      //{
      //  leaf.SearchTerms.Add(GetFreeTextOr(leaf.Values));
      //}
      //else
      //{
      foreach (var value in leaf.Values)
      {
        var key = SqlBuilder.ReplaceStar(value.ToString());
        if (!string.IsNullOrEmpty(key))
        {
          leaf.SearchTerms.Add(key);
        }
      }
      //}

      int i = 0;
      foreach (var value in leaf.SearchTerms)
      {
        command.Parameters.Add(new SqlParameter($"term{i++}", value));
      }
    }
    return await ReadIds(searchTree, database, command);
  }

  private async static Task<ResultSet> ReadIds(SearchTree searchTree, DatabaseData databaseData, SqlCommand sqlCommand)
  {
    var result = new ResultSet(databaseData);

    var previous = searchTree.PreviousResults != null ? new HashSet<int>(searchTree.PreviousResults.Ids) : null;
    using var reader = await sqlCommand.ExecuteReaderAsync(searchTree.Cancellation);
    while (await reader.ReadAsync())
    {
      result.Add(reader.GetInt32(0), searchTree.DatasetFilter, previous);
    }

    if (!searchTree.SortFields.Any())
    {
      result.Ids = [.. result.Ids.Distinct()];
      result.Ids.Sort();
    }
    result.Count = result.Ids.Count;

    return result;
  }

  public async Task<ResultSet> FindFlatIndexedRecordSetAsync(IDbCommand command, SearchTree searchTree, SearchTreeLeaf leaf)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));

    if (command is not SqlCommand sqlCommand)
    {
      throw new NullReferenceException(nameof(sqlCommand));
    }

    if (index.TableName.Contains('_'))
    {
      switch (index.Type)
      {
        case IndexTypeEnum.FreeText:
          WordSearch(searchTree, leaf, command);
          break;

        case IndexTypeEnum.IsoDate:
          IsoDateSearch(searchTree, leaf, command);
          break;

        default:
          if (field.Enumeration)
          {
            EnumerativeSearch(searchTree, leaf, command);
          }
          else
          {
            TermSearch(searchTree, leaf, command);
          }
          break;
      }
    }
    else
    {
      sqlCommand.CommandText = SqlBuilder.IdSearch(index.TableName, leaf);
      leaf.CopyValuesToSearchTerms();
    }

    int i = 0;
    foreach (var term in leaf.SearchTerms)
    {
      sqlCommand.Parameters.AddWithValue($"term{i++}", ConvertKey(index.Type, term, index.DateCompletion));
    }

    return await ReadIds(searchTree, field.Database, sqlCommand);
  }

  /// <summary>
  /// Convert a key according to index type, but pass through a single % (empty search)
  /// </summary>
  /// <param name="type"></param>
  /// <param name="term"></param>
  /// <param name="dateCompletion"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  private static object ConvertKey(IndexTypeEnum type, string term, DateCompletionEnum dateCompletion)
    => (term == "%") ? term : type switch
    {
      IndexTypeEnum.Undefined => term,
      IndexTypeEnum.Date => KeyConversions.DateTimeStringToInt(term),
      IndexTypeEnum.Text => term,
      IndexTypeEnum.FreeText => term,
      IndexTypeEnum.Integer => Convert.ToInt32(term),
      IndexTypeEnum.IsoDate => KeyConversions.IsoDateToDecimal(term, dateCompletion),
      IndexTypeEnum.Boolean => term,
      IndexTypeEnum.AlphaNumeric => term, //KeyConversions.AlphaKeyValue(term, 10),
      _ => throw new NotImplementedException(),
    };


  private static void TermSearch(SearchTree searchTree, SearchTreeLeaf leaf, IDbCommand command)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));

    leaf.SearchTerms.Clear();
    foreach (var value in leaf.Values)
    {
      var key = value.ToString();
      if (key != null)
      {
        var truncated = SqlBuilder.ReplaceStar(key);
        if (truncated != null)
        {
          leaf.SearchTerms.Add(truncated);
        }
      }
    }

    var index = field.PreferredIndex;

    if (leaf.Operator == SearchOperatorEnum.Generic)
    {
      command.CommandText = SqlBuilder.GenericSearch(searchTree, leaf);
    }
    else
    {
      if (index != null && index.UseFullText)
      {
        command.CommandText = SqlBuilder.FullTextTermSearch(searchTree, leaf);
      }
      else
      {
        command.CommandText = SqlBuilder.Search(searchTree, leaf);
      }
    }
  }

  private static void IsoDateSearch(SearchTree searchTree, SearchTreeLeaf leaf, IDbCommand command)
  {
    leaf.SearchTerms.Clear();
    foreach (var value in leaf.Values)
    {
      var key = value.ToString();
      if (key != null)
      {
        var truncated = SqlBuilder.ReplaceStar(key);
        if (truncated != null)
        {
          leaf.SearchTerms.Add(truncated);
        }
      }
    }

    command.CommandText = SqlBuilder.Search(searchTree, leaf);
  }

  private static void WordSearch(SearchTree searchTree, SearchTreeLeaf leaf, IDbCommand command)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));
    leaf.SearchTerms.Clear();

    if (index.UseFullText)
    {
      foreach (var value in leaf.Values)
      {
        leaf.SearchTerms.Add(FullTextValue(value.ToString(), false));
      }
      command.CommandText = SqlBuilder.FullTextSearch(searchTree, leaf);
    }
    else
    {
      foreach (var value in leaf.Values)
      {
        foreach (var word in SqlBuilder.GetFreeTextSearchWords(value.ToString()))
        {
          if (word != null)
          {
            var truncated = SqlBuilder.ReplaceStar(word);
            if (truncated != null)
            {
              leaf.SearchTerms.Add(truncated);
            }
          }
        }
      }
      command.CommandText = SqlBuilder.FreeTextSearch(searchTree, leaf, index.TableName);
    }
  }


  public static void EnumerativeSearch(SearchTree searchTree, SearchTreeLeaf leaf, IDbCommand command)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));
    var database = field.Database ?? throw new NullReferenceException(nameof(field.Database));

    // get a list of enumerative neutral keys
    leaf.SearchTerms.Clear();
    if (leaf.Values.Count > 0)
    {
      foreach (var value in leaf.Values)
      {
        foreach (var key in field.EnumKeys(value.ToString(), leaf.Language))
        {
          if (!leaf.SearchTerms.Contains(key))
          {
            leaf.SearchTerms.Add(key);
          }
        }
      }
    }

    command.CommandText = leaf.SearchTerms.Count > 0 ?
      database.FullText ?
        SqlBuilder.EnumSearchFullText(searchTree, leaf, database.FullTextTable, field.Tag!) :
        SqlBuilder.EnumSearch(searchTree, leaf, index.TableName) :
       SqlBuilder.EmptySearch(index.TableName);

  }

  public async Task<ResultSet> ReadAllRecordsAsync(IDbCommand command, SearchTree searchTree)
  {
    var databaseData = searchTree.Database ?? throw new NullReferenceException(nameof(searchTree.Database));
    if (command is not SqlCommand sqlCommand)
    {
      throw new NullReferenceException(nameof(sqlCommand));
    }
    sqlCommand.CommandText = SqlBuilder.ReadAllRecords(databaseData, searchTree.PreviousResults);
    return await ReadIds(searchTree, databaseData, sqlCommand);
  }

  private static string FullTextValue(string? value, bool truncate)
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

      if (truncate && !value.EndsWith('*'))
      {
        searchString.Append('*');
      }
    }
    searchString.Append('\"');

    return searchString.ToString(); ;
  }

  public async Task<AutoCompleteResult?> GetAutoCompleteAsync(IEnumerable<FieldData> fields, DatasetFilter? datasetFilter,
                                                         string? value, int? startFrom, int? limit, string? language, bool count,
                                                         CancellationToken cancellationToken)
  {
    var result = new AutoCompleteResult(startFrom ?? 1, limit ?? 10);

    var firstField = fields.FirstOrDefault() ?? throw new NullReferenceException(nameof(fields));
    var databaseData = firstField.Database!;
    var connection = await GetDbConnectionAsync(databaseData) as SqlConnection;

    try
    {
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
              sql.AppendLine(SqlBuilder.FullTextAutoComplete(databaseData.FullTextTable, value, fieldData.Tag, datasetFilter, fieldNumber));
              command.Parameters.AddWithValue($"term{fieldNumber}", FullTextValue(value, true));
            }
            else
            {
              sql.AppendLine(SqlBuilder.FreeTextAutoComplete(index.TableName, datasetFilter, fieldNumber));
              command.Parameters.AddWithValue($"term{fieldNumber}", term);
            }
            break;

          case IndexTypeEnum.Integer:
            if (fieldData.IsLinked)
            {
              sql.AppendLine(SqlBuilder.LinkedAutocomplete(fieldData, datasetFilter, fieldNumber, value));
              if (fieldData.LinkedDatabase!.FullText)
              {
                command.Parameters.AddWithValue($"term{fieldNumber}", FullTextValue(value, true));
              }
              else
              {
                command.Parameters.AddWithValue($"term{fieldNumber}", term);
              }
            }
            else
            {
              sql.AppendLine(SqlBuilder.IntegerAutoComplete(index.TableName, datasetFilter, fieldNumber));
              command.Parameters.AddWithValue($"term{fieldNumber}", number);
            }
            break;

          default:
            if (fieldData.Enumeration)
            {
              AutoCompleteEnum(command, sql, index.TableName, datasetFilter, fieldNumber, fieldData, value, language, count);
            }
            else
            {
              if (databaseData.FullText)
              {
                sql.AppendLine(SqlBuilder.FullTextAutoComplete(databaseData.FullTextTable, value, fieldData.Tag, datasetFilter, fieldNumber));
                command.Parameters.AddWithValue($"term{fieldNumber}", FullTextValue(value, true));
              }
              else
              {
                sql.AppendLine(SqlBuilder.FlatAutoComplete(index.TableName, datasetFilter, fieldNumber));
                command.Parameters.AddWithValue($"term{fieldNumber}", term);
              }
            }
            break;
        }

        if (!string.IsNullOrWhiteSpace(fieldData.LinkDomain))
        {
          command.Parameters.AddWithValue($"domain{fieldNumber}", fieldData.LinkDomain);
        }
      }

      sql.AppendLine(") result");
      sql.AppendLine(" group by term, [use] order by term");

      command.CommandText = sql.ToString();
      using var reader = await command.ExecuteReaderAsync(cancellationToken);

      int counter = 0;
      while (await reader.ReadAsync(cancellationToken))
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
      if (fields.Count() == 1 && firstField.Enumeration && language != null)
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
    catch (Exception)
    {
      throw;
    }
    finally
    {
      connection?.Dispose();
    }
  }

  private static void AutoCompleteEnum(SqlCommand command, StringBuilder sql, string tableName,
                                DatasetFilter? datasetFilter, int fieldNumber, FieldData fieldData,
                                string? value, string? language, bool count)
  {
    value ??= "";
    if (!value.EndsWith('*'))
    {
      value += "*";
    }
    var keys = fieldData.EnumKeys(value, language);

    sql.AppendLine(SqlBuilder.EnumAutoComplete(tableName, datasetFilter, fieldNumber, keys, count));

    int i = 0;
    foreach (var key in keys)
    {
      if (key != null)
      {
        command.Parameters.AddWithValue($"term{fieldNumber}key{i}", key.ToLower());
        i++;
      }
    }
  }

  public async Task<int> GetNewRecordIdAsync(IDbConnection connection, IDbTransaction transaction, DatabaseData databaseData, DatasetData? datasetData)
  {
    int min = datasetData != null ? datasetData.LowerLimit : 1;
    int max = datasetData != null ? datasetData.UpperLimit : int.MaxValue;
    using var command = (SqlCommand)connection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = $"select max(priref) from {databaseData.Name} where priref between @min and @max";
    command.Parameters.AddRange([new SqlParameter("@min", min), new SqlParameter("@max", max)]);

    var last = await command.ExecuteScalarAsync();
    var lastPriref = last == DBNull.Value ? 0 : (int)last!;
    if (lastPriref == max)
    {
      var text = datasetData != null ? $"Dataset {datasetData.Name}" : $"Database {databaseData.Name}";
      throw new DDException($"{text} is full");
    }

    return lastPriref + 1;
  }

  public async Task WriteNewDataAsync(string table, int id, DateTime creation,
                                      DateTime modification, string data,
                                      IDbConnection connection,
                                      IDbTransaction transaction,
                                      CancellationToken cancellationToken)
  {
    using var command = (SqlCommand)connection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = $"insert into [{table}] (priref, creation, modification, data)" +
                           "values (@priref, @creation, @modification, @data)";
    command.Parameters.AddRange(
      [
        new SqlParameter("priref", id),
        new SqlParameter("creation", creation),
        new SqlParameter("modification", modification),
        new SqlParameter("data", data)
      ]);
    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task UpdateDataAsync(IDbConnection connection, IDbTransaction transaction, string table,
                               int id, DateTime modification, string data, CancellationToken cancellationToken)
  {
    using var command = (SqlCommand)connection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = $"update [{table}] set modification = @modification, data = @data where priref = @priref";
    command.Parameters.AddRange(
      [
        new SqlParameter("priref", id),
        new SqlParameter("modification", modification),
        new SqlParameter("data", data)
      ]);
    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<IDbTransaction> StartTransactionAsync(IDbConnection connection)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    return await sqlConnection.BeginTransactionAsync();
  }

  public async Task RollbackAsync(IDbTransaction transaction, CancellationToken cancellationToken) =>
    await ((SqlTransaction)transaction).RollbackAsync(cancellationToken);

  public async Task CommitAsync(IDbTransaction transaction, CancellationToken cancellationToken) =>
    await ((SqlTransaction)transaction).CommitAsync(cancellationToken);

  public async Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IntegerIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    using var command = sqlConnection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = SqlBuilder.InsertKey(row.Table);
    command.Parameters.AddRange(
      [
        new SqlParameter("key", row.Term),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction,
                                   IntegerIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    using var command = sqlConnection.CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = SqlBuilder.DeleteKey(row.Table);
    command.Parameters.AddRange(
      [
        new SqlParameter("key", row.Term),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction,
                                   string? fullTextTable, TermIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var deletes = 0;
    if (fullTextTable != null)
    {
      deletes += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.DeleteFullTextKey(fullTextTable, row.Domain), row, cancellationToken);
      if (row!.Index!.Unique)
      {
        deletes += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.DeleteTextKey(row.Table, row.Domain), row, cancellationToken);
      }
    }
    else
    {
      deletes += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.DeleteTextKey(row.Table, row.Domain), row, cancellationToken);
    }
    return deletes;
  }

  public async Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction,
                                string? fullTextTable, TermIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var inserts = 0;
    if (fullTextTable != null)
    {
      inserts += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertFullTextKey(fullTextTable), row, cancellationToken);
      if (row!.Index!.Unique)
      {
        inserts += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertTextKey(row.Table, row.Domain), row, cancellationToken);
      }
    }
    else
    {
      inserts += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertTextKey(row.Table, row.Domain), row, cancellationToken);
    }
    return inserts;
  }

  public async Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, DateIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    return await UpdateDateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertDateKey(row.Table), row, cancellationToken);
  }

  public async Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, BooleanIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    return await UpdateBooleanIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertBooleanKey(row.Table), row, cancellationToken);
  }

  public async Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, DateIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    return await UpdateDateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.DeleteDateKey(row.Table), row, cancellationToken);
  }

  public async Task<int> AddIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IsoDateIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    return await UpdateDateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertIsoDateKey(row.Table), row, cancellationToken);
  }


  public async Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, IsoDateIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    return await UpdateDateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.DeleteIsoDateKey(row.Table), row, cancellationToken);
  }

  public async Task<int> DeleteIndexKeyAsync(IDbConnection connection, IDbTransaction transaction, BooleanIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    return await UpdateBooleanIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.DeleteBooleanKey(row.Table), row, cancellationToken);
  }

  private static async Task<int> UpdateDateIndexTableAsync(SqlConnection connection, SqlTransaction transaction, string commandText, DateIndexRow row, CancellationToken cancellationToken)
  {
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("term", row.Term),
        new SqlParameter("displayTerm", row.DisplayTerm),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  private static async Task<int> UpdateBooleanIndexTableAsync(SqlConnection connection, SqlTransaction transaction, string commandText, BooleanIndexRow row, CancellationToken cancellationToken)
  {
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("term", row.Term),
        new SqlParameter("displayTerm", row.DisplayTerm),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  private static async Task<int> UpdateIndexTableAsync(SqlConnection connection, SqlTransaction transaction, string commandText,
                                             TermIndexRow row, CancellationToken cancellationToken)
  {
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("tag", row.Tag),
        new SqlParameter("occ", row.Occ),
        new SqlParameter("key", row.Term),
        new SqlParameter("displayTerm", row.DisplayTerm),
        new SqlParameter("strippedTerm", row.StrippedTerm != null ? row.StrippedTerm : DBNull.Value),
        new SqlParameter("domain", row.Domain != null ? row.Domain : DBNull.Value),
        new SqlParameter("language", row.Language != null ? row.Language : DBNull.Value),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<List<int>> ReadLinkedRecordIdsAsync(IDbConnection connection, IDbTransaction transaction, string tableName, int id, CancellationToken cancellationToken)
  {
    var result = new List<int>();
    using var command = ((SqlConnection)connection).CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.Parameters.Add(new SqlParameter("id", id));
    command.CommandText = SqlBuilder.ReadLinkedRecords(tableName);
    try
    {
      using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
      {
        result.Add(reader.GetInt32(0));
      }
    }
    // This is a temporary catch to prevent the program from aborting when a specific SQL table is missing.
    catch (SqlException ex)
    {
      if (ex.ErrorCode != -2146232060)
      {
        throw;
      }
    }
    return result;
  }

  public async Task<int> DeleteIndexKeysAsync(IDbConnection connection, IDbTransaction transaction,
                              string tableName, int id, CancellationToken cancellationToken)
  {
    using var command = ((SqlConnection)connection).CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.Parameters.Add(new SqlParameter("id", id));
    command.CommandText = SqlBuilder.DeleteIndexKeys(tableName);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public static async Task<bool> TableExists(IDbConnection connection, string tableName)
  {
    using var command = ((SqlConnection)connection).CreateCommand();
    command.CommandText = "select count(*) from [information_schema].[tables] where [table_Name] = @tableName";
    command.Parameters.AddWithValue("tableName", tableName);
    var result = await command.ExecuteScalarAsync();
    return result != null && (int)result == 0;
  }

  /// <summary>
  /// A bit of a complex story here. Alphanumeric keys are normal keys, but for a full text index they also need to be added there.
  /// </summary>
  /// <param name="connection"></param>
  /// <param name="transaction"></param>
  /// <param name="fullTextTable"></param>
  /// <param name="row"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="NullReferenceException"></exception>
  public async Task<int> AddIndexKey(IDbConnection connection, IDbTransaction transaction, string? fullTextTable,
                                     AlphaNumericIndexRow row, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    int inserts = await UpdateIndexTable(sqlConnection, sqlTransaction, SqlBuilder.InsertAlphanumericKey(row.Table), row, cancellationToken);
    if (fullTextTable != null)
    {
      if (row.Index != null && row.Tag != null && row.DisplayTerm != null)
      {
        var indexRow = new TermIndexRow(row.Index, row.Tag, row.Occ, row.DisplayTerm, row.Id);
        inserts += await UpdateIndexTableAsync(sqlConnection, sqlTransaction, SqlBuilder.InsertFullTextKey(fullTextTable), indexRow, cancellationToken);
      }
    }
    return inserts;
  }

  private static async Task<int> UpdateIndexTable(SqlConnection connection, SqlTransaction transaction, string commandText,
                                            AlphaNumericIndexRow row, CancellationToken cancellationToken)
  {
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("tag", row.Tag),
        new SqlParameter("occ", row.Occ),
        new SqlParameter("key", row.Term),
        new SqlParameter("displayTerm", row.DisplayTerm),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task<int> DeleteIndexKey(IDbConnection connection, IDbTransaction transaction, string? fullTextTable,
                                  AlphaNumericIndexRow alphaNumericRow, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    int deletes = await UpdateIndexTable(sqlConnection, sqlTransaction, SqlBuilder.DeleteAlphaNumericKey(alphaNumericRow.Table), alphaNumericRow, cancellationToken);
    if (fullTextTable != null)
    {
      deletes += await UpdateIndexTable(sqlConnection, sqlTransaction, SqlBuilder.DeleteFullTextKey(fullTextTable), alphaNumericRow, cancellationToken);
    }
    return deletes;
  }

  public static async Task<int> WriteRecordSetMetaDataAsync(RecordSetMetaData metaData, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    string sql;
    if (metaData.Number == 0 || !await CheckIfRecordSetExists(metaData.Database!, metaData.Number, connection, transaction, cancellationToken))
    {
      metaData.Number = await GetNewRecordSetNumber(metaData.Database!, connection, transaction, cancellationToken);
      sql = SqlBuilder.CreateRecordSet(metaData.Database!);
    }
    else
    {
      sql = SqlBuilder.UpdateRecordSet(metaData.Database!);
    }
    await WriteRecordSetMetaDataAsync(metaData, sql, connection, transaction, cancellationToken);
    return metaData.Number;
  }

  private static async Task WriteRecordSetMetaDataAsync(RecordSetMetaData metaData, string sql, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = sql;

    command.Parameters.AddWithValue("number", metaData.Number);
    command.Parameters.AddWithValue("title", metaData.Title);
    command.Parameters.AddWithValue("selection", metaData.Selection);
    command.Parameters.AddWithValue("modification", metaData.Modified == DateTime.MinValue ? DateTime.Now : metaData.Modified);
    command.Parameters.AddWithValue("hitcount", metaData.Hits);
    command.Parameters.AddWithValue("creation", metaData.Created);
    command.Parameters.AddWithValue("owner", metaData.Owner);

    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  private static async Task<bool> CheckIfRecordSetExists(string database, int number, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = SqlBuilder.ExistsSetNumber(database);
    command.Parameters.AddWithValue("number", number);
    return (int)await command.ExecuteScalarAsync(cancellationToken) == 1;
  }

  private static async Task<int> GetNewRecordSetNumber(string database, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = SqlBuilder.GetNewSetNumber(database);
    return (int)await command.ExecuteScalarAsync(cancellationToken);
  }

  public static Task ClearRecordSetHitsAsync(string database, int setNo,
                                             IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
    => Clear(SqlBuilder.ClearRecordSetHits(database), setNo, connection, transaction, cancellationToken);

  public static Task ClearRecordSetAccessAsync(string database, int setNo,
                                               IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
   => Clear(SqlBuilder.ClearRecordSetAccess(database), setNo, connection, transaction, cancellationToken);

  public static Task ClearRecordSetEmailsAsync(string database, int setNo,
                                               IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  => Clear(SqlBuilder.ClearRecordSetEmails(database), setNo, connection, transaction, cancellationToken);

  private static Task ClearRecordSetMetaData(string database, int setNo,
                                             IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  => Clear(SqlBuilder.ClearRecordSetMetaData(database), setNo, connection, transaction, cancellationToken);

  private static async Task Clear(string commandText, int number, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = commandText;
    command.Parameters.AddWithValue("number", number);
    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public static async Task WriteHitsAsync(RecordSetMetaData metaData, ResultSet set, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    var sqlTransaction = (SqlTransaction)transaction ?? throw new NullReferenceException(nameof(transaction));
    var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = SqlBuilder.WriteRecordSetHits(metaData.Database);
    foreach (var id in set.Ids)
    {
      command.Parameters.Clear();
      command.Parameters.AddWithValue("number", metaData.Number);
      command.Parameters.AddWithValue("id", id);
      await command.ExecuteNonQueryAsync(cancellationToken);
    }
  }

  public async Task<int> WriteRecordSetAsync(string folder, RecordSetMetaData metaData, ResultSet set, CancellationToken cancellationToken = default)
  {
    var database = metaData.Database!;
    var setNo = metaData.Number;
    var databaseData = MetaDataCache.ReadDatabase(folder, metaData.Database!) ?? throw new DatabaseNotFoundException(folder, database);
    var connection = await GetDbConnectionAsync(databaseData);
    var transaction = await StartTransactionAsync(connection);
    try
    {
      metaData.Hits = set.Ids.Count;
      int number = await WriteRecordSetMetaDataAsync(metaData, connection, transaction, cancellationToken);
      await ClearRecordSetHitsAsync(database, setNo, connection, transaction, cancellationToken);
      await ClearRecordSetAccessAsync(database, setNo, connection, transaction, cancellationToken);
      await ClearRecordSetEmailsAsync(database, setNo, connection, transaction, cancellationToken);
      await WriteHitsAsync(metaData, set, connection, transaction, cancellationToken);
      await CommitAsync(transaction, cancellationToken);
      return number;
    }
    catch
    {
      await RollbackAsync(transaction, cancellationToken);
      throw;
    }
  }

  public async Task DeleteRecordSetAsync(string folder, string database, int setNo, CancellationToken cancellationToken)
  {
    var databaseData = MetaDataCache.ReadDatabase(folder, database) ?? throw new DatabaseNotFoundException(folder, database);
    using var connection = await GetDbConnectionAsync(databaseData);
    var transaction = await StartTransactionAsync(connection);
    try
    {
      await ClearRecordSetMetaData(database, setNo, connection, transaction, cancellationToken);
      await ClearRecordSetHitsAsync(database, setNo, connection, transaction, cancellationToken);
      await ClearRecordSetAccessAsync(database, setNo, connection, transaction, cancellationToken);
      await ClearRecordSetEmailsAsync(database, setNo, connection, transaction, cancellationToken);
      await CommitAsync(transaction, cancellationToken);
    }
    catch
    {
      await RollbackAsync(transaction, cancellationToken);
    }
  }

  public async Task<List<HierarchyNode>> SearchLocationsAsync(DatabaseData locations, string nameField, string barcodeField, string value, SearchLimits limits)
  {
    var result = new List<HierarchyNode>();
    using var connection = (SqlConnection)await GetDbConnectionAsync(locations);
    await SearchByBarcode(connection, locations, barcodeField, value, limits, result);
    if (limits.Count < limits.Limit)
    {
      await SearchByLocationCode(connection, locations, nameField, value, limits, result);
    }
    return result;
  }

  private async Task SearchByBarcode(SqlConnection connection, DatabaseData locations, string barcodeField, string value, SearchLimits limits, List<HierarchyNode> result)
  {
    var barcodeFieldData = locations.FindFieldByTagOrName(barcodeField) ??
    throw new FieldNotFoundException(barcodeField, locations.Name);
    var barcodeTable = barcodeFieldData.GetIndexTableName();
    using var command = connection.CreateCommand();
    command.CommandText = $"select priref, displayTerm from {barcodeTable} where term like @term";
    command.Parameters.AddWithValue("term", value + "%");
    using var reader = await command.ExecuteReaderAsync();
    while (reader.Read() && limits.Count < limits.Limit)
    {
      var node = new HierarchyNode
      {
        Id = reader.GetInt32(0),
        Key = reader.GetString(1),
        Match = true
      };

      limits.Count++;
      result.Add(node);
    }
  }

  private async static Task SearchByLocationCode(SqlConnection connection, DatabaseData locations, string nameField, string value, SearchLimits limits, List<HierarchyNode> result)
  {
    var nameFieldData = locations.FindFieldByTagOrName(nameField) ??
      throw new FieldNotFoundException(nameField, locations.Name);
    var nameTable = nameFieldData.GetIndexTableName();
    var partsOfField = nameFieldData.GetPartsOfField();
    var partsOfTable = partsOfField?.GetIndexTableName();
    var parts = value.Split([' ', '\\', '/', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    int level = 0;
    using var command = connection.CreateCommand();
    command.CommandText = $"select priref, displayTerm from {nameTable} where term like @term and priref not in (select priref from {partsOfTable})";
    command.Parameters.AddWithValue("term", parts[0] + "%");

    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync() && limits.Count < limits.Limit)
    {
      var node = new HierarchyNode
      {
        Id = reader.GetInt32(0),
        Key = reader.GetString(1),
        Children = await GetChildren(connection, partsOfTable!, reader.GetInt32(0), nameTable, parts, level + 1, limits),
        Match = parts.Length <= level + 1
      };

      if (node.Match)
      {
        limits.Count++;
      }
      result.Add(node);
    }
  }

  private static async Task<List<HierarchyNode>> GetChildren(SqlConnection connection, string partOfTable, int id, string nameTable,
                                                             string[] parts, int level, SearchLimits limits)
  {
    var key = level < parts.Length ? parts[level] : string.Empty;

    using var command = connection.CreateCommand();

    command.CommandText = $"""
      select [{partOfTable}].[priref], [{nameTable}].[displayterm] from [{partOfTable}] inner join [{nameTable}] on [{partOfTable}].[priref] = [{nameTable}].[priref]
      where [{partOfTable}].[term] = @id
      """;

    command.Parameters.AddWithValue("id", id);

    if (key != string.Empty)
    {
      command.CommandText += $" and [{nameTable}].[term] like @term";
      command.Parameters.AddWithValue("term", parts[level] + "%");
    }

    var result = new List<HierarchyNode>();
    using var partsReader = await command.ExecuteReaderAsync();

    while (await partsReader.ReadAsync() && limits.Count < limits.Limit)
    {
      var node = (new HierarchyNode
      {
        Id = partsReader.GetInt32(0),
        Key = partsReader.GetString(1),
        Children = await GetChildren(connection, partOfTable, partsReader.GetInt32(0), nameTable, parts, level + 1, limits),
        Match = parts.Length <= level + 1
      });

      if (node.Match)
      {
        limits.Count++;
      }
      result.Add(node);
    }

    return result;
  }

  public Task<IEnumerable<int>> SearchLinksAsync(DatabaseData database, DatasetData? dataset, FieldData field, string searchValue, string domain, SearchLimits limits)
      => SearchLinksDomainAsync(database, dataset, field.LinkedFieldData!, searchValue, domain, limits);

  private async Task<IEnumerable<int>> SearchLinksDomainAsync(DatabaseData linkedDatabase, DatasetData? linkedDataset, FieldData linkedField,
                                       string searchValue, string domain, SearchLimits limits)
  {
    var table = linkedField.GetIndexTableName();
    using var connection = (SqlConnection)await GetDbConnectionAsync(linkedDatabase);
    using var command = connection.CreateCommand();
    command.CommandText = $"select priref from {table} where term like @term and domain = @domain";
    if (linkedDataset != null)
    {
      command.CommandText += " and priref between @lowerLimit and @upperLimit";
      command.Parameters.AddWithValue("lowerLimit", linkedDataset.LowerLimit);
      command.Parameters.AddWithValue("upperLimit", linkedDataset.UpperLimit);
    }

    command.Parameters.AddWithValue("term", searchValue + "%");
    command.Parameters.AddWithValue("domain", domain ?? string.Empty);
    using var reader = await command.ExecuteReaderAsync();
    var ids = new List<int>();
    int count = 0;
    while (reader.Read())
    {
      count++;
      if (count >= limits.StartFrom)
      {
        ids.Add(reader.GetInt32(0));
        if (ids.Count >= limits.Limit)
        {
          break;
        }
      }
    }
    return ids;
  }

  public async Task<string> GetAutoNumberValue(IDbConnection connection, IDbTransaction transaction,
                                   FieldData fieldData, CancellationToken cancellationToken)
  {
    using var command = ((SqlConnection)connection).CreateCommand();
    command.Transaction = (SqlTransaction)transaction;
    command.CommandText = SqlBuilder.GetAutoNumberValue();
    command.Parameters.AddWithValue("databaseName", fieldData.Database!.Name!);
    command.Parameters.AddWithValue("tag", fieldData.Tag);
    command.Parameters.AddWithValue("delta", fieldData.AutoNumberIncrement);
    command.Parameters.AddWithValue("minimum", fieldData.AutoNumberStartValue);
    command.Parameters.AddWithValue("prefix", fieldData.AutoNumberPrefix);
    command.Parameters.AddWithValue("suffix", fieldData.AutoNumberSuffix);
    var result = await command.ExecuteScalarAsync(cancellationToken);
    if (result == null || result == DBNull.Value)
    {
      throw new DDException($"Could not retrieve auto number value for field '{fieldData.Tag}' in database '{fieldData.Database.Name}'");
    }
    return result.ToString()
      ?? throw new DDException($"Could not convert auto number value for field '{fieldData.Tag}' in database '{fieldData.Database.Name}' to string");
  }
}



using System.Management.Automation.Runspaces;

namespace DDigit.Repository;

public partial class MSSqlRepository : IDDRepository
{
  public async Task<RecordSetList> GetRecordSetMetaDataPerDatabaseAsync(DatabaseData database, string? searchTerm = null,
                                                                             int startFrom = 1, int limit = 0, RecordSetSortEnum? sort = null, SearchSortOrderEnum? sortOrder = null,
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
      command!.CommandText = SqlBuilder.GetSetData(table, searchTerm, sort, sortOrder is SearchSortOrderEnum.Descending);
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
      Encrypt = false,
      Pooling = true,
      MaxPoolSize = 8,
      ApplicationName = "DDLib",
      MultipleActiveResultSets = false //true
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

  public async Task<int> AddWord(string word, string language, SqlStateInfo sqlState)
  {
    word = SqlBuilder.MaxLength(word);

    using var command = (SqlCommand)sqlState.Connection!.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = SqlBuilder.SelectHighestWordNumber;
    var wordNumber = (int?)await command.ExecuteScalarAsync(sqlState.CancellationToken)! + 1;
    command.CommandText = SqlBuilder.AddWord;
    command.Parameters.Add(new SqlParameter("term", word));
    command.Parameters.Add(new SqlParameter("displayTerm", word));
    command.Parameters.Add(new SqlParameter("language", language));
    command.Parameters.Add(new SqlParameter("wordNumber", wordNumber));
    await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
    return wordNumber != null ? wordNumber.Value : 0;
  }

  public async Task<int> GetWordNumberAsync(string word, string language, SqlStateInfo sqlState)
  {
    using var command = (SqlCommand)sqlState.Connection!.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = SqlBuilder.GetWordNumber;
    command.Parameters.Add(new SqlParameter("term", word));
    command.Parameters.Add(new SqlParameter("language", language));
    var result = await command.ExecuteScalarAsync(sqlState.CancellationToken);
    return result != null && result != DBNull.Value ? (int)result : 0;
  }

  public async Task<int> FindLink(string table, DatasetData? dataset, string? tag, string? domain, string key,
                                  bool isMultiLingual, string language, SqlStateInfo sqlState)
  {
    using var command = (SqlCommand)sqlState.Connection!.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = SqlBuilder.FindLink(table, tag, domain, isMultiLingual, language, dataset);

    command.Parameters.AddWithValue("term", key);

    if (tag is not null)
    {
      command.Parameters.AddWithValue("tag", tag);
    }

    if (isMultiLingual && language is not null)
    {
      command.Parameters.AddWithValue("language", language);
    }

    if (domain is not null)
    {
      command.Parameters.AddWithValue("domain", domain);
    }

    if (dataset is not null)
    {
      command.Parameters.AddWithValue("lower", dataset.LowerLimit);
      command.Parameters.AddWithValue("upper", dataset.UpperLimit);
    }

    var result = await command.ExecuteScalarAsync(sqlState.CancellationToken);
    return result is not null && result != DBNull.Value ? (int)result : 0;
  }

  public async Task<object> ReadDataAsync(DatabaseData database, int id, SqlStateInfo sqlState)
  {
    var createConnection = sqlState.Connection is null;
    var createTransaction = sqlState.Transaction is null;

    try
    {
      if (createConnection)
      {
        sqlState.Connection = await GetDbConnectionAsync(database);
      }

      if (createTransaction)
      {
        sqlState.Transaction = await StartTransactionAsync(sqlState.Connection!);
      }

      await using var command = ((SqlConnection)sqlState.Connection!).CreateCommand();
      command.Transaction = (SqlTransaction?)sqlState.Transaction;
      command.CommandText = SqlBuilder.SelectData(database.Name!);
      command.Parameters.AddWithValue("id", id);
      var result = await command.ExecuteScalarAsync(sqlState.CancellationToken);

      if (createTransaction)
      {
        await ((SqlTransaction)sqlState.Transaction!).CommitAsync(CancellationToken.None);
        await ((SqlTransaction)sqlState.Transaction!).DisposeAsync();
        sqlState.Transaction = null;
      }

      if (createConnection)
      {
        await ((SqlConnection)sqlState.Connection!).DisposeAsync();
        sqlState.Connection = null;
      }
      return result;
    }

    catch (OperationCanceledException) when (sqlState.CancellationToken.IsCancellationRequested)
    {
      if (createTransaction && sqlState.Transaction is SqlTransaction transaction && transaction.Connection is not null)
      {
        await transaction.RollbackAsync(CancellationToken.None);
      }
      throw;
    }

    catch
    {
      if (createTransaction && sqlState.Transaction is SqlTransaction transaction && transaction.Connection is not null)
      {
        await transaction.RollbackAsync(CancellationToken.None);
      }
      throw;
    }

    finally
    {
      if (createTransaction && sqlState.Transaction is SqlTransaction transaction)
      {
        await transaction.DisposeAsync();
        sqlState.Transaction = null;
      }

      if (createConnection && sqlState.Connection is SqlConnection connection)
      {
        await connection.DisposeAsync();
        sqlState.Connection = null;
      }
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
    return await ReadIds(searchTree, database, command);
  }

  public async Task PreparePreviousResultTable(SearchTree searchTree)
  {
    var previousResults = searchTree.PreviousResults;
    var cancellationToken = searchTree.SqlState.CancellationToken;
    if (previousResults != null)
    {
      if (searchTree.SqlState.Connection is not SqlConnection sqlConnection)
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
      if (searchTree.SqlState.Connection is not SqlConnection sqlConnection)
      {
        throw new NullReferenceException(nameof(sqlConnection));
      }
      using var command = sqlConnection.CreateCommand();
      command.CommandText = SqlBuilder.DropPreviousResultsTable;
      await command.ExecuteNonQueryAsync(searchTree.SqlState.CancellationToken);
    }
  }

  public async Task<ResultSet> FindLinkedRecordSetAsync(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    if (searchTree.SqlState.Connection is not SqlConnection sqlConnection)
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
    using var reader = await sqlCommand.ExecuteReaderAsync(searchTree.SqlState.CancellationToken);
    while (await reader.ReadAsync())
    {
      result.Add(reader.GetInt32(0), searchTree.DatasetFilter, previous);
    }

    if (!searchTree.SortFields.Any())
    {
      result.Ids = [.. result.Ids.Distinct()];
      result.Ids.Sort();
    }
    result.Hits = result.Ids.Count;
    return result;
  }

  public async Task<ResultSet> FindFlatIndexedRecordSetAsync(IDbCommand command, SearchTree searchTree, SearchTreeLeaf leaf)
  {

    /// <summary>
    /// Convert a key according to index type, but pass through a single % (empty search)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="term"></param>
    /// <param name="dateCompletion"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static object ConvertKey(IndexTypeEnum type, string term, DateCompletionEnum dateCompletion)
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
          if (field.IsEnumeration)
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
      database.IsFullTextEnabled ?
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

  public async Task<AutoCompleteResult?> GetAutoCompleteAsync(IEnumerable<FieldData> fields,
                                                         DatasetFilter? datasetFilter,
                                                         string? value, int? startFrom, int? limit,
                                                         string? language,
                                                         bool count,
                                                         CancellationToken cancellationToken)
  {
    var firstField = fields.FirstOrDefault() ??
        throw new DDException("No field(s) requested for autocomplete");

    var databaseData = firstField.Database ??
        throw new DDException($"Database is null for field {firstField.Name}");

    using var connection = await GetDbConnectionAsync(databaseData) as SqlConnection ??
        throw new DDException("No database connection for autocomplete");

    var returnRows = startFrom + limit + 1; // return one more than the requested number of keys

    Dictionary<string, object> Parameters = [];

    string SaveParameter(string name, object value)
    {
      var parameterName = $"@{name}{Parameters.Count + 1}";
      Parameters[parameterName] = value;
      return parameterName;
    }

    IEnumerable<string> fieldSql()
    {
      var result = new List<string>();
      foreach (var field in fields)
      {
        var index = field.PreferredIndex ??
          throw new DDException($"No index for field {field}");

        var indexTableName = index.TableName;

        string Domain()
          => !string.IsNullOrWhiteSpace(field.LinkDomain) ?
            $"and domain = {SaveParameter("domain", field.LinkDomain)}" : "";
        
        result.Add(
       $"""
        select distinct value as term, [{indexTableName}].priref as priref from
          [people_fullText] 
          inner join [{indexTableName}] on [people_fullText].priref = [{indexTableName}].term
          where tag = 'BA' {Domain()} and contains(value, @term1)
          and ([{indexTableName}].priref between 1 and 25000000)) result
          group by term
       """
        );
      }
      return result;
    }

    var sql =
    $"""
      declare @term1 nvarchar(40) = 'vries*'
      select top {returnRows} term, [use] , sum(hits) as hits from (
      select distinct term, '' as [use], count(priref) as hits from
      (
        {string.Join(" union", fieldSql())}
      ) result
       group by term, [use] order by term
      """;

    async Task<AutoCompleteResult> GetResult()
    {
      var result = new AutoCompleteResult(startFrom ?? 1, limit ?? 10);
      using var command = connection.CreateCommand();
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

      // If we are doing autocomplete on a single enumerated field translate he keys to re requested value.
      if (fields.Count() == 1 && firstField.IsEnumeration && language != null)
      {
        foreach (var r in result)
        {
          if (r.Term is not null)
          {
            r.Term = firstField.GetLanguageEnumValue(r.Term, language);
          }
        }
        result.Sort();
      }
      return result;
    }

    return await GetResult();
  }


  public async Task<AutoCompleteResult?> GetAutoCompleteAsyncEx(IEnumerable<FieldData> fields, DatasetFilter? datasetFilter,
                                                         string? value, int? startFrom, int? limit, string? language, bool count,
                                                         CancellationToken cancellationToken)
  {
    var result = new AutoCompleteResult(startFrom ?? 1, limit ?? 10);

    var firstField = fields.FirstOrDefault() ??
      throw new DDException("No field(s) requested for autocomplete");
    var databaseData = firstField.Database!;

    using var connection = await GetDbConnectionAsync(databaseData) as SqlConnection
      ?? throw new DDException("No database connection for autocomplete");

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
          if (databaseData.IsFullTextEnabled)
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
            if (fieldData.LinkedDatabase!.IsFullTextEnabled)
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
          if (fieldData.IsEnumeration)
          {
            AutoCompleteEnum(command, sql, index.TableName, datasetFilter, fieldNumber, fieldData, value, language, count);
          }
          else
          {
            if (databaseData.IsFullTextEnabled)
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

    // If we are doing autocomplete on a single enumerated field translate he keys to re requested value.
    if (fields.Count() == 1 && firstField.IsEnumeration && language != null)
    {
      foreach (var r in result)
      {
        if (r.Term is not null)
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

  public async Task<int> GetNewRecordIdAsync(DatabaseData databaseData, DatasetData? datasetData, SqlStateInfo sqlState)
  {
    int min = datasetData != null ? datasetData.LowerLimit : 1;
    int max = datasetData != null ? datasetData.UpperLimit : int.MaxValue;
    using var command = (SqlCommand)sqlState.Connection!.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
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
                                      DateTime modification, string data, SqlStateInfo sqlState)
  {
    using var command = (SqlCommand)sqlState.Connection!.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = $"insert into [{table}] (priref, creation, modification, data)" +
                           "values (@priref, @creation, @modification, @data)";
    command.Parameters.AddRange(
      [
        new SqlParameter("priref", id),
        new SqlParameter("creation", creation),
        new SqlParameter("modification", modification),
        new SqlParameter("data", data)
      ]);
    await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task UpdateDataAsync(string table, int id, DateTime modification, string data, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection?)sqlState.Connection ?? throw new NullReferenceException(nameof(sqlState.Connection));
    var sqlTransaction = (SqlTransaction?)sqlState.Transaction ?? throw new NullReferenceException(nameof(sqlState.Transaction));
    using var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = $"update [{table}] set modification = @modification, data = @data where priref = @priref";
    command.Parameters.AddRange(
      [
        new SqlParameter("priref", id),
        new SqlParameter("modification", modification),
        new SqlParameter("data", data)
      ]);
    await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task<IDbTransaction> StartTransactionAsync(IDbConnection connection)
  {
    var sqlConnection = (SqlConnection)connection ?? throw new NullReferenceException(nameof(connection));
    return await sqlConnection.BeginTransactionAsync();
  }

  public async Task RollbackAsync(SqlStateInfo sqlState)
  {
    await ((SqlTransaction)sqlState.Transaction!).RollbackAsync(sqlState.CancellationToken);
    sqlState.Transaction = null;
  }

  public async Task CommitAsync(SqlStateInfo sqlState)
  {
    await ((SqlTransaction)sqlState.Transaction!).CommitAsync(sqlState.CancellationToken);
    sqlState.Transaction = null;
  }

  public async Task<int> AddIndexKeyAsync(IntegerIndexRow row, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    using var command = sqlConnection.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = SqlBuilder.InsertKey(row.Table);
    command.Parameters.AddRange(
      [
        new SqlParameter("key", row.Term),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task<int> DeleteIndexKeyAsync(IntegerIndexRow row, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    using var command = sqlConnection.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = SqlBuilder.DeleteKey(row.Table);
    command.Parameters.AddRange(
      [
        new SqlParameter("key", row.Term),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task<int> DeleteIndexKeyAsync(string? fullTextTable, TermIndexRow row, SqlStateInfo sqlState)
  {
    int deletes = 0;
    if (fullTextTable is not null)
    {
      deletes += await UpdateIndexTableAsync(SqlBuilder.DeleteFullTextKey(fullTextTable, row.Domain), row, sqlState);
      IndexData indexData = row.Index;
      if (indexData.TableExists.HasValue && indexData.TableExists.Value)
      {
        deletes += await UpdateIndexTableAsync(SqlBuilder.DeleteTextKey(row.Table, row.Domain), row, sqlState);
      }
    }
    else
    {
      deletes += await UpdateIndexTableAsync(SqlBuilder.DeleteTextKey(row.Table, row.Domain), row, sqlState);
    }
    return deletes;
  }

  public async Task<int> AddIndexKeyAsync(string? fullTextTable, TermIndexRow row, SqlStateInfo sqlState)
  {
    int inserts = 0;
    if (fullTextTable is not null)
    {
      inserts += await UpdateIndexTableAsync(SqlBuilder.InsertFullTextKey(fullTextTable), row, sqlState);
      IndexData indexData = row.Index;
      if (indexData.TableExists.HasValue && indexData.TableExists.Value)
      {
        inserts += await UpdateIndexTableAsync(SqlBuilder.InsertTextKey(row.Table, row.Domain), row, sqlState);
      }
    }
    else
    {
      inserts += await UpdateIndexTableAsync(SqlBuilder.InsertTextKey(row.Table, row.Domain), row, sqlState);
    }
    return inserts;
  }

  public Task<int> AddIndexKeyAsync(DateIndexRow row, SqlStateInfo sqlState)
    => UpdateDateIndexTableAsync(SqlBuilder.InsertDateKey(row.Table), row, sqlState);

  public Task<int> AddIndexKeyAsync(BooleanIndexRow row, SqlStateInfo sqlState)
    => UpdateBooleanIndexTableAsync(SqlBuilder.InsertBooleanKey(row.Table), row, sqlState);

  public Task<int> DeleteIndexKeyAsync(DateIndexRow row, SqlStateInfo sqlState)
    => UpdateDateIndexTableAsync(SqlBuilder.DeleteDateKey(row.Table), row, sqlState);

  public Task<int> AddIndexKeyAsync(IsoDateIndexRow row, SqlStateInfo sqlState)
   => UpdateDateIndexTableAsync(SqlBuilder.InsertIsoDateKey(row.Table), row, sqlState);

  public Task<int> DeleteIndexKeyAsync(IsoDateIndexRow row, SqlStateInfo sqlState)
    => UpdateDateIndexTableAsync(SqlBuilder.DeleteIsoDateKey(row.Table), row, sqlState);

  public Task<int> DeleteIndexKeyAsync(BooleanIndexRow row, SqlStateInfo sqlState)
    => UpdateBooleanIndexTableAsync(SqlBuilder.DeleteBooleanKey(row.Table), row, sqlState);

  private static async Task<int> UpdateDateIndexTableAsync(string commandText, DateIndexRow row, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    using var command = sqlConnection.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("term", row.Term),
        new SqlParameter("displayTerm", row.DisplayTerm),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  private static async Task<int> UpdateBooleanIndexTableAsync(string commandText, BooleanIndexRow row, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    using var command = sqlConnection.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
    command.CommandText = commandText;
    command.Parameters.AddRange(
      [
        new SqlParameter("term", row.Term),
        new SqlParameter("displayTerm", row.DisplayTerm),
        new SqlParameter("id", row.Id)
      ]);
    return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  private static async Task<int> UpdateIndexTableAsync(string commandText, TermIndexRow row, SqlStateInfo sqlState)
  {
    var connection = (SqlConnection)sqlState.Connection!;
    using var command = connection.CreateCommand();
    command.Transaction = (SqlTransaction)sqlState.Transaction!;
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
    return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task<List<int>> ReadLinkedRecordIdsAsync(string tableName, int id, SqlStateInfo sqlState)
  {
    var result = new List<int>();
    var connection = (SqlConnection)sqlState.Connection!;
    var transaction = (SqlTransaction)sqlState.Transaction!;

    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.Parameters.Add(new SqlParameter("id", id));
    command.CommandText = SqlBuilder.ReadLinkedRecords(tableName);
    try
    {
      using var reader = await command.ExecuteReaderAsync(sqlState.CancellationToken);
      while (await reader.ReadAsync(sqlState.CancellationToken))
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

  public async Task<int> DeleteIndexKeysAsync(string tableName, int id, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    var sqlTransaction = (SqlTransaction)sqlState.Transaction!;
    using var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.Parameters.Add(new SqlParameter("id", id));
    command.CommandText = SqlBuilder.DeleteIndexKeys(tableName);
    return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
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
  public async Task<int> AddIndexKeyAsync(string? fullTextTable, AlphaNumericIndexRow row, SqlStateInfo sqlState)
  {
    int inserts = await UpdateIndexTableAsync(SqlBuilder.InsertAlphanumericKey(row.Table), row, sqlState);
    if (fullTextTable != null)
    {
      if (row.Index != null && row.Tag != null && row.DisplayTerm != null)
      {
        var indexRow = new TermIndexRow(row.Index, row.Tag, row.Occ, row.DisplayTerm, row.Id);
        inserts += await UpdateIndexTableAsync(SqlBuilder.InsertFullTextKey(fullTextTable), indexRow, sqlState);
      }
    }
    return inserts;
  }

  private static async Task<int> UpdateIndexTableAsync(string commandText, IndexRow indexRow, SqlStateInfo sqlState)
  {
    try
    {
      using var command = ((SqlConnection)sqlState.Connection!).CreateCommand();
      command.Transaction = (SqlTransaction)sqlState.Transaction!;
      command.CommandText = commandText;
      command.Parameters.AddRange(
        [
          new SqlParameter("tag", indexRow.Tag),
        new SqlParameter("occ", indexRow.Occ),
        new SqlParameter("key", indexRow.Term),
        new SqlParameter("displayTerm", indexRow.DisplayTerm),
        new SqlParameter("id", indexRow.Id)
        ]);
      return await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public async Task<int> DeleteIndexKeyAsync(string? fullTextTable, AlphaNumericIndexRow alphaNumericRow, SqlStateInfo sqlState)
  {
    int deletes = await UpdateIndexTableAsync(SqlBuilder.DeleteAlphaNumericKey(alphaNumericRow.Table), alphaNumericRow, sqlState);
    if (fullTextTable is not null)
    {
      deletes += await UpdateIndexTableAsync(SqlBuilder.DeleteFullTextKey(fullTextTable), alphaNumericRow, sqlState);
    }
    return deletes;
  }

  public async Task<int> WriteRecordSetMetaDataAsync(RecordSetMetaData metaData, SqlStateInfo sqlState)
  {
    static async Task<bool> CheckIfRecordSetExists(string database, int number, SqlStateInfo sqlState)
    {
      var sqlConnection = (SqlConnection)sqlState.Connection! ?? throw new NullReferenceException(nameof(sqlState.Connection));
      var sqlTransaction = (SqlTransaction)sqlState.Transaction! ?? throw new NullReferenceException(nameof(sqlState.Transaction));
      var command = sqlConnection.CreateCommand();
      command.Transaction = sqlTransaction;
      command.CommandText = SqlBuilder.ExistsSetNumber(database);
      command.Parameters.AddWithValue("number", number);
      return (int)await command.ExecuteScalarAsync(sqlState.CancellationToken) == 1;
    }

    static async Task<int> GetNewRecordSetNumber(string database, SqlStateInfo sqlState)
    {
      var sqlConnection = (SqlConnection)sqlState.Connection! ?? throw new NullReferenceException(nameof(sqlState.Connection));
      var sqlTransaction = (SqlTransaction)sqlState.Transaction! ?? throw new NullReferenceException(nameof(sqlState.Transaction));
      var command = sqlConnection.CreateCommand();
      command.Transaction = sqlTransaction;
      command.CommandText = SqlBuilder.GetNewSetNumber(database);
      return (int)await command.ExecuteScalarAsync(sqlState.CancellationToken);
    }

    static async Task WriteRecordSetMetaDataAsync(RecordSetMetaData metaData, string sql, SqlStateInfo sqlState)
    {
      var sqlConnection = (SqlConnection)sqlState.Connection! ?? throw new NullReferenceException(nameof(sqlState.Connection));
      var sqlTransaction = (SqlTransaction)sqlState.Transaction! ?? throw new NullReferenceException(nameof(sqlState.Transaction));
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

      await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
    }

    string sql;
    if (metaData.Number == 0 || !await CheckIfRecordSetExists(metaData.Database!, metaData.Number, sqlState))
    {
      metaData.Number = await GetNewRecordSetNumber(metaData.Database!, sqlState);
      sql = SqlBuilder.CreateRecordSet(metaData.Database!);
    }
    else
    {
      sql = SqlBuilder.UpdateRecordSet(metaData.Database!);
    }
    await WriteRecordSetMetaDataAsync(metaData, sql, sqlState);
    return metaData.Number;
  }

  public static Task ClearRecordSetHitsAsync(string database, int setNo, SqlStateInfo sqlState)
    => Clear(SqlBuilder.ClearRecordSetHits(database), setNo, sqlState);

  public static Task ClearRecordSetAccessAsync(string database, int setNo, SqlStateInfo sqlState)
   => Clear(SqlBuilder.ClearRecordSetAccess(database), setNo, sqlState);

  public static Task ClearRecordSetEmailsAsync(string database, int setNo, SqlStateInfo sqlState)
  => Clear(SqlBuilder.ClearRecordSetEmails(database), setNo, sqlState);

  private static Task ClearRecordSetMetaData(string database, int setNo, SqlStateInfo sqlState)
  => Clear(SqlBuilder.ClearRecordSetMetaData(database), setNo, sqlState);

  private static async Task Clear(string commandText, int number, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    var sqlTransaction = (SqlTransaction)sqlState.Transaction!;
    var command = sqlConnection.CreateCommand();
    command.Transaction = sqlTransaction;
    command.CommandText = commandText;
    command.Parameters.AddWithValue("number", number);
    await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task<int> WriteRecordSetAsync(string folder, RecordSetMetaData metaData, ResultSet set, CancellationToken cancellationToken)
  {
    static async Task WriteHitsAsync(RecordSetMetaData metaData, ResultSet set, SqlStateInfo sqlState)
    {
      var sqlConnection = (SqlConnection)sqlState.Connection!;
      var sqlTransaction = (SqlTransaction)sqlState.Transaction!;
      var command = sqlConnection.CreateCommand();
      command.Transaction = sqlTransaction;
      command.CommandText = SqlBuilder.WriteRecordSetHits(metaData.Database);
      foreach (var id in set.Ids)
      {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("number", metaData.Number);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
      }
    }

    var setNo = metaData.Number;
    var databaseData = MetaDataCache.ReadDatabase(folder, metaData.Database!) ?? throw new DatabaseNotFoundException(folder, metaData.Database);
    var connection = await GetDbConnectionAsync(databaseData);
    var transaction = await StartTransactionAsync(connection);

    var sqlState = new SqlStateInfo
    {
      Connection = connection,
      Transaction = transaction,
      CancellationToken = cancellationToken
    };

    try
    {
      metaData.Hits = set.Ids.Count;
      var table = databaseData.Name!;
      int number = await WriteRecordSetMetaDataAsync(metaData, sqlState);
      await ClearRecordSetHitsAsync(table, setNo, sqlState);
      await ClearRecordSetAccessAsync(table, setNo, sqlState);
      await ClearRecordSetEmailsAsync(table, setNo, sqlState);
      await WriteHitsAsync(metaData, set, sqlState);
      await CommitAsync(sqlState);
      return number;
    }
    catch
    {
      await RollbackAsync(sqlState);
      throw;
    }
  }

  public async Task DeleteRecordSetAsync(string folder, string database, int setNo, CancellationToken cancellationToken)
  {
    var databaseData = MetaDataCache.ReadDatabase(folder, database) ?? throw new DatabaseNotFoundException(folder, database);
    var connection = await GetDbConnectionAsync(databaseData);
    var transaction = await StartTransactionAsync(connection);

    var sqlState = new SqlStateInfo
    {
      Connection = connection,
      Transaction = transaction,
      CancellationToken = cancellationToken
    };

    try
    {
      await ClearRecordSetMetaData(database, setNo, sqlState);
      await ClearRecordSetHitsAsync(database, setNo, sqlState);
      await ClearRecordSetAccessAsync(database, setNo, sqlState);
      await ClearRecordSetEmailsAsync(database, setNo, sqlState);
      await CommitAsync(sqlState);
    }
    catch
    {
      await RollbackAsync(sqlState);
    }
  }


  private static async Task<List<HierarchyNode>> GetChildren(SqlConnection connection, string partOfTable, int id, string nameTable,
                                                             string[] parts, int level)
  {
    var key = level < parts.Length ? parts[level] : "";

    using var command = connection.CreateCommand();

    command.CommandText = $"""
          select [{partOfTable}].[priref], [{nameTable}].[displayterm] from [{partOfTable}] inner join [{nameTable}] on [{partOfTable}].[priref] = [{nameTable}].[priref]
          where [{partOfTable}].[term] = @id
          """;

    command.Parameters.AddWithValue("id", id);

    if (key != "")
    {
      command.CommandText += $" and [{nameTable}].[term] = @term";
      command.Parameters.AddWithValue("term", parts[level]);
    }

    var children = new List<HierarchyNode>();
    using (var reader = await command.ExecuteReaderAsync())
    {
      while (await reader.ReadAsync())
      {
        var childKey = reader.GetString(1);
        children.Add(new HierarchyNode
        {
          Id = reader.GetInt32(0),
          Key = reader.GetString(1)
        });
      }
    }

    foreach (var child in children)
    {
      child.Children = await GetChildren(connection, partOfTable, child.Id, nameTable, parts, level + 1);
    }
    return children;
  }

  public Task<IEnumerable<int>> SearchLinksAsync(DatabaseData database, DatasetData? dataset, FieldData field, string searchValue,
                                                 string domain, SearchLimits limits, SqlStateInfo sqlState)
      => SearchLinksDomainAsync(database, dataset, field.LinkedFieldData!, searchValue, domain, limits, sqlState);

  private async Task<IEnumerable<int>> SearchLinksDomainAsync(DatabaseData linkedDatabase, DatasetData? linkedDataset, FieldData linkedField,
                                       string searchValue, string domain, SearchLimits limits, SqlStateInfo sqlState)
  {
    var table = linkedDatabase.IsFullTextEnabled ? linkedDatabase.FullTextTable : linkedField.GetIndexTableName();

    using var connection = (SqlConnection)await GetDbConnectionAsync(linkedDatabase);
    using var command = connection.CreateCommand();
    command.CommandText = $"select priref from {table} where term like @term ";

    if (linkedDatabase.IsFullTextEnabled)
    {
      command.CommandText += " and tag = @tag";
      command.Parameters.AddWithValue("tag", linkedField.Tag);
    }

    if (!string.IsNullOrEmpty(domain))
    {
      command.CommandText += " and domain = @domain";
      command.Parameters.AddWithValue("domain", domain);
    }
    if (linkedDataset != null)
    {
      command.CommandText += " and priref between @lowerLimit and @upperLimit";
      command.Parameters.AddWithValue("lowerLimit", linkedDataset.LowerLimit);
      command.Parameters.AddWithValue("upperLimit", linkedDataset.UpperLimit);
    }

    command.Parameters.AddWithValue("term", searchValue + "%");
    using var reader = await command.ExecuteReaderAsync(sqlState.CancellationToken);
    var ids = new List<int>();
    int count = 0;
    while (await reader.ReadAsync())
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

  public async Task<string> GetAutoNumberValue(FieldData fieldData, SqlStateInfo sqlState)
  {
    var connection = (SqlConnection)sqlState.Connection!;
    var transaction = (SqlTransaction)sqlState.Transaction!;
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = SqlBuilder.GetAutoNumberValue();
    command.Parameters.AddWithValue("databaseName", fieldData.Database!.Name!);
    command.Parameters.AddWithValue("tag", fieldData.Tag);
    command.Parameters.AddWithValue("delta", fieldData.AutoNumberIncrement);
    command.Parameters.AddWithValue("minimum", fieldData.AutoNumberStartValue);
    command.Parameters.AddWithValue("prefix", fieldData.AutoNumberPrefix);
    command.Parameters.AddWithValue("suffix", fieldData.AutoNumberSuffix);
    var result = await command.ExecuteScalarAsync(sqlState.CancellationToken);
    if (result == null || result == DBNull.Value)
    {
      throw new DDException($"Could not retrieve auto number value for field '{fieldData.Tag}' in database '{fieldData.Database.Name}'");
    }
    return result.ToString()
      ?? throw new DDException($"Could not convert auto number value for field '{fieldData.Tag}' in database '{fieldData.Database.Name}' to string");
  }

  public async Task<RecordSetMetaData?> GetResultSetMetaDataAsync(DatabaseData database, int set,
    CancellationToken cancellationToken)
  {
    var table = database.Name!;
    var connection = await GetDbConnectionAsync(database);

    using var command = connection.CreateCommand() as SqlCommand;
    command!.CommandText = SqlBuilder.GetSetMetaData(table);
    command.Parameters.AddWithValue("number", set);
    using var reader = await command.ExecuteReaderAsync(cancellationToken);
    var metadata = (await reader.ReadAsync(cancellationToken)) ? new RecordSetMetaData(table, reader) : null;

    connection.Dispose();
    return metadata;
  }

  public Task AddToRecordSetAsync(RecordSet recordSet, int id, SqlStateInfo sqlState)
   => UpdateHitAsync(SqlBuilder.AddToRecordSet(recordSet.Database.Name!), recordSet, id, sqlState);

  public Task RemoveFromRecordSetAsync(RecordSet recordSet, int id, SqlStateInfo sqlState)
    => UpdateHitAsync(SqlBuilder.RemoveFromRecordSet(recordSet.Database.Name!), recordSet, id, sqlState);

  private static async Task UpdateHitAsync(string sql, RecordSet recordSet, int id, SqlStateInfo sqlState)
  {
    var sqlConnection = (SqlConnection)sqlState.Connection!;
    var sqlTransaction = (SqlTransaction)sqlState.Transaction!;

    using var command = sqlConnection.CreateCommand();
    command!.Transaction = sqlTransaction;
    command!.CommandText = sql;
    command.Parameters.AddWithValue("id", id);
    command.Parameters.AddWithValue("number", recordSet.MetaData.Number);
    await command.ExecuteNonQueryAsync(sqlState.CancellationToken);
  }

  public async Task<ResultSet> RunSqlSearch(DatabaseData databaseData, string sql, Dictionary<string, object> parameters, SqlStateInfo sqlState)
  {
    using var sqlConnection = (await GetDbConnectionAsync(databaseData) as SqlConnection)!;
    var sqlTransaction = (SqlTransaction)sqlState.Transaction!;

    using var command = sqlConnection.CreateCommand();
    command!.Transaction = sqlTransaction;
    command!.CommandText = sql;

    foreach (var (key, value) in parameters)
    {
      command.Parameters.AddWithValue(key, value);
    }

    var result = new ResultSet();
    using var reader = await command.ExecuteReaderAsync(sqlState.CancellationToken);
    while (await reader.ReadAsync())
    {
      result.AddId(reader.GetInt32(0));
      if (result.Hits == 0 && reader.FieldCount > 1)
      {
        result.Hits = reader.GetInt32(1);
      }
      if (reader.FieldCount > 2)
      {
        result.AddKey(reader[2]);
      }
    }
    return result;
  }

  public bool CheckIndexTable(DatabaseData databaseData, string tableName)
  {
    using var sqlConnection = (GetDbConnection(databaseData) as SqlConnection)!;
    using var command = sqlConnection.CreateCommand();
    command.CommandText = "select count(table_name) from [information_schema].[tables] where [table_name] = @tableName";
    command.Parameters.AddWithValue("@tableName", tableName);
    var result = command.ExecuteScalar();
    return (int)result! is 1;
  }
}



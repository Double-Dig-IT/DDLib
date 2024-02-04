
using Azure;
using DDigit.MetaData;

namespace DDigit.Repository;

internal class SqlBuilder
{
  internal static string Search(string tableName, SearchOperators searchOperator, ref string? value, ResultSet? previousResults)
  {
    var sql = new StringBuilder();
    sql.Append($"select distinct priref from [{tableName}]");

    if (!string.IsNullOrWhiteSpace(value))
    {
      sql.Append(" where term");
      if (Truncate(value))
      {
        value = ReplaceStar(value);
        sql.Append(" like ");
      }
      else
      {
        sql.Append($" {SqlOperator(searchOperator)} ");
      }
      sql.Append($"@term0");

      if (previousResults != null)
      {
        sql.Append($" intersect select id from {previousTable}");
      }
      sql.Append(" order by priref");
    }
    return sql.ToString();
  }

  internal static string EnumSearch(string tableName, string[] searchWords, ResultSet? previousResults)
  {
    var sql = new StringBuilder();
    for (int i = 0; i < searchWords.Length; i++)
    { 
      if (i > 0)
      {
        sql.Append(" union ");
      }
      sql.Append($"select distinct priref from [{tableName}] where term = @term{i}");
    }

    if (previousResults != null)
    {
      sql.Append($" intersect select id from {previousTable}");
    }
    sql.Append(" order by priref");

    return sql.ToString();
  }

  internal static string EnumSearchFullText(string tableName, string tag, string[] searchWords, ResultSet? previousResults)
  {
    var sql = new StringBuilder();
    for (int i = 0; i < searchWords.Length; i++)
    {
      if (i > 0)
      {
        sql.Append(" union ");
      }
      sql.Append($"select distinct priref from [{tableName}] where contains(value, @term{i}) and tag = '{tag}'");
    }

    if (previousResults != null)
    {
      sql.Append($" intersect select id from {previousTable}");
    }
    sql.Append(" order by priref");

    return sql.ToString();
  }

  private static string SqlOperator(SearchOperators searchOperator)
    => searchOperator switch
    {
      SearchOperators.Equals => "=",
      SearchOperators.Greater => ">",
      SearchOperators.Smaller => "<",
      SearchOperators.GreaterOrEquals => ">=",
      SearchOperators.SmallerOrEquals => "<=",
      _ => throw new DDException($"Sql does not support {searchOperator}"),
    };

  internal static string LinkedFieldSearch(FieldData fieldData, ResultSet? previousResults)
  {
    var index = fieldData.PreferredIndex ?? throw new NullReferenceException(nameof(fieldData.PreferredIndex));
    var localTable = index.TableName ?? throw new NullReferenceException(nameof(index.TableName));
    var linkedDatabase = fieldData.LinkedDatabase ?? throw new NullReferenceException(nameof(fieldData.LinkedDatabase));
    var lookupTag = fieldData.LinkIndexTag ?? throw new NullReferenceException(nameof(fieldData.LinkIndexTag));
    var linkedFieldData = linkedDatabase.FindFieldByTagOrName(lookupTag) ?? throw new NullReferenceException("linkedFieldData");
    var remoteIndex = linkedFieldData.PreferredIndex ?? throw new NullReferenceException(nameof(linkedFieldData.PreferredIndex));
    var remoteTable = remoteIndex.TableName ?? throw new NullReferenceException(nameof(remoteIndex.TableName));

    var sql = new StringBuilder();

    sql.Append($"select [{localTable}].priref from [{localTable}] ");

    if (linkedDatabase.FullText)
    {
      sql.Append($"where term in (select [{linkedDatabase.FullTextTable}].priref from [{linkedDatabase.FullTextTable}] where Contains(value, @term0) and tag = '{linkedFieldData.Tag}')");
    }
    else
    {
      sql.Append($"where term in (select [{remoteTable}].priref from [{remoteTable}] where [{remoteTable}].term like @term0)");
    }

    if (previousResults != null)
    {
      sql.Append($" intersect select id from {previousTable}");
    }

    sql.Append($" order by [{localTable}].priref");
    return sql.ToString();
  }

  internal static string FreeTextSearch(string tableName, string[] words, ResultSet? previousResults)
  {
    var sql = new StringBuilder();
    for (int i = 0; i < words.Length; i++)
    {
      words[i] = ReplaceStar(words[i])!;
      sql.Append(i == 0 ? WordQuery(tableName, i) : $" and priref in ({WordQuery(tableName, i)})");
    }
    if (previousResults != null)
    {
      sql.Append($" intersect select id from {previousTable}");
    }
    sql.Append(" order by id");
    return sql.ToString();
  }

  internal static string FullTextSearch(DatabaseData databaseData, string? tag, ResultSet? previousResults)
  {
    var sql = new StringBuilder();

    sql.Append($"select priref as id from [{databaseData.FullTextTable}] where contains(value, @term0)");
    
    if (tag != null)
    {
      sql.Append(" and tag = '{tag}'");
    }

    if (previousResults != null)
    {
      sql.Append($" intersect select id from {previousTable}");
    }
    sql.Append(" order by id");
    return sql.ToString();
  }

  private static string WordQuery(string tableName, int i)
    => $"select [{tableName}].priref as id from wordlist inner join [{tableName}] on wordlist.wordNumber = [{tableName}].term " +
       $"where wordlist.term like @term{i}";

  public static string? ReplaceStar(string? text) => text != null && text.Contains('*') ?
    text.Replace('*', '%') : text;

  private static bool Truncate(string? text) => text != null && text.Contains('*');

  internal static string SelectFromHitList(string name) =>
      $"select priref from {PointerFileHitListTable(name)} where pfNumber = @set";

  private const string previousTable = "#previous";

  internal static string DropPreviousResultsTable => $"drop table [{previousTable}]";

  internal static string CreatePreviousResultsTable => $"create table [{previousTable}] (id int)";

  internal static string CreatePreviousResultsIndex => $"create index id on [{previousTable}] (id)";

  internal static string AddToPreviousResults => $"insert into [{previousTable}] (id) values (@id)";

  internal static string SelectAllRecordLocks => "select * from recordLocks";

  internal static string GetWordNumber =>
    $"select wordNumber from [wordlist] where term = @term and language = @language";

  internal static string SelectHighestWordNumber => "select max(wordNumber) from [wordlist]";

  internal static string AddWord => "insert into wordlist (term, displayTerm, language, wordNumber) " +
                          "values (@term, @displayTerm, @language, @wordNumber)";

  internal static string InsertKey(string table) => 
    $"insert into [{table}] (term, priref) values (@key, @id)";

  internal static string InsertTextKey(string table, string? domain) =>
    domain != null ?
    $"insert into [{table}] (term, displayTerm, language, domain, priref) values (@key, @displayTerm, @language, @domain, @id)" :
    $"insert into [{table}] (term, displayTerm, language, priref) values (@key, @displayTerm, @language, @id)";


  internal static string DeleteKey(string table) => 
    $"delete from [{table}] where term = @key and priref = @id";

  internal static string DeleteTextKey(string table, string? domain) =>
    domain != null ?
   $"delete from [{table}] where term = @key and priref = @id and domain = @domain and " +
    "language = @language and displayTerm = @displayTerm" :
    $"delete from [{table}] where term = @key and priref = @id and " +
    "language = @language and displayTerm = @displayTerm";

  private static string PointerFileTable(string table) => $"{table}_pointerFiles2";

  private static string PointerFileHitListTable(string table) => $"{table}_pointerFiles2_hitlist";

  internal static string SelectData(string table) => $"select data from {table} where priref = @priref";

  internal static string GetPointerFileData(string table) => $"select * from {PointerFileTable(table)}";

  internal static string ReadAllRecords(DatabaseData databaseData, ResultSet? previousResults) =>
    previousResults != null ?
    $"select id from [{previousTable}] order by id" :
    $"select priref from [{databaseData.Name}] order by priref";

  internal static string IdSearch(string indexTable, SearchOperators searchOperator)
  => $"select priref from {indexTable} where priref {SqlOperator(searchOperator)} @term0";

  internal static string FlatAutoComplete(string tableName, DatasetFilter? datasetFilter, int field)
  {
    var result = new StringBuilder();
    result.Append($"select distinct displayTerm as term, '' as [use], count(displayTerm) as hits from [{tableName}] where term like @term{field}");
    AddDatasets(result, datasetFilter, tableName);
    result.Append(" group by displayTerm");
    return result.ToString();
  }

  internal static string? EnumAutoComplete(string tableName, DatasetFilter? datasetFilter, int fieldNumber, IEnumerable<string?> keys, bool count)
  {
    var result = new StringBuilder();
    int i = 0;
    if (count)
    {
      foreach (var key in keys)
      {
        if (key != null)
        {
          if (i > 0)
          {
            result.Append(" union ");
          }
          result.Append($"select distinct term, priref, '' as [use], 1 as hits from [{tableName}] where ");
          result.Append($"term = @term{fieldNumber}key{i++} group by term, priref");
        }
      }
    }
    else
    {
      result.Append("select * from (values ");
      foreach (var key in keys)
      {
        if (key != null)
        {
          if (i > 0)
          {
            result.Append(", ");
          }
          result.Append($"('{key}', '', 0)");
          i++;
        }
      }
      result.Append(") data(term, [use], hits)");
    }
      
    AddDatasets(result, datasetFilter, tableName);
    return result.ToString();
  }

  internal static string IntegerAutoComplete(string tableName, DatasetFilter? datasetFilter, int field)
  {
    var result = new StringBuilder();
    result.Append($"select distinct term, '' as [use], count(term) as hits from [{tableName}] where term > @term{field}");
    AddDatasets(result, datasetFilter, tableName);
    result.Append(" group by term");
    return result.ToString();
  }

  private static void AddDatasets(StringBuilder result, DatasetFilter? datasetFilter, string table)
  {
    if (datasetFilter != null && datasetFilter.Count > 0)
    {
      result.Append(" and (");
      var first = true;
      foreach (var dataset in datasetFilter.Values)
      {
        if (!first)
        {
          result.Append(" or ");
        }
        result.Append($"[{table}].priref between {dataset.LowerLimit} and {dataset.UpperLimit}");
        first = false;
      }
      result.Append(')');
    }
  }

  internal static string FreeTextAutoComplete(string tableName, DatasetFilter? datasetFilter, int field)
  {
    var result = new StringBuilder();
    result.Append($"select distinct displayTerm as term, '' as [use], count(displayTerm) as hits from wordlist inner join [{tableName}]");
    result.Append($" on [{tableName}].term = wordlist.wordNumber where wordlist.term like @term{field}");
    AddDatasets(result, datasetFilter, tableName);
    result.Append(" group by displayTerm");
    return result.ToString();
  }

  internal static string? FullTextAutoComplete(string tableName, string? tag, DatasetFilter? datasetFilter, int field)
  {
    var result = new StringBuilder();
    result.Append($"select distinct term, '' as [use], count(term) as hits from [{tableName}]");
    result.Append($" where contains(value, @term{field})");
    if (tag != null)
    {
      result.Append(" and tag = '{tag}'");
    }
    AddDatasets(result, datasetFilter, tableName);
    result.Append(" group by term");
    return result.ToString();
  }

  internal static string? FullTextEmptyAutoComplete(string tableName, string? tag, DatasetFilter? datasetFilter, int fieldNumber)
  {
    var result = new StringBuilder();
    result.Append($"select distinct term, '' as [use], count(term) as hits from [{tableName}]");
    result.Append($" where tag = '{tag}'");
    AddDatasets(result, datasetFilter, tableName);
    result.Append(" group by term");
    return result.ToString();
  }

  internal static string LinkedAutocomplete(FieldData fieldData, DatasetFilter? datasetFilter, int field, string? value)
  {
    var result = new StringBuilder();

    var linkedFieldData = fieldData.LinkedFieldData ?? throw new NullReferenceException(nameof(fieldData.LinkedFieldData));
    var linkedDatabase = linkedFieldData.Database ?? throw new NullReferenceException(nameof(linkedFieldData.Database));
    var localIndexTable = fieldData.PreferredIndex!.TableName;
    var remoteIndexTable = linkedFieldData.PreferredIndex!.TableName;
    var nonPreferredTable = fieldData.UseFieldData?.PreferredIndex?.TableName;

    if (linkedDatabase.FullText)
    {
      var table = linkedDatabase.FullTextTable;
      result.Append($"select distinct term, '' as [use], count(priref) as hits from ");
      result.Append($"(select distinct value as term, [{localIndexTable}].priref as priref from [{table}] ");
      result.Append($"inner join [{localIndexTable}] on [{table}].priref = [{localIndexTable}].term where ");
      if (!string.IsNullOrWhiteSpace(value))
      {
        result.Append($"contains(value, @term{field}) and "); 
      }
      result.Append($"tag = '{linkedFieldData.Tag}'");
      AddDomains(result, fieldData);
      AddDatasets(result, datasetFilter, localIndexTable);
      result.Append($") result group by term");
    }
    else
    {
      result.Append($"select distinct displayTerm as term, '' as [use], count(displayTerm) as hits from [{remoteIndexTable}] ");
      result.Append($"inner join [{localIndexTable}] on [{remoteIndexTable}].priref = [{localIndexTable}].term where [{remoteIndexTable}].term like @term{field}");
      if (!string.IsNullOrEmpty(fieldData.LinkDomain))
      {
        result.Append($" and domain = @domain{field}");
      }
      AddDatasets(result, datasetFilter, localIndexTable);
      result.Append(" group by displayTerm");

      if (!string.IsNullOrEmpty(nonPreferredTable))
      {
        result.Append(" union ");
        result.Append($"select term.displayTerm as term, useTerm.displayTerm as [use], 0 as hits from [{nonPreferredTable}] inner join [{remoteIndexTable}]");
        result.Append($" as term on term.priref = [{nonPreferredTable}].priref inner join [{remoteIndexTable}] as useTerm on useTerm.priref = [{nonPreferredTable}].term");
        result.Append($" where term.term like @term{field}");
        if (!string.IsNullOrEmpty(fieldData.LinkDomain))
        {
          result.Append($" and term.domain = @domain{field}");
        }
      }
    }

    return result.ToString();
  }

  private static void AddDomains(StringBuilder result, FieldData fieldData)
  {
    var domains = GetDomains(fieldData);
    if (domains.Count > 0)
    {
      result.Append($" and domain in ({string.Join(',', domains.Select(domain => $"'{domain}'"))})");
    }
  }

  internal static string FindLink(string table)
    => $"select priref from [{table}] where term = @term and domain = @domain and language = @language";

  private static List<string>GetDomains(FieldData fieldData)
  {
    var domains = new List<string>();
    if (!string.IsNullOrEmpty(fieldData.LinkDomain))
    {
      domains.Add(fieldData.LinkDomain);
    }
    else
    {
      if (!string.IsNullOrWhiteSpace(fieldData.LinkDomainTag))
      {
        var tag = fieldData.LinkDomainTag;
        var databaseData = fieldData.Database ?? throw new NullReferenceException(nameof(fieldData.Database));
        var domainFieldData = databaseData.FindFieldByTagOrName(tag) ?? throw new FieldNotFoundException(tag, databaseData.Name);
        if (domainFieldData.IsEnumeration.HasValue && domainFieldData.IsEnumeration.Value)
        {
          domains.AddRange(domainFieldData.EnumKeys("", null));
        }
      }
    }
    return domains;
  }

  
}

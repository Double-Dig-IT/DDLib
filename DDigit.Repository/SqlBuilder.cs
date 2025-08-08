namespace DDigit.Repository;

internal class SqlBuilder
{
  private const int MaxWordLength = 32;

  internal static string Search(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));
    var sql = new StringBuilder();
    sql.Append($"select priref as id from [{index.TableName}]");
    // For or some clever reason Axiell decided to use the displayterm column for alphanumeric searches.
    // This bug fix compensates for this.
    AddWhereClause(sql, leaf, index.Type == IndexTypeEnum.AlphaNumeric ? "displayTerm" : "term");
    AddPreviousResult(sql, searchTree.PreviousResults);
    sql.Append(" order by id");
    return sql.ToString();
  }

  private static void AddColumToWhereClause(StringBuilder sql, int i, string column, SearchOperatorEnum op, string term)
  {
    sql.Append($"{column}");
    if (term == "*")
    {
      sql.Append(" is not null ");
    }
    else
    {
      sql.Append(SqlSearchOperator(op, term));
      sql.Append($"@term{i}");
    }
  }

  private static void AddWhereClause(StringBuilder sql, SearchTreeLeaf leaf, string column)
  {
    int i = 0;
    foreach (var value in leaf.Values)
    {
      var term = value.ToString();
      if (!string.IsNullOrWhiteSpace(term))
      {
        sql.Append(i == 0 ? " where " : " or ");
        AddColumToWhereClause(sql, i++, column, leaf.Operator, term);
      }
    }
  }

  private static void AddWhereClauseForFullTextIndex(StringBuilder sql, SearchTreeLeaf leaf, string column, string? tag)
  {
    int i = 0;
    foreach (var value in leaf.Values)
    {
      var term = value.ToString();
      if (!string.IsNullOrWhiteSpace(term))
      {
        sql.Append(i == 0 ? " where (" : " or ");
        AddColumToWhereClause(sql, i++, column, leaf.Operator, term);
      }
    }
    sql.Append(')');
    // if the tag is not null, we add it to the where clause
    sql.Append($" and tag = '{tag}'");
  }

  private static void AddWhereClause(StringBuilder sql, SearchTreeLeaf leaf, string column, bool fullText, string? tag)
  {
    if (fullText)
    {
      AddWhereClauseForFullTextIndex(sql, leaf, column, tag);
    }
    else
    {
      AddWhereClause(sql, leaf, column);
    }
  }

  internal static string EnumSearch(SearchTree searchTree, SearchTreeLeaf leaf, string tableName)
  {
    var sql = new StringBuilder();
    for (int i = 0; i < leaf.SearchTerms.Count; i++)
    {
      if (i > 0)
      {
        sql.Append(" union ");
      }
      sql.Append($"select distinct priref as id from [{tableName}] where term = @term{i}");
    }

    AddPreviousResult(sql, searchTree.PreviousResults);
    sql.Append(" order by id");
    return sql.ToString();
  }

  internal static string EnumSearchFullText(SearchTree searchTree, SearchTreeLeaf leaf,
                                            string tableName, string tag)
  {
    var sql = new StringBuilder();
    for (int i = 0; i < leaf.SearchTerms.Count; i++)
    {
      if (i > 0)
      {
        sql.Append(" union ");
      }
      sql.Append($"select distinct priref from [{tableName}] where contains(value, @term{i}) and tag = '{tag}'");
    }

    AddPreviousResult(sql, searchTree.PreviousResults);
    sql.Append(" order by priref");

    return sql.ToString();
  }

  internal static string EmptySearch(string tableName)
     => $"select priref from [{tableName}] where 1 = 0";

  private static string SqlSearchOperator(SearchOperatorEnum searchOperator, string value = "")
    => searchOperator switch
    {
      SearchOperatorEnum.Equals => IsTruncated(value) ? " like " : "=",
      SearchOperatorEnum.Greater => ">",
      SearchOperatorEnum.Smaller => "<",
      SearchOperatorEnum.GreaterOrEquals => ">=",
      SearchOperatorEnum.SmallerOrEquals => "<=",
      SearchOperatorEnum.Generic => " generic ",
      _ => throw new UnsupportedSearchOperatorException(searchOperator),
    };

  internal static string LinkedFieldSearchEx(SearchTree searchTree, SearchTreeLeaf leaf, ResultSet? previousResults)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));
    var localTable = index.TableName ?? throw new NullReferenceException(nameof(index.TableName));
    var linkedDatabase = field.LinkedDatabase ?? throw new NullReferenceException(nameof(field.LinkedDatabase));
    var lookupTag = field.LinkIndexTag ?? throw new NullReferenceException(nameof(field.LinkIndexTag));
    var linkedFieldData = linkedDatabase.FindFieldByTagOrName(lookupTag) ?? throw new NullReferenceException("linkedFieldData");
    var remoteIndex = linkedFieldData.PreferredIndex ?? throw new NullReferenceException(nameof(linkedFieldData.PreferredIndex));
    var remoteTable = remoteIndex.TableName ?? throw new NullReferenceException(nameof(remoteIndex.TableName));

    var sql = new StringBuilder();

    sql.Append($"select [{localTable}].priref as id from [{localTable}] ");

    if (!leaf.SearchAll)
    {
      if (remoteIndex != null && remoteIndex.UseFullText)
      {
        sql.Append($"where term in (select [{linkedDatabase.FullTextTable}].priref from [{linkedDatabase.FullTextTable}] where Contains(value, @term0) and tag = '{linkedFieldData.Tag}')");
      }
      else
      {
        var inSet = new StringBuilder();
        int i = 0;
        foreach (var value in leaf.Values)
        {
          if (i > 0)
          {
            inSet.Append(" union ");
          }
          var term = value.ToString()?.Trim();
          if (term != null)
          {
            if (term == "*")
            {
              inSet.Append($"select distinct [{remoteTable}].priref from [{remoteTable}]");
            }
            else
            {
              var compare = IsTruncated(term) ? "like" : "=";
              inSet.Append($"select [{remoteTable}].priref from [{remoteTable}] where [{remoteTable}].term {compare} @term{i++}");
            }
          }
        }
        sql.Append($"where term in ({inSet})");
      }
    }

    AddPreviousResult(sql, previousResults);
    AddOrderClause(sql, searchTree, leaf, remoteTable);
    return sql.ToString();
  }


  internal static string LinkedFieldSearch(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    var sql = new StringBuilder();
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));
    var localTable = index.TableName ?? throw new NullReferenceException(nameof(index.TableName));
    var linkedDatabase = field.LinkedDatabase ?? throw new NullReferenceException(nameof(field.LinkedDatabase));
    var lookupTag = field.LinkIndexTag ?? throw new NullReferenceException(nameof(field.LinkIndexTag));
    var linkedFieldData = linkedDatabase.FindFieldByTagOrName(lookupTag) ?? throw new NullReferenceException("linkedFieldData");
    var remoteIndex = linkedFieldData.PreferredIndex ?? throw new NullReferenceException(nameof(linkedFieldData.PreferredIndex));
    var remoteTable = remoteIndex.UseFullText ?
      linkedDatabase.FullTextTable ?? throw new NullReferenceException(nameof(linkedDatabase.FullTextTable)) :
      remoteIndex.TableName ?? throw new NullReferenceException(nameof(remoteIndex.TableName));

    sql.Append($"select [{localTable}].[priref] as [id] from [{localTable}] ");
    sql.Append($"inner join [{remoteTable}] on [{localTable}].[term] = [{remoteTable}].[priref] ");

    AddWhereClause(sql, leaf, $"[{remoteTable}].[term]", remoteIndex.UseFullText, remoteIndex.Tag);
    AddPreviousResult(sql, searchTree.PreviousResults);
    AddOrderClause(sql, searchTree, leaf, remoteTable ?? throw new DDException("No RemoteTable"));
    return sql.ToString();
  }

  internal static string LinkedFieldIndirectionSearch(SearchTree searchTree,
                                                      SearchTreeLeaf leaf)
  {
    var sql = new StringBuilder();
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var index = field.PreferredIndex ?? throw new NullReferenceException(nameof(field.PreferredIndex));
    var localTable = index.TableName ?? throw new NullReferenceException(nameof(index.TableName));
    string? indirectionTable = null;
    sql.Append($"select [{localTable}].[priref] as [id] from [{localTable}] ");
    foreach (var indirectionNode in leaf.IndirectionFields!)
    {
      var indirectionLeaf = (SearchTreeLeaf)indirectionNode;
      var indirectionField = indirectionLeaf.Field!;
      var indirectionIndex = indirectionField.PreferredIndex!;
      indirectionTable = indirectionIndex.TableName;

      sql.Append($"inner join [{indirectionTable}] on [{localTable}].[term] = [{indirectionTable}].[priref] ");
    }
    AddWhereClause(sql, leaf, $"[{indirectionTable}].[term]");

    AddPreviousResult(sql, searchTree.PreviousResults);
    AddOrderClause(sql, searchTree, leaf, indirectionTable ?? throw new DDException("No indirectionTable"));
    return sql.ToString();
  }

  private static void AddOrderClause(StringBuilder sql, SearchTree searchTree, SearchTreeLeaf leaf, string index)
  {
    if (searchTree.SortFields.Count > 0)
    {
      var sortField = searchTree.SortFields[0];
      if (sortField.SortOrder == SearchSortOrderEnum.Source)
      {
        sql.Append(" order by case\n");
        for (int i = 0; i < leaf.Values.Count; i++)
        {
          sql.Append($"when [{index}].[term] = @term{i} then {i}\n");
        }
        sql.Append($"else {leaf.Values.Count}\n");
        sql.Append("end;\n");
        return;
      }
    }
    else
    {
      sql.Append(" order by id");
    }
  }

  private static readonly char[] separators = [' ', ',', '\'', '\"'];

  internal static List<string> GetFreeTextSearchWords(string? value)
  {
    var result = value != null ?
     value.Split(separators, StringSplitOptions.RemoveEmptyEntries) : [];
    return [.. result.Select(MaxLength)];
  }

  internal static string MaxLength(string value) => value.Length > MaxWordLength ? value[..MaxWordLength] : value;

  internal static string FreeTextSearch(SearchTree searchTree, SearchTreeLeaf leaf, string tableName)
  {
    var sql = new StringBuilder();

    int parameter = 0;
    for (int i = 0; i < leaf.Values.Count; i++)
    {
      if (i > 0)
      {
        sql.Append(" union ");
      }
      sql.Append('(');
      var words = GetFreeTextSearchWords(leaf.Values[i].ToString());
      for (int j = 0; j < words.Count; j++)
      {
        if (j > 0)
        {
          sql.Append(" intersect ");
        }
        sql.Append(WordQuery(tableName, IsTruncated(words[j]), parameter++));
      }
      sql.Append(')');
    }

    AddPreviousResult(sql, searchTree.PreviousResults);
    sql.Append(" order by id");
    return sql.ToString();
  }

  internal static string FullTextSearch(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));

    var sql = new StringBuilder();
    var and = false;
    sql.Append($"select distinct priref as id from [{field.Database.FullTextTable}] where ");

    if (leaf.Values.Count > 0 && leaf.Values[0].ToString() != "*")
    {
      sql.Append("contains(value, @term0)");
      and = true;
    }

    if (field.Tag != null)
    {
      if (and)
      {
        sql.Append(" and ");
      }
      sql.Append($"tag = '{field.Tag}'");
    }

    AddPreviousResult(sql, searchTree.PreviousResults);
    sql.Append(" order by id");
    return sql.ToString();
  }

  internal static string FullTextTermSearch(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));

    var sql = new StringBuilder();
    var and = false;
    sql.Append($"select distinct priref as id from [{field.Database.FullTextTable}] where ");

    if (leaf.Values.Count > 0)
    {
      var value = leaf.Values[0].ToString();
      if (value != null)
      {
        sql.Append($"term {SqlSearchOperator(leaf.Operator, value)} @term0");
        and = true;
      }
    }

    if (field.Tag != null)
    {
      if (and)
      {
        sql.Append(" and ");
      }
      sql.Append($"tag = '{field.Tag}'");
    }

    AddPreviousResult(sql, searchTree.PreviousResults);
    sql.Append(" order by id");
    return sql.ToString();
  }

  private static void AddPreviousResult(StringBuilder sql, ResultSet? previousResults)
  {
    if (previousResults != null)
    {
      sql.Append($" intersect select distinct id from {previousTable}");
    }
  }

  private static string WordQuery(string tableName, bool truncate, int i)
    => $"select distinct [{tableName}].priref as id from wordlist inner join [{tableName}] on wordlist.wordNumber = [{tableName}].term " +
       $"where wordlist.term {(truncate ? "like" : "=")} @term{i}";

  public static string? ReplaceStar(string? text)
  {
    if (text != null && text.Contains('*'))
    {
      // we use truncation to an SQL like operator will be used
      // replace the star by a percent and escape the opening bracket
      text = text.Replace('*', '%').Replace("[", "[[]").Replace("_", "[_]");
    }
    return text;
  }

  private static bool IsTruncated(string? text) => text != null && text.Contains('*');

  internal static string SelectFromHitList(string name) =>
      $"select priref from {RecordSetHitsTable(name)} where pfNumber = @set";

  private const string previousTable = "#previous";

  internal static string DropPreviousResultsTable => $"drop table [{previousTable}]";

  internal static string CreatePreviousResultsTable => $"create table [{previousTable}] (id int)";

  internal static string CreatePreviousResultsIndex => $"create index id on [{previousTable}] (id)";

  internal static string AddToPreviousResults => $"insert into [{previousTable}] (id) values (@id)";

  internal static string SelectAllRecordLocks => "select * from recordLocks";

  internal static string GetWordNumber =>
    $"select wordNumber from [wordlist] where term = @term and language = @language";

  internal static string SelectHighestWordNumber => "select max(wordNumber) from [wordlist]";

  internal static string AddWord => """
                                      insert into wordlist (term, displayTerm, language, wordNumber)
                                      values (@term, @displayTerm, @language, @wordNumber)
                                    """;

  internal static string InsertKey(string table) =>
    $"insert into [{table}] (term, priref) values (@key, @id)";

  internal static string InsertTextKey(string table, string? domain) =>
    domain != null ?
    $"insert into [{table}] (term, displayTerm, language, domain, priref) values (@key, @displayTerm, @language, @domain, @id)" :
    $"insert into [{table}] (term, displayTerm, language, priref) values (@key, @displayTerm, @language, @id)";

  internal static string InsertFullTextKey(string table)
    => $"insert into [{table}] (tag, occ, value, stripped, language, domain, priref) values (@tag, @occ, @key, @strippedTerm, @language, @domain, @id)";

  internal static string DeleteKey(string table) =>
    $"delete from [{table}] where term = @key and priref = @id";

  internal static string DeleteTextKey(string table, string? domain) =>
    domain != null ?
   $"delete from [{table}] where term = @key and priref = @id and domain = @domain and " +
    "language = @language and displayTerm = @displayTerm" :
    $"delete from [{table}] where term = @key and priref = @id and " +
    "language = @language and displayTerm = @displayTerm";

  internal static string DeleteFullTextKey(string fullTextTable, string? domain) =>
    domain != null ?
      $"delete from [{fullTextTable}] where value = @key and priref = @id and @domain = @domain and language = @language and tag = @tag" :
      $"delete from [{fullTextTable}] where value = @key and priref = @id and language = @language and tag = @tag";

  internal static string DeleteFullTextKey(string fullTextTable) => $"delete from [{fullTextTable}] where value = @displayTerm and priref = @id";

  private static string RecordSetMetaDataTable(string table) => $"[{table}_pointerFiles2]";

  private static string RecordSetHitsTable(string table) => $"[{table}_pointerFiles2_hitlist]";

  private static string RecordSetAccessTable(string table) => $"[{table}_pointerFiles2_access]";

  private static string RecordSetEmailsTable(string table) => $"[{table}_pointerFiles2_email]";

  internal static string SelectData(string table) => $"select data from {table} where priref = @id";

  internal static string GetSetData(string table, string? searchTerms)
  {
    var sql = new StringBuilder($"select * from {RecordSetMetaDataTable(table)}");
    if (!string.IsNullOrWhiteSpace(searchTerms))
    {
      sql.Append($" where [title] like @term or [selectionStatement] like @term or [owner] like @term");
    }
    return sql.ToString();
  }

  internal static string GetNewSetNumber(string table)
  => $""""
      select min(t.[pfnumber] + 1) from {RecordSetMetaDataTable(table)} t
      left join {RecordSetMetaDataTable(table)} t2 on t.[pfnumber] + 1 = t2.[pfnumber] 
      where t2.[pfnumber] is null; 
     """";

  internal static string ExistsSetNumber(string table)
    => $""""
        select count([pfnumber]) from {RecordSetMetaDataTable(table)} where [pfnumber] = @number  
        """";

  internal static string CreateRecordSet(string table)
    => $""""
        insert into {RecordSetMetaDataTable(table)} 
        (pfnumber, owner, title, selectionstatement, hitcount, modification, creation)
        values
        (@number, @owner, @title, @selection, @hitcount, @modification, @creation)
  
        """";

  internal static string UpdateRecordSet(string table)
    => $""""
        update {RecordSetMetaDataTable(table)} 
        set 
        owner = @owner, title = @title, selectionstatement = @selection, modification = @modification, hitcount = @hitcount
        where pfnumber = @number; 
        """";

  internal static string ClearRecordSetHits(string table) => Clear(RecordSetHitsTable(table));

  internal static string ClearRecordSetAccess(string table) => Clear(RecordSetAccessTable(table));

  internal static string ClearRecordSetEmails(string table) => Clear(RecordSetEmailsTable(table));

  internal static string ClearRecordSetMetaData(string table) => Clear(RecordSetMetaDataTable(table));

  internal static string Clear(string table) => $"delete from {table} where pfnumber = @number";

  internal static string WriteRecordSetHits(string table)
    => $""""
        insert into {RecordSetHitsTable(table)} 
        (pfnumber, priref) values (@number, @id)
        """";

  internal static string ReadAllRecords(DatabaseData databaseData, ResultSet? previousResults) =>
    previousResults != null ?
    $"select id from [{previousTable}] order by id" :
    $"select priref from [{databaseData.Name}] order by priref";

  internal static string IdSearch(string indexTable, SearchTreeLeaf leaf)
  {
    var op = SqlSearchOperator(leaf.Operator);
    var sql = new StringBuilder();
    sql.Append($"select priref from {indexTable} where priref {op} @term0");

    for (int i = 1; i < leaf.Values.Count; i++)
    {
      sql.Append($" or priref {op} @term{i}");
    }

    return sql.ToString();
  }

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

  private static void AddDatasets(StringBuilder result, DatasetFilter? datasetFilter, string table, bool searchValuePresent = true)
  {
    if (datasetFilter != null && datasetFilter.Count > 0)
    {
      result.Append(searchValuePresent ? " and " : " where ");
      result.Append('(');
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

  internal static string? FullTextAutoComplete(string tableName, string? value, string? tag, DatasetFilter? datasetFilter, int field)
  {
    var sql = new StringBuilder();
    sql.Append($"select distinct term, '' as [use], count(term) as hits from [{tableName}]");

    var searchValuePresent = !string.IsNullOrWhiteSpace(value) || !string.IsNullOrWhiteSpace(tag);

    if (searchValuePresent)
    {
      sql.Append($" where ");
      if (!string.IsNullOrWhiteSpace(value))
      {
        sql.Append($"contains(value, @term{field})");
      }
      if (tag != null)
      {
        if (!string.IsNullOrWhiteSpace(value))
        {
          sql.Append(" and ");
        }
        sql.Append($"tag = '{tag}'");
      }
    }

    AddDatasets(sql, datasetFilter, tableName, searchValuePresent);
    sql.Append(" group by term");
    return sql.ToString();
  }

  internal static string LinkedAutocomplete(FieldData fieldData, DatasetFilter? datasetFilter, int i, string? value)
  {
    var sql = new StringBuilder();

    var linkedFieldData = fieldData.LinkedFieldData ?? throw new NullReferenceException(nameof(fieldData.LinkedFieldData));
    var linkedDatabase = linkedFieldData.Database ?? throw new NullReferenceException(nameof(linkedFieldData.Database));
    var localIndexTable = fieldData.PreferredIndex!.TableName;
    var remoteIndexTable = linkedFieldData.PreferredIndex!.TableName;
    var nonPreferredTable = fieldData.UseFieldData?.PreferredIndex?.TableName;

    if (linkedDatabase.FullText)
    {
      var table = linkedDatabase.FullTextTable;
      sql.Append($"select distinct term, '' as [use], count(priref) as hits from ");
      sql.Append($"(select distinct value as term, [{localIndexTable}].priref as priref from [{table}] ");
      sql.Append($"inner join [{localIndexTable}] on [{table}].priref = [{localIndexTable}].term where ");
      if (!string.IsNullOrWhiteSpace(value))
      {
        sql.Append($"contains(value, @term{i}) and ");
      }
      sql.Append($"tag = '{linkedFieldData.Tag}'");
      AddDomains(sql, fieldData);
      AddDatasets(sql, datasetFilter, localIndexTable);
      sql.Append($") result group by term");
    }
    else
    {
      sql.Append($"select distinct displayTerm as term, '' as [use], count(displayTerm) as hits from [{remoteIndexTable}] ");
      sql.Append($"inner join [{localIndexTable}] on [{remoteIndexTable}].priref = [{localIndexTable}].term where [{remoteIndexTable}].term like @term{i}");
      if (!string.IsNullOrEmpty(fieldData.LinkDomain))
      {
        sql.Append($" and domain = @domain{i}");
      }
      AddDatasets(sql, datasetFilter, localIndexTable);
      sql.Append(" group by displayTerm");

      if (!string.IsNullOrEmpty(nonPreferredTable))
      {
        sql.Append(" union ");
        sql.Append($"select term.displayTerm as term, useTerm.displayTerm as [use], 0 as hits from [{nonPreferredTable}] inner join [{remoteIndexTable}]");
        sql.Append($" as term on term.priref = [{nonPreferredTable}].priref inner join [{remoteIndexTable}] as useTerm on useTerm.priref = [{nonPreferredTable}].term");
        sql.Append($" where term.term like @term{i}");
        if (!string.IsNullOrEmpty(fieldData.LinkDomain))
        {
          sql.Append($" and term.domain = @domain{i}");
        }
      }
    }

    return sql.ToString();
  }

  private static void AddDomains(StringBuilder result, FieldData fieldData)
  {
    var domains = GetDomains(fieldData);
    if (domains.Count > 0)
    {
      result.Append($" and domain in ({string.Join(',', domains.Select(domain => $"'{domain}'"))})");
    }
  }

  internal static string FindLink(string table, string? domain, string? language, DatasetData? dataset)
  {
    var sql = new StringBuilder($"select priref from [{table}] where term = @term");
    if (domain != null)
    {
      sql.Append($" and domain = @domain");
    }
    if (language != null)
    {
      sql.Append($" and language = @language");
    }
    if (dataset != null)
    {
      sql.Append($" and priref between @lower and @upper");
    }
    return sql.ToString();
  }

  private static List<string> GetDomains(FieldData fieldData)
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
        if (domainFieldData.Enumeration)
        {
          domains.AddRange(domainFieldData.EnumKeys("", null));
        }
      }
    }
    return domains;
  }

  internal static string DeleteIndexKeys(string tableName) =>
    $"delete from [{tableName}] where priref = @id";

  internal static string ReadLinkedRecords(string tableName) =>
    $"select priref from [{tableName}] where term = @id";

  internal static string InsertBooleanKey(string table) => $"insert into [{table}] (term, displayTerm, priref) values (@term, @displayTerm, @id)";

  internal static string DeleteBooleanKey(string table) => $"delete from [{table}] where term = @term and priref = @id";

  internal static string DeleteAlphaNumericKey(string table) => $"delete from [{table}] where term = @key and priref = @id";

  internal static string InsertDateKey(string table) => $"insert into [{table}] (term, displayTerm, priref) values (@term, @displayTerm, @id)";

  internal static string DeleteDateKey(string table) => $"delete from [{table}] where term = @term and priref = @id";

  internal static string DeleteIsoDateKey(string table)
  {
    throw new NotImplementedException();
  }

  internal static string InsertIsoDateKey(string table)
  {
    throw new NotImplementedException();
  }

  internal static string InsertAlphanumericKey(string table) =>
    $"insert into [{table}] (term, displayTerm, priref) values (@key, @displayTerm, @id)";

  internal static string GenericSearch(SearchTree searchTree, SearchTreeLeaf leaf)
  {
    var field = leaf.Field ?? throw new NullReferenceException(nameof(leaf.Field));
    var indexTable = field.GetIndexTableName();

    var partsField = field.GetPartsField();
    var partsIndexTable = partsField?.GetIndexTableName();

    return
      $""""
      WITH RecursiveTree AS (
      
      -- Anchor: select initial IDs based on the name.term condition
      SELECT
         [{indexTable}].priref AS root_id,
         [{indexTable}].priref AS current_id
      FROM
         [{indexTable}]
      WHERE
         [{indexTable}].term LIKE @term0
      UNION ALL

      -- Recursive part: follow child relationships in {partsIndexTable}
      SELECT
         rt.root_id,
         lp.priref AS current_id
      FROM
         RecursiveTree rt
         JOIN [{partsIndexTable}] lp ON lp.term = rt.current_id
      )
      -- Final result: list of all descendant IDs
      SELECT DISTINCT current_id
      FROM RecursiveTree
      ORDER BY current_id OPTION (MAXRECURSION 32767);
      """";
  }

  internal static string GetAutoNumberValue() =>
   """"
        merge [auto_numbering] as target
    using (select @databaseName as database_name, @tag as tag) as source
    on target.database_name = source.database_name and target.tag = source.tag

    when matched then
        update set [counter] = 
            case 
                when [counter] + @delta < @minimum then @minimum
                else [counter] + @delta
            end

    when not matched then
        insert (database_name, tag, [counter])
        values (@databaseName, @tag, 
            case 
                when @minimum > @delta then @minimum
                else @minimum + @delta
            end)

    output @prefix + cast(inserted.[counter] as varchar) + @suffix;
    """";
}
 

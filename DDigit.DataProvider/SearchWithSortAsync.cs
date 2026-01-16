namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  readonly Random random = new();
  public async Task<ResultSet> SearchWithSortAsync(SearchTree searchTree, SearchNode searchNode)
  {
    var parameters = new ParameterCollection();
    var commonTableExpressions = new CommonTableExpressionCollection();
    var sortColumns = searchTree.SortFields.ColumList();

    // If sample size has a value then we will take a random sample from the result set.
    // We ither use the provided seed value, or generate a new random seed.
    if (searchTree.SampleSize.HasValue)
    {
      parameters["@sample_n"] = searchTree.SampleSize.Value;
      parameters["@seed"] = searchTree.Seed ?? random.Next();
    }

    parameters["@startfrom"] = searchTree.StartFrom ?? 1;
    parameters["@limit"] = searchTree.Limit ?? 20;

    const string EmptySet =
      """
          select [result].[id]
          from (
            select 1 as id where 1 = 0
          ) as [result]
          """;

    string TableName(FieldData fieldData)
    {
      var databaseData = fieldData.Database;
      var indexData = fieldData.PreferredIndex!;
      string tableName = fieldData.PreferredIndex!.TableName;

      if (databaseData.IsFullTextEnabled)
      {
        if (!indexData.TableExists.HasValue)
        {
          indexData.TableExists = Repository.CheckIndexTable(databaseData, tableName);
        }
        if (indexData.TableExists.Value)
        {
          return tableName;
        }
        tableName = databaseData.FullTextTable;
      }

      return tableName;
    }

    // Construct the sortColumn, if the sort order == Source, then grab it from the temp sortkeys table, 
    // otherwise from and index table
    string SortColumn(SortField sortField, int index)
      => sortField.SortOrder == SearchSortOrderEnum.Source ?
          $"[sortkeys].[sortkey{index}]" :
          $"[{TableName(sortField.Field!.IsLinked ? sortField.Field.LinkedFieldData! : sortField.Field)}].[term] as [sortkey{index}]";

    // Create a comma separated list of sort keys,
    // we always need a comma, since the sort columns are following the selection columns
    string SortFields(SortFieldList sortFields)
      => string.Concat(sortFields.Select((s, index) => ($",\n  {SortColumn(s, index)}")));

    string Datasets()
    {
      if (searchTree.DatasetFilter == null || searchTree.DatasetFilter.Values.Count == 0)
      {
        return string.Empty;
      }

      var parts = new List<string>();
      foreach (var dataset in searchTree.DatasetFilter.Values)
      {
        parts.Add($"([result].[id] between {parameters.Save(dataset.LowerLimit)} and {parameters.Save(dataset.UpperLimit)})");
      }

      var condition = searchTree.SortFields.Any() ? "and" : "where";
      return $"{condition} ({string.Join(" or ", parts)})";
    }

    string Joins(SearchTreeLeaf leaf)
    {
      var fieldData = leaf.Field!;
      var indexData = fieldData.PreferredIndex ?? throw new MissingIndexException(fieldData.Database.Name, fieldData.Name);
      var indexTable = indexData.TableName;

      var sb = new StringBuilder();

      if (leaf.IndirectionFields is not null && leaf.IndirectionFields.Count > 0)
      {
        var linkIdTag = fieldData.LinkIdTag;
        var linkIdField = fieldData.Database.FindFieldByTagOrName(linkIdTag);
        var linkIdIndexTable = linkIdField!.PreferredIndex!.TableName!;

        var indirectfieldData = (leaf.IndirectionFields[0] as SearchTreeLeaf)!.Field;
        var indirectTable = indirectfieldData!.PreferredIndex!.TableName!;

        sb.AppendLine($"join [{indirectTable}] on [{linkIdIndexTable}].[term] = [{indirectTable}].[priref]");
      }
      else if (fieldData.IsLinked)
      {
        var linkedTable = TableName(fieldData.LinkedFieldData!);
        sb.AppendLine($"join [{linkedTable}] on [{indexTable}].[term] = [{linkedTable}].[priref]");
      }

      return sb.ToString();
    }


    string SortJoins(SortFieldList sortFields)
    {
      var sb = new StringBuilder();
      int i = 0;
      foreach (var sortField in sortFields)
      {
        var fieldData = sortField.Field!;
        var sortTable = TableName(fieldData);
        var linkedTable = fieldData.IsLinked ? TableName(fieldData.LinkedFieldData!) : null;

        sb.AppendLine($"join [{sortTable}] on [result].[id] = [{sortTable}].[priref]");
        if (fieldData.IsLinked)
        {
          sb.AppendLine($"join [{linkedTable}] on [{sortTable}].[term] = [{linkedTable}].[priref]");
        }

        if (sortField.SortOrder == SearchSortOrderEnum.Source)
        {
          var rankTable = fieldData.IsLinked ? linkedTable : sortTable;
          sb.AppendLine(
           $"""
                      join
                       (values {parameters.Ranks()})
                      as [sortkeys]([term], [sortkey{i++}])
                      on [{rankTable}].[term] = [sortkeys].[term]
                      """);
        }

        if (!string.IsNullOrEmpty(fieldData.LinkDomain))
        {
          sb.AppendLine(
            $"""
                       where [{linkedTable}].[domain] = {parameters.Save(fieldData.LinkDomain)}
                       """);
        }
      }
      return sb.ToString();
    }

    static string SortStatement(SortFieldList sortFields)
    {
      var sb = new StringBuilder("order by ");
      if (sortFields.Any())
      {
        int i = 0;
        foreach (var field in sortFields)
        {
          if (i > 0)
          {
            sb.Append(", ");
          }
          sb.Append($"sortkey{i++}");
          sb.Append(field.SortOrder == SearchSortOrderEnum.Descending ? " desc" : " asc");
        }
      }
      else
      {
        sb.Append("[id]");
      }
      return sb.ToString();
    }

    static bool IsTruncated(string? text) => text != null && text.Contains('*');

    // Put the right truncation character in 
    // if thuncation is used we only want the asterix (*) to be replaced by the % wildcard of SQL server
    // There are more wildcards in Sql server like: _ and [
    // To allow searches with _, [ and % these need to be escaped by putting then between '[' and ']'.
    static string? Truncate(string? text)
    {
      if (text is not null && IsTruncated(text))  // skip null and non-truncated texts
      {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
          sb.Append(
            ch switch
            {
              '*' => '%',
              '_' => "[_]",
              '[' => "[[]",
              '%' => "[%]",
              _ => ch
            }
           );
        }
        text = sb.ToString();
      }
      return text;
    }

    string SqlSearchOperator(SearchOperatorEnum searchOperator, string value)
        => searchOperator switch
        {
          SearchOperatorEnum.Equals => IsTruncated(value) ? "like" : "=",
          SearchOperatorEnum.Greater => ">",
          SearchOperatorEnum.Smaller => "<",
          SearchOperatorEnum.GreaterOrEquals => ">=",
          SearchOperatorEnum.SmallerOrEquals => "<=",
          SearchOperatorEnum.Generic => "generic",
          _ => throw new UnsupportedSearchOperatorException(searchOperator),
        };

    string WhereStatement(SearchTreeLeaf leaf, string indexTable, FieldData fieldData)
    {
      FieldData FindSearchFieldData(FieldData from) =>
         from.LinkedFieldData != null ? FindSearchFieldData(from.LinkedFieldData) : from;

      List<object> SearchValues(FieldData fieldData, List<object> searchValues)
      {
        if (!fieldData.IsEnumeration)
        {
          return searchValues;
        }

        var result = new List<object>();
        if (leaf.Values.Count > 0)
        {
          foreach (var value in leaf.Values)
          {
            foreach (var key in fieldData.EnumKeys(value.ToString(), leaf.Language))
            {
              if (!result.Contains(key))
              {
                result.Add(key);
              }
            }
          }
        }
        return result;
      }

      object? ConvertKey(IndexData? indexData, string term)
          => term == "*" ? null :     // this is a special case, if a single * is entered this means any value and will emit 'is not null' in the sql generator
          indexData is null ? Truncate(term) : indexData.Type switch
          {
            IndexTypeEnum.Text or IndexTypeEnum.FreeText or IndexTypeEnum.AlphaNumeric => Truncate(term),
            IndexTypeEnum.Date => KeyConversions.DateTimeStringToInt(term),
            IndexTypeEnum.Integer => Convert.ToInt32(term),
            IndexTypeEnum.IsoDate => KeyConversions.IsoDateToDecimal(term, indexData is not null ? indexData.DateCompletion : DateCompletionEnum.FirstDay),
            IndexTypeEnum.Boolean => term,
            _ => throw new NotImplementedException()
          };

      var databaseData = fieldData.Database;
      var searchField = FindSearchFieldData(fieldData);
      var indexData = searchField.PreferredIndex;

      var keyColumn = indexData switch
      {
        null => "n.value('(text())[1]', 'nvarchar(200)')",      // sequential search in XML
        { Tag: "%0" } => "priref",                               // search on priref 
        { Type: IndexTypeEnum.AlphaNumeric } => "displayterm",  // search on displayTerm for alphanumeric indices
        _ => "term"                                             // term by default
      };

      indexTable = indexData is null ? "" : $"[{indexTable}].";
      var whereBuilder = new StringBuilder();

      foreach (var value in SearchValues(searchField, leaf.Values))
      {
        if (whereBuilder.Length > 0)
        {
          whereBuilder.AppendLine(" or ");
        }
        var searchValue = value.ToString()!;
        var searchTerm = ConvertKey(indexData, searchValue);
        var sortField = searchTree.SortFields.Any(sf => sf.Field == leaf.Field);
        var parameterName = sortField ? "sortTerm" : "term";

        whereBuilder.Append(
          searchTerm is null ? $"{indexTable}{keyColumn} is not null" :   // searchTerm is null means any value, so in sql 'is not null' ;-)
                               $"{indexTable}{keyColumn} {SqlSearchOperator(leaf.Operator, searchValue)} {parameters.Save(parameterName, searchTerm)}"
                           );

        if (fieldData.IsLinked)
        {
          if (!string.IsNullOrWhiteSpace(fieldData.LinkDomain))
          {
            whereBuilder.Append($" and [domain] = {parameters.Save(fieldData.LinkDomain)}");
          }
          if (!string.IsNullOrWhiteSpace(fieldData.LinkDomainTag))
          {
            var linkDomainField = fieldData.LinkDomainField;
            if (linkDomainField?.IsEnumeration is true)
            {
              var domains = linkDomainField.EnumerationValues.Select(v => $"'{v}'");
              whereBuilder.Append($" and [domain] in ({string.Join(", ", domains)})");
            }
          }
        }
        if (fieldData.Database!.IsFullTextEnabled &&
            searchField.Type == FieldTypeEnum.Text &&
            indexData is null)
        {
          whereBuilder.Append($" and [tag] = {parameters.Save(searchField.Tag!)}");
        }
      }
      if (whereBuilder.Length == 0)
      {
        whereBuilder.Append(" 1 = 0");  // results in an empty set ;-)
      }
      return whereBuilder.ToString();
    }

    string LeafSql(SearchTreeLeaf leaf)
    {
      string SequentialSearch(SearchTreeLeaf leaf)
      {
        var fieldData = leaf.Field!;
        var table = fieldData.Database.Name!;
        var tag = fieldData.Tag;

        return $"""
          select trim(n.value('(text())[1]', 'nvarchar(128)')) as term, priref as id
          from [{table}] record
          cross apply record.data.nodes('/record/field[@tag="{tag}"]') as t(n)
          where {WhereStatement(leaf, table, fieldData)}
          """;
      }

      var fieldData = leaf.Field!;
      if (fieldData.PreferredIndex is null)
      {
        return SequentialSearch(leaf);
      }

      var indexData = fieldData.PreferredIndex;
      var indexTable = TableName(fieldData);

      string FreeTextWordSql(string phrase)
      {
        const int maxWordLength = 32;
        string SearchWordSql()
        {
          var parts = new List<string>();

          string Quote(string text) => $"\"{text}\"";

          string UnQuote(string text)
          {
            if (text.Length > 0 && (text[0] == '"' || text[0] == '\''))
            {
              var q = text[0];
              var i = text.Length - 1;
              while (i > 0 && text[i] != q)
              {
                i--;
              }
              if (i > 0)
              {
                text = text[1..i] + text[(i + 1)..];
              }
            }
            return text;
          }

          string AddTag() => fieldData.Tag != null ? $" and [tag] = {parameters.Save(fieldData.Tag!)}" : "";

          if (searchTree.Database.IsFullTextEnabled)
          {
            phrase = UnQuote(phrase);
            parts.Add($"select [priref] as [id] from [{indexTable}] where contains(value, {parameters.Save(Quote(Truncate(phrase!)!))}){AddTag()}");
          }
          else
          {
            List<string> SplitFreeTextSearchWords(string? value)
              => value is not null ? TextTokenizer.GetWords(value, true) : [];

            foreach (var word in SplitFreeTextSearchWords(phrase.ToString()))
            {
              parts.Add(
              $"""
               select distinct [priref] as [id] from [{indexTable}] inner join [wordlist] on
                [{indexData.TableName}].[term] = [wordlist].[wordnumber]
                where [wordlist].[term] {SqlSearchOperator(SearchOperatorEnum.Equals, word)} substring({parameters.Save(Truncate(word!)!)}, 1, {maxWordLength})
              """);
            }
            if (parts.Count == 0)
            {
              parts.Add(EmptySet);
            }
          }
          return string.Join(" intersect ", parts);
        }
        return $"({SearchWordSql()})";
      }

      string FreeTextSql(SearchTreeLeaf leaf)
      {
        var parts = new List<string>();
        foreach (var value in leaf.Values)
        {
          parts.Add(FreeTextWordSql(value.ToString()!));
        }
        return string.Join(" union ", parts);
      }

      string UnderSearch(SearchTreeLeaf leaf)
      {
        var fieldData = leaf.Field!;
        var index = fieldData.PreferredIndex!;
        var databaseData = searchTree.Database!;
        var mainTable = databaseData.IsFullTextEnabled ? databaseData.FullTextTable : index.TableName!;
        var andTag = databaseData.IsFullTextEnabled ?
            $"and [{mainTable}].[tag] = {parameters.Save(fieldData.Tag!)}" : "";

        var internalLink = databaseData.InternalLinks
            .FirstOrDefault(il => il.TermTag == fieldData.Tag && il.RelationType == RelationTypeEnum.Hierarchical) ??
            throw new DDException("Field should have a hierarchical internal link in order to use the path search operator");
        var partOfLref = internalLink.BroaderTermLinkIdTag ??
            throw new DDException("Hierarchical relation should have a broader term linkref tag.");
        var broaderField = databaseData.FindFieldByTagOrName(partOfLref)!;
        var broaderIndex = broaderField.PreferredIndex!;
        var broaderTable = broaderIndex.TableName!;

        var value = leaf.Values[0].ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
          throw new DDException("A search value is required for a path search.");
        }

        var separator = value.Contains('\\') ? '\\' : value.Contains('-') ? '-' : '/';
        var separatorParameter = parameters.Save("separator", separator);
        var pathParameter = parameters.Save("path", value);
        var segments = $"segments{commonTableExpressions.Count}";
        var walk = $"walk{commonTableExpressions.Count}";
        var root = $"root{commonTableExpressions.Count}";
        var under = $"under{commonTableExpressions.Count}";

        commonTableExpressions.Add(
        $"""
                 with [{segments}] as (
                 select
                   cast([value] as nvarchar(50)) as [code],
                   [ordinal] as [level_no]
                   from string_split({pathParameter}, {separatorParameter}, 1)
                 )
                 """);

        commonTableExpressions.Add(
        $"""
                 [{walk}] as (
                 select
                   [{segments}].[level_no],
                   [{mainTable}].[priref] as [id]
                 from [{mainTable}] 
                 join [{segments}]
                   on [{mainTable}].[term] = [{segments}].[code] {andTag}
                 where [{segments}].[level_no] = 1
     
                 union all
     
                 select
                   [{segments}].[level_no],
                   [{mainTable}].[priref] as [id]
                 from [{walk}] 
                 join [{broaderTable}] 
                   on [{broaderTable}].[term] = [{walk}].[id]
                 join [{mainTable}] 
                   on [{mainTable}].[priref] = [{broaderTable}].[priref]
                 join [{segments}]
                   on [{segments}].[level_no] = [{walk}].[level_no] + 1
                 and  [{mainTable}].[term] = [{segments}].[code] {andTag}
                 )
                 """);

        commonTableExpressions.Add(
        $"""
                 [{root}] as (
                 select top (1) [id]
                 from [{walk}]
                   where [level_no] = (select max([level_no]) from [{segments}])
                   order by [level_no] desc
                 )
                 """);

        commonTableExpressions.Add(
        $"""
                 [{under}] as (
                 -- anchor: start at the resolved root node
                 select
                   [id],
                   cast(0 as int) as [depth]
                 from [{root}]

                 union all

                 -- recursive: go from parent id to its children
                 select
                   [{mainTable}].[priref] as [id],
                   [{under}].[depth] + 1  as [depth]
                 from [{under}]
                 join [{broaderTable}]
                   on [{broaderTable}].[term] = [{under}].[id]
                 join [{mainTable}]
                   on [{mainTable}].[priref] = [{broaderTable}].[priref] {andTag}
                 )
                 """);

        var sql =
        $"""
                 select distinct [id] from [{under}]
                 where depth > 0
                """;

        return sql;

      }

      string PathSearch(SearchTreeLeaf leaf)
      {
        var fieldData = leaf.Field!;
        var index = fieldData.PreferredIndex!;
        var databaseData = searchTree.Database!;
        var mainTable = databaseData.IsFullTextEnabled ? databaseData.FullTextTable : index.TableName!;
        var andTag = databaseData.IsFullTextEnabled ?
            $"and [{mainTable}].[tag] = {parameters.Save(fieldData.Tag!)}" : "";

        var internalLink = databaseData.InternalLinks
            .FirstOrDefault(il => il.TermTag == fieldData.Tag && il.RelationType == RelationTypeEnum.Hierarchical) ??
            throw new DDException("Field should have a hierarchical internal link in order to use the path search operator");
        var partOfLref = internalLink.BroaderTermLinkIdTag ??
            throw new DDException("Hierarchical relation should have a broader term linkref tag.");
        var broaderField = databaseData.FindFieldByTagOrName(partOfLref)!;
        var broaderIndex = broaderField.PreferredIndex!;
        var broaderTable = broaderIndex.TableName!;

        var value = leaf.Values[0].ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
          throw new DDException("A search value is required for a path search.");
        }

        var separator = value.Contains('\\') ? '\\' : value.Contains('-') ? '-' : '/';
        var separatorParameter = parameters.Save("separator", separator);
        var pathParameter = parameters.Save("path", value);
        var segments = $"segments{commonTableExpressions.Count}";
        var walk = $"walk{commonTableExpressions.Count}";

        var cte =
          $"""
                    with [{segments}] as (
                        select
                            cast([value] as nvarchar(50)) as [code],
                            [ordinal]                     as [level_no]
                        from string_split({pathParameter}, {separatorParameter}, 1)
                    ),
                     
                    [{walk}] as (
                        select
                            [{segments}].[level_no],
                            [{mainTable}].[priref] as [id]
                        from [{mainTable}] 
                        join [{segments}]
                          on [{mainTable}].[term] = [{segments}].[code] {andTag}
                        where [{segments}].[level_no] = 1
                     
                        union all
                     
                        select
                            [{segments}].[level_no],
                            [{mainTable}].[priref] as [id]
                        from [{walk}] 
                        join [{broaderTable}] 
                          on [{broaderTable}].[term] = [{walk}].[id]
                        join [{mainTable}] 
                          on [{mainTable}].[priref] = [{broaderTable}].[priref]
                        join [{segments}]
                          on [{segments}].[level_no] = [{walk}].[level_no] + 1
                         and [{mainTable}].[term] = [{segments}].[code] {andTag}
                     )
                    """;

        commonTableExpressions.Add(cte);

        var sql = $"""
                    select top (1) [id] from [{walk}]
                    where [level_no] = (select max([level_no]) from [{segments}])
                    order by [level_no] desc
                    """;
        return sql;
      }

      string GenericSearch(SearchTreeLeaf leaf)
      {
        throw new NotImplementedException();
      }

      string WhereClause(SearchTreeLeaf leaf)
      {
        FieldData FindSearchFieldData(FieldData from) =>
          from.LinkedFieldData is not null ? FindSearchFieldData(from.LinkedFieldData) : from;

        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(indexTable))
        {
          var indexFieldData = fieldData;
          if (leaf.IndirectionFields is not null && leaf.IndirectionFields.Count > 0)
          {
            indexFieldData = (leaf.IndirectionFields[0] as SearchTreeLeaf)!.Field;
            indexTable = indexFieldData!.PreferredIndex!.TableName!;
          }
          else if (fieldData.IsLinked)
          {
            indexTable = TableName(fieldData.LinkedFieldData!);
          }
          sb.Append($"where {WhereStatement(leaf, indexTable, indexFieldData)}");
        }

        return sb.ToString();
      }

     string IndexedSearch(SearchTreeLeaf leaf)
          => leaf.Operator switch
          {
            SearchOperatorEnum.Path => PathSearch(leaf),
            SearchOperatorEnum.Under => UnderSearch(leaf),
            SearchOperatorEnum.Generic => GenericSearch(leaf),
            _ => $"""
                             select distinct 
                             [{indexTable}].[priref] as [id]
                             from [{indexTable}]
                             {Joins(leaf)}
                             {WhereClause(leaf)}
                             """,
          };


      return indexData.Type == IndexTypeEnum.FreeText ? FreeTextSql(leaf) : IndexedSearch(leaf);
    }

    string TreeSql(SearchNode searchNode)
    {
      static string SqlOperator(BooleanOperator op)
        => op switch
        {
          BooleanOperator.Or => "union",
          BooleanOperator.And => "intersect",
          BooleanOperator.Not => "except",
          _ => throw new DDException($"Illegal Operator {op}"),
        };

      string SetSql(SearchSetLeaf setLeaf)
        => $"select [priref] as [id] from [{searchTree.Database.Name}_pointerfiles2_hitlist] where [pfnumber] = {parameters.Save(setLeaf.SetId)}";

      string tree = searchNode switch
      {
        SearchTreeLeaf leaf => LeafSql(leaf),
        SearchTreeNode node => string.Join(' ', [TreeSql(node.Left), SqlOperator(node.Operator), TreeSql(node.Right)]),
        SearchSetLeaf setLeaf => SetSql(setLeaf),
        _ => throw new DDException("Wrong search node")
      };

      return $"({tree})";
    }

    string CreateTempTable(string tableName)
    {
      var parts = new List<string>();

      foreach (var sortField in searchTree.SortFields)
      {
        var name = $"sortkey{parts.Count}";
        if (sortField.SortOrder == SearchSortOrderEnum.Source)
        {
          parts.Add($"{name} int");
        }
        else
        {
          parts.Add(sortField.Field switch
          {
            { Type: FieldTypeEnum.Integer } => $"{name} int",
            { Type: FieldTypeEnum.Text } => $"{name} nvarchar({(sortField.Field.Length > 0 ? sortField.Field.Length : "max")})",
            { Type: FieldTypeEnum.Temporary } => $"{name} nvarchar({(sortField.Field.Length > 0 ? sortField.Field.Length : "max")})",
            _ => $"{name} nvarchar(80)"
          });
        }
      }

      var sortColumns = parts.Count > 0 ? $", {string.Join(",", parts)}" : "";

      return
        $"""
        if (object_id('tempdb..{tableName}')) is not null drop table {tableName};
        create table {tableName} ([id] int not null{sortColumns});
        """;
    }

    string CreateSample()
    {
      return searchTree.SampleSize.HasValue ?
        $"""
          -- stage table with the SAME schema as #r
         {CreateTempTable("#rs")}

         -- insert exactly @sample_n rows in a seedable random order
         insert into #rs ([id]{sortColumns})
         select top (@sample_n)
           r.id{sortColumns}
         from #r as r
         order by hashbytes('SHA2_256',
                       convert(varbinary(4), @seed) + convert(varbinary(8), r.id)), r.id;  -- tie-breaker

         -- swap contents: clear #r and refill with the sampled rows
         truncate table #r;
         insert into #r (id{sortColumns}) select id{sortColumns} from #rs;
         drop table #rs;
        """ : "";
    }

    string sortFields = SortFields(searchTree.SortFields);

    string searchTreeSql = TreeSql(searchTree.Root!);

    string sortJoins = SortJoins(searchTree.SortFields);

    string datasets = Datasets();

    string  sortStatement = SortStatement(searchTree.SortFields);

    string sample = CreateSample();

    string declarations = parameters.ToSql();

    string limit = parameters.TryGetValue("@limit", out object? l) && (int)l > 0 ? "fetch next @limit rows only" : "";

    string sql =
     $"""
      declare @offset int = 
        case 
          when @startfrom > 0 then @startfrom - 1 else 0
        end;

      {CreateTempTable("#r")}

      {commonTableExpressions.ToSql()}
      
      insert into #r([id]{sortColumns})
      select
        [result].[id]{sortFields}
        from
        (
          select [idlist].[id] 
          from
          (
            {searchTreeSql}
          )
          as [idlist]
        )
      as [result]
      {sortJoins}
      {datasets};
      {sample}

      declare @total int;
      select @total = count(id) from #r; 

      select [id], @total as [total]{sortColumns} from #r {sortStatement}
      offset @offset rows 
      {limit};
      """;

    string completeSql =
      $"""
      use [{searchTree.Database.DSN}];
      {declarations}
      {sql}
      """;

    return await Repository.RunSqlSearch(searchTree.Database, sql, parameters, searchTree.SqlState);
  }

}
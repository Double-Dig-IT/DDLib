namespace DDigit.Search;

public class DDSearchParser
{
  public SearchTree Parse(DatabaseData databaseData, string searchStatement, CancellationToken cancellationToken)
  {
    var searchTree = new SearchTree()
    {
      Database = databaseData,
      Statement = searchStatement,
      Cancellation = cancellationToken,
    };

    this.databaseData = databaseData;
    var stack = new Stack<SearchNode>();
    var tokenizer = new SearchTokenizer(searchStatement);
    ProcessToken(tokenizer, stack);

    while (stack.Count == 1 && !tokenizer.IsEndOfText)
    {
      if (tokenizer.Accept("sort"))
      {
        if (searchTree.SortFields.Count != 0)
        {
          throw new UnexpectedTokenException(tokenizer.Accepted, string.Empty, searchStatement);
        }
        ExpectSortFields(tokenizer, searchTree.SortFields);
        continue;
      }

      if (tokenizer.Accept("random"))
      {
        GetRandomFilter(searchTree, tokenizer);
        continue;
      }

      if (tokenizer.Accept("startFrom"))
      {
        if (searchTree.StartFrom.HasValue)
        {
          throw new UnexpectedTokenException(tokenizer.Accepted, string.Empty, searchStatement);
        }
        searchTree.StartFrom = tokenizer.ExpectInteger();
        continue;
      }

      if (tokenizer.Accept("limit"))
      {
        if (searchTree.Limit.HasValue)
        {
          throw new UnexpectedTokenException(tokenizer.Accepted, string.Empty, searchStatement);
        }
        searchTree.Limit = tokenizer.ExpectInteger();
        continue;
      }

      throw new UnexpectedTokenException(tokenizer.ToString(), string.Empty, searchStatement);
    }

    if (!tokenizer.IsEndOfText)
    {
      throw new UnexpectedTokenException(tokenizer.Token, string.Empty, searchStatement);
    }

    searchTree.Root = stack.Pop();
    return searchTree;
  }

  private void ExpectSortFields(SearchTokenizer tokenizer, List<SortField> sortFields)
  {
    do
    {
      var fieldName = ExpectFieldName(tokenizer, databaseData!);
      var sortField = new SortField(fieldName);
      if (tokenizer.Accept("["))
      {
        sortField.Language = ExpectLanguage(tokenizer);
      }

      while (tokenizer.Accept("-"))
      {
        if (tokenizer.Accept(">"))
        {
          tokenizer.GetToken();
          if (tokenizer.Accept("["))
          {
            ExpectLanguage(tokenizer);
          }
        }
      }

      if (Enum.TryParse<SearchSortOrderEnum>(tokenizer.Token!, true, out var sortOrder))
      {
        sortField.SortOrder = sortOrder;
        tokenizer.GetToken();
      }
      sortFields.Add(sortField);
    } while (tokenizer.Accept(","));
  }

  private void ProcessToken(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    if (tokenizer.Accept("("))
    {
      ProcessToken(tokenizer, stack);
      tokenizer.Expect(")");
    }
    else
    {
      if (tokenizer.Accept("set") || tokenizer.Accept("pointer"))
      {
        stack.Push(new SearchSetLeaf(tokenizer.ExpectInteger()));
      }
      else
      {
        ExpectSimpleSearchStatement(tokenizer, stack);

        if (stack.Peek() is SearchTreeLeaf leaf)
        {
          var field = leaf.Field ?? throw new ParseStackException("Field is missing");
          while (tokenizer.Accept(",") || tokenizer.Accept("+") || tokenizer.Accept("-"))
          {
            var right = new SearchTreeLeaf(field, ExpectValue(field, tokenizer)
              ?? throw new SearchValueExpectedException(), leaf.Operator, leaf.Language, leaf.IndirectionFields);
            PushBoolean(right, stack.Pop(), GetBooleanOperator(tokenizer.Accepted), stack);
          }
        }
      }
    }
    if (!tokenizer.IsEndOfText)
    {
      AcceptNot(tokenizer, stack);
    }
  }

  private static void PushBoolean(SearchNode right, SearchNode left, BooleanOperator booleanOperator,
                           Stack<SearchNode> stack)
  {
    if (booleanOperator == BooleanOperator.Or)
    {
      if (right is SearchTreeLeaf rightLeaf && left is SearchTreeLeaf leftLeaf && rightLeaf.Field != null && leftLeaf.Field != null &&
          rightLeaf.Field.Tag == leftLeaf.Field.Tag && rightLeaf.Language == leftLeaf.Language &&
          rightLeaf.Operator == SearchOperatorEnum.Equals && leftLeaf.Operator == SearchOperatorEnum.Equals)
      {
        leftLeaf.Values.Add(rightLeaf.Values[0]);
        stack.Push(leftLeaf);
        return;
      }
    }
    stack.Push(new SearchTreeNode(right, left, booleanOperator));
  }

  private static void GetRandomFilter(SearchTree searchTree, SearchTokenizer tokenizer)
  {
    searchTree.SampleSize = tokenizer.ExpectInteger();
    searchTree.Seed = 0;
    if (tokenizer.Accept("seed"))
    {
      searchTree.Seed = tokenizer.ExpectInteger();
    }
    searchTree.Unique = false;
    if (tokenizer.Accept("unique"))
    {
      searchTree.Unique = true;
    }
  }

  private static BooleanOperator GetBooleanOperator(string? token)
    => token switch
    {
      "," => BooleanOperator.Or,
      "+" => BooleanOperator.And,
      "-" => BooleanOperator.Not,
      _ => throw new NotImplementedException(),
    };

  private void AcceptNot(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    if (tokenizer.Accept("not"))
    {
      ProcessToken(tokenizer, stack);
      stack.Push(new SearchTreeNode(stack.Pop(), stack.Pop(), BooleanOperator.Not));
    }
    else
    {
      AcceptOr(tokenizer, stack);
    }
  }

  private void AcceptOr(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    if (tokenizer.Accept("or"))
    {
      ProcessToken(tokenizer, stack);
      PushBoolean(stack.Pop(), stack.Pop(), BooleanOperator.Or, stack);
    }
    else
    {
      AcceptAnd(tokenizer, stack);
    }
  }

  private void AcceptAnd(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    if (tokenizer.Accept("and"))
    {
      var BooleanOperator = Classes.BooleanOperator.And;
      if (tokenizer.Accept("not"))
      {
        BooleanOperator = Classes.BooleanOperator.Not;
      }
      ProcessToken(tokenizer, stack);
      stack.Push(new SearchTreeNode(stack.Pop(), stack.Pop(), BooleanOperator));
    }
  }

  private void ExpectSimpleSearchStatement(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    if (!AcceptQSearch(tokenizer, stack) && !AcceptAllSearch(tokenizer, stack))
    {
      var field = ExpectFieldName(tokenizer, databaseData!);
      var language = AcceptLanguage(tokenizer);
      var op = ExpectOperator(tokenizer);
      var linkPath = new List<SearchNode>();

      if (!field.IsLinked && field.IsMergedField && op != SearchOperatorEnum.Indirection)
      {
        var linkedField = field.LinkedFieldData!;
        var linkSourceField = linkedField.LinkSourceField(field);
        linkPath.Add(new SearchTreeLeaf(linkSourceField!, null, op, language, null));
        field = linkedField;
      }
      else
      {
        while (op == SearchOperatorEnum.Indirection)
        {
          var linkedDatabaseData = field.LinkedDatabase ?? throw new DDException($"field '{field}' does not have a linked database");
          var linkedField = ExpectFieldName(tokenizer, linkedDatabaseData);
          var linkedLanguage = AcceptLanguage(tokenizer);
          linkPath.Add(new SearchTreeLeaf(linkedField, null, op, linkedLanguage, null));
          op = ExpectOperator(tokenizer);
        }
      }
      var value = ExpectValue(field, tokenizer);
      stack.Push(new SearchTreeLeaf(field, value!, op, language, linkPath));
    }
  }

  private static string? AcceptLanguage(SearchTokenizer tokenizer)
  {
    string? language = null;
    if (tokenizer.Accept("["))
    {
      language = ExpectLanguage(tokenizer);
    }
    return language;
  }

  private bool AcceptAllSearch(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    var result = false;
    if (tokenizer.Accept("all"))
    {
      var idField = databaseData!.FindFieldByTagOrName("priref") ??
        throw new FieldNotFoundException("priref", databaseData.Name);
      stack.Push(new SearchTreeLeaf(idField, 0, SearchOperatorEnum.Greater, null, null));
      result = true;
    }
    return result;
  }

  private static bool AcceptQSearch(SearchTokenizer tokenizer, Stack<SearchNode> stack)
  {
    var result = false;
    if (tokenizer.Accept("q"))
    {
      tokenizer.GetToken();
      tokenizer.Expect("=");
      tokenizer.GetToken();
      var text = tokenizer.Token;
      if (text != null)
      {
        stack.Push(new SearchTreeQLeaf(text));
        result = true;
      }
    }
    return result;
  }

  private static long? ExpectLongValue(string? token)
  {
    long? result = null;
    if (token != null)
    {
      token = token.Trim('*');  // remove any added truncation signs
      if (token.Length > 0)
      {
        if (long.TryParse(token, out var number))
        {
          result = number;
        }
      }
    }
    return result;
  }

  private static object? ExpectValue(FieldData fieldData, SearchTokenizer tokenizer)
  {
    object? result = null;
    if (tokenizer.Token != null)
    {
      result = fieldData.Type switch
      {
        FieldTypeEnum.Integer => ExpectLongValue(tokenizer.Token),  
        FieldTypeEnum.DateEuropean or
        FieldTypeEnum.DateGeneral or
        FieldTypeEnum.DateUsa or
        FieldTypeEnum.DateIso => ExpectDate(fieldData, tokenizer),
        _ => tokenizer.Token,
      };
    }
    tokenizer.GetToken();
    return result;
  }

  private static string? ExpectDate(FieldData fieldData, SearchTokenizer tokenizer)
  {
    if (tokenizer.Accept("today"))
    {
      var today = DateTime.Now;
      if (tokenizer.Accept("-"))
      {
        return FormatDate(fieldData.Type, today.AddDays(-tokenizer.ExpectInteger()));
      }
      if (tokenizer.Accept("+"))
      {
        return FormatDate(fieldData.Type, today.AddDays(tokenizer.ExpectInteger()));
      }
      return FormatDate(fieldData.Type, today);
    }
    else
    {
      return tokenizer.Token;
    }
  }

  private static string FormatDate(FieldTypeEnum type, DateTime dateTime)
    => type switch
    {
      FieldTypeEnum.DateIso or
      FieldTypeEnum.DateGeneral => $"{dateTime:yyyy-MM-dd}",
      FieldTypeEnum.DateEuropean => $"{dateTime:dd/MM/yyyy}", // 31/01/2002
      FieldTypeEnum.DateUsa => $"{dateTime:MM/dd/yyyy}", // 01/31/2002
      _ => throw new ArgumentException("Invalid field type", nameof(type)),
    };

  private static string? ExpectLanguage(SearchTokenizer tokenizer)
  {
    var language = tokenizer.Token;
    tokenizer.GetToken();
    if (tokenizer.Accept("]"))
    {
      // this indicates a two letter language code (e.g. en)
      return language;
    }
    else
    {
      tokenizer.Expect("-");
      // this indicates a language plus region code (e.g. en-US)
      language = $"{language}-{tokenizer.Token}";
      if (supportedLanguages.Contains(language))
      {
        tokenizer.GetToken();
        tokenizer.Expect("]");
        return language;
      }
    }
    throw new LanguageExpectedException(tokenizer.Token);
  }

  private static FieldData ExpectFieldName(SearchTokenizer tokenizer, DatabaseData databaseData)
  {
    var token = tokenizer.Token;
    if (string.IsNullOrWhiteSpace(token))
    {
      throw new FieldNameExpectedException(token);
    }
    var field = databaseData.FindFieldByTagOrName(token) ?? throw new FieldNameExpectedException(token);
    tokenizer.GetToken();
    return field;
  }

  private static SearchOperatorEnum ExpectOperator(SearchTokenizer tokenizer)
  {
    if (Enum.TryParse(tokenizer.Token, true, out SearchOperatorEnum searchOperator))
    {
      tokenizer.GetToken();
      return searchOperator;
    }

    switch (tokenizer.Token)
    {
      // accept  =  => or =<
      case "=":
        searchOperator = SearchOperatorEnum.Equals;
        tokenizer.GetToken();
        if (tokenizer.Accept(">"))
        {
          searchOperator = SearchOperatorEnum.GreaterOrEquals;
        }
        if (tokenizer.Accept("<"))
        {
          searchOperator = SearchOperatorEnum.SmallerOrEquals;
        }
        break;

      // accept > or >=
      case ">":
        searchOperator = SearchOperatorEnum.Greater;
        tokenizer.GetToken();
        if (tokenizer.Accept("="))
        {
          searchOperator = SearchOperatorEnum.GreaterOrEquals;
        }
        break;

      // accept < or <= 
      case "<":
        searchOperator = SearchOperatorEnum.Smaller;
        tokenizer.GetToken();
        if (tokenizer.Accept("="))
        {
          searchOperator = SearchOperatorEnum.SmallerOrEquals;
        }
        break;

      // accept ->
      case "-":
        tokenizer.GetToken();
        tokenizer.Expect(">");
        searchOperator = SearchOperatorEnum.Indirection;
        break;

      default:
        throw new OperatorExpectedException(tokenizer.Token);
    }

    return searchOperator;
  }

  private DatabaseData? databaseData;

  private static readonly string[] supportedLanguages = ["en-US", "nl-NL", "fr-FR", "da-DK", "de-DE"];
}

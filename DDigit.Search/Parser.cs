namespace DDigit.Search;

internal class Parser
{
  internal Parser(DatabaseData databaseData, string searchStatement)
  {
    this.databaseData = databaseData;
    statement = searchStatement;
    length = statement.Length;
  }

  internal void Parse()
  {
    GetToken();
    while (token != null)
    {
      ProcessToken();
    }
  }

  private void ProcessToken()
  {
    if (Accept("("))
    {
      ProcessToken();
      Expect(")");
    }
    else
    {
      if (Accept("set") || Accept("pointer"))
      {
        GetSetNumber();
      }
      else if (Accept("random"))
      {
        GetRandomFilter();
      }
      else
      {
        ExpectSimpleSearchStatement();
        if (stack.Peek() is SearchTreeLeaf leaf)
        {
          var field = leaf.Field ?? throw new ParseStackException("Field is missing");
          while (Accept(",") || Accept("+") || Accept("-"))
          {
            stack.Push(new SearchTreeNode(new SearchTreeLeaf(field, AcceptValue(field) ?? throw new SearchValueExpectedException(), leaf.Operator, leaf.Language),
                                          stack.Pop(),
                                          GetBooleanOperator(accepted![0])));
          }
        }
      }
    }
    AcceptNot();
  }

  private bool Accept(string accept)
  {
    var result = false;
    if (string.Compare(token, accept, true) == 0)
    {
      result = true;
      accepted = token;
      GetToken();
    }
    return result;
  }

  private void SkipWhiteSpace()
  {
    while (!EndOfText && IsWhiteSpace)
    {
      pos++;
    }
  }

  private void GetRandomFilter()
  {
    var node = stack.Peek();
    node.SampleSize = ExpectNumber();
    node.Seed = null;
    if (Accept("seed"))
    {
      node.Seed = ExpectNumber();
    }
    node.Unique = Accept("unique");
  }

  private void GetSetNumber()
  {
    stack.Push(new SearchSetLeaf(ExpectNumber()));
    GetToken();
  }

  private int ExpectNumber()
  {
    if (int.TryParse(token, out int number))
    {
      GetToken();
      return number;
    }
    throw new NumberExpectedException(token!);
  }

  private static BooleanOperator GetBooleanOperator(char token)
    => token switch
    {
      ',' => BooleanOperator.Or,
      '+' => BooleanOperator.And,
      '-' => BooleanOperator.Not,
      _ => throw new NotImplementedException(),
    };

  private void AcceptNot()
  {
    if (Accept("not"))
    {
      ProcessToken();
      stack.Push(new SearchTreeNode(stack.Pop(), stack.Pop(), BooleanOperator.Not));
    }
    else
    {
      AcceptOr();
    }
  }

  private void AcceptOr()
  {
    if (Accept("or"))
    {
      ProcessToken();
      stack.Push(new SearchTreeNode(stack.Pop(), stack.Pop(), BooleanOperator.Or));
    }
    else
    {
      AcceptAnd();
    }
  }

  private void AcceptAnd()
  {
    if (Accept("and"))
    {
      var BooleanOperator = Classes.BooleanOperator.And;
      if (Accept("not"))
      {
        BooleanOperator = Classes.BooleanOperator.Not;
      }
      ProcessToken();
      stack.Push(new SearchTreeNode(stack.Pop(), stack.Pop(), BooleanOperator));
    }
  }

  private void ExpectSimpleSearchStatement()
  {
    if (!AcceptQSearch() && !AcceptAllSearch())
    {
      var field = ExpectFieldName();
      string? language = null;
      if (Accept("["))
      {
        language = ExpectLanguage();
      }
      var op = ExpectOperator();
      var value = AcceptValue(field);
      stack.Push(new SearchTreeLeaf(field, value!, op, language));
    }
  }

  private bool AcceptAllSearch()
  {
    var result = false;
    if (Accept("all"))
    {
      stack.Push(new SearchTreeLeaf(databaseData.FindFieldByTagOrName("priref"), 0, SearchOperators.Greater));
      result = true;
    }
    return result;
  }

  private bool AcceptQSearch()
  {
    var result = false;
    if (Accept("q"))
    {
      Expect("=");
      var text = AcceptString();
      if (text != null)
      {
        stack.Push(new SearchTreeQLeaf(text));
        result = true;
      }
    }
    return result;
  }

  private object? AcceptValue(FieldData fieldData)
  {
    object? any = null;
    if (token != null)
    {
      any = fieldData.Type switch
      {
        FieldTypeEnum.Integer => long.Parse(token),
        _ => token,
      };
    }

    GetToken();
    return any;
  }

  private string? AcceptString()
  {
    string? token = this.token;
    GetToken();
    return token;
  }

  private string? ExpectLanguage()
  {
    var language = token;
    GetToken();
    if (Accept("]"))
    {
      // this indicates a two letter language code (e.g. en)
      return language;
    }
    else
    {
      Expect("-");
      // this indicates a language plus region code (e.g. en-US)
      language = $"{language}-{token}";
      if (supportedLanguages.Contains(language))
      {
        GetToken();
        Expect("]");
        return language;
      }
    }
    throw new LanguageExpectedException(token);
  }

  private FieldData ExpectFieldName()
  {
    var field = token == null ? null : databaseData.FindFieldByTagOrName(token);
    if (field != null)
    {
      GetToken();
      return field;
    }
    throw new FieldNameExpectedException(token);
  }

  private SearchOperators ExpectOperator()
  {
    var searchOperator = token switch
    {
      "=" => SearchOperators.Equals,
      ">" => SearchOperators.Greater,
      "<" => SearchOperators.Smaller,
      ">=" => SearchOperators.GreaterOrEquals,
      "<=" => SearchOperators.SmallerOrEquals,
      _ => throw new OperatorExpectedException(token),
    }; ;
    GetToken();
    return searchOperator;
  }

  private void Expect(string expect)
  {
    if (string.Compare(token, expect, true) != 0)
    {
      throw new UnexpectedTokenException(token, expect);
    }
    GetToken();
  }

  private void GetToken()
  {
    StringBuilder result = new();
    SkipWhiteSpace();
    char? literal = null;
    while (!EndOfText && ((literal == null && !IsWhiteSpace && !IsSpecial) || literal != null))
    {
      if (literal == null && Literal)
      {
        literal = Cp;
      }
      else if (Cp == literal)
      {
        literal = null;
      }
      else
      {
        result.Append(Cp);
      }
      pos++;
    }

    if (result.Length > 0)
    {
      token = result.ToString();
      return;
    }

    if (IsSpecial)
    {
      result = new();
      while (!EndOfText && IsSpecial)
      {
        result.Append(Cp);
        if (IsOpening || IsClosing)
        {
          pos++;
          break;
        }
        pos++;
      }
      token = result.ToString();
      return;
    }

    token = null;
  }

  public SearchNode? Result => stack.Peek();

  private readonly DatabaseData databaseData;

  private readonly string statement;

  private string? token;

  private string? accepted;

  private char? Cp => !EndOfText ? statement[pos] : null;

  private bool EndOfText => pos >= length;

  private bool IsWhiteSpace => white.Contains(Cp);

  private bool IsSpecial => specials.Contains(Cp);

  private bool IsClosing => closing.Contains(Cp);

  private bool IsOpening => opening.Contains(Cp);

  private bool Literal => Cp == '\'' || Cp == '"';

  private readonly Stack<SearchNode> stack = new();

  private int pos = 0;

  private readonly int length;

  private readonly char?[] white = [' ', '\t', '\n'];

  private readonly char?[] specials = ['(', ')', '[', ']', '=', '<', '>', ',', '+', '-'];

  private readonly char?[] closing = [']', ')'];

  private readonly char?[] opening = ['[', '('];

  private readonly string[] supportedLanguages = ["en-US", "nl-NL", "fr-FR", "da-DK", "de-DE"];
}

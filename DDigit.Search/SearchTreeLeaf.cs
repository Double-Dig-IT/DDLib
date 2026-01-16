namespace DDigit.Search;

public class SearchTreeLeaf : SearchNode
{
  public SearchTreeLeaf(FieldData field, object? value, SearchOperatorEnum @operator, string? language, 
                        List<SearchNode>? indirectionFields)
  {
    Field = field;
    if (value is not null)
    {
      Values.Add(value);
    }
    Operator = @operator;
    Language = language;
    IndirectionFields = indirectionFields;
  }

  public SearchTreeLeaf(FieldData? field, object? value)
  {
    Field = field;
    if (value is not null)
    {
      Values.Add(value);
    }
  }

  public override string ToString() => $"{Field!.Name}{LanguageString}{Indirection} {Operator} {string.Join(", ", Values.Select(v => $"'{v}'"))}";

  public void CopyValuesToSearchTerms()
  {
    Values.ForEach(v =>
    {
      var value = v.ToString();
      if (value != null)
      {
        SearchTerms.Add(value.ToString());
      }
    });
  }

  private string Indirection => IndirectionFields != null && IndirectionFields.Count > 0? 
    $"->{string.Join(FieldData.IndirectionOperator, (IndirectionFields).Select(i => ((SearchTreeLeaf)i).Field!.Name))}" 
    : string.Empty;

  public bool SearchAll => Values.Any((value) => value.ToString() == "*");

  public FieldData? Field { get; private set; }

  public SearchOperatorEnum Operator { get; private set; }

  private string LanguageString => Language != null ? $"[{Language}]" : string.Empty;

  public string? Language { get; private set; }
  public List<SearchNode>? IndirectionFields { get; private set; }
  public List<object> Values { get; private set; } = [];

  public List<string> SearchTerms { get; private set; } = [];

  public string Parameters
  {
    get
    {
      var result = new StringBuilder();
      int i = 0;
      foreach (var value in SearchTerms)
      {
        result.AppendLine($"declare @term{i++} nvarchar({value.Length}) = '{value}'");
      }
      return result.ToString();
    }
  }
}

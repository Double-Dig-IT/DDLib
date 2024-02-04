namespace DDigit.Search;

public class SearchTreeLeaf(FieldData? fieldData, object value,
  SearchOperators @operator = SearchOperators.Equals, string? language = null) : SearchNode
{
  public override string ToString() =>
    $"{Field}{LanguageString} {Operator} {Value}";

  public FieldData? Field = fieldData;

  public SearchOperators Operator = @operator;

  private string LanguageString => Language != null ? $"[{Language}]" : string.Empty;

  public string? Language = language;

  public object Value = value;
}


namespace DDigit.Search;

public class SortField(FieldData field)
{
  public string? Language { get; internal set; }
  public FieldData Field { get; internal set; } = field;
  public SearchSortOrderEnum SortOrder { get; internal set; }
  public override string ToString() => $"{Field.Name} {SortOrder}";
}
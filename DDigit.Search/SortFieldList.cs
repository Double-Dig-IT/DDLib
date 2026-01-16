
namespace DDigit.Search;

public class SortFieldList : List<SortField>
{
  public string ColumList()
   => Count > 0 ? $", {string.Join(", ", this.Select((f, i) => ($"sortkey{i}")))}" : string.Empty;
}

namespace DDigit.Classes;

public class AutoCompleteResult(int startFrom, int limit) : List<AutoCompleteObject>
{
  public int Hits { get; set; }

  public int Limit { get; } = limit;

  public int StartFrom { get; } = startFrom;

  public int Last { get; } = startFrom + limit;
}
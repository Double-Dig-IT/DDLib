namespace DDigit.Search;

public class SearchLimits
{
  public int StartFrom { get; set; } = 1; // Default start from the first record
  public int Limit { get; set; } = 20; // Default limit to 100 records
  public int Count { get; set; } = 0;  // Default count is 0
  public SearchLimits() { }
  public SearchLimits(int startFrom, int limit)
  {
    StartFrom = startFrom;
    Limit = limit;
  }
  public override string ToString() => $"StartFrom: {StartFrom}, Limit: {Limit}";
}

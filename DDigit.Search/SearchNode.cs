namespace DDigit.Search;

public abstract class SearchNode
{
  public int? SampleSize { get; internal set; }

  public int? Seed { get; internal set; }

  public bool? Unique { get; internal set; }
}

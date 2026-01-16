namespace DDigit.Search;

public class SearchTree()
{
  public override string ToString()
  {
    var result = new StringBuilder();

    if (Root is not null)
    {
      result.Append(Root.ToString());
    }

    if (DatasetFilter is not null)
    {
      result.Append(DatasetFilter.ToString());
    }

    if (SortFields.Count > 0)
    {
      result.Append($" sort {string.Join(", ", SortFields)}");
    }

    if (SampleSize.HasValue)
    { 
      result.Append($" random {SampleSize}");
    
      if (Seed.HasValue)
      {
        result.Append($" seed {Seed}");
      }
      if (Unique.HasValue && Unique.Value)
      {
        result.Append(" unique");
      }
    }

    if (StartFrom.HasValue)
    {
      result.Append($" startfrom {StartFrom}");
    }

    if (Limit.HasValue)
    {
      result.Append($" limit {Limit}");
    }
    
    return result.ToString();
  }

  public required SqlStateInfo SqlState { get; set; }
  public required DatabaseData Database { get; set; }
  public DatasetFilter? DatasetFilter { get; set; } = null;
  public string? Statement { get; internal set; }
  public ResultSet? PreviousResults { get; set; }
  public SearchNode? Root { get; set; }
  public SortFieldList SortFields { get; private set; } = [];
  public int? SampleSize { get; set; }
  public int? Seed { get; set; }
  public bool? Unique { get; set; } = false;
  public int? StartFrom { get; internal set; } = null;
  public int? Limit { get; set; } = null;
  public int Milestone { get; set; } = 0;
  public EventHandler<MilestoneEventArgs>? MilestoneReached { get; set; } = null;
}
 
using System.Data;

namespace DDigit.Search;

public class SearchTree()
{
  public override string ToString()
  {
    var result = new StringBuilder();
    if (Root != null)
    {
      result.Append(Root.ToString());
    }

    if (DatasetFilter != null)
    {
      result.Append(DatasetFilter.ToString());
    }

    if (SortFields.Count > 0)
    {
      result.Append($" sort {string.Join(", ", SortFields)}");
    }

    if (SampleSize != 0)
    { 
      result.Append($" random {SampleSize}");
    
      if (Seed != 0)
      {
        result.Append($" seed {Seed}");
      }
      if (Unique)
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

  public required DatabaseData Database { get; set; }
  public DatasetFilter? DatasetFilter { get; set; } = null;
  public string? Statement { get; internal set; }
  public CancellationToken Cancellation { get; set; } = default;
  public ResultSet? PreviousResults { get; set; }
  public SearchNode? Root { get; set; }
  public SortFieldList SortFields { get; private set; } = [];
  public int SampleSize { get; set; } = 0;
  public int Seed { get; set; } = 0;
  public bool Unique { get; set; } = false;
  public int? StartFrom { get; internal set; } = null;
  public int? Limit { get; set; } = null;
  public IDbConnection? Connection { get; set; }
  public int Milestone { get; set; } = 0;
  public EventHandler<MilestoneEventArgs>? MilestoneReached { get; set; } = null;
}
 
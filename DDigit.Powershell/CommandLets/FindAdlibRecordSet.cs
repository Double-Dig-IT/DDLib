using DDigit.Search;

namespace DDigit.Scripting;

/// <summary>
/// Find a set of records based of field and values
/// Search operators are not supported (yet)
/// </summary>
[Cmdlet(VerbsCommon.Find, AdlibNouns.RecordSet)]
public class FindAdlibRecordSet : DDCmdlet
{
  /// <summary>
  /// The database to search in.
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Database
  {
    get; set;
  }

  /// <summary>
  /// The field (tag or name) to search in
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Field
  {
    get; set;
  }

  /// <summary>
  /// The search value.
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Value
  {
    get; set;
  }

  /// <summary>
  /// Optional parameter for dataset
  /// </summary>
  [Parameter]
  public string[]? Dataset
  {
    get; set;
  }

  /// <summary>
  /// Pipeline result of a previous search
  /// </summary>
  [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  public ResultSet? Results
  {
    get; set;
  }

  /// <summary>
  /// Language to search for
  /// </summary>
  [Parameter]
  public string? Language
  {
    get;
    set;
  }

  /// <summary>
  /// Perform the search asynchronously
  /// </summary>
  protected override void ProcessRecord()
  { 
    Result = RunWithEvent(
        () => provider.MilestoneReached += DataProvider_MilestoneChanged,
        () => provider.FindRecordSet(WorkingDirectory, Database, Dataset, Field, Language, Value, Results, default),
        () => provider.MilestoneReached -= DataProvider_MilestoneChanged
    );

    if (SessionState is not null)
    {
      WriteObject(Result);
    }
  }
}




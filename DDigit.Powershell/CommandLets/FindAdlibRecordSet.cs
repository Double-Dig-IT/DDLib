namespace DDigit.PowerShell;

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
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// The field (tag or name) to search in
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Field
  {
    get; set;
  }

  /// <summary>
  /// The search value.
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Value
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
    get; set;
  }

  /// <summary>
  /// Perform the search
  /// </summary>
  protected override void ProcessRecord()
  {
    provider.MilestoneReached += DataProvider_MilestoneChanged;

    Exception? caught = null;
    Task.Run(async () =>
    {
      try
      {
        var result = await provider.FindRecordSet(WorkingDirectory, Database!, Dataset, Field!, Language, Value, Results);
        Result = result;
      }
      catch (Exception ex)
      {
        caught = ex;
      }
    }).Wait();

    if (caught != null)
    {
      throw caught;
    }

    if (SessionState != null)
    {
      WriteObject(Result);
    }
  }
}




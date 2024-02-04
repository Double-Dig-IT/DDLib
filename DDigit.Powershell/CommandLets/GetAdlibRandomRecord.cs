namespace DDigit.PowerShell;

/// <summary>
/// Get a random sample from a database or record set
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.RandomSample)]
[OutputType(typeof(ResultSet))]
public class GetAdlibRandomRecord : DDCmdlet
{
  /// <summary>
  /// The database to retrieve the random set from
  /// </summary>
  [Parameter()]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// Pipeline result of a previous result set
  /// </summary>
  [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  public ResultSet? Results
  {
    get; set;
  }

  /// <summary>
  /// Size of the sample to get
  /// </summary>
  [Parameter()]
  public int Sample
  {
    get; set;
  }

  /// <summary>
  /// Seed to generate the random samples 
  /// </summary>
  [Parameter()]
  public int? Seed
  {
    get; set;
  }

  /// <summary>
  /// Do we want unique records?
  /// </summary>
  [Parameter()]
  public bool Unique
  {
    get; set;
  }

  /// <summary>
  /// Optional parameter for dataset(s)
  /// </summary>
  [Parameter]
  public string[]? Dataset
  {
    get; set;
  }

  protected async override void ProcessRecord()
  {
    provider.MilestoneReached += DataProvider_MilestoneChanged;

    Result = await provider.RandomSample(WorkingDirectory, Database, Dataset, Results, Sample, Seed, Unique);
    if (SessionState != null)
    {
      WriteObject(Result);
    }
  }
}

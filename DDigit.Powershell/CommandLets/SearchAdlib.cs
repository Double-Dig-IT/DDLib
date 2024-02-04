namespace DDigit.PowerShell;

[Cmdlet(VerbsCommon.Search, AdlibNouns.Adlib)]
public class SearchAdlib : DDCmdlet
{
  /// <summary>
  /// The database for which to search
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// The database for which to search
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Statement
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
  /// Optional parameter for milestone
  /// </summary>
  [Parameter]
  public int Milestone
  {
    get; set;
  } = 1000;

  /// <summary>
  /// Pipeline result of a previous search
  /// </summary>
  [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  public ResultSet? Results
  {
    get; set;
  }

  protected override void ProcessRecord()
  {
    provider.MilestoneReached += DataProvider_MilestoneChanged;

    async Task Search()
    {
      Result = await provider.Search(WorkingDirectory, Database!, Dataset, Statement!, Results, Milestone);
    }

    RunAsyncTask(Search);

    if (SessionState != null)
    {
      WriteObject(Result);
    }
  }
}

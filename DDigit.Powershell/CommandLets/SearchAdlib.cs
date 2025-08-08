namespace DDigit.Scripting.CommandLets;

[Cmdlet(VerbsCommon.Search, AdlibNouns.Adlib)]
public class SearchAdlib : DDCmdlet
{
  /// <summary>
  /// The database for which to search
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Database
  {
    get; set;
  }

  /// <summary>
  /// The database for which to search
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Statement
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

  /// <summary>
  /// Perform the search asynchronously
  /// </summary>
  protected override void ProcessRecord()
  {
    Result = RunWithEvent(
        () => provider.MilestoneReached += DataProvider_MilestoneChanged,
        () => provider.SearchAsync(WorkingDirectory, Database, Dataset, Statement, Results, Milestone, default),
        () => provider.MilestoneReached -= DataProvider_MilestoneChanged
    );

    if (SessionState != null)
    {
      WriteObject(Result);
    }
  }

  public static ResultSet? Search(string folder, string database, string statement)
  { 
    var task = SearchAsync(folder, database, statement);
    task.Wait();
    var result = task.Result;
    return result;
  }

  public async static Task<ResultSet?> SearchAsync(string folder, string database, string statement)
  {
    var provider = new DDataProvider(new MSSqlRepository());
    return await provider.SearchAsync(folder, database, null, statement, null, 1000, default);
  }
}

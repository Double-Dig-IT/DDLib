namespace DDigit.PowerShell;

/// <summary>
/// base class for all double digit cmdlets
/// </summary>
public abstract class DDCmdlet : PSCmdlet
{
  /// <summary>
  /// THe storage provider
  /// </summary>
  protected IDataProvider provider = new DDataProvider(new Repository.MSSqlRepository());

  /// <summary>
  /// All cmdlets have na optional Path parameter
  /// </summary>
  [Parameter()]
  public string? Path
  {
    get; set;
  }

  /// <summary>
  /// Get the Working directory, this is either the path, when entered or the current folder
  /// </summary>
  protected string WorkingDirectory => Path ?? new SessionState().Path.CurrentLocation.ToString();


  /// <summary>
  /// Write warning to the PowerShell host
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected void DataProvider_MessageSent(object? sender, MessageEventArgs e)
  {
    if (SessionState != null)
    {
      WriteWarning(e.Message);
    }
  }

  protected void DataProvider_MilestoneChanged(object? sender, MilestoneEventArgs e)
  {
    if (SessionState != null)
    {
      int percentage = (int)((double)e.Milestone / e.Total * 100);
      var progressRecord = new ProgressRecord(1, "Find", $"Reading {e.Milestone:n0} / {e.Total:n0} ({percentage}%), {e.Hits} hits.")
      {
        PercentComplete = percentage
      };
      WriteProgress(progressRecord);
    }
  }

  /// <summary>
  /// Run a test in test explorer
  /// </summary>
  public void RunTest()
  {
    BeginProcessing();
    ProcessRecord();
    EndProcessing();
  }

  /// <summary>
  /// The result of the search
  /// </summary>
  public ResultSet? Result
  {
    get; set;
  }

  protected delegate Task AsyncTask();

  protected static void RunAsyncTask(AsyncTask asyncTask)
  {
    Exception? caught = null;

    Task.Run(async () =>
    {
      try
      {
        await asyncTask();
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
  }
}

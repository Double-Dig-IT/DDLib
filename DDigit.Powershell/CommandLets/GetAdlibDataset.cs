namespace DDigit.PowerShell;

/// <summary>
/// Get a list of datasets
/// </summary>
[Cmdlet(VerbsCommon.Get, "AdlibDataset")]
public class GetAdlibDataset : DDCmdlet
{
  /// <summary>
  /// The database for which to get the dataset
  /// </summary>
  [Parameter()]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetDataset(WorkingDirectory, Database);
    if (SessionState != null)
    {
      foreach (var dataset in result)
      {
        WriteObject(dataset);
      }
    }
  }
}
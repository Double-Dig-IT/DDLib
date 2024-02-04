namespace DDigit.PowerShell;

/// <summary>
/// Get all the data sources from a folder
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.DataSource)]
[OutputType(typeof(DataSourceData))]
public class GetAdlibDataSource : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetDatasource(WorkingDirectory);
    if (SessionState != null)
    {
      foreach (var item in result)
      {
        WriteObject(item);
      }
    }
  }
}

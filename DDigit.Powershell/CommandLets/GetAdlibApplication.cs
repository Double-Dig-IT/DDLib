namespace DDigit.PowerShell;

/// <summary>
/// Get the application from a folder
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.Application)]
[OutputType(typeof(DataSourceData))]
public class GetAdlibApplication : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetApplication(WorkingDirectory);
    if (SessionState != null)
    {
      WriteObject(result);
    }
  }
}

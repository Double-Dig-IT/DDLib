namespace DDigit.PowerShell;

/// <summary>
/// Get the application from a folder
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.Database)]
[OutputType(typeof(DatabaseData))]
public class GetAdlibDatabase : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetDatabase(WorkingDirectory);
    if (SessionState != null)
    {
      WriteObject(result);
    }
  }
}


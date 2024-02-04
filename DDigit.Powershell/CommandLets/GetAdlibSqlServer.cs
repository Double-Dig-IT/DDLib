namespace DDigit.PowerShell;

/// <summary>
/// Get the settings of the SQL server for the databases in a folder.
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.SqlServer)]
[OutputType(typeof(SqlSetting))]
public class GetAdlibSqlServer : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetSqlServer(WorkingDirectory);
    if (SessionState != null)
    {
      foreach (var item in result)
      {
        WriteObject(item);
      }
    }
  }
}

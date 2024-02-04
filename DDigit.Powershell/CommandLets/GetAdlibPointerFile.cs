namespace DDigit.PowerShell;

/// <summary>
/// Get a list of pointer files
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.PointerFile)]
[OutputType(typeof(RecordSetMetaData))]
public class GetAdlibPointerFile : DDCmdlet
{
  /// <summary>
  /// The database to get the pointer files for.
  /// </summary>
  [Parameter()]
  public string? Database
  {
    get; set;
  } = "*";

  /// <summary>
  /// Do the work
  /// </summary>
  protected override async void ProcessRecord()
  {
    var pointerFiles = await provider.GetRecordSetMetaData(WorkingDirectory, Database, null);
    if (SessionState != null)
    {
      foreach (var pointerFile in pointerFiles)
      {
        WriteObject(pointerFile);
      }
    }
  }
}

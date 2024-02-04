namespace DDigit.PowerShell;

/// <summary>
/// Get the record locks in a database
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.RecordLock)]
[OutputType(typeof(RecordLock))]
public class GetAdlibRecordLock : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override async void ProcessRecord()
  {
    var recordLocks = await provider.GetRecordLock(WorkingDirectory);
    if (SessionState != null)
    {
      foreach (var recordLock in recordLocks)
      {
        WriteObject(recordLock);
      }
    }
  }
}

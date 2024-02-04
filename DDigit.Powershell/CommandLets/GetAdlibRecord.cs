namespace DDigit.PowerShell;

/// <summary>
/// Get a single record
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.Record)]
[OutputType(typeof(Record))]
public class GetAdlibRecord : DDCmdlet
{
  /// <summary>
  /// The database to retrieve the record from
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// The record number to retrieve
  /// </summary>
  [Parameter(Mandatory = true)]
  public int Id
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    if (Database != null)
    {
      var record = Read();
      if (SessionState != null)
      {
        WriteObject(record);
      }
    }
  }

  /// <summary>
  /// Read a record
  /// </summary>
  /// <returns>record or null</returns>
  public async Task<Record?> Read() =>
    await provider.ReadRecord(WorkingDirectory, Database!, Id);
}

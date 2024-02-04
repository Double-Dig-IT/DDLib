namespace DDigit.PowerShell;

/// <summary>
/// Create a new record
/// </summary>
[Cmdlet(VerbsCommon.New, AdlibNouns.Record)]
public class NewAdlibRecord : DDCmdlet
{
  /// <summary>
  /// The database to retrieve the record from
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Database
  {
    get; set;
  }

  [Parameter]
  public string? Dataset 
  { 
    get;
    set; 
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    if (Database != null)
    {
      var record = provider.NewRecord(WorkingDirectory, Database, Dataset);
      if (SessionState != null)
      {
        WriteObject(record);
      }
    }
  }

  public Record? New()
  {
    if (Database == null)
    {
      throw new ArgumentNullException(nameof(Database));
    }
    return provider.NewRecord(WorkingDirectory, Database, Dataset);
  }
}

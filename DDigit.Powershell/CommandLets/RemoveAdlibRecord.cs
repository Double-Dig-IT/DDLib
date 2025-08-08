namespace DDigit.Scripting;

/// <summary>
/// Delete an Adlib record
/// </summary>
[Cmdlet(VerbsCommon.Remove, AdlibNouns.Record)]
public class RemoveAdlibRecord : DDCmdlet
{
  /// <summary>
  /// The database to delete the record from
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Database
  {
    get; set;
  }

  [Parameter(Mandatory = true)]
  public required int Id { get; set; }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    async Task Remove()
    {
      await provider.RemoveRecord(WorkingDirectory, Database, Id, default);
    }

    RunAsyncTask(Remove);
  }
}


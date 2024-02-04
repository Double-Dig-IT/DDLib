namespace DDigit.PowerShell;

/// <summary>
/// Get a set of records
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.RecordSet)]
[OutputType(typeof(ResultSet))]
public class GetAdlibRecordSet : DDCmdlet
{
  /// <summary>
  /// Existing results
  /// </summary>
  [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  public ResultSet? Results
  {
    get; set;
  }

  /// <summary>
  /// The database for which to get the set
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// The set number
  /// </summary>
  [Parameter(Mandatory = true)]
  public int Set
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetRecordSet(WorkingDirectory, Database!, Set!);
    if (SessionState != null)
    {
      WriteObject(result);
    }
  }
}

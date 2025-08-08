using DDigit.Search;

namespace DDigit.Scripting;

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
  public required string Database
  {
    get; set;
  }

  /// <summary>
  /// The set number
  /// </summary>
  [Parameter(Mandatory = true)]
  public required int Set
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetRecordSetAsync(WorkingDirectory, Database, Set, CancellationToken.None);
    if (SessionState != null)
    {
      WriteObject(result);
    }
  }
}

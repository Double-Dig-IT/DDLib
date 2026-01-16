namespace DDigit.Scripting.CommandLets;

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

  [Parameter()]
  public int StartFrom
  {
    get; set;
  } = 1;

  [Parameter()]
  public int Limit
  {
    get; set;
  } = 0;

  [Parameter()]
  public string? SearchTerm
  {
    get; set;
  }

  [Parameter()]
  public RecordSetSortEnum? Sort
  {
    get; set;
  }

  [Parameter()]
  public SearchSortOrderEnum? SortOrder
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override async void ProcessRecord()
  {
    var sets = await provider.GetRecordSetMetaDataAsync(WorkingDirectory, Database, null, SearchTerm, StartFrom, Limit, Sort, SortOrder, default);
    if (SessionState != null)
    {
      foreach (var set in sets)
      {
        WriteObject(set);
      }
    }
  }
}

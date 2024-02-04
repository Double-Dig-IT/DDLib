namespace DDigit.PowerShell;

/// <summary>
/// Show the fields of a set of records
/// </summary>
[Cmdlet(VerbsCommon.Show, AdlibNouns.RecordSet)]
public class ShowAdlibRecordSet : DDCmdlet
{
  /// <summary>
  /// Input from the pipeline
  /// </summary>
  [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  public ResultSet? Results
  {
    get; set;
  }

  /// <summary>
  /// The first record to show
  /// </summary>
  [Parameter]
  public int StartFrom
  {
    get; set;
  }

  /// <summary>
  /// The number of records to show
  /// </summary>
  [Parameter]
  public int Limit
  {
    get; set;
  }

  /// <summary>
  /// A list of fields to show
  /// </summary>
  
  [Parameter]
  [Alias(["Fields"])]
  public string[]? Field
  {
    get; set;
  }

  /// <summary>
  /// The data language to show
  /// </summary>
  [Parameter]
  public string Language
  {
    get; set;
  } = "";

  /// <summary>
  /// Do the work
  /// </summary>
  protected async override void ProcessRecord()
  {
    if (Results != null && Results.Database != null)
    {
      var database = Results.Database;
      var folder = Results!.WorkingDirectory!;
      var written = 0;
      var i = 0;
      foreach (var hit in Results.Ids)
      {
        i++;
        if (StartFrom > 0)
        {
          if (i < StartFrom)
          {
            continue;
          }
        }
        var record = await provider.ReadRecord(folder, database, hit);
        OutputRecord(record);
        written++;
        if (Limit > 0 && written == Limit)
        {
          break;
        }
      }
    }
  }

  private void OutputRecord(Record? record)
  {
    if (Field != null && record != null)
    {
      var responseObject = new PSObject(64);
      foreach (var field in Field)
      {
        if (field != null)
        {
          responseObject.Members.Add(new PSNoteProperty(field, record.GetDataAsync(field, 1, Language)?.ToString()));
        }
      }
      if (SessionState != null)
      {
        WriteObject(responseObject);
      }
    }
    else
    {
      if (SessionState != null)
      {
        WriteObject(record);
      }
    }
  }
}

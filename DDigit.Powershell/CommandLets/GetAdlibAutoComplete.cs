namespace DDigit.PowerShell;

[Cmdlet(VerbsCommon.Get, AdlibNouns.Autocomplete)]
public class GetAdlibAutoComplete : DDCmdlet
{
  /// <summary>
  /// The database for which to get suggestions
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// The dataset(s) for which to get suggestions
  /// </summary>
  [Parameter()]
  public string[]? Dataset
  {
    get; set;
  }

  /// <summary>
  /// The field for which to get the suggestions
  /// </summary>
  [Parameter(Mandatory = true)]
  public string[]? Field
  {
    get; set;
  }

  /// <summary>
  /// The start value for the suggestions
  /// </summary>
  [Parameter]
  public string? Value
  {
    get; set;
  } = string.Empty;

  /// <summary>
  /// The first suggestion to show
  /// </summary>
  [Parameter]
  public int? StartFrom
  {
    get; set;
  }

  /// <summary>
  /// The number of suggestions to show
  /// </summary>
  [Parameter]
  public int? Limit
  {
    get; set;
  }

  [Parameter]
  public string? Language
  {
    get; set;
  }

  [Parameter]
  public bool Count
  {
    get; set;
  } = true;

  public new AutoCompleteResult? Result { get; private set; } 

  protected override void ProcessRecord()
  {
    async Task AutoComplete()
    {
      Result = await provider.GetAutoComplete(WorkingDirectory, Database!, Dataset, Field!, Value, StartFrom, Limit, Language, Count);
    }

    RunAsyncTask(AutoComplete);

    if (SessionState != null)
    {
      WriteObject(Result);
    }
  }
}

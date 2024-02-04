namespace DDigit.PowerShell;

/// <summary>
/// Get the forms from a folder
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.Form)]
[OutputType(typeof(FormData))]
public class GetAdlibForm : DDCmdlet
{
  /// <summary>
  /// The name of the form
  /// </summary>
  public string? FormName
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetForm(WorkingDirectory, FormName);
    if (SessionState != null)
    {
      WriteObject(result);
    }
  }
}

namespace DDigit.PowerShell;

/// <summary>
/// Get the paths of image fields.
/// </summary>
[Cmdlet(VerbsCommon.Get, "AdlibImagePath")]
[OutputType(typeof(ImageData))]
public class GetAdlibImagePath : DDCmdlet
{
  /// <summary>
  /// The database for which to retrieve the field information.
  /// </summary>
  [Parameter()]
  public string? Database
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var imagePaths = provider.GetImagePath(WorkingDirectory, Database);
    if (SessionState != null)
    {
      foreach (var imagePath in imagePaths)
      {
        WriteObject(imagePath);
      }
    }
  }
}
namespace DDigit.PowerShell;

/// <summary>
/// Set n Adlib user
/// </summary>
[Cmdlet(VerbsCommon.Set, AdlibNouns.User)]
public class SetAdlibUser : DDCmdlet
{
  /// <summary>
  /// The user to set
  /// </summary>
  [Parameter(Mandatory = true)]
  public string? User
  {
    get; set;
  }

  /// <summary>
  /// The role to set
  /// </summary>
  [Parameter()]
  public string? Role
  {
    get; set;
  }

  /// <summary>
  /// The password to set
  /// </summary>
  [Parameter()]
  public string? Password
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    provider.SetUser(WorkingDirectory, User!, Role, Password);
  }
}

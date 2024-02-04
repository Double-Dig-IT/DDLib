namespace DDigit.PowerShell;

/// <summary>
/// Get a list of users
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.User)]
public class GetAdlibUser : DDCmdlet
{
  /// <summary>
  /// Filter on user
  /// </summary>
  [Parameter()]
  public string? User
  {
    get; set;
  }

  /// <summary>
  /// Filter on role
  /// </summary>
  [Parameter()]
  public string? Role
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var users = provider.GetUser(WorkingDirectory, User, Role);
    if (SessionState != null)
    {
      foreach (var user in users)
      {
        WriteObject(user);
      }
    }
  }
}

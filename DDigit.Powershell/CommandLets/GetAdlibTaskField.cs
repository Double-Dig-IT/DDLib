namespace DDigit.PowerShell;

/// <summary>
/// Get the properties of a field in a Task
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.TaskField)]
public class GetAdlibTaskField : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var fields = provider.GetTaskField(WorkingDirectory);
    if (SessionState != null)
    {
      foreach (var field in fields)
      {
        WriteObject(field);
      }
    }
  }
}


namespace DDigit.PowerShell;

/// <summary>
/// Get a list of tasks
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.Task)]
[OutputType(typeof(TaskData))]
public class GetAdlibTask : DDCmdlet
{
  /// <summary>
  /// Do the work.
  /// </summary>
  protected override void ProcessRecord()
  {
    var tasks = provider.GetTask(WorkingDirectory);
    if (SessionState != null)
    {
      foreach (var task in tasks)
      {
        WriteObject(task);
      }
    }
  }
}

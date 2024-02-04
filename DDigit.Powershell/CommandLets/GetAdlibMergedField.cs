namespace DDigit.PowerShell;

[Cmdlet(VerbsCommon.Get, "AdlibMergedField")]
public class GetAdlibMergedField : DDCmdlet
{
  protected override void ProcessRecord()
  {
    var fields = provider.GetMergedField(WorkingDirectory);
    if (SessionState != null)
    {
      foreach (var field in fields)
      {
        WriteObject(field);
      }
    }
  }
}

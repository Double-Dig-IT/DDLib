namespace DDigit.PowerShell;

/// <summary>
/// <para type="synopsis">Retrieve information about Adlib fields.</para>
/// </summary>
[Cmdlet(VerbsCommon.Get, "AdlibField")]
[OutputType(typeof(FieldData))]
public class GetAdlibField : DDCmdlet
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
  /// The tag for which to retrieve the field information.
  /// </summary>
  [Parameter()]
  public string? Tag
  {
    get; set;
  }

  /// <summary>
  /// Type filter
  /// </summary>
  [Parameter()]
  public FieldTypeEnum? Type
  {
    get; set;
  }

  /// <summary>
  /// Filter only linked fields
  /// </summary>
  [Parameter()]
  public bool? IsLinked
  {
    get; set;
  }

  /// <summary>
  /// Filter on group
  /// </summary>
  [Parameter()]
  public string? Group
  {
    get; set;
  }

  /// <summary>
  /// Filter on tag or field name
  /// </summary>
  [Parameter()]
  public string? Field
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var result = provider.GetField(WorkingDirectory, Database, Tag, Type, IsLinked, Field, Group);
    if (SessionState != null)
    {
      foreach (var field in result)
      {
        WriteObject(field);
      }
    }
  }
}

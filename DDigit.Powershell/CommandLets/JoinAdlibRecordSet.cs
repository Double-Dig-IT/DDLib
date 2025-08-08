using DDigit.Search;

namespace DDigit.Scripting;

/// <summary>
/// Perform a boolean operator on two sets.
/// </summary>
[Cmdlet(VerbsCommon.Join, "AdlibRecordSet")]
public class JoinAdlibRecordSet : DDCmdlet
{

  /// <summary>
  /// The boolean operator to use
  /// </summary>
  [Parameter()]
  public BooleanOperator Operator
  {
    get; set;
  } = BooleanOperator.And;

  /// <summary>
  /// The left hand set
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  public required ResultSet Left
  {
    get; set;
  }

  /// <summary>
  /// The right hand side set
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  public required ResultSet Right
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    if (Left == null)
    {
      throw new ArgumentNullException(nameof(Left));
    }

    if (Right == null)
    {
      throw new ArgumentNullException(nameof(Right));
    }

    var result = provider.JoinRecordSet(Left, Operator, Right);
    if (SessionState != null)
    {
      WriteObject(result);
    }
  }
}

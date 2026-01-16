namespace DDigit.Results;


/// <summary>
/// Describes a single data issue
/// </summary>
public sealed record Issue
{
  /// <summary>
  /// Severity
  /// </summary>
  public MessageSeverityEnum Severity { get; set; }
  /// <summary>
  /// Type of error.
  /// </summary>
  public ErrorCodeEnum ErrorCode { get; set; } = ErrorCodeEnum.NoError;
  /// <summary>
  /// Value that caused the error.
  /// </summary>
  public required object OffendingValue { get; set; }
  /// <summary>
  /// Printable verion of the issue.
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{Severity}: {ErrorCode}, {OffendingValue}";
}

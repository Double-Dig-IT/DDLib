namespace DDigit.MetaData;

/// <summary>
/// Indicates the repeatability of a field
/// </summary>
public enum RepeatabilityEnum : short
{
  /// <summary>
  /// The field is not repeatable
  /// </summary>
  NotRepeated = 0,

  /// <summary>
  /// The field is repeated
  /// </summary>
  Repeated = 1,

  /// <summary>
  /// The field is repeated with unique values
  /// </summary>
  RepeatedUnique = 2
}

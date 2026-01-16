namespace DDigit.MetaData;

/// <summary>
/// Types of conditions
/// </summary>
public enum ConditionEnum : short
{
  /// <summary>
  /// Values must be equal
  /// </summary>
  Equal = 0,

  /// <summary>
  /// Values must not be equal
  /// </summary>
  Unequal = 1,

  /// <summary>
  /// Value must match a regular expression
  /// </summary>
  RegularExpression = 2
}

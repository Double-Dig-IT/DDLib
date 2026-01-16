
namespace DDigit.MetaData;

/// <summary>
/// Condition boolean operators
/// </summary>
public enum ConditionBooleanEnum : short
{
  /// <summary>
  /// and
  /// </summary>
  And = 0,

  /// <summary>
  /// or
  /// </summary>
  Or = 1,

  /// <summary>
  /// not
  /// </summary>
  Not = 2,

  /// <summary>
  /// and with same occurrence
  /// </summary>
  When = 3
}

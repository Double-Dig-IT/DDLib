namespace DDigit.MetaData;

/// <summary>
/// Determines whether for auto numbering field the user is allowed to enter numbers "by hand"
/// </summary>
public enum AutoNumberingAllowManualAssignmentEnum
{
  /// <summary>
  /// Undefined
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// The user cannot enter a number and always the calculated number will be used
  /// </summary>
  No = 1,

  /// <summary>
  /// The user can enter data manually, only when the record is stored and no data is presen
  /// the automatic assignment takes place
  /// </summary>
  Yes = 2
}
namespace DDigit.MetaData;

/// <summary>
/// Defines when numbers are defined inautomatically numbering fields
/// </summary>
public enum AutoNumberAssignmentEnum
{
  /// <summary>
  /// Undefined, no numbers are aassigned
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// Never, no numbers are assigned
  /// </summary>
  Never = 1,
  
  /// <summary>
  /// During input or edit
  /// The number is calculated when the user goes into edit mode and the field is not empty
  /// The result is that the user will see the generated number
  /// A disadvantage is that when tje user cancels the inputor edit the number will not be released.
  /// </summary>
  DuringInputOrEdit = 2,

  /// <summary>
  /// After input or editing when a user saves the record.
  /// The user can only see the number once the record is saved.
  /// If the transaction fails no number is used.
  /// </summary>
  BeforeStorage = 3,

  /// <summary>
  /// When a digital asset is assigned to the record.
  /// </summary>
  OnDigitalAsset = 4
}
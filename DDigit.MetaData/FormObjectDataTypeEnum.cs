namespace DDigit.MetaData;

/// <summary>
/// The type of data in a field.
/// This is old style validation, data dictionary validation is preferred.
/// </summary>
public enum FormObjectDataTypeEnum : short
{
  /// <summary>
  /// Not defined
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// Any text is valid
  /// </summary>
  Text = 1,

  /// <summary>
  /// Only letters (A-Z = a-z) are allowed
  /// </summary>
  Letters = 2,

  /// <summary>
  /// Floating point number 
  /// </summary>
  Numeric = 3,

  /// <summary>
  /// Integer value
  /// </summary>
  Integer = 4,

  /// <summary>
  /// Date is allowed
  /// </summary>
  Date = 5,

  /// <summary>
  /// Time is allowed
  /// </summary>
  Time = 6,

  /// <summary>
  /// ISBN (International Standard Book Number)is allowed
  /// </summary>
  ISBN = 7,

  /// <summary>
  /// ISSN (International Standard Serial Number)is allowed
  /// </summary>
  ISSN = 8
}

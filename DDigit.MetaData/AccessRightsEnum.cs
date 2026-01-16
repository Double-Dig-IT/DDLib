namespace DDigit.MetaData;

/// <summary>
/// The different types of access rights.
/// </summary>
public enum AccessRightsEnum
{
  /// <summary>
  /// Undefined or unknown
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// The user has no access
  /// </summary>
  None = 1,

  /// <summary>
  /// The user has read only access.
  /// </summary>
  Read = 2,

  /// <summary>
  /// The user can read and write.
  /// </summary>
  Write = 3,

  /// <summary>
  /// The user has full access, read, write and delete
  /// </summary>
  Full = 4
}
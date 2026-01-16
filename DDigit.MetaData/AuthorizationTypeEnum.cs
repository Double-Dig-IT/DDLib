namespace DDigit.MetaData;

/// <summary>
/// Different record authorization methods.
/// </summary>
public enum AuthorizationTypeEnum
{
  /// <summary>
  /// No authorization is used
  /// </summary>
  None = 0,

  /// <summary>
  /// Exclude certain users from access to records
  /// Permit access to the rest of the world
  /// </summary>
  Exclude = 1,

  /// <summary>
  /// Deny access to the record for everyone, 
  /// Except a specific list of users 
  /// </summary>
  Include = 2,

  /// <summary>
  /// Record in the record itself who has what rights to it.
  /// </summary>
  Record = 3
}

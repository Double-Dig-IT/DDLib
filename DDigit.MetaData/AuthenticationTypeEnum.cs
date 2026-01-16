namespace DDigit.MetaData;

/// <summary>
/// How are we authenticating users
/// </summary>
public enum AuthenticationTypeEnum
{
  /// <summary>
  /// No authentication
  /// </summary>
  None = 0,

  /// <summary>
  /// User name, password and roles are stored in the adlib.pbk file.
  /// </summary>
  AdlibPbk = 1,

  /// <summary>
  /// The user name, role and passwords are stored in an Adlib Database.
  /// </summary>
  AdlibDatabase = 2,

  /// <summary>
  /// We use Active directory
  /// </summary>
  ActiveDirectory = 3,

  /// <summary>
  /// Use some web service for autheticating users.
  /// </summary>
  Http = 4
}
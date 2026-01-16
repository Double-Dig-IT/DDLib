namespace DDigit.MetaData;

/// <summary>
/// How do we implement 2 factor authentication?
/// </summary>
public enum TwoFactorMethodEnum
{
  /// <summary>
  /// No 2 factor authentication
  /// </summary>
  None = 0,

  /// <summary>
  /// Send an email
  /// </summary>
  Email = 1,

  /// <summary>
  /// Use a mobile app
  /// </summary>
  MobileApp = 2,

  /// <summary>
  /// Send a text message to a phone
  /// </summary>
  SMS = 3
}

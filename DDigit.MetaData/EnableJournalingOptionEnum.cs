namespace DDigit.MetaData;

public enum EnableJournalingOptionEnum
{
  /// <summary>
  /// Journaling not switched on
  /// </summary>
  Disabled = 0,

  /// <summary>
  /// The old method that was used in Adlib for Windows
  /// </summary>
  Legacy = 1,

  /// <summary>
  /// A new method of logging that is used in Axiell Collections
  /// </summary>
  RecordHistory = 2
}
namespace DDigit.MetaData;

/// <summary>
/// What kind of algorithm will be used for phonetic type searches
/// </summary>
[Adlib("No longer needed")]
public enum PhoneticTypeEnum : short
{
  /// <summary>
  /// No algorithm
  /// </summary>
  None = 0,

  /// <summary>
  /// Use the double metaphone algorithm
  /// cref="https://en.wikipedia.org/wiki/Metaphone#Double_Metaphone"
  /// </summary>
  DoubleMetaPhone = 1
}
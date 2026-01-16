namespace DDigit.MetaData;

/// <summary>
/// Character sets used in various software versions
/// </summary>
public enum CharacterSetEnum
{
  /// <summary>
  /// The MS-DOS character set
  /// </summary>
  [Adlib("For the dos version")]
  DOS = 0,

  /// <summary>
  /// Ansi character set
  /// </summary>
  [Adlib("MS-DOS and early Windows versions")]
  ANSI = 1,

  /// <summary>
  /// Unicode
  /// </summary>
  Utf8 = 2
}
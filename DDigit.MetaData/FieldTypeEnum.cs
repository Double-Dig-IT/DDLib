namespace DDigit.MetaData;

/// <summary>
/// The different field types for Adlib / Axiell Collections / Double Digit Collection Suite
/// </summary>
public enum FieldTypeEnum
{
  /// <summary>
  /// No restrictions
  /// </summary>
  Default = 0,

  /// <summary>
  /// Text field
  /// </summary>
  Text = 1,

  /// <summary>
  /// Only letters a-z and A-Z are allowed
  /// </summary>
  Letters = 2,

  /// <summary>
  /// Floating point number, with a . as decimal separator
  /// </summary>
  Float = 3,

  /// <summary>
  /// 32 bit signed integer
  /// </summary>
  Integer = 4,

  /// <summary>
  /// General date format
  /// </summary>
  DateGeneral = 5,

  /// <summary>
  /// American dates mm/dd/yyyy
  /// </summary>
  DateUsa = 6,

  /// <summary>
  /// European dates dd-mm-yyyy
  /// </summary>
  DateEuropean = 7,

  /// <summary>
  /// ISO dates yyyy-mm-dd
  /// </summary>
  DateIso = 8,

  /// <summary>
  /// Time 24 hours hh:mm:ss
  /// </summary>
  Time = 9,

  /// <summary>
  /// ISBN number
  /// </summary>
  ISBN = 10,

  /// <summary>
  /// ISSN number
  /// </summary>
  ISSN = 11,

  /// <summary>
  /// Boolean, x = true
  /// </summary>
  Boolean = 12,

  /// <summary>
  /// An lookup list
  /// </summary>
  Enumerate = 13,

  /// <summary>
  /// Application field, contains a file path with extension.
  /// The extension determines which application will be opened
  /// </summary>
  Application = 14,

  /// <summary>
  /// Field contains an identifier for an image (not the image data itself)
  /// </summary>
  Image = 15,

  /// <summary>
  /// Temporary field, any content in this field is never stored
  /// </summary>
  Temporary = 16,

  /// <summary>
  /// Rich Text Field
  /// </summary>
  Rtf = 17,

  /// <summary>
  /// HTML field, field contains HTML code that can be rendered
  /// </summary>
  Html = 18,

  /// <summary>
  /// A Date period (sart and end)
  /// </summary>
  DatePeriod = 19,

  /// <summary>
  /// Some group
  /// </summary>
  Group = 20,

  /// <summary>
  /// Geo Coordinates
  /// </summary>
  GeoLocation = 21,

  /// <summary>
  /// A password
  /// </summary>
  Password = 22,

  /// <summary>
  /// A field for URI's
  /// </summary>
  URI = 23,

  /// <summary>
  /// Geo JSON
  /// </summary>
  GEOJson = 24,
}
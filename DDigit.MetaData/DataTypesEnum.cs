namespace DDigit.MetaData;

/// <summary>
/// The different properties that object in the setup can have.
/// </summary>
public enum DataTypesEnum
{
  /// <summary>
  /// String
  /// </summary>
  String,

  /// <summary>
  /// A boolean as a short
  /// </summary>
  Bool,

  /// <summary>
  /// Short integer 
  /// </summary>
  Int16,

  /// <summary>
  /// An enumerated list as a short
  /// </summary>
  Enum,
  
  /// <summary>
  /// An enumerated list as an int
  /// </summary>
  Enum32,

  /// <summary>
  /// An invered boolean 1 = false, 0 = true (as a short)
  /// </summary>
  BoolI,

  /// <summary>
  /// int
  /// </summary>
  Int32,

  /// <summary>
  /// This property is not used (anymore)
  /// </summary>
  Skip,

  /// <summary>
  /// A Global Unique Identifier
  /// </summary>
  Guid,

  /// <summary>
  /// A boolen as an int
  /// </summary>
  Bool32,

  /// <summary>
  /// An inversed boolean as an int
  /// </summary>
  Bool32I,

  /// <summary>
  /// An unsigned int
  /// </summary>
  UInt32,

  /// <summary>
  /// A 32 bit float
  /// </summary>
  Float
}

namespace DDigit.MetaData;

/// <summary>
/// Different types of internal links
/// </summary>
public enum RelationTypeEnum : short
{
  /// <summary>
  /// Default value, not known
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// Hierarchical relationship, like broader / narrower term or parent / child
  /// </summary>
  Hierarchical = 1,

  /// <summary>
  /// (Loosly) Related 
  /// </summary>
  Related = 2,

  /// <summary>
  /// Two completely equivalent terms
  /// </summary>
  Equivalence = 3,

  /// <summary>
  /// Use / Used for relationship
  /// </summary>
  Preference = 4,

  /// <summary>
  /// Terms split in their semantic factors
  /// </summary>
  SemanticFactor = 5,

  /// <summary>
  /// Pseudonyms, as in writer names
  /// </summary>
  Pseudonym = 6
}
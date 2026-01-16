namespace DDigit.MetaData;

/// <summary>
/// Interface for the propertymap
/// </summary>
public interface IHasPropertyMap<TSelf> where TSelf : IHasPropertyMap<TSelf>
{
  /// <summary>
  /// The list of Properties
  /// </summary>
  static abstract IReadOnlyList<PropertyMap> Properties { get; }
}

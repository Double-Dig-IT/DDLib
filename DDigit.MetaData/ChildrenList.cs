namespace DDigit.MetaData;

/// <summary>
/// A list of child objects for the parent
/// </summary>
public class ChildrenList(IEnumerable<BaseData> objects, IReadOnlyList<PropertyMap> properties)
{
  /// <summary>
  /// The list object for which to write the data.
  /// </summary>
  public IEnumerable<BaseData> Objects { get; private set; } = objects;

  /// <summary>
  /// A list of properties for these objects
  /// </summary>
  public IReadOnlyList<PropertyMap> Properties { get; private set; } = properties;

  /// <summary>
  /// For debugging
  /// </summary>
  /// <returns></returns>
  public override string ToString()
    => Objects is null || !Objects.Any() ? "Empty list" : $"{objects.ElementAt(0).GetType().Name}";
}

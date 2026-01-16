namespace DDigit.MetaData;

/// <summary>
/// A list of properties for an object
/// </summary>
public class PropertyList : List<PropertyMap>, IReadOnlyList<PropertyMap>
{
  /// <summary>
  ///  Retrieve the Element count for a certain property
  /// </summary>
  /// <param name="propertyName">Name of the property</param>
  /// <returns>Element count / index </returns>
  /// <exception cref="DDException"></exception>
  public short GetElementCount(string propertyName)
  {
    var elementIndex = this.FirstOrDefault(p => p.Name == propertyName)?.ElementIndex;
    if (!elementIndex.HasValue)
    {
      throw new DDException($"Element index not found for property '{propertyName}'");
    }
    return elementIndex.Value;
  }
}

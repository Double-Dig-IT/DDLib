namespace DDigit.MetaData;

public class PropertyList : List<PropertyMap>
{
  public short GetElementCount(string propertyName)
  {
    var elementIndex = this.FirstOrDefault(p => p.Name == propertyName)?.ElementIndex;
    if (!elementIndex.HasValue)
    {
      throw new DDException($"Element index not found for property '{propertyName}'");
    }
    return (elementIndex.Value);
  }
}

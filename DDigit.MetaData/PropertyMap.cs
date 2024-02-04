namespace DDigit.MetaData;

public class PropertyMap
{
  internal PropertyMap(short element, DataTypesEnum dataType, string? name = null, Type? type = null)
  {
    Element = element;
    Type = type;
    DataType = dataType;
    Name = name;
  }

  internal short Element
  {
    get; private set;
  }

  internal Type? Type
  {
    get; private set;
  }

  public DataTypesEnum DataType
  {
    get; private set;
  }

  internal string? Name
  {
    get; private set;
  }

  public long? Position
  {
    get;
    internal set;
  }

  public override string ToString() => $"{Name} {DataType} {Position}";

}

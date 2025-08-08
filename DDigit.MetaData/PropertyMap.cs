namespace DDigit.MetaData;

public class PropertyMap
{
  internal PropertyMap(short elementIndex, DataTypesEnum dataType, string? name = null, Type? type = null)
  {
    ElementIndex = elementIndex;
    Type = type;
    DataType = dataType;
    Name = name;
  }

  internal short ElementIndex
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

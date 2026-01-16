namespace DDigit.MetaData;

/// <summary>
/// A single element from a property map
/// </summary>
public sealed record PropertyMap
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
    get; 
    private set;
  }

  internal Type? Type
  {
    get; 
    private set;
  }

  /// <summary>
  /// The data type of the property
  /// </summary>
  public DataTypesEnum DataType
  {
    get;
    private set;
  }

  internal string? Name
  {
    get;
    private set;
  }

  /// <summary>
  /// The current position in the binary file.
  /// </summary>
  public long? Position
  {
    get;
    internal set;
  }

  /// <summary>
  /// Nice override for debugging.
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{Name} Type={DataType} ElementIndex = {ElementIndex}, Position={Position}";
}

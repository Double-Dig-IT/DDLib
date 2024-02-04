namespace DDigit.MetaData;

public class LinkNodeData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The name of the node
  /// </summary>
  public string? Name
  {
    get; private set;
  }

  /// <summary>
  /// The global unique identifier of this node.
  /// </summary>
  public Guid? ID
  {
    get; private set;
  }

  /// <summary>
  /// The id of the parent Node.
  /// </summary>
  public Guid? ParentID
  {
    get; private set;
  }

  public List<LinkNodeData> ChildNodes
  {
    get;
    private set;
  } = [];

  public override string ToString() => $"{Name} {ID}";

  private static readonly PropertyList Properties =
  [
    new PropertyMap(0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap(1, DataTypesEnum.String, "Name"),
    new PropertyMap(2, DataTypesEnum.Guid,   "ID"),
    new PropertyMap(3, DataTypesEnum.Guid,   "ParentID")
  ];
}
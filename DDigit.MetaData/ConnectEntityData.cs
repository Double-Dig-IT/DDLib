namespace DDigit.MetaData;

public class ConnectEntityData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace)
{
  public string Id
  {
    get; private set;
  } = Guid.NewGuid().ToString();

  public string? DataSourceId
  {
    get; private set;
  }

  public string? SourceField
  {
    get; private set;
  }

  public string? DestinationField
  {
    get; private set;
  }

  /// <summary>
  /// Labels for this connection entity
  /// </summary>
  public TextsList Texts
  {
    get;
  } = [];


  /// <summary>
  /// Access Rights for this connection
  /// </summary>
  public AccessControlList Rights
  {
    get;
  } = [];

  internal void Add(LanguageTextData text) => Texts.Add(text);

  internal void Add(AccessRightsData rights) => Rights.Add(rights);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,   "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String,  "Id"),
    new PropertyMap (2,  DataTypesEnum.String,  "DataSourceId"),
    new PropertyMap (3,  DataTypesEnum.String,  "SourceField"),
    new PropertyMap (4,  DataTypesEnum.String,  "DestinationField"),
  ];

  internal override ChildrenList[] Children =>
  [
    new ChildrenList(Texts, LanguageTextData.Properties),
    new ChildrenList(Rights, AccessRightsData.Properties),
  ];
}
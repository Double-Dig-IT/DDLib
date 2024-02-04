namespace DDigit.MetaData;

public class ConnectEntityData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
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

  public List<LanguageTextData> Texts
  {
    get;
  } = [];


  public List<AccessRightsData> Rights
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

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
    (LanguageTextData.Properties, Texts),
    (AccessRightsData.Properties, Rights),
  ];
}
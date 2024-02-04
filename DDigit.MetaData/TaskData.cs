namespace DDigit.MetaData;

public class TaskData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? ScreenName
  {
    get;
    private set;
  }

  public string? ScriptName
  {
    get;
    private set;
  }

  public string? Id
  {
    get;
    private set;
  } = Guid.NewGuid().ToString();

  public List<LanguageTextData> Texts
  {
    get;
    internal set;
  } = [];

  public List<AccessRightsData> Rights
  {
    get;
    internal set;
  } = [];

  public List<FieldData> Fields
  {
    get;
    internal set;
  } = [];


  public override string? ToString() => Id;

  internal void Add(AccessRightsData rights) => Rights.Add(rights);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,   "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String,  "ScreenName"),
    new PropertyMap (2,  DataTypesEnum.String,  "ScriptName"),
    new PropertyMap (3,  DataTypesEnum.String,  "Id"),
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
    (LanguageTextData.Properties, Texts),
    (AccessRightsData.Properties, Rights),
    (FieldData.Properties, Fields)
  ];
}
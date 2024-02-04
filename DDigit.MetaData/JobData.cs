namespace DDigit.MetaData;

public class JobData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? Adapl
  {
    get; protected set;
  }

  public string? Template
  {
    get; protected set;
  }

  public string? Title
  {
    get; protected set;
  }

  public string? Comment
  {
    get; protected set;
  }

  public TemplateTypeEnum TemplateType
  {
    get; protected set;
  }

  /// <summary>
  /// A ; separated list of templates
  /// </summary>
  public string? Templates
  {
    get; protected set;
  }

  public XmlTypeEnum XmlType
  {
    get; protected set;
  }

  public string? PrintServiceUrl
  {
    get; protected set;
  }

  public List<LanguageTextData> Texts
  {
    get; private set;
  } = [];

  public List<LanguageTextData> Descriptions
  {
    get; private set;
  } = [];

  public List<AccessRightsData> AccessRights
  {
    get; private set;
  } = [];

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,   "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String,  "Adapl"),
    new PropertyMap (2,  DataTypesEnum.Skip),
    new PropertyMap (3,  DataTypesEnum.String,  "Template"),
    new PropertyMap (4,  DataTypesEnum.String,  "Title"),
    new PropertyMap (5,  DataTypesEnum.String,  "Comment"),
    new PropertyMap (6,  DataTypesEnum.Enum,    "TemplateType", typeof(TemplateTypeEnum)),
    new PropertyMap (7,  DataTypesEnum.Skip),
    new PropertyMap (8,  DataTypesEnum.Skip),
    new PropertyMap (9,  DataTypesEnum.String,  "Templates"),
    new PropertyMap (10, DataTypesEnum.Enum,    "XmlType", typeof (XmlTypeEnum)),
    new PropertyMap (11, DataTypesEnum.Skip),
    new PropertyMap (12, DataTypesEnum.String,  "PrintServiceUrl")
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
    (AccessRightsData.Properties, AccessRights),
    (LanguageTextData.Properties, Texts),
    (LanguageTextData.Properties, Descriptions)
  ];


}
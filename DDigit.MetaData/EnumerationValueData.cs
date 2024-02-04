namespace DDigit.MetaData;

public class EnumerationValueData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The neutral value for this field
  /// </summary>
  public string? NeutralValue
  {
    get;
    private set;
  }

  /// <summary>
  /// Th language dependent texts.
  /// </summary>
  public List<LanguageTextData> Texts
  {
    get; private set;
  } = [];

  /// <summary>
  /// The access rights for enumeration value.
  /// </summary>
  public List<AccessRightsData> AccessRights
  {
    get;
    internal set;
  } = [];


  /// <summary>
  /// Where is this used?
  /// </summary>
  public List<AccessRightsData> RecordTypeAccessRights
  {
    get; internal set;
  } = [];

  public override string? ToString() => NeutralValue;

  internal void Add(LanguageTextData languageTextData) => Texts.Add(languageTextData);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String, "NeutralValue"),
    new PropertyMap (2,  DataTypesEnum.Skip),
    new PropertyMap (3,  DataTypesEnum.Skip),
    new PropertyMap (4,  DataTypesEnum.Skip),
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
      (LanguageTextData.Properties, Texts),
      (AccessRightsData.Properties, AccessRights),
      (AccessRightsData.Properties, RecordTypeAccessRights),
  ];
}

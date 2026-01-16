namespace DDigit.MetaData;

/// <summary>
/// A clas that holds information about an enumeration value, the neutral value
/// and its text in various languages.
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>
public class EnumerationValueData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace)
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
  public TextsList Texts
  {
    get; private set;
  } = [];

  /// <summary>
  /// The access rights for enumeration value.
  /// </summary>
  public AccessControlList AccessRights
  {
    get;
    internal set;
  } = [];


  /// <summary>
  /// Where is this used?
  /// </summary>
  public AccessControlList RecordTypeAccessRights
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// ToString() method, handy for debugging.
  /// </summary>
  /// <returns></returns>
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

  internal override ChildrenList[] Children =>
  [
      new ChildrenList(Texts, LanguageTextData.Properties),
      new ChildrenList(AccessRights, AccessRightsData.Properties),
      new ChildrenList(RecordTypeAccessRights, AccessRightsData.Properties),
  ];
}

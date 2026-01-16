namespace DDigit.MetaData;

/// <summary>
/// Task parametere
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>
public class TaskData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace), IHasFields
{
  /// <summary>
  /// The screen used for this task
  /// </summary>
  public string? ScreenName
  {
    get;
    private set;
  }

  /// <summary>
  /// The script used in this task
  /// </summary>
  public string? ScriptName
  {
    get;
    private set;
  }

  /// <summary>
  /// Guid for the task
  /// </summary>
  public string? Id
  {
    get;
    private set;
  } = Guid.NewGuid().ToString();

  /// <summary>
  /// Text list describing the task
  /// </summary>
  public TextsList Texts
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// Access Control List
  /// </summary>
  public AccessControlList Rights
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of fields for the task
  /// </summary>
  public FieldList Fields
  {
    get;
    internal set;
  } = [];


  /// <summary>
  /// Override for ToString (debugging)
  /// </summary>
  /// <returns></returns>
  public override string? ToString() => Id;

  internal void Add(AccessRightsData rights) => Rights.Add(rights);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,   nameof(ElementCount)),
    new PropertyMap (1,  DataTypesEnum.String,  nameof(ScreenName)),
    new PropertyMap (2,  DataTypesEnum.String,  nameof(ScriptName)),
    new PropertyMap (3,  DataTypesEnum.String,  nameof(Id)),
  ];

  internal override ChildrenList[] Children =>
  [
    new ChildrenList(Texts, LanguageTextData.Properties),
    new ChildrenList(Rights, AccessRightsData.Properties),
    new ChildrenList(Fields, FieldData.Properties)
  ];
}
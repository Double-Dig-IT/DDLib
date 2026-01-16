namespace DDigit.MetaData;

/// <summary>
/// The data for a method.
/// </summary>
public class MethodData : BaseData, IHasScreens
{
  /// <summary>
  /// Constructor that read the data from a file.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public MethodData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
    base(objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// Contructor to create a new method
  /// </summary>
  public MethodData() : base(ObjectTypeEnum.Method, Properties)
  {
  }

  /// <summary>
  /// The type of this method
  /// </summary>
  public MethodTypeEnum Type
  {
    get;
    set;
  }

  /// <summary>
  /// The title of this method.
  /// </summary>
  public string? Title
  {
    get => Texts[0].Text;
    set
    {
      if (Texts.Count > 0)
      {
        Texts[0].Text = value;
      }
      else
      {
        Texts.Add(new LanguageTextData(ObjectTypeEnum.MethodText, value));
      }
    }
  }

  /// <summary>
  /// The index tag for this method.
  /// </summary>
  public string Index
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The second index tag for this method (in case of a range search)
  /// </summary>
  public string Index2
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The query this method (in case of a fixed query)
  /// </summary>
  public string Query
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The help key for this method
  /// </summary>
  public string HelpKey
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The sort key for this method
  /// </summary>
  public string SortKey
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The sort type for this method
  /// </summary>
  public string SortType
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The sort order for this method
  /// </summary>
  public string SortOrder
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// The sort adapl for this method
  /// </summary>
  public string SortAdapl
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// Truncation for this method
  /// </summary>
  public TruncationTypeEnum TruncationType
  {
    get;
    private set;
  }

  /// <summary>
  /// Domain for this method
  /// </summary>
  public string Domain
  {
    get;
    set;
  } = "";

  /// <summary>
  /// Initial screen for this method
  /// </summary>
  public string InitialScreen
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// Range name index for this method
  /// </summary>
  public string RangeNameIndex
  {
    get;
    private set;
  } = "";

  /// <summary>
  /// Additional index tags for this method
  /// </summary>
  public string IndexTags
  {
    get;
    set;
  } = "";

  /// <summary>
  /// The texts for the method
  /// </summary>
  public TextsList Texts
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// THe list of screens
  /// </summary>
  public TextsList Screens
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of access rights.
  /// </summary>
  public AccessControlList AccessRights
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// How are records sorted in this method.
  /// </summary>
  public List<MethodSortSpecificationData> SortSpecification
  {
    get;
    private set;
  } = [];

  internal void Add(MethodSortSpecificationData methodSortSpecificationData) => SortSpecification.Add(methodSortSpecificationData);

  /// <summary>
  /// Override for debugging
  /// </summary>
  /// <returns></returns>
  public override string? ToString() => Title;

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,   "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.Enum,    "Type", typeof(MethodTypeEnum)),
    new PropertyMap (2,  DataTypesEnum.String,  "Title"),
    new PropertyMap (3,  DataTypesEnum.String,  "Index"),
    new PropertyMap (4,  DataTypesEnum.String,  "Index2"),
    new PropertyMap (5,  DataTypesEnum.String,  "Query"),
    new PropertyMap (6,  DataTypesEnum.String,  "HelpKey"),
    new PropertyMap (7,  DataTypesEnum.String,  "SortKey"),
    new PropertyMap (8,  DataTypesEnum.String,  "SortType"),
    new PropertyMap (9,  DataTypesEnum.String,  "SortOrder"),
    new PropertyMap (10, DataTypesEnum.String,  "SortAdapl"),
    new PropertyMap (11, DataTypesEnum.Skip),
    new PropertyMap (12, DataTypesEnum.Skip),
    new PropertyMap (13, DataTypesEnum.Skip),
    new PropertyMap (14, DataTypesEnum.Skip),
    new PropertyMap (15, DataTypesEnum.Enum,    "TruncationType", typeof (TruncationTypeEnum)),
    new PropertyMap (16, DataTypesEnum.String,  "Domain"),
    new PropertyMap (17, DataTypesEnum.Skip),
    new PropertyMap (18, DataTypesEnum.String,  "InitialScreen"),
    new PropertyMap (19, DataTypesEnum.String,  "RangeNameIndex"),
    new PropertyMap (20, DataTypesEnum.String,  "IndexTags")
  ];

  internal override ChildrenList[] Children =>
   [
     new ChildrenList(Screens, LanguageTextData.Properties),
     new ChildrenList(Texts, LanguageTextData.Properties),
     new ChildrenList(SortSpecification, MethodSortSpecificationData.Properties),
     new ChildrenList(AccessRights, AccessRightsData.Properties)
   ];
}

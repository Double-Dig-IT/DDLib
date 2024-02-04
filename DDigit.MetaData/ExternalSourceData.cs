namespace DDigit.MetaData;

public class ExternalSourceData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The path or url of the external source.
  /// </summary>
  public string? PathOrUrl
  {
    get; private set;
  }

  /// <summary>
  /// The link screen to use for this external source.
  /// </summary>
  public string? LinkScreen
  {
    get; private set;
  }

  /// <summary>
  /// Adapl to use while linking from an external source.
  /// </summary>
  public string? Adapl
  {
    get;
    internal set;
  }

  /// <summary>
  /// ?? How does this work?
  /// </summary>
  public string? GroupByField
  {
    get;
    internal set;
  }

  /// <summary>
  /// ?? How does this work?
  /// </summary>
  public int? GroupSize
  {
    get;
    internal set;
  }

  /// <summary>
  /// The field name.
  /// </summary>
  public string? Name
  {
    get;
    internal set;
  }

  /// <summary>
  /// A list with all external source names.
  /// </summary>
  public List<LanguageTextData> Names
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// How the links are sorted
  /// </summary>
  public List<ExternalSourceSortData> Sort
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list with all the mappings for this external source.
  /// </summary>
  public List<ExternalSourceMappingData> Mapping
  {
    get;
    internal set;
  } = [];


  public override string? ToString() => Name;

  internal static readonly PropertyList Properties =
  [
    new PropertyMap(0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap(1, DataTypesEnum.String, "PathOrUrl"),
    new PropertyMap(2, DataTypesEnum.String, "Name"),
    new PropertyMap(3, DataTypesEnum.Skip),
    new PropertyMap(4, DataTypesEnum.String, "LinkScreen"),
    new PropertyMap(5, DataTypesEnum.String, "Adapl"),
    new PropertyMap(6, DataTypesEnum.String, "GroupByField"),
    new PropertyMap(7, DataTypesEnum.Int32,  "GroupSize"),
    new PropertyMap(8, DataTypesEnum.Skip),
    new PropertyMap(9, DataTypesEnum.Skip),
  ];

  internal override (PropertyList, IEnumerable<object>)[]? Children =>
    [
       (LanguageTextData.Properties, Names),
       (ExternalSourceSortData.Properties, Sort),
       (ExternalSourceMappingData.Properties, Mapping),
    ];

}
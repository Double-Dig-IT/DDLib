namespace DDigit.MetaData;

/// <summary>
/// Metadata for a dataset.
/// </summary>
public class DatasetData : BaseData
{
  /// <summary>
  /// Contructor to read the object from disk
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public DatasetData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
    base(objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// Constructor to create an empty <see cref="DatasetData"/>
  /// </summary>
  public DatasetData() : base(ObjectTypeEnum.Dataset)
  {
  }

  /// <summary>
  /// The name of the dataset
  /// </summary>
  public string? Name
  {
    get; set;
  }

  /// <summary>
  /// The lower limit of the dataset
  /// </summary>
  public int LowerLimit
  {
    get; set;
  }

  /// <summary>
  /// The upper limit of the dataset
  /// </summary>
  public int UpperLimit
  {
    get; set;
  }

  /// <summary>
  /// A list of access rights.
  /// </summary>
  public AccessControlList AccessRights
  {
    get; private set;
  } = [];

  /// <summary>
  /// A list of dataset specific fields.
  /// </summary>
  public List<FieldData> Fields
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// Override for debugging.
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{Name} {LowerLimit}-{UpperLimit}";

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,  nameof(ElementCount)),
     new PropertyMap (1, DataTypesEnum.String, nameof(Name)),
     new PropertyMap (2, DataTypesEnum.Int32,  nameof(LowerLimit)),
     new PropertyMap (3, DataTypesEnum.Int32,  nameof(UpperLimit)),
     new PropertyMap (4, DataTypesEnum.Skip),
     new PropertyMap (5, DataTypesEnum.Skip),
     new PropertyMap (6, DataTypesEnum.Skip),
  ];

  internal override ChildrenList[] Children =>
  [
      new ChildrenList(AccessRights, AccessRightsData.Properties),
      new ChildrenList(Fields, FieldData.Properties)
  ];
}
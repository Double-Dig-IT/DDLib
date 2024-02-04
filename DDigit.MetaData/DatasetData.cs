namespace DDigit.MetaData;

public class DatasetData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

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
  public List<AccessRightsData> AccessRights
  {
    get; private set;
  } = [];

  public override string ToString() => $"{Name} {LowerLimit}-{UpperLimit}";

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
     new PropertyMap (1, DataTypesEnum.String, "Name"),
     new PropertyMap (2, DataTypesEnum.Int32,  "LowerLimit"),
     new PropertyMap (3, DataTypesEnum.Int32,  "UpperLimit"),
     new PropertyMap (4, DataTypesEnum.Skip),
     new PropertyMap (5, DataTypesEnum.Skip),
     new PropertyMap (6, DataTypesEnum.Skip),
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
      (AccessRightsData.Properties, AccessRights),
  ];
}
namespace DDigit.MetaData;

public class ApplicationFeatureData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
             BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  public FeatureTypeEnum FeatureType
  {
    get; 
    set;
  }

  /// <summary>
  /// A list of access rights.
  /// </summary>
  public List<AccessRightsData> AccessRights
  {
    get; private set;
  } = [];

  internal static readonly PropertyList Properties =
  [
    new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1, DataTypesEnum.Int16, "FeatureType", typeof(FeatureTypeEnum))
  ];
}

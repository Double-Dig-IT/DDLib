namespace DDigit.MetaData;

/// <summary>
/// A feature of an application
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>
public class ApplicationFeatureData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
             BaseData(objectType, stream, encoding, Properties, trace)
{

  /// <summary>
  /// The feature
  /// </summary>
  public FeatureTypeEnum FeatureType
  {
    get; 
    set;
  }

  /// <summary>
  /// A list of access rights.
  /// </summary>
  public AccessControlList AccessRights
  {
    get; 
    private set;
  } = [];

  internal static readonly PropertyList Properties =
  [
    new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1, DataTypesEnum.Int16, "FeatureType", typeof(FeatureTypeEnum))
  ];
}

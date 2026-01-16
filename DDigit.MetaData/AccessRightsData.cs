namespace DDigit.MetaData;

/// <summary>
/// Metadata about access rights;
/// </summary>
public class AccessRightsData : BaseData
{
  /// <summary>
  /// Construtor while reading
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public AccessRightsData(ObjectTypeEnum objectType, FileStream? stream, Encoding encoding, bool trace)
    : base(objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// General constructor
  /// </summary>
  /// <param name="objectType"></param>
  public AccessRightsData(ObjectTypeEnum objectType) : base(objectType, Properties)
  {
  }

  /// <summary>
  /// The role
  /// </summary>
  public string? Role
  {
    get; set;
  }

  /// <summary>
  /// The rights that the role has
  /// </summary>
  public RightsEnum Rights
  {
    get; set;
  }

  /// <summary>
  /// Override for debugging.
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{Role}:{Rights}";

  internal static PropertyList Properties =
   [
        new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
        new PropertyMap (1, DataTypesEnum.String, "Role"),
        new PropertyMap (2, DataTypesEnum.Enum,   "Rights", typeof(RightsEnum))
   ];
}
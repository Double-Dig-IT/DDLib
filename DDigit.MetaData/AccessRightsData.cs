namespace DDigit.MetaData;

public class AccessRightsData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The role
  /// </summary>
  public string? Role
  {
    get; private set;
  }

  /// <summary>
  /// The rights that the role has
  /// </summary>
  public RightsEnum Rights
  {
    get; private set;
  }

  public override string ToString() => $"{Role}:{Rights}";

  internal static PropertyList Properties =
   [
        new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
        new PropertyMap (1, DataTypesEnum.String, "Role"),
        new PropertyMap (2, DataTypesEnum.Enum,   "Rights", typeof(RightsEnum))
   ];
}
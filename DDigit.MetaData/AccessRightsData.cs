namespace DDigit.MetaData;

public class AccessRightsData : BaseData
{
  public AccessRightsData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace)
    : base(objectType, stream, encoding, fileName, Properties, trace)
  {
  }

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

  public override string ToString() => $"{Role}:{Rights}";

  internal static PropertyList Properties =
   [
        new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
        new PropertyMap (1, DataTypesEnum.String, "Role"),
        new PropertyMap (2, DataTypesEnum.Enum,   "Rights", typeof(RightsEnum))
   ];
}
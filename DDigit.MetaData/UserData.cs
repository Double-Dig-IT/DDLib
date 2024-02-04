namespace DDigit.MetaData;

public class UserData : BaseData
{
  public UserData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace)
    : base(objectType, stream, encoding, fileName, Properties, trace)
  {
    ElementCount = Properties.Max(property => property.Element);
  }

  public UserData() : base(ObjectTypeEnum.User, Properties)
  {
  }

  public string Name
  {
    get; set;
  } = string.Empty;

  public string Role
  {
    get; set;
  } = string.Empty;

  /// <summary>
  /// Do not expose password outside this module
  /// </summary>
  internal string Password
  {
    get; private set;
  } = string.Empty;

  /// <summary>
  /// Use this function to set the password (write only)
  /// </summary>
  /// <param name="password">The password for the user</param>
  public void SetPassword(string password) => Password = password;

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String, "Name"),
    new PropertyMap (2,  DataTypesEnum.String, "Role"),
    new PropertyMap (3,  DataTypesEnum.String, "Password")
  ];
}
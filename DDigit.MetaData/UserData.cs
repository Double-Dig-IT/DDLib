namespace DDigit.MetaData;

/// <summary>
/// Data structure to store information about a user
/// </summary>
public class UserData : BaseData
{
  /// <summary>
  /// Constructor that reads the data from disk
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public UserData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace)
    : base(objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// Constructor to create a user from scratch
  /// </summary>
  public UserData() : base(ObjectTypeEnum.User, Properties)
  {
  }

  /// <summary>
  /// User name
  /// </summary>
  public string Name
  {
    get;
    set;
  } = "";

  /// <summary>
  /// Role of the user
  /// </summary>
  public string Role
  {
    get;
    set;
  } = "";

  /// <summary>
  /// Do not expose password outside this module
  /// </summary>
  public string? Password
  {
    get;
    private set;
  } 

  /// <summary>
  /// Use this function to set the password (write only)
  /// </summary>
  /// <param name="password">The password for the user</param>
  public void SetPassword(string password) => Password = password;

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  nameof(ElementCount)),
    new PropertyMap (1,  DataTypesEnum.String, nameof(Name)),
    new PropertyMap (2,  DataTypesEnum.String, nameof(Role)),
    new PropertyMap (3,  DataTypesEnum.String, nameof(Password))
  ];
}
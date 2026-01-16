namespace DDigit.MetaData;

public class FeedbackLinkData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace)
{

  /// <summary>
  /// The name of the database
  /// </summary>
  public string? DatabasePath
  {
    get; private set;
  }

  /// <summary>
  /// The format string (not used as far as we know).
  /// </summary>
  public string? FormatString
  {
    get; private set;
  }

  public override string? ToString() => DatabasePath;

  internal static readonly PropertyList Properties =
  [
      new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
      new PropertyMap (1,  DataTypesEnum.String, "DatabasePath"),
      new PropertyMap (2,  DataTypesEnum.String, "FormatString"),
  ];
}
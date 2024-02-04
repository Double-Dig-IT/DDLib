namespace DDigit.MetaData;

public class ExternalSourceMappingData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The source of the data.
  /// </summary>
  public string? Source
  {
    get; private set;
  }

  /// <summary>
  /// The destination of the data.
  /// </summary>
  public string? Destination
  {
    get; private set;
  }

  /// <summary>
  /// Some flags, no idea where this is used for.
  /// </summary>
  public int? Flags
  {
    get; private set;
  }

  public override string? ToString() => $"{Source} -> {Destination}";

  internal static readonly PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String, "Source"),
    new PropertyMap (2,  DataTypesEnum.String, "Destination"),
    new PropertyMap (3,  DataTypesEnum.Int32,  "Flags")
  ];
}
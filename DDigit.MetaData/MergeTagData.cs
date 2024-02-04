namespace DDigit.MetaData;

public class MergeTagData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The source tag to get the data from.
  /// </summary>
  public string? Source
  {
    get; private set;
  }

  /// <summary>
  /// The destination tag to write the data to.
  /// </summary>
  public string? Destination
  {
    get; private set;
  }

  public override string ToString() => $"{Source} => {Destination}";

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,   "ElementCount"),
     new PropertyMap (1, DataTypesEnum.String,  "Destination"),
     new PropertyMap (2, DataTypesEnum.String,  "Source"),
  ];
}
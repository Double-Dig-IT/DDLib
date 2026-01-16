namespace DDigit.MetaData;

public class MergeTagData : BaseData
{
  internal MergeTagData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
    base(objectType, stream, encoding, Properties, trace)
  {

  }

  /// <summary>
  /// General constructor
  /// </summary>
  /// <remarks>
  /// Made public for creation of FieldData using scripts
  /// </remarks>
  public MergeTagData(ObjectTypeEnum objectType) : base(objectType)
  {
    ElementCount = 2;
  }

  /// <summary>
  /// The source tag to get the data from.
  /// </summary>
  public string? Source
  {
    get; set;
  }

  /// <summary>
  /// The destination tag to write the data to.
  /// </summary>
  public string? Destination
  {
    get; set;
  }

  public override string ToString() => $"{Source} => {Destination}";

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,   "ElementCount"),
     new PropertyMap (1, DataTypesEnum.String,  "Destination"),
     new PropertyMap (2, DataTypesEnum.String,  "Source"),
  ];
}
namespace DDigit.MetaData;

public class ExternalSourceSortData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// The tag to sort on
  /// </summary>
  public string? Tag
  {
    get; private set;
  }

  /// <summary>
  /// The sort priority
  /// </summary>
  public string? Priority
  {
    get; private set;
  }

  /// <summary>
  /// The sort order
  /// </summary>
  public SortSequenceEnum Order
  {
    get; private set;
  }

  internal static readonly PropertyList Properties =
  [
    new PropertyMap(0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap(1, DataTypesEnum.String, "Tag"),
    new PropertyMap(2, DataTypesEnum.String, "Priority"),
    new PropertyMap(3, DataTypesEnum.Int16,  "Order", typeof(SortSequenceEnum))
  ];
}

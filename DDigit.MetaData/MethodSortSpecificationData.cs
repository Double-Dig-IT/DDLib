namespace DDigit.MetaData;

public class MethodSortSpecificationData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
    BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? Tag
  {
    get; private set;
  }

  public SortKeyTypeEnum KeyType
  {
    get;
    private set;
  }

  public SortSequenceEnum Sequence
  {
    get;
    private set;
  }

  public bool SortAll
  {
    get;
    private set;
  }

  internal static PropertyList Properties =
  [
    new PropertyMap (1,  DataTypesEnum.Int16,   "ElementCount"),
    new PropertyMap (2,  DataTypesEnum.String,  "Tag"),
    new PropertyMap (3,  DataTypesEnum.Enum,    "KeyType", typeof (SortKeyTypeEnum)),
    new PropertyMap (4,  DataTypesEnum.Enum,    "Sequence", typeof (SortSequenceEnum)),
    new PropertyMap (5,  DataTypesEnum.Bool,    "SortAll")
  ];
}
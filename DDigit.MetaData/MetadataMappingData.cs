namespace DDigit.MetaData;

public class MetadataMappingData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? Tag { get; private set; }

  public string? MetadataTag { get; private set; }

  public override string ToString() => $"{Tag} <-> {MetadataTag}";
  
  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,   "ElementCount"),
     new PropertyMap (1, DataTypesEnum.String,  "Tag"),
     new PropertyMap (2, DataTypesEnum.String,  "MetadataTag"),
  ];
}

namespace DDigit.MetaData;

public class FacsData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? Name
  {
    get; private set;
  }

  public string? Directory
  {
    get; private set;
  }

  public string? Dataset
  {
    get; private set;
  }

  public override string? ToString() => Name;

  public static readonly PropertyList Properties =
  [
    new PropertyMap(0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap(1, DataTypesEnum.String, "Name"),
    new PropertyMap(2, DataTypesEnum.String, "Directory"),
    new PropertyMap(3, DataTypesEnum.String, "Dataset"),
  ];
}
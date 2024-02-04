namespace DDigit.MetaData;

public class LanguageTextData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? Text
  {
    get;
    set;
  }

  public override string? ToString() => Text;

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,   "ElementCount"),
     new PropertyMap (1, DataTypesEnum.String,  "Text"),
  ];
}

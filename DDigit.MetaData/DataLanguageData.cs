namespace DDigit.MetaData;

public class DataLanguageData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{

  /// <summary>
  /// Locale for id for a language.
  /// </summary>
  public short LocaleId
  {
    get; private set;
  }

  /// <summary>
  /// The name for the language.
  /// </summary>
  public string? Name
  {
    get; private set;
  }

  public override string? ToString() => $"{LocaleId} ({Name})";

  internal static readonly PropertyList Properties =
  [
    new PropertyMap(0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap(1, DataTypesEnum.Int16,  "LocaleId"),
    new PropertyMap(2, DataTypesEnum.String, "Name")
  ];
}
namespace DDigit.MetaData;

/// <summary>
/// Structure to store data languages for an application
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>
public class DataLanguageData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace)
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

  /// <summary>
  /// Debugging
  /// </summary>
  /// <returns></returns>
  public override string? ToString() => $"{LocaleId} ({Name})";

  internal static readonly PropertyList Properties =
  [
    new PropertyMap(0, DataTypesEnum.Int16,  nameof(ElementCount)),
    new PropertyMap(1, DataTypesEnum.Int16,  nameof(LocaleId)),
    new PropertyMap(2, DataTypesEnum.String, nameof(Name))
  ];
}
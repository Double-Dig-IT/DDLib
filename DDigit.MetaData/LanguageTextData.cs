namespace DDigit.MetaData;

/// <summary>
/// Text for a specific language
/// </summary>
public class LanguageTextData : BaseData
{
  /// <summary>
  /// Constructor to read a language specific text form disk.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public LanguageTextData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
    base (objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// Constructor to create a new Language specific text.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="text"></param>
  public LanguageTextData(ObjectTypeEnum objectType, string? text) : base (objectType, Properties)
  {
    Text = text;
    ElementCount = 1;
  }

  /// <summary>
  /// The text for a language
  /// </summary>
  [DDesigner(DDesignerPropertyTypeEnum.Label, "Text", 0)]
  public string? Text { get; set; } 

  /// <summary>
  /// Return the text only
  /// </summary>
  /// <returns></returns>
  public override string? ToString() => Text;

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,   nameof(ElementCount)),
     new PropertyMap (1, DataTypesEnum.String,  nameof(Text)),
  ];

}

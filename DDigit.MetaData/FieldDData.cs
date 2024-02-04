namespace DDigit.MetaData;

public class FieldDData : BaseData
{

  public FieldDData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, PropertyList properties, bool trace) : 
    base (objectType, stream, encoding, fileName, properties, trace)
  {

  }

  internal FieldDData() : base(ObjectTypeEnum.Field, string.Empty)
  {

  }


  /// <summary>
  /// The tag of the field.
  /// </summary>
  public string? Tag
  {
    get; protected set;
  }

  /// <summary>
  /// The field name.
  /// </summary>
  public string? Name
  {
    get;
    set;
  }

  /// <summary>
  /// A list with all field names
  /// </summary>
  public List<LanguageTextData> Names
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// Is this field repeatable?
  /// </summary>
  public bool IsRepeated
  {
    get; protected set;
  }

  public string? FieldName(string language)
  {
    var languageNumber = Languages.GetAdlibNo(language);
    var fieldName = languageNumber == 0 ? Name : Names[languageNumber - 1]?.Text;
    return string.IsNullOrWhiteSpace(fieldName) ? Name : fieldName;
  }

  public override string ToString() => $"{Tag} {Name}";
}
namespace DDigit.MetaData;

/// <summary>
/// Summary (basic fields) from FieldData.
/// </summary>
public class FieldDData : BaseData
{

  /// <summary>
  /// Constructor to read the binary information from a stream.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="properties"></param>
  /// <param name="trace"></param>
  public FieldDData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, IReadOnlyList<PropertyMap> properties, bool trace) :
    base(objectType, stream, encoding, properties, trace)
  {

  }

  /// <summary>
  /// Constructor to create a brand new field.
  /// </summary>
  internal FieldDData() : base(ObjectTypeEnum.Field)
  {

  }


  /// <summary>
  /// The tag of the field.
  /// </summary>
  public string? Tag
  {
    get; 
    set;
  }

  /// <summary>
  /// The field name.
  /// </summary>
  public string? Name
  {
    get => Names[0].Text;
    set
    {
      if (Names.Count > 0)
      {
        Names[0].Text = value;
      }
      else
      {
        Names.Add(new LanguageTextData(ObjectTypeEnum.FieldName, value));
      }
    }
  }

  /// <summary>
  /// A list with all field names
  /// </summary>
  public FieldNameList Names
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// Is this field repeatable?
  /// </summary>
  public bool IsRepeated
  {
    get; set;
  }

  /// <summary>
  /// Validate if the field is repeated.
  /// </summary>
  /// <param name="occ"></param>
  /// <exception cref="FieldIsNotRepeatedException"></exception>
  public void ValidateOccurrence(int occ)
  {
    if (occ < 1)
    {
      throw new InvalidOccurrenceException(Name, occ);
    }

    if (occ > 1 && !IsRepeated)
    {
      throw new FieldIsNotRepeatedException(Name, occ);
    }
  }

  /// <summary>
  /// Get the field name in a specific language.
  /// </summary>
  /// <param name="language">ISO code of the language</param>
  /// <returns>Field name</returns>
  public string? FieldName(string language) => Names[Languages.GetAdlibNo(language)]?.Text;


  /// <summary>
  /// Default ToString method.
  /// </summary>
  /// <returns>Tag + Name</returns>
  public override string ToString() => $"{Tag} {Name}";
}
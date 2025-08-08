namespace DDigit.MetaData;

public class FieldDData : BaseData
{

  public FieldDData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, PropertyList properties, bool trace) :
    base(objectType, stream, encoding, fileName, properties, trace)
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
  public TextsList Names
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

  public string? FieldName(string language) => Names[Languages.GetAdlibNo(language)]?.Text;
    

  public override string ToString() => $"{Tag} {Name}";
}
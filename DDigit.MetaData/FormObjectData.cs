
namespace DDigit.MetaData;

/// <summary>
/// Defines an object on a screen
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="textEncoding"></param>
/// <param name="trace"></param>
public class FormObjectData : BaseData, IHasPropertyMap<FieldData>
{
  /// <summary>
  /// Constructor to create an empty <see cref="FormObjectData"/>
  /// </summary>
  public FormObjectData() : base(ObjectTypeEnum.FormObjectData)
  {
  }

  /// <summary>
  /// Constructor to create <see cref="FormObjectData"/> from disk
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="textEncoding"></param>
  /// <param name="trace"></param>
  public FormObjectData(ObjectTypeEnum objectType, FileStream stream, Encoding textEncoding, bool trace) : base(objectType, stream, textEncoding, Properties, trace)
  {
  }

  /// <summary>
  /// The type of object
  /// </summary>
  public FormObjectTypeEnum Type { get; private set; }

  /// <summary>
  /// The tag of the field
  /// </summary>
  public string? Tag { get; private set; }

  /// <summary>
  /// The title of the object
  /// </summary>
  public string? Title
  {
    get => Texts[0].Text;
    set
    {
      if (Texts.Count > 0)
      {
        Texts[0].Text = value;
      }
      else
      {
        Texts.Add(new LanguageTextData(ObjectTypeEnum.LabelText, value));
      }
    }
  }

  /// <summary>
  /// The line position of the label
  /// </summary>
  public short LabelY { get; private set; }

  /// <summary>
  /// The column position of the label
  /// </summary>
  public short LabelX { get; private set; }

  /// <summary>
  /// The line position of the control
  /// </summary>
  public short ControlY { get; private set; }

  /// <summary>
  /// The column position of the control
  /// </summary>
  public short ControlX { get; private set; }

  /// <summary>
  /// The width of the control in characters
  /// </summary>
  public short Width { get; private set; }

  /// <summary>
  /// Occurrence number of the control
  /// </summary>
  public short Occurrence { get; set; }

  /// <summary>
  /// Group number
  /// </summary>
  [Obsolete("Has been replaced by group names in the datadictionary")]
  public short Group { get; set; }

  /// <summary>
  /// The foreground color of text labels
  /// </summary>
  [Obsolete("replaced by TextForegroundRGB")]
  public short TextColor { get; private set; }

  /// <summary>
  /// The foreground color of data
  /// </summary>
  [Obsolete("replaced by DataForegroundRGB")]
  public short DataColor { get; private set; }

  /// <summary>
  /// Determines whether this is a read-write or readonly screen
  /// </summary>
  public FormObjectAccessEnum Access { get; private set; }

  /// <summary>
  /// Is this field repeatable
  /// </summary>
  public RepeatabilityEnum Repeated { get; private set; }

  /// <summary>
  /// How is text justified in the field
  /// </summary>
  public JustificationEnum Justification { get; private set; }

  /// <summary>
  /// Data type validations per screen, this is old style
  /// </summary>
  public FormObjectDataTypeEnum DataType { get; private set; }

  /// <summary>
  /// Data format validations.
  /// </summary>
  public FormObjectValidationEnum Validation { get; private set; }

  /// <summary>
  /// A regular expression that can be used for validation.
  /// </summary>
  public string? RegularExpression { get; private set; }

  /// <summary>
  /// The name of a zoom screen if this field is linked.
  /// </summary>
  public string? ZoomForm { get; set; }

  /// <summary>
  /// The name of a zoom edit screen if this field is linked.
  /// </summary>
  public string? ZoomEditForm { get; set; }

  /// <summary>
  /// Foreground color of text (RRGGBB)
  /// </summary>
  public int TextForegroundRGB { get; private set; }

  /// <summary>
  /// Background color of text areas (RRGGBB)
  /// </summary>
  public int TextBackgroundRGB { get; private set; }

  /// <summary>
  /// Foreground color of text in data areas (RRGGBB)
  /// </summary>
  public int DataForegroundRGB { get; private set; }

  /// <summary>
  /// Background color of data areas (RRGGBB)
  /// </summary>
  public int DataBackgroundRGB { get; private set; }

  /// <summary>
  /// Spare space
  /// </summary>
  [Obsolete("spare property, is not used as far as we know)")]
  public int Spare1 { get; private set; }

  /// <summary>
  /// Determines the moment when a script is executed.
  /// </summary>
  public ScriptTypeEnum ScriptType { get; private set; }

  /// <summary>
  /// Something with editing links, exact function unclear
  /// </summary>
  public bool OnDemandLinkEdit { get; private set; }

  [Obsolete("Not clear when this was ever used.")]
  /// <summary>
  /// Obsolete
  /// </summary>
  public bool LegacyOnly { get; private set; }

  /// <summary>
  /// The name of the font that is used in this field.
  /// </summary>
  public string? Font { get; private set; }

  /// <summary>
  /// Size of the font for this field.
  /// </summary>
  public float FontSize { get; private set; }

  /// <summary>
  /// Bold, Italic etc.
  /// </summary>
  public int FontStyle { get; private set; }

  /// <summary>
  /// How is the label aligned.
  /// </summary>
  public JustificationEnum LabelAlign { get; private set; }

  /// <summary>
  /// A condition that renders the display of the field as readonly.
  /// </summary>
  public string? SuppressCondition { get; private set; }

  /// <summary>
  /// A condition that suppresses the display of the field.
  /// </summary>
  public string? ReadOnlyCondition { get; private set; }

  /// <summary>
  /// How is data wrapped in fields
  /// </summary>
  public WrapModeEnum WrapMode { get; private set; }

  /// <summary>
  /// The labels for this screen
  /// </summary>
  public TextsList Texts { get; private set; } = [];

  /// <summary>
  /// A compiled list of suppress conditions.
  /// </summary>
  public List<FieldConditionData> SuppressConditions { get; private set; } = [];

  /// <summary>
  /// A compiled list of form suppress conditions.
  /// </summary>
  public List<FormConditionData> FormSuppressConditions { get; private set; } = [];

  /// <summary>
  /// For debugging
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{Type} {Tag} ({LabelY},{LabelX})-({ControlY},{ControlX})";

  internal static PropertyList Properties =
  [
     new PropertyMap( 0, DataTypesEnum.Int16,  nameof(ElementCount)),
     new PropertyMap( 1, DataTypesEnum.Int16,  nameof(Type), typeof(FormObjectTypeEnum)),
     new PropertyMap( 2, DataTypesEnum.String, nameof(Tag)),
     new PropertyMap( 3, DataTypesEnum.String, nameof(Title)),
     new PropertyMap( 4, DataTypesEnum.Int16,  nameof(LabelY)),
     new PropertyMap( 5, DataTypesEnum.Int16,  nameof(LabelX)),
     new PropertyMap( 6, DataTypesEnum.Int16,  nameof(ControlY)),
     new PropertyMap( 7, DataTypesEnum.Int16,  nameof(ControlX)),
     new PropertyMap( 8, DataTypesEnum.Int16,  nameof(Width)),
     new PropertyMap( 9, DataTypesEnum.Int16,  nameof(Occurrence)),
     new PropertyMap(10, DataTypesEnum.Int16,  nameof(Group)),
     new PropertyMap(11, DataTypesEnum.Int16,  nameof(TextColor)),
     new PropertyMap(12, DataTypesEnum.Int16,  nameof(DataColor)),
     new PropertyMap(13, DataTypesEnum.Int16,  nameof(Access), typeof(FormObjectAccessEnum)),
     new PropertyMap(14, DataTypesEnum.Int16,  nameof(Repeated), typeof(RepeatabilityEnum)),
     new PropertyMap(15, DataTypesEnum.Int16,  nameof(Justification), typeof(JustificationEnum)),
     new PropertyMap(16, DataTypesEnum.Int16,  nameof(DataType), typeof(FormObjectDataTypeEnum)),
     new PropertyMap(17, DataTypesEnum.Int16,  nameof(Validation), typeof(FormObjectValidationEnum)),
     new PropertyMap(18, DataTypesEnum.String, nameof(RegularExpression)),
     new PropertyMap(19, DataTypesEnum.String, nameof(ZoomForm)),
     new PropertyMap(21, DataTypesEnum.String, nameof(ZoomEditForm)),
     new PropertyMap(22, DataTypesEnum.Int32,  nameof(TextForegroundRGB)),
     new PropertyMap(23, DataTypesEnum.Int32,  nameof(TextBackgroundRGB)),
     new PropertyMap(24, DataTypesEnum.Int32,  nameof(DataForegroundRGB)),
     new PropertyMap(25, DataTypesEnum.Int32,  nameof(DataBackgroundRGB)),
     new PropertyMap(26, DataTypesEnum.Int32,  nameof(Spare1)),
     new PropertyMap(27, DataTypesEnum.Int32,  nameof(ScriptType), typeof(ScriptTypeEnum)),
     new PropertyMap(28, DataTypesEnum.Bool32, nameof(OnDemandLinkEdit)),
     new PropertyMap(29, DataTypesEnum.Bool32, nameof(LegacyOnly)),
     new PropertyMap(34, DataTypesEnum.String, nameof(Font)),
     new PropertyMap(35, DataTypesEnum.Float,  nameof(FontSize)),
     new PropertyMap(36, DataTypesEnum.Int32,  nameof(FontStyle)),
     new PropertyMap(37, DataTypesEnum.Int16,  nameof(LabelAlign), typeof(JustificationEnum)),
     new PropertyMap(38, DataTypesEnum.String, nameof(SuppressCondition)),
     new PropertyMap(39, DataTypesEnum.String, nameof(ReadOnlyCondition)),
     new PropertyMap(40, DataTypesEnum.Int16,  nameof(WrapMode), typeof(WrapModeEnum))
    ];

  internal override ChildrenList[] Children =>
  [
    new ChildrenList(Texts, LanguageTextData.Properties),
    new ChildrenList(SuppressConditions, FieldConditionData.Properties),
    new ChildrenList(FormSuppressConditions, FormConditionData.Properties)
  ];

  static IReadOnlyList<PropertyMap> IHasPropertyMap<FieldData>.Properties => Properties;
}
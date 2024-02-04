namespace DDigit.MetaData;

public class FormObjectData(ObjectTypeEnum objectType, Stream stream, Encoding textEncoding, string? fileName, bool trace) :
    BaseData(objectType, stream, textEncoding, fileName, Properties, trace)
{
  public FormObjectTypeEnum Type { get; private set; }

  public string? Tag { get; private set; }

  public string? Title { get; private set; }

  public short LabelY { get; private set; }

  public short LabelX { get; private set; }

  public short ControlY { get; private set; }

  public short ControlX { get; private set; }

  public short Width { get; private set; }

  public short Occurrence { get; private set; }

  public short Group { get; private set; }

  public short TextColor { get; private set; }

  public short DataColor { get; private set; }

  public FormObjectAccessEnum Access { get; private set; }

  public RepeatabilityEnum Repeated { get; private set; }

  public JustificationEnum Justification { get; private set; }

  public FormObjectDataTypeEnum DataType { get; private set; }

  public FormObjectValidationEnum Validation { get; private set; }

  public string? RegularExpression { get; private set; }

  public string? ZoomForm { get; private set; }

  public string? ZoomEditForm { get; private set; }

  public int TextForegroundRGB { get; private set; }

  public int TextBackgroundRGB { get; private set; }

  public int DataForegroundRGB { get; private set; }

  public int DataBackgroundRGB { get; private set; }

  public int Spare1 { get; private set; }

  public ScriptTypeEnum ScriptType { get; private set; }

  public bool OnDemandLinkEdit { get; private set; }

  public bool LegacyOnly { get; private set; }

  public string? Font { get; private set; }

  public float FontSize { get; private set; }

  public int FontStyle { get; private set; }

  public JustificationEnum LabelAlign { get; private set; }

  public string? SuppressCondition { get; private set; }

  public string? ReadOnlyCondition { get; private set; }

  public WrapModeEnum WrapMode { get; private set; }

  public List<LanguageTextData> Texts { get; private set; } = [];

  public List<FieldConditionData> SuppressConditions { get; private set; } = [];

  public List<FormConditionData> FormSuppressConditions { get; private set; } = [];


  internal static PropertyList Properties =
  [
     new PropertyMap( 0, DataTypesEnum.Int16,  "ElementCount"),
     new PropertyMap( 1, DataTypesEnum.Int16,  "Type", typeof(FormObjectTypeEnum)),
     new PropertyMap( 2, DataTypesEnum.String, "Tag"),
     new PropertyMap( 3, DataTypesEnum.String, "Title"),
     new PropertyMap( 4, DataTypesEnum.Int16,  "LabelY"),
     new PropertyMap( 5, DataTypesEnum.Int16,  "LabelX"),
     new PropertyMap( 6, DataTypesEnum.Int16,  "ControlY"),
     new PropertyMap( 7, DataTypesEnum.Int16,  "ControlX"),
     new PropertyMap( 8, DataTypesEnum.Int16,  "Width"),
     new PropertyMap( 9, DataTypesEnum.Int16,  "Occurrence"),
     new PropertyMap(10, DataTypesEnum.Int16,  "Group"),
     new PropertyMap(11, DataTypesEnum.Int16,  "TextColor"),
     new PropertyMap(12, DataTypesEnum.Int16,  "DataColor"),
     new PropertyMap(13, DataTypesEnum.Int16,  "Access", typeof(FormObjectAccessEnum)),
     new PropertyMap(14, DataTypesEnum.Int16,  "Repeated", typeof(RepeatabilityEnum)),
     new PropertyMap(15, DataTypesEnum.Int16,  "Justification", typeof(JustificationEnum)),
     new PropertyMap(16, DataTypesEnum.Int16,  "DataType", typeof(FormObjectDataTypeEnum)),
     new PropertyMap(17, DataTypesEnum.Int16,  "Validation", typeof(FormObjectValidationEnum)),
     new PropertyMap(18, DataTypesEnum.String, "RegularExpression"),
     new PropertyMap(19, DataTypesEnum.String, "ZoomForm"),
     new PropertyMap(21, DataTypesEnum.String, "ZoomEditForm"),
     new PropertyMap(22, DataTypesEnum.Int32,  "TextForegroundRGB"),
     new PropertyMap(23, DataTypesEnum.Int32,  "TextBackgroundRGB"),
     new PropertyMap(24, DataTypesEnum.Int32,  "DataForegroundRGB"),
     new PropertyMap(25, DataTypesEnum.Int32,  "DataBackgroundRGB"),
     new PropertyMap(26, DataTypesEnum.Int32,  "Spare1"),
     new PropertyMap(27, DataTypesEnum.Int32,  "ScriptType", typeof(ScriptTypeEnum)),
     new PropertyMap(28, DataTypesEnum.Bool32, "OnDemandLinkEdit"),
     new PropertyMap(29, DataTypesEnum.Bool32, "LegacyOnly"),
     new PropertyMap(34, DataTypesEnum.String, "Font"),
     new PropertyMap(35, DataTypesEnum.Float,  "FontSize"),
     new PropertyMap(36, DataTypesEnum.Int32,  "FontStyle"),
     new PropertyMap(37, DataTypesEnum.Int16,  "LabelAlign", typeof(JustificationEnum)),
     new PropertyMap(38, DataTypesEnum.String, "SuppressCondition"),
     new PropertyMap(39, DataTypesEnum.String, "ReadOnlyCondition"),
     new PropertyMap(40, DataTypesEnum.Int16,  "WrapMode", typeof(WrapModeEnum))
    ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
    (LanguageTextData.Properties, Texts),
    (FieldConditionData.Properties, SuppressConditions),
    (FormConditionData.Properties, FormSuppressConditions)
  ];
}
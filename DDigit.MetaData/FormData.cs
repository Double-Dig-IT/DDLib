namespace DDigit.MetaData;

/// <summary>
/// Metadata about a form
/// </summary>
public class FormData : FileData
{

  /// <summary>
  /// Constructor to create an empty form.
  /// </summary>
  public FormData() : base(ObjectTypeEnum.Form, null, false)
  {
  }

  /// <summary>
  /// Constructor to read the object from disk
  /// </summary>
  /// <param name="path"></param>
  /// <param name="trace"></param>
  public FormData(string path, bool trace = false) : base(ObjectTypeEnum.Form, path, trace)
  {
  }

  /// <summary>
  /// Decode the binary data from disk
  /// </summary>
  /// <param name="stream"></param>
  /// <param name="trace"></param>
  /// <exception cref="InvalidDataException"></exception>
  /// <exception cref="InvalidMetaDataException"></exception>
  protected override void Decode(FileStream stream, bool trace)
  {
    Magic = stream.ReadInt16();
    TextEncoding = Magic switch
    {
      32766 => Extensions.DosEncoding,
      32756 => Extensions.WindowsEncoding,
      32746 => Encoding.UTF8,
      _ => throw new InvalidDataException($"Invalid magic number in file {FileName}, number found = {Magic}"),
    };

    FormObjectData? field = null;

    while (stream.Position < stream.Length)
    {
      var objectType = (ObjectTypeEnum)stream.ReadEnum(typeof(ObjectTypeEnum));
      try
      {
        switch (objectType)
        {
          case ObjectTypeEnum.FormData:
            ObjectType = objectType;
            ReadProperties(this, Properties!, stream, TextEncoding, trace);
            break;

          case ObjectTypeEnum.FormText:
            Texts.Add(new LanguageTextData(objectType, stream, TextEncoding, trace));
            break;

          case ObjectTypeEnum.FormRights:
            AccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, trace));
            break;

          case ObjectTypeEnum.FormObjectData:
            Fields.Add(field = new FormObjectData(objectType, stream, TextEncoding, trace));
            break;

          case ObjectTypeEnum.LabelText:
            field?.Texts.Add(new LanguageTextData(objectType, stream, TextEncoding, trace));
            break;

          case ObjectTypeEnum.FormSuppressCondition:
            field?.FormSuppressConditions.Add(new FormConditionData(objectType, stream, TextEncoding, trace));
            break;

          case ObjectTypeEnum.FieldSuppressCondition:
            field?.SuppressConditions.Add(new FieldConditionData(objectType, stream, TextEncoding, trace));
            break;

          default:
            throw new InvalidMetaDataException(objectType, stream.Name, stream.Position);
        }
      }
      catch (Exception ex)
      {
        if (ex is InvalidDataException)
        {
          throw;
        }
        throw new InvalidMetaDataException(objectType, stream.Name, stream.Position, ex);
      }
    }
  }

  /// <summary>
  /// Obsolete property, determined whether this is a visible screen or not.
  /// </summary>
  public ScreenBehaviorEnum Behavior { get; private set; }

  /// <summary>
  /// The name of this tab or form
  /// </summary>
  public string? TabName { get; private set; }

  /// <summary>
  /// The title of the form
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
        Texts.Add(new LanguageTextData(ObjectTypeEnum.FormText, value));
      }
    }
  }

  /// <summary>
  /// The height of the tab or form in lines.
  /// </summary>
  public short Height { get; private set; }

  /// <summary>
  /// The Width of the tab or screen in columns.
  /// </summary>
  public short Width { get; private set; }

  /// <summary>
  /// The start row of the screen (offset from top)
  /// </summary>
  public short StartRow { get; private set; }

  /// <summary>
  /// The start column of the screen (offset from the left)
  /// </summary>
  public short StartColumn { get; private set; }

  /// <summary>
  /// The foreground color of text labels
  /// </summary>
  [Adlib("replaced by TextForegroundRGB")]
  public short TextColor { get; private set; }

  /// <summary>
  /// The foreground color of data
  /// </summary>
  [Adlib("replaced by DataForegroundRGB")]
  public short DataColor { get; private set; }

  /// <summary>
  /// The highlight color
  /// </summary>
  [Adlib("replaced by HighlightForegroundRGB")]
  public short HighlightColor { get; private set; }

  /// <summary>
  /// Help key (index in help file)
  /// </summary>
  public string? HelpKey { get; private set; }

  /// <summary>
  /// Script (Adapl or Python) before this tab is shown.
  /// </summary>
  public string? BeforeScreenScript { get; set; }

  /// <summary>
  /// Script (Adapl or Python) after this tab is shown.
  /// </summary>
  public string? AfterScreenScript { get; set; }

  /// <summary>
  /// Flag to indicate whether an old MSDOS style character set was used.
  /// </summary>
  [Adlib("MSDOS character set is no longer supported")]
  public bool ConvertToOEM { get; private set; }

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
  /// Higlighted foreground color (RRGGBB)
  /// </summary>
  public int HighlightForegroundRGB { get; private set; }

  /// <summary>
  /// Higlighted background color (RRGGBB)
  /// </summary>
  public int HighlightBackgroundRGB { get; private set; }

  /// <summary>
  ///Background color of the form or tab
  /// </summary>
  public int FormBackgroundRGB { get; private set; }

  /// <summary>
  /// Condition (expression) that decides whether or not show the form or tab
  /// </summary>
  public string? FormCondition { get; private set; }

  /// <summary>
  /// AThe list of fields on this form or tab.
  /// </summary>
  public List<FormObjectData> Fields { get; private set; } = [];

  /// <summary>
  /// The titles of the tab or screen.
  /// </summary>
  public FormTitleList Texts { get; private set; } = [];

  /// <summary>
  /// Access Rights to this screen
  /// </summary>
  public AccessControlList AccessRights { get; private set; } = [];

  /// <summary>
  /// Write the screen or tab to a stream (serialize).
  /// </summary>
  /// <param name="stream"></param>
  protected override void Encode(FileStream stream)
  {
    stream.WriteInt16(Magic);
    WriteProperties(this, Properties, Children, stream, TextEncoding);
  }

  internal static PropertyList Properties =
[
    new PropertyMap ( 0, DataTypesEnum.Int16,   nameof(ElementCount)),
      new PropertyMap ( 1, DataTypesEnum.Enum,    nameof(Behavior), typeof(ScreenBehaviorEnum)),
      new PropertyMap ( 2, DataTypesEnum.String,  nameof(TabName)),
      new PropertyMap ( 3, DataTypesEnum.String,  nameof(Title)),
      new PropertyMap ( 4, DataTypesEnum.Int16,   nameof(Height)),
      new PropertyMap ( 5, DataTypesEnum.Int16,   nameof(Width)),
      new PropertyMap ( 6, DataTypesEnum.Int16,   nameof(StartRow)),
      new PropertyMap ( 7, DataTypesEnum.Int16,   nameof(StartColumn)),
      new PropertyMap ( 8, DataTypesEnum.Int16,   nameof(TextColor)),
      new PropertyMap ( 9, DataTypesEnum.Int16,   nameof(DataColor)),
      new PropertyMap (10, DataTypesEnum.Int16,   nameof(HighlightColor)),
      new PropertyMap (11, DataTypesEnum.String,  nameof(HelpKey)),
      new PropertyMap (12, DataTypesEnum.String,  nameof(BeforeScreenScript)),
      new PropertyMap (13, DataTypesEnum.String,  nameof(AfterScreenScript)),
      new PropertyMap (17, DataTypesEnum.Bool,    nameof(ConvertToOEM)),
      new PropertyMap (20, DataTypesEnum.Int32,   nameof(TextForegroundRGB)),
      new PropertyMap (21, DataTypesEnum.Int32,   nameof(TextBackgroundRGB)),
      new PropertyMap (22, DataTypesEnum.Int32,   nameof(DataForegroundRGB)),
      new PropertyMap (23, DataTypesEnum.Int32,   nameof(DataBackgroundRGB)),
      new PropertyMap (24, DataTypesEnum.Int32,   nameof(HighlightForegroundRGB)),
      new PropertyMap (25, DataTypesEnum.Int32,   nameof(HighlightBackgroundRGB)),
      new PropertyMap (26, DataTypesEnum.Int32,   nameof(FormBackgroundRGB)),
      new PropertyMap (27, DataTypesEnum.String,  nameof(FormCondition))
];

  internal override ChildrenList[] Children =>
  [
    new ChildrenList(Fields, FormObjectData.Properties),
    new ChildrenList(Texts, LanguageTextData.Properties),
    new ChildrenList(AccessRights, AccessRightsData.Properties)
  ];

  /// <summary>
  /// The file extension of screens.
  /// </summary>
  public static string Extension => ".fmt";

  /// <summary>
  /// For debugging
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{FileName}";

}

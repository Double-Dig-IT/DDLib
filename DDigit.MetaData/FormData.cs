using System.Data;
using System;

namespace DDigit.MetaData;

public class FormData : FileData
{

  public FormData() : base(ObjectTypeEnum.Form, null, false)
  {
  }

  protected override void Decode(Stream stream, bool trace)
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
            ReadProperties(this, Properties!, stream, TextEncoding, FileName, trace);
            break;

          case ObjectTypeEnum.FormText:
            Texts.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FormAccessRights:
            AccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FormField:
            Fields.Add(field = new FormObjectData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.LabelText:
            field?.Texts.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FormSuppressCondition:
            field?.FormSuppressConditions.Add(new FormConditionData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldSuppressCondition:
            field?.SuppressConditions.Add(new FieldConditionData(objectType, stream, TextEncoding, FileName, trace));
            break;

          default:
            throw new InvalidMetaDataException(objectType, FileName, stream.Position);
        }
      }
      catch (Exception ex)
      {
        if (ex is InvalidDataException)
        {
          throw;
        }
        throw new InvalidMetaDataException(objectType, FileName, stream.Position, ex);
      }
    }
  }

  public ScreenBehaviorEnum Behavior { get; private set; }

  public string? ObjectName { get; private set; }

  public string? Title { get; private set; }

  public short Height { get; private set; }

  public short Width { get; private set; }

  public short StartRow { get; private set; }

  public short StartColumn { get; private set; }

  public short TextColor { get; private set; }

  public short DataColor { get; private set; }

  public short HighlightColor { get; private set; }

  public string? HelpKey { get; private set; }

  public string? BeforeScreenScript { get; private set; }

  public string? AfterScreenScript { get; private set; }

  public bool ConvertToOEM { get; private set; }

  public int TextForegroundRGB { get; private set; }

  public int TextBackgroundRGB { get; private set; }

  public int DataForegroundRGB { get; private set; }

  public int DataBackgroundRGB { get; private set; }

  public int HighlightForegroundRGB { get; private set; }

  public int HighlightBackgroundRGB { get; private set; }

  public int FormBackgroundRGB { get; private set; }

  public string? FormCondition { get; private set; }

  public List<FormObjectData> Fields { get; private set; } = [];

  public List<LanguageTextData> Texts { get; private set; } = [];

  public List<AccessRightsData> AccessRights { get; private set; } = [];

  protected override void Encode(Stream stream)
  {
    stream.WriteInt16(Magic);
    WriteProperties(this, Properties, Children, stream, TextEncoding);
  }

  internal static PropertyList Properties =
  [
      new PropertyMap ( 0, DataTypesEnum.Int16,  "ElementCount"),
      new PropertyMap ( 1, DataTypesEnum.Enum,   "Behavior", typeof(ScreenBehaviorEnum)),
      new PropertyMap ( 2, DataTypesEnum.String, "ObjectName"),
      new PropertyMap ( 3, DataTypesEnum.String, "Title"),
      new PropertyMap ( 4, DataTypesEnum.Int16,  "Height"),
      new PropertyMap ( 5, DataTypesEnum.Int16,  "Width"),
      new PropertyMap ( 6, DataTypesEnum.Int16,  "StartRow"),
      new PropertyMap ( 7, DataTypesEnum.Int16,  "StartColumn"),
      new PropertyMap ( 8, DataTypesEnum.Int16,  "TextColor"),
      new PropertyMap ( 9, DataTypesEnum.Int16,  "DataColor"),
      new PropertyMap (10, DataTypesEnum.Int16,  "HighlightColor"),
      new PropertyMap (11, DataTypesEnum.String, "HelpKey"),
      new PropertyMap (12, DataTypesEnum.String, "BeforeScreenScript"),
      new PropertyMap (13, DataTypesEnum.String, "AfterScreenScript"),
      new PropertyMap (17, DataTypesEnum.Bool,   "ConvertToOEM"),
      new PropertyMap (20, DataTypesEnum.Int32,  "TextForegroundRGB"),
      new PropertyMap (21, DataTypesEnum.Int32,  "TextBackgroundRGB"),
      new PropertyMap (22, DataTypesEnum.Int32,  "DataForegroundRGB"),
      new PropertyMap (23, DataTypesEnum.Int32,  "DataBackgroundRGB"),
      new PropertyMap (24, DataTypesEnum.Int32,  "HighlightForegroundRGB"),
      new PropertyMap (25, DataTypesEnum.Int32,  "HighlightBackgroundRGB"),
      new PropertyMap (26, DataTypesEnum.Int32,  "FormBackgroundRGB"),
      new PropertyMap (27, DataTypesEnum.String, "FormCondition")
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
    (FormObjectData.Properties, Fields),
    (LanguageTextData.Properties, Texts),
    (AccessRightsData.Properties, AccessRights)
  ];

  public override string Extension => ".fmt";

}

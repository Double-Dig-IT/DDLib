namespace DDigit.MetaData;

public class FriendlyDatabaseData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public string? DatabasePath
  {
    get; private set;
  }

  /// <summary>
  /// The field name.
  /// </summary>
  public string? Text
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
        Texts.Add(new LanguageTextData(ObjectTypeEnum.ExternalSourceName, value));
      }
    }
  }

  public string? SearchScreen
  {
    get; private set;
  }

  public string? ListScreen
  {
    get; private set;
  }

  public bool DeleteOriginal
  {
    get; private set;
  }

  public string? ZoomScreen
  {
    get; private set;
  }

  public string? AfterDerivationAdapl
  {
    get; private set;
  }

  public TextsList Texts
  {
    get; private set;
  } = [];

  public List<AccessRightsData> AccessRights
  {
    get; private set;
  } = [];

  internal void Add(LanguageTextData languageText) => Texts.Add(languageText);

  internal void Add(AccessRightsData accessRights) => AccessRights.Add(accessRights);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String, "DatabasePath"),
    new PropertyMap (2,  DataTypesEnum.String, "Text"),
    new PropertyMap (3,  DataTypesEnum.String, "SearchScreen"),
    new PropertyMap (4,  DataTypesEnum.String, "ListScreen"),
    new PropertyMap (5,  DataTypesEnum.Bool32, "DeleteOriginal"),
    new PropertyMap (6,  DataTypesEnum.String, "ZoomScreen" ),
    new PropertyMap (7,  DataTypesEnum.String, "AfterDerivationAdapl" ),
    new PropertyMap (8,  DataTypesEnum.Skip),
    new PropertyMap (9,  DataTypesEnum.Skip)
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
     (LanguageTextData.Properties, Texts),
     (AccessRightsData.Properties, AccessRights),
  ];
}
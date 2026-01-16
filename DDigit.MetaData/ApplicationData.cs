namespace DDigit.MetaData;

/// <summary>
/// Contains all operational parameters of an Adlib / Axiell Collections application
/// </summary>
public class ApplicationData : FileData, IFileData
{
  /// <summary>
  /// Constructor to read the setup from a file.
  /// </summary>
  /// <param name="fileName"></param>
  /// <param name="trace"></param>
  public ApplicationData(string fileName, bool trace = false) : base(ObjectTypeEnum.Application, fileName, trace)
  {
  }

  /// <summary>
  /// Constructor to create an empty application.
  /// </summary>
  public ApplicationData() : base(ObjectTypeEnum.Application, null, false)
  {
  }

  /// <summary>
  /// Encode an application
  /// </summary>
  /// <param name="stream"></param>
  protected override void Encode(FileStream stream)
  {
    if (string.IsNullOrWhiteSpace(Name))
    {
      throw new DDException($"{nameof(ApplicationData)} property {nameof(Name)} is not set");
    }
    stream.WriteInt16(Magic);
    WriteProperties(this, Properties, Children, stream, encoding);
  }

  private Encoding encoding = Encoding.UTF8;

  /// <summary>
  /// Decode an application file.
  /// </summary>
  /// <param name="stream"></param>
  /// <param name="trace"></param>
  /// <exception cref="InvalidDataException"></exception>
  protected override void Decode(FileStream stream, bool trace = false)
  {
    DataSourceData? dataSource = null;
    MethodData? method = null;
    IHasScreens? objectWithScreens = null;
    IHasFields? objectWithFields = null;
    JobData? job = null;
    FieldData? field = null;
    TaskData? task = null;
    FriendlyDatabaseData? friendlyDatabase = null;
    EnumerationValueData? enumerationValue = null;
    ConnectEntityData? connectEntity = null;
    ApplicationFeatureData? applicationFeature = null;

    Magic = stream.ReadInt16();

    encoding = Magic switch
    {
      32767 => Extensions.DosEncoding,
      32757 => Extensions.WindowsEncoding,
      32747 => Encoding.UTF8,
      _ => throw new InvalidDataException($"Invalid magic number in file {FileName}, number found = {Magic}"),
    };

    while (stream.Position < stream.Length)
    {
      var objectType = (ObjectTypeEnum)stream.ReadEnum(typeof(ObjectTypeEnum));
      try
      {
        // Before anything else:
        // Figure out where screens need to be attached. 
        // Method screens immediately follow the screen object.
        // Anything else resets the destination to datasource
        if (objectType is not ObjectTypeEnum.Screen)
        {
          objectWithScreens = dataSource;
        }

        switch (objectType)
        {
          case ObjectTypeEnum.Application:
            ObjectType = objectType;
            ReadProperties(this, Properties, stream, encoding, trace);
            break;

          case ObjectTypeEnum.ApplicationTitle:
            Titles.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.ApplicationSetting:
            Settings = new ApplicationSettingData(objectType, stream, encoding, trace);
            break;

          case ObjectTypeEnum.DataLanguage:
            Settings!.Add(new DataLanguageData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.DataSource:
            objectWithScreens = dataSource = new DataSourceData(objectType, stream, encoding, trace);
            DataSources.Add(dataSource);
            break;

          case ObjectTypeEnum.DataSourceRights:
            dataSource!.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.DataSourceText:
            dataSource!.Texts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.ConnectEntity:
            dataSource!.Add(connectEntity = new ConnectEntityData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.ConnectEntityText:
            connectEntity!.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.ConnectEntityRights:
            connectEntity!.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FriendlyDatabase:
            dataSource!.Add(friendlyDatabase = new FriendlyDatabaseData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FriendlyDatabaseText:
            friendlyDatabase!.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FriendlyDatabaseRights:
            friendlyDatabase!.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.Screen:
            var text = new LanguageTextData(objectType, stream, encoding, trace);
            objectWithScreens!.Screens.Add(text);
            break;

          case ObjectTypeEnum.Method:
            objectWithScreens = method = new MethodData(objectType, stream, encoding, trace);
            dataSource!.Methods.Add(method);
            break;

          case ObjectTypeEnum.MethodText:
            method!.Texts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.MethodRights:
            method!.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.MethodSortSpecification:
            method!.Add(new MethodSortSpecificationData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.OutputJob:
            var outputJob = new OutputJobData(objectType, stream, encoding, trace);
            job = outputJob;
            objectWithFields = outputJob;
            dataSource!.Add(outputJob);
            break;

          case ObjectTypeEnum.JobTitle:
            job!.Texts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.JobDescription:
            job!.Descriptions.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.JobRights:
            job!.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.ExportJob:
            var exportJob = new ExportJobData(objectType, stream, encoding, trace);
            job = exportJob;
            dataSource!.Add(exportJob);
            break;

          case ObjectTypeEnum.Task:
            objectWithFields = task = new TaskData(objectType, stream, encoding, trace);
            dataSource!.Add(task);
            break;

          case ObjectTypeEnum.TaskTitle:
            task!.Texts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.TaskRights:
            task!.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.Field:
            objectWithFields!.Fields.Add(field = new FieldData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FieldName:
            field!.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.EnumerationValue:
            field!.Add(enumerationValue = new EnumerationValueData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.EnumerationValueText:
            enumerationValue!.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.Defaults:
            field!.Defaults.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FieldMethodText:
            field!.MethodTexts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FieldLabelText:
            field!.LabelTexts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FieldRelationText:
            field!.RelationTexts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FieldReverseRelationText:
            field!.ReverseRelationTexts.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.LanguageFieldTag:
            field!.LanguageTags.Add(new LanguageTextData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.MergeTag:
            field!.MergeTags.Add(new MergeTagData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.RecordTypeRights:
            field!.RecordTypeRoles.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.User:
            Users.Add(new UserData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.FileAccessControl:
            FacsList.Add(new FacsData(objectType, stream, encoding, trace));
            break;

          case ObjectTypeEnum.ApplicationFeature:
            applicationFeature = new ApplicationFeatureData(objectType, stream, encoding, trace);
            ApplicationFeatures.Add(applicationFeature);
            break;

          case ObjectTypeEnum.ApplicationFeatureRole:
            applicationFeature?.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, trace));
            break;

          default:
            throw new InvalidDataException($"Invalid object type '{objectType}' in '{stream.Name}' location {stream.Position:n0}");
        }
      }
      catch (Exception ex)
      {
        if (ex is InvalidDataException)
        {
          throw;
        }
        throw new InvalidDataException($"Error in '{objectType}' in '{stream.Name}' location {stream.Position:n0} {ex.Message}");
      }
    }
  }

  /// <summary>
  /// The name of the Application
  /// </summary>
  public string? Name
  {
    get; set;
  }

  /// <summary>
  /// The list of data sources
  /// </summary>
  public List<DataSourceData> DataSources
  {
    get;
    set;
  } = [];

  /// <summary>
  /// The list of titles for this application
  /// </summary>
  [DDesigner(DDesignerPropertyTypeEnum.LanguageText, "Titles", 0)]
  public TextsList Titles
  {
    get;
    set;
  } = [];

  /// <summary>
  /// A list of users
  /// </summary>
  public List<UserData> Users
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// The general settings for this application
  /// </summary>
  [DDesigner(DDesignerPropertyTypeEnum.Object, "Settings", 2)]
  public ApplicationSettingData? Settings
  {
    get;
    set;
  }

  /// <summary>
  /// A list of FACS (File Access Control System) declarations
  /// This is used in adapl to read and write to databases other than the current one.
  /// </summary>
  public List<FacsData> FacsList
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of application features.
  /// </summary>
  public List<ApplicationFeatureData> ApplicationFeatures
  {
    get;
    private set;
  } = [];

  internal static PropertyList Properties =
  [
    new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1, DataTypesEnum.String, "Name"),
    new PropertyMap (2, DataTypesEnum.Skip),
    new PropertyMap (3, DataTypesEnum.Skip),
    new PropertyMap (4, DataTypesEnum.Skip),
    new PropertyMap (5, DataTypesEnum.Skip),
    new PropertyMap (6, DataTypesEnum.Skip)
  ];

  internal override ChildrenList[] Children =>
   [
      new ChildrenList(DataSources, DataSourceData.Properties),
      new ChildrenList(FacsList, FacsData.Properties),
      new ChildrenList(Titles, LanguageTextData.Properties),
      new ChildrenList(Users, UserData.Properties),
      new ChildrenList([Settings!], ApplicationSettingData.Properties), // the settings are not a list, but stick it in an array to conform to the writer's expectation
      new ChildrenList(ApplicationFeatures, ApplicationFeatureData.Properties)
   ];

  private readonly JsonSerializerOptions options = new()
  {
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
  };

  /// <summary>
  /// Save the object to Json
  /// </summary>
  /// <param name="fileName"></param>
  public void SaveToJson(string fileName)
    => File.WriteAllText(fileName, Utilities.JsonSerializer.Serialize(this, options));

  /// <summary>
  /// Applications have a file extension of .pbk 
  /// This is for historical reasons, pbk stood for "Parameter BlocK"
  /// </summary>
  public static string Extension => ".pbk";

  /// <summary>
  /// The default title for the application. 
  /// This is the English title.
  /// </summary>
  [JsonIgnore]
  public string? Title => Titles.Count > 0 ? Titles[0].Text : null;
}

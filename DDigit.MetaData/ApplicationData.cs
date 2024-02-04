namespace DDigit.MetaData;

public class ApplicationData: FileData
{
  public ApplicationData(string fileName, bool trace) : base (ObjectTypeEnum.Application, fileName, trace)
  {
  }

  public ApplicationData() : base(ObjectTypeEnum.Application, null, false)
  {
  }

  protected override void Encode(Stream stream)
  {
    stream.WriteInt16(Magic);
    WriteProperties(this, Properties, Children, stream, encoding);
  }

  private Encoding encoding = Encoding.UTF8;

  protected override void Decode(Stream stream, bool trace)
  {
    DataSourceData? dataSource = null;
    MethodData? method = null;
    IHasScreens? parent = null;
    JobData? job = null;
    TaskData? task = null;
    FieldData? field = null;
    FriendlyDatabaseData? friendlyDatabase = null;
    EnumerationValueData? enumerationValue = null;
    ConnectEntityData? connectEntity = null;

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
        switch (objectType)
        {
          case ObjectTypeEnum.Application:
            ObjectType = objectType;
            ReadProperties(this, Properties, stream, encoding, FileName, trace);
            break;

          case ObjectTypeEnum.ApplicationTitle:
            Titles.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.ApplicationSetting:
            Settings = new ApplicationSettingData(objectType, stream, encoding, FileName, trace);
            break;

          case ObjectTypeEnum.DataLanguage:
            Settings?.Add(new DataLanguageData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.DataSource:
            parent = dataSource = new DataSourceData(objectType, stream, encoding, FileName, trace);
            DataSources.Add(dataSource);
            break;

          case ObjectTypeEnum.DataSourceRights:
            dataSource?.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.DataSourceText:
            dataSource?.Texts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.ConnectEntity:
            dataSource?.Add(connectEntity = new ConnectEntityData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.ConnectEntityText:
            connectEntity?.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.ConnectEntityRights:
            connectEntity?.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FriendlyDatabase:
            dataSource?.Add(friendlyDatabase = new FriendlyDatabaseData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FriendlyDatabaseText:
            friendlyDatabase?.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FriendlyDatabaseRights:
            friendlyDatabase?.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.Screen:
            parent?.Screens.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.Method:
            parent = method = new MethodData(objectType, stream, encoding, FileName, trace);
            dataSource?.Methods.Add(method);
            break;

          case ObjectTypeEnum.MethodText:
            method?.Texts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.MethodRights:
            method?.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.MethodSortSpecification:
            method?.Add(new MethodSortSpecificationData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.OutputJob:
            var outputJob = new OutputJobData(objectType, stream, encoding, FileName, trace);
            job = outputJob;
            dataSource?.Add(outputJob);
            break;

          case ObjectTypeEnum.OutputJobTitle:
            job?.Texts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.OutputJobDescription:
            job?.Descriptions.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.OutputJobRights:
            job?.AccessRights.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.ExportJob:
            var exportJob = new ExportJobData(objectType, stream, encoding, FileName, trace);
            job = exportJob;
            dataSource?.Add(exportJob);
            break;

          case ObjectTypeEnum.Task:
            task = new TaskData(objectType, stream, encoding, FileName, trace);
            dataSource?.Add(task);
            break;

          case ObjectTypeEnum.TaskTitle:
            task?.Texts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.TaskRights:
            task?.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.Field:
            task?.Fields.Add(field = new FieldData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldName:
            field?.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.EnumerationValue:
            field?.Add(enumerationValue = new EnumerationValueData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.EnumerationValueText:
            enumerationValue?.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.Defaults:
            field?.Defaults.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldMethodText:
            field?.MethodTexts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldLabelText:
            field?.LabelTexts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldRelationText:
            field?.RelationTexts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldReverseRelationText:
            field?.ReverseRelationTexts.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.LanguageFieldTag:
            field?.LanguageTags.Add(new LanguageTextData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.MergeTag:
            field?.MergeTags.Add(new MergeTagData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.RecordTypeRights:
            field?.RecordTypeRoles.Add(new AccessRightsData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.User:
            Users.Add(new UserData(objectType, stream, encoding, FileName, trace));
            break;

          case ObjectTypeEnum.FileAccessControl:
            FacsList.Add(new FacsData(objectType, stream, encoding, FileName, trace));
            break;

          default:
            throw new InvalidDataException($"Invalid object type '{objectType}' in '{FileName}' location {stream.Position:n0}");
        }
      }
      catch (Exception ex)
      {
        if (ex is InvalidDataException)
        {
          throw;
        }
        throw new InvalidDataException($"Error in '{objectType}' in '{FileName}' location {stream.Position:n0} {ex.Message}");
      }
    }
  }

  /// <summary>
  /// The name of the Application
  /// </summary>
  public string? Name
  {
    get; private set;
  }

  /// <summary>
  /// The list of data sources
  /// </summary>
  public List<DataSourceData> DataSources
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// The list of titles for this application
  /// </summary>
  public List<LanguageTextData> Titles
  {
    get;
    private set;
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
  public ApplicationSettingData? Settings
  {
    get;
    private set;
  }

  public List<FacsData> FacsList
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

  internal override (PropertyList, IEnumerable<object>)[] Children =>
   [
      (DataSourceData.Properties, DataSources),
      (FacsData.Properties, FacsList),
      (LanguageTextData.Properties, Titles),
      (UserData.Properties, Users),
      (ApplicationSettingData.Properties, new ApplicationSettingData[]{Settings!}), // the settings are not a list, but stick it in an array to conform to the writer's expectation
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
  public void SaveToJson(string fileName) => File.WriteAllText(fileName, JsonSerializer.Serialize(this, options));

  public override string Extension => ".pbk";
}

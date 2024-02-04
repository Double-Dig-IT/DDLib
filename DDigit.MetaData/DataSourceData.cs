namespace DDigit.MetaData;

public class DataSourceData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace), IHasScreens
{

  /// <summary>
  /// The type of this data source
  /// </summary>
  public DataSourceTypeEnum Type
  {
    get; private set;
  }

  /// <summary>
  /// The name of the data source
  /// </summary>
  public string? Name
  {
    get; private set;
  }

  /// <summary>
  /// The path of the database
  /// </summary>
  public string? DatabasePath
  {
    get; private set;
  }

  /// <summary>
  /// The name of the dataset
  /// </summary>
  public string? Dataset
  {
    get; private set;
  }

  /// <summary>
  /// The help key for explanations
  /// </summary>
  public string? HelpKey
  {
    get; private set;
  }

  /// <summary>
  /// The global unique identifier for this dataset
  /// </summary>
  public string? GUID
  {
    get; private set;
  }

  /// <summary>
  /// A list of texts
  /// </summary>
  public List<LanguageTextData> Texts
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of screens
  /// </summary>
  public List<LanguageTextData> Screens
  {
    get;
    private set;
  } = [];


  public List<MethodData> Methods
  {
    get; private set;
  } = [];

  /// <summary>
  /// The list of output jobs
  /// </summary>
  public List<OutputJobData> OutputJobs
  {
    get;
    private set;
  } = [];

  public List<ExportJobData> ExportJobs
  {
    get;
    private set;
  } = [];

  public List<AccessRightsData> AccessRights
  {
    get;
    private set;
  } = [];

  public List<FriendlyDatabaseData> FriendlyDatabases
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// The list of tasks for this data source
  /// </summary>
  public List<TaskData> Tasks
  {
    get; private set;
  } = [];

  public List<ConnectEntityData> ConnectEntities
  {
    get; private set;
  } = [];

  internal void Add(FriendlyDatabaseData friendlyDatabase) => FriendlyDatabases.Add(friendlyDatabase);

  internal void Add(ConnectEntityData connectEntity) => ConnectEntities.Add(connectEntity);

  internal void Add(OutputJobData outputJob) => OutputJobs.Add(outputJob);

  internal void Add(ExportJobData exportJob) => ExportJobs.Add(exportJob);

  internal void Add(TaskData task) => Tasks.Add(task);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.Enum,   "Type", typeof(DataSourceTypeEnum)),
    new PropertyMap (2,  DataTypesEnum.String, "Name"),
    new PropertyMap (3,  DataTypesEnum.String, "DatabasePath"),
    new PropertyMap (4,  DataTypesEnum.String, "Dataset"),
    new PropertyMap (5,  DataTypesEnum.String, "HelpKey"),
    new PropertyMap (6,  DataTypesEnum.Skip),
    new PropertyMap (7,  DataTypesEnum.Skip),
    new PropertyMap (8,  DataTypesEnum.Skip),
    new PropertyMap (9,  DataTypesEnum.Skip),
    new PropertyMap (10, DataTypesEnum.Skip),
    new PropertyMap (11, DataTypesEnum.Skip),
    new PropertyMap (12, DataTypesEnum.Skip),
    new PropertyMap (13, DataTypesEnum.Skip),
    new PropertyMap (14, DataTypesEnum.String, "GUID")
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
   [
      (LanguageTextData.Properties, Screens),
      (MethodData.Properties, Methods),
      (JobData.Properties, OutputJobs),
      (LanguageTextData.Properties, Texts),
      (AccessRightsData.Properties, AccessRights),
      (JobData.Properties, ExportJobs),
      (FriendlyDatabaseData.Properties, FriendlyDatabases),
      //(CloudObjectData, CloudObjects), ToDo: Add cloud objects
      (TaskData.Properties, Tasks),
      (ConnectEntityData.Properties, ConnectEntities),
   ];
}
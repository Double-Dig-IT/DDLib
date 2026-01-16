namespace DDigit.MetaData;

/// <summary>
/// Data for a single data source
/// </summary>
public class DataSourceData : BaseData, IHasScreens
{
  /// <summary>
  /// Constructor to create an empty <see cref="DataSourceData"/>
  /// </summary>
  public DataSourceData() : base(ObjectTypeEnum.DataSource, Properties)
  {
  }

  /// <summary>
  /// Constructor to read <see cref="DataSourceData"/> from disk
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public DataSourceData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) : base(objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// The type of this data source
  /// </summary>
  public DataSourceTypeEnum Type
  {
    get; private set;
  }

  /// <summary>
  /// The title of the data source
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
        Texts.Add(new LanguageTextData(ObjectTypeEnum.DataSourceText, value));
      }
    }
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
    get;
    private set;
  }

  /// <summary>
  /// The global unique identifier for this dataset
  /// </summary>
  public string? GUID
  {
    get;
    private set;
  }

  /// <summary>
  /// A list of texts
  /// </summary>
  public TextsList Texts
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of screens
  /// </summary>
  public TextsList Screens
  {
    get;
    private set;
  } = [];


  /// <summary>
  /// A list of methods for this data source.
  /// </summary>
  public List<MethodData> Methods
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// The list of output jobs
  /// </summary>
  public List<OutputJobData> OutputJobs
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of export jobs for this data source.
  /// </summary>
  public List<ExportJobData> ExportJobs
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// Access Control List for the data source.
  /// </summary>
  public AccessControlList AccessRights
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of friendly databases for this data source.
  /// friendly databases are databases where data might be imported from.
  /// </summary>
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

  /// <summary>
  /// A list of connect entities.
  /// Introduced in Axiell Collections?
  /// </summary>
  public List<ConnectEntityData> ConnectEntities
  {
    get; private set;
  } = [];

  /// <summary>
  /// Override for debugging.
  /// </summary>
  /// <returns></returns>
  public override string? ToString() => Title;

  internal void Add(FriendlyDatabaseData friendlyDatabase) => FriendlyDatabases.Add(friendlyDatabase);

  internal void Add(ConnectEntityData connectEntity) => ConnectEntities.Add(connectEntity);

  internal void Add(OutputJobData outputJob) => OutputJobs.Add(outputJob);

  internal void Add(ExportJobData exportJob) => ExportJobs.Add(exportJob);

  internal void Add(TaskData task) => Tasks.Add(task);

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.Enum,   "Type", typeof(DataSourceTypeEnum)),
    new PropertyMap (2,  DataTypesEnum.String, "Title"),
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

  internal override ChildrenList[] Children =>
   [
     new ChildrenList (Screens, LanguageTextData.Properties),
      new ChildrenList(Methods, MethodData.Properties),
      new ChildrenList(OutputJobs, JobData.Properties),
      new ChildrenList(Texts, LanguageTextData.Properties),
      new ChildrenList(AccessRights, AccessRightsData.Properties),
      new ChildrenList(ExportJobs, JobData.Properties),
      new ChildrenList(FriendlyDatabases, FriendlyDatabaseData.Properties),
      //(CloudObjectData.Properties, CloudObjects), // ToDo: Add cloud objects
      new ChildrenList(Tasks, TaskData.Properties),
      new ChildrenList(ConnectEntities, ConnectEntityData.Properties),
   ];
}
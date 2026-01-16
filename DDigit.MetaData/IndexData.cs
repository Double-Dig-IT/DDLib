namespace DDigit.MetaData;

/// <summary>
/// Metadata for an index table
/// </summary>
public class IndexData : BaseData
{

  /// <summary>
  /// ConstContructor that read in index object
  /// </summary>
  /// <param name="objectType">The object type</param>
  /// <param name="stream">The filestream to read from</param>
  /// <param name="encoding">Encoding</param>
  /// <param name="database">The databasedata for which to read the index data</param>
  /// <param name="trace">debug flag</param>
  public IndexData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, DatabaseData database, bool trace) :
    base(objectType, stream, encoding, Properties, trace)
  {
    this.database = database;
  }

  /// <summary>
  /// Default constructor for an Index object.
  /// </summary>
  /// <param name="database"></param>
  public IndexData(DatabaseData database) : base(ObjectTypeEnum.Index)
  {
    this.database = database;
  }

  /// <summary>
  /// The database to which this index belongs
  /// </summary>
  private readonly DatabaseData database;

  /// <summary>
  /// Get the Sql server table for this index
  /// </summary>
  [JsonIgnore]
  public string TableName => Name == "priref" ? database.Name! : $"{database?.Name}_{Name}";

  /// <summary>
  /// Does this index exist as a table?
  /// </summary>
  [JsonIgnore]
  public bool? TableExists { get; set; }

  private short MaxElementCount(string propertyName)
   => Math.Max(Properties.GetElementCount(propertyName), ElementCount ?? 0);

  /// <summary>
  /// The name of the index
  /// </summary>
  public string? Name
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Obsolete: block size was used for non-SQL database
  /// </summary>
  internal short BlockSize
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete: the number of nodes that were cached for non-SQL database
  /// </summary>
  internal short NodesCached
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete: retrieval block size was used for non-SQL database
  /// </summary>
  internal short RetrievalBlockSize
  {
    get; private set;
  }

  /// <summary>
  /// The key type
  /// </summary>
  public IndexTypeEnum Type
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The key size
  /// </summary>
  public short Length
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Tag for the index
  /// </summary>
  public string? Tag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Obsolete, was used for File based Adlib: the physical key type
  /// </summary>
  /// <remarks>
  /// Is somehow still relevant: it needs to be set correctly in order for Designer to correctly create the index.
  /// </remarks>
  public PhysicalKeyTypeEnum PhysicalKeyType
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Obsolete, was used for File based Adlib: the physical key size
  /// </summary>
  internal short PhysicalKeySize
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete, was used for File based Adlib: the file number for the index
  /// </summary>
  internal short FileNumber
  {
    get; private set;
  }

  /// <summary>
  /// Is re-indexing needed, for instance after adding a new index tag
  /// </summary>
  public bool ReIndexingNeeded
  {
    get; private set;
  }

  /// <summary>
  /// Additional index tags
  /// </summary>
  public string? ExtraIndexTags
  {
    get; private set;
  }

  /// <summary>
  /// Index only the first occurrence of a field
  /// </summary>
  public bool FirstOccurrenceOnly
  {
    get; private set;
  }

  /// <summary>
  /// Index must contain unique keys
  /// </summary>
  public bool Unique
  {
    get; private set;
  }

  /// <summary>
  /// The tag in the record that contains the domains
  /// </summary>
  public string? DomainTag
  {
    get; private set;
  }

  /// <summary>
  /// How phonetic indexes are constructed (never uses as far as i know)
  /// </summary>
  internal PhoneticTypeEnum PhoneticType
  {
    get; private set;
  }

  /// <summary>
  /// How ISO dates are completed before they get written.
  /// </summary>
  public DateCompletionEnum DateCompletion
  {
    get; private set;
  }

  /// <summary>
  /// Metadata type (how is this used?)
  /// </summary>
  public MetaDataTypeEnum MetaDataType
  {
    get; private set;
  }

  /// <summary>
  /// Tree separator (how is this used?)
  /// </summary>
  public string? TreeSeparator
  {
    get; private set;
  }

  /// <summary>
  /// Show a summary of indexing properties
  /// </summary>
  /// <returns>A string with properties</returns>
  public override string ToString() => $"{Tag}, {Name}, {Type}, {Length} Unique: {Unique}";

  internal static PropertyList Properties =
  [
     new PropertyMap (0,  DataTypesEnum.Int16,  nameof(ElementCount)),
     new PropertyMap (1,  DataTypesEnum.String, nameof(Name)),
     new PropertyMap (2,  DataTypesEnum.Int16,  nameof(BlockSize)),
     new PropertyMap (3,  DataTypesEnum.Int16,  nameof(NodesCached)),
     new PropertyMap (4,  DataTypesEnum.Int16,  nameof(RetrievalBlockSize)),
     new PropertyMap (5,  DataTypesEnum.Int16,  nameof(Type), typeof(IndexTypeEnum)),
     new PropertyMap (6,  DataTypesEnum.Int16,  nameof(Length)),
     new PropertyMap (7,  DataTypesEnum.String, nameof(Tag)),
     new PropertyMap (8,  DataTypesEnum.Int16,  nameof(PhysicalKeyType), typeof(PhysicalKeyTypeEnum)),
     new PropertyMap (9,  DataTypesEnum.Int16,  nameof(PhysicalKeySize)),
     new PropertyMap (10, DataTypesEnum.Int16,  nameof(FileNumber)),
     new PropertyMap (11, DataTypesEnum.Bool,   nameof(ReIndexingNeeded)),
     new PropertyMap (12, DataTypesEnum.String, nameof(ExtraIndexTags)),
     new PropertyMap (13, DataTypesEnum.Bool,   nameof(FirstOccurrenceOnly)),
     new PropertyMap (14, DataTypesEnum.Bool,   nameof(Unique)),
     new PropertyMap (15, DataTypesEnum.String, nameof(DomainTag)),
     new PropertyMap (16, DataTypesEnum.Int16,  nameof(PhoneticType), typeof(PhoneticTypeEnum)),
     new PropertyMap (17, DataTypesEnum.Int16,  nameof(DateCompletion), typeof(DateCompletionEnum)),
     new PropertyMap (18, DataTypesEnum.Int16,  nameof(MetaDataType), typeof(MetaDataTypeEnum)),
     new PropertyMap (19, DataTypesEnum.String, nameof(TreeSeparator))
  ];

  /// <summary>
  /// The list of tags that are indexed in this index
  /// </summary>
  public List<string> IndexTags
  {
    get
    {
      if (field is null)
      {
        field = [Tag!];
        if (ExtraIndexTags is not null)
        {
          field.AddRange(ExtraIndexTags.Split(separators,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
      }
      return field;
    }
  }

  /// <summary>
  /// A boolean which indicates whether the index has a domain column.
  /// </summary>
  public bool HasDomain => !string.IsNullOrEmpty(DomainTag);

  /// <summary>
  /// A boolean to determine if the full text table should be used.
  /// This should be used if the database is full text enabledm but not for
  /// - unique indexes
  /// - alphanumeric indexes 
  /// - boolean indexes
  /// </summary>
  public bool UseFullText => database.IsFullTextEnabled && !Unique && Type != IndexTypeEnum.AlphaNumeric && Type != IndexTypeEnum.Boolean;


  private static readonly char[] separators = [',', ' '];
}
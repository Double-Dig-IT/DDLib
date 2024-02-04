namespace DDigit.MetaData;

public class IndexData : BaseData
{

  public IndexData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, DatabaseData database, bool trace) : base (objectType, stream, encoding, fileName, Properties, trace)
  {
    this.database = database;
  }

  internal IndexData(DatabaseData database) : base (ObjectTypeEnum.Index, string.Empty)
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
  public string TableName => Name == "priref" ? database.Name! : $"{database?.Name}_{Name}";

  /// <summary>
  /// The name of the index
  /// </summary>
  public string? Name
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete: block size was used for non-SQL database
  /// </summary>
  internal int BlockSize
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete: the number of nodes that were cached for non-SQL database
  /// </summary>
  internal int NodesCached
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete: retrieval block size was used for non-SQL database
  /// </summary>
  internal int RetrievalBlockSize
  {
    get; private set;
  }

  /// <summary>
  /// The key type
  /// </summary>
  public IndexTypeEnum Type
  {
    get; private set;
  }

  /// <summary>
  /// The key size
  /// </summary>
  public int Length
  {
    get; private set;
  }

  /// <summary>
  /// Tag for the index
  /// </summary>
  public string? Tag
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete, was used for File based Adlib: the physical key type
  /// </summary>
  public PhysicalKeyTypeEnum PhysicalKeyType
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete, was used for File based Adlib: the physical key size
  /// </summary>
  internal int PhysicalKeySize
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete, was used for File based Adlib: the file number for the index
  /// </summary>
  internal int FileNumber
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

  public override string ToString() => $"{Tag}, {Name}, {Type}, {Length} Unique: {Unique}";

  internal static PropertyList Properties =
  [
     new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
     new PropertyMap (1,  DataTypesEnum.String, "Name"),
     new PropertyMap (2,  DataTypesEnum.Int16,  "BlockSize"),
     new PropertyMap (3,  DataTypesEnum.Int16,  "NodesCached"),
     new PropertyMap (4,  DataTypesEnum.Int16,  "RetrievalBlockSize"),
     new PropertyMap (5,  DataTypesEnum.Int16,  "Type", typeof(IndexTypeEnum)),
     new PropertyMap (6,  DataTypesEnum.Int16,  "Length"),
     new PropertyMap (7,  DataTypesEnum.String, "Tag"),
     new PropertyMap (8,  DataTypesEnum.Int16,  "PhysicalKeyType", typeof(PhysicalKeyTypeEnum)),
     new PropertyMap (9,  DataTypesEnum.Int16,  "PhysicalKeySize"),
     new PropertyMap (10, DataTypesEnum.Int16,  "FileNumber"),
     new PropertyMap (11, DataTypesEnum.Bool,   "ReIndexingNeeded"),
     new PropertyMap (12, DataTypesEnum.String, "ExtraIndexTags"),
     new PropertyMap (13, DataTypesEnum.Bool,   "FirstOccurrenceOnly"),
     new PropertyMap (14, DataTypesEnum.Bool,   "Unique"),
     new PropertyMap (15, DataTypesEnum.String, "DomainTag"),
     new PropertyMap (16, DataTypesEnum.Int16,  "PhoneticType", typeof(PhoneticTypeEnum)),
     new PropertyMap (17, DataTypesEnum.Int16,  "DateCompletion", typeof(DateCompletionEnum)),
     new PropertyMap (18, DataTypesEnum.Int16,  "MetaDataType", typeof(MetaDataTypeEnum)),
     new PropertyMap (19, DataTypesEnum.String, "TreeSeparator")
  ];

  public List<string>? indexTags = null;
  public List<string> IndexTags
  {
    get
    {
      if (indexTags == null)
      {
        indexTags = [Tag!];
        if (ExtraIndexTags != null)
        {
          indexTags.AddRange(ExtraIndexTags.Split(separators,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
      }
      return indexTags;
    }
  }

  public bool HasDomain => !string.IsNullOrEmpty(DomainTag);

  private static readonly char[] separators = [',', ' '];
}
using System.IO;

namespace DDigit.MetaData;

public class DatabaseData : FileData
{
  /// <summary>
  /// Read a database.inf file as an object.
  /// </summary>
  /// <param name="fileName">The path to the inf file to read (e.g. c:\museum\data\collect.inf)</param>
  /// <param name="trace">A boolean which can be set to true to follow the parsing via the Console</param>
  public DatabaseData(string? fileName, bool trace = false) : base(ObjectTypeEnum.Database,
    AddExtension(fileName, ".inf"), trace)
  {
    GetRecordMetaDataFields();
    GetLocationFields();
    GetAutoNumberingFields();
    CreateFullTextField();
  }

  internal void GetAutoNumberingFields() 
    => AutomaticNumberingFields = Fields.Where(field => field.IsAutoNumberField);
  
  public void GetLocationFields()
  {
    LocationFields = new LocationFieldData
    {
      Authorizer = FindFieldByTagOrName("current_location.authoriser"),
      AuthorizerId = FindFieldByTagOrName("current_location.authoriser.lref"),
      Barcode = FindFieldByTagOrName("current_location.barcode"),
      Context = FindFieldByTagOrName("current_location.context"),
      Executor = FindFieldByTagOrName("current_location.executor"),
      Id = FindFieldByTagOrName("current_location.lref"),
      Name = FindFieldByTagOrName("current_location.name"),
      Notes = FindFieldByTagOrName("current_location.notes"),
      StartDate = FindFieldByTagOrName("current_location.date"),
      StartTime = FindFieldByTagOrName("current_location.time"),
      Suitability = FindFieldByTagOrName("current_location.suitability"),
      Type = FindFieldByTagOrName("current_location.type")
    };

    LocationHistoryFields = new LocationFieldData
    {
      Authorizer = FindFieldByTagOrName("location.history.authoriser"),
      AuthorizerId = FindFieldByTagOrName("location.history.authoriser.lref"),
      Barcode = FindFieldByTagOrName("location.history.barcode"),
      Context = FindFieldByTagOrName("location.history.context"),
      EndDate = FindFieldByTagOrName("location.history.date.end"),
      EndTime = FindFieldByTagOrName("location.history.removal_time"),
      Executor = FindFieldByTagOrName("location.history.executor"),
      Id = FindFieldByTagOrName("location.history.lref"),
      Name = FindFieldByTagOrName("location.history.name"),
      Notes = FindFieldByTagOrName("location.history.notes"),
      StartDate = FindFieldByTagOrName("location.history.date.start"),
      StartTime = FindFieldByTagOrName("location.history.time"),
      Suitability = FindFieldByTagOrName("location.history.suitability"),
      Type = FindFieldByTagOrName("location.history.type")
    };
  }

  public void GetRecordMetaDataFields()
  {
    InputGroup = new EditFieldData
    {
      Name = FindFieldByTagOrName("input.name"),
      Date = FindFieldByTagOrName("input.date"),
      Time = FindFieldByTagOrName("input.time"),
      Source = FindFieldByTagOrName("input.source"),
      Notes = FindFieldByTagOrName("input.notes")
    };

    EditGroup = new EditFieldData
    {
      Name = FindFieldByTagOrName("edit.name"),
      Date = FindFieldByTagOrName("edit.date"),
      Time = FindFieldByTagOrName("edit.time"),
      Source = FindFieldByTagOrName("edit.source"),
      Notes = FindFieldByTagOrName("edit.notes")
    };

    EditHistoryGroup = new EditFieldData
    {
      Name = FindFieldByTagOrName("edit.history.name"),
      Date = FindFieldByTagOrName("edit.history.date"),
      Time = FindFieldByTagOrName("edit.history.time"),
      Source = FindFieldByTagOrName("edit.history.source"),
      Notes = FindFieldByTagOrName("edit.history.notes")
    };
  }

  public DatabaseData() : base(ObjectTypeEnum.Database, null, false)
  {
    CreateFullTextField();
  }

  private void CreateFullTextField()
  {
    fullTextFieldData.Database = this;
    fullTextFieldData.IndexList?.Add
    (
      new IndexData(this)
    );
  }

  protected override void Encode(Stream stream)
  {
    stream.WriteFixedLengthString(Version, VersionSize);
    stream.WriteInt16(Magic);
    WriteProperties(this, Properties, Children, stream, TextEncoding);
  }

  private const string VersionId = "ADL20";

  protected override void Decode(Stream stream, bool trace)
  {
    FieldData? field = null;
    EnumerationValueData? enumeration = null;
    DatasetData? dataset = null;
    ExternalSourceData? externalSource = null;
    InternalLinkData? internalLink = null;

    Version = stream.ReadFixedLengthString(VersionSize);
    if (Version != VersionId)
    {
      throw new InvalidDataException($"Invalid version identifier in file {FileName}, found = {Version}, but expected {VersionId}");
    }

    Magic = stream.ReadInt16();
    TextEncoding = Magic switch
    {
      32764 => Extensions.DosEncoding,
      32754 => Extensions.WindowsEncoding,
      32744 => Encoding.UTF8,
      _ => throw new InvalidDataException($"Invalid magic number in file {FileName}, number found = {Magic}"),
    };

    while (stream.Position < stream.Length)
    {
      var objectType = (ObjectTypeEnum)stream.ReadEnum(typeof(ObjectTypeEnum));
      try
      {

        switch (objectType)
        {
          case ObjectTypeEnum.Database:
            ObjectType = objectType;
            ReadProperties(this, Properties!, stream, TextEncoding, FileName, trace);
            break;

          case ObjectTypeEnum.Index:
            Indexes.Add(new IndexData(objectType, stream, TextEncoding, FileName, this, trace));
            break;

          case ObjectTypeEnum.Field:
            field = new FieldData(objectType, stream, TextEncoding, FileName, trace, this);
            if (dataset != null)
            {
              dataset.Fields.Add(field);
            }
            else
            {
              Fields.Add(field);
            }
            break;

          case ObjectTypeEnum.FieldName:
            field?.Names.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.Defaults:
            field?.Defaults.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldMethodText:
            field?.MethodTexts.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldLabelText:
            field?.LabelTexts.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldRelationText:
            field?.RelationTexts.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldReverseRelationText:
            field?.ReverseRelationTexts.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.DatabaseRights:
            AccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.DatasetRights:
            dataset?.AccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.RecordTypeRights:
            field?.RecordTypeRoles.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FieldRights:
            field?.AccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.EnumerationValueRights:
            enumeration?.AccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.DefaultPointerFileRights:
            PointerFileAccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.CandidateTermRights:
            CandidateTermAccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.EnumerationValue:
            field?.EnumerationValues.Add(enumeration = new EnumerationValueData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.EnumerationValueText:
            enumeration?.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.MergeTag:
            field?.MergeTags.Add(new MergeTagData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.WriteBackTag:
            field?.WriteBackTags.Add(new MergeTagData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.MergeListTag:
            field?.MergeListTags.Add(new MergeTagData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.LanguageFieldTag:
            field?.LanguageTags.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.Dataset:
            Datasets.Add(dataset = new DatasetData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.InternalLink:
            InternalLinks.Add(internalLink = new InternalLinkData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.FeedbackLink:
            FeedbackLinks.Add(new FeedbackLinkData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.ExternalSourceInfo:
            field?.ExternalSources.Add(externalSource = new ExternalSourceData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.ExternalSourceName:
            externalSource?.Names.Add(new LanguageTextData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.ExternalSourceMapping:
            externalSource?.Mapping.Add(new ExternalSourceMappingData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.EnumerationValueRecordTypeRights:
            enumeration?.RecordTypeAccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.LinkControl:
            internalLink?.Add(new LinkNodeData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.DefaultRecordRights:
            DefaultRecordAccessRights.Add(new AccessRightsData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.DataLanguage:
            DefaultInvariantLanguages.Add(new DataLanguageData(objectType, stream, TextEncoding, FileName, trace));
            break;

          case ObjectTypeEnum.MetadataMapping:
            field?.MetadataMappings.Add(new MetadataMappingData(objectType, stream, TextEncoding, FileName, trace));
            break;

          default:
            throw new InvalidMetaDataException(objectType, FileName, stream.Position, null);
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
    InternalLinks.ForEach(internalLink => internalLink.AddLinkRefs(this));
    FindIndexedLinks();
  }

  private void FindIndexedLinks()
  {
    foreach (var field in Fields.Where(field => field.IsIndexedLink))
    {
      var index = Indexes.FirstOrDefault((i) => i.Tag == field.LinkIdTag);
      if (index != null)
      {
        IndexedLinks.Add(new IndexedLinkData(field, index));
        Indexes.Remove(index);
      }
    }
  }

  UpdateLinksData? updateLinks;
  [JsonIgnore]
  public UpdateLinksData UpdateLinks
  {
    get
    {
      updateLinks ??= new UpdateLinksData(this);
      return updateLinks;
    }
  }

  /// <summary>
  /// Version number of the inf file, obsolete should always be "ADL20"
  /// </summary>
  internal string Version
  {
    get;
    private set;
  } = VersionId;

  private const int VersionSize = 6;

  /// <summary>
  /// Name of the database
  /// </summary>
  public string? Name
  {
    get; private set;
  }

  private string GetExtensionPath(string adaplPath, string extension)
   => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(PhysicalPath)!, adaplPath + extension));

  /// <summary>
  /// Before storage Adapl to be executed
  /// </summary>
  public string? BeforeStorageAdapl
  {
    get;
    set
    {
      field = value;
      if (!string.IsNullOrWhiteSpace(value))
      {
        var pythonPath = GetExtensionPath(value, ".py");
        if (File.Exists(pythonPath))
        {
          BeforeStoragePythonScript = pythonPath;
        }
        var powerShellPath = GetExtensionPath(value, ".ps1");
        if (File.Exists(powerShellPath))
        {
          BeforeStoragePowerShellScript = powerShellPath;
        }
      }
    }
  }

  public string? BeforeStoragePythonScript { get; private set; }

  public string? BeforeStoragePowerShellScript { get; private set; }

  /// <summary>
  /// Image database
  /// </summary>
  public string? ImageDatabase
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete
  /// </summary>
  internal string? MagicString
  {
    get; private set;
  }

  /// <summary>
  /// Is record authorization used ?
  /// </summary>
  public bool? AuthorizationUsed
  {
    get; private set;
  }

  /// <summary>
  /// IAuthorization type (include, exclude or record based)
  /// </summary>
  public AuthorizationTypeEnum? AuthorizationType
  {
    get; private set;
  }

  /// <summary>
  /// The field that contains the authorization data
  /// </summary>
  public string? AuthorizationUserTag
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete : Minimum record size
  /// </summary>
  internal short MinimumRecordSize
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete : Logging file
  /// </summary>
  internal string? LoggingFile
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete : Locale ID
  /// </summary>
  internal short Locale
  {
    get; private set;
  }

  /// <summary>
  /// Adapl to run after retrieving a record
  /// </summary>
  public string? AfterRetrievalAdapl
  {
    get; set;
  }

  /// <summary>
  /// Adapl to run after retrieving a record
  /// </summary>
  public DatabaseTypeEnum DatabaseType
  {
    get; private set;
  }

  /// <summary>
  /// Adapl to run before copying a record
  /// </summary>
  public string? CopyRecordAdapl
  {
    get; set;
  }

  /// <summary>
  /// Obsolete: enable left truncation
  /// </summary>
  public bool? EnableLeftTruncation
  {
    get; private set;
  }

  /// <summary>
  /// Adapl to run before input of a record
  /// </summary>
  public string? BeforeInputAdapl
  {
    get; set;
  }

  /// <summary>
  /// Adapl to run before editing of a record
  /// </summary>
  public string? BeforeEditAdapl
  {
    get; set;
  }

  /// <summary>
  /// Adapl to run when entering or exiting a field
  /// </summary>
  public string? FieldAdapl
  {
    get; set;
  }

  /// <summary>
  /// The SQL database to use
  /// </summary>
  public string? DSN
  {
    get; set;
  }

  /// <summary>
  /// Obsolete: external table
  /// </summary>
  internal string? ExternalTable
  {
    get; private set;
  }

  /// <summary>
  /// The Sql user name to use, empty string means use Windows user
  /// </summary>
  public string? SqlUserId
  {
    get; private set;
  }

  /// <summary>
  /// The Sql password to use
  /// </summary>
  public string? SqlPassword
  {
    get; private set;
  }

  /// <summary>
  /// The encryption in use
  /// </summary>
  public EncryptionTypeEnum EncryptionType
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete: the sort identifier
  /// </summary>
  internal short SortId
  {
    get; private set;
  }

  /// <summary>
  /// The default access
  /// </summary>
  public AccessRightsEnum DefaultAccess
  {
    get; private set;
  }

  /// <summary>
  /// The tag of the field that contains the access rights.
  /// </summary>
  public string? RightsTag
  {
    get; private set;
  }

  /// <summary>
  /// The tag of the field that contains the record owner
  /// </summary>
  public string? RecordOwnerTag
  {
    get; private set;
  }

  /// <summary>
  /// The tag of the field that contains the record owner
  /// </summary>
  public string? SqlServer
  {
    get; private set;
  }

  /// <summary>
  /// If this is a thesaurus record, the tag of the field that contains the status.
  /// </summary>
  public string? ThesaurusTermStatusTag
  {
    get; private set;
  }

  /// <summary>
  /// If this is a thesaurus record, the tag of the field that contains the status.
  /// </summary>
  public string? DecimalSeparator
  {
    get; private set;
  }

  /// <summary>
  /// Include this field in the full text index in SQL server?
  /// </summary>
  public bool? IncludeInFullTextIndex
  {
    get; private set;
  } = new();

  /// <summary>
  /// Defines if journaling is on or off (SQL/Oracle only).
  /// </summary>
  public EnableJournalingOptionEnum? EnableJournal
  {
    get; private set;
  }

  /// <summary>
  /// Is the record owner an individual or a group.
  /// </summary>
  public RecordOwnerTypeEnum RecordOwnerType
  {
    get; private set;
  }

  /// <summary>
  /// Do we store the modification history.
  /// </summary>
  public bool? StoreModificationHistory
  {
    get; private set;
  }

  /// <summary>
  /// The tag for the field that contains the record type.
  /// </summary>
  public string? RecordTypeTag
  {
    get; private set;
  }

  /// <summary>
  /// A list of indexes
  /// </summary>
  public List<IndexData> Indexes
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of indexes
  /// </summary>
  public List<IndexedLinkData> IndexedLinks
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of Fields.
  /// </summary>
  public List<FieldData> Fields
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of Datasets.
  /// </summary>
  public List<DatasetData> Datasets
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// Find a dataset by its name
  /// </summary>
  /// <param name="name"></param>
  /// <returns>Dataset Info</returns>
  public DatasetData FindDatasetByName(string name) =>
    Datasets.First(dataset => dataset.Name == name);

  /// <summary>
  /// A list of internal links.
  /// </summary>
  public List<InternalLinkData> InternalLinks
  {
    get; private set;
  } = [];

  /// <summary>
  /// A list of access rights.
  /// </summary>
  public List<AccessRightsData> AccessRights
  {
    get; private set;
  } = [];

  /// <summary>
  /// A list of Feedback links.
  /// </summary>
  public List<FeedbackLinkData> FeedbackLinks
  {
    get; private set;
  } = [];

  /// <summary>
  /// A list of default access rights for pointer files.
  /// </summary>
  public List<AccessRightsData> PointerFileAccessRights
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of access rights for candidate terms.
  /// </summary>
  public List<AccessRightsData> CandidateTermAccessRights
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// The default record access rights.
  /// </summary>
  public List<AccessRightsData> DefaultRecordAccessRights
  {
    get;
    private set;
  } = [];

  /// <summary>
  /// A list of default invariant languages.
  /// </summary>
  public List<DataLanguageData> DefaultInvariantLanguages
  {
    get;
    private set;
  } = [];

  public override string? ToString() => Name;

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String, "Name"),
    new PropertyMap (5,  DataTypesEnum.String, "BeforeStorageAdapl"),
    new PropertyMap (6,  DataTypesEnum.String, "ImageDatabase"),
    new PropertyMap (7,  DataTypesEnum.String, "MagicString"),
    new PropertyMap (8,  DataTypesEnum.Bool,   "AuthorizationUsed"),
    new PropertyMap (9,  DataTypesEnum.Enum,   "AuthorizationType", typeof(AuthorizationTypeEnum)),
    new PropertyMap (10, DataTypesEnum.String, "AuthorizationUserTag"),
    new PropertyMap (12, DataTypesEnum.Int16,  "MinimumRecordSize"),
    new PropertyMap (13, DataTypesEnum.String, "LoggingFile"),
    new PropertyMap (17, DataTypesEnum.Int16,  "Locale"),
    new PropertyMap (18, DataTypesEnum.String, "AfterRetrievalAdapl"),
    new PropertyMap (19, DataTypesEnum.Enum,   "DatabaseType", typeof(DatabaseTypeEnum)),
    new PropertyMap (20, DataTypesEnum.String, "CopyRecordAdapl"),
    new PropertyMap (21, DataTypesEnum.Bool,   "EnableLeftTruncation"),
    new PropertyMap (22, DataTypesEnum.String, "BeforeInputAdapl"),
    new PropertyMap (23, DataTypesEnum.String, "BeforeEditAdapl"),
    new PropertyMap (24, DataTypesEnum.String, "FieldAdapl"),
    new PropertyMap (25, DataTypesEnum.String, "DSN"),
    new PropertyMap (26, DataTypesEnum.String, "ExternalTable"),
    new PropertyMap (29, DataTypesEnum.String, "SqlUserId"),
    new PropertyMap (30, DataTypesEnum.String, "SqlPassword"),
    new PropertyMap (31, DataTypesEnum.Enum32, "EncryptionType", typeof(EncryptionTypeEnum)),
    new PropertyMap (32, DataTypesEnum.Int16,  "SortId"),
    new PropertyMap (33, DataTypesEnum.Enum32, "DefaultAccess", typeof(AccessRightsEnum)),
    new PropertyMap (34, DataTypesEnum.String, "RightsTag"),
    new PropertyMap (35, DataTypesEnum.String, "RecordOwnerTag"),
    new PropertyMap (36, DataTypesEnum.String, "SqlServer"),
    new PropertyMap (37, DataTypesEnum.String, "ThesaurusTermStatusTag"),
    new PropertyMap (38, DataTypesEnum.Bool,   "IncludeInFullTextIndex"),
    new PropertyMap (39, DataTypesEnum.String, "DecimalSeparator"),
    new PropertyMap (42, DataTypesEnum.Enum,   "EnableJournal", typeof(EnableJournalingOptionEnum)),
    new PropertyMap (43, DataTypesEnum.Enum,   "RecordOwnerType", typeof(RecordOwnerTypeEnum)),
    new PropertyMap (44, DataTypesEnum.Bool,   "StoreModificationHistory"),
    new PropertyMap (46, DataTypesEnum.String, "RecordTypeTag")
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
   [
      (IndexData.Properties, Indexes),
      (FieldData.Properties, Fields),
      (DatasetData.Properties, Datasets),
      (InternalLinkData.Properties, InternalLinks),
      (FeedbackLinkData.Properties, FeedbackLinks),
      (AccessRightsData.Properties, AccessRights),
      (AccessRightsData.Properties, PointerFileAccessRights),
      (AccessRightsData.Properties, CandidateTermAccessRights),
      (AccessRightsData.Properties, DefaultRecordAccessRights),
      (DataLanguageData.Properties, DefaultInvariantLanguages)
   ];

  private readonly JsonSerializerOptions options = new()
  {
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull
  };

  /// <summary>
  /// Save the object to Json
  /// </summary>
  /// <param name="fileName"></param>
  public void SaveToJson(string fileName)
  {
    File.WriteAllText(fileName, Utilities.JsonSerializer.Serialize(this, options));
  }

  public string? GetFieldNameByTag(string? tag) => Fields.FirstOrDefault(field => field.Tag == tag)?.Name;

  public FieldData? FindFieldByTagOrName(string tagOrFieldName)
  {
    var (root, remainder) = FieldData.GetRoot(tagOrFieldName);
    return FindFieldByTagOrName(root, remainder);
  }

  public FieldData? FindFieldByTagOrName(string tagOrFieldName, string? remainder)
  {
    var fieldData = Fields.FirstOrDefault(f => f.Tag == tagOrFieldName); // First try the tag
    fieldData ??= Fields.FirstOrDefault(f => f.Name == tagOrFieldName);  // if not found try the (neutral) name
    // if still not found try the names in the different languages
    fieldData ??= Fields.FirstOrDefault(f => f.Names.FirstOrDefault(n => n.Text == tagOrFieldName) != null);

    if (fieldData != null && remainder != null && remainder.Length > 0)
    {
      if (!fieldData.IsLinked || fieldData.LinkedDatabase == null)
      {
        return null;
      }
      return fieldData.LinkedDatabase.FindFieldByTagOrName(remainder) == null ? null : fieldData;
    }

    if (fieldData == null)
    {
      if (tagOrFieldName == "fulltext")
      {
        if (FullText)
        {
          fieldData = fullTextFieldData;
        }
        else
        {
          throw new FullTextNotEnabledException(Name);
        }
      }
      if (FieldData.IsIdentifier(tagOrFieldName))
      {
        fieldData = new FieldData
        {
          Tag = "%0",
          Type = FieldTypeEnum.Integer,
          Length = 11,
          Name = "priref",
          IsMergedField = false,
          IsLinkIdField = false,
          Database = this
        };
      }
    }

    return fieldData;
  }

  /// <summary>
  /// Find the field info for a tag or field name, the field MUST exist otherwise an exception is thrown
  /// </summary>
  /// <param name="tagOrName"></param>
  /// <returns>The metadata for the field</returns>
  /// <exception cref="FieldNotFoundException"></exception>
  /// <exception cref="NullReferenceException"></exception>
  public FieldData GetFieldByTagOrName(string tagOrName)
  {
    var fieldData = FindFieldByTagOrName(tagOrName) ??
      throw new FieldNotFoundException(tagOrName, Name);

    if (fieldData.Tag == null)
    {
      throw new NullReferenceException(nameof(fieldData.Tag));
    }

    return fieldData;
  }

  public FieldData? FindFieldByMergeTag(string tag)
    => Fields
      .Where(field => field.IsLinked && field.MergeTags.Count > 0)
      .FirstOrDefault(fi => fi.MergeTags.FirstOrDefault(mi => mi.Destination == tag) != null);

  internal FieldData? FindFieldByWriteBackTag(string tag)
    => Fields
      .Where(field => field.IsLinked && field.WriteBackTags.Count > 0)
      .FirstOrDefault(fi => fi.WriteBackTags.FirstOrDefault(wb => wb.Source == tag) != null);

  internal FieldData? FindFieldByLinkIdTag(string tag)
  {
    if (string.IsNullOrWhiteSpace(tag))
    {
      throw new NullReferenceException(nameof(tag));
    }
    return Fields.FirstOrDefault(field => field.LinkIdTag == tag);
  }

  private readonly ConcurrentDictionary<string, List<FieldData>> groupCache = [];
  public List<FieldData>? FindGroup(string groupName)
  {
    if (!groupCache.TryGetValue(groupName, out var list))
    {
      list = [.. Fields.Where(field => field.Group == groupName)];
      if (list.Count > 0)
      {
        groupCache[groupName] = list;
      }
    }
    return list;
  }

  /// <summary>
  /// Returns a List of fields in the same group as the field parameter.
  /// if the field is not a member of a group then a list with the original field is returned.
  /// </summary>
  /// <param name="field">The field for which to return the group</param>
  /// <returns>A list of fields in the group</returns>
  public List<FieldData> FieldGroup(FieldData field)
  {
    var group = field.Group;
    return string.IsNullOrWhiteSpace(group) ? [field] : FindGroup(group)!;
  }

  [JsonIgnore]
  public static string Extension => ".inf";

  [JsonIgnore]
  public string FullTextTable => $"{Name}_fullText";

  [JsonIgnore]
  public bool FullText => IncludeInFullTextIndex.HasValue && IncludeInFullTextIndex.Value;

  /// <summary>
  /// Input fields
  /// </summary>
  [JsonIgnore]
  public EditFieldData InputGroup { get; private set; }

  /// <summary>
  /// Edit fields
  /// </summary>
  [JsonIgnore]
  public EditFieldData EditGroup { get; private set; }

  /// <summary>
  /// Edit history fields, these are new in model application 5.2
  /// </summary>
  [JsonIgnore]
  public EditFieldData EditHistoryGroup { get; private set; }

  [JsonIgnore]
  public LocationFieldData? LocationHistoryFields { get; set; }
  [JsonIgnore]
  public LocationFieldData? LocationFields { get; private set; }
  [JsonIgnore]
  public bool SupportsMove => LocationFields?.Id != null;
  [JsonIgnore]
  public bool SupportsMoveHistory => LocationHistoryFields?.Id != null;
  [JsonIgnore]
  public IEnumerable<FieldData> AutomaticNumberingFields { get; private set; }

  private readonly FieldData fullTextFieldData = new()
  {
    Name = "fulltext",
    IsMergedField = false,
    IsLinkIdField = false
  };

  public InternalLinkData? IsIndexedLink(string tag)
  {
    InternalLinkData? internalLinkInfo = null;
    var fieldInfo = FindFieldByTagOrName(tag);
    if (fieldInfo != null)
    {
      if (fieldInfo.IsLinkRef)
      {
        fieldInfo = FindFieldByLinkIdTag(tag);
      }

      if (fieldInfo != null)
      {
        internalLinkInfo = InternalLinks.FirstOrDefault(
            i => i.IndexedLink &&
            i.RelationType == RelationTypeEnum.Hierarchical &&
           (i.BroaderTermTag == fieldInfo.Tag || i.NarrowerTermTag == fieldInfo.Tag));
      }
    }
    return internalLinkInfo;
  }
}

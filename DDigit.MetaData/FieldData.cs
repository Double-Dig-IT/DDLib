namespace DDigit.MetaData;

public class FieldData : FieldDData
{
  public FieldData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace, DatabaseData? database = null) :
    base(objectType, stream, encoding, fileName, Properties, trace)
  {
    // The field object is also used in tasks in applications
    // In this situation there is no database, so we cannot check if the field is a LinkRef
    // We need to check the database here to prevent crashes when reading an adlib.pbk object. 
    if (database != null)
    {
      this.database = database;
      if (IsLinkRef && Type != FieldTypeEnum.Integer)
      {
        Type = FieldTypeEnum.Integer; // fix this problem in the setup, do not throw an exception
        Console.WriteLine($"Warning: Link reference fields must be of type integer, '{database.Name}', '{Name} ({Tag})'");
        //throw new DDException($"Link reference fields must be of type integer, '{database!.Name}', '{Tag}'");
      }
    }
  }

  internal FieldData()
  {

  }

  private DatabaseData? database;
  [JsonIgnore]
  public DatabaseData Database
  {
    get
    {
      if (database == null)
      {
        throw new NullReferenceException(nameof(database));
      }
      return database;
    }
    internal set => database = value;
  }

  /// <summary>
  /// The group to which this field belongs.
  /// </summary>
  public string Group
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The path of the linked database.
  /// </summary>
  public string LinkedDatabasePath
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Use strict validation for links, (the default is true).
  /// </summary>
  public bool StrictValidation
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The link reference tag.
  /// </summary>
  public string LinkIndexTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The link reference tag.
  /// </summary>
  public string LinkIdTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The reverse link reference tag.
  /// </summary>
  public string LinkReverseTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Is forcing allowed, (the default is true).
  /// </summary>
  public bool ForcingAllowed
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The preferred tag.
  /// </summary>
  public string PreferredTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The equivalent term tag.
  /// </summary>
  public string EquivalentTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The narrower term tag.
  /// </summary>
  public string NarrowerTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The narrower tag.
  /// </summary>
  public string? BroaderTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The semantic factor tag.
  /// </summary>
  public string? SemanticFactorTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// TDo we merge in multiple occurrences?
  /// </summary>
  public bool MultiOccurrenceLink
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The link screen.
  /// </summary>
  public string LinkScreen
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The zoom screen.
  /// </summary>
  public string ZoomScreen
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The edit screen.
  /// </summary>
  public string EditScreen
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The search screen.
  /// </summary>
  public string SearchScreen
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The filter Adapl.
  /// </summary>
  public string FilterAdapl
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The command Adapl.
  /// </summary>
  public string CommandAdapl
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The field length.
  /// </summary>
  public short Length
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The field type.
  /// </summary>
  public FieldTypeEnum Type
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The field link type.
  /// </summary>
  public LinkTypeEnum LinkType
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The tag which contains the dataset to force new records in.
  /// </summary>
  public string ForceInDatasetTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Is this an enumerative field?
  /// </summary>
  public bool Enumeration
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The link domain for this link.
  /// </summary>
  public string LinkDomain
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Z39.50 use attribute (only useful in Z39.50 servers).
  /// </summary>
  internal short Z3950UseAttribute
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Z39.50 GRS (General Record Structure 1) path (only useful in Z39.50 servers).
  /// </summary>
  internal string Z3950Grs1TagPath
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Z39.50 Marc (Machine readable catalog) tag (only useful in Z39.50 servers).
  /// </summary>
  internal string MarcTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Z39.50 SGML (Standard Generalized Markup Language) tag (only useful in Z39.50 servers).
  /// </summary>
  internal string SGMLTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Z39.50 tag set (only useful in Z39.50 servers).
  /// </summary>
  internal short Z3950TagSet
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The tag that stores the link domain in case of dynamic domains.
  /// </summary>
  public string LinkDomainTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// How defaults are assigned.
  /// </summary>
  public DefaultTypeEnum DefaultType
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The tag that stores the enumeration values in case of dynamic enumerations.
  /// </summary>
  public string EnumerationSourceTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The prefix string for autonumbering fields.
  /// </summary>
  public string AutoNumberPrefix
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The starting value for autonumbering fields.
  /// </summary>
  public int AutoNumberStartValue
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The increment value for autonumbering fields.
  /// </summary>
  public int AutoNumberIncrement
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The 16 bits starting value for autonumbering fields (obsolete).
  /// </summary>
  internal short AutoNumber16StartValue
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The 16 bits increment value for autonumbering fields (obsolete)
  /// </summary>
  internal short AutoNumber16Increment
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The suffix string for autonumbering fields.
  /// </summary>
  public string AutoNumberSuffix
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The format string to apply on autonumbering fields.
  /// </summary>
  public string AutoNumberFormatString
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// When are automatic numbers assigned?
  /// </summary>
  public AutoNumberAssignmentEnum AutoNumberAssignment
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Who assigns the numbers?
  /// </summary>
  public AutoNumberAssignmentSourceEnum AutoNumberAssignmentSource
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Is this field exchangeable.
  /// </summary>
  public ExchangeableEnum IsExchangeable
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The sort order of occurrences of this field.
  /// </summary>
  public SortOrderEnum SortOrder
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The sort order of enumeration values of this field.
  /// </summary>
  public EnumerationSortOrderEnum EnumerationSortOrder
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The format string of this field.
  /// </summary>
  public string FormatString
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The storage type of this field.
  /// </summary>
  public StorageTypeEnum StorageType
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Is this field multilingual?
  /// </summary>
  public bool IsMultiLingual
  {
    get; set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Do not use this field in lists.
  /// </summary>
  public bool DoNotShowInLists
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Presentation format.. how does this work?
  /// </summary>
  public int PresentationFormat
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Retrieval path.
  /// </summary>
  public string RetrievalPath
  {
    get;
    set;
  } = string.Empty;

  /// <summary>
  /// Thumbnail retrieval path.
  /// </summary>
  public string ThumbnailRetrievalPath
  {
    get;
    set;
  } = string.Empty;

  /// <summary>
  /// Range start tag
  /// </summary>
  public string RangeStartTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Range start tag
  /// </summary>
  public string RangeEndTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Exclude this field from the full text index.
  /// </summary>
  public bool ExcludeFromFullTextIndex
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Context tag.
  /// </summary>
  public string ContextTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Can we use a link screen here?
  /// </summary>
  public bool DoNotUseLinkScreen
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// In point tag.
  /// </summary>
  public string InPointTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Out point tag.
  /// </summary>
  public string OutPointTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Media storage type.
  /// </summary>
  public MediaStorageTypeEnum MediaStorageType
  {
    get;
    set;
  }

  /// <summary>
  /// Media storage type.
  /// </summary>
  public MediaRetrievalTypeEnum MediaRetrievalType
  {
    get;
    set;
  }

  /// <summary>
  /// Detail screen for this field.
  /// </summary>
  public string DetailScreen
  {
    get;
    set;
  } = string.Empty;

  /// <summary>
  /// The tag for related fields.
  /// </summary>
  public string RelatedTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Some feature for Calm
  /// </summary>
  internal short CALMExclusiveEnumeration
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Is this field inheritable?
  /// </summary>
  public bool IsInheritable
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Relation format string (used in Axiell Collections to format the relations view).
  /// </summary>
  public string RelationFormatString
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Developer notes for the field
  /// </summary>
  public string Notes
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Merge grouped Metadata?
  /// </summary>
  public bool MergeGroupedMetaData
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Pseudonym for tag
  /// </summary>
  public string PseudonymForTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Pseudonym tag
  /// </summary>
  public string PseudonymTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Format string for summaries
  /// </summary>
  public string SummaryFormatString
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// You can only write once to this field, no updates possible
  /// </summary>
  public bool WriteOnce
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The path of the metadata database.
  /// </summary>
  public string MetadataDatabasePath
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The metadata database reference tag.
  /// </summary>
  public string MetadataReferenceTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The tag of the field that contains the URI for linked data.
  /// </summary>
  public string LinkedURITag
  {
    get;
    set;
  } = string.Empty;

  /// <summary>
  /// The Linked Open Data type, Internal = data is in our system, External means that it needs to be resolved through an URI.
  /// </summary>
  public LODTypeEnum LODType
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// t
  /// Disable download for this field (LOD related?)
  /// </summary>
  public bool DisableDownload
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// The number of decimal places in floating point numbers
  /// </summary>
  public int NumberOfDecimalPlaces
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Apply zero padding in floating point numbers
  /// </summary>
  public bool ZeroPadding
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Should this link (LOD related) be indexed?
  /// </summary>
  public bool IsIndexedLink
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Sort field for the indexed link (LOD related).
  /// </summary>
  public string IndexedLinkSortField
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// How should indexed links (LOD related) be sorted.
  /// </summary>
  public SortSequenceEnum IndexedLinkSortOrder
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// How should indexed links (LOD related) be formatted.
  /// </summary>
  public string IndexedLinkFormatString
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Download path
  /// </summary>
  public string DownloadPath
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The tag of the field that contains the original file name
  /// </summary>
  public string OriginalFileNameTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The tag of the field that contains the media type
  /// </summary>
  public string MediaTypeTag
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// The type of enumeration for this field.
  /// </summary>
  public EnumerationTypeEnum EnumerationType
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  }

  /// <summary>
  /// Huh?
  /// </summary>
  public string DisableDownloadCondition
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// Huh 2?
  /// </summary>
  public string DefaultLinkFilter
  {
    get;
    set => (field, ElementCount) = (value, MaxElementCount(PropertyName(value)));
  } = string.Empty;

  /// <summary>
  /// A list with defaults
  /// </summary>
  public List<LanguageTextData> Defaults
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list with method texts
  /// </summary>
  public List<LanguageTextData> MethodTexts
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list labels
  /// </summary>
  public List<LanguageTextData> LabelTexts
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of relation texts
  /// </summary>
  public List<LanguageTextData> RelationTexts
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of reverse relation texts
  /// </summary>
  public List<LanguageTextData> ReverseRelationTexts
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of record type roles
  /// </summary>
  public List<AccessRightsData> RecordTypeRoles
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// The access rights for this field.
  /// </summary>
  public List<AccessRightsData> AccessRights
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of record type roles
  /// </summary>
  public EnumerationValueList EnumerationValues
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of merged in tags.
  /// </summary>
  public List<MergeTagData> MergeTags
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of write back tags.
  /// </summary>
  public List<MergeTagData> WriteBackTags
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of merge lists
  /// </summary>
  public List<MergeTagData> MergeListTags
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of merge tags for metadata fields
  /// </summary>
  public List<MergeTagData> MetadataMergeTags
  {
    get;
    internal set;
  } = [];

  /// <summary>
  /// A list of language tags (obsolete since we have multi-lingual fields)
  /// </summary>
  internal List<LanguageTextData> LanguageTags
  {
    get;
    set;
  } = [];

  /// <summary>
  /// A list of external sources.
  /// </summary>
  public List<ExternalSourceData> ExternalSources
  {
    get; private set;
  } = [];

  public List<MetadataMappingData> MetadataMappings
  {
    get; private set;
  } = [];

  internal void Add(EnumerationValueData enumerationValueData) => EnumerationValues.Add(enumerationValueData);

  internal void Add(LanguageTextData languageTextData) => Names.Add(languageTextData);

  public string? TranslateEnum(string? value, string language)
  {
    var enumValue = EnumerationValues.FirstOrDefault(e => e.NeutralValue == value);
    if (enumValue == null)
    {
      return value;
    }

    if (string.IsNullOrWhiteSpace(language))
    {
      return enumValue.NeutralValue;
    }

    int languageNo = Languages.GetAdlibNo(language);

    if (languageNo > enumValue.Texts.Count - 1)
    {
      throw new LanguageIsNotSupportedException(language);
    }
    return enumValue.Texts[languageNo].Text;
  }

  /// <summary>
  /// Get the list of neutral enumeration keys for a search value in a specific language
  /// </summary>
  /// <param name="value">The language specific search value</param>
  /// <param name="language">The ISO code of the language to get the keys for</param>
  /// <returns>A list of neutral values</returns>
  public IEnumerable<string> EnumKeys(string? value, string? language)
  {
    IEnumerable<EnumerationValueData> selection;
    if (!string.IsNullOrEmpty(language))
    {
      int languageNo = Languages.GetAdlibNo(language);
      selection = EnumerationValues.GetValues(value, languageNo);
    }
    else
    {
      selection = EnumerationValues.Where(v => v.NeutralValue != null &&
                                (value == null || v.NeutralValue.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)));
    }
    return selection.Select(v => v.NeutralValue!);
  }

  /// <summary>
  /// Get the language specific value for a language neutral enumeration key
  /// </summary>
  /// <param name="value">The language neutral value</param>
  /// <param name="language">the ISO code for the language to retrieve</param>
  /// <returns>The language specific representation for the neutral value or the neutral value if none was was found</returns>
  public string? GetLanguageEnumValue(string value, string language)
  {
    var enumValue = GetEnumerationValues(value);
    return enumValue != null ? language == "" ? enumValue.NeutralValue : enumValue.Texts[Languages.GetAdlibNo(language)].Text : value;
  }

  public EnumerationValueData GetEnumerationValues(string value, string language = "")
  {
    EnumerationValueData? result = null;
    if (language == "")
    {
      result = EnumerationValues.FirstOrDefault(v
        => string.Compare(v.NeutralValue, value, StringComparison.CurrentCultureIgnoreCase) == 0);
    }
    else
    {
      int languageIndex = Languages.GetAdlibNo(language);

      result = EnumerationValues.FirstOrDefault(v
        => string.Compare(v.Texts[languageIndex].Text, value, StringComparison.CurrentCultureIgnoreCase) == 0);
    }
    return result ?? throw new EnumValueException(Database.Name!, Name!, Tag!, language, value);
  }

  public string GetDomainTag()
  {
    var linkIndex = PreferredIndex ?? throw new NullReferenceException(nameof(PreferredIndex));
    return linkIndex.DomainTag ?? throw new NullReferenceException(nameof(linkIndex.DomainTag));
  }

  /// <summary>
  /// Get the sql table name for the preferred index of this field.
  /// </summary>
  /// <returns>table name</returns>
  /// <exception cref="NullReferenceException"></exception>
  public string GetIndexTableName()
    => PreferredIndex?.TableName ??
       throw new NullReferenceException(nameof(PreferredIndex)) ??
       throw new NullReferenceException(nameof(PreferredIndex.TableName));

  private short MaxElementCount(string propertyName)
     => Math.Max(Properties.GetElementCount(propertyName), ElementCount);

  public FieldData? LinkSourceField(FieldData linkedField)
  {
    var mergePair = MergeTags.FirstOrDefault(pair => pair.Destination == linkedField.Tag);
    return mergePair != null ? LinkedDatabase?.FindFieldByTagOrName(mergePair.Source!) : null;
  }

  public FieldData GetPartsField()
  {
    var relation = Database.InternalLinks.Where(il => il.RelationType == RelationTypeEnum.Hierarchical);
    if (!relation.Any())
    {
      throw new DDException($"Field {this} not found in internal links");
    }
    var partsTag = relation.ElementAt(0).NarrowerTermLinkIdTag ?? throw new DDException($"NarrowerTermLinkIdTag not found in internal links");
    return Database.FindFieldByTagOrName(partsTag) ?? throw new DDException($"Field {partsTag} not found in internal links");
  }

  public FieldData GetPartsOfField()
  {
    var relation = Database.InternalLinks.Where(il => il.RelationType == RelationTypeEnum.Hierarchical);
    if (!relation.Any())
    {
      throw new DDException($"Field {this} not found in internal links");
    }
    var partsOfTag = relation.ElementAt(0).BroaderTermLinkIdTag ?? throw new DDException($"BroaderTermLinkIdTag not found in internal links");
    return Database.FindFieldByTagOrName(partsOfTag) ?? throw new DDException($"Field {partsOfTag} not found in internal links");
  }

  public static (string root, string? remainder) GetRoot(string fieldNameOrTag)
  {
    var index = fieldNameOrTag.IndexOf(IndirectionOperator);
    return index < 0 ? (fieldNameOrTag, null) : (fieldNameOrTag[..index].Trim(), fieldNameOrTag[(index + 2)..].Trim());
  }

  public const string IndirectionOperator = "->";

  public const string Identifier = "identifier";

  public static bool IsIdentifier(string fieldNameOrTag) => (fieldNameOrTag == Identifier || fieldNameOrTag == "id" || 
                                                             fieldNameOrTag == "priref" || fieldNameOrTag == "%0");

  internal static PropertyList Properties =
    [
      new PropertyMap(0, DataTypesEnum.Int16, "ElementCount"),
      new PropertyMap(1, DataTypesEnum.String, "LinkedDatabasePath"),
      new PropertyMap(2, DataTypesEnum.String, "LinkIndexTag"),
      new PropertyMap(3, DataTypesEnum.String, "Tag"),
      new PropertyMap(5, DataTypesEnum.BoolI, "StrictValidation"),
      new PropertyMap(7, DataTypesEnum.String, "LinkIdTag"),
      new PropertyMap(9, DataTypesEnum.BoolI, "ForcingAllowed"),
      new PropertyMap(10, DataTypesEnum.String, "PreferredTag"),
      new PropertyMap(11, DataTypesEnum.String, "EquivalentTag"),
      new PropertyMap(12, DataTypesEnum.String, "NarrowerTag"),
      new PropertyMap(13, DataTypesEnum.Bool, "MultiOccurrenceLink"),
      new PropertyMap(14, DataTypesEnum.String, "LinkScreen"),
      new PropertyMap(15, DataTypesEnum.String, "ZoomScreen"),
      new PropertyMap(16, DataTypesEnum.String, "EditScreen"),
      new PropertyMap(17, DataTypesEnum.String, "FilterAdapl"),
      new PropertyMap(18, DataTypesEnum.String, "CommandAdapl"),
      new PropertyMap(19, DataTypesEnum.String, "Name"),
      new PropertyMap(20, DataTypesEnum.Int16, "Length"),
      new PropertyMap(21, DataTypesEnum.Enum, "Type", typeof(FieldTypeEnum)),
      new PropertyMap(22, DataTypesEnum.Enum, "LinkType", typeof(LinkTypeEnum)),
      new PropertyMap(23, DataTypesEnum.String, "ForceInDatasetTag"),
      new PropertyMap(24, DataTypesEnum.String, "SearchScreen"),
      new PropertyMap(25, DataTypesEnum.String, "LinkReverseTag"),
      new PropertyMap(28, DataTypesEnum.String, "BroaderTag"),
      new PropertyMap(29, DataTypesEnum.Bool, "Enumeration"),
      new PropertyMap(31, DataTypesEnum.String, "LinkDomain"),
      new PropertyMap(32, DataTypesEnum.Int16, "Z3950UseAttribute"),
      new PropertyMap(33, DataTypesEnum.String, "Z3950Grs1TagPath"),
      new PropertyMap(34, DataTypesEnum.String, "MarcTag"),
      new PropertyMap(35, DataTypesEnum.String, "SGMLTag"),
      new PropertyMap(36, DataTypesEnum.Int16, "Z3950TagSet"),
      new PropertyMap(37, DataTypesEnum.String, "LinkDomainTag"),
      new PropertyMap(38, DataTypesEnum.Enum, "DefaultType", typeof(DefaultTypeEnum)),
      new PropertyMap(41, DataTypesEnum.Enum, "EnumerationType", typeof(EnumerationTypeEnum)),
      new PropertyMap(42, DataTypesEnum.String, "EnumerationSourceTag"),
      new PropertyMap(46, DataTypesEnum.String, "Group"),
      new PropertyMap(50, DataTypesEnum.String, "SemanticFactorTag"),
      new PropertyMap(53, DataTypesEnum.String, "AutoNumberPrefix"),
      new PropertyMap(54, DataTypesEnum.Int16, "AutoNumber16StartValue"),
      new PropertyMap(55, DataTypesEnum.Int16, "AutoNumber16Increment"),
      new PropertyMap(56, DataTypesEnum.String, "AutoNumberSuffix"),
      new PropertyMap(57, DataTypesEnum.String, "AutoNumberFormatString"),
      new PropertyMap(58, DataTypesEnum.Enum, "AutoNumberAssignment", typeof(AutoNumberAssignmentEnum)),
      new PropertyMap(59, DataTypesEnum.Enum, "AutoNumberAssignmentSource", typeof(AutoNumberAssignmentSourceEnum)),
      new PropertyMap(60, DataTypesEnum.BoolI, "IsRepeated"),
      new PropertyMap(61, DataTypesEnum.Enum, "IsExchangeable", typeof(ExchangeableEnum)),
      new PropertyMap(62, DataTypesEnum.Enum, "SortOrder", typeof(SortOrderEnum)),
      new PropertyMap(63, DataTypesEnum.Enum, "EnumerationSortOrder", typeof(EnumerationSortOrderEnum)),
      new PropertyMap(64, DataTypesEnum.Int32, "AutoNumberStartValue"),
      new PropertyMap(65, DataTypesEnum.Int32, "AutoNumberIncrement"),
      new PropertyMap(67, DataTypesEnum.String, "FormatString"),
      new PropertyMap(68, DataTypesEnum.Enum, "StorageType", typeof(StorageTypeEnum)),
      new PropertyMap(69, DataTypesEnum.Bool, "IsMultiLingual"),
      new PropertyMap(70, DataTypesEnum.Bool, "DoNotShowInLists"),
      new PropertyMap(71, DataTypesEnum.Int32, "PresentationFormat"),
      new PropertyMap(72, DataTypesEnum.String, "RetrievalPath"),
      new PropertyMap(73, DataTypesEnum.String, "ThumbnailRetrievalPath"),
      new PropertyMap(74, DataTypesEnum.String, "RangeStartTag"),
      new PropertyMap(75, DataTypesEnum.String, "RangeEndTag"),
      new PropertyMap(76, DataTypesEnum.Bool, "ExcludeFromFullTextIndex"),
      new PropertyMap(78, DataTypesEnum.String, "ContextTag"),
      new PropertyMap(79, DataTypesEnum.Bool, "DoNotUseLinkScreen"),
      new PropertyMap(80, DataTypesEnum.String, "InPointTag"),
      new PropertyMap(81, DataTypesEnum.String, "OutPointTag"),
      new PropertyMap(82, DataTypesEnum.Enum, "MediaStorageType", typeof(MediaStorageTypeEnum)),
      new PropertyMap(83, DataTypesEnum.Enum, "MediaRetrievalType", typeof(MediaRetrievalTypeEnum)),
      new PropertyMap(84, DataTypesEnum.String, "DetailScreen"),
      new PropertyMap(85, DataTypesEnum.String, "RelatedTag"),
      new PropertyMap(86, DataTypesEnum.Bool, "IsInheritable"),
      new PropertyMap(89, DataTypesEnum.Int16, "CALMExclusiveEnumeration"),
      new PropertyMap(90, DataTypesEnum.Bool, "MergeGroupedMetaData"),
      new PropertyMap(91, DataTypesEnum.String, "RelationFormatString"),
      new PropertyMap(94, DataTypesEnum.String, "Notes"),
      new PropertyMap(95, DataTypesEnum.String, "PseudonymForTag"),
      new PropertyMap(96, DataTypesEnum.String, "PseudonymTag"),
      new PropertyMap(97, DataTypesEnum.String, "SummaryFormatString"),
      new PropertyMap(98, DataTypesEnum.Bool, "WriteOnce"),
      new PropertyMap(99, DataTypesEnum.String, "MetadataDatabasePath"),
      new PropertyMap(100, DataTypesEnum.String, "MetadataReferenceTag"),
      new PropertyMap(102, DataTypesEnum.String, "LinkedURITag"),
      new PropertyMap(103, DataTypesEnum.Enum, "LODType", typeof(LODTypeEnum)),
      new PropertyMap(104, DataTypesEnum.Bool, "DisableDownload"),
      new PropertyMap(105, DataTypesEnum.Int32, "NumberOfDecimalPlaces"),
      new PropertyMap(106, DataTypesEnum.Bool, "ZeroPadding"),
      new PropertyMap(107, DataTypesEnum.Bool, "IsIndexedLink"),
      new PropertyMap(108, DataTypesEnum.String, "IndexedLinkSortField"),
      new PropertyMap(109, DataTypesEnum.Enum, "IndexedLinkSortOrder", typeof(SortSequenceEnum)),
      new PropertyMap(110, DataTypesEnum.String, "IndexedLinkFormatString"),
      new PropertyMap(111, DataTypesEnum.String, "DownloadPath"),
      new PropertyMap(112, DataTypesEnum.String, "OriginalFileNameTag"),
      new PropertyMap(113, DataTypesEnum.String, "MediaTypeTag"),
      new PropertyMap(114, DataTypesEnum.String, "DisableDownloadCondition"),
      new PropertyMap(115, DataTypesEnum.String, "DefaultLinkFilter")
    ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
      (MergeTagData.Properties, MergeTags),
      (MergeTagData.Properties, WriteBackTags),
      (LanguageTextData.Properties, Names),
      (AccessRightsData.Properties, AccessRights),
      (EnumerationValueData.Properties, EnumerationValues),
      (LanguageTextData.Properties, Defaults),
      (MergeTagData.Properties, MergeListTags),
      (LanguageTextData.Properties, MethodTexts),
      (LanguageTextData.Properties, LabelTexts),
      (LanguageTextData.Properties, LanguageTags),
      (ExternalSourceData.Properties, ExternalSources),
      (AccessRightsData.Properties, RecordTypeRoles),
      (LanguageTextData.Properties, RelationTexts),
      (LanguageTextData.Properties, ReverseRelationTexts),
      (MergeTagData.Properties, MetadataMergeTags),
      (MetadataMappingData.Properties, MetadataMappings),
  ];

  public bool IsLinked => !string.IsNullOrWhiteSpace(LinkIndexTag) && !string.IsNullOrWhiteSpace(LinkedDatabasePath);

  private bool? isLinkIdField;
  [JsonIgnore]
  public bool IsLinkIdField
  {
    get
    {
      if (!isLinkIdField.HasValue)
      {
        linkedField = Database?.FindFieldByLinkIdTag(Tag!);
        isLinkIdField = linkedField != null;
      }
      return isLinkIdField.Value;
    }
    set
    {
      isLinkIdField = value;
    }
  }

  [JsonIgnore]
  private FieldData? linkedField;
  public FieldData? LinkedField
  {
    get
    {
      if (!isLinkIdField.HasValue)
      {
        linkedField = Database?.FindFieldByLinkIdTag(Tag!);
        isLinkIdField = linkedField != null;
      }
      return linkedField;
    }
  }

  private bool? isMergedField;
  [JsonIgnore]
  public bool IsMergedField
  {
    get
    {
      if (!isMergedField.HasValue)
      {
        isMergedField = Database?.FindFieldByMergeTag(Tag!) != null;
      }
      return isMergedField.Value;
    }
    set
    {
      isMergedField = value;
    }
  }

  private bool? isWriteBackField;
  [JsonIgnore]
  public bool IsWriteBackField
  {
    get
    {
      if (!isWriteBackField.HasValue)
      {
        isWriteBackField = Database?.FindFieldByWriteBackTag(Tag!) != null;
      }
      return isWriteBackField.Value;
    }
    set
    {
      isWriteBackField = value;
    }
  }

  /// <summary>
  /// If the field is a context field then figure out where is it a context field from.
  /// </summary>
  [JsonIgnore]
  public FieldData? ParentField { get; private set; }

  private bool? isContextField;

  /// <summary>
  /// True if this is a context field.
  /// </summary>
  [JsonIgnore]
  public bool IsContextField
  {
    get
    {
      if (!isContextField.HasValue)
      {
        ParentField = Database.Fields.FirstOrDefault(f => f.ContextTag == Tag);
        isContextField = ParentField != null;
      }
      return isContextField.Value;
    }
  }

  [JsonIgnore]
  public bool IsReadOnly => IsContextField;

  [JsonIgnore]
  public bool IsAutoNumberField => AutoNumberAssignment != AutoNumberAssignmentEnum.Undefined &&
                                   AutoNumberAssignment != AutoNumberAssignmentEnum.Never &&
                                   (AutoNumberIncrement > 0 || AutoNumber16Increment > 0);

  private DatabaseData? linkedDatabase = null;
  [JsonIgnore]
  public DatabaseData? LinkedDatabase
  {
    get
    {
      if (IsLinked && LinkedDatabasePath != null && linkedDatabase == null)
      {
        if (LinkedDatabasePath == "=")
        {
          linkedDatabase = Database;
        }
        else
        {
          if (LinkedDatabasePhysicalPath == null)
          {
            throw new NullReferenceException(LinkedDatabasePhysicalPath);
          }
          linkedDatabase = MetaDataCache.ReadDatabase(LinkedDatabasePhysicalPath, false);
        }
      }
      return linkedDatabase;
    }
  }

  private FieldData? linkedFieldData = null;
  [JsonIgnore]
  public FieldData? LinkedFieldData
  {
    get
    {
      if (linkedFieldData == null && LinkIndexTag != null && LinkedDatabase != null)
      {
        linkedFieldData = LinkedDatabase.FindFieldByTagOrName(LinkIndexTag);
      }
      // this could be a merged field, find the linked field.
      linkedFieldData ??= Database?.FindFieldByMergeTag(Tag!);
      return linkedFieldData;
    }
  }

  private List<IndexData>? indexList = null;
  [JsonIgnore]
  public List<IndexData>? IndexList
  {
    get
    {
      if (indexList == null && Database != null)
      {
        indexList = [];
        var tag = IsLinked ? LinkIdTag : Tag;
        indexList.AddRange(Database.Indexes.Where(index => index.IndexTags.Contains(tag!)));
      }
      return indexList;
    }
  }

  IndexData? preferredIndex;
  /// <summary>
  /// Returns the (first) preferred  index for this field
  /// </summary>
  [JsonIgnore]
  public IndexData? PreferredIndex
  {
    get
    {
      if (preferredIndex == null && IndexList != null)
      {
        foreach (var indexData in IndexList)
        {
          var indexTag = indexData.Tag;
          if (indexTag == Tag || (!string.IsNullOrEmpty(LinkIdTag) && LinkIdTag == indexTag))
          {
            preferredIndex = indexData;
            break;
          }
        }
      }
      return preferredIndex;
    }
  }

  private string? linkedDataset = null;
  [JsonIgnore]
  public string? LinkedDataset
  {
    get
    {
      if (!string.IsNullOrWhiteSpace(LinkedDatabasePath) && linkedDataset == null)
      {
        var pos = LinkedDatabasePath.IndexOf('>');
        if (pos > 0)
        {
          linkedDataset = LinkedDatabasePath[(pos + 1)..];
        }
      }
      return linkedDataset;
    }
  }

  private string? linkedDatabasePhysicalPath;
  [JsonIgnore]
  public string? LinkedDatabasePhysicalPath
  {
    get
    {
      if (linkedDatabasePhysicalPath == null)
      {
        if (LinkedDatabasePath != null)
        {
          var pos = LinkedDatabasePath.IndexOf('>');
          var path = (pos > 0 ? LinkedDatabasePath[..pos] : LinkedDatabasePath).Replace('+', Path.DirectorySeparatorChar);
          if (!path.EndsWith(".inf", StringComparison.OrdinalIgnoreCase))
          {
            path += ".inf";
          }
          linkedDatabasePhysicalPath = new FileInfo(Path.Combine(Path.GetDirectoryName(Database!.PhysicalPath)!, path)).FullName;
        }
      }
      return linkedDatabasePhysicalPath;
    }
  }

  private string? dataTag;
  [JsonIgnore]
  public string? DataTag
  {
    get
    {
      if (dataTag == null)
      {
        dataTag = Tag!;
        if (IsLinked)
        {
          if (!string.IsNullOrWhiteSpace(LinkIdTag))
          {
            dataTag = LinkIdTag;
          }
        }
        else if (IsMergedField)
        {
          var fieldData = LinkedFieldData;
          if (fieldData == null)
          {
            throw new NullReferenceException(nameof(fieldData));
          }
          dataTag = fieldData.LinkIdTag;

          if (dataTag == null)
          {
            throw new NullReferenceException(nameof(fieldData.LinkIdTag));
          }
        }
      }
      return dataTag;
    }
  }

  private FieldData? useFieldData;


  [JsonIgnore]
  public FieldData? UseFieldData
  {
    get
    {
      if (useFieldData == null)
      {
        if (!string.IsNullOrWhiteSpace(PreferredTag))
        {
          if (LinkedDatabase != null)
          {
            var linkedFieldData = LinkedDatabase.FindFieldByTagOrName(PreferredTag);
            if (linkedFieldData != null)
            {
              useFieldData = linkedFieldData;
            }
          }
        }
      }
      return useFieldData;
    }
  }


  /// <summary>
  /// Returns the physical tag that is associated with a field 
  /// (either the linkIdTag, linkIdTag of a linked field associated with a merged in field,
  ///  or the real Tag), added support for context fields on 22/03/2025 BDD
  /// </summary>
  [JsonIgnore]
  public string? PhysicalTag
  {
    get
    {
      if (IsLinked)
      {
        return LinkIdTag;
      }
      if (IsMergedField && LinkedFieldData != null && LinkedFieldData.LinkIdTag != null)
      {
        return LinkedFieldData.LinkIdTag;
      }
      if (IsContextField && ParentField != null)
      {
        return ParentField.LinkIdTag;
      }
      return Tag;
    }
  }


  private bool? isLinkRef;
  [JsonIgnore]
  public bool IsLinkRef
  {
    get
    {
      isLinkRef ??= Database != null && !string.IsNullOrWhiteSpace(Tag) && Database.FindFieldByLinkIdTag(Tag) != null;
      return isLinkRef.Value;
    }
  }

  public OccurrenceDataTypeEnum OccurrenceDataType
  {
    get
    {
      if (Enumeration)
      {
        return OccurrenceDataTypeEnum.Enumeration;
      }
      if (IsLinkIdField)
      {
        return OccurrenceDataTypeEnum.LinkRef;
      }
      if (IsMultiLingual)
      {
        return OccurrenceDataTypeEnum.Multilingual;
      }
      return OccurrenceDataTypeEnum.Standard;
    }
  }
}
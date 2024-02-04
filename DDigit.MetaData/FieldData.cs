namespace DDigit.MetaData;

public class FieldData : FieldDData
{
  public FieldData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace, DatabaseData? database = null) : 
    base (objectType, stream, encoding, fileName, Properties, trace)
  {
    Database = database;
  }

  internal FieldData()
  {

  }

  [JsonIgnore]
  public DatabaseData? Database { get; internal set; }

  /// <summary>
  /// The group to which this field belongs.
  /// </summary>
  public string? Group
  {
    get; private set;
  }

  /// <summary>
  /// The path of the linked database.
  /// </summary>
  public string? LinkedDatabasePath
  {
    get; private set;
  }

  /// <summary>
  /// Use strict validation for links, (the default is true).
  /// </summary>
  public bool? StrictValidation
  {
    get; private set;
  }

  /// <summary>
  /// The link reference tag.
  /// </summary>
  public string? LinkIndexTag
  {
    get; private set;
  }

  /// <summary>
  /// The link reference tag.
  /// </summary>
  public string? LinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The reverse link reference tag.
  /// </summary>
  public string? LinkReverseTag
  {
    get; private set;
  }

  /// <summary>
  /// Is forcing allowed, (the default is true).
  /// </summary>
  public bool? ForcingAllowed
  {
    get; private set;
  }

  /// <summary>
  /// The preferred tag.
  /// </summary>
  public string? PreferredTag
  {
    get; private set;
  }

  /// <summary>
  /// The equivalent term tag.
  /// </summary>
  public string? EquivalentTag
  {
    get; private set;
  }

  /// <summary>
  /// The narrower term tag.
  /// </summary>
  public string? NarrowerTag
  {
    get; private set;
  }

  /// <summary>
  /// The narrower tag.
  /// </summary>
  public string? BroaderTag
  {
    get; private set;
  }

  /// <summary>
  /// The semantic factor tag.
  /// </summary>
  public string? SemanticFactorTag
  {
    get; private set;
  }

  /// <summary>
  /// TDo we merge in multiple occurrences?
  /// </summary>
  public bool MultiOccurrenceLink
  {
    get; private set;
  }

  /// <summary>
  /// The link screen.
  /// </summary>
  public string? LinkScreen
  {
    get; private set;
  }

  /// <summary>
  /// The zoom screen.
  /// </summary>
  public string? ZoomScreen
  {
    get; private set;
  }

  /// <summary>
  /// The edit screen.
  /// </summary>
  public string? EditScreen
  {
    get; private set;
  }

  /// <summary>
  /// The search screen.
  /// </summary>
  public string? SearchScreen
  {
    get; private set;
  }

  /// <summary>
  /// The filter Adapl.
  /// </summary>
  public string? FilterAdapl
  {
    get; private set;
  }

  /// <summary>
  /// The command Adapl.
  /// </summary>
  public string? CommandAdapl
  {
    get; private set;
  }

  /// <summary>
  /// The field length.
  /// </summary>
  public short? Length
  {
    get; private set;
  }

  /// <summary>
  /// The field type.
  /// </summary>
  public FieldTypeEnum Type
  {
    get; private set;
  }

  /// <summary>
  /// The field link type.
  /// </summary>
  public LinkTypeEnum LinkType
  {
    get; private set;
  }

  /// <summary>
  /// The tag which contains the dataset to force new records in.
  /// </summary>
  public string? ForceInDatasetTag
  {
    get; private set;
  }

  /// <summary>
  /// Is this an enumerative field?
  /// </summary>
  public bool? IsEnumeration
  {
    get; private set;
  }

  /// <summary>
  /// The link domain for this link.
  /// </summary>
  public string? LinkDomain
  {
    get; private set;
  }

  /// <summary>
  /// Z39.50 use attribute (only useful in Z39.50 servers).
  /// </summary>
  internal short? Z3950UseAttribute
  {
    get; private set;
  }

  /// <summary>
  /// Z39.50 GRS (General Record Structure 1) path (only useful in Z39.50 servers).
  /// </summary>
  internal string? Z3950Grs1TagPath
  {
    get; private set;
  }

  /// <summary>
  /// Z39.50 Marc (Machine readable catalog) tag (only useful in Z39.50 servers).
  /// </summary>
  internal string? MarcTag
  {
    get; private set;
  }

  /// <summary>
  /// Z39.50 SGML (Standard Generalized Markup Language) tag (only useful in Z39.50 servers).
  /// </summary>
  internal string? SGMLTag
  {
    get; private set;
  }

  /// <summary>
  /// Z39.50 tag set (only useful in Z39.50 servers).
  /// </summary>
  internal short? Z3950TagSet
  {
    get; private set;
  }

  /// <summary>
  /// The tag that stores the link domain in case of dynamic domains.
  /// </summary>
  public string? LinkDomainTag
  {
    get; private set;
  }

  /// <summary>
  /// How defaults are assigned.
  /// </summary>
  public DefaultTypeEnum DefaultType
  {
    get; private set;
  }

  /// <summary>
  /// The tag that stores the enumeration values in case of dynamic enumerations.
  /// </summary>
  public string? EnumerationSourceTag
  {
    get; private set;
  }

  /// <summary>
  /// The prefix string for autonumbering fields.
  /// </summary>
  public string? AutoNumberPrefix
  {
    get; private set;
  }

  /// <summary>
  /// The starting value for autonumbering fields.
  /// </summary>
  public int? AutoNumberStartValue
  {
    get; private set;
  }

  /// <summary>
  /// The increment value for autonumbering fields.
  /// </summary>
  public int? AutoNumberIncrement
  {
    get; private set;
  }

  /// <summary>
  /// The 16 bits starting value for autonumbering fields (obsolete).
  /// </summary>
  internal short? AutoNumber16StartValue
  {
    get; private set;
  }

  /// <summary>
  /// The 16 bits increment value for autonumbering fields (obsolete)
  /// </summary>
  internal short? AutoNumber16Increment
  {
    get; private set;
  }

  /// <summary>
  /// The suffix string for autonumbering fields.
  /// </summary>
  public string? AutoNumberSuffix
  {
    get; private set;
  }

  /// <summary>
  /// The format string to apply on autonumbering fields.
  /// </summary>
  public string? AutoNumberFormatString
  {
    get; private set;
  }

  /// <summary>
  /// When are automatic numbers assigned?
  /// </summary>
  public AutoNumberAssignmentEnum AutoNumberAssignment
  {
    get; private set;
  }

  /// <summary>
  /// Who assigns the numbers?
  /// </summary>
  public AutoNumberAssignmentSourceEnum AutoNumberAssignmentSource
  {
    get; private set;
  }

  /// <summary>
  /// Is this field exchangeable.
  /// </summary>
  public ExchangeableEnum IsExchangeable
  {
    get; private set;
  }

  /// <summary>
  /// The sort order of occurrences of this field.
  /// </summary>
  public SortOrderEnum SortOrder
  {
    get; private set;
  }

  /// <summary>
  /// The sort order of enumeration values of this field.
  /// </summary>
  public EnumerationSortOrderEnum EnumerationSortOrder
  {
    get; private set;
  }

  /// <summary>
  /// The format string of this field.
  /// </summary>
  public string? FormatString
  {
    get; private set;
  }

  /// <summary>
  /// The storage type of this field.
  /// </summary>
  public StorageTypeEnum StorageType
  {
    get; private set;
  }

  /// <summary>
  /// Is this field multilingual?
  /// </summary>
  public bool IsMultiLingual
  {
    get; private set;
  }

  /// <summary>
  /// Do not use this field in lists.
  /// </summary>
  public bool? DoNotShowInLists
  {
    get;
    private set;
  }

  /// <summary>
  /// Presentation format.. how does this work?
  /// </summary>
  public int? PresentationFormat
  {
    get;
    private set;
  }

  /// <summary>
  /// Retrieval path.
  /// </summary>
  public string? RetrievalPath
  {
    get;
    private set;
  }

  /// <summary>
  /// Thumbnail retrieval path.
  /// </summary>
  public string? ThumbnailRetrievalPath
  {
    get;
    private set;
  }

  /// <summary>
  /// Range start tag
  /// </summary>
  public string? RangeStartTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Range start tag
  /// </summary>
  public string? RangeEndTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Exclude this field from the full text index.
  /// </summary>
  public bool? ExcludeFromFullTextIndex
  {
    get;
    private set;
  }

  /// <summary>
  /// Context tag.
  /// </summary>
  public string? ContextTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Context tag.
  /// </summary>
  public bool? DoNotUseLinkScreen
  {
    get;
    private set;
  }

  /// <summary>
  /// In point tag.
  /// </summary>
  public string? InPointTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Out point tag.
  /// </summary>
  public string? OutPointTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Media storage type.
  /// </summary>
  public MediaStorageTypeEnum MediaStorageType
  {
    get;
    private set;
  }

  /// <summary>
  /// Media storage type.
  /// </summary>
  public MediaRetrievalTypeEnum MediaRetrievalType
  {
    get;
    private set;
  }

  /// <summary>
  /// Detail screen for this field.
  /// </summary>
  public string? DetailScreen
  {
    get;
    private set;
  }

  /// <summary>
  /// The tag for related fields.
  /// </summary>
  public string? RelatedTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Some feature for Calm
  /// </summary>
  internal short? CALMExclusiveEnumeration
  {
    get;
    private set;
  }

  /// <summary>
  /// Is this field inheritable?
  /// </summary>
  public bool? IsInheritable
  {
    get;
    private set;
  }

  /// <summary>
  /// Relation format string (used in Axiell Collections to format the relations view).
  /// </summary>
  public string? RelationFormatString
  {
    get;
    private set;
  }

  /// <summary>
  /// Developer notes for the field
  /// </summary>
  public string? Notes
  {
    get;
    private set;
  }

  /// <summary>
  /// Merge grouped Metadata?
  /// </summary>
  public bool? MergeGroupedMetaData
  {
    get;
    private set;
  }

  /// <summary>
  /// Pseudonym for tag
  /// </summary>
  public string? PseudonymForTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Pseudonym tag
  /// </summary>
  public string? PseudonymTag
  {
    get;
    private set;
  }

  /// <summary>
  /// Format string for summaries
  /// </summary>
  public string? SummaryFormatString
  {
    get;
    private set;
  }

  /// <summary>
  /// You can only write once to this field, no updates possible
  /// </summary>
  public bool WriteOnce
  {
    get;
    private set;
  }

  /// <summary>
  /// The path of the metadata database.
  /// </summary>
  public string? MetadataDatabasePath
  {
    get;
    private set;
  }

  /// <summary>
  /// The metadata database reference tag.
  /// </summary>
  public string? MetadataReferenceTag
  {
    get;
    private set;
  }

  /// <summary>
  /// The tag of the field that contains the URI for linked data.
  /// </summary>
  public string? LinkedURITag
  {
    get;
    private set;
  }

  /// <summary>
  /// The Linked Open Data type, Internal = data is in our system, External means that it needs to be resolved through an URI.
  /// </summary>
  public LODTypeEnum LODType
  {
    get;
    private set;
  }

  /// <summary>
  /// Disable download for this field (LOD related?)
  /// </summary>
  public bool DisableDownload
  {
    get;
    private set;
  }

  /// <summary>
  /// The number of decimal places in floating point numbers
  /// </summary>
  public int NumberOfDecimalPlaces
  {
    get;
    private set;
  }

  /// <summary>
  /// Apply zero padding in floating point numbers
  /// </summary>
  public bool ZeroPadding
  {
    get;
    private set;
  }

  /// <summary>
  /// Should this link (LOD related) be indexed?
  /// </summary>
  public bool IndexedLink
  {
    get;
    private set;
  }

  /// <summary>
  /// Sort field for the indexed link (LOD related).
  /// </summary>
  public string? IndexedLinkSortField
  {
    get;
    private set;
  }

  /// <summary>
  /// How should indexed links (LOD related) be sorted.
  /// </summary>
  public SortSequenceEnum IndexedLinkSortOrder
  {
    get;
    private set;
  }

  /// <summary>
  /// How should indexed links (LOD related) be formatted.
  /// </summary>
  public string? IndexedLinkFormatString
  {
    get;
    private set;
  }

  /// <summary>
  /// Download path
  /// </summary>
  public string? DownloadPath
  {
    get;
    private set;
  }

  /// <summary>
  /// The tag of the field that contains the original file name
  /// </summary>
  public string? OriginalFileNameTag
  {
    get;
    private set;
  }

  /// <summary>
  /// The tag of the field that contains the media type
  /// </summary>
  public string? MediaTypeTag
  {
    get;
    private set;
  }

  /// <summary>
  /// The type of enumeration for this field.
  /// </summary>
  public EnumerationTypeEnum EnumerationType
  {
    get;
    private set;
  }

  /// <summary>
  /// Huh?
  /// </summary>
  public string? DisableDownloadCondition
  {
    get;
    private set;
  }

  /// <summary>
  /// Huh 2?
  /// </summary>
  public string? DefaultLinkFilter
  {
    get;
    private set;
  }

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
  public List<EnumerationValueData> EnumerationValues
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
      selection = EnumerationValues.Where(v => v.NeutralValue != null &&
                                  (value == null || (v.Texts[languageNo].Text != null &&
                                  v.Texts[languageNo].Text!.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))));
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
    var enumValue = EnumerationValues.FirstOrDefault(v => string.Compare(v.NeutralValue, value, StringComparison.CurrentCultureIgnoreCase) == 0);
    return enumValue != null ? enumValue.Texts[Languages.GetAdlibNo(language)].Text : value;
  }

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
      new PropertyMap(29, DataTypesEnum.Bool, "IsEnumeration"),
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
      new PropertyMap(107, DataTypesEnum.Bool, "IndexedLink"),
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
      (MergeTagData.Properties, MetadataMergeTags)
  ];

  public bool IsLinked => !string.IsNullOrWhiteSpace(LinkIndexTag) && !string.IsNullOrWhiteSpace(LinkedDatabasePath);

  private bool? isLinkId;
  private FieldData? linkedField;
  [JsonIgnore]
  public bool IsLinkIdField
  {
    get
    {
      if (!isLinkId.HasValue)
      {
        linkedField = Database?.FindFieldByLinkIdTag(Tag!);
        isLinkId = linkedField != null;
      }
      return isLinkId.Value;
    }
  }

  [JsonIgnore]
  public FieldData? LinkedField
  {
    get
    {
      if (!isLinkId.HasValue)
      {
        linkedField = Database?.FindFieldByLinkIdTag(Tag!);
        isLinkId = linkedField != null;
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
  }

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
          linkedDataset = LinkedDatabasePath[pos..];
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
  ///  or the real Tag)
  /// </summary>
  [JsonIgnore]
  public string? PhysicalTag
   => IsLinked ? LinkIdTag :
      IsMergedField && LinkedFieldData != null && LinkedFieldData.LinkIdTag != null ?
      LinkedFieldData.LinkIdTag : Tag;

}
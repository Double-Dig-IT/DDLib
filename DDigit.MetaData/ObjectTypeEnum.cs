namespace DDigit.MetaData;

/// <summary>
/// Determines the object type to be read
/// </summary>
public enum ObjectTypeEnum
{
  /// <summary>
  /// An Adlib or Axiell Collections application, aka adlib.pkb
  /// </summary>
  Application = 0,

  /// <summary>
  /// A data source in an application
  /// </summary>
  DataSource = 1,

  /// <summary>
  /// A Method (mostly search methods) in a data source
  /// </summary>
  Method = 2,

  /// <summary>
  /// The definition of a screen, or tab, aka .fmt
  /// </summary>
  Screen = 3,

  /// <summary>
  /// General information about a screen or tab
  /// </summary>
  FormData = 4,

  /// <summary>
  /// A field on a tab or a screen
  /// </summary>
  FormObjectData = 5,

  /// <summary>
  /// A FACS definion,
  /// Old school way to read multiple entities in adapl.
  /// </summary>
  FileAccessControl = 6,

  /// <summary>
  /// An output job
  /// </summary>
  OutputJob = 7,

  /// <summary>
  /// The text for a label in multiple languages.
  /// </summary>
  LabelText = 8,

  /// <summary>
  /// The text for a method label in multiple languages.
  /// </summary>
  MethodText = 9,

  /// <summary>
  /// A database definition, aka *.inf
  /// </summary>
  Database = 10,

  /// <summary>
  /// The definition of a search index.
  /// </summary>
  Index = 11,

  /// <summary>
  /// The metadata for a single field.
  /// </summary>
  Field = 12,

  /// <summary>
  /// The name and limits for a dataset.
  /// </summary>
  Dataset = 13,

  /// <summary>
  /// A pair of fields with source and destination where data is merged in.
  /// </summary>
  MergeTag = 14,

  /// <summary>
  /// A pair of of write back fields
  /// </summary>
  WriteBackTag = 15,

  /// <summary>
  /// Definition of internal links
  /// </summary>
  InternalLink = 17,

  /// <summary>
  /// Definition of a feedback link
  /// </summary>
  FeedbackLink = 18,

  /// <summary>
  /// Text (Title) of a data source
  /// </summary>
  DataSourceText = 19,

  /// <summary>
  /// Name of a field
  /// </summary>
  FieldName = 20,

  /// <summary>
  /// Text (Title) of a form, tab, screen
  /// </summary>
  FormText = 21,

  /// <summary>
  /// Text (Title) of an application
  /// </summary>
  ApplicationTitle = 22,

  /// <summary>
  /// User structure
  /// </summary>
  User = 23,

  /// <summary>
  /// Rights structure for a database
  /// </summary>
  DatabaseRights = 24,

  /// <summary>
  /// Sorting specification for use on a search method
  /// </summary>
  MethodSortSpecification = 25,

  /// <summary>
  /// Rights structure for a dataset
  /// </summary>
  DatasetRights = 26,

  /// <summary>
  /// Rights structure for a field
  /// </summary>
  FieldRights = 27,

  /// <summary>
  /// Rights structure for a method
  /// </summary>
  MethodRights = 28,

  /// <summary>
  /// Rights structure for a form
  /// </summary>
  FormRights = 29,

  /// <summary>
  /// Enumeration value structure
  /// </summary>
  EnumerationValue = 30,

  /// <summary>
  /// Language specific translation for an enumeration value
  /// </summary>
  EnumerationValueText = 31,

  /// <summary>
  /// General application settings structure
  /// </summary>
  ApplicationSetting = 32,

  /// <summary>
  /// Rights structure for a data source
  /// </summary>
  DataSourceRights = 33,

  /// <summary>
  /// Default value structure
  /// </summary>
  Defaults = 34,

  /// <summary>
  /// A list of merge tag structures
  /// </summary>
  MergeListTag = 37,

  /// <summary>
  /// A language specific text for a field on a method
  /// </summary>
  FieldMethodText = 38,

  /// <summary>
  /// A text for a field label
  /// </summary>
  FieldLabelText = 39,

  /// <summary>
  /// Structure for an export job
  /// </summary>
  ExportJob = 41,

  /// <summary>
  /// Text (Title) for a friendly database
  /// </summary>
  FriendlyDatabaseText = 42,

  /// <summary>
  /// Rights structure for a friendly database
  /// </summary>
  FriendlyDatabaseRights = 43,

  /// <summary>
  /// Structure for a friendly database (where one can copy from)
  /// </summary>
  FriendlyDatabase = 44,

  /// <summary>
  /// Rights structure for a job
  /// </summary>
  JobRights = 45,

  /// <summary>
  /// Text (Title) for a job
  /// </summary>
  JobTitle = 46,

  /// <summary>
  /// Text (Title) of a job
  /// </summary>
  JobDescription = 47,

  /// <summary>
  /// ??
  /// </summary>
  LanguageFieldTag = 48,

  /// <summary>
  /// A condition that hides this field.
  /// </summary>
  FieldSuppressCondition = 49,

  /// <summary>
  /// A condition that makes a field read-only
  /// </summary>
  FieldReadOnlyCondition = 50,

  /// <summary>
  /// A data language
  /// </summary>
  DataLanguage = 51,

  /// <summary>
  /// A condition that hides a complete screen, tab or form
  /// </summary>
  FormSuppressCondition = 52,

  /// <summary>
  /// Access rights for a specific enumeration value
  /// </summary>
  EnumerationValueRights = 53,

  /// <summary>
  /// A structure for an external source
  /// </summary>
  ExternalSourceInfo = 54,

  /// <summary>
  /// The name of an external source
  /// </summary>
  ExternalSourceName = 55,

  /// <summary>
  /// Default access rights to pointer files
  /// </summary>
  DefaultPointerFileRights = 56,

  /// <summary>
  /// Default rights to candiate terms
  /// </summary>
  CandidateTermRights = 57,

  /// <summary>
  /// A structure for private cloud data
  /// </summary>
  PrivateCloudData = 58,

  /// <summary>
  /// Text (Title) for a private cloud
  /// </summary>
  PrivateCloudText = 59,

  /// <summary>
  /// Access rights to a specific private cloud
  /// </summary>
  PrivateCloudRights = 60,

  /// <summary>
  /// Mappin f an external data source
  /// </summary>
  ExternalSourceMapping = 62,

  /// <summary>
  /// Link Control
  /// </summary>
  LinkControl = 64,

  /// <summary>
  /// Access rights for a record type
  /// </summary>
  RecordTypeRights = 65,

  /// <summary>
  /// The default acess rights for a record
  /// </summary>
  DefaultRecordRights = 66,

  /// <summary>
  /// A task structure
  /// </summary>
  Task = 67,

  /// <summary>
  /// Access rights for an enumeration value thatdetermines a record type
  /// </summary>
  EnumerationValueRecordTypeRights = 68,

  /// <summary>
  /// Field relation text
  /// </summary>
  FieldRelationText = 69,

  /// <summary>
  /// Reverse relation field text
  /// </summary>
  FieldReverseRelationText = 70,

  /// <summary>
  /// Text (Title) for a task
  /// </summary>
  TaskTitle = 71,

  /// <summary>
  /// Access rights for a task
  /// </summary>
  TaskRights = 72,

  /// <summary>
  /// structure for a connect entity
  /// </summary>
  ConnectEntity = 73,

  /// <summary>
  /// Text (Title) for a connect entity
  /// </summary>
  ConnectEntityText = 74,

  /// <summary>
  /// Access rights to a connect entity
  /// </summary>
  ConnectEntityRights = 75,

  /// <summary>
  /// A metadata merge tag 
  /// </summary>
  MetadataMergeTag = 77,

  /// <summary>
  /// Structure for a form
  /// </summary>
  Form = 78,

  /// <summary>
  /// Associations structure
  /// </summary>
  Associations = 79,

  /// <summary>
  /// Associations value
  /// </summary>
  AssociationValue = 80,

  /// <summary>
  /// A feature of Axiell Collections
  /// </summary> 
  ApplicationFeature = 81,

  /// <summary>
  ///  Access rights to an application feature
  /// </summary>
  ApplicationFeatureRole = 82,

  /// <summary>
  /// A metdata mapping
  /// </summary>
  MetadataMapping = 83,
}
namespace DDigit.MetaData;

public class ApplicationSettingData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public bool ListScreenOptional
  {
    get; private set;
  }

  public bool UseBigButtons
  {
    get; private set;
  }

  public bool CombineSearchDisplay
  {
    get; private set;
  }

  public bool SortDatabaseList
  {
    get; private set;
  }

  public bool AlwaysShowDetailDisplayPageOne
  {
    get; private set;
  }

  public bool AllowEmptyKeySearch
  {
    get; private set;
  }

  public bool UseExpertHelp
  {
    get; private set;
  }

  public bool DefaultFreeTextTruncation
  {
    get; private set;
  }

  public bool EnableHelpKey
  {
    get; private set;
  }

  public LanguageEnum StartupLanguage
  {
    get; private set;
  }

  public int MaxKeys
  {
    get; private set;
  }

  public int MaxRecords
  {
    get; private set;
  }

  public int Milestone
  {
    get; private set;
  }

  public bool AllowAllKeysSearch
  {
    get; private set;
  }

  public bool AllowBooleanCombination
  {
    get; private set;
  }

  public bool EnableHelpAndReturn
  {
    get; private set;
  }

  public bool EnableMenuBar
  {
    get; private set;
  }

  public bool AllowPrintToFile
  {
    get; private set;
  }

  public bool AllowPrintToScreen
  {
    get; private set;
  }

  public bool AllowPrintToPrinter
  {
    get; private set;
  }

  public int PrintDestinations
  {
    get; private set;
  }

  public bool EnableQuit
  {
    get;
    private set;
  }

  public bool StartWithAnyKey
  {
    get; private set;
  }

  public int TimeOut
  {
    get; private set;
  }

  public bool AllowMarking
  {
    get; private set;
  }

  public int MaxPrintRecords
  {
    get; private set;
  }

  public bool AllowUserPrinterSelection
  {
    get; private set;
  }

  public CharacterSetEnum ApplicationCharacterSet
  {
    get; private set;
  }

  public CharacterSetEnum HelpTextCharacterSet
  {
    get; private set;
  }

  public AccessRightsEnum DefaultAccessRights
  {
    get; private set;
  }

  public string? AdaplTextFile
  {
    get; private set;
  }

  public string? ApplicationId
  {
    get; private set;
  }

  public string? ApplicationHelpFile
  {
    get; private set;
  }

  public AuthenticationTypeEnum AuthenticationType
  {
    get; private set;
  }

  public string? AuthenticationSource
  {
    get; private set;
  }

  public string? AuthenticationUserIdField
  {
    get; private set;
  }

  public string? AuthenticationPasswordField
  {
    get; private set;
  }

  public uint ApplicationBackground
  {
    get; private set;
  }

  public bool TabsInScreenColor
  {
    get; private set;
  }

  public string? Obsolete
  {
    get; private set;
  }

  public string? AuthenticationRoleField
  {
    get; private set;
  }

  public string? AuthenticationApplicationIdField
  {
    get; private set;
  }

  public string? AuthenticationEmailField
  {
    get; private set;
  }

  public string? AuthenticationPhoneNumberField
  {
    get; private set;
  }

  public string? AuthenticationFormatString
  {
    get; private set;
  }

  public TwoFactorMethodEnum TwoFactorAuthenticationProvider
  {
    get; private set;
  }

  public string? SmtpServer
  {
    get; private set;
  }

  public int SmtpServerPort
  {
    get; private set;
  }

  public string? AuthenticationServerUrl
  {
    get; private set;
  }

  public string? SenderEmailAddress
  {
    get; private set;
  }

  public List<DataLanguageData> DataLanguages
  {
    get; private set;
  } = [];

  internal void Add(DataLanguageData dataLanguageData) => DataLanguages.Add(dataLanguageData);

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,    "ElementCount"),
     new PropertyMap (1, DataTypesEnum.Bool32I,  "ListScreenOptional"),
     new PropertyMap (2, DataTypesEnum.Bool32,   "UseBigButtons"),
     new PropertyMap (3, DataTypesEnum.Bool32,   "CombineSearchDisplay" ),
     new PropertyMap (4,  DataTypesEnum.Bool32,  "SortDatabaseList" ),
     new PropertyMap (5,  DataTypesEnum.Bool32,  "AlwaysShowDetailDisplayPageOne"),
     new PropertyMap (6,  DataTypesEnum.Bool32,  "AllowEmptyKeySearch"),
     new PropertyMap (7,  DataTypesEnum.Bool32,  "UseExpertHelp"),
     new PropertyMap (8,  DataTypesEnum.Bool32,  "DefaultFreeTextTruncation"),
     new PropertyMap (9,  DataTypesEnum.Bool32,  "EnableHelpKey"),
     new PropertyMap (10, DataTypesEnum.Enum32,  "StartupLanguage", typeof(LanguageEnum)),
     new PropertyMap (11, DataTypesEnum.Int32,   "MaxKeys"),
     new PropertyMap (12, DataTypesEnum.Int32,   "MaxRecords"),
     new PropertyMap (13, DataTypesEnum.Int32,   "Milestone"),
     new PropertyMap (14, DataTypesEnum.Bool32I, "AllowAllKeysSearch"),
     new PropertyMap (15, DataTypesEnum.Bool32I, "AllowBooleanCombination"),
     new PropertyMap (16, DataTypesEnum.Bool32,  "EnableHelpAndReturn"),
     new PropertyMap (17, DataTypesEnum.Bool32I, "EnableMenuBar"),
     new PropertyMap (18, DataTypesEnum.Bool32,  "AllowPrintToFile"),
     new PropertyMap (19, DataTypesEnum.Bool32,  "AllowPrintToScreen"),
     new PropertyMap (20, DataTypesEnum.Bool32,  "AllowPrintToPrinter"),
     new PropertyMap (21, DataTypesEnum.Int32,   "PrintDestinations"),
     new PropertyMap (22, DataTypesEnum.Bool32,  "EnableQuit"),
     new PropertyMap (23, DataTypesEnum.Bool32,  "StartWithAnyKey"),
     new PropertyMap (24, DataTypesEnum.Int32,   "TimeOut"),
     new PropertyMap (25, DataTypesEnum.Bool32I, "AllowMarking"),
     new PropertyMap (26, DataTypesEnum.Int32,   "MaxPrintRecords"),
     new PropertyMap (27, DataTypesEnum.Bool32I, "AllowUserPrinterSelection"),
     new PropertyMap (28, DataTypesEnum.Enum32,  "ApplicationCharacterSet", typeof(CharacterSetEnum)),
     new PropertyMap (29, DataTypesEnum.Enum32,  "HelpTextCharacterSet", typeof(CharacterSetEnum)),
     new PropertyMap (30, DataTypesEnum.Enum32,  "DefaultAccessRights", typeof(AccessRightsEnum)),
     new PropertyMap (31, DataTypesEnum.String,  "AdaplTextFile"),
     new PropertyMap (32, DataTypesEnum.String,  "ApplicationId"),
     new PropertyMap (33, DataTypesEnum.String,  "ApplicationHelpFile"),
     new PropertyMap (34, DataTypesEnum.Enum32,  "AuthenticationType", typeof(AuthenticationTypeEnum)),
     new PropertyMap (35, DataTypesEnum.String,  "AuthenticationSource"),
     new PropertyMap (36, DataTypesEnum.String,  "AuthenticationUserIdField"),
     new PropertyMap (37, DataTypesEnum.String,  "AuthenticationPasswordField"),
     new PropertyMap (38, DataTypesEnum.UInt32,  "ApplicationBackground"),
     new PropertyMap (39, DataTypesEnum.String,  "AuthenticationFormatString"),
     new PropertyMap (40, DataTypesEnum.Skip),
     new PropertyMap (41, DataTypesEnum.Bool,    "TabsInScreenColor"),
     new PropertyMap (42, DataTypesEnum.String,  "Obsolete"),
     new PropertyMap (43, DataTypesEnum.String,  "AuthenticationRoleField"),
     new PropertyMap (44, DataTypesEnum.String,  "AuthenticationApplicationIdField"),
     new PropertyMap (45, DataTypesEnum.String,  "AuthenticationEmailField"),
     new PropertyMap (46, DataTypesEnum.String,  "AuthenticationPhoneNumberField"),
     new PropertyMap (47, DataTypesEnum.Enum32,  "TwoFactorAuthenticationProvider", typeof(TwoFactorMethodEnum)),
     new PropertyMap (48, DataTypesEnum.String,  "SmtpServer"),
     new PropertyMap (49, DataTypesEnum.Int32,   "SmtpServerPort"),
     new PropertyMap (50, DataTypesEnum.String,  "AuthenticationServerUrl"),
     new PropertyMap (51, DataTypesEnum.String,  "SenderEmailAddress")
  ];

  internal override (PropertyList, IEnumerable<object>)[] Children =>
  [
    (DataLanguageData.Properties, DataLanguages)
  ];
}
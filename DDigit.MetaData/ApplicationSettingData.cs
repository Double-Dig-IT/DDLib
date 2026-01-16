namespace DDigit.MetaData;

/// <summary>
/// Various settings of an application
/// </summary>
public class ApplicationSettingData : BaseData
{
  /// <summary>
  /// Constructor to create an empty applicaiton settings object.
  /// </summary>
  public ApplicationSettingData() : base(ObjectTypeEnum.ApplicationSetting)
  {

  }

  /// <summary>
  /// Additional application settings data.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public ApplicationSettingData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
    base(objectType, stream, encoding, Properties, trace)
  {

  }

  /// <summary>
  /// Is the list screen optional? if so a single result in a selection will skip the list screen.
  /// </summary>
  public bool ListScreenOptional
  {
    get;
    private set;
  }

  /// <summary>
  /// This was used to display larger buttons on the main screen in older versions of the software.
  /// </summary>
  [Adlib("This was used to display larger buttons on the main screen in older versions of the software.")]
  [DDesigner(DDesignerPropertyTypeEnum.Bool, "Use Large Buttons", 5)]
  public bool UseLargeButtons
  {
    get;
    set;
  }

  /// <summary>
  /// Determines whether the search box and the key lists are in a single dialog in Adlib for Windows
  /// </summary>
  [Adlib("Determines whether the search box and the key lists are in a single dialog in Adlib for Windows")]
  public bool CombineSearchDisplay
  {
    get;
    private set;
  }

  /// <summary>
  /// Should the database in the search wizard be sorted alphabetically? 
  /// I false it is shown in the same order a set in Axiell Designer.
  /// </summary>
  public bool SortDatabaseList
  {
    get;
    private set;
  }

  /// <summary>
  /// When showing a detailed record (or paging through them) the first tab should be selected (reset to first tab)
  /// If not it stays in the previously selected tab.
  /// </summary>
  public bool AlwaysShowDetailDisplayPageOne
  {
    get;
    private set;
  }

  /// <summary>
  /// Are empty searched allowed, or should the user at least type one character in the search box?
  /// </summary>
  public bool AllowEmptyKeySearch
  {
    get;
    private set;
  }

  /// <summary>
  /// Use expert help (used in Adlib, could be used to enable AI help)
  /// </summary>
  [Adlib]
  public bool UseExpertHelp
  {
    get;
    private set;
  }

  /// <summary>
  /// Apply (right) truncation when performing free text searches
  /// </summary>
  public bool DefaultFreeTextTruncation
  {
    get;
    private set;
  }

  /// <summary>
  /// Enable the help function (why not?) 
  /// </summary>
  public bool EnableHelpKey
  {
    get;
    private set;
  }

  /// <summary>
  /// The language to start the application in.
  /// </summary>
  [Adlib("The language to start the application in.")]
  public LanguageEnum StartupLanguage
  {
    get;
    private set;
  }

  /// <summary>
  /// The maximum number of keys thata re displayed in autocomplete lists
  /// </summary>
  [Adlib("The maximum number of keys that are displayed in autocomplete lists")]
  public int MaxKeys
  {
    get;
    private set;
  }

  /// <summary>
  /// The maximum number of records that are allowed in a search result.
  /// </summary>
  [Adlib("The maximum number of records that are allowed in a search result.")]
  public int MaxRecords
  {
    get;
    private set;
  }

  /// <summary>
  /// The milestone value that is used in progress indicators.
  /// </summary>
  [Adlib]
  public int Milestone
  {
    get;
    private set;
  }

  /// <summary>
  /// Allow a search to start without any input (empty key)
  /// </summary>
  public bool AllowAllKeysSearch
  {
    get; private set;
  }

  /// <summary>
  /// Allow the user to perform Boolean searches (Adlib only)
  /// </summary>
  [Adlib]
  public bool AllowBooleanCombination
  {
    get; private set;
  }

  /// <summary>
  /// Enable the help end return keys (Adlib only)
  /// </summary>
  [Adlib]
  public bool EnableHelpAndReturn
  {
    get; private set;
  }

  /// <summary>
  /// Enable the Adlib Menu bar (Adlib only)
  /// </summary>
  [Adlib]
  public bool EnableMenuBar
  {
    get; private set;
  }

  /// <summary>
  /// Enable the possibility to print to a file (Adlib only)
  /// </summary>
  [Adlib]
  public bool AllowPrintToFile
  {
    get; private set;
  }

  /// <summary>
  /// Enable the possibility to redirect print output to the screen (Adlib only)
  /// </summary>
  [Adlib]
  public bool AllowPrintToScreen
  {
    get; private set;
  }

  /// <summary>
  /// Enable the pssibility to print output to a printer (Adlib only)
  /// Was used to disable output to printers for public access terminals
  /// </summary>
  [Adlib]
  public bool AllowPrintToPrinter
  {
    get; private set;
  }

  /// <summary>
  /// ?? Adlib only
  /// </summary>
  [Adlib]
  public int PrintDestinations
  {
    get; private set;
  }

  /// <summary>
  /// Allow the user to quit the program
  /// Was used when Adlib powered public terminals to prevent the user to close the program.
  /// </summary>
  [Adlib]
  public bool EnableQuit
  {
    get;
    private set;
  }

  /// <summary>
  /// Allowed the user to start searches by pressing any key
  /// Was used when Adlib was used for public access.
  /// </summary>
  [Adlib]
  public bool StartWithAnyKey
  {
    get; private set;
  }

  /// <summary>
  /// A timeout value in seconds.
  /// When this expires, the screen returns to the welcome page.
  /// Used when Adlib was used for public access.
  /// </summary>
  [Adlib]
  public int TimeOut
  {
    get; private set;
  }

  /// <summary>
  /// Alow the selection of records
  /// </summary>
  [Adlib]
  public bool AllowMarking
  {
    get; private set;
  }

  /// <summary>
  /// The maximum number of records that a user is allowed to print
  /// </summary>
  [Adlib]
  public int MaxPrintRecords
  {
    get; private set;
  }

  /// <summary>
  /// Give the user access to a set of printers.
  /// </summary>
  [Adlib]
  public bool AllowUserPrinterSelection
  {
    get; private set;
  }

  /// <summary>
  /// The characterset in use in the application
  /// </summary>
  [Adlib]
  public CharacterSetEnum ApplicationCharacterSet
  {
    get; private set;
  } = CharacterSetEnum.Utf8;

  /// <summary>
  /// The character set used in help files
  /// </summary>
  [Adlib]
  public CharacterSetEnum HelpTextCharacterSet
  {
    get; private set;
  } = CharacterSetEnum.Utf8;

  /// <summary>
  /// Default Access rights in the application.
  /// </summary>
  public AccessRightsEnum DefaultAccessRights
  {
    get; private set;
  }

  /// <summary>
  /// text file name for use in adapl
  /// </summary>
  [Adlib]
  public string? AdaplTextFile
  {
    get; private set;
  }

  /// <summary>
  /// Determines the name of the application
  /// </summary>
  [Adlib]
  public string? ApplicationId
  {
    get; private set;
  }

  /// <summary>
  /// The name of the application specific help file.
  /// </summary>
  [Adlib]
  public string? ApplicationHelpFile
  {
    get; private set;
  }

  /// <summary>
  /// How are we authenticating users.
  /// </summary>
  public AuthenticationTypeEnum AuthenticationType
  {
    get; set;
  }

  /// <summary>
  /// The name of the database in case of Adlib authentication, or th URL in case of http authentication
  /// </summary>
  public string? AuthenticationSource
  {
    get; private set;
  }

  /// <summary>
  /// In case of Adlib authentication, which field is used to store the user id.
  /// </summary>
  public string? AuthenticationUserIdField
  {
    get; private set;
  }

  /// <summary>
  /// In case of Adlib authentication, which field is used to store the password.
  /// </summary>
  public string? AuthenticationPasswordField
  {
    get; private set;
  }

  /// <summary>
  /// The overall background color (RGB) for the application
  /// </summary>
  [Adlib]
  public uint ApplicationBackground
  {
    get; private set;
  }

  /// <summary>
  /// Color the tabs with the background color of the screen
  /// </summary>
  [Adlib]
  public bool TabsInScreenColor
  {
    get; private set;
  }

  /// <summary>
  /// Obsolete
  /// </summary>
  [Adlib("Unknown, not used anymore")]
  public string? Obsolete
  {
    get; private set;
  }

  /// <summary>
  /// When database authentication is in use this defines the field in which the roles are stored.
  /// </summary>
  [Adlib]
  public string? AuthenticationRoleField
  {
    get; private set;
  }

  /// <summary>
  /// For which application ID is this authentication setting valid
  /// </summary>
  [Adlib]
  public string? AuthenticationApplicationIdField
  {
    get; private set;
  }

  /// <summary>
  /// When Adlib database authentication is used, this defines the field in which the email address of a user is stored
  /// </summary>
  [Adlib]
  public string? AuthenticationEmailField
  {
    get; private set;
  }

  /// <summary>
  /// When Adlib database authentication is used, this defines the field in which the phone number of a user is stored
  /// </summary>
  public string? AuthenticationPhoneNumberField
  {
    get; private set;
  }

  /// <summary>
  /// Format string for authentication requests.
  /// </summary>
  [Adlib]
  public string? AuthenticationFormatString
  {
    get; private set;
  }

  /// <summary>
  /// Which two-factor authentication method is used
  /// </summary>
  [Adlib("Used in Axiell Collections")]
  public TwoFactorMethodEnum TwoFactorAuthenticationProvider
  {
    get; private set;
  }

  /// <summary>
  /// What is the ip address or domain name of the smtp server to send mails
  /// </summary>
  public string? SmtpServer
  {
    get; private set;
  }

  /// <summary>
  /// What port is used when sending emails over smtp
  /// </summary>
  public int SmtpServerPort
  {
    get; private set;
  }

  /// <summary>
  /// What is the url of the authentication server
  /// </summary>
  public string? AuthenticationServerUrl
  {
    get; private set;
  }

  /// <summary>
  /// Email address of the sender when sending emails.
  /// </summary>
  public string? SenderEmailAddress
  {
    get; private set;
  }

  /// <summary>
  /// What data alnguages are supported in for multi-lingual fields
  /// </summary>
  public List<DataLanguageData> DataLanguages
  {
    get; private set;
  } = [];

  internal void Add(DataLanguageData dataLanguageData) => DataLanguages.Add(dataLanguageData);

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,    nameof(ElementCount)),
     new PropertyMap (1, DataTypesEnum.Bool32I,  nameof(ListScreenOptional)),
     new PropertyMap (2, DataTypesEnum.Bool32,   nameof(UseLargeButtons)),
     new PropertyMap (3, DataTypesEnum.Bool32,   nameof(CombineSearchDisplay)),
     new PropertyMap (4,  DataTypesEnum.Bool32,  nameof(SortDatabaseList)),
     new PropertyMap (5,  DataTypesEnum.Bool32,  nameof(AlwaysShowDetailDisplayPageOne)),
     new PropertyMap (6,  DataTypesEnum.Bool32,  nameof(AllowEmptyKeySearch)),
     new PropertyMap (7,  DataTypesEnum.Bool32,  nameof(UseExpertHelp)),
     new PropertyMap (8,  DataTypesEnum.Bool32,  nameof(DefaultFreeTextTruncation)),
     new PropertyMap (9,  DataTypesEnum.Bool32,  nameof(EnableHelpKey)),
     new PropertyMap (10, DataTypesEnum.Enum32,  nameof(StartupLanguage), typeof(LanguageEnum)),
     new PropertyMap (11, DataTypesEnum.Int32,   nameof(MaxKeys)),
     new PropertyMap (12, DataTypesEnum.Int32,   nameof(MaxRecords)),
     new PropertyMap (13, DataTypesEnum.Int32,   nameof(Milestone)),
     new PropertyMap (14, DataTypesEnum.Bool32I, nameof(AllowAllKeysSearch)),
     new PropertyMap (15, DataTypesEnum.Bool32I, nameof(AllowBooleanCombination)),
     new PropertyMap (16, DataTypesEnum.Bool32,  nameof(EnableHelpAndReturn)),
     new PropertyMap (17, DataTypesEnum.Bool32I, nameof(EnableMenuBar)),
     new PropertyMap (18, DataTypesEnum.Bool32,  nameof(AllowPrintToFile)),
     new PropertyMap (19, DataTypesEnum.Bool32,  nameof(AllowPrintToScreen)),
     new PropertyMap (20, DataTypesEnum.Bool32,  nameof(AllowPrintToPrinter)),
     new PropertyMap (21, DataTypesEnum.Int32,   nameof(PrintDestinations)),
     new PropertyMap (22, DataTypesEnum.Bool32,  nameof(EnableQuit)),
     new PropertyMap (23, DataTypesEnum.Bool32,  nameof(StartWithAnyKey)),
     new PropertyMap (24, DataTypesEnum.Int32,   nameof(TimeOut)),
     new PropertyMap (25, DataTypesEnum.Bool32I, nameof(AllowMarking)),
     new PropertyMap (26, DataTypesEnum.Int32,   nameof(MaxPrintRecords)),
     new PropertyMap (27, DataTypesEnum.Bool32I, nameof(AllowUserPrinterSelection)),
     new PropertyMap (28, DataTypesEnum.Enum32,  nameof(ApplicationCharacterSet), typeof(CharacterSetEnum)),
     new PropertyMap (29, DataTypesEnum.Enum32,  nameof(HelpTextCharacterSet), typeof(CharacterSetEnum)),
     new PropertyMap (30, DataTypesEnum.Enum32,  nameof(DefaultAccessRights), typeof(AccessRightsEnum)),
     new PropertyMap (31, DataTypesEnum.String,  nameof(AdaplTextFile)),
     new PropertyMap (32, DataTypesEnum.String,  nameof(ApplicationId)),
     new PropertyMap (33, DataTypesEnum.String,  nameof(ApplicationHelpFile)),
     new PropertyMap (34, DataTypesEnum.Enum32,  nameof(AuthenticationType), typeof(AuthenticationTypeEnum)),
     new PropertyMap (35, DataTypesEnum.String,  nameof(AuthenticationSource)),
     new PropertyMap (36, DataTypesEnum.String,  nameof(AuthenticationUserIdField)),
     new PropertyMap (37, DataTypesEnum.String,  nameof(AuthenticationPasswordField)),
     new PropertyMap (38, DataTypesEnum.UInt32,  nameof(ApplicationBackground)),
     new PropertyMap (39, DataTypesEnum.String,  nameof(AuthenticationFormatString)),
     new PropertyMap (40, DataTypesEnum.Skip),
     new PropertyMap (41, DataTypesEnum.Bool,    nameof(TabsInScreenColor)),
     new PropertyMap (42, DataTypesEnum.String,  nameof(Obsolete)),
     new PropertyMap (43, DataTypesEnum.String,  nameof(AuthenticationRoleField)),
     new PropertyMap (44, DataTypesEnum.String,  nameof(AuthenticationApplicationIdField)),
     new PropertyMap (45, DataTypesEnum.String,  nameof(AuthenticationEmailField)),
     new PropertyMap (46, DataTypesEnum.String,  nameof(AuthenticationPhoneNumberField)),
     new PropertyMap (47, DataTypesEnum.Enum32,  nameof(TwoFactorAuthenticationProvider), typeof(TwoFactorMethodEnum)),
     new PropertyMap (48, DataTypesEnum.String,  nameof(SmtpServer)),
     new PropertyMap (49, DataTypesEnum.Int32,   nameof(SmtpServerPort)),
     new PropertyMap (50, DataTypesEnum.String,  nameof(AuthenticationServerUrl)),
     new PropertyMap (51, DataTypesEnum.String,  nameof(SenderEmailAddress))
  ];

  internal override ChildrenList[] Children =>
  [
    new ChildrenList(DataLanguages, DataLanguageData.Properties)
  ];
}
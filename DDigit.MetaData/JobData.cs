namespace DDigit.MetaData;

/// <summary>
/// Base class for output and export jobs
/// </summary>
public class JobData : BaseData, IHasFields
{
  /// <summary>
  /// Contructor that reads JobData from a file.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="trace"></param>
  public JobData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace)
    : base(objectType, stream, encoding, Properties, trace)
  {
  }

  /// <summary>
  /// Constructor to create an empty job.
  /// </summary>
  /// <param name="objectType"></param>
  public JobData(ObjectTypeEnum objectType) 
    : base(objectType, Properties)
  {
  }

  /// <summary>
  /// The adapl that is used to create the output.
  /// </summary>
  public string Adapl
  {
    get; set;
  } = "";

  /// <summary>
  /// An (XSLT) template that is used to create the output.
  /// </summary>
  public string Template
  {
    get; set;
  } = "";

  /// <summary>
  /// The title of the job
  /// </summary>
  public string Title
  {
    get => Texts.Count > 0 ? Texts[0].Text ?? "" : "";
    set
    {
      if (Texts.Count > 0)
      {
        Texts[0].Text = value;
      }
      else
      {
        Texts.Add(new LanguageTextData(ObjectTypeEnum.JobTitle, value));
      }
    }
  }

  /// <summary>
  /// Any comments about the job.
  /// </summary>
  public string Comment
  {
    get; protected set;
  } = "";

  /// <summary>
  /// The type of template 
  /// </summary>
  public TemplateTypeEnum TemplateType
  {
    get; set;
  }

  /// <summary>
  /// A ; separated list of templates
  /// </summary>
  public string Templates
  {
    get; set;
  } = "";

  /// <summary>
  /// The type of XML to generate the output
  /// </summary>
  public XmlTypeEnum XmlType
  {
    get; protected set;
  }

  /// <summary>
  /// A web service url that is used for printing.
  /// </summary>
  public string PrintServiceUrl
  {
    get; protected set;
  } = "";

  /// <summary>
  /// A screen that allows input of parameters for this job.
  /// </summary>
  public string ParametersScreen
  {
    get; 
    protected set;
  } = "";

  /// <summary>
  /// A list of Texts (Titles) for the job.
  /// </summary>
  public TextsList Texts
  {
    get; private set;
  } = [];

  /// <summary>
  /// A list of descriptions that is explaining the job's use
  /// in various languages.
  /// </summary>
  public TextsList Descriptions
  {
    get; private set;
  } = [];

  /// <summary>
  /// A list of access rights (ACL) for this job. 
  /// </summary>
  public AccessControlList AccessRights
  {
    get; private set;
  } = [];

  /// <summary>
  /// FieldList for parameters for this output job
  /// </summary>
  public FieldList Fields
  { 
    get;
    set;
  } = []; 

  /// <summary>
  /// ToString() Override, handy for debugging, returns the English title.
  /// </summary>
  /// <returns></returns>
  public override string? ToString() => Title;

  internal static PropertyList Properties =
  [
    new PropertyMap (0,  DataTypesEnum.Int16,   "ElementCount"),
    new PropertyMap (1,  DataTypesEnum.String,  "Adapl"),
    new PropertyMap (2,  DataTypesEnum.Skip),
    new PropertyMap (3,  DataTypesEnum.String,  "Template"),
    new PropertyMap (4,  DataTypesEnum.String,  "Title"),
    new PropertyMap (5,  DataTypesEnum.String,  "Comment"),
    new PropertyMap (6,  DataTypesEnum.Enum,    "TemplateType", typeof(TemplateTypeEnum)),
    new PropertyMap (7,  DataTypesEnum.Skip),
    new PropertyMap (8,  DataTypesEnum.Skip),
    new PropertyMap (9,  DataTypesEnum.String,  "Templates"),
    new PropertyMap (10, DataTypesEnum.Enum,    "XmlType", typeof (XmlTypeEnum)),
    new PropertyMap (11, DataTypesEnum.Skip),
    new PropertyMap (12, DataTypesEnum.String,  "PrintServiceUrl"),
    new PropertyMap (13, DataTypesEnum.String,  "ParametersScreen")
  ];

  internal override ChildrenList[] Children =>
  [
    new ChildrenList(AccessRights, AccessRightsData.Properties),
    new ChildrenList(Texts, LanguageTextData.Properties),
    new ChildrenList(Descriptions, LanguageTextData.Properties),
    new ChildrenList(Fields, FieldData.Properties)
  ];
}
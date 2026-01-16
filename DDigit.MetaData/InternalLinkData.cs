
namespace DDigit.MetaData;

/// <summary>
/// Represents the info for internal links.
/// 
/// 2024-05-13 BDD Added extra properties.
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>

public class InternalLinkData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace)
{

  /// <summary>
  /// The tag of the term field
  /// </summary>
  public string? TermTag
  {
    get; private set;
  }

  /// <summary>
  /// The type of the internal relationship.
  /// </summary>
  public RelationTypeEnum RelationType
  {
    get; private set;
  }

  /// <summary>
  /// The broader term tag.
  /// </summary>
  public string? BroaderTermTag
  {
    get; private set;
  }

  /// <summary>
  /// The narrower term tag.
  /// </summary>
  public string? NarrowerTermTag
  {
    get; private set;
  }

  /// <summary>
  /// The related term tag.
  /// </summary>
  public string? RelatedTermTag
  {
    get; private set;
  }

  /// <summary>
  /// Related term link id tag
  /// </summary>
  public string? RelatedTermLinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The equivalent term tag.
  /// </summary>
  public string? EquivalentTermTag
  {
    get; private set;
  }

  /// <summary>
  /// equivalent term link id tag.
  /// </summary>
  public string? EquivalentTermLinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The use term tag.
  /// </summary>
  public string? UseTermTag
  {
    get; private set;
  }

  /// <summary>
  /// Use term link id tag.
  /// </summary>
  public string? UseTermLinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The used for term tag.
  /// </summary>
  public string? UsedForTermTag
  {
    get; private set;
  }

  /// <summary>
  /// Used for link id tag
  /// </summary>
  public string? UsedForTermLinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The sort order of relationships.
  /// </summary>
  public SortOrderEnum SortOrder
  {
    get; private set;
  }

  /// <summary>
  /// Internal link on database or dataset?
  /// </summary>
  public LinkScopeTypeEnum LinkScope
  {
    get; private set;
  }

  /// <summary>
  /// The tag for semantic factor fields.
  /// </summary>
  public string? SemanticFactorTag
  {
    get; private set;
  }

  /// <summary>
  /// The tag for semantic factor for fields.
  /// </summary>
  public string? SemanticFactorForTag
  {
    get; private set;
  }

  /// <summary>
  /// The link ref field for broader terms.
  /// </summary>
  public string? BroaderTermLinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The link ref field for narrower terms.
  /// </summary>
  public string? NarrowerTermLinkIdTag
  {
    get; private set;
  }

  /// <summary>
  /// The format string for this link.
  /// </summary>
  public string? FormatString
  {
    get; private set;
  }

  /// <summary>
  /// Are duplicate links allowed?
  /// </summary>
  public bool AllowDuplicates
  {
    get; private set;
  }

  /// <summary>
  /// Is this an indexed link?
  /// </summary>
  public bool IndexedLink
  {
    get; private set;
  }


  /// <summary>
  /// Sort field for broader relations
  /// </summary>
  public string? BroaderSortField
  {
    get; private set;
  }

  /// <summary>
  /// Sort field for narrower relations
  /// </summary>
  public string? NarrowerSortField
  {
    get; private set;
  }

  /// <summary>
  /// The sort order of broader relationships.
  /// </summary>
  public SortOrderEnum BroaderSortOrder
  {
    get; private set;
  }

  /// <summary>
  /// The sort order of narrower relationships.
  /// </summary>
  public SortOrderEnum NarrowerSortOrder
  {
    get; private set;
  }


  /// <summary>
  /// To string (for debugging)
  /// </summary>
  /// <returns></returns>
  public override string ToString() => $"{RelationType} {TermTag}";

  internal void Add(LinkNodeData node)
  {
    if (node.ParentID is null)
    {
      LinkControlNodes.Add(node);
    }
    else
    {
      AddToParent(LinkControlNodes, node);
    }
  }

  private static void AddToParent(List<LinkNodeData> linkControlNodes, LinkNodeData node)
  {
    foreach (var parent in linkControlNodes)
    {
      if (parent.ID == node.ParentID)
      {
        parent.ChildNodes.Add(node);
      }
      else
      {
        AddToParent(parent.ChildNodes, node);
      }
    }
  }

  private static string LinkIdTag(DatabaseData databaseData, string? linkTag)
  {
    var fieldData = databaseData.FindFieldByTagOrName(linkTag ??
      throw new NullReferenceException(nameof(LinkIdTag))) ??
      throw new FieldNotFoundException(linkTag, databaseData.Name);
    return fieldData.LinkIdTag ?? throw new NullReferenceException(fieldData.LinkIdTag);
  }

  internal void AddLinkRefs(DatabaseData databaseData)
  {
    if (!string.IsNullOrWhiteSpace(BroaderTermTag))
    {
      if (string.IsNullOrEmpty(NarrowerTermTag))
      {
        throw new MissingRelationTagException("Narrower term", BroaderTermTag);
      }
      if (string.IsNullOrWhiteSpace(BroaderTermLinkIdTag))
      {
        BroaderTermLinkIdTag = LinkIdTag(databaseData, BroaderTermTag);
      }
    }

    if (!string.IsNullOrWhiteSpace(NarrowerTermTag))
    {
      if (string.IsNullOrEmpty(BroaderTermTag))
      {
        throw new MissingRelationTagException("Broader term", NarrowerTermTag);
      }
      if (string.IsNullOrWhiteSpace(NarrowerTermLinkIdTag))
      {
        NarrowerTermLinkIdTag = LinkIdTag(databaseData, NarrowerTermTag);
      }
    }

    if (!string.IsNullOrWhiteSpace(RelatedTermTag) && string.IsNullOrWhiteSpace(RelatedTermLinkIdTag))
    {
      RelatedTermLinkIdTag = LinkIdTag(databaseData, RelatedTermTag);
    }

    if (!string.IsNullOrWhiteSpace(EquivalentTermTag) && string.IsNullOrWhiteSpace(EquivalentTermLinkIdTag))
    {
      EquivalentTermLinkIdTag = LinkIdTag(databaseData, EquivalentTermTag);
    }

    if (!string.IsNullOrWhiteSpace(UseTermTag) && string.IsNullOrWhiteSpace(UseTermLinkIdTag))
    {
      UseTermLinkIdTag = LinkIdTag(databaseData, UseTermTag);
    }
    if (!string.IsNullOrWhiteSpace(UsedForTermTag) && string.IsNullOrWhiteSpace(UsedForTermLinkIdTag))
    {
      UsedForTermLinkIdTag = LinkIdTag(databaseData, UsedForTermTag);
    }
  }


  /// <summary>
  /// A list of trees, no idea how it's used.
  /// </summary>
  public List<LinkNodeData> LinkControlNodes
  {
    get; private set;
  } = [];

  internal static PropertyList Properties =
   [
        new PropertyMap (0,  DataTypesEnum.Int16,  "ElementCount"),
        new PropertyMap (1,  DataTypesEnum.String, "TermTag"),
        new PropertyMap (2,  DataTypesEnum.Int16,  "RelationType", typeof(RelationTypeEnum)),
        new PropertyMap (3,  DataTypesEnum.String, "BroaderTermTag"),
        new PropertyMap (4,  DataTypesEnum.String, "NarrowerTermTag"),
        new PropertyMap (5,  DataTypesEnum.String, "RelatedTermTag"),
        new PropertyMap (6,  DataTypesEnum.String, "EquivalentTermTag"),
        new PropertyMap (7,  DataTypesEnum.String, "UseTermTag"),
        new PropertyMap (8,  DataTypesEnum.String, "UsedForTermTag"),
        new PropertyMap (9,  DataTypesEnum.Int16,  "SortOrder", typeof(SortOrderEnum)),
        new PropertyMap (10, DataTypesEnum.Int16,  "LinkScope", typeof(LinkScopeTypeEnum)),
        new PropertyMap (11, DataTypesEnum.String, "SemanticFactorTag"),
        new PropertyMap (12, DataTypesEnum.String, "SemanticFactorForTag"),
        new PropertyMap (13, DataTypesEnum.String, "BroaderTermLinkIdTag"),
        new PropertyMap (14, DataTypesEnum.String, "NarrowerTermLinkIdTag"),
        new PropertyMap (15, DataTypesEnum.String, "FormatString"),
        new PropertyMap (16, DataTypesEnum.Bool,   "AllowDuplicates"),
        new PropertyMap (17, DataTypesEnum.Bool,   "IndexedLink"),
        new PropertyMap (18, DataTypesEnum.String, "BroaderSortField"),
        new PropertyMap (19, DataTypesEnum.String, "NarrowerSortField"),
        new PropertyMap (20, DataTypesEnum.Int16,  "BroaderSortOrder", typeof (SortOrderEnum)),
        new PropertyMap (21, DataTypesEnum.Int16,  "NarrowerSortOrder", typeof (SortOrderEnum))
   ];
}
//using Microsoft.CodeAnalysis;

namespace DDigit.Data;

public class Record
{
  public Record(IDataProvider provider, string path, string database, string? dataset, int id = 0)
  {
    this.provider = provider;
    Id = id;
    if (Database == null)
    {
      Database = MetaDataCache.ReadDatabase(path, database, false);
      if (Database == null)
      {
        throw new DatabaseNotFoundException(path, database);
      }
    }

    if (!string.IsNullOrWhiteSpace(dataset))
    {
      Dataset = Database.Datasets.FirstOrDefault(ds => ds.Name == dataset)
        ?? throw new DatasetNotFoundException(dataset, database);
    }
    Creation = Modification = DateTime.Now;
  }

  public Record(IDataProvider provider, int id, object? data, DatabaseData? database)
  {
    this.provider = provider;
    Id = id;
    Database = database;
    DeSerialize(data);
  }

  private readonly IDataProvider provider;

  public int Id
  {
    get; set;
  }

  internal FieldDictionary Fields
  {
    get; set;
  } = [];


  public DatabaseData? Database
  {
    get; private set;
  }

  public DatasetData? Dataset
  {
    get; private set;
  }

  public double? ResponseTime
  {
    get;
    internal set;
  }

  public DateTime Creation
  {
    get;
    internal set;
  }

  public DateTime Modification
  {
    get;
    set;
  }

  public string? DefaultLanguage
  {
    get; set;
  }

  public Element? this[string field, string language] => this[field, 1, language];

  public Element? this[string field, int occ = 1, string language = "", bool invariant = false]
  {

    get
    {
      var task = Task.Run(() => GetDataAsync(field, occ, language));
      task.Wait();
      return task.Result;
    }

    set
    {
      if (!Fields.TryGetValue(field, out var occurrences))
      {
        occurrences = Fields[field] = [];
      }
      while (occurrences.Count < occ)
      {
        occurrences.Add(new Occurrence());
      }
      occurrences[occ - 1] = value != null ? new Occurrence(value) : new Occurrence();
    }
  }

  public async Task<string?> GetAsync(string tagOrFieldName, int occ = 1, string language = "")
  {
    var fieldElement = await GetDataAsync(tagOrFieldName, occ, language);
    return fieldElement != null && fieldElement.Values.Count > 0 ?
      fieldElement.Values.First()?.ToString() : null;
  }

  public string? Get(string tagOrFieldName, int occ = 1, string language = "")
  {
    var task = Task.Run(() => GetAsync(tagOrFieldName, occ, language));
    task.Wait();
    return task.Result;
  }

  public void Set(string field, string? value) => Set(field, 1, value);

  public void Set(string field, int occ, string? value) => Set(field, occ, "", value);

  public void Set(string field, string language, string? value, bool invariant = false) => Set(field, 1, language, value, invariant);

  /// <summary>
  /// Determine the language for the data
  /// </summary>
  /// <param name="fieldData"></param>
  /// <param name="language"></param>
  /// <returns>the language attribute</returns>
  /// <exception cref="LanguageNotSetException"></exception>
  private string DetermineLanguage(FieldData fieldData, string language)
  {
    if (fieldData.IsMultiLingual)
    {
      if (string.IsNullOrWhiteSpace(language))
      {
        if (DefaultLanguage == null)
        {
          throw new LanguageNotSetException(fieldData.ToString());
        }
        language = DefaultLanguage;
      }
    }
    else
    {
      // if the field is not multi-lingual we can ignore the language
      language = "";
    }
    return language;
  }

  public void Set(string tagOrFieldName, int occ, string language, string? value, bool invariant = false)
  {
    var fieldData = DatabaseNotNull.GetFieldByTagOrName(tagOrFieldName);
    foreach (var field in DatabaseNotNull.FieldGroup(fieldData))
    {
      var occurrence = Fields.
          FindOrCreateOccurrenceList(field.Tag).
            FindOrCreate(occ);
      if (field.Tag == fieldData.Tag)
      {
        occurrence.Set(DetermineLanguage(fieldData, language), value, invariant);
      }
    }
  }

  public void Insert(string tagOrFieldName, int occ, string language, string? value, bool invariant = false)
  {
    var fieldData = DatabaseNotNull.GetFieldByTagOrName(tagOrFieldName);
    foreach (var field in DatabaseNotNull.FieldGroup(fieldData))
    {
      var occurrence = Fields.
          FindOrCreateOccurrenceList(field.Tag).
            InsertOrCreate(occ);
      if (field.Tag == fieldData.Tag)
      {
        occurrence.Set(DetermineLanguage(fieldData, language), value, invariant);
      }
    }
  }

  public void Delete(string tagOrFieldName, int occ)
  {
    var fieldData = DatabaseNotNull.GetFieldByTagOrName(tagOrFieldName);
    foreach (var field in DatabaseNotNull.FieldGroup(fieldData))
    {
      Fields.
        FindOrCreateOccurrenceList(field.Tag).
          Delete(occ);
    }
  }

  private DatabaseData DatabaseNotNull
    => Database ?? throw new NullReferenceException(nameof(Database));

  public async Task<Element?> GetDataAsync(string fieldNameOrTag, int occ = 1, string language = "")
  {
    if (fieldNameOrTag == "identifier")
    {
      return new Element(Id);
    }

    var fieldData = ValidateGetData(DatabaseNotNull, fieldNameOrTag, occ, language)!;
    var data = GetData(fieldData, occ, fieldData.IsMultiLingual ? language : "");
    if (data == null && (fieldData.IsLinked || fieldData.IsMergedField))
    {
      var linkFieldData = fieldData.IsLinked ? fieldData : fieldData.LinkedFieldData;
      if (linkFieldData == null)
      {
        throw new NullReferenceException(nameof(linkFieldData));
      }
      var linkedRecord = await GetLinkedRecordAsync(fieldData.IsLinked ? fieldData : linkFieldData, occ);
      if (linkedRecord == null)
      {
        ClearMergedData(linkFieldData);
      }
      else
      {
        linkedRecord.Database ??= linkFieldData.LinkedDatabase;
        GetMergedData(linkFieldData, linkedRecord, occ, language);
      }
      data = GetData(fieldData, occ, fieldData.IsMultiLingual ? language : "");
    }
    if (fieldData.IsEnumeration.HasValue && fieldData.IsEnumeration.Value)
    {
      if (data?.Values.Count > 0)
      {
        data.NeutralValue ??= data.Values.Peek();
        data.Values.Pop();
        data.Values.Push(fieldData.TranslateEnum((string?)data.NeutralValue, language));
      }
    }
    return data;
  }


  private void ClearMergedData(FieldData fieldData) =>
    fieldData.MergeTags.ForEach(mergePair => this[mergePair.Destination!] = null);

  private void GetMergedData(FieldData fieldData, Record linkedRecord, int occ, string language)
  {
    foreach (var mergePair in fieldData.MergeTags)
    {
      this[mergePair.Destination!, occ, language] = linkedRecord[mergePair.Source!, language];
    }
  }

  private Element? GetData(FieldData fieldData, int occ, string language)
    => Fields.TryGetValue(fieldData.Tag!, out var occurrences) && occ <= occurrences.Count ?
      occurrences[occ - 1][language] : null;

  private async Task<Record?> GetLinkedRecordAsync(FieldData fieldData, int occ)
  {

    if (string.IsNullOrWhiteSpace(fieldData.LinkIdTag))
    {
      throw new LinkIdFieldMissingException(fieldData.Name, fieldData.Database?.Name);
    }

    Record? result = null;
    var linkId = this[fieldData.LinkIdTag, occ];
    if (linkId != null)
    {
      var data = linkId.ToString();
      if (!string.IsNullOrWhiteSpace(data))
      {
        result = await GetLinkedRecordAsync(fieldData, data);
      }
    }
    return result;
  }

  private async Task<Record?> GetLinkedRecordAsync(FieldData fieldData, string key)
  {
    var linkedDatabase = fieldData.LinkedDatabase;
    return linkedDatabase != null && int.TryParse(key, out var id) && linkedDatabase.Name != null ?
      await provider.GetRecord(linkedDatabase, linkedDatabase.Name, id) : null;
  }

  private static FieldData? ValidateGetData(DatabaseData database, string field, int occ, string language)
  {
    FieldData? fieldData = null;
    if (database != null)
    {
      fieldData = database.FindFieldByTagOrName(field) ??
        throw new FieldNotFoundException(field, occ, language, database.Name);

      if (occ < 1)
      {
        throw new InvalidOccurrenceException(occ);
      }

      if (occ > 1 && !fieldData.IsRepeated)
      {
        throw new FieldIsNotRepeatedException(field, occ);
      }
    }
    return fieldData;
  }

  private void DeSerialize(object? data)
  {
    if (data != null)
    {
      var xml = XDocument.Parse((string)data);
      var recordElement = xml.Element("record");
      if (recordElement == null)
      {
        throw new NullReferenceException(nameof(recordElement));
      }

      var creationAttribute = recordElement.Attribute("creation");
      if (creationAttribute == null)
      {
        creationAttribute = recordElement.Attribute("created");
        if (creationAttribute == null)
        {
          throw new NullReferenceException(nameof(creationAttribute));
        }
      }
      Creation = DateTime.Parse(creationAttribute.Value);

      var modificationAttribute = recordElement.Attribute("modification");
      if (modificationAttribute == null)
      {
        throw new NullReferenceException(nameof(modificationAttribute));
      }
      Modification = DateTime.Parse(modificationAttribute.Value);

      if (recordElement != null)
      {
        foreach (var fieldElement in recordElement.Elements("field"))
        {
          var tag = fieldElement.Attribute("tag")?.Value;
          var occ = int.Parse(fieldElement.Attribute("occ")?.Value!);
          var languageAttribute = fieldElement.Attribute("lang");
          var language = languageAttribute != null ? languageAttribute.Value : "";
          var invariantAttribute = fieldElement.Attribute("invariant");
          var invariant = invariantAttribute != null && bool.Parse(invariantAttribute.Value);
          var element = new Element(fieldElement.Value, language, invariant);
          this[tag!, occ, language, invariant] = element;
        }
      }
    }
  }

  public XDocument Serialize()
  {
    var xml = new XDocument();
    var recordElement = new XElement("record");
    recordElement.Add(new XAttribute("creation", Creation.ToString("s")));
    recordElement.Add(new XAttribute("modification", Modification.ToString("s")));
    recordElement.Add(new XAttribute("priref", Id));
    foreach (var (tag, occurrences) in Fields)
    {
      var fieldData = DatabaseNotNull.FindFieldByTagOrName(tag) ??
        throw new NullReferenceException(nameof(FieldData));
      if (!fieldData.IsLinked && !fieldData.IsMergedField)
      {
        Serialize(recordElement, tag, occurrences);
      }
    }
    xml.Add(recordElement);
    return xml;
  }

  public async Task<JsonObject> ToJsonAsync(SerializeOptions? options = null)
  {
    var result = new JsonObject();
    var recordObject = new JsonObject();
    result["record"] = recordObject;

    recordObject.AddField("identifier", Id, options);
    recordObject.AddField("creation", Creation, options);
    recordObject.AddField("modification", Modification, options);

    foreach (var field in Fields)
    {
      await AddFieldAsync(recordObject, DatabaseNotNull, field, options);
    }
    return result;
  }

  private async Task AddFieldAsync(JsonObject parent, DatabaseData database,
    KeyValuePair<string, OccurrenceList> field, SerializeOptions? options)
  {
    var fieldData = database.FindFieldByTagOrName(field.Key);
    if (fieldData != null)
    {
      var fieldName = fieldData.IsLinkIdField ? fieldData.LinkedField!.Name : fieldData.Name;
      if (!string.IsNullOrEmpty(fieldName))
      {
        var fields = options?.Fields;
        if (fields == null || fields.Contains(fieldName) || fields.Contains("*"))
        {
          var nodeName = EncodeFieldName(fieldName);
          var groupName = fieldData.Group;
          if (string.IsNullOrEmpty(groupName))
          {
            await AddNonGroupedFieldAsync(parent, nodeName, fieldData, field, options);
          }
          else
          {
            await AddGroupedFieldAsync(parent, EncodeFieldName(groupName), nodeName, fieldData, field, options);
          }
        }
      }
    }
  }

  private async Task AddGroupedFieldAsync(JsonObject parent, string groupNodeName, string nodeName,
    FieldData fieldData, KeyValuePair<string, OccurrenceList> field, SerializeOptions? options)
  {
    if (parent[groupNodeName] is not JsonArray array)
    {
      array = [];
      parent.Add(groupNodeName, array);
    }

    while (array.Count < field.Value.Count)
    {
      array.Add(new JsonObject());
    }

    int occ = 0;
    foreach (var occurrence in field.Value)
    {
      var occurrenceObject = array[occ];
      if (occurrenceObject != null)
      {
        await AddSingleFieldAsync(occurrenceObject, nodeName, fieldData, field.Value[occ], options);
      }
      occ++;
    }
  }

  private async Task AddNonGroupedFieldAsync(JsonNode parent, string nodeName, FieldData fieldData,
    KeyValuePair<string, OccurrenceList> field, SerializeOptions? options)
  {
    if (fieldData.IsRepeated)
    {
      await AddRepeatedFieldAsync(parent, nodeName, fieldData, field, options);
    }
    else
    {
      await AddSingleFieldAsync(parent, nodeName, fieldData, field.Value[0], options);
    }
  }

  private async Task AddRepeatedFieldAsync(JsonNode node, string nodeName, FieldData fieldData,
    KeyValuePair<string, OccurrenceList> field, SerializeOptions? options)
  {
    var array = new JsonArray();
    var arrayNodeName = nodeName;
    foreach (var occurrence in field.Value)
    {
      arrayNodeName = await AddSingleFieldAsync(array, nodeName, fieldData, occurrence, options);
    }
    node[arrayNodeName] = array;
  }

  private async Task<string> AddSingleFieldAsync(JsonNode node, string nodeName, FieldData fieldData,
    Occurrence occurrence, SerializeOptions? options)
  {
    if (!fieldData.IsMultiLingual)
    {
      if (fieldData.IsLinkIdField && fieldData.LinkedField != null)
      {
        var linkId = occurrence[""];
        if (linkId != null)
        {
          if (options != null && options.JsonLD)
          {
            return await AddLinkedData(node, nodeName, fieldData, linkId, options);
          }
          else
          {
            var fields = options?.Fields;
            if (fields != null && fieldData.Name != null && fields.Contains(fieldData.Name))
            {
              AddElementToNode(node, EncodeFieldName(fieldData.Name), linkId);
            }
            var linkedRecord = await GetLinkedRecordAsync(fieldData.LinkedField, linkId.ToString()!);
            if (linkedRecord != null
              && linkedRecord.Fields.TryGetValue(fieldData.LinkedField.LinkIndexTag!, out var linkedOccurrences))
            {
              return await AddSingleFieldAsync(node, EncodeFieldName(fieldData.LinkedField.Name!), fieldData.LinkedField,
                     linkedOccurrences[0], options);
            }
          }
        }
      }
      var element = occurrence[""];
      if (element != null && element.Values != null && element.Values.Count > 0)
      {
        AddElementToNode(node, nodeName, element);
      }
    }
    else
    {
      var languages = new JsonObject();
      foreach (var (language, element) in occurrence.Elements)
      {
        languages[language] = element.ToString();
      }
      AddNodeToNode(node, nodeName, languages);
    }
    return nodeName;
  }

  private async Task<string> AddLinkedData(JsonNode node, string nodeName, FieldData fieldData,
    Element linkId, SerializeOptions options)
  {
    if (fieldData.Name != null)
    {
      var id = GetLinkedDataId(fieldData.LinkedField!, linkId, options);
      if (id != null)
      {
        var jsonLd = new JsonObject { ["@id"] = id };

        var linkedRecord = await GetLinkedRecordAsync(fieldData.LinkedField!, linkId.ToString()!);
        if (linkedRecord != null
          && linkedRecord.Fields.TryGetValue(fieldData.LinkedField!.LinkIndexTag!, out var linkedOccurrences))
        {
          await AddSingleFieldAsync(jsonLd, "term", fieldData.LinkedField, linkedOccurrences[0], options);
        }

        AddNodeToNode(node, nodeName, jsonLd);
      }
    }
    return nodeName;
  }

  private static string? GetLinkedDataId(FieldData fieldData, Element linkId, SerializeOptions options)
  {
    string? result = null;

    if (fieldData.Type == FieldTypeEnum.Image)
    {
      if (options.IIIF != null)
      {
        result = $"{options.IIIF}/{linkId}/full/max/0/default.jpg";
      }
    }
    else
    {
      if (options.API != null && options.NAAN != null)
      {
        var url = $"{options.API}/ark:/{options.NAAN}";

        var database = fieldData.LinkedDatabase?.Name?.ToLower();
        var arkName = $"{database}{(database != null ? "-" : "")}{linkId}";

        result = $"{url}/{arkName}";
      }
    }
    return result;
  }

  private static void AddElementToNode(JsonNode node, string name, Element element)
  {
    if (node is JsonArray array)
    {
      array.Add(element.ToString());
    }
    else
    {
      node[name] = element.ToString();
    }
  }

  private static void AddNodeToNode(JsonNode node, string name, JsonNode addition)
  {
    if (node is JsonArray array)
    {
      array.Add(addition);
    }
    else
    {
      node[name] = addition;
    }
  }

  private static string EncodeFieldName(string fieldName) => fieldName.Replace('.', '-');

  private static void Serialize(XElement recordElement, string tag, OccurrenceList occurrences)
  {
    int occ = 1;
    foreach (var occurrence in occurrences)
    {
      foreach (var (language, element) in occurrence.Elements)
      {
        var fieldElement = new XElement("field");
        if (element.Values.Count > 0)
        {
          fieldElement.Value = element.ToString()!;
        }
        fieldElement.Add(new XAttribute("tag", tag));
        fieldElement.Add(new XAttribute("occ", occ));
        if (!string.IsNullOrEmpty(language))
        {
          fieldElement.Add(new XAttribute("lang", language));
          if (element.Invariant)
          {
            fieldElement.Add(new XAttribute("invariant", true));
          }
        }
        recordElement.Add(fieldElement);
      }
      occ++;
    }
  }

  public bool Match(FieldData fieldData, string? value, string language = "")
  {
    var result = false;
    if (Fields.TryGetValue(fieldData.Tag!, out var occurrences))
    {
      foreach (var occurrence in occurrences)
      {
        var text = occurrence[language]?.Values.First()?.ToString();
        if (text != null && MatchList.Match(value, text))
        {
          result = true;
          break;
        }
      }
    }
    return result;
  }

  public List<string?> FieldNames =>
    FieldTags.Select(tag => Database?.GetFieldNameByTag(tag)).ToList();

  public List<string> FieldTags => [.. Fields.Keys];

  public int RepCount(string fieldNameOrTag)
  {
    if (fieldNameOrTag == "identifier")
    {
      return 1;
    }

    int result = 0;
    foreach (var field in FindFields(fieldNameOrTag))
    {
      var tag = field.PhysicalTag;
      if (tag != null && Fields.TryGetValue(tag, out var occurrences))
      {
        result = Math.Max(occurrences.Count, result);
      }
    }
    return result;
  }

  private List<FieldData> FindFields(string groupOrFieldNameOrTag)
  {
    var fieldData = Database?.FindFieldByTagOrName(groupOrFieldNameOrTag);
    if (fieldData != null)
    {
      return [fieldData!];
    }
    var fields = Database?.FindGroup(groupOrFieldNameOrTag);

    if (fields == null || fields.Count == 0)
    {
      throw new FieldNotFoundException(groupOrFieldNameOrTag, Database!.Name);
    }
    return fields;
  }

  public async Task<List<IndexChanges>> CreateIndexKeysAsync()
  {
    var result = new List<IndexChanges>();
    foreach (var index in DatabaseNotNull.Indexes)
    {
      var changes = new IndexChanges(index);
      await CreateIndexKeysAsync(changes);
      if (changes.Modifications.Count > 0)
      {
        result.Add(changes);
      }
    }
    return result;
  }

  private async Task CreateIndexKeysAsync(IndexChanges changes)
  {
    foreach (var tag in changes.Index.IndexTags)
    {
      if (tag != "%0")
      {
        if (Fields.TryGetValue(tag, out var occurrences))
        {
          if (occurrences != null)
          {
            foreach (var occurrence in occurrences)
            {
              foreach (var (language, element) in occurrence.Elements)
              {
                switch (changes.Index.Type)
                {
                  case IndexTypeEnum.Text:
                    await CreateTextIndexKeysAsync(changes, language, element);
                    break;

                  case IndexTypeEnum.Integer:
                    CreateIntegerIndexKeys(changes, element);
                    break;

                  case IndexTypeEnum.FreeText:
                    await CreateFreeTextIndexKeysAsync(changes, language, element);
                    break;

                  case IndexTypeEnum.IsoDate:
                    CreateIsoDateIndexKeys(changes, element);
                    break;

                  default:
                    throw new DDException($"Index type {changes.Index.Type} is not supported.");
                }
              }
            }
          }
        }
      }
    }
  }

  private void CreateIsoDateIndexKeys(IndexChanges changes, Element element)
  {
    var index = changes.Index;
    var table = index.TableName;
    if (element.Modified)
    {
      var insertValue = element.InsertValue?.ToString();
      if (!string.IsNullOrEmpty(insertValue))
      {
        changes.InsertKey(table, int.Parse(insertValue), Id);
      }

      if (element.Values.Count > 1)
      {
        var deleteValue = element.DeleteValue?.ToString();
        if (!string.IsNullOrEmpty(deleteValue))
        {
          changes.DeleteKey(table, int.Parse(deleteValue), Id);
        }
      }
    }
  }

  private async Task CreateFreeTextIndexKeysAsync(IndexChanges changes, string language, Element element)
  {
    var index = changes.Index;
    var table = index.TableName;
    if (element.Modified)
    {
      var words = TextTokenizer.GetWords(language, element.InsertValue);
      var wordNumbers = await WordList.GetWordNumbers(provider.Repository, words, language);
      foreach (var wordNumber in wordNumbers)
      {
        changes.InsertKey(table, wordNumber, Id);
      }

      if (element.Values.Count > 1)
      {
        var deleteValue = element.DeleteValue;
        if (deleteValue != null)
        {
          words = TextTokenizer.GetWords(language, deleteValue);
          wordNumbers = await WordList.GetWordNumbers(provider.Repository, words, language);
          foreach (var wordNumber in wordNumbers)
          {
            changes.DeleteKey(table, wordNumber, Id);
          }
        }
      }
    }
  }

  private void CreateIntegerIndexKeys(IndexChanges changes, Element element)
  {
    var index = changes.Index;
    var table = index.TableName;
    if (element.Modified)
    {
      var insertValue = element.InsertValue?.ToString();
      if (!string.IsNullOrEmpty(insertValue))
      {
        changes.InsertKey(table, int.Parse(insertValue), Id);
      }

      if (element.Values.Count > 1)
      {
        var deleteValue = element.DeleteValue?.ToString();
        if (!string.IsNullOrEmpty(deleteValue))
        {
          changes.DeleteKey(table, int.Parse(deleteValue), Id);
        }
      }
    }
  }

  private async Task CreateTextIndexKeysAsync(IndexChanges changes, string language, Element element)
  {
    var index = changes.Index;
    var table = index.TableName;
    if (element.Modified)
    {
      string? domain = null;
      if (index.HasDomain)
      {
        // Not ready yet!
        var domainElement = await GetDataAsync(index.DomainTag!);
        domain = domainElement?.Value?.ToString();
      }
      var insertValue = element.InsertValue?.ToString();

      if (!string.IsNullOrEmpty(insertValue))
      {
        var term = Term(index, insertValue);
        var displayTerm = DisplayTerm(index, insertValue);
        if (!string.IsNullOrEmpty(domain))
        {
          changes.InsertKey(table, term, displayTerm, domain, language, Id);
        }
        changes.InsertKey(table, term, displayTerm, null, language, Id);
      }

      if (element.Values.Count > 1)
      {
        var deleteValue = element.DeleteValue?.ToString();
        if (!string.IsNullOrEmpty(deleteValue))
        {
          var term = Term(index, deleteValue);
          var displayTerm = DisplayTerm(index, deleteValue);
          changes.DeleteKey(table, term, displayTerm, domain, language, Id);
        }
      }
    }
  }

  private static string DisplayTerm(IndexData index, string text) =>
    (text.Length > index.Length) ? text[..index.Length] : text;

  private static string Term(IndexData index, string text) => DisplayTerm(index, text).ToLower();

  public override string ToString() => $"{Database} {Id}";

  public async Task ResolveLinksAsync()
  {
    foreach (var (tag, occurrences) in Fields.Clone())
    {
      var fieldData = Database!.FindFieldByTagOrName(tag);
      if (fieldData!.IsLinked)
      {
        await ResolveLinkAsync(fieldData, occurrences);
      }
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, OccurrenceList occurrences)
  {
    int occ = 1;
    foreach (var occurrence in occurrences)
    {
      await ResolveLinkAsync(fieldData, occurrence, occ++);
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, Occurrence occurrence, int occ)
  {
    foreach (var (language, element) in occurrence.Elements)
    {
      await ResolveLinkAsync(fieldData, language, element, occ);
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, string language, Element element, int occ)
  {
    if (fieldData.LinkIdTag == null)
    {
      throw new NullReferenceException(nameof(fieldData.LinkIdTag));
    }

    var term = element.ToString();
    if (term != null)
    {
      var domain = GetDomain(fieldData);
      var linkedDatabase = fieldData.LinkedDatabase!;
      var linkField = linkedDatabase.FindFieldByTagOrName(fieldData.LinkIndexTag!)!;
      var linkDataset = fieldData.LinkedDataset;
      var linkId = await ResolveLinkAsync(linkDataset, linkField, domain, term, language);
      if (linkId == 0)
      {
        throw new DDException($"Cannot resolve link for field {fieldData.Name}, term = {term}, domain = {domain}");
      }
      Set(fieldData.LinkIdTag, occ, linkId.ToString());
    }
  }

  private async Task<int> ResolveLinkAsync(string? linkDataset, FieldData linkFieldData, string? domain, string term, string language)
  {
    int linkId;
    var index = linkFieldData.PreferredIndex!;
    if (string.IsNullOrWhiteSpace(linkDataset))
    {
      linkId = await provider.Repository.FindLink(index.TableName, domain, Term(index, term), language);
      if (linkId == 0)
      {
        // link with domain not found, now try without a domain
        linkId = await provider.Repository.FindLink(index.TableName, "", Term(index, term), language);
        if (linkId == 0)
        {
          // still not found, force it in if this is allowed
          linkId = await ForceLinkAsync(linkFieldData, domain, term, language);
        }
        else
        {
          // we must add the domain to the existing record
          throw new NotImplementedException();
        }
      }
    }
    else
    {
      throw new NotImplementedException();
    }
    return linkId;
  }

  private async Task<int> ForceLinkAsync(FieldData linkFieldData, string? domain, string term, string language)
  {
    var database = linkFieldData.Database ?? throw new NullReferenceException(nameof(linkFieldData.Database));
    var databaseName = database.Name ?? throw new NullReferenceException(nameof(database.Name));
    var index = linkFieldData.PreferredIndex ?? throw new NullReferenceException(nameof(linkFieldData.PreferredIndex));
    var record = new Record(provider, database!.Folder!, databaseName, linkFieldData.LinkedDataset);
    record.Set(linkFieldData.Tag!, language, term, true);
    var domainTag = index.DomainTag;
    if (domain != null && !string.IsNullOrWhiteSpace(domainTag))
    {
      record.Set(domainTag, domain);
    }
    await provider.WriteRecordAsync(record);
    return record.Id;
  }

  private string? GetDomain(FieldData fieldData)
  => !string.IsNullOrWhiteSpace(fieldData.LinkDomain) ? fieldData.LinkDomain :
     !string.IsNullOrEmpty(fieldData.LinkDomainTag) ? this[fieldData.LinkDomainTag]?.ToString() : null;


  /// <summary>
  /// Write this record to the database (Async version)
  /// </summary>
  /// <returns></returns>
  public async Task WriteAsync() => await provider.WriteRecordAsync(this);

  /// <summary>
  /// Write this record to the database.
  /// </summary>
  public void Write()
  {
    var task = Task.Run(WriteAsync);
    task?.Wait();
  }
}

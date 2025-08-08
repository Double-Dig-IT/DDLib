namespace DDigit.Data;

public class Record : IRecord
{
  public Record()
  {

  }

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

  private readonly IDataProvider? provider;

  public int Id
  {
    get; set;
  }

  /// <summary>
  /// The original version of the record.
  /// </summary>
  public Record? Original
  {
    get; set;
  }

  internal FieldDictionary Fields
  {
    get; set;
  } = [];

  public DatabaseData? Database
  {
    get; set;
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

  public string? this[FieldData field]
  {
    get => this[field.Tag!];
    set => this[field.Tag!] = value;
  }

  public string? this[FieldData field, int occ]
  {
    get => this[field.Tag!, occ];
    set => this[field.Tag!, occ] = value;
  }


  public string? this[string field,
                       IDbConnection? connection = null, IDbTransaction? transaction = null,
                       CancellationToken cancellationToken = default]
  {
    get => this[field, 1, "", connection, transaction, cancellationToken]?.ToString();
    set => this[field, 1, "", connection, transaction, cancellationToken] = value;
  }

  public string? this[string field, int occ,
                       IDbConnection? connection = null, IDbTransaction? transaction = null,
                       CancellationToken cancellationToken = default]
  {
    get => this[field, occ, "", connection, transaction, cancellationToken];
    set => this[field, occ, "", connection, transaction, cancellationToken] = value;
  }

  public string? this[string field, string language,
                       IDbConnection? connection = null, IDbTransaction? transaction = null,
                       CancellationToken cancellationToken = default]
  {
    get => this[field, 1, language, connection, transaction, cancellationToken];
    set => this[field, 1, language, connection, transaction, cancellationToken] = value;
  }

  public string? this[string field, int occ, string language,
                       IDbConnection? connection = null, IDbTransaction? transaction = null,
                       CancellationToken cancellationToken = default]
  {
    get
    {
      var task = Task.Run(() => GetAsync(field, occ, language,
                                         connection, transaction, cancellationToken));
      task.Wait(cancellationToken);
      return task.Result;
    }

    set => Set(field, occ, language, value);
  }


  public async Task<string?> GetAsync(string tagOrFieldName,
                                      int occ = 1,
                                      string language = "",
                                      IDbConnection? connection = null,
                                      IDbTransaction? transaction = null,
                                      CancellationToken cancellationToken = default)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, connection, transaction, cancellationToken))?.GetData(language);

  public async Task<int?> GetLinkIdAsync(string tagOrFieldName,
                                     int occ = 1,
                                     IDbConnection? connection = null,
                                     IDbTransaction? transaction = null,
                                     CancellationToken cancellationToken = default)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, connection, transaction, cancellationToken))?.LinkId;

  public string? Get(string tagOrFieldName, int occ,
                    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    var task = Task.Run(() => GetAsync(tagOrFieldName, occ, "",
                                       connection, transaction, cancellationToken));
    task.Wait(cancellationToken);
    return task.Result;
  }

  internal string? Get(string tagOrFieldName,
                    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    var task = Task.Run(() => GetAsync(tagOrFieldName, 1, "",
                                       connection, transaction, cancellationToken));
    task.Wait(cancellationToken);
    return task.Result;
  }

  public void Set(string field, object? value) => Set(field, 1, value);

  public void Set(string field, int occ, object? value) => Set(field, occ, "", value);

  public void Set(string field, string language, object? value, bool invariant = false) =>
    Set(field, 1, language, value, invariant);

  /// <summary>
  /// Determine the language for the data
  /// </summary>
  /// <param name="fieldData"></param>
  /// <param name="language"></param>
  /// <returns>the language attribute</returns>
  /// <exception cref="LanguageNotSetException"></exception>
  private string DetermineLanguage(FieldData fieldData, string? language, bool fromDeserialization = false)
  {
    var result = string.Empty;
    if (fieldData.IsMultiLingual)
    {
      if (language == null || (string.IsNullOrWhiteSpace(language) && !fromDeserialization))
      {
        if (DefaultLanguage == null)
        {
          throw new LanguageNotSetException(fieldData.ToString());
        }
        result = DefaultLanguage;
      }
      else
      {
        result = Languages.GetDataLanguage(language);
      }
    }
    return result;
  }

  public void Set(string tagOrFieldName, int occ, string language, object? value, bool invariant = false)
    => Set(Database!.GetFieldByTagOrName(tagOrFieldName), occ, language, value, invariant);

  public void Set(FieldData fieldData, int occ, string language, object? value, bool invariant = false)
  {
    foreach (var field in DatabaseNotNull.FieldGroup(fieldData))
    {
      Set(fieldData, field, occ, language, value, invariant);
      if (field.IsLinked && !string.IsNullOrWhiteSpace(fieldData.LinkIdTag))
      {
        Set(fieldData.LinkIdTag, occ, 0);
      }
    }
  }

  private void Set(FieldData fieldData, FieldData groupFieldData, int occ, string language, object? value, bool invariant = false)
  {
    if (occ > 1 && !fieldData.IsRepeated)
    {
      throw new FieldIsNotRepeatedException(fieldData.Name!, occ);
    }
    var occurrence = Fields.
            FindOrCreateOccurrenceList(groupFieldData.Tag).
              FindOrCreate(fieldData.OccurrenceDataType, occ);
    if (groupFieldData.Tag == fieldData.Tag)
    {
      if (fieldData.Enumeration)
      {
        occurrence.DataType = OccurrenceDataTypeEnum.Enumeration;
        if (value == null)
        {
          ClearEnumField(occurrence);
        }
        else
        {
          SetEnumField(occurrence,
            fieldData.GetEnumerationValues(value.ToString()!, language));
        }
      }
      else
      {
        var dataType = fieldData.OccurrenceDataType;
        var lang = dataType == OccurrenceDataTypeEnum.Multilingual || dataType == OccurrenceDataTypeEnum.Enumeration ? DetermineLanguage(fieldData, language) : "";
        occurrence.Set(dataType, lang, value, invariant);
      }
    }
  }

  private static void ClearEnumField(Occurrence occurrence)
  {
    occurrence.Elements.Clear();
    occurrence.NeutralValue = string.Empty;
  }

  private static void SetEnumField(Occurrence occurrence, EnumerationValueData enumerationValues)
  {
    occurrence.NeutralValue = enumerationValues.NeutralValue;
    occurrence.Elements.Clear();
    for (var i = 0; i < enumerationValues.Texts.Count; i++)
    {
      var enumValue = enumerationValues.Texts[i].Text;
      var language = Languages.GetAdlibLang(i);

      occurrence.Elements[language] = new Element(enumValue, language);
    }
  }


  public void Insert(string tagOrFieldName, int occ, string language = "", object? value = null, bool invariant = false)
    => Insert(GetFieldData(tagOrFieldName), occ, language, value, invariant);

  public void Insert(FieldData fieldData, int occ, string language = "", object? value = null, bool invariant = false)
  {
    foreach (var fieldInGroupData in Database!.FieldGroup(fieldData))
    {
      // link id fields are created by the linked field, so we do not have to insert them separately.
      if (!fieldInGroupData.IsLinkIdField)
      {
        var occurrence = Fields.
            FindOrCreateOccurrenceList(fieldInGroupData.Tag).
              InsertOrCreate(fieldInGroupData.OccurrenceDataType, occ);
        if (fieldData.Tag == fieldInGroupData.Tag)
        {
          var dataType = fieldInGroupData.OccurrenceDataType;
          var lang = dataType == OccurrenceDataTypeEnum.Multilingual || dataType == OccurrenceDataTypeEnum.Enumeration ? DetermineLanguage(fieldInGroupData, language) : "";
          occurrence.Set(dataType, lang, value, invariant);
          if (fieldInGroupData.IsLinked && fieldInGroupData.LinkIdTag != null)
          {
            // if we just inserted a linked field, then we also need to insert the appropriate link id field and set it to be unresolved (0)
            var linkRefOccurrence = Fields.
              FindOrCreateOccurrenceList(fieldInGroupData.LinkIdTag).
                InsertOrCreate(fieldInGroupData.OccurrenceDataType, occ);
            linkRefOccurrence.Set(OccurrenceDataTypeEnum.LinkRef, "", 0, false);
          }
        }
      }
    }
  }

  public void Delete(string tagOrFieldName, int occ) => Delete(GetFieldData(tagOrFieldName), occ);

  public void Delete (FieldData fieldData, int occ)
  {
    List<string> deletedTags = [];

    foreach (var field in Database!.FieldGroup(fieldData))
    {
      if (deletedTags.Contains(field.Tag!))
      {
        // If we already deleted this tag, skip it.
        // This can happen if the field is part of a group and is linked or merged.
        continue;
      }
      var occurrences = Fields.FindOrCreateOccurrenceList(field.Tag);
      occurrences.Delete(occ);
      deletedTags.Add(field.Tag!);

      // If the field is linked, we also need to remove the link reference.
      if (fieldData.IsLinked && fieldData.LinkIdTag != null && !deletedTags.Contains(fieldData.LinkIdTag))
      {
        var linkRefOccurrences = Fields.
          FindOrCreateOccurrenceList(fieldData.LinkIdTag);
        linkRefOccurrences.Delete(occ);
        deletedTags.Add(fieldData.LinkIdTag!);
      }
    }

    // Remove empty tags.
    foreach (var (key, _) in Fields.Where(f => f.Value.Count == 0))
    {
      Fields.Remove(key, out _);
    }
  }

  private DatabaseData DatabaseNotNull
    => Database ?? throw new NullReferenceException(nameof(Database));

  private async Task<Occurrence?> GetOccurrenceAsync(string fieldNameOrTag, int occ,
                                                     IDbConnection? connection, IDbTransaction? transaction,
                                                     CancellationToken cancellationToken)
  {
    Occurrence? occurrence = null;

    if (FieldData.IsIdentifier(fieldNameOrTag))
    {
      occurrence = new Occurrence(OccurrenceDataTypeEnum.Id, new Element(Id));
    }
    else
    {
      var (root, remainder) = FieldData.GetRoot(fieldNameOrTag);
      var fieldData = ValidateGetData(DatabaseNotNull, root, remainder, occ) ?? throw new FieldNotFoundException(fieldNameOrTag, Database?.Name);
      if (string.IsNullOrWhiteSpace(remainder))
      {
        occurrence = GetOccurrence(fieldData, occ);
      }
      if (occurrence == null)
      {
        // occurrence not found, check if we are asking for a linked field or a merged field
        // if so get the data from the remote record.
        if (fieldData.IsLinked || fieldData.LinkIdTag == null)
        {
          occurrence = await GetOccurrenceForLinkedField(fieldData, remainder, occ, connection, transaction, cancellationToken);
        }
        else if (fieldData.IsMergedField)
        {
          var linkFieldData = fieldData.LinkedFieldData ?? throw new NullReferenceException(nameof(fieldData.LinkedFieldData));
          // force a read and resolve of the merged in group
          var mainLinkOccurrence = await GetOccurrenceAsync(linkFieldData.Tag!, occ, connection, transaction, cancellationToken);
          return mainLinkOccurrence != null ?
            await GetOccurrenceAsync(fieldData.Tag!, occ, connection, transaction, cancellationToken) : null;
        }
        else if (fieldData.IsContextField)
        {
          occurrence = await GetOccurrenceForContextFieldAsync(fieldData, occ, connection, transaction, cancellationToken);
        }
      }
    }

    return occurrence;
  }

  private async Task<Occurrence?> GetOccurrenceForContextFieldAsync(FieldData fieldData, int occ, IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    Occurrence? result = null;
    var parentField = fieldData.ParentField!;
    var linkRefField = Database!.GetFieldByTagOrName(parentField.LinkIdTag) ?? throw new DDException($"No linkID field found for field {parentField}");
    var linkId = GetData(linkRefField, occ)?.LinkId;
    if (linkId.HasValue)
    {
      var term = await GetTermAsync(parentField.LinkedDatabase!, linkId.Value, parentField.LinkIndexTag!, connection, transaction, cancellationToken);
      if (term != null)
      {
        result = new Occurrence(OccurrenceDataTypeEnum.Standard, new Element(term));
      }
    }
    return result;
  }

  private async Task<string?> GetTermAsync(DatabaseData databaseData, int id, string termTag, IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    string? result = string.Empty;
    var linkedRecord = await ReadLinkedRecordAsync(databaseData, id, connection, transaction, cancellationToken);
    if (linkedRecord != null)
    {
      var parentId = await linkedRecord.ParentId(termTag);
      if (parentId != 0)
      {
        result = await GetTermAsync(databaseData, parentId, termTag, connection, transaction, cancellationToken);
      }
      var term = await linkedRecord.GetAsync(termTag, 1, cancellationToken: cancellationToken);
      if (term != null)
      {
        if (result != string.Empty)
        {
          result += '/';
        }
        result += term;
      }
    }
    return result;
  }

  private async Task<int> ParentId(string linkIndexTag)
  {
    int parentId = 0;
    var hierarchy = Database!.InternalLinks.FirstOrDefault(il => il.TermTag == linkIndexTag);
    if (hierarchy != null && hierarchy.BroaderTermLinkIdTag != null)
    {
      var broaderId = await GetAsync(hierarchy.BroaderTermLinkIdTag);
      if (!string.IsNullOrEmpty(broaderId))
      {
        parentId = int.Parse(broaderId);
      }
    }
    return parentId;
  }

  private async Task<Occurrence?> GetOccurrenceForLinkedField(FieldData fieldData, string? path, int occ,
                                      IDbConnection? connection, IDbTransaction? transaction,
                                      CancellationToken cancellationToken)
  {
    var linkFieldData = fieldData.IsLinked ? fieldData : fieldData.LinkedFieldData;
    if (linkFieldData == null)
    {
      throw new NullReferenceException(nameof(linkFieldData));
    }

    var linkOccurrence = await GetOccurrenceAsync(fieldData.LinkIdTag, occ, connection, transaction, cancellationToken);
    if (linkOccurrence != null)
    {
      var linkedRecord = await GetLinkedRecordAsync(linkFieldData, linkOccurrence,
                                                    connection, transaction, cancellationToken);
      if (path != null && linkedRecord != null)
      {
        // If we have a path, we need to go deeper into the linked record.
        return await linkedRecord.GetOccurrenceAsync(path, 1, connection, transaction, cancellationToken);
      }
      else
      {
        if (linkedRecord == null)
        {
          ClearMergedData(linkFieldData, occ);
        }
        else
        {
          linkedRecord.Database ??= linkFieldData.LinkedDatabase;
          await GetMergedDataAsync(linkFieldData, linkedRecord, occ,
                                   connection, transaction, cancellationToken);
        }
      }
    }
    return GetOccurrence(fieldData, occ);
  }

  private Occurrence? GetOccurrence(FieldData fieldData, int occ)
  {
    Occurrence? result = null;
    if (Fields.TryGetValue(fieldData.Tag!, out var occurrences))
    {
      if (occ > 0 && occ <= occurrences.Count)
      {
        result = occurrences[occ - 1];
      }
    }
    return result;
  }

  private void ClearMergedData(FieldData fieldData, int occ)
    => fieldData.MergeTags.ForEach(mergePair => ClearField(mergePair.Destination!, occ));

  private void ClearField(string tag, int occ)
  {
    var fieldData = Database!.FindFieldByTagOrName(tag) ?? throw new FieldNotFoundException(tag, null);
    if (!Fields.TryGetValue(tag, out var occurrences))
    {
      occurrences = Fields[tag] = [];
    }
    while (occurrences.Count < occ)
    {
      occurrences.Add(new Occurrence(fieldData.OccurrenceDataType));
    }
    occurrences[occ - 1] = new Occurrence(fieldData.OccurrenceDataType);
  }

  private async Task GetMergedDataAsync(FieldData fieldData, Record linkedRecord, int occ, IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    foreach (var mergePair in fieldData.MergeTags)
    {
      var occurrence = await linkedRecord.GetOccurrenceAsync(mergePair.Source!, 1, connection, transaction, cancellationToken);
      var destinationFieldData = Database?.GetFieldByTagOrName(mergePair.Destination ??
        throw new NullReferenceException(nameof(mergePair.Destination))) ??
          throw new NullReferenceException(nameof(Database));

      if (!Fields.TryGetValue(mergePair.Destination!, out var occurrences))
      {
        occurrences = Fields[mergePair.Destination!] = [];
      }
      while (occurrences.Count < occ)
      {
        occurrences.Add(new Occurrence(destinationFieldData.OccurrenceDataType));
      }
      if (occurrence != null)
      {
        occurrences[occ - 1] = occurrence;
      }
    }
  }


  private Element? GetData(FieldData fieldData, int occ, string language)
  {
    if (Fields.TryGetValue(fieldData.Tag!, out var occurrences))
    {
      if (occ > 0 && occ <= occurrences.Count)
      {
        // If 5 letter language is used, the 2 letter version is used as fall back.
        var value = language.Length switch
        {
          0 or 2 => occurrences[occ - 1][language],
          5 => occurrences[occ - 1][language] ?? occurrences[occ - 1][Languages.GetMainLanguage(language)],
          _ => throw new LanguageIsNotSupportedException(language),
        };

        if (value == null)
        {
          if (fieldData.IsMultiLingual)
          {
            value = occurrences[occ - 1].Invariant;
          }
          else if (fieldData.Enumeration)
          {
            value = new Element(occurrences[occ - 1].NeutralValue);
          }
        }
        return value;
      }
    }
    return null;
  }

  public Occurrence? GetData(FieldData fieldData, int occ)
  {
    Occurrence? result = null;
    var tag = fieldData.Tag ?? throw new NullReferenceException(nameof(fieldData.Tag));

    if (Fields.TryGetValue(tag, out var occurrences))
    {
      if (occ > 0 && occ <= occurrences.Count)
      {
        result = occurrences[occ - 1];
      }
    }
    return result;
  }

  private async Task<Record?> GetLinkedRecordAsync(FieldData fieldData, Occurrence linkOccurrence,
                                                   IDbConnection? connection,
                                                   IDbTransaction? transaction,
                                                   CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(fieldData.LinkIdTag))
    {
      throw new LinkIdFieldMissingException(fieldData.Name, fieldData.Database?.Name);
    }

    if (fieldData.LinkedDatabase == null)
    {
      throw new NullReferenceException(nameof(fieldData.LinkedDatabase));
    }

    Record? record = null;
    int? linkId;
    try
    {
      linkId = linkOccurrence.LinkId;
    }
    catch (InvalidCastException ex)
    {
      throw new InvalidLinkIdException(fieldData.Name, fieldData.Database?.Name, Id, ex);
    }

    if (linkId.HasValue && linkId.Value > 0)
    {
      record = await ReadLinkedRecordAsync(fieldData.LinkedDatabase, linkId.Value, connection, transaction, cancellationToken);
    }

    return record;
  }

  private async Task<Record?> ReadLinkedRecordAsync(DatabaseData linkedDatabaseData, int id,
                                                   IDbConnection? connection,
                                                   IDbTransaction? transaction,
                                                   CancellationToken cancellationToken)
  => linkedDatabaseData != null && linkedDatabaseData.Name != null ?
        await provider!.ReadRecordAsync(linkedDatabaseData, id, connection, transaction, cancellationToken) : null;

  private static FieldData? ValidateGetData(DatabaseData database, string field, string? remainder, int occ)
  {
    FieldData? result = null;
    if (database != null)
    {
      result = database.FindFieldByTagOrName(field, remainder) ??
        throw new FieldNotFoundException(field, occ, database.Name);

      if (occ < 1)
      {
        throw new InvalidOccurrenceException(occ);
      }

      if (occ > 1 && !result.IsRepeated)
      {
        throw new FieldIsNotRepeatedException(field, occ);
      }
    }
    return result;
  }

  private static XElement ParseRecord(object data)
  {
    var xml = XDocument.Parse((string)data);
    var recordElement = xml.Element("record");
    if (recordElement == null)
    {
      throw new NullReferenceException(nameof(recordElement));
    }
    return recordElement;
  }

  private static DateTime ParseCreationDateTime(XElement record)
  {
    var creationAttribute = record.Attribute("creation");
    if (creationAttribute == null)
    {
      creationAttribute = record.Attribute("created");
      if (creationAttribute == null)
      {
        throw new NullReferenceException(nameof(creationAttribute));
      }
    }
    return DateTime.Parse(creationAttribute.Value);
  }

  private static DateTime ParseModificationDateTime(XElement record)
  {
    var modificationAttribute = record.Attribute("modification");
    if (modificationAttribute == null)
    {
      throw new NullReferenceException(nameof(modificationAttribute));
    }
    return DateTime.Parse(modificationAttribute.Value);
  }

  private (string tag, int occ, Element element) ParseField(XElement field)
  {
    var tag = field.Attribute("tag")?.Value ?? throw new NullReferenceException("Tag");
    var fieldData = Database?.FindFieldByTagOrName(tag);
    var occ = int.Parse(field.Attribute("occ")?.Value!);
    var languageAttribute = field.Attribute("lang");
    var language = languageAttribute != null ? languageAttribute.Value : "";
    var invariantAttribute = field.Attribute("invariant");
    var invariant = invariantAttribute != null && bool.Parse(invariantAttribute.Value);
    var isLocalAttribute = field.Attribute("islocal");
    bool? isLocal = isLocalAttribute != null ? bool.Parse(isLocalAttribute.Value) : null;
    return (tag, occ, new Element(ParseValue(fieldData, field.Value), language, invariant, isLocal));
  }

  private static object? ParseValue(FieldData? fieldData, string value)
    => fieldData != null ?
     // TODO: Add other types
     fieldData.Type switch
     {
       FieldTypeEnum.Integer => !string.IsNullOrEmpty(value) ? int.Parse(value) : 0,
       _ => value,
     } : value;

  private void SetField(FieldDictionary fields, string field, int occ, Element element, bool fromDeserialization = false)
  {
    var fieldData = Database?.GetFieldByTagOrName(field) ??
      throw new FieldNotFoundException(field, Database?.Name);

    var tag = fieldData.Tag ?? throw new NullReferenceException(fieldData.Tag);

    if (!fields.TryGetValue(tag, out var occurrences))
    {
      occurrences = fields[tag] = [];
    }

    while (occurrences.Count < occ)
    {
      occurrences.Add(new Occurrence(fieldData.OccurrenceDataType));
    }

    if (fieldData.Enumeration)
    {
      var occurrence = occurrences[occ - 1] = new Occurrence(OccurrenceDataTypeEnum.Enumeration);
      if (fromDeserialization && string.IsNullOrWhiteSpace((string?)element.Value))
      {
        occurrences[occ - 1] = new Occurrence(fieldData.OccurrenceDataType, element);
      }
      else
      {
        SetEnumField(occurrence,
          fieldData.GetEnumerationValues(element.Value!.ToString()!));
      }
    }
    else if (fieldData.IsMultiLingual)
    {
      if (element.Value is string && !string.IsNullOrWhiteSpace((string?)element.Value))
      {
        element.Language = DetermineLanguage(fieldData, element.Language, fromDeserialization);
      }

      var occurrence = occurrences[occ - 1];
      occurrence.Elements[element.Language] = new(element.Value, element.Language, element.Invariant, element.IsLocal);

      if (element.Invariant)
      {
        occurrence.Elements[""] = new Element(element.Value, "");
      }
    }
    else
    {
      var occurrence = occurrences[occ - 1];
      occurrence.Elements[""] = new(element.Value);
    }
  }

  private void DeSerialize(object? data)
  {
    if (data != null)
    {
      var recordElement = ParseRecord(data);
      Creation = ParseCreationDateTime(recordElement);
      Modification = ParseModificationDateTime(recordElement);
      try
      {
        foreach (var fieldElement in recordElement.Elements("field"))
        {
          var (tag, occ, element) = ParseField(fieldElement);
          SetField(Fields, tag, occ, element, true);  // Allow empty language fields here to be able to read "malformed" records
        }
      }
      catch (Exception ex)
      {
        throw new RecordParserException(Database?.Name, Id, ex);
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
      if (!fieldData.IsLinked &&
          !fieldData.IsMergedField)
      {
        Serialize(fieldData, recordElement, tag, occurrences);
      }
    }
    xml.Add(recordElement);
    return xml;
  }

  public async Task<JsonObject> ToJsonAsync(SerializeOptions? options,
                                            IDbConnection? connection,
                                            IDbTransaction? transaction,
                                            CancellationToken cancellationToken)
  {
    var result = new JsonObject();
    var recordObject = new JsonObject();
    result["record"] = recordObject;

    if (options?.LDContext != null && options.JsonLD && Database?.Name != null)
    {
      recordObject["@context"] = options.LDContext;
      recordObject["@id"] = GetLinkedDataId(Database.Name, Id, options);
    }

    recordObject.AddField("identifier", Id, options);
    recordObject.AddField("creation", Creation, options);
    recordObject.AddField("modification", Modification, options);

    foreach (var field in Fields)
    {
      await AddFieldAsync(recordObject, DatabaseNotNull, field, options,
                          connection, transaction, cancellationToken);
    }
    return result;
  }

  private async Task AddFieldAsync(JsonObject parent, DatabaseData database,
    KeyValuePair<string, OccurrenceList> field, SerializeOptions? options,
    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    var fieldData = database.FindFieldByTagOrName(field.Key);
    if (fieldData != null)
    {
      var fieldName = fieldData.IsLinkIdField ?
                      fieldData.LinkedField!.Name : fieldData.Name;
      if (!string.IsNullOrEmpty(fieldName))
      {
        var fields = options?.Fields;
        if (fields == null || fields.Contains(fieldName) || fields.Contains("*"))
        {
          var nodeName = EncodeFieldName(fieldName);
          var groupName = fieldData.Group;
          if (string.IsNullOrEmpty(groupName))
          {
            await AddNonGroupedFieldAsync(parent, nodeName, fieldData, field, options,
                                          connection, transaction, cancellationToken);
          }
          else
          {
            await AddGroupedFieldAsync(parent, EncodeFieldName(groupName), nodeName,
                                       fieldData, field, options,
                                       connection, transaction, cancellationToken);
          }
        }
      }
    }
  }

  private async Task AddGroupedFieldAsync(JsonObject parent, string groupNodeName, string nodeName,
    FieldData fieldData, KeyValuePair<string, OccurrenceList> field, SerializeOptions? options,
    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
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
        await AddSingleFieldAsync(occurrenceObject, nodeName, fieldData, field.Value[occ], options,
                                  connection, transaction, cancellationToken);
      }
      occ++;
    }
  }

  private async Task AddNonGroupedFieldAsync(JsonNode parent, string nodeName, FieldData fieldData,
    KeyValuePair<string, OccurrenceList> field, SerializeOptions? options,
    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    if (fieldData.IsRepeated)
    {
      await AddRepeatedFieldAsync(parent, nodeName, fieldData, field, options,
                                  connection, transaction, cancellationToken);
    }
    else
    {
      await AddSingleFieldAsync(parent, nodeName, fieldData, field.Value[0], options,
                                connection, transaction, cancellationToken);
    }
  }

  private async Task AddRepeatedFieldAsync(JsonNode node, string nodeName, FieldData fieldData,
    KeyValuePair<string, OccurrenceList> field, SerializeOptions? options,
    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    var array = new JsonArray();
    var arrayNodeName = nodeName;
    foreach (var occurrence in field.Value)
    {
      arrayNodeName = await AddSingleFieldAsync(array, nodeName, fieldData, occurrence, options,
                                                connection, transaction, cancellationToken);
    }
    node[arrayNodeName] = array;
  }

  private async Task<string> AddSingleFieldAsync(JsonNode node, string nodeName, FieldData fieldData,
    Occurrence occurrence, SerializeOptions? options,
    IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken)
  {
    if (!fieldData.IsMultiLingual)
    {
      if (fieldData.IsLinkIdField && fieldData.LinkedField != null)
      {
        var linkId = occurrence[""];
        if (linkId != null)
        {
          if (options != null && options.JsonLD &&
              fieldData.Name != null && fieldData.LinkedField != null && fieldData.LinkedField.LinkedDatabase != null &&
              GetLinkedDataId(fieldData.LinkedField, linkId, options) is string id)
          {
            var jsonLd = new JsonObject { ["@id"] = id };
            var linkedRecord =
              await ReadLinkedRecordAsync(fieldData.LinkedField.LinkedDatabase, int.Parse(linkId.ToString()!),
                                         connection, transaction, cancellationToken);
            if (linkedRecord != null
              && linkedRecord.Fields.TryGetValue(fieldData.LinkedField!.LinkIndexTag!, out var linkedOccurrences))
            {
              await AddSingleFieldAsync(jsonLd, "term", fieldData.LinkedField, linkedOccurrences[0], options,
                                        connection, transaction, cancellationToken);
            }
            AddNodeToNode(node, nodeName, jsonLd);
            return nodeName;
          }
          else
          {
            var fields = options?.Fields;
            if (fields != null && fieldData.Name != null && fields.Contains(fieldData.Name) && fieldData.LinkedField.LinkedDatabase != null)
            {
              AddElementToNode(node, EncodeFieldName(fieldData.Name), linkId);
            }
            var linkedDatabase = fieldData.LinkedField.LinkedDatabase ??
              throw new NullReferenceException(nameof(fieldData.LinkedField.LinkedDatabase));
            var linkedRecord =
              await ReadLinkedRecordAsync(linkedDatabase, (int)linkId.Value!, connection, transaction, cancellationToken);
            if (linkedRecord != null
              && linkedRecord.Fields.TryGetValue(fieldData.LinkedField.LinkIndexTag!, out var linkedOccurrences))
            {
              return await AddSingleFieldAsync(node, EncodeFieldName(fieldData.LinkedField.Name!), fieldData.LinkedField,
                     linkedOccurrences[0], options, connection, transaction, cancellationToken);
            }
          }
        }
      }
      var element = occurrence[""];
      if (element != null && element.Values != null && !element.Values.IsEmpty)
      {
        AddElementToNode(node, nodeName, element);
      }
    }
    else
    {
      var languages = new JsonObject();
      foreach (var (language, element) in occurrence.Elements)
      {
        languages[!string.IsNullOrEmpty(language) ? language : "@none"] = element.ToString();
      }
      if (languages.Count > 1)
      {
        AddNodeToNode(node, nodeName, languages);
      }
      else
      {
        var element = occurrence[""];
        if (element != null && element.Values != null && !element.Values.IsEmpty)
        {
          AddElementToNode(node, nodeName, element);
        }
      }
    }
    return nodeName;
  }

  private static string? GetLinkedDataId(FieldData fieldData, Element linkId, SerializeOptions options)
  {
    string? result = null;

    if (fieldData.Type == FieldTypeEnum.Image && options.IIIF != null)
    {
      result = $"{options.IIIF}/{linkId}/full/max/0/default.jpg";
    }
    else
    {
      var database = fieldData.LinkedDatabase?.Name;
      if (database != null && options.Databases != null &&
          options.Databases.Contains(database) &&
          options.API != null && options.NAAN != null &&
          linkId.IntValue is int identifier)
      {
        result = GetLinkedDataId(database, identifier, options);
      }
    }
    return result;
  }

  private static string? GetLinkedDataId(string database, int id, SerializeOptions options)
  {
    var url = $"{options.API}/ark:/{options.NAAN}";
    var arkName = $"{database}{(database != null ? "/" : "")}{id}";
    return $"{url}/{arkName}";
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


  private static void Serialize(FieldData fieldData, XElement recordElement, string tag, OccurrenceList occurrences)
  {
    try
    {
      int occ = 0;
      foreach (var occurrence in occurrences)
      {
        occ++;

        if (occurrence.NeutralValue != null)
        {
          recordElement.Add(CreateFieldElement(tag, occ, occurrence.NeutralValue));
          continue;
        }

        if (fieldData.IsMultiLingual && occurrence.Elements.Count == 1 && string.IsNullOrEmpty(occurrence.Elements.Keys.First()))
        {
          // Fix for invalid data, if the only language value does not have a language then set it to default (en-US)
          occurrence.Elements["en-US"] = occurrence.Elements[""];
          occurrence.Elements["en-US"].Invariant = true;
        }

        foreach (var (language, element) in occurrence.Elements)
        {
          if (!fieldData.IsMultiLingual || !string.IsNullOrEmpty(language))
          {
            // Link references are internally set to 0 in DDLib, in the database they are stored as blanks
            var value = fieldData.IsLinkIdField && element.Value != null &&
                         element.Value is int v && v == 0 ? string.Empty : element.Value;

            var fieldElement = CreateFieldElement(tag, occ, value);
            if (element.IsLocal.HasValue)
            {
              fieldElement.Add(new XAttribute("islocal", element.IsLocal.Value));
            }

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
        }
      }
    }
    catch (Exception ex)
    {
      throw new SerializationException(fieldData.Database.Name, fieldData.Tag, fieldData.Name, ex);
    }
  }

  private static XElement CreateFieldElement(string tag, int occ, object? value)
  {
    var fieldElement = new XElement("field");
    if (value != null)
    {
      fieldElement.Value = value.ToString()!;
    }
    fieldElement.Add(new XAttribute("tag", tag));
    fieldElement.Add(new XAttribute("occ", occ));
    return fieldElement;
  }

  public bool Match(FieldData fieldData, List<object> values, string language = "")
  {
    var result = false;
    if (Fields.TryGetValue(fieldData.Tag!, out var occurrences))
    {
      foreach (var occurrence in occurrences)
      {
        values.ForEach(value =>
        {
          var text = occurrence[language]?.Values.First()?.ToString();
          if (text != null && MatchList.Match(value.ToString(), text))
          {
            result = true;
          }
        });
        if (result)
        {
          break;
        }
      }
    }
    return result;
  }

  public List<string?> FieldNames => [.. FieldTags.Select(tag => Database?.GetFieldNameByTag(tag))];

  public List<string> FieldTags => [.. Fields.Keys];

  public string? User { get; set; }

  public int RepCount(FieldData field) => RepCount(field.Tag!);

  public int RepCount(string fieldNameOrTag)
  {
    if (fieldNameOrTag == "identifier")
    {
      return 1;
    }

    int result = 0;
    var (root, _) = FieldData.GetRoot(fieldNameOrTag);
    foreach (var field in FindFields(root))
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

  public async Task<IndexChangesList> CreateIndexKeysAsync(IDbConnection connection,
                                                           IDbTransaction transaction,
                                                           CancellationToken cancellationToken)
  {
    var result = new IndexChangesList();
    foreach (var index in DatabaseNotNull.Indexes)
    {
      var changes = await CreateIndexKeysAsync(connection, transaction, index, cancellationToken);
      if (changes.Count != 0)
      {
        result.Add(changes);
      }
    }
    return result;
  }

  private async Task<IndexChanges> CreateIndexKeysAsync(IDbConnection connection, IDbTransaction transaction,
                                                        IndexData index, CancellationToken cancellationToken)
  {
    var indexTag = index.IndexTags.FirstOrDefault() ??
      throw new DDException("IndexTag is missing");  // use the first tag as the index tag, the rest is secondary

    var changes = new IndexChanges(index);
    foreach (var tag in index.IndexTags)
    {
      if (tag != "%0")
      {
        if (Fields.TryGetValue(tag, out var occurrences) && occurrences != null)
        {
          await CreateIndexKeysForOccurrenceAsync(connection, transaction, indexTag, occurrences, changes, 1, cancellationToken);
        }
        if (Original != null && Original.Fields.TryGetValue(tag, out occurrences) && occurrences != null)
        {
          await Original.CreateIndexKeysForOccurrenceAsync(connection, transaction, indexTag, occurrences, changes, -1, cancellationToken);
        }
      }
    }
    return changes;
  }

  private async Task CreateIndexKeysForOccurrenceAsync(IDbConnection connection, IDbTransaction transaction,
                                                       string indexTag,
                                                       OccurrenceList occurrences, IndexChanges changes,
                                                       int count, CancellationToken cancellationToken)
  {
    int occ = 1;
    foreach (var occurrence in occurrences)
    {
      if (occurrence.NeutralValue != null)
      {
        CreateEnumIndexKeysForElement(indexTag, occ, changes, count, occurrence.NeutralValue);
      }
      else
      {
        foreach (var (language, element) in occurrence.Elements)
        {
          await CreateIndexKeysForElement(connection, transaction, indexTag, occ, changes, count, language, element, cancellationToken);
        }
      }
      occ++;
    }
  }

  private async Task CreateIndexKeysForElement(IDbConnection connection, IDbTransaction transaction,
                                               string indexTag,
                                               int occ, IndexChanges changes, int count, string language, Element element,
                                               CancellationToken cancellationToken)
  {
    switch (changes.Index.Type)
    {
      case IndexTypeEnum.Text:
        CreateTextIndexKeys(indexTag, occ, changes, count, language, element);
        break;

      case IndexTypeEnum.Integer:
        CreateIntegerIndexKeys(changes, count, element);
        break;

      case IndexTypeEnum.FreeText:
        if (Database!.FullText)
        {
          CreateTextIndexKeys(indexTag, occ, changes, count, language, element);
        }
        else
        {
          await CreateFreeTextIndexKeysAsync(connection, transaction, changes, count, language, element, cancellationToken);
        }
        break;

      case IndexTypeEnum.IsoDate:
        CreateIsoDateIndexKeys(changes, count, element);
        break;

      case IndexTypeEnum.Date:
        CreateDateIndexKeys(changes, count, element);
        break;

      case IndexTypeEnum.Boolean:
        CreateBooleanIndexKeys(changes, count, element);
        break;

      case IndexTypeEnum.AlphaNumeric:
        CreateAlphaNumericIndexKeys(indexTag, changes, count, element);
        break;

      default:
        throw new DDException($"Index type '{changes.Index.Type}' for tag '{indexTag}' is not supported.");
    }
  }

  private void CreateBooleanIndexKeys(IndexChanges changes, int count, Element element)
  {
    if (element.Value != null)
    {
      var stringValue = element.Value.ToString();
      if (!string.IsNullOrEmpty(stringValue))
      {
        var row = new BooleanIndexRow(changes.Index, stringValue, Id);
        changes.FindOrCreateRow(row).Count += count;
      }
    }
  }

  private void CreateDateIndexKeys(IndexChanges changes, int count, Element element)
  {
    if (element.Value != null)
    {
      var row = new DateIndexRow(changes.Index, element.Value, Id);
      changes.FindOrCreateRow(row).Count += count;
    }
  }

  private async Task CreateFreeTextIndexKeysAsync(IDbConnection connection, IDbTransaction transaction,
                                                  IndexChanges changes, int count, string language,
                                                  Element element, CancellationToken cancellationToken)
  {
    if (element != null)
    {
      var words = TextTokenizer.GetWords(language, element.Value);
      var wordNumbers = await WordList.GetWordNumbers(connection, transaction, provider!.Repository, words, language, cancellationToken);
      foreach (var wordNumber in wordNumbers)
      {
        var row = new IntegerIndexRow(changes.Index, wordNumber, Id);
        changes.FindOrCreateRow(row).Count += count;
      }
    }
  }

  private void CreateIsoDateIndexKeys(IndexChanges changes, int count, Element element)
  {
    if (element.Value != null && !string.IsNullOrEmpty(element.Value.ToString()))
    {
      var index = changes.Index;
      var row = new IsoDateIndexRow(index, element.Value, Id);
      changes.FindOrCreateRow(row).Count += count;
    }
  }

  private void CreateIntegerIndexKeys(IndexChanges changes, int count, Element element)
  {
    int? key = null;
    if (element.Value != null)
    {
      if (element.Value is string)
      {
        var v = element.Value.ToString();
        if (!string.IsNullOrWhiteSpace(v))
        {
          key = int.Parse(v);
        }
      }
      else
      {
        key = (int)element.Value;
      }

      if (key == null)
      {
        throw new NullReferenceException(nameof(key));
      }
      var row = new IntegerIndexRow(changes.Index, (int)key, Id);
      changes.FindOrCreateRow(row).Count += count;
    }
  }

  private void CreateTextIndexKeys(string? tag, int occ, IndexChanges changes, int count, string language, Element element)
  {
    if (element.Value != null)
    {
      var index = changes.Index;
      if (index.HasDomain)
      {
        AddDomains(index, tag, occ, language, element, changes, count);
      }
      else
      {
        AddCount(index, tag, occ, language, null, element, changes, count);
      }
    }
  }

  private void CreateAlphaNumericIndexKeys(string tag, IndexChanges changes, int count, Element element)
  {
    if (element.Value != null)
    {
      var row = new AlphaNumericIndexRow(changes.Index, tag, element.Value, Id);
      changes.FindOrCreateRow(row).Count += count;
    }
  }

  private void CreateEnumIndexKeysForElement(string tag, int occ, IndexChanges changes, int count, object neutralValue)
  {
    var row = new TermIndexRow(changes.Index, tag, occ, neutralValue, Id);
    changes.FindOrCreateRow(row).Count += count;
  }

  private void AddDomains(IndexData index, string? tag, int occ, string language, Element element, IndexChanges changes, int count)
  {
    var domainTag = index.DomainTag ?? throw new NullReferenceException(nameof(index.DomainTag));

    for (int domainOcc = 1; domainOcc <= Fields.RepCount(domainTag); domainOcc++)
    {
      var domain = Fields.GetData(domainTag, domainOcc);
      if (!string.IsNullOrEmpty(domain))
      {
        AddCount(index, tag, occ, language, domain, element, changes, count);
      }
    }
    AddCount(index, tag, occ, language, "", element, changes, count);
  }

  private void AddCount(IndexData index, string? tag, int occ, string language, string? domain, Element element, IndexChanges changes, int count)
  {
    var row = new TermIndexRow(index, tag, occ, element.Value, index.HasDomain ? domain : null, language, Id);
    changes.FindOrCreateRow(row).Count += count;
  }

  public override string ToString() => $"{Database} {Id}";

  public async Task ResolveLinksAsync(IDbConnection connection,
                                      IDbTransaction transaction,
                                      CancellationToken cancellationToken)
  {
    foreach (var (tag, occurrences) in Fields.Clone())
    {
      var fieldData = Database!.FindFieldByTagOrName(tag) ??
        throw new FieldNotFoundException(tag, Database.Name);
      if (fieldData.IsLinked)
      {
        await ResolveLinkAsync(fieldData, occurrences,
                               connection, transaction, cancellationToken);
      }
    }
  }


  public async Task ProcessReverseLinksAsync(IDbConnection currentConnection, IDbTransaction currentTransaction,
                                             CancellationToken cancellationToken)
  {
    foreach (var (tag, occurrences) in Fields)
    {
      var fieldData = Database!.FindFieldByTagOrName(tag) ??
        throw new FieldNotFoundException(tag, Database.Name);
      if (!string.IsNullOrWhiteSpace(fieldData.LinkReverseTag) &&
           fieldData.LinkIdTag != null && fieldData.LinkedDatabase != null && fieldData.LinkReverseTag != null)
      {
        var linkIdOccurrences = Fields[fieldData.LinkIdTag];
        await ProcessReverseLinkAsync(fieldData.LinkedDatabase, fieldData.LinkReverseTag, linkIdOccurrences,
                                      currentConnection, currentTransaction, cancellationToken);
      }
    }
  }

  private async Task ProcessReverseLinkAsync(DatabaseData linkedDatabase, string linkReverseTag, OccurrenceList linkIdOccurrences, IDbConnection connection,
    IDbTransaction transaction, CancellationToken cancellationToken)
  {
    foreach (var occurrence in linkIdOccurrences)
    {
      if (occurrence.Elements.TryGetValue("", out var element))
      {
        var linkId = element.IntValue;
        if (linkId.HasValue)
        {
          await ProcessReverseLinkIdAsync(linkedDatabase, linkReverseTag, linkId.Value,
                                          connection, transaction, cancellationToken);
        }
      }
      // TODO: this should never happen
    }
  }

  private async Task ProcessReverseLinkIdAsync(DatabaseData linkedDatabase, string linkReverseTag, int id,
                                               IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var record = await provider!.ReadRecordAsync(linkedDatabase, id, connection, transaction, cancellationToken);
    if (record != null)
    {
      record.User = User;
      if (record.RepFind(linkReverseTag, Id) == 0)
      {
        record.Append(linkReverseTag, Id);
        await record.WriteAsync(connection, transaction, cancellationToken);
      }
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, OccurrenceList occurrences,
                                    IDbConnection connection,
                                    IDbTransaction transaction,
                                    CancellationToken cancellationToken)
  {
    int occ = 1;
    foreach (var occurrence in occurrences)
    {
      await ResolveLinkAsync(fieldData, occurrence, occ++,
                             connection, transaction, cancellationToken);
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, Occurrence occurrence, int occ,
                                      IDbConnection connection, IDbTransaction transaction,
                                      CancellationToken cancellationToken)
  {
    foreach (var (language, element) in occurrence.Elements)
    {
      await ResolveLinkAsync(fieldData, language, element, occ,
                             connection, transaction, cancellationToken);
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, string language, Element element, int occ,
                                      IDbConnection connection,
                                      IDbTransaction transaction,
                                      CancellationToken cancellationToken)
  {
    if (fieldData.LinkIdTag == null)
    {
      throw new NullReferenceException(nameof(fieldData.LinkIdTag));
    }

    var term = element.ToString();
    if (term != null)
    {
      var domain = await GetDomainAsync(fieldData, connection, transaction, cancellationToken);
      var linkedDatabase = fieldData.LinkedDatabase ??
        throw new NullReferenceException(nameof(fieldData.LinkedDatabase));
      var linkField = linkedDatabase.FindFieldByTagOrName(fieldData.LinkIndexTag!)!;
      var linkedDatasetName = fieldData.LinkedDataset;
      var linkDataset = string.IsNullOrEmpty(linkedDatasetName) ? null :
        linkedDatabase.FindDatasetByName(linkedDatasetName);

      var linkId = await ResolveLinkAsync(linkField, linkDataset,
                                          domain, term, language,
                                          connection, transaction, cancellationToken);
      if (linkId == 0)
      {
        throw new DDException($"Cannot resolve link for field {fieldData.Name}, term = {term}, domain = {domain}");
      }
      Set(fieldData.LinkIdTag, occ, linkId);
    }
  }

  private async Task<int> ResolveLinkAsync(FieldData linkFieldData, DatasetData? linkDataset,
                                           string? domain, string term, string language,
                                           IDbConnection connection,
                                           IDbTransaction transaction,
                                           CancellationToken cancellationToken)
  {
    var index = linkFieldData.PreferredIndex ??
      throw new NullReferenceException(nameof(linkFieldData.PreferredIndex));
    var termValue = KeyConversions.TermValue(term, index.Length);
    var repository = provider!.Repository;
    var fullText = linkFieldData.Database.FullText;
    string table = fullText ? linkFieldData.Database.FullTextTable : index.TableName;
    int linkId = 0;
    if (domain != null)
    {
      // if there is first try to find the link with the domain
      linkId = await repository.FindLink(table, linkDataset, domain, termValue, language,
                                             connection, transaction, cancellationToken);
    }
    if (linkId == 0)
    {
      // link with domain not found, now try without a domain
      linkId = await repository.FindLink(table, linkDataset, null, termValue, language,
                                         connection, transaction, cancellationToken);
      if (linkId == 0)
      {
        // still not found, force it in if this is allowed
        linkId = await ForceLinkAsync(linkFieldData, domain, term, language, User,
                                      connection, transaction, cancellationToken);
      }
      else
      {
        if (domain != null)
        {
          await AddDomainToLinkedRecord(linkFieldData, linkId, domain, User, connection, transaction, cancellationToken);
        }
      }
    }

    return linkId;
  }

  /// <summary>
  ///  Add a domain to an existing linked existing record
  /// </summary>
  /// <param name="linkFieldData">The field for which to add the domain.</param>
  /// <param name="id">The record number of the linked record to add the domain to.</param>
  /// <param name="domain">The domain that we want to add.</param>
  /// <param name="cancellationToken">Cancellation token to abort the adding</param>
  /// <returns>nothing</returns>
  /// <exception cref="NullReferenceException">If the linked record could not be found,</exception>
  private async Task AddDomainToLinkedRecord(FieldData linkFieldData, int id, string domain, string? user,
                                             IDbConnection connection,
                                             IDbTransaction transaction,
                                             CancellationToken cancellationToken)
  {
    var record = await provider!.ReadRecordAsync(linkFieldData.Database ??
      throw new NullReferenceException(nameof(linkFieldData.Database)), id, connection, transaction, cancellationToken);
    if (record == null)
    {
      throw new NullReferenceException(nameof(record));
    }
    record.User = user;
    var domainTag = linkFieldData.GetDomainTag();
    if (record.RepFind(domainTag, domain) == 0)
    {
      var occ = record.RepCount(domainTag) + 1;
      record.Set(domainTag, occ, domain);
      await record.WriteAsync(connection, transaction, cancellationToken);
    }
  }

  private async Task<int> ForceLinkAsync(FieldData linkFieldData, string? domain, string term, string language,
                                         string? user,
                                         IDbConnection connection,
                                         IDbTransaction transaction,
                                         CancellationToken cancellationToken)
  {
    var database = linkFieldData.Database ?? throw new NullReferenceException(nameof(linkFieldData.Database));
    var databaseName = database.Name ?? throw new NullReferenceException(nameof(database.Name));
    var record = new Record(provider!, database.Folder!, databaseName, linkFieldData.LinkedDataset)
    {
      DefaultLanguage = DefaultLanguage,
      User = user
    };
    record.Set(linkFieldData, 1, language, term, true);

    if (domain != null)
    {
      record.Set(linkFieldData.GetDomainTag(), domain);
    }

    await provider!.WriteRecordAsync(record, connection, transaction, cancellationToken);
    return record.Id;
  }

  private async Task<string?> GetDomainAsync(FieldData fieldData,
    IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  => !string.IsNullOrWhiteSpace(fieldData.LinkDomain) ? fieldData.LinkDomain :
     !string.IsNullOrEmpty(fieldData.LinkDomainTag) ?
     (await GetAsync(fieldData.LinkDomainTag, 1, "",
                     connection, transaction, cancellationToken)) : null;


  /// <summary>
  /// Write this record to the database (Async version)
  /// </summary>
  /// <returns>Nothing</returns>
  public Task WriteAsync(IDbConnection? connection = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default) =>
     provider!.WriteRecordAsync(this, connection, transaction, cancellationToken);

  /// <summary>
  /// Delete this record from the database
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns>Nothing</returns>
  public Task DeleteAsync(CancellationToken cancellationToken) =>
     provider!.DeleteRecordAsync(this, cancellationToken);

  /// <summary>
  /// Write this record to the database.
  /// </summary>
  public void Write(IDbConnection? connection = null,
                    IDbTransaction? transaction = null,
                    CancellationToken cancellationToken = default)
  {
    var task = Task.Run(() => WriteAsync(connection, transaction, cancellationToken), cancellationToken);
    task?.Wait(cancellationToken);
  }

  public int RepFind(string tagOrFieldName, object value, string language = "")
  {
    var field = Database!.GetFieldByTagOrName(tagOrFieldName) ?? throw new FieldNotFoundException(tagOrFieldName, Database.Name);
    int result = 0;
    if (Fields.TryGetValue(field.Tag!, out var occurrences))
    {
      var text = value.ToString();
      int occ = 1;
      foreach (var occurrence in occurrences)
      {
        if (occurrence.NeutralValue != null)
        {
          // this is an enumerative field with a neutral value.
          if ((string)occurrence.NeutralValue == text)
          {
            result = occ;
            break;
          }
        }
        else
        {
          if (occurrence.Elements.TryGetValue(language, out var element))
          {
            if (element.Value != null && element.Value.ToString() == text)
            {
              result = occ;
              break;
            }
          }
        }
        occ++;
      }
    }
    return result;
  }

  public IEnumerable<int> Repeats(string fieldOrGroup) =>
    Enumerable.Range(1, RepCount(fieldOrGroup));

  public int Append(string tag, object value)
  {
    int occ = RepCount(tag) + 1;
    Set(tag, occ, value);
    return occ;
  }

  public async Task<bool> LinkIdPresent(string tag, int value,
                                   IDbConnection connection, IDbTransaction transaction,
                                   CancellationToken cancellationToken)
  {
    var result = false;
    var maxOcc = RepCount(tag);
    for (int occ = 1; !result && occ <= maxOcc; occ++)
    {
      var id = await GetLinkIdAsync(tag, occ, connection, transaction, cancellationToken);
      if (id.HasValue && id.Value == value)
      {
        result = true;
        break;
      }
    }
    return result;
  }

  public void Set(FieldData fieldData, object? value) => Set(fieldData, 1, string.Empty, value);

  public void Set(FieldData fieldData, int occ, object? value) => Set(fieldData, occ, string.Empty, value);

  internal object? Get(string tagOrFieldName, int occ = 1) => Get(tagOrFieldName, occ, null, null, default);

  internal object? Get(FieldData fieldData, int occ = 1) => Get(fieldData.Tag!, occ, null, null, default);

  public async Task<string> Context(string fieldName, string separator = "/")
  {
    if (Database == null)
    {
      throw new NullReferenceException(nameof(Database));
    }

    var fieldData = Database.GetFieldByTagOrName(fieldName) ??
      throw new FieldNotFoundException(fieldName, Database.Name);

    var hierarchy = Database.InternalLinks.FirstOrDefault(link => link.TermTag == fieldData.Tag && link.RelationType == RelationTypeEnum.Hierarchical) ??
      throw new DDException("Hierarchy not found for field: " + fieldName);

    var parentIdTag = hierarchy.BroaderTermLinkIdTag ??
      throw new NullReferenceException(nameof(hierarchy.BroaderTermLinkIdTag));

    return await ContextInternal(Database, fieldData, parentIdTag, separator);
  }

  private async Task<string> ContextInternal(DatabaseData database, FieldData fieldData, string parentIdTag, string separator)
  {
    var result = await GetAsync(fieldData.Tag!) ?? string.Empty;

    var parentId = await GetAsync(parentIdTag, 1);
    if (parentId != null)
    {
      var parentRecord = await provider!.ReadRecordAsync(database, int.Parse(parentId), null, null, default);
      if (parentRecord != null)
      {
        result = await parentRecord.ContextInternal(database, fieldData, parentIdTag, separator) + separator + result;
      }
    }

    return result;
  }

  public async Task SetAutoNumberValue(IDbConnection connection, IDbTransaction transaction,
                                 FieldData fieldData, CancellationToken cancellationToken)
  => Set(fieldData, await provider!.GetAutoNumberValue(connection, transaction, fieldData, cancellationToken));

  public FieldData GetFieldData(string tagOrFieldName)
  {
    if (Database == null)
    {
      throw new NullReferenceException(nameof(Database));
    }
    var fieldData = Database!.GetFieldByTagOrName(tagOrFieldName) ??
      throw new FieldNotFoundException(tagOrFieldName, Database.Name);
    return fieldData;
  }

  public void Duplicate(string tagOrFieldName, int occ) => Duplicate(GetFieldData(tagOrFieldName), occ);

  public void Duplicate(FieldData fieldData, int occ)
  {
    foreach (var groupField in Database!.FieldGroup(fieldData))
    {
      var tag = groupField.Tag!;
      var occurrences = Fields[tag];
      var occurrence = occurrences[occ - 1] ?? throw new InvalidOccurrenceException(occ);
      var newOccurrence = occurrences.InsertOrCreate(groupField.OccurrenceDataType, occ);
      newOccurrence.NeutralValue = occurrence.NeutralValue;
      newOccurrence.Elements.Clear();
      newOccurrence.Elements.AddRange(occurrence.Elements);
    }
  }

  /// <summary>
  /// Utility property to dump the record fields and their values.
  /// Handy during debugging or logging.
  /// </summary>
  public IEnumerable<string> Dump
  {
    get
    {
      var dump = new List<string>();
      foreach (var (tag, occurrences) in Fields)
      {
        var fieldData = Database!.FindFieldByTagOrName(tag) ??
          throw new FieldNotFoundException(tag, Database.Name);
        int occ = 1;
        foreach (var occurrence in occurrences)
        {
          foreach (var (language, element) in occurrence.Elements)
          {
            dump.Add($"""
              {fieldData.Name}[{occ},'{language}'] = '{element}'
              """);
          }
          occ++;
        }
      }
      return dump;
    }
  }
}

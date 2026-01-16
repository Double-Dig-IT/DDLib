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
    if (Database is null)
    {
      Database = MetaDataCache.ReadDatabase(path, database, false);
      if (Database is null)
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
    FillDefaults();
  }

  private void FillDefaults()
  {
    if (Database is not null)
    {
      var defaultFields = Database.DefaultValueFields;
      if (defaultFields is not null)
      {
        foreach (var field in defaultFields)
        {
          FillDefault(field);
        }
      }
    }
  }

  private void FillDefault(FieldData field)
  {
    Set(field, field.DefaultType switch
    {
      DefaultTypeEnum.Value => field.Defaults[0].Text,
      DefaultTypeEnum.UserName => Environment.UserName,
      DefaultTypeEnum.CurrentDate => GetCurrentDate(field),
      _ => throw new DDException($"Unsupported default value type {field.DefaultType} for field {field.Name} in database {field.Database.Name}")
    });
  }

  private static string GetCurrentDate(FieldData field)
    =>
    field.Type switch
    {
      FieldTypeEnum.DateIso => DateTime.Now.ToString("yyyy-MM-dd"),
      _ => throw new DDException($"Unsupported default date type {field.Type} for field {field.Name} in database {field.Database.Name}")
    };

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

  public string? this[string tagOrFieldName]
  {
    get => Get(tagOrFieldName, SqlStateInfo.Default);
    set => Set(tagOrFieldName, value);
  }

  public string? this[string tagOrFieldName, int occ]
  {
    get => Get(tagOrFieldName, occ, SqlStateInfo.Default);
    set => Set(tagOrFieldName, occ, "", value, false);
  }

  public string? this[string tagOrFieldName, string language]
  {
    get => Get(tagOrFieldName, 1, language, SqlStateInfo.Default);
    set => Set(tagOrFieldName, 1, language, value, false);
  }

  public string? this[string tagOrFieldName, int occ, string language]
  {
    get => Get(tagOrFieldName, occ, language, SqlStateInfo.Default);
    set => Set(tagOrFieldName, occ, language, value, false);
  }

  public string? this[FieldData fieldData]
  {
    get => Get(fieldData, SqlStateInfo.Default);
    set => Set(fieldData, value);
  }

  public string? this[FieldData fieldData, int occ]
  {
    get => Get(fieldData, occ, SqlStateInfo.Default);
    set => Set(fieldData, occ, value);
  }

  public string? this[FieldData fieldData, int occ, string language]
  {
    get => Get(fieldData, occ, language, SqlStateInfo.Default);
    set => Set(fieldData, occ, language, value);
  }

  internal string? Get(string tagOrFieldName, SqlStateInfo sqlState)
  {
    var task = Task.Run(() => GetAsync(tagOrFieldName, sqlState));
    task.Wait(sqlState.CancellationToken);
    return task.Result;
  }

  public string? Get(string tagOrFieldName, int occ, SqlStateInfo sqlState)
  {
    var task = Task.Run(() => GetAsync(tagOrFieldName, occ, sqlState));
    task.Wait(sqlState.CancellationToken);
    return task.Result;
  }

  public string? Get(string tagOrFieldName, int occ, string language, SqlStateInfo sqlState)
  {
    var task = Task.Run(() => GetAsync(tagOrFieldName, occ, language, sqlState));
    task.Wait(sqlState.CancellationToken);
    return task.Result;
  }

  internal string? Get(FieldData fieldData, SqlStateInfo sqlState)
  {
    var task = Task.Run(() => GetAsync(fieldData, sqlState));
    task.Wait(sqlState.CancellationToken);
    return task.Result;
  }

  public string? Get(FieldData fieldData, int occ, SqlStateInfo sqlState)
  {
    var task = Task.Run(() => GetAsync(fieldData, occ, sqlState));
    task.Wait(sqlState.CancellationToken);
    return task.Result;
  }

  public string? Get(FieldData fieldData, int occ, string language, SqlStateInfo sqlState)
  {
    var task = Task.Run(() => GetAsync(fieldData, occ, language, sqlState));
    task.Wait(sqlState.CancellationToken);
    return task.Result;
  }

  public async Task<string?> GetAsync(string tagOrFieldName, SqlStateInfo sqlState)
   => (await GetOccurrenceAsync(tagOrFieldName, 1, sqlState))?.GetData("");

  public async Task<string?> GetAsync(string tagOrFieldName, CancellationToken cancellationToken)
   => (await GetOccurrenceAsync(tagOrFieldName, 1, new SqlStateInfo { CancellationToken = cancellationToken }))?.GetData("");

  public async Task<string?> GetAsync(string tagOrFieldName, int occ, SqlStateInfo sqlState)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, sqlState))?.GetData("");

  public async Task<string?> GetAsync(string tagOrFieldName, int occ, string language)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, SqlStateInfo.Default))?.GetData(language);

  public async Task<string?> GetAsync(string tagOrFieldName, int occ, string language, SqlStateInfo sqlState)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, sqlState))?.GetData(language);

  public async Task<string?> GetAsync(string tagOrFieldName, int occ, string language, CancellationToken cancellationToken)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, new SqlStateInfo { CancellationToken = cancellationToken }))?.GetData(language);

  public async Task<string?> GetAsync(FieldData fieldData, SqlStateInfo sqlState)
  => (await GetOccurrenceAsync(fieldData, 1, sqlState))?.GetData("");

  public async Task<string?> GetAsync(FieldData fieldData, int occ, SqlStateInfo sqlState)
  => (await GetOccurrenceAsync(fieldData, occ, sqlState))?.GetData("");

  public async Task<string?> GetAsync(FieldData fieldData, int occ, string language, SqlStateInfo sqlState)
  => (await GetOccurrenceAsync(fieldData, occ, sqlState))?.GetData(language);

  public async Task<int?> GetLinkIdAsync(string tagOrFieldName, int occ, SqlStateInfo sqlState)
    => (await GetOccurrenceAsync(tagOrFieldName, occ, sqlState))?.LinkId;

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
      if (language is null || (string.IsNullOrWhiteSpace(language) && !fromDeserialization))
      {
        if (DefaultLanguage is null)
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
    fieldData.ValidateOccurrence(occ);

    var occurrence = Fields.
            FindOrCreateOccurrenceList(groupFieldData.Tag).
              FindOrCreate(fieldData.OccurrenceDataType, occ);
    if (groupFieldData.Tag == fieldData.Tag)
    {
      if (fieldData.IsEnumeration)
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
      var occurrenceList = Fields.FindOrCreateOccurrenceList(fieldInGroupData.Tag!);
      var occurrence = occurrenceList.InsertOrCreate(fieldInGroupData.OccurrenceDataType, occ);

      if (fieldData.Tag == fieldInGroupData.Tag)
      {
        var dataType = fieldInGroupData.OccurrenceDataType;
        var lang = dataType == OccurrenceDataTypeEnum.Multilingual || dataType == OccurrenceDataTypeEnum.Enumeration ? DetermineLanguage(fieldInGroupData, language) : "";
        occurrence.Set(dataType, lang, value, invariant);
      }
    }
  }

  public void Delete(string tagOrFieldName, int occ) => Delete(GetFieldData(tagOrFieldName), occ);

  public void Delete(FieldData fieldData, int occ)
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
      var occurrences = Fields.FindOrCreateOccurrenceList(field.Tag!);
      occurrences.Delete(occ);
      deletedTags.Add(field.Tag!);

      // If the field is linked, we also need to remove the link reference.
      if (fieldData.IsLinked && fieldData.LinkIdTag is not null && !deletedTags.Contains(fieldData.LinkIdTag))
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

  private async Task<Occurrence?> GetOccurrenceForMergedFieldAsync(FieldData fieldData, int occ, SqlStateInfo sqlState)
  {
    var linkFieldData = fieldData.LinkedFieldData ?? throw new DDException(nameof(fieldData.LinkedFieldData));
    var linkIdTag = linkFieldData.LinkIdTag ?? throw new DDException($"linkId tag for field '{linkFieldData.Name}' in database '{linkFieldData.Database.Name}' is null");

    // force a read and resolve of the merged in group
    var mainLinkOccurrence = await GetOccurrenceAsync(linkFieldData, occ, sqlState);
    // Daan: made this a TryGetValue as Fields[linkIdTag] could throw KeyNotFound exception.
    var linkId = Fields.TryGetValue(linkIdTag, out var occurrences) && occurrences is [var first, ..] ? first.LinkId : 0;
    // if the link is 0 the record has not been written yet, stop resolving.
    return mainLinkOccurrence is not null && linkId > 0 ?
      await GetOccurrenceAsync(fieldData, occ, sqlState) : null;
  }

  private async Task<Occurrence?> GetOccurrenceAsync(string fieldNameOrTag, int occ, SqlStateInfo sqlState)
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
      if (occurrence is null)
      {
        occurrence ??= await (fieldData switch
        {
          { IsLinked: true, LinkIdTag: not null } => GetOccurrenceForLinkedField(fieldData, remainder, occ, sqlState),
          { IsMergedField: true } => GetOccurrenceForMergedFieldAsync(fieldData, occ, sqlState),
          { IsContextField: true } => GetOccurrenceForContextFieldAsync(fieldData, occ, sqlState),
          _ => Task.FromResult<Occurrence?>(null)
        });
      }
    }

    return occurrence;
  }

  private async Task<Occurrence?> GetOccurrenceAsync(FieldData fieldData, int occ, SqlStateInfo sqlState)
  {
    var occurrence = GetOccurrence(fieldData, occ);

    if (occurrence is null)
    {
      occurrence ??= await (fieldData switch
      {
        { IsLinked: true, LinkIdTag: not null } => GetOccurrenceForLinkedField(fieldData, null, occ, sqlState),
        { IsMergedField: true } => GetOccurrenceForMergedFieldAsync(fieldData, occ, sqlState),
        { IsContextField: true } => GetOccurrenceForContextFieldAsync(fieldData, occ, sqlState),
        _ => Task.FromResult<Occurrence?>(null)
      });
    }

    return occurrence;
  }

  private async Task<Occurrence?> GetOccurrenceForContextFieldAsync(FieldData fieldData, int occ, SqlStateInfo sqlState)
  {
    Occurrence? result = null;
    var parentField = fieldData.ParentField!;
    var linkRefField = Database!.GetFieldByTagOrName(parentField.LinkIdTag) ?? throw new DDException($"No linkID field found for field {parentField}");
    var linkId = GetData(linkRefField, occ)?.LinkId;
    if (linkId.HasValue)
    {
      var term = await GetTermAsync(parentField.LinkedDatabase!, linkId.Value, parentField.LinkIndexTag!, sqlState);
      if (term != null)
      {
        result = new Occurrence(OccurrenceDataTypeEnum.Standard, new Element(term));
      }
    }
    return result;
  }

  private async Task<string?> GetTermAsync(DatabaseData databaseData, int id, string termTag, SqlStateInfo sqlState)
  {
    string? result = string.Empty;
    var linkedRecord = await ReadLinkedRecordAsync(databaseData, id, sqlState);
    if (linkedRecord != null)
    {
      var parentId = await linkedRecord.ParentId(termTag, sqlState);
      if (parentId != 0)
      {
        result = await GetTermAsync(databaseData, parentId, termTag, sqlState);
      }
      var term = await linkedRecord.GetAsync(termTag, 1, sqlState);
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

  private async Task<int> ParentId(string linkIndexTag, SqlStateInfo sqlState)
  {
    int parentId = 0;
    var hierarchy = Database!.InternalLinks.FirstOrDefault(il => il.TermTag == linkIndexTag);
    if (hierarchy != null && hierarchy.BroaderTermLinkIdTag != null)
    {
      var broaderId = await GetAsync(hierarchy.BroaderTermLinkIdTag, sqlState);
      if (!string.IsNullOrEmpty(broaderId))
      {
        parentId = int.Parse(broaderId);
      }
    }
    return parentId;
  }

  private async Task<Occurrence?> GetOccurrenceForLinkedField(FieldData fieldData, string? path, int occ, SqlStateInfo sqlState)
  {
    var linkFieldData = fieldData.IsLinked ? fieldData : fieldData.LinkedField;
    if (linkFieldData is null)
    {
      throw new NullReferenceException(nameof(linkFieldData));
    }

    var linkOccurrence = await GetOccurrenceAsync(fieldData.LinkIdTag, occ, sqlState);
    if (linkOccurrence is not null)
    {
      var linkedRecord = await GetLinkedRecordAsync(linkFieldData, linkOccurrence, sqlState);
      if (path is not null && linkedRecord is not null)
      {
        // If we have a path, we need to go deeper into the linked record.
        return await linkedRecord.GetOccurrenceAsync(path, 1, sqlState);
      }
      else
      {
        if (linkedRecord is null)
        {
          ClearMergedData(linkFieldData, occ);
        }
        else
        {
          linkedRecord.Database ??= linkFieldData.LinkedDatabase;
          await GetMergedDataAsync(linkFieldData, linkedRecord, occ, sqlState);
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

  private async Task GetMergedDataAsync(FieldData fieldData, Record linkedRecord, int occ, SqlStateInfo sqlState)
  {
    foreach (var mergePair in fieldData.MergeTags)
    {
      var occurrence = await linkedRecord.GetOccurrenceAsync(mergePair.Source!, 1, sqlState);
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
          else if (fieldData.IsEnumeration)
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

  private async Task<Record?> GetLinkedRecordAsync(FieldData fieldData, Occurrence linkOccurrence, SqlStateInfo sqlState)
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
      record = await ReadLinkedRecordAsync(fieldData.LinkedDatabase, linkId.Value, sqlState);
    }

    return record;
  }

  private async Task<Record?> ReadLinkedRecordAsync(DatabaseData linkedDatabaseData, int id, SqlStateInfo sqlState)
  => linkedDatabaseData != null && linkedDatabaseData.Name != null ?
        await provider!.ReadRecordAsync(linkedDatabaseData, id, sqlState) : null;

  private FieldData? ValidateGetData(DatabaseData database, string field, string? remainder, int occ)
  {
    FieldData? fieldData = null;
    if (database != null)
    {
      fieldData = database.FindFieldByTagOrName(field, remainder) ??
        throw new FieldNotFoundException(field, occ, database.Name, Id);

      fieldData.ValidateOccurrence(occ);
    }
    return fieldData;
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

  private object? ParseValue(FieldData? fieldData, string value)
  {
    if (fieldData is not null && !string.IsNullOrWhiteSpace(value))
    {
      switch (fieldData.Type)
      {
        case FieldTypeEnum.Integer:
          if (int.TryParse(value, out var intValue))
          {
            return intValue;
          }
          throw new InvalidIntegerException(fieldData.Database?.Name, fieldData.Name, value, Id);
        default:
          return value;
      }
    }
    return value;
  }


  private void DeSerialize(object? data)
  {
    void SetField(FieldDictionary fields, string field, int occ, Element element)
    {
      var fieldData = Database?.FindFieldByTagOrName(field) ??
        throw new FieldNotFoundException(field, occ, Database?.Name, Id);

      var tag = fieldData.Tag ?? throw new NullReferenceException(fieldData.Tag);

      OccurrenceList GetOccurrenceList()
      {
        if (!fields.TryGetValue(tag, out var occurrences))
        {
          occurrences = fields[tag] = [];
        }

        while (occurrences.Count < occ)
        {
          occurrences.Add(new Occurrence(fieldData.OccurrenceDataType));
        }
        return occurrences;
      }

      switch (fieldData)
      {
        case { IsLinked: true }:
          break; // Linked field is dynamically rendered on GetOccurrenceAsync, ignore this value

        case { IsMergedField: true }:
          break; // Merged field is dynamically rendered on GetOccurrenceAsync, ignore this value

        case { IsContextField: true }:
          break; // Context field is dynamically rendered on GetOccurrenceAsync, ignore this value

        case { IsEnumeration: true }:
          {
            var occurrences = GetOccurrenceList();
            var occurrence = occurrences[occ - 1] = new Occurrence(OccurrenceDataTypeEnum.Enumeration);
            if (string.IsNullOrWhiteSpace((string?)element.Value))
            {
              occurrences[occ - 1] = new Occurrence(fieldData.OccurrenceDataType, element);
            }
            else
            {
              SetEnumField(occurrence,
                fieldData.GetEnumerationValues(element.Value!.ToString()!));
            }
          }
          break;

        case { IsMultiLingual: true }:
          {
            if (element.Value is string && !string.IsNullOrWhiteSpace((string?)element.Value))
            {
              element.Language = DetermineLanguage(fieldData, element.Language, fromDeserialization: true);
            }

            var occurrences = GetOccurrenceList();
            var occurrence = occurrences[occ - 1];
            occurrence.Elements[element.Language] = new(element.Value, element.Language, element.Invariant, element.IsLocal);

            if (element.Invariant)
            {
              occurrence.Elements[""] = new Element(element.Value, "");
            }
          }
          break;

        default:
          {
            var occurrences = GetOccurrenceList();
            var occurrence = occurrences[occ - 1];
            occurrence.Elements[""] = new(element.Value);
          }
          break;
      }
    }

    if (data is not null)
    {
      var recordElement = ParseRecord(data);
      Creation = ParseCreationDateTime(recordElement);
      Modification = ParseModificationDateTime(recordElement);

      try
      {
        foreach (var fieldElement in recordElement.Elements("field"))
        {
          var (tag, occ, element) = ParseField(fieldElement);
          SetField(Fields, tag, occ, element);  // Allow empty language fields here to be able to read "malformed" records
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
          !fieldData.IsMergedField &&
          !fieldData.IsContextField)
      {
        Serialize(fieldData, recordElement, tag, occurrences);
      }
    }
    xml.Add(recordElement);
    return xml;
  }

  public Task<JsonObject> ToJsonAsync()
    => ToJsonAsync(null, SqlStateInfo.Default);

  public Task<JsonObject> ToJsonAsync(CancellationToken cancellationToken)
    => ToJsonAsync(null, new SqlStateInfo { CancellationToken = cancellationToken });

  public Task<JsonObject> ToJsonAsync(SerializeOptions? options)
    => ToJsonAsync(options, SqlStateInfo.Default);

  public Task<JsonObject> ToJsonAsync(SerializeOptions? options, CancellationToken cancellationToken)
    => ToJsonAsync(options, new SqlStateInfo { CancellationToken = cancellationToken });

  public async Task<JsonObject> ToJsonAsync(SerializeOptions? options, SqlStateInfo sqlState)
  {
    var result = new JsonObject();
    var recordObject = new JsonObject();
    result["record"] = recordObject;

    async Task AddFieldAsync(JsonObject parent, FieldData fieldData, string fieldName)
    {
      var nodeName = fieldData.Name!;
      var groupName = fieldData.Group;
      if (!string.IsNullOrEmpty(groupName))
      {
        await AddGroupedFieldAsync();
      }
      else
      {
        await AddNonGroupedFieldAsync();
      }

      async Task AddGroupedFieldAsync()
      {
        var repCount = RepCount(fieldData);
        if (repCount > 0)
        {
          if (parent[groupName] is not JsonArray array)
          {
            array = [];
            parent.Add(groupName, array);
          }

          while (array.Count < repCount)
          {
            array.Add(new JsonObject());
          }

          for (var occ = 1; occ <= repCount; occ++)
          {
            if (array[occ - 1] is { } occurrenceObject)
            {
              await AddOccurrence(occurrenceObject, fieldData, fieldName, occ);
            }
          }
        }
      }

      async Task AddNonGroupedFieldAsync()
      {
        if (fieldData.IsRepeated)
        {
          var repCount = RepCount(fieldData);
          if (repCount > 0)
          {
            if (parent[nodeName] is not JsonArray array)
            {
              array = [];
              parent.Add(nodeName, array);
            }

            for (var occ = 1; occ <= repCount; occ++)
            {
              await AddOccurrence(array, fieldData, fieldName, occ);
            }
          }
        }
        else
        {
          await AddOccurrence(parent, fieldData, fieldName, 1);
        }
      }

      async Task AddOccurrence(JsonNode node, FieldData occurrenceFieldData, string occurrenceFieldName, int occ)
      {
        var occurrence = await GetOccurrenceAsync(occurrenceFieldName, occ, sqlState);
        if (occurrence != null)
        {
          var occurrenceNodeName = occurrenceFieldData.Name!;
          switch (occurrenceFieldData)
          {
            case { IsLinked: true }:

              // TODO: Deal with possible indirection
              if (FieldData.GetRoot(occurrenceFieldName).remainder is { } linkField)
              {
                // Excessive and dirty fix to get the IndexField for indirection...
                //static string DetermineIndexField(string source, string old, string _new)
                //  => source.Length > old.Length ? string.Concat(source.AsSpan(0, source.Length - old.Length), _new) : _new;
              }
              else
              {
                var linkedId = (await GetOccurrenceAsync(occurrenceFieldData.LinkIdTag, occ, sqlState))?.LinkId;
                if (linkedId == null)
                {
                  // This is a case of data corruption, but we do not want the API to crash on it, so just ignore the situation.
                  return;
                }
                var linkedDatase = occurrenceFieldData.LinkedDatabase!;
                var linkedData = GetLinkedDataFromNode(linkedDatase.Name!, linkedId.Value);
                // TODO: IIIF currently only gets added on linked fields...
                if (occurrenceFieldData.Type == FieldTypeEnum.Image)
                {
                  linkedData.AddIIIF(linkedId.ToString(), options);
                }
                await AddOccurrence(linkedData, occurrenceFieldData.LinkedFieldData!, occurrenceFieldName, occ);
              }
              break;

            case { IsLinkIdField: true }:
              await AddOccurrence(node, occurrenceFieldData!.LinkedField!, occurrenceFieldData.LinkedField!.Name!, occ);
              break;

            case { IsMultiLingual: true }:

              var languages = new JsonObject();
              foreach (var (language, languageElement) in occurrence.Elements)
              {
                languages[!string.IsNullOrEmpty(language) ? language : "@none"] = languageElement.ToString();
              }
              node.Add(occurrenceNodeName, languages);
              break;

            case { IsEnumeration: true }:
              node.Add(occurrenceNodeName, occurrence.NeutralValue);
              break;

            default:
              node.Add(occurrenceNodeName, occurrence[""]);
              break;
          }

          JsonObject GetLinkedDataFromNode(string linkedDatabase, int linkedId)
          {
            JsonObject GetNewLinkedObject()
            {
              var result = new JsonObject();
              result.AddId(linkedDatabase, linkedId, options);
              return result;
            }

            JsonObject GetLinkedObjectFromArray(JsonArray array)
            {
              if (array.ElementAtOrDefault(occ - 1) is not JsonObject linkedData)
              {
                linkedData = GetNewLinkedObject();
                array.Add(linkedData);
              }
              return linkedData;
            }

            JsonObject GetLinkedDataFromNode(JsonNode array)
            {
              if (node[occurrenceNodeName] is not JsonObject linkedData)
              {
                node[occurrenceNodeName] = linkedData = GetNewLinkedObject();
              }
              return linkedData;
            }

            return node is JsonArray array ?
              GetLinkedObjectFromArray(array) : GetLinkedDataFromNode(node);
          }
        }
      }
    }

    if (options?.LDContext != null)
    {
      recordObject["@context"] = options.LDContext;
    }

    recordObject.AddId(Database!.Name!, Id, options);
    recordObject.AddField("creation", Creation, options);
    recordObject.AddField("modification", Modification, options);

    var fields = options?.Fields ?? [];
    if (fields.Count == 0 || fields.Contains("*"))
    {
      fields.Remove("*");
      fields = [.. fields, .. Fields.Keys];
    }
    foreach (var field in fields)
    {
      if (DatabaseNotNull.FindFieldByTagOrName(field) is { } fieldData)
      {
        await AddFieldAsync(recordObject, fieldData, field);
      }
    }

    return result;
  }

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
    if (FieldData.IsIdentifier(fieldNameOrTag))
    {
      return 1;
    }

    int result = 0;
    var (root, _) = FieldData.GetRoot(fieldNameOrTag);
    foreach (var field in FindFields())
    {
      if (Fields.TryGetValue(field.PhysicalTag!, out var occurrences))
      {
        result = Math.Max(occurrences.Count, result);
      }
      // Also test the tag itself, it might be unresolved
      if (Fields.TryGetValue(field.Tag!, out var tagOccurrences))
      {
        result = Math.Max(tagOccurrences.Count, result);
      }
    }
    return result;

    List<FieldData> FindFields()
    {
      var fieldData = Database?.FindFieldByTagOrName(root);
      if (fieldData is not null)
      {
        return [fieldData!];
      }
      var fields = Database?.FindGroup(root);

      if (fields == null || fields.Count == 0)
      {
        throw new FieldNotFoundException(root, Database!.Name);
      }
      return fields;
    }
  }


  public async Task<IndexChangesList> CreateIndexKeysAsync(SqlStateInfo sqlState)
  {
    async Task<IndexChanges> CreateIndexKeysAsync(IndexData index, SqlStateInfo sqlState)
    {
      var indexTag = index.IndexTags.FirstOrDefault() ??
        throw new DDException("IndexTag is missing");  // use the first tag as the index tag, the rest is secondary

      var changes = new IndexChanges(index);
      foreach (var tag in index.IndexTags)
      {
        if (tag != "%0")
        {
          if (Fields.TryGetValue(tag, out var occurrences) && occurrences is not null)
          {
            await CreateIndexKeysForOccurrenceAsync(indexTag, occurrences, changes, 1, sqlState);
          }
          if (Original is not null && Original.Fields.TryGetValue(tag, out occurrences) && occurrences is not null)
          {
            await Original.CreateIndexKeysForOccurrenceAsync(indexTag, occurrences, changes, -1, sqlState);
          }
        }
      }
      return changes;
    }

    var result = new IndexChangesList();
    foreach (var index in DatabaseNotNull.Indexes)
    {
      var changes = await CreateIndexKeysAsync(index, sqlState);
      if (changes.Count != 0)
      {
        result.Add(changes);
      }
    }
    return result;
  }


  private async Task CreateIndexKeysForOccurrenceAsync(string indexTag, OccurrenceList occurrences, IndexChanges changes,
                                                       int count, SqlStateInfo sqlState)
  { 
    var index = changes.Index;
    if (index is null)
    {
      throw new NullReferenceException(nameof(index));
    }

    int occ = 1;

    async Task CreateIndexKeysForElement(string language, Element element)
    {
      void AddCount(string? domain, Element element, int count)
      {
        var row = new TermIndexRow(index, indexTag, occ, element.Value, index.HasDomain ? domain : null, language, Id);
        changes.FindOrCreateRow(row).Count += count;
      }

      void AddDomains(Element element, int count)
      {
        var domainTag = index.DomainTag ?? throw new NullReferenceException(nameof(index.DomainTag));

        for (int domainOcc = 1; domainOcc <= Fields.RepCount(domainTag); domainOcc++)
        {
          var domain = Fields.GetData(domainTag, domainOcc);
          if (!string.IsNullOrEmpty(domain))
          {
            AddCount(domain, element, count);
          }
        }
        AddCount("", element, count);
      }

      async Task CreateFreeTextIndexKeysAsync()
      {
        if (element is not null)
        {
          var words = TextTokenizer.GetWords(element.Value);
          var wordNumbers = await WordList.GetWordNumbers(provider!.Repository, words, language, sqlState);
          foreach (var wordNumber in wordNumbers)
          {
            var row = new IntegerIndexRow(changes.Index, wordNumber, Id);
            changes.FindOrCreateRow(row).Count += count;
          }
        }
      }

      void CreateTextIndexKeys()
      {
        if (element.Value != null)
        {
          if (index.HasDomain)
          {
            AddDomains(element, count);
          }
          else
          {
            AddCount(null, element, count);
          }
        }
      }

      void CreateIntegerIndexKeys()
      {
        int? key = null;
        if (element.Value is not null)
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

          if (key is not null)
          {
            var row = new IntegerIndexRow(changes.Index, (int)key, Id);
            changes.FindOrCreateRow(row).Count += count;
          }
        }
      }

      void CreateIsoDateIndexKeys()
      {
        if (element.Value is not null && !string.IsNullOrEmpty(element.Value.ToString()))
        {
          var row = new IsoDateIndexRow(index, element.Value, Id);
          changes.FindOrCreateRow(row).Count += count;
        }
      }

      void CreateDateIndexKeys()
      {
        if (element.Value != null)
        {
          var row = new DateIndexRow(changes.Index, element.Value, Id);
          changes.FindOrCreateRow(row).Count += count;
        }
      }

      void CreateBooleanIndexKeys()
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

      void CreateAlphaNumericIndexKeys()
      {
        if (element.Value != null)
        {
          var row = new AlphaNumericIndexRow(changes.Index, indexTag, element.Value, Id);
          changes.FindOrCreateRow(row).Count += count;
        }
      }

      switch (changes.Index.Type)
      {
        case IndexTypeEnum.Text:
          CreateTextIndexKeys();
          break;

        case IndexTypeEnum.Integer:
          CreateIntegerIndexKeys();
          break;

        case IndexTypeEnum.FreeText:
          if (Database!.IsFullTextEnabled)
          {
            CreateTextIndexKeys();
          }
          else
          {
            await CreateFreeTextIndexKeysAsync();
          }
          break;

        case IndexTypeEnum.IsoDate:
          CreateIsoDateIndexKeys();
          break;

        case IndexTypeEnum.Date:
          CreateDateIndexKeys();
          break;

        case IndexTypeEnum.Boolean:
          CreateBooleanIndexKeys();
          break;

        case IndexTypeEnum.AlphaNumeric:
          CreateAlphaNumericIndexKeys();
          break;

        default:
          throw new DDException($"Index type '{changes.Index.Type}' for tag '{indexTag}' is not supported.");
      }
    }

    void CreateEnumIndexKeysForElement(object neutralValue)
    {
      var row = new TermIndexRow(changes.Index, indexTag, occ, neutralValue, Id);
      changes.FindOrCreateRow(row).Count += count;
    }

    foreach (var occurrence in occurrences)
    {
      if (occurrence.NeutralValue != null)
      {
        CreateEnumIndexKeysForElement(occurrence.NeutralValue);
      }
      else
      {
        foreach (var (language, element) in occurrence.Elements)
        {
          await CreateIndexKeysForElement(language, element);
        }
      }
      occ++;
    }
  }

  public override string ToString() => $"{Database} {Id}";

  public async Task ResolveLinksAsync(SqlStateInfo sqlState)
  {
    foreach (var (tag, occurrences) in Fields.Clone())
    {
      var fieldData = Database!.FindFieldByTagOrName(tag) ?? throw new FieldNotFoundException(tag, Database.Name);
      if (fieldData.IsLinked)
      {
        await ResolveLinkAsync(fieldData, occurrences, sqlState);
      }
    }
  }


  public async Task ProcessReverseLinksAsync(SqlStateInfo sqlState)
  {
    async Task ProcessReverseLinkAsync(DatabaseData linkedDatabase, string linkReverseTag,
                                          OccurrenceList linkIdOccurrences, SqlStateInfo sqlState)
    {
      async Task ProcessReverseLinkIdAsync(DatabaseData linkedDatabase, string linkReverseTag, int id, SqlStateInfo sqlState)
      {
        var record = await provider!.ReadRecordAsync(linkedDatabase, id, sqlState);
        if (record != null)
        {
          record.User = User;
          if (record.RepFind(linkReverseTag, Id) == 0)
          {
            record.Append(linkReverseTag, Id);
            await record.WriteAsync(sqlState);
          }
        }
      }

      foreach (var occurrence in linkIdOccurrences)
      {
        if (occurrence.Elements.TryGetValue("", out var element))
        {
          var linkId = element.IntValue;
          if (linkId.HasValue)
          {
            await ProcessReverseLinkIdAsync(linkedDatabase, linkReverseTag, linkId.Value, sqlState);
          }
        }
        // TODO: this should never happen
      }
    }

    sqlState.ProcessingReverseLinks = true;
    foreach (var (tag, occurrences) in Fields)
    {
      var fieldData = Database!.FindFieldByTagOrName(tag) ??
        throw new FieldNotFoundException(tag, Database.Name);
      if (!string.IsNullOrWhiteSpace(fieldData.LinkReverseTag) &&
           fieldData.LinkIdTag != null && fieldData.LinkedDatabase != null && fieldData.LinkReverseTag != null)
      {
        var linkIdOccurrences = Fields[fieldData.LinkIdTag];
        await ProcessReverseLinkAsync(fieldData.LinkedDatabase, fieldData.LinkReverseTag, linkIdOccurrences, sqlState);
      }
    }
    sqlState.ProcessingReverseLinks = false;
  }

  private async Task ResolveLinkAsync(FieldData fieldData, OccurrenceList occurrences, SqlStateInfo sqlState)
  {
    async Task ResolveLinkAsync(FieldData fieldData, Occurrence occurrence, int occ, SqlStateInfo sqlState)
    {
      foreach (var (language, element) in occurrence.Elements)
      {
        await this.ResolveLinkAsync(fieldData, language, element, occ, sqlState);
      }
    }

    int occ = 1;
    foreach (var occurrence in occurrences)
    {
      await ResolveLinkAsync(fieldData, occurrence, occ++, sqlState);
    }
  }

  private async Task ResolveLinkAsync(FieldData fieldData, string language, Element element, int occ, SqlStateInfo sqlState)
  {
    async Task<int> ResolveLinkAsync(FieldData linkFieldData, DatasetData? linkDataset,
                                         string? domain, string term, string language, SqlStateInfo sqlState)
    {
      /// <summary>
      ///  Add a domain to an existing linked existing record
      /// </summary>
      /// <param name="linkFieldData">The field for which to add the domain.</param>
      /// <param name="id">The record number of the linked record to add the domain to.</param>
      /// <param name="domain">The domain that we want to add.</param>
      /// <param name="cancellationToken">Cancellation token to abort the adding</param>
      /// <returns>nothing</returns>
      /// <exception cref="NullReferenceException">If the linked record could not be found,</exception>
      async Task AddDomainToLinkedRecord(int id, string domain)
      {
        var record = await provider!.ReadRecordAsync(linkFieldData.Database ??
          throw new NullReferenceException(nameof(linkFieldData.Database)), id, sqlState);
        if (record is null)
        {
          throw new NullReferenceException(nameof(record));
        }
        record.User = User;
        var domainTag = linkFieldData.GetDomainTag();
        if (record.RepFind(domainTag, domain) == 0)
        {
          var occ = record.RepCount(domainTag) + 1;
          record.Set(domainTag, occ, domain);
          await record.WriteAsync(sqlState);
        }
      }

      async Task<int> ForceLinkAsync(string? domain, string term, string language)
      {
        if (fieldData.ForcingAllowed)
        {
          var database = linkFieldData.Database ?? throw new NullReferenceException(nameof(linkFieldData.Database));
          var databaseName = database.Name ?? throw new NullReferenceException(nameof(database.Name));
          var record = new Record(provider!, Path.GetDirectoryName(database.FileName)!, databaseName, linkFieldData.LinkedDataset)
          {
            DefaultLanguage = DefaultLanguage,
            User = User
          };
          record.Set(linkFieldData, 1, language, term, true);

          if (domain is not null)
          {
            record.Set(linkFieldData.GetDomainTag(), domain);
          }

          await provider!.WriteRecordAsync(record, sqlState);
          return record.Id;
        }
        throw new ForcingIsNotAlllowedException(fieldData.Database.Name!, fieldData.Name!);
      }

      var index = linkFieldData.PreferredIndex ??
            throw new NullReferenceException(nameof(linkFieldData.PreferredIndex));
      var termValue = KeyConversions.TermValue(term, index.Length);
      var repository = provider!.Repository;
      var fullText = linkFieldData.Database.IsFullTextEnabled;
      string table = fullText ? linkFieldData.Database.FullTextTable : index.TableName;
      var tag = fullText ? linkFieldData.Tag : null;
      int linkId = 0;
      if (domain is not null)
      {
        // if there is first try to find the link with the domain
        linkId = await repository.FindLink(table, linkDataset, tag, domain, termValue, linkFieldData.IsMultiLingual, language, sqlState);
      }
      if (linkId is 0)
      {
        // link with domain not found, now try without a domain
        linkId = await repository.FindLink(table, linkDataset, tag, null, termValue, linkFieldData.IsMultiLingual, language, sqlState);
        if (linkId == 0)
        {
          // still not found, force it in if this is allowed
          linkId = await ForceLinkAsync(domain, term, language);
        }
        else
        {
          if (domain is not null)
          {
            await AddDomainToLinkedRecord(linkId, domain);
          }
        }
      }

      return linkId;
    }

    if (fieldData.LinkIdTag is null)
    {
      throw new NullReferenceException(nameof(fieldData.LinkIdTag));
    }

    async Task<string?> GetDomainAsync(FieldData fieldData)
     => !string.IsNullOrWhiteSpace(fieldData.LinkDomain) ? fieldData.LinkDomain :
        !string.IsNullOrEmpty(fieldData.LinkDomainTag) ? (await GetAsync(fieldData.LinkDomainTag, 1, "")) : null;


    var term = element.ToString();
    if (term != null)
    {
      var domain = await GetDomainAsync(fieldData);
      var linkedDatabase = fieldData.LinkedDatabase ??
        throw new NullReferenceException(nameof(fieldData.LinkedDatabase));
      var linkField = linkedDatabase.FindFieldByTagOrName(fieldData.LinkIndexTag!)!;
      var linkedDatasetName = fieldData.LinkedDataset;
      var linkDataset = string.IsNullOrEmpty(linkedDatasetName) ? null :
        linkedDatabase.FindDatasetByName(linkedDatasetName);

      var linkId = await ResolveLinkAsync(linkField, linkDataset, domain, term, language, sqlState);
      if (linkId == 0)
      {
        throw new DDException($"Cannot resolve link for field {fieldData.Name}, term = {term}, domain = {domain}");
      }
      Set(fieldData.LinkIdTag, occ, linkId);
    }
  }

  /// <summary>
  /// Write this record to the database (Async version)
  /// </summary>
  /// <returns>Nothing</returns>
  public Task WriteAsync(SqlStateInfo? sqlState = default, RecordWriteOptionsFlag? writeOptions = RecordWriteOptionsFlag.None) =>
     provider!.WriteRecordAsync(this, sqlState!, writeOptions);

  /// <summary>
  /// Write this record to the database (Async version)
  /// </summary>
  /// <returns>Nothing</returns>
  public Task WriteAsync(RecordWriteOptionsFlag? writeOptions = RecordWriteOptionsFlag.None, CancellationToken cancellationToken = default) =>
     provider!.WriteRecordAsync(this, new SqlStateInfo { CancellationToken = cancellationToken }, writeOptions);

  /// <summary>
  /// Write this record to the database (Async version)
  /// </summary>
  /// <returns>Nothing</returns>
  public Task WriteAsync(CancellationToken cancellationToken = default) =>
     provider!.WriteRecordAsync(this, new SqlStateInfo { CancellationToken = cancellationToken });

  /// <summary>
  /// Delete this record from the database
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns>Nothing</returns>
  public Task DeleteAsync(CancellationToken cancellationToken = default) =>
   provider!.DeleteRecordAsync(this, cancellationToken);

  /// <summary>
  /// Write this record to the database.
  /// </summary>
  public void Write()
  {
    var sqlState = SqlStateInfo.Default;
    var task = Task.Run(() => WriteAsync(sqlState));
    task?.Wait(sqlState.CancellationToken);
  }

  /// <summary>
  /// Write this record to the database.
  /// </summary>
  public void Write(SqlStateInfo sqlState)
  {
    var task = Task.Run(() => WriteAsync(sqlState));
    task?.Wait(sqlState.CancellationToken);
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

  public async Task<bool> LinkIdPresent(string tag, int value, SqlStateInfo sqlState)
  {
    var result = false;
    var maxOcc = RepCount(tag);
    for (int occ = 1; !result && occ <= maxOcc; occ++)
    {
      var id = await GetLinkIdAsync(tag, occ, sqlState);
      if (id.HasValue && id.Value == value)
      {
        result = true;
        break;
      }
    }
    return result;
  }

  public void Set(FieldData fieldData, object? value) => Set(fieldData, 1, "", value);

  public void Set(FieldData fieldData, int occ, object? value) => Set(fieldData, occ, "", value);

  public Task<string> Context(string fieldName, SqlStateInfo sqlState) => Context(fieldName, "/", sqlState);

  public Task<string> Context(string fieldName) => Context(fieldName, "/", SqlStateInfo.Default);

  public async Task<string> Context(string fieldName, string separator, SqlStateInfo sqlState)
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

    return await ContextInternal(Database, fieldData, parentIdTag, separator, sqlState);
  }

  private async Task<string> ContextInternal(DatabaseData database, FieldData fieldData, string parentIdTag, string separator,
                                             SqlStateInfo sqlState)
  {
    var result = await GetAsync(fieldData.Tag!, sqlState) ?? string.Empty;

    var parentId = await GetAsync(parentIdTag, 1, sqlState);
    if (parentId != null)
    {
      var parentRecord = await provider!.ReadRecordAsync(database, int.Parse(parentId), sqlState);
      if (parentRecord != null)
      {
        result = await parentRecord.ContextInternal(database, fieldData, parentIdTag, separator, sqlState) + separator + result;
      }
    }

    return result;
  }

  public async Task SetAutoNumberValue(FieldData fieldData, SqlStateInfo sqlState)
  => Set(fieldData, await provider!.GetAutoNumberValue(fieldData, sqlState));

  public FieldData GetFieldData(string tagOrFieldName)
  {
    if (Database is null)
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

  private Element? FindElement(string tagOrFieldName, int occ, string language = "")
  {
    Element? result = null;
    var fieldData = Database?.GetFieldByTagOrName(tagOrFieldName) ??
      throw new FieldNotFoundException(tagOrFieldName, Database?.Name);
    var tag = fieldData.Tag ?? throw new NullReferenceException(fieldData.Tag);
    if (Fields.TryGetValue(tag, out var occurrences))
    {
      if (occ >= 1 && occ <= occurrences.Count)
      {
        var occurrence = occurrences[occ - 1] ?? throw new InvalidOccurrenceException(occ);
        occurrence.Elements.TryGetValue(language, out result);
      }
    }
    return result;
  }

  /// <summary>
  /// Restores a previously modified version of the specified field occurrence and language.
  /// </summary>
  /// <param name="tagOrFieldName">Field name or tag</param>
  /// <param name="occ">Occurrence</param>
  /// <param name="language">Language</param>
  /// <exception cref="DDException"></exception>
  public void Undo(string tagOrFieldName, int occ = 1, string language = "")
  {
    var element = FindElement(tagOrFieldName, occ, language) ??
      throw new ElementNotFoundException(Database?.Name, tagOrFieldName, occ, language);
    element.Undo();
  }

  /// <summary>
  /// Redoes a previously undone modification of the specified field occurrence and language.
  /// </summary>
  /// <param name="tagOrFieldName">Field name or tag</param>
  /// <param name="occ">Occurrence</param>
  /// <param name="language">Language</param>
  /// <exception cref="DDException"></exception>
  public void Redo(string tagOrFieldName, int occ = 1, string language = "")
  {
    var element = FindElement(tagOrFieldName, occ, language) ??
       throw new ElementNotFoundException(Database?.Name, tagOrFieldName, occ, language);
    element.Redo();
  }

  public void ForceRelinks()
  {
    foreach (var (tag, occurrence) in Fields)
    {
      var fieldData = Database!.FindFieldByTagOrName(tag);
      if (fieldData is not null && (fieldData.IsLinked || fieldData.IsMergedField))
      {
        Fields[tag].Clear();
      }
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

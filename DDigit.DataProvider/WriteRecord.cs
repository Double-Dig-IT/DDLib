namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task WriteRecordAsync(Record record, SqlStateInfo sqlState, RecordWriteOptionsFlag? writeOptions = RecordWriteOptionsFlag.None)
  {
    var databaseData = record.Database ?? throw new NullReferenceException(nameof(record.Database));
    var table = record.Database.Name ?? throw new NullReferenceException(nameof(record.Database.Name));
    var fullTextTable = databaseData.IsFullTextEnabled ? databaseData.FullTextTable : null;

    var createdConnection = sqlState.Connection == null;
    var createdTransaction = sqlState.Transaction == null;

    sqlState.Connection ??= await Repository.GetDbConnectionAsync(record.Database);
    sqlState.Transaction ??= await Repository.StartTransactionAsync(sqlState.Connection);

    // This saves the exception in the try catch block and throws it later.
    ExceptionDispatchInfo? capturedException = null;

    async Task<int> WriteIndexDataAsync()
    {
      async Task<int> WriteChangesAsync(IndexRow indexRow)
         => indexRow.Count switch
         {
           > 0 => indexRow switch
           {
             IntegerIndexRow integerRow => await Repository.AddIndexKeyAsync(integerRow, sqlState),
             TermIndexRow termRow => await Repository.AddIndexKeyAsync(fullTextTable, termRow, sqlState),
             IsoDateIndexRow isoDateRow => await Repository.AddIndexKeyAsync(isoDateRow, sqlState),
             DateIndexRow dateRow => await Repository.AddIndexKeyAsync(dateRow, sqlState),
             BooleanIndexRow booleanRow => await Repository.AddIndexKeyAsync(booleanRow, sqlState),
             AlphaNumericIndexRow alphaNumericRow => await Repository.AddIndexKeyAsync(fullTextTable, alphaNumericRow, sqlState),
             _ => throw new NotImplementedException($"Unknown type {indexRow.GetType()} for data row"),
           },
           < 0 => indexRow switch
           {
             IntegerIndexRow integerRow => await Repository.DeleteIndexKeyAsync(integerRow, sqlState),
             TermIndexRow termRow => await Repository.DeleteIndexKeyAsync(fullTextTable, termRow, sqlState),
             IsoDateIndexRow isoDateRow => await Repository.DeleteIndexKeyAsync(isoDateRow, sqlState),
             DateIndexRow dateRow => await Repository.DeleteIndexKeyAsync(dateRow, sqlState),
             BooleanIndexRow booleanRow => await Repository.DeleteIndexKeyAsync(booleanRow, sqlState),
             AlphaNumericIndexRow alphaNumericRow => await Repository.DeleteIndexKeyAsync(fullTextTable, alphaNumericRow, sqlState),
             _ => throw new NotImplementedException($"Unknown type {indexRow.GetType()} for data row"),
           },
           _ => 0, // No changes to write
         };

      int result = 0;
     
      var indexChanges = await record.CreateIndexKeysAsync(sqlState);
      foreach (var index in indexChanges)
      {
        foreach (var change in index.Where(change => change.Count != 0))
        {
          IndexData indexData = change.Index;
          if (!indexData.TableExists.HasValue)
          {
            indexData.TableExists = Repository.CheckIndexTable(databaseData, indexData.TableName);
          }
          result += await WriteChangesAsync(change);
        }
      }
      return result;
    }

    async Task AutomaticNumbering()
    {
      foreach (var fieldData in record.Database!.AutomaticNumberingFields!)
      {
        if (fieldData.AutoNumberAssignmentSource == AutoNumberingAllowManualAssignmentEnum.Yes &&
               !string.IsNullOrWhiteSpace(await record.GetAsync(fieldData.Tag!, 1, sqlState)))
        {
          continue; // If the field is already set, skip auto-numbering
        }
        await record.SetAutoNumberValue(fieldData, sqlState);
      }
    }

    async Task ProcessInternalLinkAsync(InternalLinkData internalLink)
    {
      async Task CheckLinkAsync(string? tag, string? relatedTag)
      {
        if (record.Database is not null && tag is not null && relatedTag is not null)
        {
          int maxOcc = record.RepCount(tag);
          for (int occ = 1; occ <= maxOcc; occ++)
          {
            var relationId = await record.GetLinkIdAsync(tag, occ, sqlState);
            {
              if (relationId.HasValue)
              {
                var relatedRecord = await ReadRecordAsync(record.Database, relationId.Value, sqlState);
                if (relatedRecord is not null && !await relatedRecord.LinkIdPresent(relatedTag, record.Id, sqlState))
                {
                  relatedRecord.Append(relatedTag, record.Id);
                  await relatedRecord.WriteAsync(sqlState);
                }
              }
            }
          }
        }
      }

      async Task ProcessRelatedAsync(InternalLinkData internalLink)
      {
        var tag = internalLink.RelatedTermLinkIdTag;
        if (tag is not null)
        {
          int maxOcc = record.RepCount(tag);
          for (int occ = 1; occ <= maxOcc; occ++)
          {
            var relationId = await record.GetLinkIdAsync(tag, occ, sqlState);
          }
        }
      }

      async Task ProcessEquivalentAsync(InternalLinkData internalLink)
      {
        var tag = internalLink.EquivalentTermLinkIdTag;
        if (tag is not null)
        {
          int maxOcc = record.RepCount(tag);
          for (int occ = 1; occ <= maxOcc; occ++)
          {
            var relationId = await record.GetLinkIdAsync(tag, occ, sqlState);
          }
        }
      }

      async Task ProcessSemanticFactorAsync(InternalLinkData internalLink)
      {
        throw new NotImplementedException();
      }

      switch (internalLink.RelationType)
      {
        case RelationTypeEnum.Hierarchical:
          var broader = string.IsNullOrWhiteSpace(internalLink.BroaderTermLinkIdTag) ? internalLink.BroaderTermTag : internalLink.BroaderTermLinkIdTag;
          var narrower = string.IsNullOrWhiteSpace(internalLink.NarrowerTermLinkIdTag) ? internalLink.NarrowerTermTag : internalLink.NarrowerTermLinkIdTag;
          await CheckLinkAsync(broader, narrower);
          await CheckLinkAsync(narrower, broader);
          break;

        case RelationTypeEnum.Related:
          await ProcessRelatedAsync(internalLink);
          break;

        case RelationTypeEnum.Equivalence:
          await ProcessEquivalentAsync(internalLink);
          break;

        // Pesudonyms are treated in the same way as preferred / non-preferred
        case RelationTypeEnum.Pseudonym:
        case RelationTypeEnum.Preference:
          var use = string.IsNullOrWhiteSpace(internalLink.UseTermLinkIdTag) ? internalLink.UseTermTag : internalLink.UseTermLinkIdTag;
          var usedFor = string.IsNullOrWhiteSpace(internalLink.UsedForTermLinkIdTag) ? internalLink.UsedForTermTag : internalLink.UsedForTermLinkIdTag;
          await CheckLinkAsync(use, usedFor);
          await CheckLinkAsync(usedFor, use);
          break;

        case RelationTypeEnum.SemanticFactor:
          await ProcessSemanticFactorAsync(internalLink);
          break;

        default:
          throw new NotImplementedException();
      }
    }

    static void RunScript(Record record, ScriptTriggerCodeEnum triggerCode)
    {
      var script = record.Database?.BeforeStoragePythonScript;
      if (!string.IsNullOrWhiteSpace(script))
      {
        ScriptHelper.RunPython(script, record, triggerCode, Console.WriteLine);
        return;
      }

      script = record.Database?.BeforeStoragePowerShellScript;
      if (!string.IsNullOrWhiteSpace(script))
      {
        // TODO: Add trigger code and console output
        ScriptHelper.RunPowerShell(script, record);
      }
    }

    try
    {
      if ((writeOptions & RecordWriteOptionsFlag.NoAutoNumbering) == 0)
      {
        await AutomaticNumbering();
      }

      if ((writeOptions & RecordWriteOptionsFlag.NoScripts) == 0)
      {
        RunScript(record, ScriptTriggerCodeEnum.BeforeStorage);
      }

      if ((writeOptions & RecordWriteOptionsFlag.NoEditHistory) == 0)
      {
        RecordMetaData.SetInputEditMetaData(record);
      }

      await record.ResolveLinksAsync(sqlState);

      if (record.Id == 0)  // new record
      {
        record.Id = await Repository.GetNewRecordIdAsync(record.Database, record.Dataset, sqlState);
      }

      // Any records changes due to file uploads happen here,
      // But the resolving of ExternalResources gets executed after successfull database write
      var externalResources = (writeOptions & RecordWriteOptionsFlag.NoFilesUpload) == 0 ?
        await ResolveExternalResources.ResolveAsync(record, sqlState) : [];

      var data = record.Serialize().ToString();  // BDD: it's easier to keep a variable here for debugging purposes 
      record.Original = await ReadRecordAsync(record.Database, record.Id, sqlState);
      if (record.Original is null)
      {
        await Repository.WriteNewDataAsync(table, record.Id, record.Creation, record.Modification, data, sqlState);
      }
      else
      {
        await Repository.UpdateDataAsync(table, record.Id, record.Modification, data, sqlState);
      }

      var rows = await WriteIndexDataAsync();

      //rows = await WriteIndexedLinksDataAsync(currentConnection, currentTransaction, record, cancellationToken);

      // This was temporarily turned off; causes large slowdown on records with many internal links
      // re-enabled the processing of internal links on 29/07/2025, this is needed for hierarchical links
      foreach (var internalLink in databaseData.InternalLinks)
      {
        await ProcessInternalLinkAsync(internalLink);
      }

      if (!sqlState.ProcessingReverseLinks)
      {
        await record.ProcessReverseLinksAsync(sqlState);
      }

      foreach (var externalResource in externalResources)
      {
        await externalResource.ResolveAsync();
      }

      if (createdTransaction)
      {
        await Repository.CommitAsync(sqlState);
      }

      if (createdConnection)
      {
        var sqlConnection = (SqlConnection)sqlState.Connection;
        await sqlConnection.CloseAsync();
        await sqlConnection.DisposeAsync();
        sqlState.Connection = null;
      }

      RunScript(record, ScriptTriggerCodeEnum.AfterStorage);
    }
    catch (Exception ex)
    {
      if (createdTransaction)
      {
        Debug.WriteLine(ex.ToString());
        await Repository.RollbackAsync(sqlState);
      }

      if (createdConnection && sqlState.Connection != null)
      {
        var sqlConnection = (SqlConnection)sqlState.Connection!;
        await sqlConnection.CloseAsync();
        await sqlConnection.DisposeAsync();
        sqlState.Connection = null;
      }

      // Save the exception to be thrown later
      capturedException = ExceptionDispatchInfo.Capture(ex);
    }

    record.ForceRelinks();

    // If an exception was captured, rethrow it
    capturedException?.Throw();
  }

  private async Task<int> WriteIndexedLinksDataAsync(Record record, SqlStateInfo sqlState)
  {
    async Task<int> WriteIndexedLink(Record record, IndexedLinkData indexedLink, SqlStateInfo sqlState)
    {
      throw new NotImplementedException("WriteIndexedLink");
    }

    int rows = 0;
    var databaseData = record.Database ?? throw new NullReferenceException(nameof(record.Database));
    foreach (var indexedLink in databaseData.IndexedLinks)
    {
      rows += await WriteIndexedLink(record, indexedLink, sqlState);
    }
    return rows;
  }
}



using DDigit.ScriptingLibrary;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task WriteRecordAsync(Record record,
                                     IDbConnection? connection = null,
                                     IDbTransaction? transaction = null,
                                     CancellationToken cancellationToken = default)
  {
    var databaseData = record.Database ?? throw new NullReferenceException(nameof(record.Database));
    var table = record.Database.Name ?? throw new NullReferenceException(nameof(record.Database.Name));
    var fullTextTable = databaseData.FullText ? databaseData.FullTextTable : null;
    var currentConnection = connection ?? await Repository.GetDbConnectionAsync(record.Database);
    var currentTransaction = transaction ?? await Repository.StartTransactionAsync(currentConnection);

    // This saves the exception in the try catch block and throws it later.
    ExceptionDispatchInfo? capturedException = null;

    try
    {
      await AutomaticNumbering(currentConnection, currentTransaction, record, cancellationToken);
      RunScript(record, ScriptTriggerCodeEnum.BeforeStorage);

      RecordMetaData.SetInputEditMetaData(record);

      await record.ResolveLinksAsync(currentConnection, currentTransaction, cancellationToken);
      if (record.Id == 0)
      {
        record.Id = await Repository.GetNewRecordIdAsync(currentConnection, currentTransaction, record.Database, record.Dataset);
        record.Original = null;

        var data = record.Serialize().ToString();  // BDD: it's easier to keep a variable here for debugging purposes 

        await Repository.WriteNewDataAsync(table, record.Id, record.Creation,
                                             record.Modification, data,
                                             currentConnection, currentTransaction, cancellationToken);

      }
      else
      {
        record.Original = await ReadRecordAsync(record.Database, record.Id,
                                                currentConnection, currentTransaction, cancellationToken);
        await Repository.UpdateDataAsync(currentConnection, currentTransaction, table, record.Id,
                                    record.Modification, record.Serialize().ToString(), cancellationToken);
      }

      var rows = await WriteIndexDataAsync(currentConnection, currentTransaction, fullTextTable, record, cancellationToken);

      //rows = await WriteIndexedLinksDataAsync(currentConnection, currentTransaction, record, cancellationToken);

      // This was temporarily turned off; causes large slowdown on records with many internal links
      // re-enabled the processing of internal links on 29/07/2025, this is needed for hierarchical links
      foreach (var internalLink in databaseData.InternalLinks)
      {
        await ProcessInternalLinkAsync(currentConnection, currentTransaction, internalLink, record, cancellationToken);
      }

      await record.ProcessReverseLinksAsync(currentConnection, currentTransaction, cancellationToken);

      if (transaction == null)
      {
        await Repository.CommitAsync(currentTransaction, cancellationToken);
      }
      if (connection == null)
      {
        currentConnection?.Dispose();
      }

      RunScript(record, ScriptTriggerCodeEnum.AfterStorage);
    }
    catch (Exception ex)
    {
      if (transaction == null)
      {
        Debug.WriteLine(ex.ToString());
        await Repository.RollbackAsync(currentTransaction, cancellationToken);
      }
      if (connection != null)
      {
        currentConnection?.Dispose();
      }

      // Save the exception to be thrown later
      capturedException = ExceptionDispatchInfo.Capture(ex);
    }

    // If an exception was captured, rethrow it
    capturedException?.Throw();
  }

  private async static Task AutomaticNumbering(IDbConnection connection, IDbTransaction transaction,
                                         Record record, CancellationToken cancellationToken)
  {
    foreach (var fieldData in record.Database!.AutomaticNumberingFields)
    {
      if (fieldData.AutoNumberAssignment == AutoNumberAssignmentEnum.BeforeStorage ||
          fieldData.AutoNumberAssignment == AutoNumberAssignmentEnum.DuringInputOrEdit)
      {
        if (fieldData.AutoNumberAssignmentSource == AutoNumberAssignmentSourceEnum.AllowManual &&
               !string.IsNullOrWhiteSpace(await record.GetAsync(fieldData.Tag!, 1, cancellationToken:cancellationToken)))
        { 
          continue; // If the field is already set, skip auto-numbering
        }
        await record.SetAutoNumberValue(connection, transaction, fieldData, cancellationToken);
      }
    }
  }

  private async Task<int> WriteIndexedLinksDataAsync(IDbConnection currentConnection, IDbTransaction currentTransaction, Record record, CancellationToken cancellationToken)
  {
    int rows = 0;
    var databaseData = record.Database ?? throw new NullReferenceException(nameof(record.Database));
    foreach (var indexedLink in databaseData.IndexedLinks)
    {
      rows += await WriteIndexedLink(indexedLink, currentConnection, currentTransaction, record, cancellationToken);
    }
    return rows;
  }

  private async Task<int> WriteIndexedLink(IndexedLinkData indexedLink, IDbConnection currentConnection, IDbTransaction currentTransaction,
    Record record, CancellationToken cancellationToken)
  {
    throw new NotImplementedException("WriteIndexedLink");
  }

  private async Task ProcessInternalLinkAsync(IDbConnection connection, IDbTransaction transaction,
                                                     InternalLinkData internalLink, Record record,
                                                     CancellationToken cancellationToken)
  {
    switch (internalLink.RelationType)
    {
      case RelationTypeEnum.Hierarchical:
        var broader = string.IsNullOrWhiteSpace(internalLink.BroaderTermLinkIdTag) ? internalLink.BroaderTermTag : internalLink.BroaderTermLinkIdTag;
        var narrower = string.IsNullOrWhiteSpace(internalLink.NarrowerTermLinkIdTag) ? internalLink.NarrowerTermTag : internalLink.NarrowerTermLinkIdTag;
        await CheckLinkAsync(record, broader, narrower, connection, transaction, cancellationToken);
        await CheckLinkAsync(record, narrower, broader, connection, transaction, cancellationToken);
        break;

      case RelationTypeEnum.Related:
        await ProcessRelatedAsync(record, internalLink, connection, transaction, cancellationToken);
        break;

      case RelationTypeEnum.Equivalence:
        await ProcessEquivalentAsync(record, internalLink, connection, transaction, cancellationToken);
        break;

      case RelationTypeEnum.Preference:
        var use = string.IsNullOrWhiteSpace(internalLink.UseTermLinkIdTag) ? internalLink.UseTermTag : internalLink.UseTermLinkIdTag;
        var usedFor = string.IsNullOrWhiteSpace(internalLink.UsedForTermLinkIdTag) ? internalLink.UsedForTermTag : internalLink.UsedForTermLinkIdTag;
        await CheckLinkAsync(record, use, usedFor, connection, transaction, cancellationToken);
        await CheckLinkAsync(record, usedFor, use, connection, transaction, cancellationToken);
        break;

      case RelationTypeEnum.Pseudonym:
        await ProcessPseudonymAsync(record, internalLink, connection, transaction, cancellationToken);
        break;

      case RelationTypeEnum.SemanticFactor:
        await ProcessSemanticFactorAsync(record, internalLink, connection, transaction, cancellationToken);
        throw new NotImplementedException();

      default:
        throw new NotImplementedException();
    }
  }

  private async Task ProcessSemanticFactorAsync(Record record, InternalLinkData internalLink, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    return;
  }

  private async Task ProcessPseudonymAsync(Record record, InternalLinkData internalLink, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    return;
  }

  private static async Task ProcessPreferenceAsync(Record record, InternalLinkData internalLink,
                                                   IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var tag = internalLink.UseTermLinkIdTag;
    if (tag != null)
    {
      int maxOcc = record.RepCount(tag);
      for (int occ = 1; occ <= maxOcc; occ++)
      {
        var relationId = await record.GetLinkIdAsync(tag, occ, connection, transaction, cancellationToken);
      }
    }

    tag = internalLink.UsedForTermLinkIdTag;
    if (tag != null)
    {
      int maxOcc = record.RepCount(tag);
      for (int occ = 1; occ <= maxOcc; occ++)
      {
        var relationId = await record.GetLinkIdAsync(tag, occ, connection, transaction, cancellationToken);
      }
    }
  }

  private static async Task ProcessEquivalentAsync(Record record, InternalLinkData internalLink,
                                                   IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var tag = internalLink.EquivalentTermLinkIdTag;
    if (tag != null)
    {
      int maxOcc = record.RepCount(tag);
      for (int occ = 1; occ <= maxOcc; occ++)
      {
        var relationId = await record.GetLinkIdAsync(tag, occ, connection, transaction, cancellationToken);
      }
    }
  }

  private static async Task ProcessRelatedAsync(Record record, InternalLinkData internalLink,
                                                IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var tag = internalLink.RelatedTermLinkIdTag;
    if (tag != null)
    {
      int maxOcc = record.RepCount(tag);
      for (int occ = 1; occ <= maxOcc; occ++)
      {
        var relationId = await record.GetLinkIdAsync(tag, occ, connection, transaction, cancellationToken);
      }
    }
  }

  private async Task CheckLinkAsync(Record record, string? tag, string? relatedTag,
                                    IDbConnection connection, IDbTransaction transaction,
                                    CancellationToken cancellationToken)
  {
    if (record.Database != null && tag != null && relatedTag != null)
    {
      int maxOcc = record.RepCount(tag);
      for (int occ = 1; occ <= maxOcc; occ++)
      {
        var relationId = await record.GetLinkIdAsync(tag, occ, connection, transaction, cancellationToken);
        {
          if (relationId.HasValue)
          {
            var relatedRecord = await ReadRecordAsync(record.Database, relationId.Value,
                                                      connection, transaction, cancellationToken);
            if (relatedRecord != null && !await relatedRecord.LinkIdPresent(relatedTag, record.Id,
                                                        connection, transaction, cancellationToken))
            {
              relatedRecord.Append(relatedTag, record.Id);
              await relatedRecord.WriteAsync(connection, transaction, cancellationToken);
            }
          }
        }
      }
    }
  }

  private static void RunScript(Record record, ScriptTriggerCodeEnum triggerCode)
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

  private static string GetExtensionPath(DatabaseData database, string adaplPath, string extension)
    => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(database.PhysicalPath)!, adaplPath + extension));


  private async Task<int> WriteIndexDataAsyncParallel(IDbConnection connection, IDbTransaction transaction,
                                         string? fullTextTable, Record record, CancellationToken cancellationToken)
  {
    var tasks = new List<Task<int>>();
    var indexChanges = await record.CreateIndexKeysAsync(connection, transaction, cancellationToken);
    foreach (var index in indexChanges)
    {
      foreach (var change in index.Where(change => change.Count != 0))
      {
        tasks.Add(
           WriteChangesAsync(connection, transaction, fullTextTable, change, cancellationToken)
          );
      }
    }
    var numbers = await Task.WhenAll(tasks);
    return numbers.Sum();
  }
  private async Task<int> WriteIndexDataAsync(IDbConnection connection, IDbTransaction transaction,
                                         string? fullTextTable, Record record, CancellationToken cancellationToken)
  {
    int result = 0;
    var indexChanges = await record.CreateIndexKeysAsync(connection, transaction, cancellationToken);
    foreach (var index in indexChanges)
    {
      foreach (var change in index.Where(change => change.Count != 0))
      {
        result += await WriteChangesAsync(connection, transaction, fullTextTable, change, cancellationToken);
      }
    }
    return result;
  }

  private async Task<int> WriteChangesAsync(IDbConnection connection, IDbTransaction transaction, string? fullTextTable,
                                       IndexRow index, CancellationToken cancellationToken)
  {
    if (index is IntegerIndexRow integerRow)
    {
      if (index.Count > 0)
      {
        return await Repository.AddIndexKeyAsync(connection, transaction, integerRow, cancellationToken);
      }
      else if (index.Count < 0)
      {
        return await Repository.DeleteIndexKeyAsync(connection, transaction, integerRow, cancellationToken);
      }
    }

    if (index is TermIndexRow termRow)
    {
      if (index.Count > 0)
      {
        return await Repository.AddIndexKeyAsync(connection, transaction, fullTextTable, termRow, cancellationToken);
      }
      else if (index.Count < 0)
      {
        return await Repository.DeleteIndexKeyAsync(connection, transaction, fullTextTable, termRow, cancellationToken);
      }
    }

    if (index is DateIndexRow dateRow)
    {
      if (index.Count > 0)
      {
        return await Repository.AddIndexKeyAsync(connection, transaction, dateRow, cancellationToken);
      }
      else if (index.Count < 0)
      {
        return await Repository.DeleteIndexKeyAsync(connection, transaction, dateRow, cancellationToken);
      }
    }

    if (index is IsoDateIndexRow isoDateRow)
    {
      if (index.Count > 0)
      {
        return await Repository.AddIndexKeyAsync(connection, transaction, isoDateRow, cancellationToken);
      }
      else if (index.Count < 0)
      {
        return await Repository.DeleteIndexKeyAsync(connection, transaction, isoDateRow, cancellationToken);
      }
    }

    if (index is BooleanIndexRow booleanRow)
    {
      if (index.Count > 0)
      {
        return await Repository.AddIndexKeyAsync(connection, transaction, booleanRow, cancellationToken);
      }
      else if (index.Count < 0)
      {
        return await Repository.DeleteIndexKeyAsync(connection, transaction, booleanRow, cancellationToken);
      }
    }

    if (index is AlphaNumericIndexRow alphaNumericRow)
    {
      if (index.Count > 0)
      {
        return await Repository.AddIndexKey(connection, transaction, fullTextTable, alphaNumericRow, cancellationToken);
      }
      else if (index.Count < 0)
      {
        return await Repository.DeleteIndexKey(connection, transaction, fullTextTable, alphaNumericRow, cancellationToken);
      }
    }

    throw new NotImplementedException($"Unknown type {index.GetType()} for data row");
  }
}



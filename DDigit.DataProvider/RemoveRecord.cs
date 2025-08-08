using Microsoft.Data.SqlClient;

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task RemoveRecord(string folder, string databaseName, int id, CancellationToken cancellationToken)
  {
    var database = MetaDataCache.ReadDatabase(folder, databaseName, false) ??
      throw new DatabaseNotFoundException(folder, databaseName);

    await DeleteRecord(database, id, cancellationToken);
  }

  private async Task DeleteRecord(DatabaseData database, int id, CancellationToken cancellationToken)
  {
    using var connection = await Repository.GetDbConnectionAsync(database) as SqlConnection ?? throw new NullReferenceException("connection");
    var transaction = await Repository.StartTransactionAsync(connection) as SqlTransaction ?? throw new NullReferenceException("transaction");
    try
    {
      await DeleteLinksToRecord(connection, transaction, database, id, cancellationToken);
      await DeleteInternalLinks(connection, transaction, database, id, cancellationToken);
      await DeleteIndexKeys(connection, transaction, database, id, cancellationToken);
      await transaction.CommitAsync(cancellationToken);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }

  private async Task DeleteInternalLinks(IDbConnection connection, IDbTransaction transaction, DatabaseData database, int id, CancellationToken cancellationToken)
  {
    foreach (var link in database.InternalLinks)
    {
      switch (link.RelationType)
      {
        case RelationTypeEnum.Hierarchical:
          await DeleteInternalLinks(connection, transaction, database, link.BroaderTermTag, link.BroaderTermLinkIdTag, id, cancellationToken);
          await DeleteInternalLinks(connection, transaction, database, link.NarrowerTermTag, link.NarrowerTermLinkIdTag, id, cancellationToken);
          break;

        case RelationTypeEnum.Related:
          await DeleteInternalLinks(connection, transaction, database, link.RelatedTermTag, link.RelatedTermLinkIdTag, id, cancellationToken);
          break;

        case RelationTypeEnum.Preference:
          await DeleteInternalLinks(connection, transaction, database, link.UsedForTermTag, link.UsedForTermLinkIdTag, id, cancellationToken);
          await DeleteInternalLinks(connection, transaction, database, link.UseTermTag, link.UseTermLinkIdTag, id, cancellationToken);
          break;

        default:
          throw new NotImplementedException();
      }

    }
  }

  private async Task DeleteInternalLinks(IDbConnection connection, IDbTransaction transaction, DatabaseData database, string? linkTag, string? linkIdTag, int id, CancellationToken cancellationToken)
  {
    var linkField = database.FindFieldByTagOrName(linkTag ?? throw new NullReferenceException(nameof(linkTag))) ?? throw new FieldNotFoundException(linkTag, database.Name);
    if (!string.IsNullOrWhiteSpace(linkField.LinkIdTag))
    {
      if (string.IsNullOrWhiteSpace(linkIdTag) && linkField.LinkIdTag != linkField.LinkIdTag)
      {
        throw new DDException("Internal link Configuration error");
      }
      linkIdTag = linkField.LinkIdTag;
    }
    if (string.IsNullOrWhiteSpace(linkIdTag))
    {
      throw new DDException("Internal links without linkId field are not supported (yet)");
    }

    var _ = database.GetFieldByTagOrName(linkIdTag) ?? throw new FieldNotFoundException(linkIdTag, database.Name);
    await DeleteLinkToField(connection, transaction, database, linkField, id, cancellationToken);
  }

  private async Task DeleteLinksToRecord(IDbConnection connection, IDbTransaction transaction, DatabaseData database,
    int id, CancellationToken cancellationToken)
  {
    foreach (var (linkedDatabase, fieldList) in database.UpdateLinks)
    {
      foreach (var field in fieldList)
      {
        await DeleteLinkToField(connection, transaction, linkedDatabase, field, id, cancellationToken);
      }
    }
  }

  private async Task DeleteLinkToField(IDbConnection connection, IDbTransaction transaction, DatabaseData linkedDatabase, FieldData field,
    int id, CancellationToken cancellationToken)
  {
    if (field.IndexList != null && field.IndexList.Count > 0)
    {
      var linkedRecordsIds = await Repository.ReadLinkedRecordIdsAsync(connection, transaction, field.IndexList[0].TableName, id, cancellationToken);
      foreach (var recordId in linkedRecordsIds)
      {
        if (field.LinkIdTag != null)
        {
          await DeleteLinkFromRecord(linkedDatabase, field.LinkIdTag, recordId, id, connection, transaction, cancellationToken);
        }
      }
    }
  }

  private async Task DeleteLinkFromRecord(DatabaseData database, string tag, int linkedRecordId, int id,
                                          IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
  {
    var linkedRecord = await ReadRecordAsync(database, linkedRecordId, connection, transaction, cancellationToken);
    if (linkedRecord != null)
    {
      var occ = linkedRecord.RepFind(tag, id);
      if (occ > 0)
      {
        linkedRecord.Delete(tag, 1);
      }
      await linkedRecord.WriteAsync(connection, transaction, cancellationToken);
    }
  }

  private async Task<int> DeleteIndexKeys(IDbConnection connection, IDbTransaction transaction, DatabaseData database,
                                     int id, CancellationToken cancellationToken)
  {
    var fullTextDeleted = false;
    var count = 0;
    foreach (var index in database.Indexes)
    {
      if (index.Name != "wordlist")
      {
        var isText = index.Type == IndexTypeEnum.Text || index.Type == IndexTypeEnum.FreeText;
        if (!isText || database.FullText == false || (database.FullText && fullTextDeleted == false))
        {
          var tableName = isText && database.FullText ? database.FullTextTable : index.TableName;
          count += await Repository.DeleteIndexKeysAsync(connection, transaction, tableName, id, cancellationToken);
          if (isText && database.FullText)
          {
            fullTextDeleted = true;
          }
        }
      }
    }
    return count;
  }
}

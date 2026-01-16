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
        async Task<int> DeleteIndexKeys(DatabaseData database, int id, SqlStateInfo sqlState)
        {
            var fullTextDeleted = false;
            var count = 0;
            foreach (var index in database.Indexes)
            {
                if (index.Name != "wordlist")
                {
                    var isText = index.Type == IndexTypeEnum.Text || index.Type == IndexTypeEnum.FreeText;
                    if (!isText || database.IsFullTextEnabled == false || (database.IsFullTextEnabled && fullTextDeleted == false))
                    {
                        var tableName = isText && database.IsFullTextEnabled ? database.FullTextTable : index.TableName;
                        count += await Repository.DeleteIndexKeysAsync(tableName, id, sqlState);
                        if (isText && database.IsFullTextEnabled)
                        {
                            fullTextDeleted = true;
                        }
                    }
                }
            }
            return count;
        }

        var connection = await Repository.GetDbConnectionAsync(database) as SqlConnection ?? throw new NullReferenceException("connection");
        var transaction = await Repository.StartTransactionAsync(connection) as SqlTransaction ?? throw new NullReferenceException("transaction");

        var sqlState = new SqlStateInfo
        {
            Connection = connection,
            Transaction = transaction,
            CancellationToken = cancellationToken
        };

        try
        {
            await DeleteLinksToRecord(database, id, sqlState);
            await DeleteInternalLinks(database, id, sqlState);
            await DeleteIndexKeys(database, id, sqlState);
            await transaction.CommitAsync(sqlState.CancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (sqlState.Connection != null && sqlState.Connection.State == ConnectionState.Open)
            {
                sqlState.Connection.Close();
                sqlState.Connection.Dispose();
            }
        }
    }

    private async Task DeleteInternalLinks(DatabaseData database, int id, SqlStateInfo sqlState)
    {
        foreach (var link in database.InternalLinks)
        {
            switch (link.RelationType)
            {
                case RelationTypeEnum.Hierarchical:
                    await DeleteInternalLinks(database, link.BroaderTermTag, link.BroaderTermLinkIdTag, id, sqlState);
                    await DeleteInternalLinks(database, link.NarrowerTermTag, link.NarrowerTermLinkIdTag, id, sqlState);
                    break;

                case RelationTypeEnum.Related:
                    await DeleteInternalLinks(database, link.RelatedTermTag, link.RelatedTermLinkIdTag, id, sqlState);
                    break;

                case RelationTypeEnum.Preference:
                    await DeleteInternalLinks(database, link.UsedForTermTag, link.UsedForTermLinkIdTag, id, sqlState);
                    await DeleteInternalLinks(database, link.UseTermTag, link.UseTermLinkIdTag, id, sqlState);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    private async Task DeleteInternalLinks(DatabaseData database, string? linkTag, string? linkIdTag, int id, SqlStateInfo sqlState)
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
        await DeleteLinkToField(database, linkField, id, sqlState);
    }

    private async Task DeleteLinksToRecord(DatabaseData database, int id, SqlStateInfo sqlState)
    {
        foreach (var (linkedDatabase, fieldList) in database.UpdateLinks)
        {
            foreach (var field in fieldList)
            {
                await DeleteLinkToField(linkedDatabase, field, id, sqlState);
            }
        }
    }

    private async Task DeleteLinkToField(DatabaseData linkedDatabase, FieldData field, int id, SqlStateInfo sqlState)
    {
        async Task DeleteLinkFromRecord(DatabaseData database, string tag, int linkedRecordId, int id, SqlStateInfo sqlState)
        {
            var linkedRecord = await ReadRecordAsync(database, linkedRecordId, sqlState);
            if (linkedRecord != null)
            {
                var occ = linkedRecord.RepFind(tag, id);
                if (occ > 0)
                {
                    linkedRecord.Delete(tag, 1);
                }
                await linkedRecord.WriteAsync(sqlState);
            }
        }

        if (field.IndexList is not null && field.IndexList.Count > 0)
        {
            var linkedRecordsIds = await Repository.ReadLinkedRecordIdsAsync(field.IndexList[0].TableName, id, sqlState);
            foreach (var recordId in linkedRecordsIds)
            {
                if (field.LinkIdTag is not null)
                {
                    await DeleteLinkFromRecord(linkedDatabase, field.LinkIdTag, recordId, id, sqlState);
                }
            }
        }
    }

}

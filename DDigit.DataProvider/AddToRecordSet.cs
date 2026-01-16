namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task AddToRecordSetAsync(RecordSet recordSet, IEnumerable<int> ids, 
                                        CancellationToken cancellationToken)
  {
    var connection = await Repository.GetDbConnectionAsync(recordSet.Database);
    var transaction = await Repository.StartTransactionAsync(connection);

    var sqlState = new SqlStateInfo
    {
      CancellationToken = cancellationToken,
      Connection = connection,
      Transaction = transaction
    };

    try
    {
      foreach (var id in ids)
      {
        if (!recordSet.Set.Ids.Contains(id))
        {
          await Repository.AddToRecordSetAsync(recordSet, id, sqlState);
          recordSet.Set.Ids.Add(id);
        }
      }
      recordSet.MetaData.Hits = recordSet.Set.Hits = recordSet.Set.Ids.Count;
      await Repository.WriteRecordSetMetaDataAsync(recordSet.MetaData, sqlState);
      await Repository.CommitAsync(sqlState);
    }
    catch (Exception ex)
    {
      await Repository.RollbackAsync(sqlState);
      throw new DDException($"Failed to add records to record set {recordSet.MetaData.Title}: {ex.Message}", ex);
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

  public async Task RemoveFromRecordSetAsync(RecordSet recordSet, IEnumerable<int> ids, CancellationToken cancellationToken)
  {
    var connection = await Repository.GetDbConnectionAsync(recordSet.Database);
    var transaction = await Repository.StartTransactionAsync(connection);

    var sqlState = new SqlStateInfo
    {
      CancellationToken = cancellationToken,
      Connection = connection,
      Transaction = transaction
    };

    try
    {
      foreach (var id in ids)
      {
        if (recordSet.Set.Ids.Contains(id))
        {
          await Repository.RemoveFromRecordSetAsync(recordSet, id, sqlState);
          recordSet.Set.Ids.Remove(id);
        }
      }
      recordSet.MetaData.Hits = recordSet.Set.Hits = recordSet.Set.Ids.Count;
      await Repository.WriteRecordSetMetaDataAsync(recordSet.MetaData, sqlState);
      await Repository.CommitAsync(sqlState);
    }
    catch (Exception ex)
    {
      await Repository.RollbackAsync(sqlState);
      throw new DDException($"Failed to remove records from record set {recordSet.MetaData.Title}: {ex.Message}", ex);
    }
    finally
    {
      if (sqlState.Connection != null && sqlState.Connection.State == ConnectionState.Open)
      {
        connection.Close();
        connection.Dispose();
      }
    }
  }
}

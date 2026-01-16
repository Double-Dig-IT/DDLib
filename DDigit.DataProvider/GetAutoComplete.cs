namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<AutoCompleteResult?> GetAutoComplete(string folder, string database, IEnumerable<string>? datasets,
                                                         string[] fields, string? value, int? startFrom, int? limit, string? language, bool count,
                                                         CancellationToken cancellationToken)
  {
    try
    {
      var databaseData = MetaDataCache.ReadDatabase(folder, database, false);

      if (databaseData == null)
      {
        return null;
      }

      var datasetFilter = datasets != null ? new DatasetFilter(databaseData, datasets) : null;
      var fieldData = fields.Select(field => databaseData.FindFieldByTagOrName(field) ?? throw new FieldNotFoundException(field, database));

      return await Repository.GetAutoCompleteAsync(fieldData, datasetFilter, value, startFrom, limit, language, count, cancellationToken);
    }
    catch (TaskCanceledException)
    {
      throw;
    }
    catch (Exception ex) when (ex is InvalidOperationException || ex is SqlException)
    {
      // Todo: Improve this
      // These can also caused by the cancellation of a task,
      // But this should be determined more explicitly
      // Git Issue #9 in DDLib-development
      throw new TaskCanceledException();
    }
    catch (Exception ex)
    {
      throw new AutoCompleteException(folder, database, fields, value, ex);
    }
  }
}

namespace DDigit.DataProvider;

public partial class DDataProvider
{
  public async Task<List<Record>> SearchLinksAsync(DatabaseData database, string fieldName, string searchValue, 
    int startFrom, int limit, Record? record = null, int occ = 1)
  {
    var field = database.FindFieldByTagOrName(fieldName) ?? throw new FieldNotFoundException(fieldName, database.Name);
    if (!field.IsLinked)
    {
      throw new DDException($"Field '{fieldName}' from '{database.Name}'");
    }
    var linkedDatabase = field.LinkedDatabase ?? throw new DDException($"Field '{fieldName}' from '{database.Name}' has no linked database");
    var linkedDatasetName = field.LinkedDataset;
    DatasetData? linkedDataset = null;
    if (!string.IsNullOrEmpty(linkedDatasetName))
    {
      linkedDataset = linkedDatabase.FindDatasetByName(linkedDatasetName) ??
        throw new DDException($"Field '{fieldName}' from '{database.Name}' has a linked dataset '{linkedDatasetName}' that does not exist in the linked database '{linkedDatabase.Name}'.");
    }
    var limits = new SearchLimits(startFrom, limit); // Default start from the first record, limit to 100 records
    var domain = field.LinkDomain; // Pick up the right domain for the search, if any

    if (string.IsNullOrEmpty(domain))
    {
      if (record == null)
      {
        throw new DDException($"Field '{fieldName}' from '{database.Name}' requires a record to search links, but no record was provided.");
      }
      if (string.IsNullOrEmpty(field.LinkDomainTag))
      {
        throw new DDException($"Field '{fieldName}' from '{database.Name}' requires a link  domain tag, but none was provided.");
      }
      domain = record.Get(field.LinkDomainTag, occ, null, null, default);
      if (string.IsNullOrEmpty(domain))
      {
        throw new DDException($"Field '{fieldName}' from '{database.Name}' has no value for the link domain tag '{field.LinkDomainTag}' in the provided record.");
      } 
    }

    var ids = await Repository.SearchLinksAsync(linkedDatabase, linkedDataset, field, searchValue, domain, limits);
    return await ReadRecords(field.LinkedDatabase, ids);
  }
}

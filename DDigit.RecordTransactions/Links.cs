namespace DDigit.RecordTransactions;

public class Links : DatabaseAccess
{
  /// <summary>
  /// Search for linked records in a database.
  /// </summary>
  /// <param name="folder">The folder in which the inf file is stored</param>
  /// <param name="database">Then name of the database</param>
  /// <param name="fieldName">The field for which the links needs to be searched</param>
  /// <param name="searchValue">Search value</param>
  /// <param name="startFrom">Where to start in the result set</param>
  /// <param name="limit">How many records to return</param>
  /// <param name="record">Optional: the current record if a dynamic domain is used, the link definition determines which field is used as a domain value.</param>
  /// <param name="occ">Optional: the current occurrence if a dynamic domain is used, the link definition determines which field is used as a domain value.</param>
  /// <returns></returns>
  /// <exception cref="DatabaseNotFoundException"></exception>
  public static async Task<List<Record>> SearchLinks(string folder, string database, string fieldName, string searchValue,
                                                     int startFrom = 1, int limit = 25, Record? record = null, int occ = 1)
  {
    var databaseData = OpenDatabase(folder, database) ?? throw new DatabaseNotFoundException(folder, database);
    var dataProvider = new DDataProvider(new MSSqlRepository());
    var sqlState = new SqlStateInfo();
    return await dataProvider.SearchLinksAsync(databaseData, fieldName, searchValue, startFrom, limit, record, occ, sqlState);
  }
}

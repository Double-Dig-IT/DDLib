using DDigit.Exceptions;
using DDigit.Search;

namespace DDigit.Scripting;

public class AdlibPython
{
  public static Record? Read(string folder, string database, int id)
  {
    var provider = new DDataProvider(new MSSqlRepository());
    var databaseData = provider.GetDatabase(folder, database) ??
      throw new DatabaseNotFoundException(folder, database);
    var task = provider.ReadRecordAsync(databaseData, id);
    task.Wait();
    var record = task.Result;
    return record;
  }

  public static ResultSet? Search(string folder, string database, string statement)
  {
    var provider = new DDataProvider(new MSSqlRepository());

    var task = provider.SearchAsync(folder, database, null, statement, null, 1000, default);
    task.Wait();
    var result = task.Result;
    return result;
  }
}

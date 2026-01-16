namespace DDigit.RecordTransactions;

public abstract class DatabaseAccess
{
  public static DatabaseData OpenDatabase(string folder, string databaseName) =>
    MetaDataCache.ReadDatabase(folder, databaseName, false) ??
    throw new DatabaseNotFoundException(folder, databaseName);
}

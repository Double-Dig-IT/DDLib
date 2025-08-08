namespace DDigit.Exceptions;

public class SearchException(string folder, string database, string statement, Exception ex) :
  Exception($"Search Exception: folder='{folder}', database='{database}', '{statement}'", ex)
{
  public string Folder { get; } = folder;

  public string Database { get; } = database;

  public string Statement { get; } = statement;
}

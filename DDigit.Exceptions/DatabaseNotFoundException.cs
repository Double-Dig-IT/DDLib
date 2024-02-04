namespace DDigit.Exceptions;

public class DatabaseNotFoundException(string folder, string database) : DDException($"Database '{database}' not found in folder '{folder}'")
{
  public string Folder
  {
    get;
  } = folder;
  public string Database
  {
    get;
  } = database;
}
namespace DDigit.Exceptions;

public class RecordNotFoundException(string folder, string database, int id) : DDException($"Record not found in folder '{folder}', database '{database}', ID '{id}'.")
{
  public string Folder { get; } = folder;
  public string Database { get; } = database;
  public int Id { get; } = id;
}


namespace DDigit.Exceptions;

public class MissingIndexException(string? database, string? fieldName) : DDException($"Index missing in database '{database}' for field '{fieldName}'")
{
  public string? Database { get; } = database;

  public string? FieldName { get; } = fieldName;
}

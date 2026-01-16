namespace DDigit.Exceptions;

public class InvalidIntegerException(string? database, string? fieldName, string value, int id) :
  DDException($"Invalid integer value in database '{database}', field '{fieldName}, record '{id}', value '{value}'")
{
  public string? Database { get; private set; } = database;
  public string? FieldName { get; private set; } = fieldName;
  public string Value { get; private set; } = value;
  public int Id { get; private set; } = id;
}

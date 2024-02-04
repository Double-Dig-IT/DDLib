namespace DDigit.Exceptions;

public class LinkIdFieldMissingException(string? fieldName, string? database) 
  : DDException($"Setup error: linked field '{fieldName}' in database '{database}' does not have a link ID field")
{
  public string? FieldName { get; set; } = fieldName;

  public string? Database { get; set; } = database;
}
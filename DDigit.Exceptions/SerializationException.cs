namespace DDigit.Exceptions;

public class SerializationException(string? database, string? fieldTag, string? fieldName, Exception ex)
  : DDException($"Serialization Error in database: '{database}' tag: {fieldTag} ({fieldName}) - {ex.Message}", ex)
{
  public string? Database { get; private set; } = database;

  public string? FieldTag { get; private set; } = fieldTag;

  public string? FieldName { get; private set; } = fieldName;
}

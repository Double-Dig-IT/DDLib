namespace DDigit.Exceptions;

public class ForcingIsNotAlllowedException(string databaseName, string fieldName) : DDException($"Forcing is not allowed from field '{fieldName}' in '{databaseName}'")
{
  public string DatabaseName { get; } = databaseName;
  public string FieldName { get; } = fieldName;
}

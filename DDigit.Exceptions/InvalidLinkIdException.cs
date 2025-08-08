namespace DDigit.Exceptions;

public class InvalidLinkIdException : DDException
{
  public string? FieldName { get; private set; }
  public string? DatabaseName { get; private set; }
  public int Id { get; private set; }

  public InvalidLinkIdException(string? fieldName, string? databaseName, int id, InvalidCastException ex) : 
    base("$Invalid linkRef for field {fieldName} in database {databaseName} for record {id}", ex)
  {
    FieldName = fieldName;
    DatabaseName = databaseName;
    Id = id;
  }
}
namespace DDigit.Exceptions;

public class RecordParserException(string? database, int id, Exception innerException) 
  : Exception($"Parse error in record '{id}', in database '{database}' ---> {innerException.Message}", innerException)
{
  public string? Database { get; } = database;

  public int Id { get; } = id;
}
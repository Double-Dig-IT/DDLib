namespace DDigit.Exceptions;

public class FullTextNotEnabledException(string? database) : Exception($"Full text is not enabled in database {database}")
{
  public string? Database { get; private set; } = database;
}
namespace DDigit.Exceptions;

public class ElementNotFoundException(string? database, string? tagOrFieldName, int occ, string language) :
  DDException($"Element not found in database '{database}' for field '{tagOrFieldName}', occurrence {occ}, language '{language}'")
{
  public string? Database { get; } = database;
  public string? TagOrFieldName { get; } = tagOrFieldName;
  public int Occ { get; } = occ;
  public string Language { get; } = language;
}

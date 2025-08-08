
namespace DDigit.Exceptions;

public class EnumValueException(string database, string field, string tag, string language, object value) :
  DDException($"Enumeration value '{value}' not found in field '{field} ({tag})' in database '{database}' for language '{language}'")
{
  public string Database { get; } = database;
  public string Tag { get; } = tag;
  public string Field { get; } = field;
  public string Language { get; } = language;
  public object Value { get; } = value;
}
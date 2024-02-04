namespace DDigit.Exceptions;

public class LanguageNotSetException(string field) :
  DDException($"Language (or default language) not set for field '{field}'.")
{
  public string Field { get; private set; } = field;
}
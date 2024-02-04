namespace DDigit.Exceptions;


public class LanguageIsNotSupportedException(string language) : DDException($"Language {language} is not supported")
{
  public string Language { get; } = language;
}
namespace DDigit.Search;

public class LanguageExpectedException(string format, string? token) : Exception(string.Format(format, token))
{
  public LanguageExpectedException(string? token) : this(Format, token)
  {
  }

  public const string Format = "Expected a language, but got '{0}'";

  public string? Token { get; } = token;

}
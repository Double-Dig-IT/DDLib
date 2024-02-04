namespace DDigit.Search;

public class UnexpectedTokenException(string format, string? token, string expected) : Exception(string.Format(format, expected, token))
{
  public UnexpectedTokenException(string? token, string expected)
    : this(Format, token, expected)
  {
  }

  public const string Format = "Unexpected token, expected {0}, but got {1}";

  public string? Token { get; } = token;

  public string Expected { get; } = expected;
}
namespace DDigit.Search;

public class OperatorExpectedException(string format, string? token) : Exception(string.Format(format, token))
{
  public OperatorExpectedException(string? token) : this(Format, token)
  {
  }

  public const string Format = "Expected a search operator, but got '{0}'";

  public string? Token { get; } = token;

}
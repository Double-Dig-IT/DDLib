namespace DDigit.Search;

public class FieldNameExpectedException(string format, string? token) : Exception(string.Format(format, token))
{
  public FieldNameExpectedException(string? token) : this(Format, token)
  {
  }

  public const string Format = "Expected a field name, but got '{0}'";

  public string? Token { get; } = token;

}
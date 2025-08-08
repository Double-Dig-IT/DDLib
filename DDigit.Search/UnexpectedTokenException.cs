using System.Management.Automation.Language;

namespace DDigit.Search;

public class UnexpectedTokenException(string format, string? token, string expected, string? statement = null) 
  : Exception(string.Format(format, expected, token, statement))
{
  public UnexpectedTokenException(string? token, string expected, string? statement = null)
    : this(Format, token, expected, statement)
  {
  }

  public const string Format = "Unexpected token, expected '{0}', but got '{1}'";

  public string? Token { get; } = token;

  public string Expected { get; } = expected;

  public string? Statement { get; } = statement;
}
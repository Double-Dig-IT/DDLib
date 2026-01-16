namespace DDigit.Search;

[Serializable]
public class PositiveIntegerExpectedException(string? token) : DDException($"Integer expected, but got '{token}'.")
{
  public string? Token { get; } = token;
}
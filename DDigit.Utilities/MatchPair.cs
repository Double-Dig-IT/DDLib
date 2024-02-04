namespace DDigit.Utilities;

public struct MatchPair(string? column, string value)
{
  public string? Column = column;
  public string Value = value;
}

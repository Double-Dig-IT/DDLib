namespace DDigit.Utilities;

public class MatchList : List<MatchPair>
{
  public static bool Match(IEnumerable<MatchPair> p)
  {
    var result = true;
    foreach (var pair in p)
    {
      result = result && Match(pair.Column, pair.Value);
      if (!result)
      {
        break;
      }
    }
    return result;
  }

  public static bool Match(string? pattern, string? text) =>
    pattern == null || (text != null && text.Match(pattern, true));

}

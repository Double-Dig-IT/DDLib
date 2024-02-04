namespace DDigit.Search;

public class DDSearch
{
  public static SearchNode? Parse(DatabaseData databaseData, string searchStatement)
  {
    var parser = new Parser(databaseData, searchStatement);
    parser.Parse();
    return parser.Result;
  }
}

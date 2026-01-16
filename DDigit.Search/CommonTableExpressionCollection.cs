
namespace DDigit.Search;

public class CommonTableExpressionCollection : List<string>
{
    public string ToSql() => string.Join(",\n\n", this);
}
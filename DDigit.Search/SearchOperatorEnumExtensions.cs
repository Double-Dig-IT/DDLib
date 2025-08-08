namespace DDigit.Search;

public static class SearchOperatorEnumExtensions
{
  public static string ToSymbol(this SearchOperatorEnum searchOperator)
    => searchOperator switch
    {
      SearchOperatorEnum.Equals => "=",
      SearchOperatorEnum.Greater => ">",
      SearchOperatorEnum.Smaller => "<",
      SearchOperatorEnum.GreaterOrEquals => ">=",
      SearchOperatorEnum.SmallerOrEquals => "<=",
      _ => searchOperator.ToString()
    };
}

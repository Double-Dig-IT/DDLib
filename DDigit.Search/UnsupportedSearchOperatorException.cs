namespace DDigit.Search;

public class UnsupportedSearchOperatorException(SearchOperatorEnum op) : DDException($"Unsupported search operator: {op}")
{
  private SearchOperatorEnum Operator { get; } = op;
}
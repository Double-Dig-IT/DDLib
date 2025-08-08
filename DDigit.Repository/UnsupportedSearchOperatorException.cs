
namespace DDigit.Repository;

internal class UnsupportedSearchOperatorException(SearchOperatorEnum op) : DDException($"Unsupported search operator: {op}")
{
  private SearchOperatorEnum Operator { get; } = op;
}
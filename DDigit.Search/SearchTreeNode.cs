namespace DDigit.Search;

public class SearchTreeNode(SearchNode right, SearchNode left, BooleanOperator booleanOperator = BooleanOperator.And) : SearchNode
{
  public BooleanOperator Operator = booleanOperator;

  public SearchNode Left = left;

  public SearchNode Right = right;

  public override string ToString() => $"({Left} {Operator} {Right})";
}

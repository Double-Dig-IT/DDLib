namespace DDigit.Search;

public class SearchSetLeaf(int setId) : SearchNode
{
  public int SetId { get; private set; } = setId;

  public override string ToString() => $"Set {SetId}";
}
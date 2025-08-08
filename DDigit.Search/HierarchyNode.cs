namespace DDigit.Search;

public class HierarchyNode
{
  public required string Key { get; set; }
  public int Id { get; set; }
  public List<HierarchyNode> Children { get; set; } = [];
  public override string ToString() => $"{Key} ({Id})";
  public bool Match {  get; set; }

  public List<int> GetAllIds()
  {
    List<int> ids = Match ? [Id] : [];  
    foreach (var child in Children)
    {
      ids.AddRange(child.GetAllIds());
    }
    return ids;
  }
}

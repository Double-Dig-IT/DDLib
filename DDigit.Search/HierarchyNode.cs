
namespace DDigit.Search;

public class HierarchyNode
{
    public required string Key { get; set; }
    public int Id { get; set; }
    public List<HierarchyNode> Children { get; set; } = [];
    public override string ToString() => $"{Key} ({Id})";

    public int? LastId
    {
        get
        {
            if (Children.Count == 0)
            {
                return Id;
            }
            return Children.Last().LastId;
        }
    }
}

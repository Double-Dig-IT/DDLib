namespace DDigit.Classes;

public class AutoCompleteObject : IComparable<AutoCompleteObject>
{
  [JsonIgnore] public IndexTypeEnum IndexType { get; set; }

  [JsonPropertyName("term")] public string? Term { get; set; }

  [JsonPropertyName("use")] public string? Use { get; set; }

  [JsonPropertyName("hits")] public int Hits { get; set; }

  public int CompareTo(AutoCompleteObject? other) => string.Compare(Term, other?.Term);
  
  public override string ToString() => $"{Term} ({Hits:n0})";
}

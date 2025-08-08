namespace DDigit.Data;

public class SerializeOptions
{
  public HashSet<string>? Fields { get; set; }

  public HashSet<string>? Databases { get; set; }

  public bool JsonLD { get; set; }

  public string? LDContext { get; set; }

  public string? API { get; set; }

  public string? IIIF { get; set; }

  public int? NAAN { get; set; }
}

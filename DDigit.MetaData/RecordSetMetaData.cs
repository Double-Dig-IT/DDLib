namespace DDigit.MetaData;

public class RecordSetMetaData
{
  public int Number
  {
    get; set;
  }

  public string? Database
  {
    get; set;
  }
  public string? Title
  {
    get; set;
  }
  public string? Owner
  {
    get; set;
  }
  public string? Selection
  {
    get; set;
  }

  public int? Hits
  {
    get; set;
  }
  public DateTime Created
  {
    get;
    set;
  }
  public DateTime Modified
  {
    get;
    set;
  }

  public override string ToString() => $"{Database} {Number} {Title} ({Hits})";

}

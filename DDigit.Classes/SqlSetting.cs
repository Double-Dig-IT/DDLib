namespace DDigit.Classes;

public class SqlSetting
{
  public string? Server
  {
    get; set;
  }

  public string? Database
  {
    get; set;
  }

  public string? Table
  {
    get; set;
  }

  public string? User
  {
    get; set;
  }

  public string? Password
  {
    get; set;
  }

  public override string ToString() => $"{Database} {Server} {Table} {User}";
}
